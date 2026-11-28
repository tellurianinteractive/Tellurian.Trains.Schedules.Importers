using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A module of the hall plan a <see cref="Layout"/> was made in, as the layout keeps it: what it is called, who owns it,
/// the package it belongs to and the location it is part of, without its drawing (<c>ModelSurfaceAnalysis.md</c> 3.3).
/// Every module is listed, also the plain-line ones that are only part of the distance of a <see cref="TrackStretch"/>.
/// </summary>
public sealed class LayoutModule
{
    /// <summary>The identity of the module within the layout, which stretches refer to (<see cref="TrackStretch.ModuleIds"/>).</summary>
    public int Id { get; set; }

    /// <summary>The identity of the module in its own file, the same in every hall plan it is placed in; null when not given.</summary>
    public Guid? SourceId { get; set; }

    /// <summary>What the module is called: its title, with prefix, number and name as far as they are given.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Who owns the module, as written in its file; null when not stated.</summary>
    public string? Owners { get; set; }

    /// <summary>The label of the package of modules it belongs to, if any.</summary>
    public string? PackageLabel { get; set; }

    /// <summary>The <see cref="OperationLocation.Id"/> of the location the module is part of; null for plain line.</summary>
    public int? OperationLocationId { get; set; }

    /// <summary>
    /// What other systems keep about the module, with one entry per system, copied unchanged from its source, e.g. the
    /// Module Registry's module id. An update uses it to find the same module again.
    /// </summary>
    public Dictionary<string, JsonElement>? ExternalReferences { get; set; }

    /// <summary>A copy, with a copy of the external references.</summary>
    public LayoutModule Copy()
    {
        var copy = (LayoutModule)MemberwiseClone();
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        return copy;
    }

    /// <inheritdoc/>
    public override string ToString() => Title;
}
