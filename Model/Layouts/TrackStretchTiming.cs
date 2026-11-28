using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>The running time of a <see cref="TrackStretch"/>, worked out from its distance and speed.</summary>
public static class TrackStretchTiming
{
    /// <summary>
    /// The fast-clock minutes a train takes over <paramref name="stretch"/> at its <see cref="TrackStretch.Speed"/>:
    /// the distance at the real speed that the scale speed maps to, times the fast-clock speed, rounded, and at least 1.
    /// </summary>
    public static int RunningMinutes(TrackStretch stretch, TimeAndSpeedSettings settings)
    {
        ArgumentNullException.ThrowIfNull(stretch);
        ArgumentNullException.ThrowIfNull(settings);
        var realSpeed = settings.RealSpeedMetersPerSecond(stretch.LimitingSpeed);
        if (realSpeed <= 0) return stretch.Time;
        var minutes = stretch.Distance / realSpeed / 60.0 * settings.FastClockSpeed;
        return Math.Max(1, (int)Math.Round(minutes, MidpointRounding.AwayFromZero));
    }
}
