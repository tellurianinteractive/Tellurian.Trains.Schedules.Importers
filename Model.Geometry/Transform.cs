namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// A rigid transform: rotation about the origin followed by a translation.
/// It deliberately has no mirroring, because a physical module can't be mirrored.
/// </summary>
public readonly record struct Transform(Angle Rotation, Vector Translation)
{
    /// <summary>The transform that changes nothing.</summary>
    public static Transform Identity => new(Angle.Zero, Vector.Zero);

    /// <summary>The transform that maps <see cref="Pose.Origin"/> to <paramref name="pose"/>.</summary>
    public static Transform FromPose(Pose pose) => new(pose.Heading, pose.Position.AsVector);

    /// <summary>The point rotated about the origin and then translated.</summary>
    public Point Apply(Point point)
    {
        var rotated = point.AsVector.Rotated(Rotation);
        return new(rotated.X + Translation.X, rotated.Y + Translation.Y);
    }

    /// <summary>The vector rotated; a translation doesn't affect vectors.</summary>
    public Vector Apply(Vector vector) => vector.Rotated(Rotation);

    /// <summary>The angle rotated by <see cref="Rotation"/>.</summary>
    public Angle Apply(Angle angle) => angle + Rotation;

    /// <summary>The pose with its position and heading transformed.</summary>
    public Pose Apply(Pose pose) => new(Apply(pose.Position), Apply(pose.Heading));

    /// <summary>The transform that first applies this one and then <paramref name="next"/>.</summary>
    public Transform Then(Transform next) =>
        new(Rotation + next.Rotation, next.Apply(Translation) + next.Translation);

    /// <summary>The transform that undoes this one.</summary>
    public Transform Inverse()
    {
        var inverseRotation = -Rotation;
        return new(inverseRotation, -Translation.Rotated(inverseRotation));
    }

    /// <summary>True if the rotations are within <paramref name="angleTolerance"/> degrees and the translations within <paramref name="lengthTolerance"/> millimetres.</summary>
    public bool IsCloseTo(Transform other, double lengthTolerance = Tolerance.Length, double angleTolerance = Tolerance.Angle) =>
        Rotation.IsCloseTo(other.Rotation, angleTolerance) && Translation.IsCloseTo(other.Translation, lengthTolerance);

    /// <inheritdoc/>
    public override string ToString() => $"rotate {Rotation}, translate {Translation}";
}
