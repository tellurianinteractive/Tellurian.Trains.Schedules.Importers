namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// Operational time durations for a station. Used both as layout-wide defaults
/// (on <see cref="TimeAndSpeedSettings"/>) and as per-station overrides on an operation location.
/// Each value is nullable: on a station, <c>null</c> means the layout default applies.
/// </summary>
public sealed class StationTimings
{
    /// <summary>Minimum time a train stops at a manned station, in fast-clock minutes.</summary>
    public int? MinimumStopMinutes { get; set; }

    /// <summary>
    /// Real time for a locomotive to run around its train: minutes below <see cref="FirstValueInSeconds"/>,
    /// seconds from there (see <see cref="RealDurationOf"/>). The name is from when it was minutes only.
    /// </summary>
    public int? LocoRunaroundRealMinutes { get; set; }

    /// <summary>
    /// Real time for telephone dispatch clearance: minutes below <see cref="FirstValueInSeconds"/>, seconds
    /// from there (see <see cref="RealDurationOf"/>). The name is from when it was minutes only.
    /// </summary>
    public int? TrainClearanceRealMinutes { get; set; }

    /// <summary>
    /// The value from which a real duration is read as seconds instead of minutes. Nobody needs ten real
    /// minutes to clear a train, and nobody ten seconds to run a loco round, so one field takes both.
    /// </summary>
    public const int FirstValueInSeconds = 10;

    /// <summary>Whether a real duration value is in seconds rather than minutes.</summary>
    /// <param name="value">The value as entered.</param>
    public static bool IsSeconds(int value) => value >= FirstValueInSeconds;

    /// <summary>
    /// The real time a value stands for: minutes below <see cref="FirstValueInSeconds"/>, seconds from there.
    /// Zero or less is no time at all.
    /// </summary>
    /// <param name="value">The value as entered.</param>
    public static TimeSpan RealDurationOf(int value) =>
        value <= 0 ? TimeSpan.Zero : IsSeconds(value) ? TimeSpan.FromSeconds(value) : TimeSpan.FromMinutes(value);

    /// <summary>The layout-wide default timings as specified in the requirements (3 / 5 / 1).</summary>
    public static StationTimings LayoutDefaults => new()
    {
        MinimumStopMinutes = 3,
        LocoRunaroundRealMinutes = 5,
        TrainClearanceRealMinutes = 1
    };
}
