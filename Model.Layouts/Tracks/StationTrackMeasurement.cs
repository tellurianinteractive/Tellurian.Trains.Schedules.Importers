using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;
using StationTrack = Tellurian.Trains.Schedules.Model.Layouts.StationTrack;
using static Tellurian.Trains.Schedules.Model.Layouts.LayoutPartCopying;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tracks;

/// <summary>
/// What a track of a module's list (<see cref="StationTrack"/>) gets from the drawing: its length and whether it is a stub.
/// <para>
/// <b>Length.</b> A track ends where a train on another track can still pass freely, the fouling point, not at the
/// points of a turnout. A part of the track's centre lines is <i>free</i> where it is at least the standard's
/// <see cref="ModuleStandard.FoulingDistance"/> to the side of every centre line of another track. The length is that
/// of the longest unbroken free stretch: for a track between two turnouts, the stretch between their fouling points.
/// The stem of a turnout, where its routes run together, is part of no track, and the track outside the turnouts
/// doesn't add to the length.
/// </para>
/// <para>
/// <b>Stub.</b> A track is a stub when one of its ends is neither joined to another track element nor at an exit:
/// it ends at a buffer stop or just ends.
/// </para>
/// </summary>
public static class StationTrackMeasurement
{
    /// <summary>The distance between the points along a centre line where its clearance is tested, mm.</summary>
    public const double SampleStep = 5;

    /// <summary>How closely a fouling point is found along the track, mm.</summary>
    public const double FoulingPointPrecision = 0.01;

    private const double MillimetresPerMetre = 1000;

    /// <summary>
    /// The centre distance at which trains on two tracks pass each other freely, mm: the standard's own value, or else
    /// the track distance of its default double-track end profile. Null when neither is known; a track's length is
    /// then the whole of it, with its turnouts.
    /// </summary>
    public static double? FoulingDistance(ModuleStandard standard, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(standard);
        ArgumentNullException.ThrowIfNull(endProfiles);
        if (standard.FoulingDistance is { } distance) return distance;
        if (standard.DefaultDoubleTrackEndProfileId is not { } id || endProfiles(id) is not { } profile || profile.Tracks.Count < 2) return null;
        var offsets = profile.Tracks.Select(t => t.Offset).Order().ToList();
        return offsets.Zip(offsets.Skip(1), (a, b) => b - a).Min();
    }

    /// <summary>The length of the track that a train can use, mm.</summary>
    public static double UsableLength(ModuleDrawing drawing, int trackId, double? foulingDistance) =>
        UsableLength(new TrackNetwork(drawing), trackId, foulingDistance);

    /// <summary>True if the track has an end that is neither joined to another element nor at an exit.</summary>
    public static bool IsStub(ModuleDrawing drawing, int trackId, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(endProfiles);
        return IsStub(new TrackNetwork(drawing), drawing, trackId, endProfiles);
    }

    /// <summary>
    /// The module with the length and the stub mark of every track as the drawing gives them, except those the user
    /// has entered (<see cref="ModuleDrawing.TracksWithEnteredLength"/>, <see cref="ModuleDrawing.TracksWithEnteredStub"/>).
    /// Lengths are written in metres, to the millimetre. Unchanged when nothing differs.
    /// </summary>
    public static Module WithMeasuredTracks(this Module module, double? foulingDistance, Func<Guid, EndProfile?> endProfiles)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(endProfiles);
        if (module.Tracks.Count == 0) return module;
        var drawing = module.Drawing;
        var network = new TrackNetwork(drawing);
        var measured = module.Tracks.Select(track => (track,
            Length: drawing.TracksWithEnteredLength.Contains(track.Id) ? track.Length : Metres(UsableLength(network, track.Id, foulingDistance)),
            IsStub: drawing.TracksWithEnteredStub.Contains(track.Id) ? track.IsStub : IsStub(network, drawing, track.Id, endProfiles))).ToList();
        // The tracks compare by designation, so whether measuring changed anything is told by what it measures.
        if (measured.All(m => m.track.Length == m.Length && m.track.IsStub == m.IsStub)) return module;
        return module.With(m => m.Tracks = [.. measured.Select(x => x.track.With(t =>
        {
            t.Length = x.Length;
            t.IsStub = x.IsStub;
        }))]);
    }

    /// <summary>A length in mm as metres, rounded to the millimetre.</summary>
    public static double Metres(double millimetres) => Math.Round(millimetres) / MillimetresPerMetre;

    /// <summary>A free part of a centre line, between two distances along it.</summary>
    private sealed record Stretch(TrackNetwork.Line Line, double From, double To)
    {
        public double Length => To - From;

        /// <summary>The ends of the line that the stretch runs all the way to.</summary>
        public IEnumerable<Point> EndsReached()
        {
            if (From <= Tolerance.Length) yield return Line.Path.Start;
            if (To >= Line.Path.Length - Tolerance.Length) yield return Line.Path.End;
        }
    }

    private static double UsableLength(TrackNetwork network, int trackId, double? foulingDistance)
    {
        var own = network.LinesOfTrack(trackId).ToList();
        var clearance = foulingDistance is > 0 ? foulingDistance.Value : 0;
        var others = clearance > 0
            ? network.Lines.Where(l => l.TrackId != trackId).Select(l => (l.Path, Bounds: l.Path.Bounds.Inflated(clearance))).ToList()
            : [];
        var stretches = own.SelectMany(l => FreeStretches(l, others, clearance)).ToList();
        return LongestUnbroken(stretches);
    }

    /// <summary>The parts of <paramref name="line"/> that are at least <paramref name="clearance"/> to the side of all <paramref name="others"/>.</summary>
    private static IEnumerable<Stretch> FreeStretches(TrackNetwork.Line line, IReadOnlyList<(SegmentPath Path, BoundingBox Bounds)> others, double clearance)
    {
        var path = line.Path;
        bool IsFree(double at)
        {
            var point = path.PoseAt(at).Position;
            return others.All(o => !o.Bounds.Contains(point) || IsClear(point, o.Path, clearance));
        }

        var length = path.Length;
        var steps = Math.Max(1, (int)Math.Ceiling(length / SampleStep));
        var from = 0.0;
        var fromIsFree = IsFree(from);
        double? start = fromIsFree ? 0 : null;
        for (var i = 1; i <= steps; i++)
        {
            var to = i == steps ? length : length * i / steps;
            var toIsFree = IsFree(to);
            if (fromIsFree != toIsFree)
            {
                var change = Change(IsFree, from, to, fromIsFree);
                if (start is { } begun)
                {
                    yield return new(line, begun, change);
                    start = null;
                }
                else
                {
                    start = change;
                }
            }
            (from, fromIsFree) = (to, toIsFree);
        }
        if (start is { } open) yield return new(line, open, length);
    }

    /// <summary>
    /// True if <paramref name="point"/> is at least <paramref name="clearance"/> to the side of <paramref name="other"/>.
    /// A point beyond the end of the other line is clear of it: that line continues the track, or ends before it,
    /// and doesn't run beside it.
    /// </summary>
    private static bool IsClear(Point point, SegmentPath other, double clearance)
    {
        var projection = other.Project(point);
        var distance = point.DistanceTo(projection.Pose.Position);
        if (distance >= clearance - Tolerance.Length) return true;
        var isBeside = Math.Abs(projection.Offset) >= distance - TrackNetwork.JoinTolerance;
        return !isBeside;
    }

    /// <summary>Where between <paramref name="from"/> and <paramref name="to"/> the track stops or starts being free.</summary>
    private static double Change(Func<double, bool> isFree, double from, double to, bool fromIsFree)
    {
        while (to - from > FoulingPointPrecision)
        {
            var middle = (from + to) / 2;
            if (isFree(middle) == fromIsFree) from = middle;
            else to = middle;
        }
        return (from + to) / 2;
    }

    /// <summary>
    /// The length of the longest run of stretches that continue each other: two stretches do when both run to an end
    /// of their lines, and those ends are joined.
    /// </summary>
    private static double LongestUnbroken(IReadOnlyList<Stretch> stretches)
    {
        var run = Enumerable.Range(0, stretches.Count).ToArray();
        int RunOf(int i) => run[i] == i ? i : run[i] = RunOf(run[i]);

        for (var i = 0; i < stretches.Count; i++)
        {
            for (var j = i + 1; j < stretches.Count; j++)
            {
                if (ReferenceEquals(stretches[i].Line, stretches[j].Line)) continue;
                var continues = stretches[i].EndsReached().Any(a => stretches[j].EndsReached().Any(b => a.IsCloseTo(b, TrackNetwork.JoinTolerance)));
                if (continues) run[RunOf(i)] = RunOf(j);
            }
        }
        return stretches.Count == 0 ? 0 : stretches.Select((s, i) => (Run: RunOf(i), s.Length)).GroupBy(s => s.Run).Max(g => g.Sum(s => s.Length));
    }

    private static bool IsStub(TrackNetwork network, ModuleDrawing drawing, int trackId, Func<Guid, EndProfile?> endProfiles) =>
        network.LinesOfTrack(trackId).Any(line => line.Ends.Any(end =>
            !network.LinesEndingAt(end, line).Any() && !TrackNetwork.IsAtExit(end, drawing, endProfiles)));
}
