using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

/// <summary>
/// Element-by-element comparison of two end profile drawings in the same coordinates.
/// Shared by the joint fit evaluator and the symmetry analysis.
/// </summary>
public static class EndProfileComparison
{
    /// <summary>Compares the tracks in order of offset: positions, gauges and rail systems, within <paramref name="tolerance"/>, mm. Only <see cref="FitDifferences.TrackCount"/> when the counts differ.</summary>
    public static FitDifferences CompareTracks(IReadOnlyList<EndProfileTrack> first, IReadOnlyList<EndProfileTrack> second, double tolerance)
    {
        if (first.Count != second.Count) return FitDifferences.TrackCount;
        var a = first.OrderBy(t => t.Offset).ToList();
        var b = second.OrderBy(t => t.Offset).ToList();
        var result = FitDifferences.None;
        for (var i = 0; i < a.Count; i++)
        {
            if (Math.Abs(a[i].Offset - b[i].Offset) > tolerance) result |= FitDifferences.TrackPositions;
            if (!a[i].SharesAGaugeWith(b[i], tolerance)) result |= FitDifferences.Gauge;
            if (a[i].RailSystem != b[i].RailSystem) result |= FitDifferences.RailSystem;
        }
        return result;
    }

    /// <summary>Pairs circles, such as screw holes or dowels, whose centres are within <paramref name="tolerance"/> of each other, mm.</summary>
    public static PointMatchResult MatchCircles(IReadOnlyList<Circle> first, IReadOnlyList<Circle> second, double tolerance) =>
        PointMatching.Match([.. first.Select(c => c.Center)], [.. second.Select(c => c.Center)], tolerance);

    /// <summary>
    /// The maximum deviation between two ground lines over the lateral range they have in common,
    /// so that plates of different widths are compared where they meet. Null if they don't overlap.
    /// </summary>
    public static ShapeDeviation? CompareGroundLines(PathDrawing first, PathDrawing second, double sampleSpacing)
    {
        var a = first.Tessellate();
        var b = second.Tessellate();
        var minX = Math.Max(a.Bounds.Min.X, b.Bounds.Min.X);
        var maxX = Math.Min(a.Bounds.Max.X, b.Bounds.Max.X);
        if (maxX - minX < Tolerance.Length) return null;
        var clippedA = ClipX(a, minX, maxX);
        var clippedB = ClipX(b, minX, maxX);
        if (clippedA is null || clippedB is null) return null;
        return ShapeComparison.MaximumDeviation(clippedA, clippedB, sampleSpacing);
    }

    /// <summary>The parts of an open polyline with X between <paramref name="minX"/> and <paramref name="maxX"/>.</summary>
    public static Polyline? ClipX(Polyline polyline, double minX, double maxX)
    {
        var points = new List<Point>();
        foreach (var (a, b) in polyline.Edges())
        {
            var (t0, t1) = (0.0, 1.0);
            var dx = b.X - a.X;
            if (Math.Abs(dx) < Tolerance.Length)
            {
                if (a.X < minX - Tolerance.Length || a.X > maxX + Tolerance.Length) continue;
            }
            else
            {
                var tMin = (minX - a.X) / dx;
                var tMax = (maxX - a.X) / dx;
                if (tMin > tMax) (tMin, tMax) = (tMax, tMin);
                t0 = Math.Max(t0, tMin);
                t1 = Math.Min(t1, tMax);
                if (t0 > t1) continue;
            }
            Add(points, Point.Lerp(a, b, t0));
            Add(points, Point.Lerp(a, b, t1));
        }
        return points.Count >= 2 ? new Polyline(points, false) : null;

        static void Add(List<Point> points, Point point)
        {
            if (points.Count == 0 || !points[^1].IsCloseTo(point)) points.Add(point);
        }
    }
}
