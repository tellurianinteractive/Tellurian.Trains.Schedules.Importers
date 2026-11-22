namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Compares two shapes, e.g. the ground lines of two facing end profiles (spec §3.2 rule 3)
/// or an end profile against its own mirror image (symmetry).
/// </summary>
public static class ShapeComparison
{
    /// <summary>
    /// The Hausdorff-style maximum deviation: the largest distance from any point on one polyline
    /// to the nearest point on the other, in both directions. Edges are sampled at most
    /// <paramref name="sampleSpacing"/> apart, so the result is accurate to about half that spacing.
    /// </summary>
    public static ShapeDeviation MaximumDeviation(Polyline first, Polyline second, double sampleSpacing = 1.0)
    {
        if (sampleSpacing <= 0) throw new ArgumentOutOfRangeException(nameof(sampleSpacing), sampleSpacing, "The sample spacing must be positive.");
        var fromFirst = OneWay(first, second, sampleSpacing);
        var fromSecond = OneWay(second, first, sampleSpacing);
        return fromFirst.Maximum >= fromSecond.Maximum ? fromFirst : fromSecond;
    }

    /// <summary>
    /// True if <see cref="MaximumDeviation"/> would be within <paramref name="tolerance"/>, found much faster: each
    /// sample first tries the edge nearest the previous sample, which for matching shapes is almost always within
    /// the tolerance, and the check stops at the first sample that isn't.
    /// </summary>
    public static bool IsWithin(Polyline first, Polyline second, double tolerance, double sampleSpacing = 1.0)
    {
        if (sampleSpacing <= 0) throw new ArgumentOutOfRangeException(nameof(sampleSpacing), sampleSpacing, "The sample spacing must be positive.");
        return OneWayWithin(first, second, tolerance, sampleSpacing) && OneWayWithin(second, first, tolerance, sampleSpacing);
    }

    private static bool OneWayWithin(Polyline from, Polyline to, double tolerance, double sampleSpacing)
    {
        var edges = to.Edges().ToList();
        if (edges.Count == 0) return false;
        var nearest = 0;
        foreach (var point in Samples(from, sampleSpacing))
        {
            if (Measure.PointToSegment(point, edges[nearest].Start, edges[nearest].End) <= tolerance) continue;
            nearest = edges.FindIndex(e => Measure.PointToSegment(point, e.Start, e.End) <= tolerance);
            if (nearest < 0) return false;
        }
        return true;
    }

    private static ShapeDeviation OneWay(Polyline from, Polyline to, double sampleSpacing)
    {
        var worst = new ShapeDeviation(0, from.Points[0]);
        foreach (var point in Samples(from, sampleSpacing))
        {
            var distance = Measure.PointToPolyline(point, to);
            if (distance > worst.Maximum) worst = new(distance, point);
        }
        return worst;
    }

    private static IEnumerable<Point> Samples(Polyline polyline, double sampleSpacing)
    {
        yield return polyline.Points[0];
        foreach (var (a, b) in polyline.Edges())
        {
            var steps = Math.Max(1, (int)Math.Ceiling(a.DistanceTo(b) / sampleSpacing));
            for (var i = 1; i <= steps; i++) yield return Point.Lerp(a, b, (double)i / steps);
        }
    }
}
