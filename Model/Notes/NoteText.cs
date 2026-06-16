using System.Globalization;
using Tellurian.Localization.Implementatioms;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Resolves localised generated-note text from the <see cref="Resources.Notes"/> resx resources for the
/// current UI culture, using the synchronous resx lookup in Tellurian.Localization.
/// </summary>
internal static class NoteText
{
    private static readonly ResxResourceProvider Provider = new(typeof(Resources.Notes));

    /// <summary>
    /// Returns the localised note text for <paramref name="key"/>, formatted with
    /// <paramref name="args"/> in the current UI culture; falls back to the key when no translation exists.
    /// </summary>
    public static string Format(string key, params object[] args) =>
        string.Format(CultureInfo.CurrentUICulture, Provider.GetTranslation(key).Text, args);
}
