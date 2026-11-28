using Tellurian.Trains.Schedules.Model.Drawings.Modules;

namespace Tellurian.Trains.Schedules.Model.Drawings.AutoDraw;

/// <summary>
/// An exit as registered in the Module Registry (<c>ModuleExit</c>). An exit with the end profile
/// for internal joints (Module Draw's auto-draw) is an internal joint between sections, not an exit.
/// </summary>
public sealed record AutoDrawExit : ExtensibleRecord
{
    /// <summary>The exit label, unique within the module.</summary>
    public required string Label { get; init; }
    /// <summary>The side of the module the exit is on.</summary>
    public ExitSide Direction { get; init; }

    /// <summary>The end profile designation, e.g. "FREMO-H0-B96".</summary>
    public required string EndProfile { get; init; }

    /// <summary>The end profile id in the catalogue, if known; takes precedence over the designation.</summary>
    public Guid? EndProfileId { get; init; }
}
