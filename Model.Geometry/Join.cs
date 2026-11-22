namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// The math for joining modules exit to exit. An exit pose points out of its module,
/// so two joined exits are at the same position facing opposite ways.
/// </summary>
public static class Join
{
    /// <summary>
    /// The placement transform that puts a module's exit <paramref name="localExit"/> (in module
    /// coordinates) against the exit <paramref name="target"/> (in world coordinates):
    /// <c>T ∘ Rot(180°) ∘ E⁻¹</c>.
    /// </summary>
    public static Transform PlacementFor(Pose target, Pose localExit) =>
        Transform.FromPose(localExit).Inverse().Then(Transform.FromPose(target.Reversed()));

    /// <summary>
    /// How far <paramref name="other"/> is from fitting against <paramref name="exit"/>, expressed in the
    /// frame of <paramref name="exit"/>: X along its heading (positive: the other exit is too far out),
    /// Y to its left, and the angle the other exit must turn to face <paramref name="exit"/> exactly.
    /// All zero when the exits fit. Used for the closure gap of loops and rejoining lines.
    /// </summary>
    public static ClosureGap Gap(Pose exit, Pose other)
    {
        var offset = (other.Position - exit.Position).Rotated(-exit.Heading);
        var angle = (other.Heading - exit.Reversed().Heading).Signed;
        return new(offset.X, offset.Y, angle);
    }
}
