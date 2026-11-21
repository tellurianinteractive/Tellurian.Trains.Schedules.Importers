namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Distances and intersections between points, straight edges and polylines.
/// </summary>
public static class Measure
{
    /// <summary>The shortest distance from <paramref name="point"/> to the edge from <paramref name="a"/> to <paramref name="b"/>.</summary>
    public static double PointToSegment(Point point, Point a, Point b) =>
        point.DistanceTo(ClosestPointOnSegment(point, a, b));

    /// <summary>The point on the edge from <paramref name="a"/> to <paramref name="b"/> closest to <paramref name="point"/>.</summary>
    public static Point ClosestPointOnSegment(Point point, Point a, Point b)
    {
        var edge = b - a;
        var lengthSquared = edge.LengthSquared;
        if (lengthSquared < Tolerance.Length * Tolerance.Length) return a;
        var t = Math.Clamp((point - a).Dot(edge) / lengthSquared, 0, 1);
        return a + edge * t;
    }

    /// <summary>
    /// The intersection point of the edges a1–a2 and b1–b2, or null if they don't meet.
    /// Collinear overlapping edges return one point of the overlap.
    /// </summary>
    public static Point? SegmentIntersection(Point a1, Point a2, Point b1, Point b2)
    {
        var r = a2 - a1;
        var s = b2 - b1;
        var denominator = r.Cross(s);
        var offset = b1 - a1;
        if (Math.Abs(denominator) < Tolerance.Length * Tolerance.Length)
        {
            if (Math.Abs(offset.Cross(r)) > Tolerance.Length * Math.Max(1, r.Length)) return null;
            // Collinear: return an end point lying on the other edge, if any.
            foreach (var candidate in new[] { b1, b2, a1, a2 })
            {
                if (PointToSegment(candidate, a1, a2) <= Tolerance.Length && PointToSegment(candidate, b1, b2) <= Tolerance.Length)
                    return candidate;
            }
            return null;
        }
        var t = offset.Cross(s) / denominator;
        var u = offset.Cross(r) / denominator;
        var epsilonT = Tolerance.Length / Math.Max(r.Length, Tolerance.Length);
        var epsilonU = Tolerance.Length / Math.Max(s.Length, Tolerance.Length);
        if (t < -epsilonT || t > 1 + epsilonT || u < -epsilonU || u > 1 + epsilonU) return null;
        return a1 + r * Math.Clamp(t, 0, 1);
    }

    /// <summary>The shortest distance between the edges a1–a2 and b1–b2; zero if they intersect.</summary>
    public static double SegmentToSegment(Point a1, Point a2, Point b1, Point b2)
    {
        if (SegmentIntersection(a1, a2, b1, b2) is not null) return 0;
        return Math.Min(
            Math.Min(PointToSegment(a1, b1, b2), PointToSegment(a2, b1, b2)),
            Math.Min(PointToSegment(b1, a1, a2), PointToSegment(b2, a1, a2)));
    }

    /// <summary>The shortest distance from <paramref name="point"/> to the edges of <paramref name="polyline"/>.</summary>
    public static double PointToPolyline(Point point, Polyline polyline)
    {
        var minimum = double.PositiveInfinity;
        foreach (var (a, b) in polyline.Edges())
        {
            minimum = Math.Min(minimum, PointToSegment(point, a, b));
        }
        return minimum;
    }

    /// <summary>
    /// The shortest distance between the edges of two polylines; zero if they cross or touch.
    /// It doesn't detect that one closed polyline lies entirely inside another; use
    /// <see cref="Polyline.Contains"/> for that.
    /// </summary>
    public static double PolylineToPolyline(Polyline first, Polyline second, double stopBelow = 0)
    {
        var minimum = double.PositiveInfinity;
        foreach (var (a1, a2) in first.Edges())
        {
            var edgeBox = BoundingBox.Empty.Including(a1).Including(a2);
            if (BoxDistance(edgeBox, second.Bounds) >= minimum) continue;
            foreach (var (b1, b2) in second.Edges())
            {
                var distance = SegmentToSegment(a1, a2, b1, b2);
                if (distance < minimum)
                {
                    minimum = distance;
                    if (minimum <= stopBelow) return minimum;
                }
            }
        }
        return minimum;
    }

    /// <summary>True if any edge of <paramref name="first"/> crosses or touches any edge of <paramref name="second"/>.</summary>
    public static bool Intersects(Polyline first, Polyline second) =>
        first.Bounds.Intersects(second.Bounds) && PolylineToPolyline(first, second) <= Tolerance.Length;

    /// <summary>All points where edges of <paramref name="first"/> cross edges of <paramref name="second"/>.</summary>
    public static IReadOnlyList<Point> Intersections(Polyline first, Polyline second)
    {
        var result = new List<Point>();
        if (!first.Bounds.Inflated(Tolerance.Length).Intersects(second.Bounds)) return result;
        foreach (var (a1, a2) in first.Edges())
        {
            foreach (var (b1, b2) in second.Edges())
            {
                if (SegmentIntersection(a1, a2, b1, b2) is { } point && !result.Any(p => p.IsCloseTo(point)))
                    result.Add(point);
            }
        }
        return result;
    }

    /// <summary>The shortest distance between two bounding boxes; zero if they overlap.</summary>
    public static double BoxDistance(BoundingBox first, BoundingBox second)
    {
        if (first.IsEmpty || second.IsEmpty) return double.PositiveInfinity;
        var dx = Math.Max(0, Math.Max(first.Min.X - second.Max.X, second.Min.X - first.Max.X));
        var dy = Math.Max(0, Math.Max(first.Min.Y - second.Max.Y, second.Min.Y - first.Max.Y));
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
