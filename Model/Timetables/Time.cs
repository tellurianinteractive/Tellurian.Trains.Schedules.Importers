using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents a time of day for railway scheduling purposes.
/// </summary>
[JsonConverter(typeof(TimeJsonConverter))]
public readonly struct Time : IComparable<Time?>, IEquatable<Time>
{
    /// <summary>
    /// Gets a time value representing midnight (00:00).
    /// </summary>
    public static Time Zero => new(TimeSpan.Zero);

    /// <summary>
    /// Returns the earlier of two times.
    /// </summary>
    /// <param name="a">The first time.</param>
    /// <param name="b">The second time.</param>
    /// <returns>The earlier time.</returns>
    public static Time Min(Time a, Time b) => a < b ? a : b;

    /// <summary>
    /// Returns the later of two times.
    /// </summary>
    /// <param name="a">The first time.</param>
    /// <param name="b">The second time.</param>
    /// <returns>The later time.</returns>
    public static Time Max(Time a, Time b) => a > b ? a : b;

    /// <summary>
    /// Parses a time string in HH:mm format.
    /// </summary>
    /// <param name="time">The time string to parse.</param>
    /// <returns>The parsed time.</returns>
    /// <exception cref="ArgumentException">Thrown when the string is not a valid time.</exception>
    public static Time FromString(string time)
    {
        if (TimeSpan.TryParse(time, out var value)) return new(value);
        throw new ArgumentException("Not a valid time.", time);
    }

    /// <summary>
    /// Creates a time from a TimeSpan value.
    /// </summary>
    /// <param name="value">The TimeSpan value.</param>
    /// <returns>The time.</returns>
    public static Time FromTimeSpan(TimeSpan value) => new(value);

    /// <summary>
    /// Creates a time from hours and minutes.
    /// </summary>
    /// <param name="hours">The hours (0-23).</param>
    /// <param name="minutes">The minutes (0-59).</param>
    /// <returns>The time.</returns>
    public static Time FromHourAndMinute(int hours, int minutes) => FromDayHourMinute(0, hours, minutes);

    /// <summary>
    /// Creates a time from days, hours, and minutes.
    /// </summary>
    /// <param name="days">The number of days.</param>
    /// <param name="hours">The hours (0-23).</param>
    /// <param name="minutes">The minutes (0-59).</param>
    /// <returns>The time.</returns>
    public static Time FromDayHourMinute(int days, int hours, int minutes) => new(new TimeSpan(days, hours, minutes, 0));

    /// <summary>
    /// Creates a time from a string representing days as a decimal number.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <returns>The time.</returns>
    public static Time FromDays(string value) => value is null ? FromDays(0) : FromDays(double.Parse(value.Replace(",", ".", StringComparison.OrdinalIgnoreCase), NumberStyles.Float, CultureInfo.InvariantCulture));

    /// <summary>
    /// Creates a time from days as a decimal number.
    /// </summary>
    /// <param name="days">The number of days as a decimal.</param>
    /// <returns>The time.</returns>
    public static Time FromDays(double days)
    {
        var t = TimeSpan.FromDays(days).Add(TimeSpan.FromMilliseconds(1)); // Vi lägger till en millisekund för att kompensera avrundningsfel i ODS-filen.
        return new Time(new TimeSpan(t.Hours, t.Minutes, 0));
    }

    private Time(TimeSpan value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the underlying TimeSpan value.
    /// </summary>
    public TimeSpan Value { get; }

    /// <summary>
    /// Gets the total number of minutes.
    /// </summary>
    public double TotalMinutes => Value.TotalMinutes;

    /// <inheritdoc/>
    public int CompareTo(Time? other) => other?.Value is null ? int.MinValue : (int)(Value - other.Value.Value).TotalMinutes;

    /// <summary>Determines whether two times are not equal.</summary>
    public static bool operator !=(Time? time1, Time? time2) => time1?.CompareTo(time2) != 0;

    /// <summary>Determines whether two times are equal.</summary>
    public static bool operator ==(Time? time1, Time? time2) => time1?.CompareTo(time2) == 0;

    /// <summary>Determines whether the first time is earlier than or equal to the second.</summary>
    public static bool operator <=(Time? time1, Time? time2) => time1?.CompareTo(time2) <= 0;

    /// <summary>Determines whether the first time is later than or equal to the second.</summary>
    public static bool operator >=(Time? time1, Time? time2) => time1?.CompareTo(time2) >= 0;

    /// <summary>Determines whether the first time is earlier than the second.</summary>
    public static bool operator <(Time? time1, Time? time2) => time1?.CompareTo(time2) < 0;

    /// <summary>Determines whether the first time is later than the second.</summary>
    public static bool operator >(Time? time1, Time? time2) => time1?.CompareTo(time2) > 0;

    /// <inheritdoc/>
    public bool Equals(Time other) => other.Value == Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Time other && Value.Equals(other.Value);

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => string.Format(CultureInfo.InvariantCulture, "{0:hh\\:mm}", Value);
}

/// <summary>
/// Serialises <see cref="Time"/> as an invariant <see cref="TimeSpan"/> string (e.g. "07:24:00",
/// or "1.02:30:00" for times past midnight), preserving the full value including days.
/// </summary>
internal sealed class TimeJsonConverter : JsonConverter<Time>
{
    /// <inheritdoc/>
    public override Time Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var text = reader.GetString();
        return string.IsNullOrEmpty(text)
            ? Time.Zero
            : Time.FromTimeSpan(TimeSpan.Parse(text, CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Time value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value.ToString("c", CultureInfo.InvariantCulture));
}

/// <summary>
/// Provides extension methods for <see cref="Time"/>.
/// </summary>
public static class TimeExtensions
{
    /// <summary>
    /// Formats the time as HH:mm.
    /// </summary>
    /// <param name="me">The time to format.</param>
    /// <returns>A formatted time string.</returns>
    public static string HHMM(this Time me) => string.Format(CultureInfo.InvariantCulture, "{0:hh\\:mm}", me.Value);

    /// <summary>
    /// Gets the hours component of the time.
    /// </summary>
    /// <param name="me">The time.</param>
    /// <returns>The hours component.</returns>
    public static int Hours(this Time me) => me.Value.Hours;

    /// <summary>
    /// Subtracts one time from another.
    /// </summary>
    /// <param name="time1">The first time.</param>
    /// <param name="time2">The time to subtract.</param>
    /// <returns>The difference as a time.</returns>
    public static Time Subtract(this Time time1, Time time2) => Time.FromTimeSpan(time1.Value - time2.Value);

    /// <summary>
    /// Adds the specified number of minutes to the time.
    /// </summary>
    /// <param name="me">The time.</param>
    /// <param name="minutes">The number of minutes to add.</param>
    /// <returns>The new time.</returns>
    public static Time AddMinutes(this Time me, int minutes) => Time.FromTimeSpan(me.Value.Add(TimeSpan.FromMinutes(minutes)));

    /// <summary>
    /// Adds the specified number of days to the time.
    /// </summary>
    /// <param name="me">The time.</param>
    /// <param name="days">The number of days to add.</param>
    /// <returns>The new time.</returns>
    public static Time AddDays(this Time me, int days) => Time.FromTimeSpan(me.Value.Add(TimeSpan.FromDays(days)));

    /// <summary>
    /// Parses a string that may contain decimal days or HH:mm format.
    /// </summary>
    /// <param name="value">The string value to parse.</param>
    /// <returns>The parsed time.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the value is null or whitespace.</exception>
    public static Time ParseDays(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(value));
        var parts = value.Split(':');
        if (parts.Length == 1)
        {
            return Time.FromDays(double.Parse(value.Replace(',', '.'), CultureInfo.InvariantCulture) + 0.0001); // Excel uses decimal days.
        }
        else
        {
            return Time.FromHourAndMinute(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture));
        }
    }
}
