namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// A circular arc around <see cref="Center"/>. <see cref="StartAngle"/> is the direction from the centre
/// to the start point. <see cref="SweepDegrees"/> is positive counter-clockwise (turning left) and
/// negative clockwise (turning right).
/// </summary>
public sealed record ArcSegment : Segment
{
    /// <summary>An arc around <paramref name="center"/> with <paramref name="radius"/> millimetres, from the direction <paramref name="startAngle"/> seen from the centre, sweeping <paramref name="sweepDegrees"/> (positive counter-clockwise).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The radius is less than <see cref="Tolerance.Length"/>, or the sweep is closer to zero than <see cref="Tolerance.Angle"/>.</exception>
    public ArcSegment(Point center, double radius, Angle startAngle, double sweepDegrees)
    {
        if (radius < Tolerance.Length) throw new ArgumentOutOfRangeException(nameof(radius), radius, "The radius must be positive.");
        if (Math.Abs(sweepDegrees) < Tolerance.Angle) throw new ArgumentOutOfRangeException(nameof(sweepDegrees), sweepDegrees, "The sweep must not be zero.");
        Center = center;
        Radius = radius;
        StartAngle = startAngle;
        SweepDegrees = sweepDegrees;
    }

    /// <summary>The centre of the circle.</summary>
    public Point Center { get; }
    /// <summary>The radius in millimetres.</summary>
    public double Radius { get; }
    /// <summary>The direction from the centre to the start point.</summary>
    public Angle StartAngle { get; }
    /// <summary>The swept angle in degrees: positive counter-clockwise (turning left), negative clockwise (turning right).</summary>
    public double SweepDegrees { get; }

    /// <summary>True if the arc turns left.</summary>
    public bool IsCounterClockwise => SweepDegrees > 0;
    /// <summary>The direction from the centre to the end point.</summary>
    public Angle EndAngle => StartAngle + new Angle(SweepDegrees);

    /// <inheritdoc/>
    public override Point Start => PointOnCircle(StartAngle);
    /// <inheritdoc/>
    public override Point End => PointOnCircle(EndAngle);
    /// <inheritdoc/>
    public override double Length => Radius * Math.Abs(SweepDegrees) * Math.PI / 180.0;
    /// <inheritdoc/>
    public override Angle StartHeading => TangentAt(StartAngle);
    /// <inheritdoc/>
    public override Angle EndHeading => TangentAt(EndAngle);

    /// <summary>The exact bounding box, including the extreme points of the circle that the arc passes.</summary>
    public override BoundingBox Bounds
    {
        get
        {
            var box = BoundingBox.Empty.Including(Start).Including(End);
            for (var cardinal = 0; cardinal < 360; cardinal += 90)
            {
                var angle = new Angle(cardinal);
                if (Covers(angle)) box = box.Including(PointOnCircle(angle));
            }
            return box;
        }
    }

    /// <summary>
    /// An arc starting at <paramref name="start"/>, tangent to its heading, turning
    /// <paramref name="turnDegrees"/> (positive left, negative right) with <paramref name="radius"/>.
    /// </summary>
    public static ArcSegment FromStart(Pose start, double radius, double turnDegrees)
    {
        var turnsLeft = turnDegrees > 0;
        var center = start.Position + start.Left * (turnsLeft ? radius : -radius);
        var startAngle = start.Heading + new Angle(turnsLeft ? -90 : 90);
        return new ArcSegment(center, radius, startAngle, turnDegrees);
    }

    /// <inheritdoc/>
    public override Point PointAt(double distance) => PointOnCircle(AngleAt(distance));

    /// <inheritdoc/>
    public override Angle HeadingAt(double distance) => TangentAt(AngleAt(distance));

    /// <inheritdoc/>
    public override Segment Reversed() => new ArcSegment(Center, Radius, EndAngle, -SweepDegrees);

    /// <inheritdoc/>
    public override Segment Between(double from, double to)
    {
        var start = Clamp(from);
        var end = Clamp(to);
        return new ArcSegment(Center, Radius, AngleAt(start), Math.Sign(SweepDegrees) * (end - start) / Radius * 180.0 / Math.PI);
    }

    /// <inheritdoc/>
    public override Segment Transformed(Transform transform) =>
        new ArcSegment(transform.Apply(Center), Radius, transform.Apply(StartAngle), SweepDegrees);

    /// <inheritdoc/>
    public override Segment MirroredX() =>
        new ArcSegment(Center.MirroredX(), Radius, Angle.Half - StartAngle, -SweepDegrees);

    /// <summary>A concentric arc at <paramref name="distance"/> to the left of the direction of travel (negative: to the right).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The offset would make the radius zero or negative.</exception>
    public override Segment Offset(double distance)
    {
        // The centre is on the left when turning left, so an offset to the left shrinks the radius.
        var radius = IsCounterClockwise ? Radius - distance : Radius + distance;
        if (radius < Tolerance.Length) throw new ArgumentOutOfRangeException(nameof(distance), distance, "The offset is larger than the radius.");
        return new ArcSegment(Center, radius, StartAngle, SweepDegrees);
    }

    /// <inheritdoc/>
    public override void AppendTessellation(List<Point> points, double chordTolerance)
    {
        var steps = StepsFor(chordTolerance);
        for (var i = 1; i <= steps; i++)
        {
            points.Add(PointOnCircle(StartAngle + new Angle(SweepDegrees * i / steps)));
        }
    }

    /// <summary>The distance from the start to the closest point on the arc; for a point outside the arc's angular range, the nearer end (0 or <see cref="Length"/>).</summary>
    public override double ClosestDistance(Point point)
    {
        var direction = point - Center;
        if (direction.Length >= Tolerance.Length && Covers(direction.Direction))
        {
            var delta = IsCounterClockwise ? (direction.Direction - StartAngle).Normalized.Degrees : (StartAngle - direction.Direction).Normalized.Degrees;
            return Clamp(delta * Math.PI / 180.0 * Radius);
        }
        return point.DistanceTo(Start) <= point.DistanceTo(End) ? 0 : Length;
    }

    /// <summary>True if the direction <paramref name="angle"/> from the centre lies within the arc.</summary>
    public bool Covers(Angle angle)
    {
        if (Math.Abs(SweepDegrees) >= 360) return true;
        var delta = IsCounterClockwise ? (angle - StartAngle).Normalized.Degrees : (StartAngle - angle).Normalized.Degrees;
        return delta <= Math.Abs(SweepDegrees) + Tolerance.Angle;
    }

    private int StepsFor(double chordTolerance)
    {
        var tolerance = Math.Clamp(chordTolerance, Tolerance.Length, Radius);
        var maxStepRadians = 2 * Math.Acos(1 - tolerance / Radius);
        var sweepRadians = Math.Abs(SweepDegrees) * Math.PI / 180.0;
        return Math.Max(1, (int)Math.Ceiling(sweepRadians / maxStepRadians));
    }

    private Angle AngleAt(double distance) =>
        StartAngle + new Angle(Math.Sign(SweepDegrees) * Clamp(distance) / Radius * 180.0 / Math.PI);

    private Point PointOnCircle(Angle angle) => Center + Vector.FromAngle(angle, Radius);

    private Angle TangentAt(Angle angle) => angle + new Angle(IsCounterClockwise ? 90 : -90);
}
