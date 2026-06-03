namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Configures which validations are run and the thresholds they use when validating a schedule.
/// </summary>
public class ValidationOptions
{
    /// <summary>Gets or sets a value indicating whether station call timings are validated.</summary>
    public bool ValidateStationCalls { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether station track usage is validated.</summary>
    public bool ValidateStationTracks { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether track stretch conflicts are validated.</summary>
    public bool ValidateStretches { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether train speeds between calls are validated.</summary>
    public bool ValidateTrainSpeed { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether train numbers are validated.</summary>
    public bool ValidateTrainNumbers { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether vehicle schedules are validated for overlaps and double bookings.</summary>
    public bool ValidateVehicleSchedules { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether locomotive coverage (gaps and overlaps) is validated.</summary>
    public bool ValidateLocomotiveCoverage { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether driver duties are validated.</summary>
    public bool ValidateDriverDuties { get; set; } = true;
    /// <summary>Gets or sets the minimum allowed train speed, in metres per clock minute.</summary>
    public double MinTrainSpeedMetersPerClockMinute { get; set; } = 0.3;
    /// <summary>Gets or sets the maximum allowed train speed, in metres per clock minute.</summary>
    public double MaxTrainSpeedMetersPerClockMinute { get; set; } = 10;
    /// <summary>Gets or sets the minimum number of minutes required between successive uses of the same track.</summary>
    public int MinMinutesBetweenTrackUsage { get; set; }
}
