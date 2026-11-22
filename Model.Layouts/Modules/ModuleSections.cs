using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>
/// The sections of a module drawing (MD-4, MD-4a), derived from its whole outline and its internal joints: each joint
/// is a polyline across the module with its ends on the outline, and cuts one part of the module in two. Sections
/// are numbered 1, 2, 3… along the track (the main path), and each joint names the sections on either side.
/// </summary>
public static class ModuleSections
{
    /// <summary>How far a joint's end may be from the edge it should be on, mm.</summary>
    public const double EdgeTolerance = ModuleGeometry.CornerTolerance;

    /// <summary>
    /// The drawing with sections cut from <paramref name="outline"/> by <paramref name="joints"/>, and the joints naming
    /// the sections they join; null if a joint doesn't cut one part of the module in two, e.g. when it crosses another
    /// joint or leaves the module.
    /// </summary>
    public static ModuleDrawing? WithJoints(ModuleDrawing drawing, SegmentPath outline, IReadOnlyList<InternalJoint> joints)
    {
        ArgumentNullException.ThrowIfNull(drawing);
        ArgumentNullException.ThrowIfNull(outline);
        ArgumentNullException.ThrowIfNull(joints);
        var regions = new List<SegmentPath> { outline };
        foreach (var joint in joints)
        {
            var points = joint.Points;
            var index = regions.FindIndex(r => IsOnEdge(r, points[0]) && IsOnEdge(r, points[^1]) && IsInside(r, points));
            if (index < 0 || Split(regions[index], points) is not { } parts) return null;
            regions[index] = parts.First;
            regions.Insert(index + 1, parts.Second);
        }
        var main = drawing.MainPath?.ToPath();
        var ordered = regions.Select(r => (Region: r, Key: OrderAlongTrack(r, main))).OrderBy(x => x.Key.Along).ThenBy(x => x.Key.Across).Select(x => x.Region).ToList();
        var sections = ordered.Select((r, i) => new Section { Label = $"{i + 1}", Outline = PathDrawing.From(r) }).ToList();
        var named = new List<InternalJoint>(joints.Count);
        foreach (var joint in joints)
        {
            // The sections on either side share the joint's first part as an edge.
            var middle = Point.Lerp(joint.Points[0], joint.Points[1], 0.5);
            var labels = ordered.Select((r, i) => (Region: r, Label: i + 1)).Where(x => IsOnEdge(x.Region, middle)).Select(x => x.Label).Order().ToList();
            if (labels.Count != 2) return null;
            named.Add(joint.With(edited => { edited.FirstSection = $"{labels[0]}"; edited.SecondSection = $"{labels[1]}"; }));
        }
        return drawing.With(edited => { edited.Sections = sections; edited.InternalJoints = named; });
    }

    /// <summary>
    /// A straight joint across the module at right angles to the track, through the point on the track nearest
    /// <paramref name="at"/>, from edge to edge of <paramref name="outline"/>; across the module's longer extent when
    /// it has no main path. Null where no such line crosses the module.
    /// </summary>
    public static InternalJoint? Across(SegmentPath outline, PathDrawing? mainPath, Point at)
    {
        ArgumentNullException.ThrowIfNull(outline);
        Point origin;
        Vector across;
        if (mainPath?.ToPath() is { } main)
        {
            var pose = main.Project(at).Pose;
            (origin, across) = (pose.Position, pose.Left);
        }
        else
        {
            var bounds = outline.Bounds;
            (origin, across) = (at, bounds.Width >= bounds.Height ? new Vector(0, 1) : new Vector(1, 0));
        }
        double? before = null, after = null;
        foreach (var (a, b) in outline.Tessellate().Edges())
        {
            if (Crossing(origin, across, a, b) is not { } t) continue;
            if (t < 0 && (before is null || t > before)) before = t;
            if (t > 0 && (after is null || t < after)) after = t;
        }
        if (before is not { } start || after is not { } end) return null;
        // The tessellated outline is close to the true one; the ends are put exactly on it.
        return new()
        {
            FirstSection = string.Empty,
            SecondSection = string.Empty,
            Start = OnEdge(outline, origin + across * start),
            End = OnEdge(outline, origin + across * end),
        };
    }

    /// <summary>The joint across the module <paramref name="distance"/> along the main path from its start.</summary>
    public static InternalJoint? AcrossAt(SegmentPath outline, PathDrawing mainPath, double distance)
    {
        ArgumentNullException.ThrowIfNull(mainPath);
        var main = mainPath.ToPath();
        return Across(outline, mainPath, main.PoseAt(Math.Clamp(distance, 0, main.Length)).Position);
    }

    /// <summary>
    /// The joint across the module where the main path has turned <paramref name="degrees"/> from the start of its curve
    /// (<see cref="SegmentPath.DistanceAtTurning"/>). On a curve the joint is radial, so the angle is the same measured at
    /// the track or at either edge. Null when the main path doesn't turn one way only (<see cref="IsCurve"/>).
    /// </summary>
    public static InternalJoint? AcrossAtAngle(SegmentPath outline, PathDrawing mainPath, double degrees) =>
        DistanceAtAngle(mainPath, degrees) is { } distance ? AcrossAt(outline, mainPath, distance) : null;

    /// <summary>
    /// The distance along the main path where it has turned <paramref name="degrees"/> from the start of its curve; null
    /// when it doesn't turn one way only (<see cref="IsCurve"/>).
    /// </summary>
    public static double? DistanceAtAngle(PathDrawing mainPath, double degrees)
    {
        ArgumentNullException.ThrowIfNull(mainPath);
        var main = mainPath.ToPath();
        return main.TurnsOneWay ? main.DistanceAtTurning(degrees) : null;
    }

    /// <summary>Whether the module is a curve: its main path turns, and only one way. Then a joint can be placed by its angle.</summary>
    public static bool IsCurve(PathDrawing? mainPath) => mainPath?.ToPath().TurnsOneWay == true;

    /// <summary>
    /// Where a joint is on a curve: how far the main path has turned, in degrees from the start of the curve, where the
    /// joint crosses it. Null when the module isn't a curve (<see cref="IsCurve"/>).
    /// </summary>
    public static double? AngleAlongTrack(InternalJoint joint, PathDrawing mainPath)
    {
        ArgumentNullException.ThrowIfNull(joint);
        ArgumentNullException.ThrowIfNull(mainPath);
        var main = mainPath.ToPath();
        return main.TurnsOneWay ? main.TurningAt(DistanceAlongTrack(joint, mainPath)) : null;
    }

    /// <summary>
    /// The straight joints that divide the module into <paramref name="count"/> sections of the same length along the
    /// main path, at right angles to it: on a curve also the same angle, and the same length along either edge. None
    /// for one section; null when <paramref name="count"/> is less than one or a joint can't be drawn across the module.
    /// </summary>
    public static IReadOnlyList<InternalJoint>? EqualSections(SegmentPath outline, PathDrawing mainPath, int count)
    {
        ArgumentNullException.ThrowIfNull(mainPath);
        if (count < 1) return null;
        var length = mainPath.ToPath().Length;
        var joints = new List<InternalJoint>();
        for (var k = 1; k < count; k++)
        {
            if (AcrossAt(outline, mainPath, length * k / count) is not { } joint) return null;
            joints.Add(joint);
        }
        return joints;
    }

    /// <summary>Where a joint is along the track: the distance along the main path to where its first part crosses it.</summary>
    public static double DistanceAlongTrack(InternalJoint joint, PathDrawing mainPath)
    {
        ArgumentNullException.ThrowIfNull(mainPath);
        var main = mainPath.ToPath();
        var line = new Polyline(joint.Points, isClosed: false);
        var crossing = Measure.Intersections(line, main.Tessellate()).FirstOrDefault(Point.Lerp(joint.Start, joint.End, 0.5));
        return main.Project(crossing).Distance;
    }

    /// <summary>A section's length along the track: how far along the main path its outline reaches.</summary>
    public static double LengthAlongTrack(Section section, PathDrawing mainPath)
    {
        ArgumentNullException.ThrowIfNull(mainPath);
        var main = mainPath.ToPath();
        var distances = section.Outline.Tessellate().Points.Select(p => main.Project(p).Distance).ToList();
        return distances.Max() - distances.Min();
    }

    /// <summary>The closest point on <paramref name="outline"/> to <paramref name="point"/>, e.g. for a dragged joint end.</summary>
    public static Point OnEdge(SegmentPath outline, Point point) => outline.Project(point).Pose.Position;

    private static bool IsOnEdge(SegmentPath region, Point point) => region.Project(point).Pose.Position.DistanceTo(point) <= EdgeTolerance;

    /// <summary>True if the joint's bends and the middles of its parts are inside the region, so it doesn't leave it.</summary>
    private static bool IsInside(SegmentPath region, IReadOnlyList<Point> points)
    {
        var polygon = region.Tessellate();
        for (var i = 0; i < points.Count - 1; i++)
        {
            if (i > 0 && !polygon.Contains(points[i])) return false;
            if (!polygon.Contains(Point.Lerp(points[i], points[i + 1], 0.5))) return false;
        }
        return true;
    }

    /// <summary>The two parts of <paramref name="region"/> on either side of the polyline <paramref name="points"/>.</summary>
    private static (SegmentPath First, SegmentPath Second)? Split(SegmentPath region, IReadOnlyList<Point> points)
    {
        var length = region.Length;
        var from = region.Project(points[0]).Distance;
        var to = region.Project(points[^1]).Distance;
        if (Math.Abs(to - from) < EdgeTolerance) return null;
        var cut = Line(points);
        if (cut is null) return null;
        var forward = Along(region, from, to, length);
        var backward = Along(region, to, from, length);
        if (forward is null || backward is null) return null;
        // The edge from the joint's start to its end, then back along the joint; and the rest of the edge, then along it.
        return (Closed([.. forward, .. cut.Reversed().Segments]), Closed([.. backward, .. cut.Segments]));
    }

    /// <summary>The edge of a closed path from <paramref name="from"/> to <paramref name="to"/>, going past its start if needed.</summary>
    private static List<Segment>? Along(SegmentPath region, double from, double to, double length)
    {
        var parts = new List<Segment>();
        if (from < to) Add(from, to);
        else
        {
            Add(from, length);
            Add(0, to);
        }
        return parts.Count == 0 ? null : parts;

        void Add(double a, double b)
        {
            if (b - a >= Tolerance.Length) parts.AddRange(region.Between(a, b).Segments);
        }
    }

    private static SegmentPath? Line(IReadOnlyList<Point> points)
    {
        var segments = new List<Segment>();
        for (var i = 0; i < points.Count - 1; i++)
            if (points[i].DistanceTo(points[i + 1]) >= Tolerance.Length) segments.Add(new LineSegment(points[i], points[i + 1]));
        return segments.Count == 0 ? null : new SegmentPath(segments);
    }

    /// <summary>A closed path of <paramref name="segments"/>; small gaps from rounding are bridged by moving the ends together.</summary>
    private static SegmentPath Closed(List<Segment> segments)
    {
        for (var i = 0; i < segments.Count; i++)
        {
            var next = segments[(i + 1) % segments.Count];
            if (!segments[i].End.Equals(next.Start) && segments[i].End.DistanceTo(next.Start) <= EdgeTolerance && segments[i] is LineSegment line)
                segments[i] = new LineSegment(line.Start, next.Start);
        }
        return new SegmentPath(segments, isClosed: true);
    }

    /// <summary>Where a part of the module is along the track; without a track, from left to right and then bottom to top.</summary>
    private static (double Along, double Across) OrderAlongTrack(SegmentPath region, SegmentPath? main)
    {
        var centre = region.Tessellate().Centroid;
        return main is null ? (centre.X, centre.Y) : (main.Project(centre).Distance, 0);
    }

    /// <summary>Where the line from <paramref name="origin"/> along <paramref name="direction"/> crosses the edge a–b, as a multiple of the direction; null if it doesn't.</summary>
    private static double? Crossing(Point origin, Vector direction, Point a, Point b)
    {
        var edge = b - a;
        var denominator = direction.Cross(edge);
        if (Math.Abs(denominator) < Tolerance.Collinear) return null;
        var offset = a - origin;
        var t = offset.Cross(edge) / denominator;
        var u = offset.Cross(direction) / denominator;
        return u is >= 0 and <= 1 ? t : null;
    }
}
