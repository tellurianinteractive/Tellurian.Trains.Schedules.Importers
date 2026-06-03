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
    public Offset TimeAxisSpacing { get; set; }
    public Offset KilometerAxisSpacing { get; set; }
    public int EndMargin { get; set; }
    public int TrackSpacing { get; set; }
    public int MinStationSpacing { get; set; }
    public int MinuteSpacing { get; set; }
    public int KilometerSpacing { get; set; }
    public bool ShowArrivalMinutes { get; set; }
    public bool ShowDepartureMinutes { get; set; }
}

public enum TimeAxisDirection
{
    Horisontal,
    Vertical
}
