using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A region or country that a station represents, mostly a shadow station standing for the world beyond the layout.
/// An entry of the layout's catalogue, <see cref="Layout.Regions"/>.
/// </summary>
public sealed class Region
{
    /// <summary>The identity of the region within the layout.</summary>
    public int Id { get; set; }

    /// <summary>What the region is called.</summary>
    public required string Name { get; set; }

    /// <summary>The id of the country the region is in, or null.</summary>
    public int? CountryId { get; set; }

    /// <summary>The colour the region is shown in, as a CSS hex colour.</summary>
    public string BackgroundColor { get; set; } = "#ffff00";

    /// <summary>The properties of the JSON object that this version doesn't know, as read. They are written last.</summary>
    [JsonExtensionData]
    [JsonPropertyOrder(int.MaxValue)]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
