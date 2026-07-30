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
/// Provides extension methods for rendering a <see cref="Sessions"/> pattern as weekdays.
/// </summary>
/// <remarks>
/// The session bits are positional: bit 0 is the first day of the operating week, bit 1 the second, and
/// so on. Which calendar weekday each position names is decided by the layout's start day, so the same
/// pattern reads as <c>Mo, We, Fr, Su</c> when the week starts on Monday and <c>Su, Tu, Th, Sa</c> when it
/// starts on Sunday. Days are always listed in operating-week order (start day first).
/// </remarks>
public static class DaysExtensions
{
    private static readonly ResourceManager ResourceManager = new(typeof(Resources.Days));

    extension(Sessions sessions)
    {
        /// <summary>
        /// Gets the active weekdays, mapping the first-week session bits so that bit 0 is
        /// <paramref name="startDay"/>, bit 1 the next day, and so on, in operating-week order.
        /// </summary>
        public Days[] ActiveDays(DayOfWeek startDay) =>
            [.. sessions.DayOffsets.Select(offset => WeekdayAt(startDay, offset))];

        /// <summary>
        /// Gets the number of active days (session bits within the operating week). Independent of the
        /// start day, so it is safe to use for counting without knowing which weekday each bit names.
        /// </summary>
        public int DayCount => sessions.DayOffsets.Length;

        /// <summary>
        /// Gets the active days as full day names translated in the current UI culture and ordered from
        /// <paramref name="startDay"/>. No active days yields the translated "no operating days" text; all
        /// seven yields the translated "daily" text.
        /// </summary>
        public string DaysFull(DayOfWeek startDay) => DaysText(sessions, startDay, useShort: false);

        /// <summary>
        /// Gets the active days as short day names translated in the current UI culture and ordered from
        /// <paramref name="startDay"/>. No active days yields the short "none" text; all seven yields the
        /// short "daily" text.
        /// </summary>
        public string DaysShort(DayOfWeek startDay) => DaysText(sessions, startDay, useShort: true);

        /// <summary>
        /// Gets a resource key (or comma/hyphen-joined keys) for the active days as full names, mapped from
        /// <paramref name="startDay"/>. Consumers that translate with a non-default culture use this instead
        /// of <c>DaysFull</c>.
        /// </summary>
        public string FullDayNamesResourceKey(DayOfWeek startDay) => DayNamesResourceKey(sessions, startDay, useShort: false);

        /// <summary>
        /// Gets a resource key (or comma/hyphen-joined keys) for the active days as short names, mapped from
        /// <paramref name="startDay"/>. Consumers that translate with a non-default culture use this instead
        /// of <c>DaysShort</c>.
        /// </summary>
        public string ShortDayNamesResourceKey(DayOfWeek startDay) => DayNamesResourceKey(sessions, startDay, useShort: true);

        /// <summary>The 0-based operating-week positions (0–6) of the active first-week session bits, ascending.</summary>
        internal int[] DayOffsets =>
            [.. Enumerable.Range(0, 7).Where(offset => (sessions.Flags & (1 << offset)) != 0)];
    }

    /// <summary>
    /// The translated name of the day at an operating-week offset (0 = <paramref name="startDay"/>).
    /// Shared with <c>SessionsFormatting</c> so both day renderings name a day the same way.
    /// </summary>
    internal static string DayNameAt(DayOfWeek startDay, int offset, bool useShort) =>
        DayName(WeekdayAt(startDay, offset), useShort);

    /// <summary>The translated text for a resource key in the day resources.</summary>
    internal static string DayResource(string key) => Translate(key);

    /// <summary>Maps an operating-week offset (0 = <paramref name="startDay"/>) to a calendar weekday flag.</summary>
    private static Days WeekdayAt(DayOfWeek startDay, int offset)
    {
        var dayOfWeek = (DayOfWeek)(((int)startDay + offset) % 7);
        return dayOfWeek == DayOfWeek.Sunday ? Days.Sunday : (Days)(1 << ((int)dayOfWeek - 1));
    }

    private static string Translate(string key) => ResourceManager.GetString(key) ?? key;

    private static string DayName(Days day, bool useShort) =>
        Translate(useShort ? day + "Short" : day.ToString());

    private static string DaysText(Sessions sessions, DayOfWeek startDay, bool useShort)
    {
        var offsets = sessions.DayOffsets;
        if (offsets.Length == 0) return Translate(useShort ? "NoneShort" : nameof(Days.None));
        if (offsets.Length == 7) return Translate(useShort ? "DailyShort" : "Daily");
        var days = offsets.Select(offset => WeekdayAt(startDay, offset)).ToArray();
        if (!AreConsecutive(offsets))
            return string.Join(", ", days.Select(d => DayName(d, useShort)));
        var (first, last) = (DayName(days[0], useShort), DayName(days[^1], useShort));
        // Short ranges use a hyphen (Mo-Fr); long ranges spell out the localised connector (Monday to Friday).
        return useShort ? $"{first}-{last}" : $"{first} {Translate("DayRangeConnector")} {last}";
    }

    private static string DayNamesResourceKey(Sessions sessions, DayOfWeek startDay, bool useShort)
    {
        var offsets = sessions.DayOffsets;
        var suffix = useShort ? "Short" : string.Empty;
        return offsets.Length switch
        {
            0 => nameof(Days.None),
            7 => "Daily" + suffix,
            _ when AreConsecutive(offsets) => $"{WeekdayAt(startDay, offsets[0])}{suffix}-{WeekdayAt(startDay, offsets[^1])}{suffix}",
            _ => string.Join(",", offsets.Select(offset => WeekdayAt(startDay, offset) + suffix)),
        };
    }

    /// <summary>True when the operating-week offsets form an unbroken run of two or more.</summary>
    private static bool AreConsecutive(int[] offsets)
    {
        if (offsets.Length < 2) return false;
        for (var i = 1; i < offsets.Length; i++)
            if (offsets[i] != offsets[i - 1] + 1) return false;
        return true;
    }
}
