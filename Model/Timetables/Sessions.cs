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


        internal bool IsConsequtiveSessions
        {

            get
            {
                var numbers = sessions.Numbers;
                if (numbers.Length == 0) return false;
                for (var i = 1; i < numbers.Length; i++)
                {
                    if (numbers[i] != numbers[i - 1] + 1) return false;
                }
                return true;
            }
        }

        /// <summary>
        /// If sessions is days, gets the DaysResourceKey, otherwise SessionNumbers.
        /// </summary>
        public string FullNameResourceKey => sessions.IsDays ? sessions.FullDayNamesResourceKey : sessions.SessionsNumbers;


        /// <summary>
        /// Gets a display string for the session numbers.
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
                    _ when sessions.IsConsequtiveSessions => $"{numbers.First()}-{numbers.Last()}",
                    _ => string.Join(" ", numbers.Select(n => n.ToString())),
                };
            }
        }

        internal bool IsConsequtiveDays
        {
            get
            {
                var days = sessions.Days;
                if (days.Length == 0) return false;
                for (var i = 1; i < days.Length; i++)
                {
                    if (days[i].DayNumber != days[i - 1].DayNumber + 1) return false;
                }
                return true;

            }
        }

        /// <summary>
        /// Gets a resource key for displaying the days.
        /// </summary>
        public string FullDayNamesResourceKey
        {
            get
            {
                var days = sessions.Days;
                return days.Length switch
                {
                    0 => nameof(Days.None),
                    7 => "Daily",
                    _ when sessions.IsConsequtiveDays => $"{days.First()}-{days.Last()}",
                    _ => string.Join(",", days.Select(s => s.ToString())),
                };
            }
        }
        /// <summary>
        /// Gets a resource key for displaying the days.
        /// </summary>
        public string ShortDayNamesResourceKey
        {
            get
            {
                var days = sessions.Days;
                return days.Length switch
                {
                    0 => nameof(Days.None),
                    7 => "DailyShort",
                    _ when sessions.IsConsequtiveDays => $"{days.First()}Short-{days.Last()}Short",
                    _ => string.Join(",", days.Select(s => s.ToString() + "Short")),
                };
            }
        }
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
