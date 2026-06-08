using Tellurian.Localization;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>
/// Service for localization support
/// </summary>
public class LanguageService
{
    /// <summary>
    /// All languages fully supported in the application (gui, reports, dynamic content)
    /// </summary>
    public static IEnumerable<Language> SupportedLanguages => [
        new Language("en", true) { CultureCode = "GB" },
        new Language("de", true) {CapitalizesNouns = true},
        new Language("da", true),
        new Language("nb", true) { CultureCode = "NO"},
        new Language("sv", true) { CultureCode = "SE"},
        ];
}
