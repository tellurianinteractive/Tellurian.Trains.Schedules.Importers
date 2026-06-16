using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Specifies the severity level of a message.
/// </summary>
public enum Severity
{
    /// <summary>
    /// No severity.
    /// </summary>
    None = 0,

    /// <summary>
    /// Informational message.
    /// </summary>
    Information = 1,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error message.
    /// </summary>
    Error = 3,

    /// <summary>
    /// System message.
    /// </summary>
    System = 4
}

internal static class SeverityExtensions
{
    public static string ToLanguageString(this Severity me, CultureInfo culture) =>
        Strings.ResourceManager.GetString(me.ToString(), culture) ?? string.Empty;
}
