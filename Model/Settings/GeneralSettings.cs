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
}
