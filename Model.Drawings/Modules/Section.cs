namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>A physically separate baseboard part of a module drawing.</summary>
public sealed class Section : LayoutPart
{
    /// <summary>Unique within the drawing, e.g. "1", "2".</summary>
    public required string Label { get; set; }

    /// <summary>A closed path in module coordinates.</summary>
    public required PathDrawing Outline { get; set; }

    /// <inheritdoc/>
    public override Section Copy() => (Section)base.Copy();
}
