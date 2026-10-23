using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// The least time a train must stand at a stop, in fast-clock minutes: the minimum stop, raised to the time
/// the train clearance takes at a manned station and, where the loco is run round, to the time the
/// runaround takes. They are not added together; the longest of them applies.
/// </summary>
public static class RequiredStopTimes
{
    extension(TimeAndSpeedSettings settings)
    {
        /// <summary>
        /// A real duration, entered as minutes or seconds (see <see cref="StationTimings.RealDurationOf"/>),
        /// as whole fast-clock minutes, rounded up since a timetable is in whole minutes.
        /// </summary>
        /// <param name="value">The value as entered.</param>
        public int FastClockMinutesOf(int value) =>
            (int)Math.Ceiling(StationTimings.RealDurationOf(value).TotalMinutes * settings.FastClockSpeed - 1e-9);

        /// <summary>
        /// The least time a train must stand at a stop at <paramref name="location"/>, in fast-clock minutes.
        /// </summary>
        /// <param name="location">The location the train stops at.</param>
        /// <param name="locoRunsRound">Whether the loco is run round to the other end of the train there.</param>
        public int RequiredStopMinutes(OperationLocation location, bool locoRunsRound)
        {
            var defaults = settings.StationTimings;
            var required = location.Timings.MinimumStopMinutes ?? defaults.MinimumStopMinutes ?? 3;
            if (location.HasTrainClearanceTime)
                required = Math.Max(required, settings.FastClockMinutesOf(location.Timings.TrainClearanceRealMinutes ?? defaults.TrainClearanceRealMinutes ?? 1));
            if (locoRunsRound)
                required = Math.Max(required, settings.FastClockMinutesOf(location.Timings.LocoRunaroundRealMinutes ?? defaults.LocoRunaroundRealMinutes ?? 5));
            return required;
        }
    }
}
