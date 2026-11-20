using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A module set up in a <see cref="Layout"/>, as it was when it was placed. Every module of the hall plan is listed,
/// also the plain-line ones that are only part of the distance of a <see cref="TrackStretch"/>. What is decided for the
/// layout is on the module's <see cref="Module.OperationLocation"/> and its tracks.
/// </summary>
public sealed class LayoutModule : LayoutPart
{
    /// <summary>The identity of the module within the layout, which stretches refer to. Given once and never changed.</summary>
    public int Id { get; set; }

    /// <summary>
    /// The module itself. When it is updated from its source, what it is is copied onto it and what is decided stays.
    /// Set here, the module is placed in a layout, which decides how it is written (<see cref="Module.IsPlaced"/>).
    /// </summary>
    public required Module Module
    {
        get;
        set { field = value; if (value is not null) value.IsPlaced = true; }
    }

    /// <summary>The id of the module drawing, when the module comes from one.</summary>
    public Guid? SourceId { get; set; }

    /// <summary>
    /// What other systems keep about the module, with one entry per system, copied unchanged from its source, e.g. the
    /// Module Registry's module id. An import uses it to find the same module again.
    /// </summary>
    public Dictionary<string, JsonElement>? ExternalReferences { get; set; }

    /// <summary>A copy with a copy of the module.</summary>
    public override LayoutModule Copy()
    {
        var copy = (LayoutModule)base.Copy();
        copy.Module = Module.Copy();
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        return copy;
    }

    /// <inheritdoc/>
    public override string ToString() => Module.ToString();
}
