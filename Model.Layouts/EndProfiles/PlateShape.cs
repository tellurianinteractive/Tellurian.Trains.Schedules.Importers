using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>
/// The shape of an end plate (EP-3, EP-4): its upper edge, where the track is, from left to right as a start point
/// and segments (lines, arcs and fillets), and its bottom. The side edges follow the edge's first and last points,
/// so the plate is as wide as the upper edge. The upper edge is also the profile's ground line, compared at joints.
/// </summary>
public sealed record PlateShape : ExtensibleRecord
{
    /// <summary>Arcs are approximated by points this close to the arc where points are needed, mm.</summary>
    public const double ArcTolerance = 0.5;

    private readonly IReadOnlyList<Segment?> _resolved;

    /// <summary>An upper edge from <paramref name="start"/> through <paramref name="segments"/>, and the Y of the bottom.</summary>
    /// <exception cref="ArgumentException">The edge has no drawable segment, doesn't go from left to right, or the bottom isn't below it.</exception>
    [JsonConstructor]
    public PlateShape(Point start, IReadOnlyList<PlateSegment> segments, double bottom)
    {
        ArgumentNullException.ThrowIfNull(segments);
        _resolved = Resolve(start, segments);
        var drawn = _resolved.OfType<Segment>().ToList();
        if (drawn.Count == 0) throw new ArgumentException("The upper edge needs at least one segment.", nameof(segments));
        Edge = new(drawn);
        if (Edge.End.X - Edge.Start.X < Tolerance.Length) throw new ArgumentException("The upper edge must go from left to right.", nameof(segments));
        if (Edge.Bounds.Min.Y - bottom < Tolerance.Length) throw new ArgumentException("The bottom must be below the upper edge.", nameof(bottom));
        Start = start;
        Segments = segments;
        Bottom = bottom;
        UpperEdge = [start, .. _resolved.Select((s, i) => s?.End ?? (i == 0 ? start : VertexBefore(i)))];
    }

    /// <summary>An upper edge of straight lines through <paramref name="upperEdge"/>, from left to right.</summary>
    public PlateShape(IReadOnlyList<Point> upperEdge, double bottom)
        : this(upperEdge.Count > 0 ? upperEdge[0] : throw new ArgumentException("The upper edge needs at least two points.", nameof(upperEdge)),
               upperEdge.Count >= 2 ? [.. upperEdge.Skip(1).Select(PlateSegment.Line)] : throw new ArgumentException("The upper edge needs at least two points.", nameof(upperEdge)),
               bottom)
    {
    }

    /// <summary>The left end of the upper edge, in end-view coordinates (rail top at Y = 0).</summary>
    public Point Start { get; }

    /// <summary>The upper edge's segments from left to right, as entered.</summary>
    public IReadOnlyList<PlateSegment> Segments { get; }

    /// <summary>The Y of the plate's bottom edge; negative, below rail top.</summary>
    public double Bottom { get; }

    /// <summary>The upper edge as geometry, with fillets worked out.</summary>
    [JsonIgnore]
    public SegmentPath Edge { get; }

    /// <summary>The upper edge's vertices: the start and the end of each segment, a fillet's end worked out.</summary>
    [JsonIgnore]
    public IReadOnlyList<Point> UpperEdge { get; }

    /// <summary>The X of the plate's left side.</summary>
    [JsonIgnore]
    public double Left => Edge.Start.X;
    /// <summary>The X of the plate's right side.</summary>
    [JsonIgnore]
    public double Right => Edge.End.X;
    /// <summary>The plate's width, from its left side to its right side.</summary>
    [JsonIgnore]
    public double Width => Right - Left;
    /// <summary>The height of the left side, from the bottom to the upper edge.</summary>
    [JsonIgnore]
    public double LeftHeight => Edge.Start.Y - Bottom;
    /// <summary>The height of the right side, from the bottom to the upper edge.</summary>
    [JsonIgnore]
    public double RightHeight => Edge.End.Y - Bottom;

    /// <summary>The closed outline: the bottom edge, the right side, the upper edge right to left, and the left side.</summary>
    public PathDrawing Outline()
    {
        Point leftBottom = new(Left, Bottom), rightBottom = new(Right, Bottom);
        Segment[] segments =
        [
            new LineSegment(leftBottom, rightBottom),
            new LineSegment(rightBottom, Edge.End),
            .. Edge.Reversed().Segments,
            new LineSegment(Edge.Start, leftBottom),
        ];
        return PathDrawing.From(new SegmentPath(segments, isClosed: true));
    }

    /// <summary>The ground line: the upper edge.</summary>
    public PathDrawing GroundLine() => PathDrawing.From(Edge);

    /// <summary>A valid shape, or null: at least one segment, from left to right, the bottom below the edge.</summary>
    public static PlateShape? TryCreate(Point start, IEnumerable<PlateSegment> segments, double bottom)
    {
        try
        {
            return new(start, [.. segments], bottom);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>A valid shape of straight lines through <paramref name="upperEdge"/>, or null.</summary>
    public static PlateShape? TryCreate(IEnumerable<Point> upperEdge, double bottom)
    {
        var points = upperEdge.ToList();
        return points.Count < 2 ? null : TryCreate(points[0], points.Skip(1).Select(PlateSegment.Line), bottom);
    }

    /// <summary>The shape with its upper edge starting at <paramref name="start"/>, or null if that is invalid.</summary>
    public PlateShape? WithStart(Point start) => TryCreate(start, Segments, Bottom);
    /// <summary>The shape with its bottom at <paramref name="bottom"/>, or null if that is invalid.</summary>
    public PlateShape? WithBottom(double bottom) => TryCreate(Start, Segments, bottom);
    /// <summary>The shape with segment <paramref name="index"/> replaced by <paramref name="segment"/>, or null if that is invalid.</summary>
    public PlateShape? WithSegment(int index, PlateSegment segment) => TryCreate(Start, Segments.Select((s, i) => i == index ? segment : s), Bottom);
    /// <summary>The shape with <paramref name="segment"/> inserted at <paramref name="index"/>, or null if that is invalid.</summary>
    public PlateShape? WithSegmentInserted(int index, PlateSegment segment) => TryCreate(Start, [.. Segments.Take(index), segment, .. Segments.Skip(index)], Bottom);
    /// <summary>The shape without segment <paramref name="index"/>, or null if that is invalid.</summary>
    public PlateShape? WithoutSegment(int index) => TryCreate(Start, Segments.Where((_, i) => i != index), Bottom);

    /// <summary>The whole shape moved, edge and bottom.</summary>
    public PlateShape? Moved(Vector offset) => TryCreate(Start + offset, Segments.Select(s => s.Moved(offset)), Bottom + offset.Y);

    /// <summary>
    /// The Y of the upper edge at <paramref name="x"/>, the highest where the edge is vertical there; null outside the plate.
    /// </summary>
    public double? EdgeHeightAt(double x)
    {
        if (x < Left - Tolerance.Length || x > Right + Tolerance.Length) return null;
        var points = PathDrawing.From(Edge).Tessellate().Points;
        double? highest = null;
        for (var i = 1; i < points.Count; i++)
        {
            Point a = points[i - 1], b = points[i];
            if (x < Math.Min(a.X, b.X) - Tolerance.Length || x > Math.Max(a.X, b.X) + Tolerance.Length) continue;
            var y = Math.Abs(b.X - a.X) < Tolerance.Length ? Math.Max(a.Y, b.Y) : a.Y + (b.Y - a.Y) * (x - a.X) / (b.X - a.X);
            highest = highest is { } h ? Math.Max(h, y) : y;
        }
        return highest;
    }

    /// <summary>
    /// Whether the upper edge is level at <paramref name="y"/> all the way from <paramref name="left"/> to
    /// <paramref name="right"/>, inside the plate, e.g. under a track's underside.
    /// </summary>
    public bool IsLevel(double left, double right, double y, double tolerance)
    {
        if (left < Left - tolerance || right > Right + tolerance || right - left <= 2 * tolerance) return false;
        bool At(double value) => Math.Abs(value - y) <= tolerance;
        // Just inside the ends, so a step up or down right at an end isn't counted.
        if (EdgeHeightAt(left + tolerance) is not { } atLeft || !At(atLeft) || EdgeHeightAt(right - tolerance) is not { } atRight || !At(atRight)) return false;
        return PathDrawing.From(Edge).Tessellate().Points.Where(p => p.X > left + tolerance && p.X < right - tolerance).All(p => At(p.Y));
    }

    /// <summary>
    /// The shape with the track bed at <paramref name="x"/> moved from Y <paramref name="fromY"/> to <paramref name="toY"/>,
    /// e.g. under a track whose height changed: the whole level stretch of the upper edge the track rests on, its
    /// vertices at <paramref name="fromY"/> on both sides of <paramref name="x"/>, moves; the segments next to it follow
    /// its ends, and the rest of the edge stays. Null when the edge isn't level at <paramref name="fromY"/> there, when
    /// the stretch has an arc or fillet, or when the shape would be invalid.
    /// </summary>
    public PlateShape? WithTrackBed(double x, double fromY, double toY, double tolerance)
    {
        var points = UpperEdge;
        bool AtBed(Point p) => Math.Abs(p.Y - fromY) <= tolerance;
        // The level segment under x: from the last vertex at or left of x to the next one right of it, both at the bed.
        var right = -1;
        for (var i = 1; i < points.Count; i++)
            if (points[i - 1].X <= x + tolerance && points[i].X >= x - tolerance && AtBed(points[i - 1]) && AtBed(points[i])) { right = i; break; }
        if (right < 0) return null;
        var first = right - 1;
        var last = right;
        while (first > 0 && AtBed(points[first - 1])) first--;
        while (last < points.Count - 1 && AtBed(points[last + 1])) last++;
        // The segments that end at the stretch's vertices must be lines, so their ends can move.
        for (var i = Math.Max(first, 1); i <= last; i++)
            if (Segments[i - 1].Kind != PlateSegmentKind.Line) return null;
        if (last < Segments.Count && Segments[last].Kind != PlateSegmentKind.Line) return null;
        var shape = this;
        for (var i = first; i <= last; i++)
            if (shape?.WithVertex(i, new(points[i].X, toY)) is { } moved) shape = moved;
            else return null;
        return shape;
    }

    /// <summary>
    /// Vertex <paramref name="index"/> (0 the start, then each segment's end) moved to <paramref name="point"/>;
    /// a fillet's end is worked out, so it can't be moved. Null if the shape would be invalid.
    /// </summary>
    public PlateShape? WithVertex(int index, Point point) =>
        index == 0 ? WithStart(point)
        : index <= Segments.Count && Segments[index - 1].Kind != PlateSegmentKind.Fillet ? WithSegment(index - 1, Segments[index - 1].WithEnd(point))
        : null;

    /// <summary>
    /// The plate shape of a profile: the stored one, as entered, or else read from its outline: the lowest edge is the bottom, and the outline between its two
    /// corners, without the sides, is the upper edge. Lines and arcs are kept as they are drawn. Null if the
    /// profile has no outline.
    /// </summary>
    public static PlateShape? From(EndProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (profile.Plate is { } stored) return stored;
        if (profile.Outline is not { } outline) return null;
        var segments = outline.ToPath().Segments.Where(s => s.Length > Tolerance.Length).ToList();
        if (segments.Count < 3) return null;
        var bottom = segments.Min(s => s.Start.Y);
        var corners = segments.Select((s, i) => (s.Start, i)).Where(x => Math.Abs(x.Start.Y - bottom) <= Tolerance.Length).ToList();
        var leftCorner = corners.MinBy(x => x.Start.X).i;
        var rightCorner = corners.MaxBy(x => x.Start.X).i;
        if (leftCorner == rightCorner) return null;

        // The outline runs from one bottom corner to the other two ways; the upper edge is the way that isn't the bottom.
        var one = Chain(segments, rightCorner, leftCorner);
        var other = Chain(segments, leftCorner, rightCorner);
        var isBottom = (List<Segment> chain) => chain.All(s => s is LineSegment && Math.Abs(s.Start.Y - bottom) <= Tolerance.Length && Math.Abs(s.End.Y - bottom) <= Tolerance.Length);
        var upper = isBottom(one) ? other : one;
        var leftToRight = upper[0].Start.X <= upper[^1].End.X ? upper : [.. upper.AsEnumerable().Reverse().Select(s => s.Reversed())];

        // Without the side edges, which only join the corners to the upper edge.
        var edge = leftToRight.ToList();
        if (edge.Count > 1 && edge[0] is LineSegment && Math.Abs(edge[0].Start.Y - bottom) <= Tolerance.Length) edge.RemoveAt(0);
        if (edge.Count > 1 && edge[^1] is LineSegment && Math.Abs(edge[^1].End.Y - bottom) <= Tolerance.Length) edge.RemoveAt(edge.Count - 1);
        var plateSegments = edge.Select(s => s is ArcSegment arc ? PlateSegment.Arc(arc.Center, arc.End) : PlateSegment.Line(s.End));
        return TryCreate(edge[0].Start, plateSegments, bottom);
    }

    /// <summary>The segments from the one starting at <paramref name="from"/> up to the one ending where <paramref name="to"/> starts.</summary>
    private static List<Segment> Chain(IReadOnlyList<Segment> segments, int from, int to)
    {
        var chain = new List<Segment>();
        for (var i = from; i != to; i = (i + 1) % segments.Count) chain.Add(segments[i]);
        return chain;
    }

    private Point VertexBefore(int index)
    {
        for (var i = index - 1; i >= 0; i--)
            if (_resolved[i] is { } segment) return segment.End;
        return Start;
    }

    /// <summary>
    /// The geometry of each segment, null where a segment can't be drawn (e.g. a fillet without a line before it
    /// or a segment after it). Arcs run the shorter way round from where the previous segment ends; a fillet
    /// continues the previous segment's direction and ends where it meets the next segment tangentially.
    /// </summary>
    private static IReadOnlyList<Segment?> Resolve(Point start, IReadOnlyList<PlateSegment> segments)
    {
        var result = new Segment?[segments.Count];
        var current = start;
        Angle? heading = null;
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            Segment? geometry = segment.Kind switch
            {
                PlateSegmentKind.Line when segment.End is { } end => current.IsCloseTo(end) ? null : new LineSegment(current, end),
                PlateSegmentKind.Arc when segment.Center is { } center && segment.End is { } arcEnd => ArcFrom(current, center, arcEnd),
                PlateSegmentKind.Fillet when heading is { } h && i + 1 < segments.Count && segment.Radius is > 0 and var r => Fillet(current, h, r, segment.Side, segments[i + 1]),
                _ => null,
            };
            result[i] = geometry;
            if (geometry is null) continue;
            current = geometry.End;
            heading = geometry.EndHeading;
        }
        return result;
    }

    /// <summary>
    /// An arc from <paramref name="start"/> that ends exactly at <paramref name="end"/>, with the radius from
    /// <paramref name="center"/> to the end. A drawing's rounded values may put the start slightly off that circle;
    /// then the drawn arc's centre moves the fraction of a millimetre needed, keeping the end, as a plate's corner
    /// matters most. The shorter way round is used.
    /// </summary>
    private static ArcSegment? ArcFrom(Point start, Point center, Point end)
    {
        if (start.IsCloseTo(end)) return null;
        var radius = end.DistanceTo(center);
        if (radius < Tolerance.Length) return null;
        var drawnCenter = Math.Abs(start.DistanceTo(center) - radius) <= Tolerance.Length ? center : CentreThrough(start, end, radius, center);
        var sweep = ((end - drawnCenter).Direction - (start - drawnCenter).Direction).Signed.Degrees;
        return Math.Abs(sweep) < Tolerance.Angle ? null : new ArcSegment(drawnCenter, start.DistanceTo(drawnCenter), (start - drawnCenter).Direction, sweep);
    }

    /// <summary>The centre of a circle of <paramref name="radius"/> through both points, on the side of <paramref name="near"/>.</summary>
    private static Point CentreThrough(Point a, Point b, double radius, Point near)
    {
        var middle = Point.Lerp(a, b, 0.5);
        var half = a.DistanceTo(b) / 2;
        var along = Math.Sqrt(Math.Max(radius * radius - half * half, 0));
        var normal = (b - a).Normalized.LeftNormal;
        Point one = middle + normal * along, other = middle - normal * along;
        return one.DistanceTo(near) <= other.DistanceTo(near) ? one : other;
    }

    /// <summary>
    /// A fillet of <paramref name="radius"/> from <paramref name="start"/>, heading <paramref name="heading"/>, with its
    /// centre on <paramref name="side"/> of the edge, or else turning whichever way lets it meet <paramref name="next"/>
    /// tangentially: an arc's circle (outside or inside it), or a line to the next end point. Drawing values are
    /// rounded, so the closest fit is used.
    /// </summary>
    private static ArcSegment? Fillet(Point start, Angle heading, double radius, FilletSide? side, PlateSegment next)
    {
        ArcSegment? best = null;
        var bestError = double.MaxValue;
        // The edge runs from left to right, so a centre to the left of the heading is above it.
        double[] turns = side switch { FilletSide.Above => [1.0], FilletSide.Below => [-1.0], _ => [1.0, -1.0] };
        foreach (var turn in turns)
        {
            var center = start + Vector.FromAngle(heading).LeftNormal * (radius * turn);
            foreach (var (tangent, error) in TangentPoints(center, radius, turn, next))
            {
                var sweep = turn * Positive(turn * ((tangent - center).Direction - (start - center).Direction).Degrees);
                if (Math.Abs(sweep) < Tolerance.Angle) continue;
                // A rounded drawing makes the fit slightly off; the smallest error, then the shortest arc, wins.
                if (error < bestError - Tolerance.Length || (Math.Abs(error - bestError) <= Tolerance.Length && Math.Abs(sweep) < Math.Abs(best!.SweepDegrees)))
                {
                    best = new ArcSegment(center, radius, (start - center).Direction, sweep);
                    bestError = error;
                }
            }
        }
        return best;
    }

    /// <summary>Where a fillet circle meets the next segment tangentially, with how far off the fit is.</summary>
    private static IEnumerable<(Point Tangent, double Error)> TangentPoints(Point center, double radius, double turn, PlateSegment next)
    {
        switch (next.Kind)
        {
            case PlateSegmentKind.Arc when next.Center is { } other:
                var otherRadius = next.EffectiveRadius;
                var between = other - center;
                var distance = between.Length;
                if (distance < Tolerance.Length) yield break;
                yield return (center + between / distance * radius, Math.Abs(distance - (radius + otherRadius)));
                yield return (center - between / distance * radius, Math.Abs(distance - Math.Abs(otherRadius - radius)));
                break;
            case PlateSegmentKind.Line when next.End is { } lineEnd:
                var toEnd = lineEnd - center;
                var d = toEnd.Length;
                if (d <= radius) yield break;
                var spread = Math.Acos(radius / d) * 180 / Math.PI;
                foreach (var side in new[] { 1.0, -1.0 })
                {
                    var tangent = center + Vector.FromAngle(toEnd.Direction + new Angle(side * spread), radius);
                    // Travelling round the circle in the turn direction, the line must leave the tangent point forwards.
                    var travel = Vector.FromAngle((tangent - center).Direction + new Angle(90 * turn));
                    if (travel.Dot((lineEnd - tangent).Normalized) > 1 - 1e-9) yield return (tangent, 0);
                }
                break;
        }
    }

    /// <summary>An angle in degrees as 0 to 360.</summary>
    private static double Positive(double degrees) => ((degrees % 360) + 360) % 360;
}
