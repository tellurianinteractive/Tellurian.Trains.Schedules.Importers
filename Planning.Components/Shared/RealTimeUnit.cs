using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Planning.App.Translations;

namespace Tellurian.Trains.Schedules.Planning.Components.Shared;

/// <summary>
/// The unit written after a real duration that is entered as minutes or seconds depending on its size (see
/// <see cref="StationTimings.RealDurationOf"/>).
/// </summary>
public static class RealTimeUnit
{
    /// <summary>The unit the value is in: real minutes or real seconds; empty where there is no value.</summary>
    /// <param name="translator">Translates the unit.</param>
    /// <param name="value">The value as entered.</param>
    public static string Of(Translator translator, int? value) => value switch
    {
        null => string.Empty,
        { } v => translator(StationTimings.IsSeconds(v) ? "RealSecondsUnit" : "RealMinutesUnit"),
    };

    /// <summary>The value with its unit after it, as in "90 real sec"; a dash where there is no value.</summary>
    /// <param name="translator">Translates the unit.</param>
    /// <param name="value">The value as entered.</param>
    /// <param name="culture">The culture the number is written in.</param>
    public static string Text(Translator translator, int? value, System.Globalization.CultureInfo culture) =>
        value is { } v ? $"{v.ToString(culture)} {Of(translator, v)}" : "—";
}
