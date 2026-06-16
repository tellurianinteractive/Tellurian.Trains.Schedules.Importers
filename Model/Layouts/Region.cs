using Microsoft.AspNetCore.Components;
using Tellurian.Utilities.Web;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Describes a destination outside the layout — a domestic region or a foreign country —
/// used for cargo flow routing. A <see cref="Station"/> (normally a shadow yard) can be
/// associated with zero, one, or several regions.
/// </summary>
public class Region
{
    /// <summary>
    /// Gets or sets the unique identifier for this region.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of this region.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the background colour used when rendering this region in notes,
    /// as a CSS colour string. The text colour is auto-contrasted from it (see DM-4.5.4).
    /// </summary>
    public string BackgroundColor { get; set; } = "#cccccc";

    /// <summary>
    /// Gets or sets a value indicating whether this region represents a foreign country
    /// (rendered with a flag icon) rather than a domestic region.
    /// </summary>
    public bool IsAbroad { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Markup display 
    /// </summary>
    public MarkupString Display =>
        new($"""
            <span class="region" style="background-color: {BackgroundColor}; color: {BackgroundColor.TextColor}">{Name}</span>
            """);
}
