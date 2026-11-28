using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Halls;

/// <summary>
/// How far the end of a line is from the exit it should join, a loop or a rejoining line closing (VA-5): the
/// <see cref="Line.EndAt"/> joint, with both modules placed.
/// </summary>
/// <param name="First">The last module's exit, where the line ends.</param>
/// <param name="Second">The exit it should join.</param>
/// <param name="Gap">The misfit, in the frame of <paramref name="First"/>.</param>
/// <param name="IsWithin">Whether the gap is within the hall plan's <see cref="HallPlanSettings.JointGap"/> and <see cref="HallPlanSettings.JointAngle"/>.</param>
public sealed record ClosureCheck(ExitReference First, ExitReference Second, ClosureGap Gap, bool IsWithin);
