using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

/// <summary>
/// Extra requirements on top of a base standard, e.g. MMM-Summer and MMM-Modern on MMM-2R. An entry of the catalogue
/// with an identity of its own, so a mutable class like the rest of the shared model.
/// </summary>
public sealed class StandardAddOn : LayoutPart
{
    /// <summary>The identity of the add-on, the same in every file.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Guid Id { get; set; }

    /// <summary>What the add-on is called, e.g. "MMM-Summer".</summary>
    public required string ShortName { get; set; }

    /// <summary>The standard the add-on adds to.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Guid BaseStandardId { get; set; }

    /// <summary>What the add-on requires, for the reader.</summary>
    public string? Description { get; set; }

    /// <summary>The document the add-on is defined in.</summary>
    public DocumentReference? Source { get; set; }

    /// <inheritdoc/>
    public override StandardAddOn Copy() => (StandardAddOn)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => ShortName;
}
