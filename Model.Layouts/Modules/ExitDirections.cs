using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>
/// Works out <see cref="ExitSide.Up"/> and <see cref="ExitSide.Down"/> (ModuleDrawing §4.1).
/// Left and right are given: by the Registry, by auto-draw or by the user. An exit whose track leaves the module
/// across the left–right direction, such as a branch at a right angle, is then up or down. The viewer has the left
/// exit on the left, so up, the side away from the viewer, is to the left of the direction from the left exit to the
/// right one.
/// </summary>
public static class ExitDirections
{
    /// <summary>
    /// An exit leaves across the left–right direction when its heading is closer to straight across than to along:
    /// more than this angle, in degrees, from the direction from the left exit to the right one, and from the opposite direction.
    /// </summary>
    public const double AcrossAngle = 45;

    /// <summary>
    /// The direction of an exit at <paramref name="exit"/> when the left exit is at <paramref name="left"/> and the
    /// right one at <paramref name="right"/>: up or down when it leaves across the left–right direction, otherwise
    /// undefined, since an exit that leaves along it may be on either side.
    /// </summary>
    public static ExitSide Infer(Pose exit, Point left, Point right)
    {
        var across = right - left;
        if (across.Length < Tolerance.Length) return ExitSide.Undefined;
        var turn = (exit.Heading - across.Direction).Signed.Degrees;
        if (Math.Abs(turn) <= AcrossAngle || Math.Abs(turn) >= Angle.Half.Degrees - AcrossAngle) return ExitSide.Undefined;
        return turn > 0 ? ExitSide.Up : ExitSide.Down;
    }

    /// <summary>
    /// The drawing with up or down set for the exits that have no direction and leave across the left–right direction.
    /// Exits that have a direction keep it, so what the user has chosen isn't changed. Nothing is inferred until the
    /// drawing has both a left and a right exit.
    /// </summary>
    public static ModuleDrawing WithInferredExitDirections(this ModuleDrawing drawing)
    {
        ArgumentNullException.ThrowIfNull(drawing);
        var left = drawing.Exits.FirstOrDefault(e => e.Direction == ExitSide.Left);
        var right = drawing.Exits.FirstOrDefault(e => e.Direction == ExitSide.Right);
        if (left is null || right is null) return drawing;
        var inferred = drawing.Exits.Select(e => e.Direction == ExitSide.Undefined ? Infer(e.Pose, left.Pose.Position, right.Pose.Position) : e.Direction).ToList();
        if (inferred.SequenceEqual(drawing.Exits.Select(e => e.Direction))) return drawing;
        // The sides refer to the exits by side and number, which change with the directions.
        return drawing.With(copy => copy.ChangeExits(exits =>
        {
            for (var i = 0; i < exits.Count; i++) exits[i].Direction = inferred[i];
        }));
    }
}
