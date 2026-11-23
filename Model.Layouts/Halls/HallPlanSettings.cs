namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>Tolerances and defaults of a hall plan (HP-1, HP-18, VA-3, VA-7, VA-10). Whether ends fit is the layout's <see cref="Layout.FitTolerances"/>.</summary>
public sealed record HallPlanSettings : ExtensibleRecord
{
    /// <summary>The width of a walking path that has none of its own, mm.</summary>
    public const double DefaultWalkingPathWidth = 600;

    /// <summary>The largest position gap of a rejoining or closing joint that is still accepted, mm (VA-3).</summary>
    public double JointGap { get; init; } = 2;

    /// <summary>The largest angle error of a rejoining or closing joint that is still accepted, degrees (VA-3).</summary>
    public double JointAngle { get; init; } = 0.5;

    /// <summary>The largest rail-top height step at a joint, mm (VA-10).</summary>
    public double Height { get; init; } = 1;

    /// <summary>The minimum clearance between modules, and to walls and obstacles, mm; null uses the preferred standard's (VA-7).</summary>
    public double? MinimumClearance { get; init; }

    /// <summary>The clearance check can be switched off (VA-7).</summary>
    public bool IsClearanceChecked { get; init; } = true;

    /// <summary>The width of walking paths that have none of their own, mm (HP-18).</summary>
    public double WalkingPathWidth { get; init; } = DefaultWalkingPathWidth;
}
