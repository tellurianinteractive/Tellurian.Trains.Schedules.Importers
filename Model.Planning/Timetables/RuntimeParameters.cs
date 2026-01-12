namespace Tellurian.Trains.Schedules.Model.Planning.Timetables;

/// <summary>
/// Represents configuration parameters for calculating train runtimes and scheduling operations.
/// </summary>
/// <remarks>
/// These parameters are used to convert between scale speeds and real speeds, and to determine
/// minimum stop durations, clearance times, and other scheduling-related timings.
/// </remarks>
public record RuntimeParameters
{
    /// <summary>
    /// The real max speed om trains in meters/second, usually between 0.1 to 0,4.
    /// </summary>
    public double MaxRealSpeed { get; set; }

    /// <summary>
    /// This speed should be the km/h speed that corresponds to the <see cref="MaxRealSpeed"/>
    /// </summary>
    public int MaxScaleSpeed { get; set; }

    /// <summary>
    /// For a train that stops at the station, this is the default minimum stop duration.
    /// </summary>
    public TimeSpan MinimumStoppingDuration { get; set; }

    /// <summary>
    /// The default real (not fast) time for reversing a loco at a station.
    /// This can be overridden for each <see cref="Model.OperationLocation"/>.
    /// </summary>
    public TimeSpan LocoReversingRealDuration { get; set; }

    /// <summary>
    /// If the strech is controlled by manual dispatch, e.g. making phone calls for clearance,
    /// this is the default real (not fast) time that should be added to the stop time at the station.
    /// </summary>
    public TimeSpan TrainClearanceRealDuration { get; set; }

    /// <summary>
    /// The expected fast clock speed determines the timings of trains.
    /// </summary>
    public int ExpectedFastClockSpeed { get; set; }

    /// <summary>
    /// Speed multiplied with this value gets real speed in meters per second.
    /// </summary>
    public double SpeedFactor => MaxRealSpeed / MaxScaleSpeed;

    /// <summary>
    /// Gets the default runtime parameters with commonly used values for model railway operations.
    /// </summary>
    /// <value>
    /// A <see cref="RuntimeParameters"/> instance configured with default values:
    /// MaxRealSpeed of 0.3 m/s, MaxScaleSpeed of 130 km/h, MinimumStoppingDuration of 3 minutes,
    /// LocoReversingRealDuration of 5 minutes, TrainClearanceRealDuration of 1 minute, and
    /// ExpectedFastClockSpeed of 5.
    /// </value>
    public static RuntimeParameters Default => new()
    {
        MaxRealSpeed = 0.3,
        MaxScaleSpeed = 130,
        MinimumStoppingDuration = TimeSpan.FromMinutes(3),
        LocoReversingRealDuration = TimeSpan.FromMinutes(5),
        TrainClearanceRealDuration = TimeSpan.FromMinutes(1),
        ExpectedFastClockSpeed = 5,
    };
}




