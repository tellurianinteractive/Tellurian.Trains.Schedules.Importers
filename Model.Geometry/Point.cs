namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>A 2D point in millimetres.</summary>
public readonly record struct Point(double X, double Y)
{
    /// <summary>The point (0, 0).</summary>
    public static Point Origin => new(0, 0);

    /// <summary>The vector from the origin to the point.</summary>
    public Vector AsVector => new(X, Y);

    /// <summary>The distance to <paramref name="other"/> in millimetres.</summary>
    public double DistanceTo(Point other) => (other - this).Length;

    /// <summary>True if the points are at most <paramref name="tolerance"/> millimetres apart.</summary>
    public bool IsCloseTo(Point other, double tolerance = Tolerance.Length) => DistanceTo(other) <= tolerance;

    /// <summary>
    /// The point mirrored about the Y axis (X → −X). Used only for comparing end profiles,
    /// where two facing ends are each seen from outside their own module. Modules are never mirrored.
    /// </summary>
    public Point MirroredX() => new(-X, Y);

    /// <summary>The point at fraction <paramref name="t"/> of the way from <paramref name="a"/> to <paramref name="b"/>; not clamped to [0, 1].</summary>
    public static Point Lerp(Point a, Point b, double t) => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    /// <summary>The vector from <paramref name="b"/> to <paramref name="a"/>.</summary>
    public static Vector operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
    /// <summary>The point moved by <paramref name="v"/>.</summary>
    public static Point operator +(Point p, Vector v) => new(p.X + v.X, p.Y + v.Y);
    /// <summary>The point moved by the opposite of <paramref name="v"/>.</summary>
    public static Point operator -(Point p, Vector v) => new(p.X - v.X, p.Y - v.Y);

    /// <inheritdoc/>
    public override string ToString() => $"({X:0.###}, {Y:0.###})";
}
