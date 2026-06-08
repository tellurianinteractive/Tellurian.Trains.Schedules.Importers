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

    /// <summary>Real time for a locomotive to run around its train, in real minutes.</summary>
    public int? LocoRunaroundRealMinutes { get; set; }

    /// <summary>Real time for telephone dispatch clearance, in real minutes.</summary>
    public int? TrainClearanceRealMinutes { get; set; }

    /// <summary>The layout-wide default timings as specified in the requirements (3 / 5 / 1).</summary>
    public static StationTimings LayoutDefaults => new()
    {
        MinimumStopMinutes = 3,
        LocoRunaroundRealMinutes = 5,
        TrainClearanceRealMinutes = 1
    };
}
