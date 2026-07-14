using System.Collections;
using System.Numerics;
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
        /// Determines whether the given 1-based session/day number is active. For day patterns the number
        /// is the operating-week position (1 = the layout's start day); the mirrored upper bits are ignored.
        /// </summary>
        /// <param name="number">The session/day number (1-14).</param>
        internal bool Includes(int number) =>
            number is >= 1 and <= 14 && (sessions.Flags & (1 << (number - 1))) != 0;

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
        /// Gets the sessions within the layout's operating period (defined by <paramref name="useDays"/>
        /// and <paramref name="maxSessions"/>) that these sessions do <em>not</em> cover — i.e. the free
        /// sessions/days still available. Bits outside the period are ignored.
        /// </summary>
        public Sessions ComplementWithin(bool useDays, int maxSessions)
        {
            var free = FreeBits(sessions, useDays, maxSessions);
            // Days are stored with the in-week bits mirrored into the upper session bits plus the days
            // marker, so a day pattern round-trips and renders as weekdays.
            var flags = useDays ? (ushort)(free | (free << 7) | DaysMarker) : free;
            return new Sessions { Flags = flags };
        }

        /// <summary>
        /// Determines whether these sessions cover every session/day of the layout's operating period
        /// (defined by <paramref name="useDays"/> and <paramref name="maxSessions"/>), i.e. nothing is free.
        /// </summary>
        public bool CoversAllWithin(bool useDays, int maxSessions) => FreeBits(sessions, useDays, maxSessions) == 0;

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
        /// A single integer that orders session/day patterns for display. Patterns starting earlier in the
        /// operating period sort first, and among those starting on the same session/day the one covering
        /// more sessions/days sorts first (so a schedule running every session leads its variants). An
        /// empty (<c>None</c>) pattern sorts last. The value compares meaningfully within one kind — a
        /// layout is either day- or session-based throughout — and day patterns compare consistently
        /// however they were built, because a day's mirrored upper bit scales in step with its in-week
        /// bit. Keeping the order in one place makes it easy to refine later.
        /// </summary>
        public int SortOrder
        {
            get
            {
                // Mask off the two marker bits (on-demand, days); the low fourteen bits — a session
                // pattern's sessions, or a day pattern's in-week days mirrored into the upper seven — drive
                // the order. Day patterns therefore rank identically whether or not the days marker is set.
                var bits = (ushort)(sessions.Flags & 0b00_1111111_1111111);
                if (bits == 0) return int.MaxValue;
                var first = BitOperations.TrailingZeroCount(bits);
                var count = BitOperations.PopCount(bits);
                // first * 16 keeps each start bucket clear of the next (count is at most 14); within a
                // bucket, (14 - count) puts the broader pattern first.
                return first * 16 + (14 - count);
            }
        }


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


    // The days marker bit (bit 15); set on day-based patterns so they render as weekdays.
    private const ushort DaysMarker = 0b10_0000000_0000000;

    // Builds a Sessions from operating-period session/day numbers (1-based). For day patterns the in-week
    // bits are mirrored into the upper session bits and the days marker is set — exactly as ComplementWithin
    // does — so the value renders as weekdays; for session patterns the numbers are simply the low bits.
    internal static Sessions FromPeriodNumbers(IEnumerable<int> numbers, bool useDays)
    {
        ushort low = 0;
        foreach (var number in numbers)
            if (number is >= 1 and <= 14) low |= (ushort)(1 << (number - 1));
        var flags = useDays ? (ushort)(low | (low << 7) | DaysMarker) : low;
        return new Sessions { Flags = flags };
    }

    // The in-period session/day bits these sessions leave free: within the operating period (7 day bits
    // when useDays, else up to 14 session bits), the bits of the period that are not set.
    private static ushort FreeBits(Sessions sessions, bool useDays, int maxSessions)
    {
        var n = Math.Clamp(maxSessions, 1, useDays ? 7 : 14);
        var full = (ushort)((1 << n) - 1);
        var low = useDays ? (ushort)0b0000000_1111111 : (ushort)0b00_1111111_1111111;
        var assigned = (ushort)(sessions.Flags & low & full);
        return (ushort)(full & ~assigned);
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
