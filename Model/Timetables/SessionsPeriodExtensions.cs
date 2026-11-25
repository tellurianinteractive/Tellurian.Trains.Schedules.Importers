namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// The sessions or days of a layout's operating period, and which of them something runs on.
/// </summary>
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
}
