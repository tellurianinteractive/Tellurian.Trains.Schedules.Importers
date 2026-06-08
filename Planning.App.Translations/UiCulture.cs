using System.Globalization;
using Tellurian.Localization;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>
/// Persistence and resolution of the selected user-interface culture, over the languages
/// defined by <see cref="LanguageService.SupportedLanguages"/>. The chosen culture is stored
/// in browser localStorage and applied on startup; this is the app's UI culture, distinct from
/// a schedule's content language.
/// </summary>
public static class UiCulture
{
    /// <summary>localStorage key holding the selected UI culture (e.g. <c>sv-SE</c>).</summary>
    public const string StorageKey = "planning.ui.culture";

    /// <summary>
    /// The supported language matching <paramref name="storedCulture"/> (by full culture name,
    /// then by language code), or the first (fallback) supported language.
    /// </summary>
    public static Language Resolve(string? storedCulture)
    {
        var languages = LanguageService.SupportedLanguages.ToList();
        return languages.FirstOrDefault(l => string.Equals(l.CultureInfo.Name, storedCulture, StringComparison.OrdinalIgnoreCase))
            ?? languages.FirstOrDefault(l => storedCulture is not null && storedCulture.StartsWith(l.TwoLetterCode, StringComparison.OrdinalIgnoreCase))
            ?? languages[0];
    }
}
