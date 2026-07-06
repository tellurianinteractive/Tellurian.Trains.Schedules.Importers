using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents a set of operating sessions during which a train or duty is active.
/// </summary>
/// <remarks>
/// Sessions are represented as a bit pattern where each bit indicates whether a specific session is active.
/// This supports modeling operating patterns like "odd sessions only", "every third session", etc.
/// </remarks>
[JsonConverter(typeof(SessionsJsonConverter))]
public readonly struct Sessions
{
    /// <summary>
    /// Initializes a new instance of <see cref="Sessions"/> from a set of days.
    /// </summary>
    /// <param name="days">The days to include.</param>
    internal Sessions(Days days) => Flags = (ushort)((ushort)days | ((ushort)days) << 7);

    /// <summary>
    /// Initializes a new instance of <see cref="Sessions"/> from session numbers.
    /// </summary>
    /// <param name="sessionNumbers">The session numbers (1-14) to include.</param>
    internal Sessions(params int[] sessionNumbers) => Flags = sessionNumbers.ToFlags;

    /// <summary>
    /// Initializes a new instance of <see cref="Sessions"/> from a bit pattern.
    /// </summary>
    /// <param name="bitPattern">The bit pattern representing active sessions.</param>
    internal Sessions(int bitPattern) => Flags = (ushort)bitPattern;

    /// <summary>
    /// Gets the bit flags representing the active sessions.
    /// </summary>
    internal ushort Flags { get; init; }

    /// <inheritdoc/>
    public override string ToString() => Flags.ToString("D6");
}

/// <summary>
/// Serialises <see cref="Sessions"/> as its numeric bit pattern, since the underlying
/// <see cref="Sessions.Flags"/> is internal and would otherwise be lost during JSON round-trips.
/// </summary>
internal sealed class SessionsJsonConverter : JsonConverter<Sessions>
{
    /// <inheritdoc/>
    public override Sessions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new() { Flags = reader.GetUInt16() };

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Sessions value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Flags);
}

/// <summary>
/// Provides extension methods for <see cref="Sessions"/>.
/// </summary>
public static class SessionsExtensions
{
    extension(Sessions sessions)
    {

        /// <summary>
        /// Creates sessions from a set of days.
        /// </summary>
        /// <param name="days">The days to include.</param>
        /// <returns>A new Sessions instance.</returns>
        public static Sessions FromDays(Days days) => new(days);

        /// <summary>
        /// Creates sessions from session numbers.
        /// </summary>
        /// <param name="sessionsNumbers">The session numbers to include.</param>
        /// <returns>A new Sessions instance.</returns>
        public static Sessions FromSessionNumbers(params int[] sessionsNumbers) => new(sessionsNumbers);

        /// <summary>
        /// Creates sessions from a bit pattern.
        /// </summary>
        /// <param name="bitPattern">The bit pattern.</param>
        /// <returns>A new Sessions instance.</returns>
        public static Sessions FromBitPattern(int bitPattern) => new(bitPattern);

        /// <summary>
        /// Gets sessions that are active for all sessions.
        /// </summary>
        public static Sessions All => FromBitPattern(CommonSessionPatterns.All);

        /// <summary>
        /// Returns the intersection of this sessions with another.
        /// </summary>
        /// <param name="other">The other sessions.</param>
        /// <returns>Sessions that are active in both.</returns>
        public Sessions And(Sessions other) => new() { Flags = sessions.Flags.And(other.Flags) };

        /// <summary>
        /// Returns the union of this sessions with another.
        /// </summary>
        /// <param name="other">The other sessions.</param>
        /// <returns>Sessions that are active in either.</returns>
        public Sessions Or(Sessions other) => new() { Flags = sessions.Flags.Or(other.Flags) };

        /// <summary>
        /// Determines whether these sessions overlap with another.
        /// </summary>
        /// <param name="other">The other sessions.</param>
        /// <returns><c>true</c> if there is any overlap; otherwise, <c>false</c>.</returns>
        public bool Overlaps(Sessions other) => sessions.And(other).Flags > 0;

        /// <summary>
        /// Returns a copy for display in which the session bits at or above <paramref name="maxSessions"/>
        /// are cleared, so a layout with a shorter operating period ignores the higher bits. The stored
        /// value is untouched — this only shapes the session/day texts, so raising the period again brings
        /// the hidden sessions back. A <paramref name="maxSessions"/> of 14 or more (or below 1) returns the
        /// value unchanged. The on-demand and days marker bits are preserved.
        /// </summary>
        /// <param name="maxSessions">The number of operating sessions/days to keep (1–14).</param>
        public Sessions Capped(int maxSessions)
        {
            if (maxSessions is < 1 or >= 14) return sessions;
            // Keep the low maxSessions session bits plus the two high marker bits (on-demand, days).
            var mask = (ushort)(((1 << maxSessions) - 1) | 0b11_0000000_0000000);
            return new() { Flags = (ushort)(sessions.Flags & mask) };
        }

        /// <summary>
        /// Returns a copy for display capped to the layout's operating period of <paramref name="maxSessions"/>
        /// sessions/days. For session texts (<paramref name="useDays"/> false) the higher session bits are
        /// dropped, as in <c>Capped</c>. For day texts only the first <paramref name="maxSessions"/> days of the
        /// operating week are kept — the session bits are positional, so a six-day week keeps bits 0–5 whatever
        /// weekday the week starts on. The stored value is never changed; the on-demand and days marker bits
        /// are preserved.
        /// </summary>
        /// <param name="useDays">Whether the value is displayed as weekdays rather than session numbers.</param>
        /// <param name="maxSessions">The number of operating sessions/days in the period (1–14).</param>
        public Sessions CappedForDisplay(bool useDays, int maxSessions)
        {
            if (!useDays) return sessions.Capped(maxSessions);
            if (maxSessions is < 1 or >= 7) return sessions;
            // Days occupy the low seven positions of the operating week; keep the first maxSessions of them.
            // The mirrored upper session bits do not affect day texts, so only the in-week day bits and the
            // marker bits are kept.
            var mask = (ushort)(((1 << maxSessions) - 1) | 0b11_0000000_0000000);
            return new() { Flags = (ushort)(sessions.Flags & mask) };
        }

        /// <summary>
        /// Gets a value indicating whether this is an on-demand train.
        /// </summary>
        public bool IsOnDemand => (sessions.Flags & CommonSessionPatterns.OnDemand) > 0;

        /// <summary>
        /// Gets "OnDemand" if this is an on-demand train, otherwise empty string.
        /// </summary>
        public string OnDemand => sessions.IsOnDemand ? "OnDemand" : string.Empty;



        /// <summary>
        /// Gets the session numbers that are active.
        /// </summary>
        public byte[] Numbers =>
            [.. new BitArray([sessions.Flags])
            .Cast<bool>()
            .Select((b, i) => (x: b, y: (byte)(i)))
            .Take(14) // Only 14 bits are for sessions.
            .Where(t => t.x)
            .Select(t => (byte)(t.y + 1))];

        internal bool IsDays => (sessions.Flags & 0b____10_0000000_0000000) > 0;

        /// <summary>
        /// True if only one session.
        /// </summary>
        public bool IsSingleSessionOrDay => sessions.Numbers.Length == 1;


        /// <summary>
        /// If sessions is days, gets the full day-names resource key (mapped from <paramref name="startDay"/>),
        /// otherwise the session numbers.
        /// </summary>
        public string FullNameResourceKey(DayOfWeek startDay) =>
            sessions.IsDays ? sessions.FullDayNamesResourceKey(startDay) : sessions.SessionsNumbers;


        /// <summary>
        /// Gets a display string for the session numbers, collapsing each run of consecutive numbers into
        /// a range and joining the runs with commas (e.g. <c>1-5,8-12</c>). A run of a single number is
        /// shown as that number.
        /// </summary>
        public string SessionsNumbers
        {
            get
            {
                var numbers = sessions.Numbers;
                return numbers.Length switch
                {
                    0 => "None",
                    14 => "All",
                    _ => FormatSessions(numbers),
                };
            }
        }
    }


    // Collapses ascending session numbers into comma-separated ranges, each sequence of two or more
    // consecutive numbers shown as "first-last" and a lone number as itself
    // (e.g. 1,2,3,4,5,8,9,10,11,12 -> "1-5,8-12").
    private static string FormatSessions(byte[] numbers)
    {
        var ranges = new List<string>();
        var start = 0;
        for (var i = 1; i <= numbers.Length; i++)
        {
            if (i < numbers.Length && numbers[i] == numbers[i - 1] + 1) continue;
            var (first, last) = (numbers[start], numbers[i - 1]);
            ranges.Add(first == last ? first.ToString() : $"{first}-{last}");
            start = i;
        }
        return string.Join(",", ranges);
    }

    extension(ushort flags)
    {
        internal ushort And(ushort other) => (ushort)(flags & other);
        internal ushort Or(ushort other) => (ushort)(flags | other);
    }

    extension(int[]? sessions)
    {
        internal ushort ToFlags
        {
            get
            {
                if (sessions is null) return 0;
                ushort flags = 0;
                for (var i = 0; i < sessions.Length; i++)
                {
                    flags |= (ushort)(1 << (sessions[i] - 1));
                }
                return flags;
            }
        }
    }
}
