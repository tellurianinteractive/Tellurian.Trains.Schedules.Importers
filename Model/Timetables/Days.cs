using System.Collections;
using System.Globalization;
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

        /// <summary>
        /// Gets the active days ordered for display according to the current culture's first day of
        /// week, which is treated as either Sunday or Monday. The stored bit pattern is unchanged;
        /// only the print order of the days differs.
        /// </summary>
        public Days[] OrderedDays => [.. sessions.Days.OrderBy(DisplayOrder)];

        /// <summary>
        /// Gets the active days as full day names translated in the current UI culture and ordered by
        /// the current culture's first day of week. No active days yields the translated "no operating
        /// days" text; all seven yields the translated "daily" text.
        /// </summary>
        public string DaysFull => DaysText(sessions, useShort: false);

        /// <summary>
        /// Gets the active days as short day names translated in the current UI culture and ordered by
        /// the current culture's first day of week. No active days yields the short "none" text; all
        /// seven yields the short "daily" text.
        /// </summary>
        public string DaysShort => DaysText(sessions, useShort: true);
    }

    extension(Days days)
    {
        internal int DayNumber =>
            _MondayToSunday.Single(kv => kv.Value == days).Key;

        internal string Translated =>
            ResourceManager.GetString(days.ToString()) ?? days.ToString();

    }

    /// <summary>
    /// True when the current culture starts its week on Sunday; otherwise the week starts on Monday.
    /// Only these two options are recognised for ordering days.
    /// </summary>
    private static bool SundayFirst =>
        CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Sunday;

    /// <summary>Gets the 1-based position of a day within the week as ordered for the current culture.</summary>
    private static int DisplayOrder(Days day) =>
        (SundayFirst ? _SundayToSaturday : _MondayToSunday).Single(kv => kv.Value == day).Key;

    private static string Translate(string key) =>
        ResourceManager.GetString(key) ?? key;

    private static string DayName(Days day, bool useShort) =>
        Translate(useShort ? day + "Short" : day.ToString());

    /// <summary>
    /// True when the days, taken in current-culture display order, form an unbroken run of two or more
    /// (e.g. Monday–Friday). Consecutiveness is judged in display order, so the same set can be a range
    /// when the week starts on one day but not the other.
    /// </summary>
    private static bool IsConsecutiveInDisplayOrder(Days[] orderedDays)
    {
        if (orderedDays.Length < 2) return false;
        for (var i = 1; i < orderedDays.Length; i++)
            if (DisplayOrder(orderedDays[i]) != DisplayOrder(orderedDays[i - 1]) + 1) return false;
        return true;
    }

    private static string DaysText(Sessions sessions, bool useShort)
    {
        var days = sessions.Days;
        if (days.Length == 0) return Translate(useShort ? "NoneShort" : nameof(Days.None));
        if (days.Length == 7) return Translate(useShort ? "DailyShort" : "Daily");
        var ordered = sessions.OrderedDays;
        if (!IsConsecutiveInDisplayOrder(ordered))
            return string.Join(", ", ordered.Select(d => DayName(d, useShort)));
        var (first, last) = (DayName(ordered[0], useShort), DayName(ordered[^1], useShort));
        // Short ranges use a hyphen (Mo-Fr); long ranges spell out the localised connector (Monday to Friday).
        return useShort ? $"{first}-{last}" : $"{first} {Translate("DayRangeConnector")} {last}";
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
