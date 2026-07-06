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

    /// <summary>Fast-clock start hour of operation, used as the graphical timetable's time-axis start. Default is 06:00.</summary>
    public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(6);

    /// <summary>Fast-clock end hour of operation, used as the graphical timetable's time-axis end. Default is 20:00.</summary>
    public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(20);

    /// <summary>
    /// Optional fast-clock break that splits the graphical timetable into two halves when printing:
    /// the first half is <see cref="StartTime"/>–<see cref="BreakTime"/>, the second half is
    /// <see cref="BreakTime"/>–<see cref="EndTime"/>. <c>null</c> means no break.
    /// </summary>
    public TimeSpan? BreakTime { get; set; }

    /// <summary>
    /// When <c>true</c>, reports render each item in the language derived from the item itself
    /// (for example, a driver duty in its operating company's language) rather than in the user's
    /// preferred interface language. The interface and any item without a derivable language keep
    /// using the user's language. Default is <c>false</c>.
    /// </summary>
    public bool UseObjectLanguageInReports { get; set; }
}
