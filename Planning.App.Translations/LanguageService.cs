using Tellurian.Localization;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>
/// Service for localization support
/// </summary>
public class LanguageService
{
    /// <summary>
    /// The language the markdown content files carrying no language suffix are written in.
    /// About.md is the English text; every other language carries its two-letter code, as About.sv.md.
    /// The markdown provider is given this so it does not ask the server for About.en.md, a file that
    /// by this convention never exists.
    /// </summary>
    public const string NeutralLanguage = "en";

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
