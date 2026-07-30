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

    /// <summary>
    /// Factor that converts a track stretch's stored distance (metres) into the kilometre figure
    /// shown in timetable reports and graphs. Default is 1, so displayed kilometres equal the
    /// stored metre value, matching distances entered before this factor existed.
    /// </summary>
    public double DistanceFactor { get; set; } = 1;

    /// <summary>
    /// Maps a scale speed (km/h) to a real model speed (m/s) using the piecewise-linear curve
    /// defined by the <see cref="Slow"/>, <see cref="Normal"/> and <see cref="High"/> points.
    /// Speeds below the slowest or above the fastest point are clamped to that point's real speed.
    /// </summary>
    /// <param name="scaleSpeedKmh">The scale speed in km/h to convert.</param>
    /// <returns>The interpolated real model speed in metres per second.</returns>
    public double RealSpeedMetersPerSecond(int scaleSpeedKmh)
    {
        var points = new[] { Slow, Normal, High }.OrderBy(p => p.ScaleSpeedKmh).ToArray();
        if (scaleSpeedKmh <= points[0].ScaleSpeedKmh) return points[0].RealSpeedMetersPerSecond;
        if (scaleSpeedKmh >= points[^1].ScaleSpeedKmh) return points[^1].RealSpeedMetersPerSecond;
        for (var i = 0; i < points.Length - 1; i++)
        {
            var low = points[i];
            var high = points[i + 1];
            if (scaleSpeedKmh > high.ScaleSpeedKmh) continue;
            var span = high.ScaleSpeedKmh - low.ScaleSpeedKmh;
            if (span == 0) return high.RealSpeedMetersPerSecond;
            var fraction = (double)(scaleSpeedKmh - low.ScaleSpeedKmh) / span;
            return low.RealSpeedMetersPerSecond + fraction * (high.RealSpeedMetersPerSecond - low.RealSpeedMetersPerSecond);
        }
        return points[^1].RealSpeedMetersPerSecond;
    }
}
