using System.Resources;

namespace Tellurian.Trains.Schedules.Planning.App.Translations.Resources;

/// <summary>
/// Typed accessor for the Strings.resx resource file.
/// Used by <see cref="ResourceManager"/> to locate the embedded resource
/// and by <c>Tellurian.Localization</c> for type-based provider registration.
/// </summary>
public static class Strings
{
    private static readonly ResourceManager ResourceManager =
        new(typeof(Strings).FullName!, typeof(Strings).Assembly);

    /// <summary>
    /// Gets the translated string for the given <paramref name="key"/>
    /// using <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>.
    /// </summary>
    public static string Get(string key) =>
        ResourceManager.GetString(key, System.Globalization.CultureInfo.CurrentUICulture) ?? key;
}
