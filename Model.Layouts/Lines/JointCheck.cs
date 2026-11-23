namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// How a joint between two exits in the lines of a <see cref="Layout"/> works out (HP-11, VA-10): the fit of the two
/// ends, the join method, and the step in rail-top height. Worked out, never stored; the planner's own decisions are in
/// <see cref="Decision"/>.
/// </summary>
public sealed record JointCheck
{
    /// <summary>The exit the line runs from.</summary>
    public required ExitReference First { get; init; }

    /// <summary>The exit the line runs to.</summary>
    public required ExitReference Second { get; init; }

    /// <summary>The <see cref="Line.Id"/> of the line the joint is part of.</summary>
    public required int LineId { get; init; }

    /// <summary>The joint is a line's <see cref="Line.EndAt"/>, closing a loop or a rejoining line.</summary>
    public bool IsClosing { get; init; }

    /// <summary>
    /// How the two ends fit, by the end profiles as built at the exits and the layout's overrides and tolerances; null
    /// when an exit isn't drawn or its end profile isn't in <see cref="Layout.EndProfiles"/>.
    /// </summary>
    public JointFitResult? Fit { get; init; }

    /// <summary>What the planner has decided for the joint, if anything.</summary>
    public Joint? Decision { get; init; }

    /// <summary>How the ends are held together: as the planner decided, else as the fit says; null when neither tells.</summary>
    public JoinMethod? JoinMethod => Decision?.JoinMethodOverride ?? Fit?.JoinMethod;

    /// <summary>The number of clamps of a clamped joint: as the planner decided, else the default.</summary>
    public int NumberOfClamps => Decision?.NumberOfClamps ?? Joint.DefaultNumberOfClamps;

    /// <summary>
    /// The rail top at <see cref="Second"/> minus that at <see cref="First"/>, mm: each exit's
    /// <see cref="ModuleExit.RailTopOffset"/> plus its module's <see cref="LineEntry.LegAdjustment"/>. Null when an exit
    /// isn't drawn.
    /// </summary>
    public double? HeightStep { get; init; }

    /// <inheritdoc/>
    public override string ToString() => $"{First} – {Second}";
}
