namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>An axis-aligned bounding box. <see cref="Empty"/> contains nothing.</summary>
public readonly record struct BoundingBox(Point Min, Point Max)
{
    /// <summary>The empty box, with infinite inverted bounds, so that including any point gives a box around just that point.</summary>
    public static BoundingBox Empty => new(
        new(double.PositiveInfinity, double.PositiveInfinity),
        new(double.NegativeInfinity, double.NegativeInfinity));

    /// <summary>True if the box contains nothing.</summary>
    public bool IsEmpty => Min.X > Max.X || Min.Y > Max.Y;
    /// <summary>The extent along X; 0 when empty.</summary>
    public double Width => IsEmpty ? 0 : Max.X - Min.X;
    /// <summary>The extent along Y; 0 when empty.</summary>
    public double Height => IsEmpty ? 0 : Max.Y - Min.Y;
    /// <summary>The centre of the box. Not defined for an empty box.</summary>
    public Point Center => new((Min.X + Max.X) / 2, (Min.Y + Max.Y) / 2);

    /// <summary>The smallest box containing <paramref name="points"/>; <see cref="Empty"/> if there are none.</summary>
    public static BoundingBox FromPoints(IEnumerable<Point> points) =>
        points.Aggregate(Empty, (box, point) => box.Including(point));

    /// <summary>The smallest box containing this box and <paramref name="point"/>.</summary>
    public BoundingBox Including(Point point) => new(
        new(Math.Min(Min.X, point.X), Math.Min(Min.Y, point.Y)),
        new(Math.Max(Max.X, point.X), Math.Max(Max.Y, point.Y)));

    /// <summary>The smallest box containing both boxes.</summary>
    public BoundingBox Union(BoundingBox other) =>
        IsEmpty ? other :
        other.IsEmpty ? this :
        new(new(Math.Min(Min.X, other.Min.X), Math.Min(Min.Y, other.Min.Y)),
            new(Math.Max(Max.X, other.Max.X), Math.Max(Max.Y, other.Max.Y)));

    /// <summary>The box grown by <paramref name="margin"/> on every side.</summary>
    public BoundingBox Inflated(double margin) =>
        IsEmpty ? this : new(new(Min.X - margin, Min.Y - margin), new(Max.X + margin, Max.Y + margin));

    /// <summary>True if <paramref name="point"/> is inside or on the boundary of the box.</summary>
    public bool Contains(Point point) =>
        point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y;

    /// <summary>True if the boxes overlap or touch; never for an empty box.</summary>
    public bool Intersects(BoundingBox other) =>
        !IsEmpty && !other.IsEmpty &&
        Min.X <= other.Max.X && other.Min.X <= Max.X &&
        Min.Y <= other.Max.Y && other.Min.Y <= Max.Y;
}
