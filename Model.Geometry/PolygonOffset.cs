namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Grows or shrinks a polygon, for visualising clearances around module outlines and obstacles.
/// The result is meant for display: it can self-intersect when shrinking by more than the local
/// width of the polygon. Distance checks use <see cref="Measure"/> on the original outlines.
/// </summary>
public static class PolygonOffset
{
    private const double MaxMiterFactor = 4;

    /// <summary>
    /// The polygon moved <paramref name="distance"/> outwards (negative: inwards). Corners on the
    /// growing side are rounded with arcs within <paramref name="chordTolerance"/>; the others are mitred.
    /// The result is counter-clockwise.
    /// </summary>
    public static Polyline Offset(Polyline polygon, double distance, double chordTolerance = Tolerance.Chord)
    {
        if (!polygon.IsClosed) throw new ArgumentException("Only closed polylines can be offset as polygons.", nameof(polygon));
        var points = Cleaned(polygon.IsCounterClockwise ? polygon.Points : polygon.Points.Reverse());
        if (points.Count < 3) throw new ArgumentException("The polygon has fewer than three distinct corners.", nameof(polygon));
        if (Math.Abs(distance) < Tolerance.Length) return new(points, true);

        var result = new List<Point>();
        var n = points.Count;
        for (var i = 0; i < n; i++)
        {
            var previous = points[(i + n - 1) % n];
            var corner = points[i];
            var next = points[(i + 1) % n];
            var incoming = (corner - previous).Normalized;
            var outgoing = (next - corner).Normalized;
            var normalIn = Outward(incoming);
            var normalOut = Outward(outgoing);
            var turn = incoming.Cross(outgoing);

            if (Math.Abs(turn) < Tolerance.Collinear && incoming.Dot(outgoing) > 0)
            {
                result.Add(corner + normalIn * distance);
            }
            else if (turn * distance > 0)
            {
                AppendArc(result, corner, normalIn, normalOut, distance, chordTolerance);
            }
            else
            {
                AppendMiter(result, corner, normalIn, normalOut, distance);
            }
        }
        return new(result, true);
    }

    /// <summary>The outward normal of a counter-clockwise polygon edge: to the right of the direction of travel.</summary>
    private static Vector Outward(Vector direction) => new(direction.Y, -direction.X);

    private static void AppendArc(List<Point> result, Point corner, Vector from, Vector to, double distance, double chordTolerance)
    {
        var radius = Math.Abs(distance);
        var sign = Math.Sign(distance);
        var startAngle = (from * sign).Direction;
        var sweep = ((to * sign).Direction - startAngle).Signed.Degrees;
        var tolerance = Math.Clamp(chordTolerance, Tolerance.Length, radius);
        var maxStepDegrees = 2 * Math.Acos(1 - tolerance / radius) * 180 / Math.PI;
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / maxStepDegrees));
        for (var s = 0; s <= steps; s++)
        {
            result.Add(corner + Vector.FromAngle(startAngle + new Angle(sweep * s / steps), radius));
        }
    }

    private static void AppendMiter(List<Point> result, Point corner, Vector normalIn, Vector normalOut, double distance)
    {
        var bisector = (normalIn + normalOut).Normalized;
        var cosHalf = bisector.Dot(normalIn);
        if (cosHalf > 1 / MaxMiterFactor)
        {
            result.Add(corner + bisector * (distance / cosHalf));
        }
        else
        {
            result.Add(corner + normalIn * distance);
            result.Add(corner + normalOut * distance);
        }
    }

    private static List<Point> Cleaned(IEnumerable<Point> points)
    {
        var result = new List<Point>();
        foreach (var point in points)
        {
            if (result.Count == 0 || !result[^1].IsCloseTo(point)) result.Add(point);
        }
        while (result.Count > 1 && result[^1].IsCloseTo(result[0])) result.RemoveAt(result.Count - 1);
        return result;
    }
}
