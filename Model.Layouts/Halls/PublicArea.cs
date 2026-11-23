namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>An area of a hall plan where visitors have access (HP-17).</summary>
public sealed class PublicArea : LayoutPart
{
    /// <summary>The identity of the area, unique among the hall plan's public areas; given by <see cref="HallPlan.Add(PublicArea)"/>.</summary>
    public int Id { get; set; }

    /// <summary>A closed path in hall coordinates, mm.</summary>
    public required PathDrawing Area { get; set; }

    /// <summary>A label shown in the drawing.</summary>
    public string? Label { get; set; }

    /// <summary>A note for the reader.</summary>
    public string? Note { get; set; }

    /// <inheritdoc/>
    public override PublicArea Copy() => (PublicArea)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => Label ?? Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
