namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// Settings governing time calculation: fast-clock speed, the speed mapping curve,
/// and the layout-wide default station operational times.
/// </summary>
public sealed class TimeAndSpeedSettings
{
    /// <summary>Expected fast-clock speed as an integer multiplier of real time. Default is 5.</summary>
    public int FastClockSpeed { get; set; } = 5;

    /// <summary>Slow speed mapping point (e.g. yard movements). Default is 60 km/h to 0.15 m/s.</summary>
    public SpeedPoint Slow { get; set; } = new(60, 0.15);

    /// <summary>Normal speed mapping point (regular services). Default is 100 km/h to 0.25 m/s.</summary>
    public SpeedPoint Normal { get; set; } = new(100, 0.25);

    /// <summary>High speed mapping point (express trains). Default is 200 km/h to 0.35 m/s.</summary>
    public SpeedPoint High { get; set; } = new(200, 0.35);

    /// <summary>Layout-wide default station operational times. Individual stations may override these.</summary>
    public StationTimings StationTimings { get; set; } = StationTimings.LayoutDefaults;
}
