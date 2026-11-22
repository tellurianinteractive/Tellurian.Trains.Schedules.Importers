namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>What kind of difference a <see cref="LayoutChange"/> is.</summary>
public enum LayoutChangeKind
{
    /// <summary>The distance or the number of tracks of a stretch between the same two locations is different.</summary>
    StretchChanged = 0,

    /// <summary>What a location is, not what is decided for it, is different: name, signature, exchange, turntable…</summary>
    LocationChanged = 1,

    /// <summary>The location is of another kind.</summary>
    KindChanged = 2,

    /// <summary>A location has a track it didn't have.</summary>
    TrackAdded = 3,

    /// <summary>What a track is, its length, platform, main or stub, is different.</summary>
    TrackChanged = 4,

    /// <summary>A location no longer has a track.</summary>
    TrackRemoved = 5,

    /// <summary>A location is new on a stretch between two locations that were neighbours: the stretch is split.</summary>
    LocationInserted = 6,

    /// <summary>A location between two others is gone, and they are neighbours: its two stretches become one.</summary>
    LocationRemoved = 7,

    /// <summary>A location is new elsewhere than on a stretch, e.g. at the end of a new branch.</summary>
    LocationAdded = 8,

    /// <summary>A location is gone, and its neighbours are not joined instead.</summary>
    LocationDropped = 9,

    /// <summary>A stretch between two locations that weren't neighbours.</summary>
    StretchAdded = 10,

    /// <summary>A stretch between two locations that are no longer neighbours.</summary>
    StretchRemoved = 11,
}

/// <summary>How a <see cref="LayoutChange"/> can be taken into a layout that has a timetable.</summary>
public enum LayoutChangeHandling
{
    /// <summary>It changes nothing the timetable depends on but distances and times, and is applied at once (<see cref="LayoutUpdate.ApplyAutomatic"/>).</summary>
    Automatic = 0,

    /// <summary>It changes the timetable in a known way, e.g. calls are moved or removed; applied after the planner has agreed.</summary>
    Confirm = 1,

    /// <summary>The order of the locations doesn't hold any more; it can't be applied to the timetable as it is.</summary>
    Manual = 2,
}

/// <summary>
/// One difference between the layout a timetable is planned on and a newer version of it, e.g. from its hall plan.
/// What it concerns is given in both layouts where it exists in both: <see cref="Location"/>, <see cref="Track"/> and
/// <see cref="Stretch"/> are of the current layout, the <c>Source</c> ones of the newer.
/// </summary>
/// <param name="Kind">What kind of difference it is.</param>
/// <param name="Handling">How it can be taken into the current layout.</param>
public sealed record LayoutChange(LayoutChangeKind Kind, LayoutChangeHandling Handling)
{
    /// <summary>The location of the current layout it concerns.</summary>
    public OperationLocation? Location { get; init; }

    /// <summary>The location of the newer layout it concerns.</summary>
    public OperationLocation? SourceLocation { get; init; }

    /// <summary>The track of the current layout it concerns.</summary>
    public StationTrack? Track { get; init; }

    /// <summary>The track of the newer layout it concerns.</summary>
    public StationTrack? SourceTrack { get; init; }

    /// <summary>The stretch of the current layout it concerns; for a location inserted, the stretch that is split.</summary>
    public TrackStretch? Stretch { get; init; }

    /// <summary>The stretch of the newer layout it concerns; for a location removed, the stretch that joins its neighbours.</summary>
    public TrackStretch? SourceStretch { get; init; }

    /// <summary>
    /// For a location inserted, the two stretches of the newer layout on either side of it; for a location removed,
    /// the two stretches of the current layout that become one.
    /// </summary>
    public IReadOnlyList<TrackStretch> Stretches { get; init; } = [];

    /// <summary>The names of the properties that are different, for a location, a track or a stretch changed.</summary>
    public IReadOnlyList<string> Properties { get; init; } = [];
}

/// <summary>
/// The differences between the layout a timetable is planned on and a newer version of it, sorted by what they do to
/// the timetable (<c>Documentation/ModelHarmonisation.md</c>, step 6). As long as the order of the locations holds, a
/// newer hall plan changes only distances and times, and what a location and its tracks are; a location inserted on
/// a stretch, or removed from between two others, changes the timetable in a known way; anything else is left to the
/// planner.
/// </summary>
public sealed class LayoutComparison
{
    private LayoutComparison(Layout current, Layout source, IReadOnlyList<LayoutChange> changes, IReadOnlyDictionary<OperationLocation, OperationLocation> matched)
    {
        Current = current;
        Source = source;
        Changes = changes;
        Matched = matched;
    }

    /// <summary>The layout the timetable is planned on.</summary>
    public Layout Current { get; }

    /// <summary>The newer layout.</summary>
    public Layout Source { get; }

    /// <summary>Every difference found.</summary>
    public IReadOnlyList<LayoutChange> Changes { get; }

    /// <summary>The locations of the current layout and those of the newer layout they are the same as.</summary>
    public IReadOnlyDictionary<OperationLocation, OperationLocation> Matched { get; }

    /// <summary>The order of the locations holds: no difference has to be left to the planner.</summary>
    public bool IsOrderIntact => Changes.All(c => c.Handling != LayoutChangeHandling.Manual);

    /// <summary>Nothing is different.</summary>
    public bool IsUnchanged => Changes.Count == 0;

    /// <summary>The differences between <paramref name="current"/> and the newer <paramref name="source"/>.</summary>
    public static LayoutComparison Compare(Layout current, Layout source)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(source);
        var matched = Match(current, source);
        var reverse = matched.ToDictionary(m => m.Value, m => m.Key);
        var changes = new List<LayoutChange>();

        foreach (var (location, sourceLocation) in matched)
        {
            CompareLocation(location, sourceLocation, changes);
            CompareTracks(location, sourceLocation, changes);
        }

        var removed = current.OperationLocations.Where(l => !matched.ContainsKey(l)).ToList();
        var added = source.OperationLocations.Where(l => !reverse.ContainsKey(l)).ToList();
        var explainedCurrent = new HashSet<TrackStretch>(ReferenceEqualityComparer.Instance);
        var explainedSource = new HashSet<TrackStretch>(ReferenceEqualityComparer.Instance);

        foreach (var stretch in current.TrackStretches)
        {
            if (!matched.TryGetValue(stretch.Start, out var start) || !matched.TryGetValue(stretch.End, out var end)) continue;
            if (Between(source, start, end) is not { } sourceStretch) continue;
            explainedCurrent.Add(stretch);
            explainedSource.Add(sourceStretch);
            var properties = new List<string>();
            if (!Same(stretch.Distance, sourceStretch.Distance)) properties.Add(nameof(TrackStretch.Distance));
            if (stretch.TracksCount != sourceStretch.TracksCount) properties.Add(nameof(TrackStretch.TracksCount));
            if (properties.Count > 0)
                changes.Add(new(LayoutChangeKind.StretchChanged, LayoutChangeHandling.Automatic) { Stretch = stretch, SourceStretch = sourceStretch, Properties = properties });
        }

        foreach (var location in removed)
        {
            var touching = current.TrackStretches.Where(s => Touches(s, location)).ToList();
            var neighbours = touching.Select(s => Other(s, location)).ToList();
            if (touching.Count == 2 && matched.TryGetValue(neighbours[0], out var first) && matched.TryGetValue(neighbours[1], out var second)
                && Between(source, first, second) is { } joined && !explainedSource.Contains(joined))
            {
                explainedCurrent.UnionWith(touching);
                explainedSource.Add(joined);
                changes.Add(new(LayoutChangeKind.LocationRemoved, LayoutChangeHandling.Confirm) { Location = location, SourceStretch = joined, Stretches = touching });
            }
            else
            {
                changes.Add(new(LayoutChangeKind.LocationDropped, LayoutChangeHandling.Manual) { Location = location });
            }
        }

        foreach (var sourceLocation in added)
        {
            var touching = source.TrackStretches.Where(s => Touches(s, sourceLocation)).ToList();
            var neighbours = touching.Select(s => Other(s, sourceLocation)).ToList();
            if (touching.Count == 2 && reverse.TryGetValue(neighbours[0], out var first) && reverse.TryGetValue(neighbours[1], out var second)
                && Between(current, first, second) is { } split && !explainedCurrent.Contains(split))
            {
                explainedSource.UnionWith(touching);
                explainedCurrent.Add(split);
                changes.Add(new(LayoutChangeKind.LocationInserted, LayoutChangeHandling.Confirm) { SourceLocation = sourceLocation, Stretch = split, Stretches = touching });
            }
            else
            {
                changes.Add(new(LayoutChangeKind.LocationAdded, LayoutChangeHandling.Manual) { SourceLocation = sourceLocation });
            }
        }

        foreach (var stretch in current.TrackStretches.Where(s => !explainedCurrent.Contains(s) && matched.ContainsKey(s.Start) && matched.ContainsKey(s.End)))
            changes.Add(new(LayoutChangeKind.StretchRemoved, LayoutChangeHandling.Manual) { Stretch = stretch });
        foreach (var stretch in source.TrackStretches.Where(s => !explainedSource.Contains(s) && reverse.ContainsKey(s.Start) && reverse.ContainsKey(s.End)))
            changes.Add(new(LayoutChangeKind.StretchAdded, LayoutChangeHandling.Manual) { SourceStretch = stretch });

        return new(current, source, changes, matched);
    }

    /// <summary>
    /// The location of <paramref name="source"/> each location of <paramref name="current"/> is: by an external
    /// reference they share, then by id when both are derived from the same hall plan, then by signature.
    /// </summary>
    private static Dictionary<OperationLocation, OperationLocation> Match(Layout current, Layout source)
    {
        var result = new Dictionary<OperationLocation, OperationLocation>(ReferenceEqualityComparer.Instance);
        var free = source.OperationLocations.ToList();
        var sameSource = current.SourceId is { } id && id == source.SourceId;
        foreach (var rule in new Func<OperationLocation, OperationLocation, bool>[]
        {
            SharesExternalReference,
            (a, b) => sameSource && a.Id == b.Id,
            (a, b) => string.Equals(a.Signature, b.Signature, StringComparison.OrdinalIgnoreCase),
        })
        {
            foreach (var location in current.OperationLocations.Where(l => !result.ContainsKey(l)))
            {
                if (free.FirstOrDefault(other => rule(location, other)) is not { } match) continue;
                result.Add(location, match);
                free.Remove(match);
            }
        }
        return result;
    }

    // By the Module Registry's station id when both have one, else by an entry of the same system with the same content;
    // never by raw text, so a reordered or extended entry still matches.
    private static bool SharesExternalReference(OperationLocation a, OperationLocation b) =>
        ExternalReferences.ReferToTheSame(a.ExternalReferences, b.ExternalReferences, ExternalReferences.StationId);

    private static void CompareLocation(OperationLocation location, OperationLocation source, List<LayoutChange> changes)
    {
        if (location.Kind != source.Kind)
            changes.Add(new(LayoutChangeKind.KindChanged, LayoutChangeHandling.Confirm) { Location = location, SourceLocation = source });
        var properties = new List<string>();
        void Check(bool same, string name)
        {
            if (!same) properties.Add(name);
        }

        Check(location.Name == source.Name, nameof(OperationLocation.Name));
        Check(string.Equals(location.Signature, source.Signature, StringComparison.Ordinal), nameof(OperationLocation.Signature));
        Check(location.CountryCode == source.CountryCode, nameof(OperationLocation.CountryCode));
        Check(location.HasPassengerExchange == source.HasPassengerExchange, nameof(OperationLocation.HasPassengerExchange));
        Check(location.HasCargoExchange == source.HasCargoExchange, nameof(OperationLocation.HasCargoExchange));
        Check(location.IsChangingTrainDirectionPossible == source.IsChangingTrainDirectionPossible, nameof(OperationLocation.IsChangingTrainDirectionPossible));
        Check(location.IsTerminus == source.IsTerminus, nameof(OperationLocation.IsTerminus));
        Check(location.IsHarbour == source.IsHarbour, nameof(OperationLocation.IsHarbour));
        Check(location.IsKeyRequired == source.IsKeyRequired, nameof(OperationLocation.IsKeyRequired));
        Check(location.Centre == source.Centre, nameof(OperationLocation.Centre));
        if (location is Station station && source is Station sourceStation)
        {
            Check(station.IsShadow == sourceStation.IsShadow, nameof(Station.IsShadow));
            Check(station.HasTurntable == sourceStation.HasTurntable, nameof(Station.HasTurntable));
        }
        if (location is SignalControlledLocation signal && source is SignalControlledLocation sourceSignal)
            Check(signal.TrainsCanCross == sourceSignal.TrainsCanCross, nameof(SignalControlledLocation.TrainsCanCross));
        if (properties.Count > 0)
            changes.Add(new(LayoutChangeKind.LocationChanged, LayoutChangeHandling.Automatic) { Location = location, SourceLocation = source, Properties = properties });
    }

    private static void CompareTracks(OperationLocation location, OperationLocation source, List<LayoutChange> changes)
    {
        var free = source.Tracks.ToList();
        foreach (var track in location.Tracks)
        {
            var match = free.FirstOrDefault(t => t.Id == track.Id && string.Equals(t.Designation, track.Designation, StringComparison.OrdinalIgnoreCase))
                ?? free.FirstOrDefault(t => string.Equals(t.Designation, track.Designation, StringComparison.OrdinalIgnoreCase));
            if (match is null)
            {
                changes.Add(new(LayoutChangeKind.TrackRemoved, LayoutChangeHandling.Confirm) { Location = location, SourceLocation = source, Track = track });
                continue;
            }
            free.Remove(match);
            var properties = TrackDifferences(track, match);
            if (properties.Count > 0)
                changes.Add(new(LayoutChangeKind.TrackChanged, LayoutChangeHandling.Automatic) { Location = location, SourceLocation = source, Track = track, SourceTrack = match, Properties = properties });
        }
        foreach (var track in free)
            changes.Add(new(LayoutChangeKind.TrackAdded, LayoutChangeHandling.Automatic) { Location = location, SourceLocation = source, SourceTrack = track });
    }

    /// <summary>The properties of what a track is in which <paramref name="track"/> and <paramref name="source"/> differ.</summary>
    internal static IReadOnlyList<string> TrackDifferences(StationTrack track, StationTrack source)
    {
        var properties = new List<string>();
        void Check(bool same, string name)
        {
            if (!same) properties.Add(name);
        }

        Check(track.Designation == source.Designation, nameof(StationTrack.Designation));
        Check(Same(track.Length, source.Length), nameof(StationTrack.Length));
        Check(Same(track.PlatformLength, source.PlatformLength), nameof(StationTrack.PlatformLength));
        Check(track.IsMain == source.IsMain, nameof(StationTrack.IsMain));
        Check(track.IsStub == source.IsStub, nameof(StationTrack.IsStub));
        Check(track.IsScheduled == source.IsScheduled, nameof(StationTrack.IsScheduled));
        Check(track.Direction == source.Direction, nameof(StationTrack.Direction));
        Check(track.SpeedLimit == source.SpeedLimit, nameof(StationTrack.SpeedLimit));
        return properties;
    }

    private static TrackStretch? Between(Layout layout, OperationLocation a, OperationLocation b) =>
        layout.TrackStretches.FirstOrDefault(s => (ReferenceEquals(s.Start, a) && ReferenceEquals(s.End, b)) || (ReferenceEquals(s.Start, b) && ReferenceEquals(s.End, a)));

    private static bool Touches(TrackStretch stretch, OperationLocation location) =>
        ReferenceEquals(stretch.Start, location) || ReferenceEquals(stretch.End, location);

    private static OperationLocation Other(TrackStretch stretch, OperationLocation location) =>
        ReferenceEquals(stretch.Start, location) ? stretch.End : stretch.Start;

    private static bool Same(double a, double b) => Math.Abs(a - b) < 0.0005;
}
