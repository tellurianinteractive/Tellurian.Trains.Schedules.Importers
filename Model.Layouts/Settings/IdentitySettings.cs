namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// Identity settings for a layout: the operating theme, the modelling scale and the default
/// country. These are the first choices made for a new, empty layout; the theme gates which
/// countries are offered.
/// </summary>
/// <remarks>
/// The scale and country are stored by their stable <c>Id</c> reference rather than as whole
/// records, so a saved layout does not embed a copy of the reference data. Resolve them through
/// <c>ScalesService.ById</c> and <c>CountriesService.ById</c> in <c>Importers.Services</c>.
/// </remarks>
public sealed class IdentitySettings
{
    /// <summary>The operating theme of the layout. Default is <see cref="Theme.European"/>.</summary>
    public Theme Theme { get; set; } = Theme.European;

    /// <summary>The <see cref="Scale.Id"/> of the layout's modelling scale. Default is <c>3</c> (H0).</summary>
    public int ScaleId { get; set; } = 3;

    /// <summary>
    /// The <see cref="Country.Id"/> of the layout's default country, used as the default language
    /// and culture for layout content. Default is <c>1</c> (Sweden, sv-SE).
    /// </summary>
    public int DefaultCountryId { get; set; } = 1;
}
