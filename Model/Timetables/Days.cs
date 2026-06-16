using System.Collections;
using System.Resources;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Specifies days of the week for operating patterns.
/// </summary>
[Flags]
public enum Days
{
    /// <summary>No days.</summary>
    None = 0,
    /// <summary>Monday.</summary>
    Monday = 1,
    /// <summary>Tuesday.</summary>
    Tuesday = 1 << 1,
    /// <summary>Wednesday.</summary>
    Wednesday = 1 << 2,
    /// <summary>Thursday.</summary>
    Thursday = 1 << 3,
    /// <summary>Friday.</summary>
    Friday = 1 << 4,
    /// <summary>Saturday.</summary>
    Saturday = 1 << 5,
    /// <summary>Sunday.</summary>
    Sunday = 1 << 6,
}

/// <summary>
/// Provides extension methods for working with <see cref="Days"/>.
/// </summary>
public static class DaysExtensions
{
    private static readonly ResourceManager ResourceManager = new(typeof(Resources.Days));

    extension(Sessions sessions)
    {
        /// <summary>
        /// Gets the days of the week that are active in these sessions.
        /// </summary>
        public Days[] Days =>
    [.. new BitArray([sessions.Flags])
            .Cast<bool>()
            .Select((b, i) => (x: b, y: (byte)(i)))
            .Take(7) // Only 7 bits are for days.
            .Where(t => t.x)
            .Select(t => _MondayToSunday[t.y+1])];

    }

    extension(Days days)
    {
        internal int DayNumber =>
            _MondayToSunday.Single(kv => kv.Value == days).Key;

        internal string Translated =>
            ResourceManager.GetString(days.ToString()) ?? days.ToString();

    }

    private static readonly Dictionary<int, Days> _MondayToSunday = new()
    {
        {1, Days.Monday },
        {2, Days.Tuesday },
        {3, Days.Wednesday },
        {4, Days.Thursday },
        {5, Days.Friday },
        {6, Days.Saturday },
        {7, Days.Sunday },
    };
    private static readonly Dictionary<int, Days> _SundayToSaturday = new()
    {
        {1, Days.Sunday },
        {2, Days.Monday },
        {3, Days.Tuesday },
        {4, Days.Wednesday },
        {5, Days.Thursday },
        {6, Days.Friday },
        {7, Days.Saturday },
    };
}
