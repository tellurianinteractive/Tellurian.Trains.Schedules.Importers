namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// 
/// </summary>
public static class GeneralSettingsExtensions
{
    extension(GeneralSettings settings)
    {
        /// <summary>
        /// Extracts <see cref="SessionsSettings"/> from <see cref="GeneralSettings"/> with default to use short weekday names.
        /// </summary>
        public SessionsSettings SessionSettings(bool useShortWeekdayNames = true) => new()
        {
            MaxNumberOfSessions = settings.MaxSessions,
            UseDaysInsteadOfSessionNumbers = settings.UseDays,
            SessionFirstWeekday = settings.StartDay,
            UseShortWeekdayNames = useShortWeekdayNames,
        };

        /// <summary>
        /// The fast-clock time the first session/day begins, as far as where vehicles start is concerned:
        /// <see cref="GeneralSettings.FirstSessionStartTime"/> when the layout runs over midnight, otherwise
        /// 00:00, since every train part then lies within the session's own operating window.
        /// </summary>
        public TimeSpan FirstSessionStart =>
            settings.RunsOverMidnight ? settings.FirstSessionStartTime : TimeSpan.Zero;

        /// <summary>
        /// The fast-clock time a session or day begins, which is where the first shift starts:
        /// <see cref="GeneralSettings.FirstSessionStartTime"/> when the layout runs over midnight and
        /// states one, otherwise the start of the operating window (<see cref="GeneralSettings.StartTime"/>).
        /// </summary>
        public TimeSpan SessionStart =>
            settings.RunsOverMidnight && settings.FirstSessionStartTime != TimeSpan.Zero
                ? settings.FirstSessionStartTime
                : settings.StartTime;

        /// <summary>
        /// The span of one session or day that the shifts divide between them: from
        /// <see cref="SessionStart"/> to the end of the operating window, or to the same time the next
        /// day when the layout runs over midnight (a session is then a whole round of the clock).
        /// </summary>
        public (TimeSpan Start, TimeSpan End) SessionSpan
        {
            get
            {
                var start = settings.SessionStart;
                var end = settings.RunsOverMidnight ? start + TimeSpan.FromHours(24) : settings.EndTime;
                return (start, end > start ? end : end + TimeSpan.FromHours(24));
            }
        }

        /// <summary>
        /// The start and end of each shift when a session is divided into <paramref name="shiftCount"/>
        /// (1–3) shifts of equal length, the first beginning at <see cref="SessionStart"/>. The boundaries
        /// fall on whole five minutes, so a shift is announced as 10:00–14:30 rather than 14:27. Times
        /// past 24 hours belong to the day after.
        /// </summary>
        /// <param name="shiftCount">How many shifts the session is divided into; kept between one and three.</param>
        public IReadOnlyList<(TimeSpan Start, TimeSpan End)> ShiftTimes(int shiftCount)
        {
            var count = Math.Clamp(shiftCount, 1, 3);
            var (start, end) = settings.SessionSpan;
            var span = end - start;
            TimeSpan Boundary(int index) =>
                index == count ? end : start + TimeSpan.FromMinutes(Math.Round(span.TotalMinutes * index / count / 5) * 5);
            return [.. Enumerable.Range(0, count).Select(index => (Boundary(index), Boundary(index + 1)))];
        }
    }
}
