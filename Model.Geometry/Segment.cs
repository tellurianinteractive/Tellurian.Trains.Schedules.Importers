namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// A part of a path: a straight line or a circular arc. Segments are stored exactly;
/// tessellation is only used by algorithms that need polylines.
/// </summary>
public abstract record Segment
{
    /// <summary>The start point.</summary>
    public abstract Point Start { get; }
    /// <summary>The end point.</summary>
    public abstract Point End { get; }
    /// <summary>The length along the segment in millimetres.</summary>
    public abstract double Length { get; }

    /// <summary>Direction of travel at the start.</summary>
    public abstract Angle StartHeading { get; }

    /// <summary>Direction of travel at the end.</summary>
    public abstract Angle EndHeading { get; }

    /// <summary>The exact bounding box of the segment.</summary>
    public abstract BoundingBox Bounds { get; }

    /// <summary>The start point with the direction of travel there.</summary>
    public Pose StartPose => new(Start, StartHeading);
    /// <summary>The end point with the direction of travel there.</summary>
    public Pose EndPose => new(End, EndHeading);

    /// <summary>The point at <paramref name="distance"/> from the start, clamped to the segment.</summary>
    public abstract Point PointAt(double distance);

    /// <summary>The direction of travel at <paramref name="distance"/> from the start, clamped to the segment.</summary>
    public abstract Angle HeadingAt(double distance);

    /// <summary>The point and direction of travel at <paramref name="distance"/> from the start, clamped to the segment.</summary>
    public Pose PoseAt(double distance) => new(PointAt(distance), HeadingAt(distance));

    /// <summary>The same segment, travelled in the opposite direction.</summary>
    public abstract Segment Reversed();

    /// <summary>
    /// The part of the segment from <paramref name="from"/> to <paramref name="to"/> along it (clamped).
    /// The part must have a length.
    /// </summary>
    public abstract Segment Between(double from, double to);

    /// <summary>The segment rotated and translated by <paramref name="transform"/>.</summary>
    public abstract Segment Transformed(Transform transform);

    /// <summary>The segment mirrored about the Y axis. Only for comparing end profiles.</summary>
    public abstract Segment MirroredX();

    /// <summary>
    /// A parallel segment at <paramref name="distance"/> to the left of the direction of travel
    /// (negative distance: to the right). Arcs keep their centre and change their radius.
    /// </summary>
    public abstract Segment Offset(double distance);

    /// <summary>
    /// Appends points along the segment to <paramref name="points"/>, excluding the start point.
    /// The deviation from the true geometry is at most <paramref name="chordTolerance"/>.
    /// </summary>
    public abstract void AppendTessellation(List<Point> points, double chordTolerance);

    /// <summary>The distance from the start to the point on the segment closest to <paramref name="point"/>.</summary>
    public abstract double ClosestDistance(Point point);

    /// <summary><paramref name="distance"/> limited to the range from 0 to <see cref="Length"/>.</summary>
    protected double Clamp(double distance) => Math.Clamp(distance, 0, Length);
}
