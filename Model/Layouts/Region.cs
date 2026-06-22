using Microsoft.AspNetCore.Components;
using Tellurian.Localization;
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

    /// <summary>The region name in English. Required; used as the fallback for untranslated cultures.</summary>
    public required string EN { get; set; }
    /// <summary>The region name in Danish, or <c>null</c> when not translated.</summary>
    public string? DA { get; set; }
    /// <summary>The region name in German, or <c>null</c> when not translated.</summary>
    public string? DE { get; set; }
    /// <summary>The region name in Norwegian Bokmål, or <c>null</c> when not translated.</summary>
    public string? NB { get; set; }
    /// <summary>The region name in Swedish, or <c>null</c> when not translated.</summary>
    public string? SV { get; set; }

    /// <summary>
    /// Selects the language-specific name property (<see cref="EN"/>, <see cref="SV"/>, …) matching the
    /// current UI culture. Assigned once at application start-up (see <c>Program.cs</c>); contexts where it
    /// is not configured fall back to the English <see cref="EN"/> name.
    /// </summary>
    public static ISynchronousResourceProvider? Localizer { get; set; }

    /// <summary>
    /// Gets or sets the background colour used when rendering this region in notes,
    /// as a CSS colour string. The text colour is auto-contrasted from it (see DM-4.5.4).
    /// </summary>
    public string BackgroundColor { get; set; } = "#cccccc";

    /// <summary>
    /// The region name localised for the current UI culture, selected from the language-specific
    /// properties (<see cref="EN"/>, <see cref="SV"/>, …) via <see cref="Localizer"/>. Falls back to
    /// the English <see cref="EN"/> name when no localiser is configured or the matching property is
    /// empty — for example an untranslated, user-defined region.
    /// </summary>
    public string Name
    {
        get
        {
            var localised = Localizer?.GetTranslation(this).Text;
            return string.IsNullOrEmpty(localised) ? EN : localised;
        }
    }

    /// <summary>
    /// Markup display: the localised <see cref="Name"/> as a coloured chip, with the text colour
    /// auto-contrasted from <see cref="BackgroundColor"/>.
    /// </summary>
    public MarkupString Display =>
        new($"""
            <span class="region" style="background-color: {BackgroundColor}; color: {BackgroundColor.TextColor}">{Name}</span>
            """);
}

/// <summary>
/// 
/// </summary>
public static class RegionExtensions
{
    extension(Region region)
    {
        /// <summary>
        /// The standard set of regions.
        /// </summary>
        public static IEnumerable<Region> Defaults => [
             new (){ Id=1, EN="South", DA="Syd", DE="Süden", NB="Syd", SV="Söder", BackgroundColor="#ffff00" },
             new (){ Id=2, EN="West", DA="Vest", DE="Westen", NB="Vest", SV="Väster", BackgroundColor="#009933" },
             new (){ Id=3, EN="East", DA="Sjælland", DE="Osten", NB="Øst", SV="Öster", BackgroundColor="#CC0000" },
             new (){ Id=4, EN="Northwest", DA="Nordvest", DE="Rhen-Main", NB="Trøndelag", SV="Nordväst", BackgroundColor="#996600" },
             new (){ Id=5, EN="Northeast", DA="Fyn", DE="Rhurgebeit", NB="", SV="Nordost", BackgroundColor="#000000" },
             new (){ Id=6, EN="North", DA="Nord", DE="Norden", NB="Nord", SV="Norr", BackgroundColor="#0066FF" },
             new (){ Id=7, EN="Middle", DA="Stykgods", DE="Stückgut", NB="Stykgods", SV="Hallsberg", BackgroundColor="#FF9933" },
        ];
    }
}
