namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>
/// The outline between two consecutive end plates, walking counter-clockwise around the module:
/// from the right corner of <see cref="FromExit"/>'s plate to the left corner of <see cref="ToExit"/>'s
/// (corners as seen standing outside the plate). An end-of-line module's only side starts and ends at its one exit.
/// </summary>
public sealed class ModuleSide : LayoutPart
{
    /// <summary>The label of the exit whose end plate the side starts at.</summary>
    public required string FromExit { get; set; }
    /// <summary>The label of the exit whose end plate the side ends at.</summary>
    public required string ToExit { get; set; }
    /// <summary>Whether the side is generated or drawn by hand.</summary>
    public SideMode Mode { get; set; }

    /// <summary>The geometry, also stored for auto sides so that files can be shown without regenerating.</summary>
    public required PathDrawing Path { get; set; }

    /// <inheritdoc/>
    public override ModuleSide Copy() => (ModuleSide)base.Copy();
}
