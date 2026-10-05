namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// General, layout-wide settings: language, the session/day model, and the operating time window.
/// </summary>
public sealed class GeneralSettings
{
    /// <summary>When <c>true</c>, content presents operating days; when <c>false</c>, it presents sessions. Default is sessions.</summary>
    public bool UseDays { get; set; }

    /// <summary>The weekday of the first session when <see cref="UseDays"/> is enabled. Default is <see cref="DayOfWeek.Monday"/>.</summary>
    public DayOfWeek StartDay { get; set; } = DayOfWeek.Monday;

    /// <summary>
    /// The number of operating sessions/days in the layout's period (1–14). Session/day <em>texts</em> ignore
    /// any bits above this, so a value of 6 shows sessions <c>1-6</c> and days <c>Mo-Sa</c> (or <c>Su-Fr</c>
    /// when the week starts on Sunday). Only the display is affected — a train's stored sessions are never
    /// touched, so raising this again brings the hidden sessions back. Default is 14 (no capping).
    /// </summary>
    public int MaxSessions { get; set; } = 14;

    /// <summary>
    /// First day of the meeting this layout is planned for, or <c>null</c> when no meeting is booked.
    /// Shown as the validity span on printed reports; a layout without dates simply omits the line
    /// rather than printing a placeholder date.
    /// </summary>
    public DateOnly? ValidFrom { get; set; }

    /// <summary>
    /// Last day of the meeting this layout is planned for, or <c>null</c> when no meeting is booked.
    /// See <see cref="ValidFrom"/>.
    /// </summary>
    public DateOnly? ValidTo { get; set; }

    /// <summary>Fast-clock start hour of operation, used as the graphical timetable's time-axis start. Default is 06:00.</summary>
    public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(6);

    /// <summary>Fast-clock end hour of operation, used as the graphical timetable's time-axis end. Default is 20:00.</summary>
    public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(20);

    /// <summary>
    /// When <c>true</c>, the operating window spans the whole day: <see cref="StartTime"/> is pinned to
    /// 00:00 and <see cref="EndTime"/> to 23:59, so a timetable whose services run over midnight is
    /// graphed across the full day. The start/end time fields are disabled while this is set. Default is <c>false</c>.
    /// </summary>
    public bool RunsOverMidnight { get; set; }

    /// <summary>
    /// Optional fast-clock break that splits the graphical timetable into two halves when printing:
    /// the first half is <see cref="StartTime"/>–<see cref="BreakTime"/>, the second half is
    /// <see cref="BreakTime"/>–<see cref="EndTime"/>. <c>null</c> means no break.
    /// </summary>
    public TimeSpan? BreakTime { get; set; }

    /// <summary>
    /// When <c>true</c>, a train that starts or ends outside <see cref="StartTime"/>–<see cref="EndTime"/>
    /// widens the window to cover it instead of being rejected: creating, moving, cloning or retiming a
    /// train that would otherwise fall outside the plan's operating window extends <see cref="StartTime"/>
    /// and/or <see cref="EndTime"/> to include it, and the checks that would otherwise refuse the change
    /// are skipped. The train must still start within the calendar day (00:00–24:00); only the plan's own
    /// window is extended, not that day boundary. Has no effect while <see cref="RunsOverMidnight"/> is set,
    /// since the window is then already unrestricted. Default is <c>false</c>.
    /// </summary>
    public bool AllowPlanTimeExtend { get; set; }

    /// <summary>
    /// The number of loco drivers expected to be available during operation. The graphical timetable
    /// compares the drivers actually required minute by minute against this number, colouring each bar
    /// by how far the demand is above or below it. <c>0</c> means no expectation is set, and the
    /// demand bars are then not shown. Default is 0.
    /// </summary>
    public int ExpectedLocoDrivers { get; set; }

    /// <summary>
    /// When <c>true</c>, the meeting is worked with passenger tickets: a passenger travels on a ticket
    /// bought where the journey starts, and a station marked
    /// <see cref="Layouts.Station.IsPassengerInterchange"/> is then a passenger interchange, where the
    /// passengers going on by another train are handed over. Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// This is what makes an interchange an interchange: with no tickets in use there is nobody to hand
    /// over, so nothing is derived from the station marks meanwhile. The marks themselves are kept — a
    /// meeting may take the tickets up again — exactly as a lock key is kept while it is out of force.
    /// See <c>PassengerInterchangeExtensions</c>, which is what everything else asks.
    /// </remarks>
    public bool UsePassengerTickets { get; set; }

    /// <summary>
    /// When <c>true</c>, reports print each item in its own local language — a driver duty in its
    /// company's language, a turnus card in its vehicle's company's language, a station's dispatch list
    /// in its country's language — rather than in the layout's default language. An item without a
    /// local language the application can print in keeps the default language. Reports never use the
    /// user-interface language. Default is <c>false</c>. See <c>ReportCultureExtensions</c>.
    /// </summary>
    /// <remarks>Named before the setting was presented as "local languages"; renaming it would reset every saved plan's value.</remarks>
    public bool UseObjectLanguageInReports { get; set; }

    /// <summary>
    /// The font family printed reports are set in, stored as the family name on its own (for example
    /// <c>Georgia</c>). An empty value means the application's default font. Only reports use it — the
    /// rest of the user interface keeps its own font, so the choice never affects working on the plan.
    /// </summary>
    /// <remarks>
    /// A name rather than a font file, so the plan stays small and portable: opened on a machine where
    /// the font is installed it is used, and elsewhere the report falls back to a font of the same kind.
    /// </remarks>
    public string ReportFontFamily { get; set; } = "";
}

/// <summary>
/// 
/// </summary>
public static class GeneralSettingsExtensions
{
    extension(GeneralSettings settings)
    {
        /// <summary>
        /// Extracts <see cref="SessionsSettings"/> from <see cref="GeneralSettings"/> with default to use short weekday names.
        /// </summary>
        public SessionsSettings SessionSettings(bool useShortWeekdayNames = true) => new()
        {
            MaxNumberOfSessions = settings.MaxSessions,
            UseDaysInsteadOfSessionNumbers = settings.UseDays,
            SessionFirstWeekday = settings.StartDay,
            UseShortWeekdayNames = useShortWeekdayNames,
        };
    }
}
