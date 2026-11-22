namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>A straight segment from <see cref="Start"/> to <see cref="End"/>.</summary>
public sealed record LineSegment : Segment
{
    private readonly Point _start;
    private readonly Point _end;

    /// <summary>A line from <paramref name="start"/> to <paramref name="end"/>, which must not be the same point.</summary>
    /// <exception cref="ArgumentException">The points are closer than <see cref="Tolerance.Length"/>.</exception>
    public LineSegment(Point start, Point end)
    {
        if (start.IsCloseTo(end)) throw new ArgumentException("A line segment must have a length.", nameof(end));
        _start = start;
        _end = end;
    }

    /// <inheritdoc/>
    public override Point Start => _start;
    /// <inheritdoc/>
    public override Point End => _end;
    /// <inheritdoc/>
    public override double Length => _start.DistanceTo(_end);
    /// <inheritdoc/>
    public override Angle StartHeading => (_end - _start).Direction;
    /// <inheritdoc/>
    public override Angle EndHeading => StartHeading;
    /// <inheritdoc/>
    public override BoundingBox Bounds => BoundingBox.Empty.Including(_start).Including(_end);

    /// <summary>A line of <paramref name="length"/> starting at <paramref name="start"/> in its heading.</summary>
    public static LineSegment FromStart(Pose start, double length) => new(start.Position, start.Advanced(length).Position);

    /// <inheritdoc/>
    public override Point PointAt(double distance) => Point.Lerp(_start, _end, Clamp(distance) / Length);

    /// <inheritdoc/>
    public override Angle HeadingAt(double distance) => StartHeading;

    /// <inheritdoc/>
    public override Segment Reversed() => new LineSegment(_end, _start);

    /// <inheritdoc/>
    public override Segment Between(double from, double to) => new LineSegment(PointAt(from), PointAt(to));

    /// <inheritdoc/>
    public override Segment Transformed(Transform transform) => new LineSegment(transform.Apply(_start), transform.Apply(_end));

    /// <inheritdoc/>
    public override Segment MirroredX() => new LineSegment(_start.MirroredX(), _end.MirroredX());

    /// <inheritdoc/>
    public override Segment Offset(double distance)
    {
        var shift = (_end - _start).Normalized.LeftNormal * distance;
        return new LineSegment(_start + shift, _end + shift);
    }

    /// <summary>Appends only the end point; a line needs no tessellation.</summary>
    public override void AppendTessellation(List<Point> points, double chordTolerance) => points.Add(_end);

    /// <inheritdoc/>
    public override double ClosestDistance(Point point) => Clamp((point - _start).Dot((_end - _start).Normalized));
}
