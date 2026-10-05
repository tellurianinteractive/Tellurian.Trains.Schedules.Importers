using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Tellurian.Trains.Schedules.Planning.App.Translations;

/// <summary>
/// Makes the translations of every supported language available in the browser, not only those of the
/// user-interface language.
/// </summary>
/// <remarks>
/// Blazor WebAssembly downloads the resource (satellite) assemblies of the startup culture alone, so a
/// lookup in any other language silently falls back to English. Reports need the others: they are printed
/// in the layout's default language, and item by item in local languages, whatever language the user
/// works in. The runtime's own loader is called for the remaining languages — the same call Blazor makes
/// for the startup culture — before the application starts.
/// </remarks>
public static partial class LanguageResources
{
    /// <summary>
    /// Loads the resource assemblies of every supported language other than the current user-interface
    /// language, which Blazor loads itself. Does nothing outside the browser, where every resource
    /// assembly is found on disk.
    /// </summary>
    public static async Task LoadAllSupportedLanguagesAsync()
    {
        if (!OperatingSystem.IsBrowser()) return;
        var current = CultureInfo.DefaultThreadCurrentUICulture?.TwoLetterISOLanguageName;
        string[] others = [.. LanguageService.SupportedLanguages
            .Select(language => language.TwoLetterCode)
            .Where(code => !code.Equals(current, StringComparison.OrdinalIgnoreCase))];
        if (others.Length > 0) await LoadSatelliteAssembliesAsync(others);
    }

    [SupportedOSPlatform("browser")]
    [JSImport("INTERNAL.loadSatelliteAssemblies")]
    private static partial Task LoadSatelliteAssembliesAsync(string[] cultures);
}
