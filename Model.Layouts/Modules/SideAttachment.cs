namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>
/// The part of a side that a zone (operator space, public side) is attached to: from
/// <see cref="From"/> to <see cref="To"/> mm along the side, measured from its start.
/// A null <see cref="To"/> means the end of the side.
/// </summary>
public sealed record SideAttachment : ExtensibleRecord
{
    /// <summary>Index into <see cref="ModuleDrawing.Sides"/>.</summary>
    public required int SideIndex { get; init; }

    /// <summary>Start of the attached part, mm along the side from its start.</summary>
    public double From { get; init; }
    /// <summary>End of the attached part, mm along the side from its start; null for the end of the side.</summary>
    public double? To { get; init; }
}
