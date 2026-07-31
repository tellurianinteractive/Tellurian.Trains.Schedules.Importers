namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// Configures which validations are run and the thresholds they use when validating a schedule.
/// </summary>
public sealed class ValidationSettings
{
    /// <summary>Gets or sets a value indicating whether station call timings are validated.</summary>
    public bool ValidateStationCalls { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether station track usage is validated.</summary>
    public bool ValidateStationTracks { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether track stretch conflicts are validated.</summary>
    public bool ValidateStretches { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether train speeds between calls are validated.</summary>
    public bool ValidateTrainSpeed { get; set; } = true;
    /// <summary>
    /// Gets or sets a value indicating whether each train's route is validated for continuity, i.e. that
    /// every leg it runs is a track stretch of the layout.
    /// </summary>
    public bool ValidateRouteContinuity { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether train numbers are validated.</summary>
    public bool ValidateTrainNumbers { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether vehicle schedules are validated for overlaps and double bookings.</summary>
    public bool ValidateSchedules { get; set; } = true;
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

    /// <summary>
    /// Gets or sets a value indicating whether a traction unit's stay between two trains counts as
    /// occupying the track, on top of what the trains themselves occupy.
    /// </summary>
    /// <remarks>
    /// Off by default, because it is only sound where the plan actually records parking. A unit that
    /// arrives with one train and leaves with the next does stand on the track for the whole time
    /// between — unless it is booked to or from parking, which is what says it went elsewhere. A plan
    /// that never sets those flags cannot distinguish "stayed here" from "nothing was recorded", and
    /// reading the silence as "stayed" makes every locomotive hold its arrival track for hours: on an
    /// imported XPLN plan that turns roughly forty genuine track conflicts into five hundred.
    /// <para>
    /// Turn it on for plans where parking is modelled deliberately; the conflicts it then adds are real
    /// ones the trains' own times cannot show.
    /// </para>
    /// </remarks>
    public bool ExtendTrackOccupancyByVehicleStay { get; set; }
}
