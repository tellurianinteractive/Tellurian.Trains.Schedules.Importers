namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

public record GraphSettings
{
    public static GraphSettings Default => new()
    {
        AxisDirection = TimeAxisDirection.Horisontal,
        DefaultStartTime = TimeSpan.FromHours(6),
        DefaultEndTime = TimeSpan.FromHours(20),
        TimeAxisSpacing = new(30, 30),
        KilometerAxisSpacing = new(100, 60),
        EndMargin = 20,
        TrackSpacing = 8,
        MinStationSpacing = 40,
        MinuteSpacing = 2,
        KilometerSpacing = 10,
        ShowArrivalMinutes = false,
        ShowDepartureMinutes = true,
    };

    public TimeAxisDirection AxisDirection { get; set; }
    public TimeSpan DefaultStartTime { get; set; }
    public TimeSpan DefaultEndTime { get; set; }

    /// <summary>Optional fast-clock break that splits the time axis into a first half
    /// (<see cref="DefaultStartTime"/>–<c>BreakTime</c>) and a last half (<c>BreakTime</c>–<see cref="DefaultEndTime"/>).
    /// <c>null</c> means no break, so only the whole graph can be shown. See <see cref="GraphHalf"/>.</summary>
    public TimeSpan? BreakTime { get; set; }
    public Offset TimeAxisSpacing { get; set; }
    public Offset KilometerAxisSpacing { get; set; }
    public int EndMargin { get; set; }
    public int TrackSpacing { get; set; }
    public int MinStationSpacing { get; set; }
    public int MinuteSpacing { get; set; }
    public int KilometerSpacing { get; set; }
    public bool ShowArrivalMinutes { get; set; }
    public bool ShowDepartureMinutes { get; set; }
    public bool ShowCompany { get; set; }
    public bool ShowTrainCategory { get; set; }
}

public enum TimeAxisDirection
{
    Horisontal,
    Vertical
}

/// <summary>Which part of the time axis a graphical schedule renders. When a break time is set,
/// the axis can be limited to the first half (start–break) or the last half (break–end); useful
/// on smaller screens, especially with a vertical time axis. <see cref="Whole"/> shows the full axis.</summary>
public enum GraphHalf
{
    Whole,
    First,
    Last
}
