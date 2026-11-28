namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Takes the differences of a <see cref="LayoutComparison"/> that change the timetable in a known way
/// (<see cref="LayoutChangeHandling.Confirm"/>) into a plan: a track removed, a location of another kind, a location
/// removed from between two neighbours, and a location inserted on a stretch. Each has a preview, which changes
/// nothing and says what would happen, for the planner to confirm; <c>Apply</c> then does exactly that. The
/// differences that change only distances and times are taken by <see cref="LayoutUpdate.ApplyAutomatic"/>.
/// </summary>
public static class LayoutChangeRules
{
    extension(Plan plan)
    {
        /// <summary>What applying <paramref name="change"/> to the plan would do, without doing it.</summary>
        /// <exception cref="ArgumentException">The change is not one the planner confirms.</exception>
        public LayoutChangePreview PreviewChange(LayoutChange change)
        {
            ArgumentNullException.ThrowIfNull(change);
            return change.Kind switch
            {
                LayoutChangeKind.TrackRemoved => PreviewTrackRemoval(change),
                LayoutChangeKind.KindChanged => PreviewKindChange(plan, change),
                LayoutChangeKind.LocationRemoved => new(change) { Deletion = plan.PreviewDelete(change.Location!) },
                LayoutChangeKind.LocationInserted => PreviewInsertion(plan, change),
                _ => throw new ArgumentException($"A {change.Kind} is not applied by the planner.", nameof(change)),
            };
        }

        /// <summary>
        /// Applies <paramref name="change"/> as <c>PreviewChange</c> describes it, or nothing when something blocks it.
        /// </summary>
        /// <returns>The preview it was applied by; nothing was changed when it is not allowed.</returns>
        public LayoutChangePreview ApplyChange(LayoutChange change)
        {
            var preview = plan.PreviewChange(change);
            if (!preview.IsAllowed) return preview;
            switch (change.Kind)
            {
                case LayoutChangeKind.TrackRemoved:
                    RemoveTrack(plan, change.Track!, preview);
                    break;
                case LayoutChangeKind.KindChanged:
                    var changed = plan.Layout.ChangeOperationLocationType(change.Location!, change.SourceLocation!.Kind);
                    plan.Timetable.ConformPlanningTo(changed);
                    LayoutUpdate.CopyWhatItIs(change.SourceLocation!, changed);
                    break;
                case LayoutChangeKind.LocationRemoved:
                    RemoveLocation(plan, change, preview.Deletion!);
                    break;
                case LayoutChangeKind.LocationInserted:
                    InsertLocation(plan, change, preview);
                    break;
            }
            return preview;
        }
    }

    // The calls on the track go to the track the location would give the train now, the removed one left out; a
    // location with no other track has nowhere to put them.
    private static LayoutChangePreview PreviewTrackRemoval(LayoutChange change)
    {
        var track = change.Track!;
        var location = change.Location!;
        if (location.Tracks.Count < 2)
            return new(change) { Blockers = [Reference.For(location)] };
        var moved = new List<MovedCall>();
        var position = location.Tracks.IndexOf(track);
        location.Tracks.RemoveAt(position);
        try
        {
            foreach (var call in track.Calls.ToList())
            {
                var calls = call.Train.CallsInRunOrder.ToList();
                var index = calls.FindIndex(c => ReferenceEquals(c, call));
                var previous = index > 0 ? calls[index - 1].OperationLocation : null;
                var next = index < calls.Count - 1 ? calls[index + 1].OperationLocation : null;
                var preference = call.Train.IsPassenger && call.IsStop ? TrackPreference.Platform : TrackPreference.MainTrack;
                if (location.PreferredTrack(call.Train.Timetable, previous, next, preference) is { } to) moved.Add(new(call, to));
            }
        }
        finally
        {
            location.Tracks.Insert(position, track);
        }
        return new(change) { MovedCalls = moved };
    }

    private static void RemoveTrack(Plan plan, StationTrack track, LayoutChangePreview preview)
    {
        // The call belongs to its train; MoveTo changes its track and keeps the derived per-track index in step.
        foreach (var (call, to) in preview.MovedCalls) call.MoveTo(to);
        // A vehicle fetched from the track, or put on it, is fetched or put on the arrival or departure track instead.
        foreach (var part in plan.Schedules.SelectMany(s => s.Parts).Concat(plan.DriverDuties.SelectMany(d => d.Parts)))
        {
            if (ReferenceEquals(part.FromTrack, track)) part.FromTrack = null;
            if (ReferenceEquals(part.ToTrack, track)) part.ToTrack = null;
        }
        plan.TryDelete(track);
    }

    // A location of a kind where trains never stop no longer has the stops the trains make there.
    private static LayoutChangePreview PreviewKindChange(Plan plan, LayoutChange change)
    {
        var location = change.Location!;
        var target = change.SourceLocation!.Kind;
        var stopping = OperationLocationRules.TrainsCanStop(target)
            ? []
            : plan.Timetable.Trains.Where(t => t.Calls.Any(c => c.OperationLocation.Equals(location) && c.IsStop)).ToList();
        return new(change) { TrainsNoLongerStopping = stopping };
    }

    // The two stretches of the location become one, as when the planner deletes it (DeletionRules), and the stretch
    // that joins its neighbours takes the newer layout's distance.
    private static void RemoveLocation(Plan plan, LayoutChange change, LocationDeletionPreview deletion)
    {
        if (deletion.Replacement is not { } planned || plan.TryDelete(change.Location!) is not DeletionResult.Success) return;
        // The deletion makes a joining stretch of its own; it is the one between the same two neighbours.
        if (plan.Layout.StretchBetween(planned.Start, planned.End) is not { } joined || change.SourceStretch is not { } source) return;
        joined.Distance = source.Distance;
        joined.MeasuredDistance = source.MeasuredDistance;
        joined.MeasuredSpeed = source.MeasuredSpeed;
        joined.TracksCount = source.TracksCount;
        if (!joined.IsTimeEntered) joined.Time = TrackStretchTiming.RunningMinutes(joined, plan.Timetable.Settings.TimeAndSpeed);
    }

    // The trains that run the stretch from one end straight to the other pass the new location in between.
    private static LayoutChangePreview PreviewInsertion(Plan plan, LayoutChange change)
    {
        var stretch = change.Stretch!;
        var passing = plan.Timetable.Trains.Where(t => Legs(t, stretch.Start, stretch.End).Any()).ToList();
        return new(change) { PassingTrains = passing };
    }

    /// <summary>
    /// The location is added as the newer layout has it, with what is decided for it left for the planner, and with
    /// ids of its own in the plan. The stretch it lies on is split in two, with the newer layout's distances; every
    /// timetable stretch through it takes both halves in its place; and each train that runs the stretch gets a call
    /// at the location between the two, passing it at the time its distance gives.
    /// </summary>
    private static void InsertLocation(Plan plan, LayoutChange change, LayoutChangePreview preview)
    {
        var layout = plan.Layout;
        var split = change.Stretch!;
        var (from, to) = (split.Start, split.End);
        var source = change.SourceLocation!;

        var location = source.Copy();
        location.Id = Free(layout.OperationLocations.Select(l => l.Id), source.Id);
        location.Layout = layout;
        location.LayoutId = layout.Id;
        location.ControlledBy = null;
        location.CargoServedFrom = null;
        location.LockKey = null;
        var trackIds = layout.OperationLocations.SelectMany(l => l.Tracks).Select(t => t.Id).ToList();
        foreach (var track in location.Tracks)
        {
            track.Id = Free(trackIds, track.Id);
            trackIds.Add(track.Id);
            track.Station = location;
            track.StationId = location.Id;
        }
        layout.OperationLocations.Add(location);

        var (first, second) = Halves(change, from, split.Distance);
        var settings = plan.Timetable.Settings.TimeAndSpeed;
        var after = new TrackStretch(Free(layout.TrackStretches.Select(s => s.Id), 0), location, to, second, split.TracksCount, split.Speed, split.Time)
        {
            IsElectrified = split.IsElectrified,
            Layout = layout,
            LayoutId = layout.Id,
        };
        var wholeTime = split.Time;
        split.End = location;
        split.EndId = location.Id;
        split.Distance = first;
        split.ModuleIds = [];
        after.ModuleIds = [];
        var (firstSource, secondSource) = SourceHalves(change, from);
        (split.MeasuredDistance, split.MeasuredSpeed) = (firstSource?.MeasuredDistance, firstSource?.MeasuredSpeed);
        (after.MeasuredDistance, after.MeasuredSpeed) = (secondSource?.MeasuredDistance, secondSource?.MeasuredSpeed);
        if (split.IsTimeEntered)
        {
            split.Time = (int)Math.Round(wholeTime * first / (first + second), MidpointRounding.AwayFromZero);
            after.Time = wholeTime - split.Time;
            after.IsTimeEntered = true;
        }
        else
        {
            split.Time = TrackStretchTiming.RunningMinutes(split, settings);
            after.Time = TrackStretchTiming.RunningMinutes(after, settings);
        }
        layout.Add(after);
        layout.InsertIntoStretchLines(from, to, location);

        foreach (var route in plan.Timetable.TimetableStretches.Where(r => r.Stretches.Contains(split)))
            route.Stretches = Rerouted(route, split, after, from);
        foreach (var dispatch in plan.Timetable.DispatchStretches.Where(d => d.Stretches.Contains(split)))
            dispatch.Stretches = Rerouted(dispatch.Stretches, split, after, from, dispatch.From);

        var callId = plan.Timetable.Trains.SelectMany(t => t.Calls).Select(c => c.Id).DefaultIfEmpty(0).Max();
        foreach (var train in preview.PassingTrains)
        {
            foreach (var (departing, arriving) in Legs(train, from, to).ToList())
            {
                var share = departing.OperationLocation.Equals(from) ? first / (first + second) : second / (first + second);
                var passes = Interpolate(departing.Departure, arriving.Arrival, share);
                if (location.PreferredTrack(plan.Timetable, departing.OperationLocation, arriving.OperationLocation, TrackPreference.MainTrack) is not { } track) continue;
                var call = new StationCall(++callId, track, passes, passes) { IsArrival = false, IsDeparture = false };
                train.AddAfter(call, departing);
            }
        }
    }

    /// <summary>The distances on either side of the new location: the newer layout's, else the stretch's divided in the middle.</summary>
    private static (double First, double Second) Halves(LayoutChange change, OperationLocation from, double whole) =>
        SourceHalves(change, from) is ({ } towardsFrom, { } towardsTo) ? (towardsFrom.Distance, towardsTo.Distance) : (whole / 2, whole / 2);

    /// <summary>The newer layout's stretches on either side of the new location: the one towards <paramref name="from"/> first.</summary>
    private static (TrackStretch? TowardsFrom, TrackStretch? TowardsTo) SourceHalves(LayoutChange change, OperationLocation from)
    {
        if (change.Stretches.Count != 2) return (null, null);
        var towardsFrom = change.Stretches.FirstOrDefault(s => s.Start.Signature == from.Signature || s.End.Signature == from.Signature);
        var towardsTo = change.Stretches.FirstOrDefault(s => !ReferenceEquals(s, towardsFrom));
        return towardsFrom is null || towardsTo is null ? (null, null) : (towardsFrom, towardsTo);
    }

    /// <summary>The consecutive calls of <paramref name="train"/> that run from <paramref name="from"/> straight to <paramref name="to"/>, either way.</summary>
    private static IEnumerable<(StationCall Departing, StationCall Arriving)> Legs(Train train, OperationLocation from, OperationLocation to)
    {
        var calls = train.CallsInRunOrder;
        for (var i = 0; i < calls.Count - 1; i++)
        {
            var (a, b) = (calls[i].OperationLocation, calls[i + 1].OperationLocation);
            if ((a.Equals(from) && b.Equals(to)) || (a.Equals(to) && b.Equals(from)))
                yield return (calls[i], calls[i + 1]);
        }
    }

    private static Time Interpolate(Time departure, Time arrival, double share)
    {
        var minutes = departure.TotalMinutes + (arrival.TotalMinutes - departure.TotalMinutes) * share;
        return Time.FromTimeSpan(TimeSpan.FromMinutes(Math.Round(minutes, MidpointRounding.AwayFromZero)));
    }

    /// <summary>The route with the stretch that was split replaced by its two halves, in the order the route runs them.</summary>
    private static ICollection<TrackStretch> Rerouted(TimetableStretch route, TrackStretch split, TrackStretch after, OperationLocation from) =>
        Rerouted(route.Stretches, split, after, from, route.Stretches.First().Start);

    private static ICollection<TrackStretch> Rerouted(ICollection<TrackStretch> stretches, TrackStretch split, TrackStretch after, OperationLocation from, OperationLocation routeStart)
    {
        var list = stretches.ToList();
        var index = list.IndexOf(split);
        // The location the route reaches the split stretch at: where the stretch before it ends, or for the first
        // stretch the start of the route.
        var entered = index > 0 ? Shared(list[index - 1], split) ?? routeStart : routeStart;
        var halves = entered.Equals(from) ? new[] { split, after } : [after, split];
        list.RemoveAt(index);
        list.InsertRange(index, halves);
        return list;

        static OperationLocation? Shared(TrackStretch a, TrackStretch b) =>
            a.Start.Equals(b.Start) || a.Start.Equals(b.End) ? a.Start :
            a.End.Equals(b.Start) || a.End.Equals(b.End) ? a.End : null;
    }

    private static int Free(IEnumerable<int> used, int wanted)
    {
        var ids = used.ToHashSet();
        return wanted > 0 && !ids.Contains(wanted) ? wanted : ids.DefaultIfEmpty(0).Max() + 1;
    }
}

/// <summary>
/// What applying a <see cref="LayoutChange"/> would do to a plan, for the planner to confirm. What it lists depends on
/// the kind of change.
/// </summary>
/// <param name="Change">The change.</param>
public sealed record LayoutChangePreview(LayoutChange Change)
{
    /// <summary>What stops the change; nothing is applied while there is anything here.</summary>
    public IReadOnlyList<Reference> Blockers { get; init; } = [];

    /// <summary>Whether the change can be applied.</summary>
    public bool IsAllowed => Blockers.Count == 0 && (Deletion?.IsAllowed ?? true);

    /// <summary>For a track removed: each call on it and the track it is moved to.</summary>
    public IReadOnlyList<MovedCall> MovedCalls { get; init; } = [];

    /// <summary>For a kind of location where trains never stop: the trains that stop there now.</summary>
    public IReadOnlyList<Train> TrainsNoLongerStopping { get; init; } = [];

    /// <summary>For a location removed: what deleting it does, as when the planner deletes it.</summary>
    public LocationDeletionPreview? Deletion { get; init; }

    /// <summary>For a location inserted: the trains that run the stretch it is inserted on, and so get a call there.</summary>
    public IReadOnlyList<Train> PassingTrains { get; init; } = [];
}

/// <summary>A call on a track that is removed, and the track it is moved to.</summary>
/// <param name="Call">The call.</param>
/// <param name="To">The track it is moved to.</param>
public sealed record MovedCall(StationCall Call, StationTrack To);
