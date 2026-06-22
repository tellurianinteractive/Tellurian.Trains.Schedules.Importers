using System.Globalization;
using Microsoft.AspNetCore.Components;
using Tellurian.Trains.Schedules.Model.Resources;
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
    /// The resource key to use for translating the region name.
    /// </summary>
    public required string EnglishName { get; set; }

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


    /// <summary>
    /// The region name localised for the current UI culture. <see cref="EnglishName"/> is used as the
    /// resource key (see <c>Resources/Regions.resx</c>); names without a translation — such as
    /// user-defined regions outside the standard <c>Defaults</c> set — fall back to the key.
    /// </summary>
    public string Name =>
        Regions.ResourceManager.GetString(EnglishName, CultureInfo.CurrentUICulture) ?? EnglishName;

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
             new (){ Id=1, EnglishName="South", BackgroundColor="#ffff00" },
             new (){ Id=2, EnglishName="West", BackgroundColor="#009933" },
             new (){ Id=3, EnglishName="East", BackgroundColor="#CC0000" },
             new (){ Id=4, EnglishName="Northwest", BackgroundColor="#996600" },
             new (){ Id=5, EnglishName="Northeast", BackgroundColor="#000000" },
             new (){ Id=6, EnglishName="North", BackgroundColor="#0066FF" },
             new (){ Id=7, EnglishName="Middle", BackgroundColor="#FF9933" },
        ];
    }
}
