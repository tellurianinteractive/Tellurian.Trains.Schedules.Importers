namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Areas of combined polygons, e.g. the floor of a hall that is free of obstacles. Overlapping polygons are counted
/// once. The plane is cut into vertical slabs at every vertex and every crossing of edges; inside a slab no edges
/// cross, so the length covered on a vertical line changes linearly and its value at the middle of the slab gives the
/// slab's area exactly. Polygons are closed polylines; arcs are tessellated first.
/// </summary>
public static class AreaMeasure
{
    /// <summary>The area covered by at least one of <paramref name="polygons"/>.</summary>
    public static double Union(IReadOnlyList<Polyline> polygons)
    {
        ArgumentNullException.ThrowIfNull(polygons);
        var closed = polygons.Where(p => p.IsClosed).ToList();
        if (closed.Count == 0) return 0;
        var range = closed.Aggregate(BoundingBox.Empty, (box, p) => box.Union(p.Bounds));
        return Integrate(closed, range.Min.X, range.Max.X, x => Length(Merged(closed, x)));
    }

    /// <summary>The area of <paramref name="region"/> that none of <paramref name="removed"/> covers.</summary>
    public static double Remaining(Polyline region, IReadOnlyList<Polyline> removed)
    {
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(removed);
        if (!region.IsClosed) return 0;
        // Only what overlaps the region's bounds can take anything away.
        var holes = removed.Where(p => p.IsClosed && Measure.BoxDistance(p.Bounds, region.Bounds) <= 0).ToList();
        if (holes.Count == 0) return Math.Abs(region.SignedArea);
        return Integrate([region, .. holes], region.Bounds.Min.X, region.Bounds.Max.X, x =>
        {
            var inside = Intervals(region, x);
            return Length(inside) - Overlap(inside, Merged(holes, x));
        });
    }

    /// <summary>Sums <paramref name="lengthAt"/> × width over the slabs between <paramref name="from"/> and <paramref name="to"/>.</summary>
    private static double Integrate(IReadOnlyList<Polyline> polygons, double from, double to, Func<double, double> lengthAt)
    {
        var cuts = Cuts(polygons).Where(x => x >= from && x <= to).Append(from).Append(to).Order().ToList();
        var area = 0.0;
        for (var i = 1; i < cuts.Count; i++)
        {
            var width = cuts[i] - cuts[i - 1];
            if (width <= Tolerance.Length) continue;
            area += width * lengthAt((cuts[i] + cuts[i - 1]) / 2);
        }
        return area;
    }

    /// <summary>The X of every vertex and of every crossing of edges of different polygons whose bounds meet.</summary>
    private static IEnumerable<double> Cuts(IReadOnlyList<Polyline> polygons)
    {
        foreach (var polygon in polygons)
            foreach (var point in polygon.Points) yield return point.X;
        for (var i = 0; i < polygons.Count; i++)
            for (var j = i + 1; j < polygons.Count; j++)
            {
                if (Measure.BoxDistance(polygons[i].Bounds, polygons[j].Bounds) > 0) continue;
                foreach (var crossing in Measure.Intersections(polygons[i], polygons[j])) yield return crossing.X;
            }
    }

    /// <summary>Where a vertical line at <paramref name="x"/> is inside <paramref name="polygon"/>, as sorted intervals of Y.</summary>
    private static List<(double From, double To)> Intervals(Polyline polygon, double x)
    {
        var intervals = new List<(double, double)>();
        if (x <= polygon.Bounds.Min.X || x >= polygon.Bounds.Max.X) return intervals;
        var crossings = new List<double>();
        foreach (var (a, b) in polygon.Edges())
            if ((a.X < x) != (b.X < x)) crossings.Add(a.Y + (x - a.X) * (b.Y - a.Y) / (b.X - a.X));
        crossings.Sort();
        // Even-odd: the line is inside between the first and second crossing, the third and fourth, and so on.
        for (var i = 0; i + 1 < crossings.Count; i += 2) intervals.Add((crossings[i], crossings[i + 1]));
        return intervals;
    }

    /// <summary>The union of the intervals of all <paramref name="polygons"/> at <paramref name="x"/>, sorted and without overlaps.</summary>
    private static List<(double From, double To)> Merged(IReadOnlyList<Polyline> polygons, double x)
    {
        var all = polygons.SelectMany(p => Intervals(p, x)).OrderBy(i => i.From).ToList();
        var merged = new List<(double From, double To)>();
        foreach (var interval in all)
        {
            if (merged.Count > 0 && interval.From <= merged[^1].To) merged[^1] = (merged[^1].From, Math.Max(merged[^1].To, interval.To));
            else merged.Add(interval);
        }
        return merged;
    }

    private static double Length(List<(double From, double To)> intervals) => intervals.Sum(i => i.To - i.From);

    /// <summary>The length common to two sorted lists of intervals without overlaps.</summary>
    private static double Overlap(List<(double From, double To)> first, List<(double From, double To)> second)
    {
        var overlap = 0.0;
        int i = 0, j = 0;
        while (i < first.Count && j < second.Count)
        {
            overlap += Math.Max(0, Math.Min(first[i].To, second[j].To) - Math.Max(first[i].From, second[j].From));
            if (first[i].To < second[j].To) i++;
            else j++;
        }
        return overlap;
    }
}
