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
    /// Gets or sets the region name, written in the layout's default language. Required.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Country.Id"/> of the country this region belongs to. A new region
    /// defaults to the layout's default country (see <c>IdentitySettings.DefaultCountryId</c>); a region
    /// representing a foreign destination is set to that country instead.
    /// </summary>
    public int? CountryId { get; set; }

    /// <summary>
    /// Gets or sets the background colour used when rendering this region in notes, as a CSS colour
    /// string. The text colour is auto-contrasted from it (see DM-4.5.4). Chosen from the
    /// region <c>Palette</c>.
    /// </summary>
    public string BackgroundColor { get; set; } = "#ffff00";

    /// <summary>
    /// Markup display: the <see cref="Name"/> as a coloured chip, with the text colour auto-contrasted
    /// from <see cref="BackgroundColor"/>.
    /// </summary>
    public MarkupString ToHtmlMarkup =>
        new($"""
            <span class="region" style="background-color: {BackgroundColor}; color: {BackgroundColor.TextColor}">{Name}</span>
            """);
    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides the standard region palette and the localised default regions for a layout.
/// </summary>
public static class RegionExtensions
{
    extension(Region region)
    {
        /// <summary>
        /// The colours permitted for a region: the seven standard region colours plus purple and pink.
        /// </summary>
        public static IReadOnlyList<string> Palette =>
        [
            "#ffff00", // yellow
            "#009933", // green
            "#CC0000", // red
            "#996600", // brown
            "#000000", // black
            "#0066FF", // blue
            "#FF9933", // orange
            "#9900CC", // purple
            "#FF66CC", // pink
        ];

        /// <summary>
        /// The standard set of regions, named in the language of the country with the given
        /// <see cref="Country.Id"/> (falling back to English) and assigned to that country.
        /// </summary>
        /// <param name="countryId">The <see cref="Country.Id"/> of the layout's default country.</param>
        public static IEnumerable<Region> DefaultsFor(int countryId)
        {
            var language = Country.ById(countryId)?.Languages
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .FirstOrDefault() ?? "en";
            return Defaults.Select(d => new Region
            {
                Id = d.Id,
                Name = d.NameFor(language),
                CountryId = countryId,
                BackgroundColor = d.Color,
            });
        }
    }

    // The localised names of the standard regions; used only when generating the default regions, after
    // which each region keeps a single Name in the layout's default language.
    private static readonly RegionDefault[] Defaults =
    [
        new(1, "#ffff00", EN: "South",     DA: "Syd",      DE: "Süden",      NB: "Syd",        SV: "Söder"),
        new(2, "#009933", EN: "West",      DA: "Vest",     DE: "Westen",     NB: "Vest",       SV: "Väster"),
        new(3, "#CC0000", EN: "East",      DA: "Sjælland", DE: "Osten",      NB: "Øst",        SV: "Öster"),
        new(4, "#996600", EN: "Northwest", DA: "Nordvest", DE: "Rhen-Main",  NB: "Trøndelag",  SV: "Nordväst"),
        new(5, "#000000", EN: "Northeast", DA: "Fyn",      DE: "Rhurgebeit", NB: "",           SV: "Nordost"),
        new(6, "#0066FF", EN: "North",     DA: "Nord",     DE: "Norden",     NB: "Nord",       SV: "Norr"),
        new(7, "#FF9933", EN: "Middle",    DA: "Stykgods", DE: "Stückgut",   NB: "Stykgods",   SV: "Hallsberg"),
    ];

    private sealed record RegionDefault(int Id, string Color, string EN, string DA, string DE, string NB, string SV)
    {
        public string NameFor(string language) => language switch
        {
            "sv" => Or(SV),
            "da" => Or(DA),
            "de" => Or(DE),
            "nb" or "nn" => Or(NB),
            _ => EN,
        };

        private string Or(string? localised) => string.IsNullOrEmpty(localised) ? EN : localised;
    }
}
