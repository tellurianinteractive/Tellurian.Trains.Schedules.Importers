namespace Tellurian.Trains.Schedules.Model.Validations;

public static partial class DeletionRules
{
    extension(Plan plan)
    {
        /// <summary>
        /// Works out everything that deleting <paramref name="location"/> would change, without changing
        /// anything: the train calls that go, the track stretches replaced and the timetable stretches
        /// rerouted, and whatever else in the layout still names the location. The planner is shown this
        /// and confirms or aborts; <c>TryDelete</c> then does exactly what it says.
        /// </summary>
        /// <param name="location">The location to delete.</param>
        public LocationDeletionPreview PreviewDelete(OperationLocation location) =>
            Analyse(plan, location.ValueOrException(nameof(location)));

        /// <summary>
        /// Determines whether an <see cref="OperationLocation"/> may be deleted. Trains calling there do
        /// not stop it — their calls are removed with it — but planning that starts or ends at one of
        /// those calls does, as does a train or timetable stretch that would be left with a route
        /// nothing joins up. See <c>PreviewDelete</c> for the whole of what a delete changes.
        /// </summary>
        public DeletionResult MayDelete(OperationLocation location)
        {
            var preview = plan.PreviewDelete(location);
            return preview.IsAllowed
                ? new DeletionResult.Success(location)
                : new DeletionResult.Failure(location, preview.Blockers);
        }

        /// <summary>
        /// Deletes an <see cref="OperationLocation"/> and everything that depends on it being there, as
        /// <c>PreviewDelete</c> describes. Returns the <see cref="DeletionResult.Failure"/> listing what
        /// blocks it, leaving the model untouched, when it may not be deleted.
        /// </summary>
        /// <remarks>
        /// Where the location lies on a line between exactly two neighbours A and C, the two track
        /// stretches A–B and B–C are replaced by one A–C (or by the one already joining A and C), so the
        /// trains that ran through B keep running A to C at the times they had, and every timetable
        /// stretch through B runs A–C instead. A train starting or ending at B now starts or ends at its
        /// neighbouring call, which takes over the preparation or finishing-up time. Dispatch stretches
        /// are derived from the track stretches, so they are generated again where the layout has any.
        /// </remarks>
        public DeletionResult TryDelete(OperationLocation location)
        {
            var preview = plan.PreviewDelete(location);
            if (!preview.IsAllowed) return new DeletionResult.Failure(location, preview.Blockers);

            var layout = plan.Layout;
            foreach (var change in preview.Trains) RemoveCallsAt(plan, location, change);
            ForgetOtherTracksAt(plan, location);
            ReplaceStretches(layout, preview);
            RepointTrackRoutes(layout, location, preview.Replacement);
            foreach (var controlled in preview.NoLongerControlled) controlled.ControlledBy = null;
            foreach (var served in preview.NoLongerCargoServed) served.CargoServedFrom = null;
            foreach (var keyed in preview.LockKeysRemoved) keyed.LockKey = null;
            foreach (var category in preview.StopPatternsChanged) category.StopLocationIds.Remove(location.Id);
            foreach (var options in preview.CargoFlowOptionsChanged)
            {
                foreach (var origin in options.Origins.Where(o => o.Location.Equals(location)).ToList()) options.Origins.Remove(origin);
                foreach (var destination in options.Destinations.Where(d => d.Location.Equals(location)).ToList()) options.Destinations.Remove(destination);
            }
            // A shunter works no train part, so nothing stops it going with the location it is stationed at.
            foreach (var shunter in preview.ShuntersRemoved) plan.TryDelete(shunter);
            layout.ForgetTopologyPositionOf(location);
            layout.OperationLocations.Remove(location);
            if (preview.RegeneratesDispatchStretches) layout.DispatchStretches = layout.CreateDispatchStretches();
            return new DeletionResult.Success(location);
        }
    }

    private static LocationDeletionPreview Analyse(Plan plan, OperationLocation location)
    {
        var layout = plan.Layout;
        var blockers = new List<Reference>();

        // Planning that starts or ends at a call here has nowhere to go when the call does: a vehicle or
        // a driver would be left changing trains at a location that is no longer there.
        foreach (var schedule in plan.Schedules.Where(s => s.Parts.Any(p => IsAt(p, location))))
            blockers.Add(Reference.For(schedule));
        foreach (var duty in plan.DriverDuties.Where(d => d.Parts.Any(p => IsAt(p, location))))
            blockers.Add(Reference.For(duty));
        foreach (var train in plan.Timetable.Trains.Where(t => t.CargoFlows.Any(cf => IsAt(cf, location))))
            blockers.Add(new("CargoFlow", train.ToString()));

        var touching = layout.TrackStretches.Where(s => Touches(s, location)).ToList();
        var replacement = ReplacementFor(layout, location, touching);

        var trains = new List<TrainCallRemoval>();
        foreach (var train in plan.Timetable.Trains.Where(t => t.Calls.Any(c => c.OperationLocation.Equals(location))))
        {
            var removal = TrainRemoval(train, location, replacement is not null);
            trains.Add(removal);
            if (removal.Kind.HasFlag(TrainCallRemovalKind.Blocked)) blockers.Add(Reference.For(train));
            else if (removal.Kind.HasFlag(TrainCallRemovalKind.TrainDeleted) && ReferencesTo(plan, train) is { Count: > 0 } references)
                blockers.AddRange(references);
        }

        var changedRoutes = new List<TimetableStretch>();
        var emptiedRoutes = new List<TimetableStretch>();
        foreach (var route in layout.TimetableStretches.Where(t => t.Stretches.Any(s => Touches(s, location))))
        {
            var rerouted = Reroute(route, location, replacement);
            if (rerouted is null) blockers.Add(Reference.For(route));
            else if (rerouted.Count == 0) emptiedRoutes.Add(route);
            else changedRoutes.Add(route);
        }

        return new LocationDeletionPreview
        {
            Location = location,
            Blockers = [.. blockers.Distinct()],
            Trains = trains,
            RemovedStretches = touching,
            Replacement = replacement,
            IsReplacementNew = replacement is not null && !layout.TrackStretches.Contains(replacement),
            ChangedTimetableStretches = changedRoutes,
            RemovedTimetableStretches = emptiedRoutes,
            RegeneratesDispatchStretches = layout.DispatchStretches.Count > 0,
            NoLongerControlled = [.. layout.OperationLocations.Where(o => !o.Equals(location) && location.Equals(o.ControlledBy))],
            NoLongerCargoServed = [.. layout.OperationLocations.Where(o => !o.Equals(location) && location.Equals(o.CargoServedFrom))],
            LockKeysRemoved = [.. layout.OperationLocations.Where(o => !o.Equals(location) && o.LockKey is { } key && location.Equals(key.HeldAt))],
            StopPatternsChanged = [.. plan.Timetable.TrainCategories.Where(c => c.StopLocationIds.Contains(location.Id))],
            CargoFlowOptionsChanged = [.. plan.Timetable.CargoFlowOptions.Where(o =>
                o.Origins.Any(origin => origin.Location.Equals(location)) ||
                o.Destinations.Any(destination => destination.Location.Equals(location)))],
            ShuntersRemoved = plan.ShuntersAt(location),
        };
    }

    private static bool IsAt(TrainPart part, OperationLocation location) =>
        part.From.OperationLocation.Equals(location) || part.To.OperationLocation.Equals(location);

    private static bool Touches(TrackStretch stretch, OperationLocation location) =>
        stretch.Start.Equals(location) || stretch.End.Equals(location);

    private static OperationLocation OtherEnd(TrackStretch stretch, OperationLocation location) =>
        stretch.Start.Equals(location) ? stretch.End : stretch.Start;

    // The stretch that joins the two neighbours once the location between them is gone. Only a location
    // on a line — two stretches to two different neighbours — has one: at a terminus there is nothing to
    // join, and at a junction no one pair of neighbours is the right one. Where a stretch already joins
    // the neighbours it is that one; otherwise a new one, not yet in the layout, runs the way the line
    // runs through the location, and is as long and takes as long as the two together, at the lowest
    // speed and track count of the two, electrified only where both are.
    private static TrackStretch? ReplacementFor(Layout layout, OperationLocation location, IReadOnlyList<TrackStretch> touching)
    {
        if (touching.Count != 2) return null;
        var (first, second) = touching[0].End.Equals(location) || touching[1].Start.Equals(location)
            ? (touching[0], touching[1])
            : (touching[1], touching[0]);
        var from = OtherEnd(first, location);
        var to = OtherEnd(second, location);
        if (from.Equals(to)) return null;
        if (layout.StretchBetween(from, to) is { } existing) return existing;

        var id = layout.TrackStretches.Max(s => s.Id) + 1;
        return new TrackStretch(id, from, to,
            first.Distance + second.Distance,
            Math.Min(first.TracksCount, second.TracksCount),
            Math.Min(first.Speed, second.Speed),
            first.Time + second.Time)
        {
            IsElectrified = first.IsElectrified && second.IsElectrified,
        };
    }

    // What deleting the location does to one train calling there. A train left with no call, or with a
    // single call it cannot work as a shunting task, goes with the location. Otherwise the calls before
    // and after each removed one must be at different locations joined by the replacement stretch: a
    // train reversing at the location would be left calling twice in a row at the same place, and one
    // running through a junction would be left with nothing joining its calls.
    private static TrainCallRemoval TrainRemoval(Train train, OperationLocation location, bool hasReplacement)
    {
        var ordered = train.CallsInRunOrder;
        var remaining = ordered.Where(c => !c.OperationLocation.Equals(location)).ToList();
        if (remaining.Count == 0 || (remaining.Count == 1 && !train.IsShuntingTask))
            return new(train, TrainCallRemovalKind.TrainDeleted);

        var kind = TrainCallRemovalKind.None;
        if (ordered[0].OperationLocation.Equals(location)) kind |= TrainCallRemovalKind.Origin;
        if (ordered[^1].OperationLocation.Equals(location)) kind |= TrainCallRemovalKind.Terminus;
        var passesThrough = ordered.Skip(1).SkipLast(1).Any(c => c.OperationLocation.Equals(location));
        if (passesThrough) kind |= TrainCallRemovalKind.Intermediate;

        for (var i = 1; i < remaining.Count; i++)
            if (remaining[i - 1].OperationLocation.Equals(remaining[i].OperationLocation))
                return new(train, kind | TrainCallRemovalKind.Blocked);
        if (passesThrough && !hasReplacement) return new(train, kind | TrainCallRemovalKind.Blocked);
        return new(train, kind);
    }

    // The route of a timetable stretch without the location: the two stretches through it become the
    // replacement, and one ending the route there is dropped. Null when that leaves the route broken —
    // the location is a junction in the middle of it, or the stretch already joining the neighbours runs
    // the other way round.
    private static List<TrackStretch>? Reroute(TimetableStretch route, OperationLocation location, TrackStretch? replacement)
    {
        var stretches = route.Stretches.ToList();
        var result = new List<TrackStretch>(stretches.Count);
        for (var i = 0; i < stretches.Count; i++)
        {
            var stretch = stretches[i];
            if (!Touches(stretch, location)) { result.Add(stretch); continue; }
            if (i + 1 < stretches.Count && stretch.End.Equals(location) && stretches[i + 1].Start.Equals(location))
            {
                if (replacement is null || !replacement.Start.Equals(stretch.Start) || !replacement.End.Equals(stretches[i + 1].End)) return null;
                result.Add(replacement);
                i++;
            }
            else if ((i == 0 && stretch.Start.Equals(location)) || (i == stretches.Count - 1 && stretch.End.Equals(location))) continue;
            else return null;
        }
        return result;
    }

    private static void RemoveCallsAt(Plan plan, OperationLocation location, TrainCallRemoval change)
    {
        var train = change.Train;
        if (change.Kind.HasFlag(TrainCallRemovalKind.TrainDeleted))
        {
            foreach (var call in train.Calls) call.Track.Calls.Remove(call);
            plan.Timetable.Trains.Remove(train);
            return;
        }
        var ordered = train.CallsInRunOrder;
        foreach (var call in ordered.Where(c => c.OperationLocation.Equals(location)))
        {
            train.Calls.Remove(call);
            call.Track.Calls.Remove(call);
        }
        var remaining = train.CallsInRunOrder;
        if (remaining.Count < 2) return;
        if (change.Kind.HasFlag(TrainCallRemovalKind.Origin)) MakeOrigin(remaining[0], DwellMinutes(ordered[0]));
        if (change.Kind.HasFlag(TrainCallRemovalKind.Terminus)) MakeTerminus(remaining[^1], DwellMinutes(ordered[^1]));
    }

    // A part may name a track here to fetch its vehicles from or put them on while its own call is
    // elsewhere; the track goes with the location, so the part forgets it (as TryDelete(StationTrack)).
    private static void ForgetOtherTracksAt(Plan plan, OperationLocation location)
    {
        foreach (var part in plan.Schedules.SelectMany(s => s.Parts))
        {
            if (part.FromTrack is { } from && location.Equals(from.Station)) part.FromTrack = null;
            if (part.ToTrack is { } to && location.Equals(to.Station)) part.ToTrack = null;
        }
    }

    private static void ReplaceStretches(Layout layout, LocationDeletionPreview preview)
    {
        if (preview.Replacement is { } replacement && preview.IsReplacementNew) layout.Add(replacement);
        foreach (var route in preview.ChangedTimetableStretches)
            route.Stretches = Reroute(route, preview.Location, preview.Replacement)!;
        foreach (var route in preview.RemovedTimetableStretches) layout.TimetableStretches.Remove(route);
        foreach (var stretch in preview.RemovedStretches) layout.TrackStretches.Remove(stretch);
    }

    // A track at a neighbour reserved for trains to or from the location is, once the location is gone,
    // reserved for trains to or from the neighbour on the other side, which is where they now come from
    // or go to. Every other route naming the location is forgotten.
    private static void RepointTrackRoutes(Layout layout, OperationLocation location, TrackStretch? replacement)
    {
        if (replacement is not null)
        {
            Repoint(replacement.Start, replacement.End);
            Repoint(replacement.End, replacement.Start);
        }
        layout.ForgetTrackRoutesTo(location);

        void Repoint(OperationLocation neighbour, OperationLocation beyond)
        {
            foreach (var track in neighbour.Tracks)
            {
                if (track.PreviousLocationId == location.Id) track.PreviousLocationId = beyond.Id;
                if (track.NextLocationId == location.Id) track.NextLocationId = beyond.Id;
            }
        }
    }
}

/// <summary>
/// Everything deleting an <see cref="OperationLocation"/> would change, worked out before anything is
/// changed, so the planner can see it and confirm or abort. See <c>DeletionRules.PreviewDelete</c>.
/// </summary>
public sealed record LocationDeletionPreview
{
    /// <summary>The location to delete.</summary>
    public required OperationLocation Location { get; init; }

    /// <summary>What stops the delete; empty when it may go ahead.</summary>
    public required IReadOnlyList<Reference> Blockers { get; init; }

    /// <summary>Whether the location may be deleted: nothing blocks it.</summary>
    public bool IsAllowed => Blockers.Count == 0;

    /// <summary>Every train calling at the location, and what happens to it.</summary>
    public required IReadOnlyList<TrainCallRemoval> Trains { get; init; }

    /// <summary>The track stretches to or from the location, all removed with it.</summary>
    public required IReadOnlyList<TrackStretch> RemovedStretches { get; init; }

    /// <summary>
    /// The track stretch joining the location's two neighbours in place of the two stretches through
    /// it, or <see langword="null"/> where it has not exactly two (a terminus or a junction).
    /// </summary>
    public required TrackStretch? Replacement { get; init; }

    /// <summary>
    /// Whether <see cref="Replacement"/> is added to the layout; otherwise it is the stretch that already
    /// joins the two neighbours.
    /// </summary>
    public required bool IsReplacementNew { get; init; }

    /// <summary>The timetable stretches that keep running, rerouted past the location.</summary>
    public required IReadOnlyList<TimetableStretch> ChangedTimetableStretches { get; init; }

    /// <summary>The timetable stretches left with no route at all, which are removed.</summary>
    public required IReadOnlyList<TimetableStretch> RemovedTimetableStretches { get; init; }

    /// <summary>Whether the layout's dispatch stretches are generated again afterwards.</summary>
    public required bool RegeneratesDispatchStretches { get; init; }

    /// <summary>The locations controlled from this one, which are left with no controlling station.</summary>
    public required IReadOnlyList<OperationLocation> NoLongerControlled { get; init; }

    /// <summary>The locations whose cargo is served from this one, which are left served from nowhere.</summary>
    public required IReadOnlyList<OperationLocation> NoLongerCargoServed { get; init; }

    /// <summary>The locations whose lock key is held here, which lose the key.</summary>
    public required IReadOnlyList<OperationLocation> LockKeysRemoved { get; init; }

    /// <summary>The train categories whose stop pattern names the location, which drop it.</summary>
    public required IReadOnlyList<TrainCategory> StopPatternsChanged { get; init; }

    /// <summary>The cargo flow descriptions with the location as an origin or destination, which drop it.</summary>
    public required IReadOnlyList<CargoFlowOptions> CargoFlowOptionsChanged { get; init; }

    /// <summary>
    /// The shunters stationed at the location, which are deleted with it, together with who was to bring them.
    /// </summary>
    public IReadOnlyList<ScheduledObject> ShuntersRemoved { get; init; } = [];
}

/// <summary>
/// What deleting a location does to one train calling there.
/// </summary>
/// <param name="Train">The train.</param>
/// <param name="Kind">What happens to it.</param>
public readonly record struct TrainCallRemoval(Train Train, TrainCallRemovalKind Kind)
{
    /// <summary>Whether the train cannot lose its calls at the location and so blocks the delete.</summary>
    public bool IsBlocked => Kind.HasFlag(TrainCallRemovalKind.Blocked);

    /// <summary>Whether the train goes with the location.</summary>
    public bool IsDeleted => Kind.HasFlag(TrainCallRemovalKind.TrainDeleted);
}

/// <summary>
/// What deleting a location does to a train calling there. A train may start, run through and end at
/// the same location, so the values combine.
/// </summary>
[Flags]
public enum TrainCallRemovalKind
{
    /// <summary>Nothing.</summary>
    None = 0,

    /// <summary>The train ran through the location and now runs straight past where it was.</summary>
    Intermediate = 1,

    /// <summary>The train started at the location and now starts at its next call.</summary>
    Origin = 2,

    /// <summary>The train ended at the location and now ends at its previous call.</summary>
    Terminus = 4,

    /// <summary>The train has no route left without the location and is deleted with it.</summary>
    TrainDeleted = 8,

    /// <summary>
    /// The train's route cannot be joined up without the location: it reverses there, or runs through
    /// it as a junction.
    /// </summary>
    Blocked = 16,
}
