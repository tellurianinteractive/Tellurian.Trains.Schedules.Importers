namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// A sequence of points joined by straight edges. A closed polyline (polygon) has an implicit
/// edge from the last point back to the first.
/// </summary>
public sealed class Polyline
{
    /// <summary>A polyline through <paramref name="points"/>, which must be at least two.</summary>
    /// <exception cref="ArgumentException">There are fewer than two points.</exception>
    public Polyline(IEnumerable<Point> points, bool isClosed)
    {
        Points = [.. points];
        if (Points.Count < 2) throw new ArgumentException("A polyline needs at least two points.", nameof(points));
        IsClosed = isClosed;
        Bounds = BoundingBox.FromPoints(Points);
    }

    /// <summary>The points in order. A closed polyline doesn't repeat the first point at the end.</summary>
    public IReadOnlyList<Point> Points { get; }
    /// <summary>True if there is an edge from the last point back to the first.</summary>
    public bool IsClosed { get; }
    /// <summary>The bounding box of the points.</summary>
    public BoundingBox Bounds { get; }
    /// <summary>The number of edges: the number of points if closed, otherwise one less.</summary>
    public int EdgeCount => IsClosed ? Points.Count : Points.Count - 1;
    /// <summary>The total length of the edges in millimetres, including the closing edge.</summary>
    public double Length => Edges().Sum(e => e.Start.DistanceTo(e.End));

    /// <summary>The signed area of a closed polyline: positive when counter-clockwise.</summary>
    public double SignedArea
    {
        get
        {
            if (!IsClosed) return 0;
            var sum = 0.0;
            for (var i = 0; i < Points.Count; i++)
            {
                var a = Points[i];
                var b = Points[(i + 1) % Points.Count];
                sum += a.X * b.Y - b.X * a.Y;
            }
            return sum / 2;
        }
    }

    /// <summary>True if the polyline is closed and its points go counter-clockwise.</summary>
    public bool IsCounterClockwise => SignedArea > 0;

    /// <summary>The centre of area of a closed polyline; the average of its points if it has no area.</summary>
    public Point Centroid
    {
        get
        {
            var area = SignedArea;
            if (!IsClosed || Math.Abs(area) < Tolerance.Length)
                return new(Points.Average(p => p.X), Points.Average(p => p.Y));
            double x = 0, y = 0;
            foreach (var (a, b) in Edges())
            {
                var cross = a.X * b.Y - b.X * a.Y;
                x += (a.X + b.X) * cross;
                y += (a.Y + b.Y) * cross;
            }
            return new(x / (6 * area), y / (6 * area));
        }
    }

    /// <summary>
    /// The edge from point <paramref name="index"/> to the next one; the last edge of a closed polyline runs from the
    /// last point back to the first.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or not less than <see cref="EdgeCount"/>.</exception>
    public (Point Start, Point End) Edge(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, EdgeCount);
        return (Points[index], Points[(index + 1) % Points.Count]);
    }

    /// <summary>All edges in order, including the closing edge of a closed polyline.</summary>
    public IEnumerable<(Point Start, Point End)> Edges()
    {
        for (var i = 0; i < EdgeCount; i++) yield return Edge(i);
    }

    /// <summary>True if <paramref name="point"/> is inside or on the boundary of a closed polyline.</summary>
    public bool Contains(Point point)
    {
        if (!IsClosed || !Bounds.Inflated(Tolerance.Length).Contains(point)) return false;
        var inside = false;
        foreach (var (a, b) in Edges())
        {
            if (Measure.PointToSegment(point, a, b) <= Tolerance.Length) return true;
            if ((a.Y > point.Y) != (b.Y > point.Y))
            {
                var x = a.X + (point.Y - a.Y) * (b.X - a.X) / (b.Y - a.Y);
                if (point.X < x) inside = !inside;
            }
        }
        return inside;
    }
}
