namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// A position with a heading. Used for exits (heading out of the module), track connection points
/// and the placement of modules.
/// </summary>
public readonly record struct Pose(Point Position, Angle Heading)
{
    /// <summary>At the origin, heading along the positive X axis.</summary>
    public static Pose Origin => new(Point.Origin, Angle.Zero);

    /// <summary>Unit vector in the heading direction.</summary>
    public Vector Forward => Vector.FromAngle(Heading);

    /// <summary>Unit vector 90° to the left of the heading.</summary>
    public Vector Left => Forward.LeftNormal;

    /// <summary>The same position, facing the opposite way.</summary>
    public Pose Reversed() => this with { Heading = Heading + Angle.Half };

    /// <summary>The pose moved <paramref name="distance"/> along its heading.</summary>
    public Pose Advanced(double distance) => this with { Position = Position + Forward * distance };

    /// <summary>True if the positions are within <paramref name="lengthTolerance"/> millimetres and the headings within <paramref name="angleTolerance"/> degrees.</summary>
    public bool IsCloseTo(Pose other, double lengthTolerance = Tolerance.Length, double angleTolerance = Tolerance.Angle) =>
        Position.IsCloseTo(other.Position, lengthTolerance) && Heading.IsCloseTo(other.Heading, angleTolerance);

    /// <inheritdoc/>
    public override string ToString() => $"{Position} {Heading}";
}
