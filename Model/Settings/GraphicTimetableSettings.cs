namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// User preferences controlling how the graphical timetable is drawn, for both on-screen viewing and printing.
/// </summary>
public sealed class GraphicTimetableSettings
{
    /// <summary>Orientation of the time axis. Default is <see cref="TimeAxisOrientation.Horizontal"/>.</summary>
    public TimeAxisOrientation Orientation { get; set; } = TimeAxisOrientation.Horizontal;

    /// <summary>Whether arrival minute labels are shown at station calls. Default is <c>false</c>.</summary>
    public bool ShowArrivalMinutes { get; set; }

    /// <summary>Whether departure minute labels are shown at station calls. Default is <c>true</c>.</summary>
    public bool ShowDepartureMinutes { get; set; } = true;

    /// <summary>Pixels per minute along the time axis. Default is 2.</summary>
    public int MinuteSpacing { get; set; } = 2;

    /// <summary>
    /// Pixels per kilometre along the distance axis. This is the scale that sizes the graph's
    /// distance extent (its height for a horizontal time axis); stations are spaced in proportion
    /// to their real <c>TrackStretch.Distance</c>. Default is 3.
    /// </summary>
    public int KilometerSpacing { get; set; } = 3;

    /// <summary>
    /// Minimum pixel spacing between stations on the distance axis, applied when the distance-based
    /// spacing would be smaller. Ensures labels on train lines between close stations have room.
    /// Default is 100.
    /// </summary>
    public int StationSpacing { get; set; } = 100;

    /// <summary>Pixel spacing between individual tracks at a station. Default is 8.</summary>
    public int TrackSpacing { get; set; } = 8;

    /// <summary>Whether the train category prefix is shown before the train number. Default is <c>false</c>.</summary>
    public bool ShowTrainCategory { get; set; }

    /// <summary>Whether the operating company signature is shown before the train number. Default is <c>false</c>.</summary>
    public bool ShowCompany { get; set; }

    /// <summary>Whether the sessions/days suffix is hidden after the train number. Default is <c>false</c>.</summary>
    public bool HideSessionsOrDays { get; set; }

    /// <summary>Font size in points of the train identity labels drawn along the train lines. Also feeds the
    /// label-thinning overlap estimate, so a larger size drops more crossing labels. Default is 8.</summary>
    public int TrainLabelFontSize { get; set; } = 8;
}
