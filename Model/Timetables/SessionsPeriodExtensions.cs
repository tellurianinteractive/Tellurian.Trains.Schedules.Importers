using Tellurian.Trains.Schedules.Model.Duties;
using Tellurian.Trains.Schedules.Model.Schedules;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// The sessions or days of a layout's operating period, and which of them something runs on.
/// </summary>
/// <remarks>
/// A stored <see cref="Sessions"/> value may hold bits outside the plan's period: sessions above the number the
/// plan has (an "all sessions" value has all 14), and, for a week of days, a second half that is not the mirror
/// of the week, left over from before the plan ran days. Those bits are no sessions or days of the plan. Whatever
/// compares sessions reads the <c>ActualSessions</c> of a train, driver duty or vehicle assignment, defined here
/// once: the stored value narrowed to the period, with a week of days mirrored so a day shift past midnight goes
/// from the last day to the first.
/// </remarks>
public static class SessionsPeriodExtensions
{
    extension(SessionsSettings settings)
    {
        /// <summary>
        /// The 1-based numbers of the sessions or days in the operating period, in order: up to
        /// <see cref="SessionsSettings.MaxNumberOfSessions"/>, and never more than 7 days or 14 sessions.
        /// </summary>
        public IReadOnlyList<int> PeriodNumbers =>
            [.. Enumerable.Range(1, Math.Clamp(settings.MaxNumberOfSessions, 1, settings.UseDaysInsteadOfSessionNumbers ? 7 : 14))];
    }

    extension(GeneralSettings general)
    {
        /// <summary>
        /// The sessions or days of the plan that <paramref name="sessions"/> stands for: only those within the
        /// operating period, the on-demand marker kept. This is the one rule for which stored bits count.
        /// </summary>
        /// <param name="sessions">A stored value.</param>
        public Sessions ActualSessions(Sessions sessions) => sessions.WithinPeriod(general.UseDays, general.MaxSessions);
    }

    extension(Sessions sessions)
    {
        /// <summary>
        /// The 1-based numbers of the sessions or days of the operating period these sessions include, in order.
        /// Bits beyond the period are ignored, as when the sessions are shown.
        /// </summary>
        /// <param name="settings">The operating period.</param>
        public IReadOnlyList<int> NumbersWithin(SessionsSettings settings) =>
            [.. settings.PeriodNumbers.Where(number => sessions.Includes(number))];
    }

    extension(Train train)
    {
        /// <summary>
        /// The sessions or days of the plan the train runs on; see <see cref="SessionsPeriodExtensions"/>. The
        /// stored value as it is when the train belongs to no layout yet.
        /// </summary>
        public Sessions ActualSessions => Actual(train.Layout ?? train.Timetable?.Layout, train.Sessions);
    }

    extension(DriverDuty duty)
    {
        /// <summary>The sessions or days of the plan the duty is worked on; see <see cref="SessionsPeriodExtensions"/>.</summary>
        public Sessions ActualSessions => Actual(duty.Plan?.Timetable?.Layout, duty.Sessions);
    }

    extension(ScheduleAssignment assignment)
    {
        /// <summary>The sessions or days of the plan the vehicle works the schedule; see <see cref="SessionsPeriodExtensions"/>.</summary>
        public Sessions ActualSessions =>
            Actual((assignment.Schedule?.Plan ?? assignment.ScheduledObject?.Plan)?.Timetable?.Layout, assignment.Sessions);
    }

    private static Sessions Actual(Layout? layout, Sessions sessions) =>
        layout?.Settings?.General is { } general ? general.ActualSessions(sessions) : sessions;
}
