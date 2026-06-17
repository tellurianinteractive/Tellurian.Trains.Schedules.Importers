using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Formats the strongly-typed generated-note resources (see <see cref="Resources.Notes"/>) with their
/// arguments. The resource string is looked up by the generated accessor using
/// <see cref="CultureInfo.CurrentUICulture"/>; this helper applies the composite formatting of the
/// arguments (numbers, dates) using <see cref="CultureInfo.CurrentCulture"/>, per the .NET convention
/// that UI culture selects the text and the formatting culture renders the values.
/// </summary>
internal static class NoteText
{
    /// <summary>
    /// Returns <paramref name="format"/> formatted with <paramref name="args"/> in the current culture.
    /// </summary>
    public static string Format(string format, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, format, args);
}
