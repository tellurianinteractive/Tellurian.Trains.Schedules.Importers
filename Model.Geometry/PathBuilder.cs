namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Builds a path step by step from a start pose, e.g. a module main path:
/// <c>new PathBuilder(Pose.Origin).Straight(500).Curve(2000, 30).Build()</c>.
/// </summary>
public sealed class PathBuilder(Pose start)
{
    private readonly List<Segment> _segments = [];

    /// <summary>The pose at the end of the path built so far.</summary>
    public Pose Current { get; private set; } = start;

    /// <summary>
    /// A straight line of <paramref name="length"/> millimetres in the current heading; ignored if within
    /// <see cref="Tolerance.Length"/> of zero, so a rounding error doesn't add a step.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is negative beyond the tolerance.</exception>
    public PathBuilder Straight(double length)
    {
        if (length < -Tolerance.Length) throw new ArgumentOutOfRangeException(nameof(length), length, "A straight can't have a negative length.");
        if (length <= Tolerance.Length) return this;
        return Add(LineSegment.FromStart(Current, length));
    }

    /// <summary>A tangent arc turning <paramref name="turnDegrees"/> (positive left, negative right).</summary>
    public PathBuilder Curve(double radius, double turnDegrees)
    {
        if (Math.Abs(turnDegrees) <= Tolerance.Angle) return this;
        return Add(ArcSegment.FromStart(Current, radius, turnDegrees));
    }

    /// <summary>A straight line to <paramref name="point"/>; the heading becomes the line's direction.</summary>
    public PathBuilder LineTo(Point point) => Add(new LineSegment(Current.Position, point));

    /// <summary>Appends <paramref name="segment"/> and moves <see cref="Current"/> to its end. The segment must start at <see cref="Current"/>, or <see cref="Build"/> throws.</summary>
    public PathBuilder Add(Segment segment)
    {
        _segments.Add(segment);
        Current = segment.EndPose;
        return this;
    }

    /// <summary>The path of the segments added so far; it must have at least one segment, and a closed path must end where it starts.</summary>
    public SegmentPath Build(bool isClosed = false) => new(_segments, isClosed);
}
