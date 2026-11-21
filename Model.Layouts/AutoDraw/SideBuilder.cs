using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>
/// Builds paths beside a main path from an <see cref="OffsetProfile"/>. Parts with a constant offset
/// are exact parallels (concentric arcs on curves); tapering parts are polylines.
/// </summary>
internal static class SideBuilder
{
    /// <summary>The largest arc angle between two polyline points of a tapering part, degrees.</summary>
    private const double MaxTaperStepDegrees = 2;

    /// <summary>The path beside <paramref name="main"/> from distance <paramref name="from"/> to <paramref name="to"/>.</summary>
    public static SegmentPath Build(SegmentPath main, OffsetProfile profile, double from, double to)
    {
        var breaks = new List<double> { from };
        breaks.AddRange(profile.KnotDistances.Where(s => s > from + Tolerance.Length && s < to - Tolerance.Length));
        breaks.Add(to);

        var segments = new List<Segment>();
        for (var i = 1; i < breaks.Count; i++)
        {
            var (u, v) = (breaks[i - 1], breaks[i]);
            if (v - u < Tolerance.Length) continue;
            var (ou, ov) = (profile.At(u), profile.At(v));
            if (Math.Abs(ou - ov) < Tolerance.Length)
            {
                segments.AddRange(main.Between(u, v).Offset(ou).Segments);
            }
            else
            {
                var points = Samples(main, u, v).Select(s => Beside(main, s, profile.At(s))).ToList();
                for (var p = 1; p < points.Count; p++)
                {
                    if (!points[p - 1].IsCloseTo(points[p])) segments.Add(new LineSegment(points[p - 1], points[p]));
                }
            }
        }
        return new(segments);
    }

    /// <summary>The point at <paramref name="offset"/> to the left of <paramref name="main"/> at distance <paramref name="s"/>.</summary>
    public static Point Beside(SegmentPath main, double s, double offset)
    {
        var pose = main.PoseAt(s);
        return pose.Position + pose.Left * offset;
    }

    private static List<double> Samples(SegmentPath main, double from, double to)
    {
        var samples = new List<double> { from };
        var start = 0.0;
        foreach (var segment in main.Segments)
        {
            var end = start + segment.Length;
            var a = Math.Max(from, start);
            var b = Math.Min(to, end);
            if (b - a >= Tolerance.Length)
            {
                var steps = segment is ArcSegment arc
                    ? Math.Max(1, (int)Math.Ceiling(Math.Abs(arc.SweepDegrees) * (b - a) / arc.Length / MaxTaperStepDegrees))
                    : 1;
                for (var k = 1; k <= steps; k++) samples.Add(a + (b - a) * k / steps);
            }
            start = end;
        }
        return samples;
    }
}
