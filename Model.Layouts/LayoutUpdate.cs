namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Takes the differences of a <see cref="LayoutComparison"/> that change nothing the timetable depends on but distances
/// and times into the current layout. What a location and its tracks are is copied from the newer layout; what is
/// decided for them in the current layout stays.
/// </summary>
public static class LayoutUpdate
{
    /// <summary>
    /// Applies every <see cref="LayoutChangeHandling.Automatic"/> change of <paramref name="comparison"/> to its current
    /// layout, and takes the hall plan's identity, revision and modules. A stretch takes the newer distance and speed
    /// unless the planner has entered its own (<see cref="TrackStretch.IsDistanceEntered"/>,
    /// <see cref="TrackStretch.IsSpeedEntered"/>). The running time is the planner's to work out again
    /// (<c>plan.UpdateFromHallPlan</c>).
    /// </summary>
    /// <returns>The changes applied.</returns>
    public static IReadOnlyList<LayoutChange> ApplyAutomatic(LayoutComparison comparison)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        var layout = comparison.Current;
        var applied = new List<LayoutChange>();
        foreach (var change in comparison.Changes.Where(c => c.Handling == LayoutChangeHandling.Automatic))
        {
            switch (change.Kind)
            {
                case LayoutChangeKind.LocationChanged:
                    CopyWhatItIs(change.SourceLocation!, change.Location!);
                    break;
                case LayoutChangeKind.TrackChanged:
                    CopyWhatItIs(change.SourceTrack!, change.Track!);
                    break;
                case LayoutChangeKind.TrackAdded:
                    var track = change.SourceTrack!.Copy();
                    track.Id = FreeTrackId(layout, track.Id);
                    track.Station = change.Location!;
                    track.StationId = change.Location!.Id;
                    change.Location!.Tracks.Add(track);
                    break;
                case LayoutChangeKind.StretchChanged:
                    Update(change.Stretch!, change.SourceStretch!, comparison.Matched);
                    break;
                default:
                    continue;
            }
            applied.Add(change);
        }
        TakeHallPlan(comparison);
        return applied;
    }

    /// <summary>
    /// Copies what <paramref name="source"/> is onto <paramref name="location"/>: name, signature, country, what can be
    /// done there, and for a station or a signal controlled location what only that kind has. What is decided for the
    /// location in its layout is left as it is. The two are expected to be of the same kind.
    /// </summary>
    public static void CopyWhatItIs(OperationLocation source, OperationLocation location)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(location);
        location.Name = source.Name;
        location.Signature = source.Signature;
        location.CountryCode = source.CountryCode;
        location.HasPassengerExchange = source.HasPassengerExchange;
        location.HasCargoExchange = source.HasCargoExchange;
        location.IsChangingTrainDirectionPossible = source.IsChangingTrainDirectionPossible;
        location.IsTerminus = source.IsTerminus;
        location.IsHarbour = source.IsHarbour;
        location.IsKeyRequired = source.IsKeyRequired;
        location.Centre = source.Centre;
        if (location is Station station && source is Station sourceStation)
        {
            station.IsShadow = sourceStation.IsShadow;
            station.HasTurntable = sourceStation.HasTurntable;
            station.HasSignals = sourceStation.HasSignals;
        }
        if (location is IndustrialArea area && source is IndustrialArea sourceArea)
            area.HasSignals = sourceArea.HasSignals;
        if (location is SignalControlledLocation signal && source is SignalControlledLocation sourceSignal)
            signal.TrainsCanCross = sourceSignal.TrainsCanCross;
    }

    /// <summary>Copies what <paramref name="source"/> is onto <paramref name="track"/>; its usage and locking, decided for the layout, stay.</summary>
    public static void CopyWhatItIs(StationTrack source, StationTrack track)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(track);
        track.Designation = source.Designation;
        track.Length = source.Length;
        track.PlatformLength = source.PlatformLength;
        track.IsMain = source.IsMain;
        track.IsStub = source.IsStub;
        track.IsScheduled = source.IsScheduled;
        track.Direction = source.Direction;
        track.SpeedLimit = source.SpeedLimit;
    }

    private static void Update(TrackStretch stretch, TrackStretch source, IReadOnlyDictionary<OperationLocation, OperationLocation> matched)
    {
        // The newer stretch may run the other way: its start is then the current stretch's end.
        var isTurned = !ReferenceEquals(matched[stretch.Start], source.Start);
        stretch.TracksCount = source.TracksCount;
        stretch.StartSide = isTurned ? source.EndSide : source.StartSide;
        stretch.EndSide = isTurned ? source.StartSide : source.EndSide;
        stretch.ModuleIds = isTurned ? [.. source.ModuleIds.Reverse()] : [.. source.ModuleIds];
        stretch.MeasuredDistance = source.MeasuredDistance;
        stretch.MeasuredSpeed = source.MeasuredSpeed;
        if (!stretch.IsDistanceEntered && Math.Abs(stretch.Distance - source.Distance) >= 0.0005) stretch.Distance = source.Distance;
        if (!stretch.IsSpeedEntered && stretch.Speed != source.Speed) stretch.Speed = source.Speed;
    }

    /// <summary>
    /// The hall plan's identity, revision, scale and standard, its modules, and the order of the locations along its
    /// lines; a module or a line that refers to a location of the newer layout refers to the current layout's.
    /// </summary>
    private static void TakeHallPlan(LayoutComparison comparison)
    {
        var layout = comparison.Current;
        var source = comparison.Source;
        var bySource = new Dictionary<OperationLocation, OperationLocation>(ReferenceEqualityComparer.Instance);
        foreach (var (current, matched) in comparison.Matched) bySource[matched] = current;
        layout.SourceId = source.SourceId;
        layout.SourceRevision = source.SourceRevision;
        layout.Scale = source.Scale;
        layout.StandardName = source.StandardName;
        var sourceLocations = source.OperationLocations.GroupBy(l => l.Id).ToDictionary(g => g.Key, g => g.First());
        layout.Modules = [.. source.Modules.Select(module =>
        {
            var copy = module.Copy();
            copy.OperationLocationId = module.OperationLocationId is int id && sourceLocations.GetValueOrDefault(id) is { } location && bySource.TryGetValue(location, out var current)
                ? current.Id
                : null;
            return copy;
        })];
        layout.StretchLines = [.. source.StretchLines.Select(line => Matched(line, bySource, source))];
    }

    /// <summary>
    /// A copy of <paramref name="line"/> of the newer layout, with the ids of the current layout's locations. A location
    /// of the newer layout that the current one doesn't have yet is passed by: the legs on either side of it become one,
    /// as the current layout's stretch still runs past it. A leg to such a location at the end of the line is dropped.
    /// </summary>
    private static StretchLine Matched(StretchLine line, Dictionary<OperationLocation, OperationLocation> bySource, Layout source)
    {
        var locations = source.OperationLocations.GroupBy(l => l.Id).ToDictionary(g => g.Key, g => g.First());
        int? Current(int sourceId) => locations.GetValueOrDefault(sourceId) is { } location && bySource.TryGetValue(location, out var current) ? current.Id : null;
        var legs = new List<LineLeg>();
        int? pendingStart = null;
        var previousEnd = (int?)null;
        foreach (var leg in line.Legs)
        {
            var start = Current(leg.StartId);
            var end = Current(leg.EndId);
            if (previousEnd != leg.StartId) pendingStart = null; // a gap in the line
            if (start is int s) pendingStart = s;
            if (end is int e)
            {
                if (pendingStart is int from && from != e) legs.Add(new(from, e));
                pendingStart = e;
            }
            previousEnd = leg.EndId;
        }
        return new() { Id = line.Id, Name = line.Name, Legs = legs };
    }

    private static int FreeTrackId(Layout layout, int wanted)
    {
        var used = layout.OperationLocations.SelectMany(l => l.Tracks).Select(t => t.Id).ToHashSet();
        return wanted > 0 && !used.Contains(wanted) ? wanted : used.DefaultIfEmpty(0).Max() + 1;
    }
}
