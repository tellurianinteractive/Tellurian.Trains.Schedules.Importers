using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Drawings.Modules;
using StationTrack = Tellurian.Trains.Schedules.Model.Layouts.StationTrack;

namespace Tellurian.Trains.Schedules.Model.Drawings.Tracks;

/// <summary>
/// The centre lines of a drawing's track elements, each with the <see cref="StationTrack"/> it is part of, and how
/// they are joined. A plain element (straight, curve, flex) is part of the track it refers to. A turnout, crossing or
/// double slip is shared: each of its routes is part of the track that continues from the route's end, so a turnout on
/// the main track gives its diverging route to the siding. Where nothing continues, the route follows its element.
/// </summary>
internal sealed class TrackNetwork
{
    /// <summary>How far apart two track ends may be and still be joined, mm (NF-8).</summary>
    public const double JoinTolerance = ModuleGeometry.CornerTolerance;

    /// <summary>A turnout seldom leads to more than a few others before a plain track; this stops a ring of turnouts.</summary>
    private const int MaxTurnoutsInARow = 16;

    private readonly IReadOnlyList<TrackElement> _elements;

    public TrackNetwork(ModuleDrawing drawing)
    {
        ArgumentNullException.ThrowIfNull(drawing);
        _elements = drawing.TrackElements;
        Lines = [.. _elements.SelectMany(LinesOf)];
    }

    /// <summary>One centre line: a route of an element.</summary>
    public sealed record Line(TrackElement Element, SegmentPath Path, int? TrackId)
    {
        public IEnumerable<Point> Ends => [Path.Start, Path.End];
    }

    public IReadOnlyList<Line> Lines { get; }

    public IEnumerable<Line> LinesOfTrack(int trackId) => Lines.Where(l => l.TrackId == trackId);

    /// <summary>The lines that have an end at <paramref name="point"/>, other than <paramref name="except"/>.</summary>
    public IEnumerable<Line> LinesEndingAt(Point point, Line? except = null) =>
        Lines.Where(l => !ReferenceEquals(l, except) && l.Ends.Any(e => e.IsCloseTo(point, JoinTolerance)));

    /// <summary>The lines that can be reached from <paramref name="from"/> through joined ends, including themselves.</summary>
    public IReadOnlyCollection<Line> ReachableFrom(IEnumerable<Line> from)
    {
        var reached = new HashSet<Line>(from, ReferenceEqualityComparer.Instance);
        var queue = new Queue<Line>(reached);
        while (queue.Count > 0)
        {
            var line = queue.Dequeue();
            foreach (var next in line.Ends.SelectMany(e => LinesEndingAt(e, line)))
            {
                if (reached.Add(next)) queue.Enqueue(next);
            }
        }
        return reached;
    }

    /// <summary>
    /// True if <paramref name="point"/> is on the joint face of an exit: where a track leaves the module.
    /// The face is as wide as the end plate, when the exit's end profile is known.
    /// </summary>
    public static bool IsAtExit(Point point, ModuleDrawing drawing, Func<Guid, EndProfile?> endProfiles)
    {
        foreach (var exit in drawing.Exits)
        {
            var offset = point - exit.Pose.Position;
            if (Math.Abs(offset.Dot(exit.Pose.Forward)) > JoinTolerance) continue;
            var halfWidth = endProfiles(exit.EndProfile.Id) is { } profile ? EndProfileAnalysis.PlateWidth(profile) / 2 : double.PositiveInfinity;
            if (Math.Abs(offset.Dot(exit.Pose.Left)) <= halfWidth + JoinTolerance) return true;
        }
        return false;
    }

    private static bool IsPlain(TrackElement element) => element is StraightTrack or CurvedTrack or FlexTrack;

    private IEnumerable<Line> LinesOf(TrackElement element)
    {
        var routes = element.Routes();
        return IsPlain(element)
            ? routes.Select(r => new Line(element, r, element.StationTrackId))
            : routes.Select(r => new Line(element, r, TrackOfRoute(element, r)));
    }

    /// <summary>The track a route of a shared element is part of: the one that continues from either of its ends.</summary>
    private int? TrackOfRoute(TrackElement element, SegmentPath route)
    {
        // A turnout's routes all start at its points, so only the far end tells them apart.
        IEnumerable<Point> ends = element is Turnout ? [route.End] : [route.End, route.Start];
        var isJoined = false;
        foreach (var end in ends)
        {
            var (found, trackId) = Continuation(element, end, MaxTurnoutsInARow);
            if (found) return trackId;
            isJoined |= trackId is null && ElementsAt(end, element).Any();
        }
        return isJoined ? null : element.StationTrackId;
    }

    /// <summary>
    /// The plain element that continues from <paramref name="point"/>, looking through turnouts entered from one of
    /// their route ends and through crossings. A turnout entered at its points, or a double slip, can continue either
    /// way, so nothing is found there.
    /// </summary>
    private (bool Found, int? TrackId) Continuation(TrackElement from, Point point, int remaining)
    {
        if (remaining == 0) return (false, null);
        foreach (var next in ElementsAt(point, from))
        {
            switch (next)
            {
                case StraightTrack or CurvedTrack or FlexTrack:
                    return (true, next.StationTrackId);
                case Turnout turnout when !turnout.Start.Position.IsCloseTo(point, JoinTolerance):
                    return Continuation(next, turnout.Start.Position, remaining - 1);
                case Crossing crossing:
                    var route = crossing.Routes().First(r => r.Start.IsCloseTo(point, JoinTolerance) || r.End.IsCloseTo(point, JoinTolerance));
                    return Continuation(next, route.Start.IsCloseTo(point, JoinTolerance) ? route.End : route.Start, remaining - 1);
            }
        }
        return (false, null);
    }

    private IEnumerable<TrackElement> ElementsAt(Point point, TrackElement except) =>
        _elements.Where(e => !ReferenceEquals(e, except) && e is not BufferStop && e.ConnectionPoints().Any(c => c.Position.IsCloseTo(point, JoinTolerance)));
}
