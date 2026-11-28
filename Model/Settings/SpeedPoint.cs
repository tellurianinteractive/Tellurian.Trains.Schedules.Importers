namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// A single point on the piecewise-linear speed mapping curve, relating a scale speed to a real model speed.
/// </summary>
public sealed class SpeedPoint
{
    /// <summary>Parameterless constructor for EF Core and JSON deserialization.</summary>
    public SpeedPoint() { }

    /// <summary>Initializes a new <see cref="SpeedPoint"/>.</summary>
    /// <param name="scaleSpeedKmh">Scale speed in km/h.</param>
    /// <param name="realSpeedMetersPerSecond">Real model speed in metres per second.</param>
    public SpeedPoint(int scaleSpeedKmh, double realSpeedMetersPerSecond)
    {
        ScaleSpeedKmh = scaleSpeedKmh;
        RealSpeedMetersPerSecond = realSpeedMetersPerSecond;
    }

    /// <summary>Scale speed in km/h.</summary>
    public int ScaleSpeedKmh { get; set; }

    /// <summary>Real model speed in metres per second.</summary>
    public double RealSpeedMetersPerSecond { get; set; }
}
