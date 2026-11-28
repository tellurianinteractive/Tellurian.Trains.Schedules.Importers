using Tellurian.Trains.Schedules.Model.Duties;
using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Which trains run in the first session or day, when the meeting begins later than the operating window.
/// </summary>
/// <remarks>
/// When the layout runs over midnight, the meeting begins at the fast-clock time the first session/day starts
/// (<c>FirstSessionStart</c> of <see cref="GeneralSettings"/>). What would have been worked before then on the first
/// session/day is not: every train of a driver duty that starts before then, since the crew is not on duty yet,
/// and every train that itself starts before then (its first arrival, which is when its duty starts).
/// The whole plan builds on this premise.
/// </remarks>
public static class FirstSessionExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// The trains that do not run in the first session or day, because they or their driver duty start
        /// before the first session/day begins. Empty when the meeting begins with the operating window.
        /// </summary>
        public IReadOnlySet<Train> TrainsStartingBeforeFirstSession
        {
            get
            {
                var firstSessionStart = plan.Timetable.Settings.General.FirstSessionStart;
                if (firstSessionStart <= TimeSpan.Zero) return new HashSet<Train>();
                var firstSession = Sessions.FromSessionNumbers(1);
                var inEarlyDuties = plan.DriverDuties
                    .Where(duty => duty.Sessions.Overlaps(firstSession) && duty.StartsBefore(firstSessionStart))
                    .SelectMany(duty => duty.Parts.Select(part => part.Train));
                var startingEarly = plan.Timetable.Trains.Where(train => train.StartsBefore(firstSessionStart));
                return inEarlyDuties.Concat(startingEarly).ToHashSet();
            }
        }

        /// <summary>
        /// Whether <paramref name="train"/> does not run in the first session or day, because it or its driver duty
        /// starts before the first session/day begins. See <see cref="FirstSessionExtensions"/>.
        /// </summary>
        /// <param name="train">A train of the plan's timetable.</param>
        public bool StartsBeforeFirstSession(Train train) => plan.TrainsStartingBeforeFirstSession.Contains(train);

        /// <summary>
        /// The 1-based numbers of the sessions or days of the operating period that <paramref name="train"/> starts
        /// on: the ones its sessions include, except the first when it starts before the first session/day begins.
        /// </summary>
        /// <param name="train">A train of the plan's timetable.</param>
        public IReadOnlyList<int> SessionNumbersOf(Train train)
        {
            var numbers = train.Sessions.NumbersWithin(plan.Timetable.Settings.General.SessionSettings());
            return plan.StartsBeforeFirstSession(train) ? [.. numbers.Where(number => number > 1)] : numbers;
        }
    }

    extension(Train train)
    {
        /// <summary>
        /// Whether the train starts before <paramref name="time"/> on its own day: its
        /// <see cref="Train.DriverStartTime"/> (the arrival of its first call) is earlier.
        /// </summary>
        /// <param name="time">A fast-clock time of day.</param>
        public bool StartsBefore(TimeSpan time) =>
            train.Calls.Count > 0 &&
            train.DriverStartTime.Value.Days == 0 &&
            train.DriverStartTime.Value < time;
    }

    extension(DriverDuty duty)
    {
        /// <summary>
        /// Whether the driver duty starts before <paramref name="time"/> on its own day.
        /// </summary>
        /// <param name="time">A fast-clock time of day.</param>
        public bool StartsBefore(TimeSpan time) =>
            duty.StartTime is { } start &&
            start.Value.Days == 0 &&
            start.Value < time;
    }
}
