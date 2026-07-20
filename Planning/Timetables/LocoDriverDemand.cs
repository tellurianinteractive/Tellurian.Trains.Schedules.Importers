using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>
/// Computes how many loco drivers a timetable actually requires at each minute of the operating window,
/// so the planned crewing can be compared against the drivers expected to be available.
/// <para>
/// A train occupies one driver for its whole service window — from the first call's arrival (when the
/// driver takes over) to the last call's departure (when the driver is released), see
/// <see cref="Train.DriverStartTime"/>/<see cref="Train.DriverEndTime"/> — including the time it stands
/// still at intermediate stations, because the driver stays with it throughout.
/// </para>
/// </summary>
public static class LocoDriverDemand
{
    private const int MinutesPerDay = 24 * 60;

    extension(Timetable timetable)
    {
        /// <summary>
        /// The number of drivers required at each minute of the window <paramref name="start"/>–<paramref name="end"/>,
        /// one entry per minute (the entry at index <c>i</c> covers the minute starting at <c>start + i</c>).
        /// <para>
        /// Trains running on different sessions never share a minute in reality, so the demand is computed per
        /// session and the highest of those is returned: the number of drivers needed on the busiest single session, which
        /// is the number that has to be available. A train whose operating pattern covers no session within the
        /// layout's period counts on every session, so it is never silently dropped.
        /// </para>
        /// <para>
        /// Service windows running past midnight (times at or beyond 24:00) are wrapped back to the start of the
        /// day, matching how the graphical timetable draws them.
        /// </para>
        /// </summary>
        /// <param name="start">First minute of the window.</param>
        /// <param name="end">Last minute of the window; an empty window yields an empty result.</param>
        /// <param name="useDays">Whether the layout's period is expressed as days rather than sessions.</param>
        /// <param name="maxSessions">The number of sessions/days in the layout's operating period (1–14).</param>
        public int[] RequiredLocoDriversPerMinute(TimeSpan start, TimeSpan end, bool useDays, int maxSessions)
        {
            var minutes = (int)(end - start).TotalMinutes;
            if (minutes <= 0) return [];

            var periodLength = Math.Clamp(maxSessions, 1, useDays ? 7 : 14);
            var trains = timetable.Trains.Where(t => t.Calls.Count > 0).ToArray();
            if (trains.Length == 0) return new int[minutes];

            // The sessions each train works, restricted to the layout's period. An empty set means the train
            // has no in-period session, and it is then counted on every session rather than disappearing.
            var trainSessions = trains
                .Select(t => t.Sessions.Numbers.Where(n => n >= 1 && n <= periodLength).ToArray())
                .ToArray();

            var startMinutes = (int)start.TotalMinutes;
            var required = new int[minutes];
            var counts = new int[minutes + 1]; // difference array, reused per session

            for (var session = 1; session <= periodLength; session++)
            {
                Array.Clear(counts);
                for (var i = 0; i < trains.Length; i++)
                {
                    var sessions = trainSessions[i];
                    if (sessions.Length > 0 && !sessions.Contains((byte)session)) continue;
                    AddService(counts, trains[i], startMinutes, minutes);
                }

                // Prefix-sum the difference array into the running count and keep the per-minute maximum.
                var running = 0;
                for (var m = 0; m < minutes; m++)
                {
                    running += counts[m];
                    if (running > required[m]) required[m] = running;
                }
            }
            return required;
        }
    }

    // Marks the train's driver service window in the difference array, clipped to the visible window. A window
    // reaching past 24:00 is also added shifted one day earlier, so its after-midnight part lands at the start
    // of the axis exactly where the graphical timetable wraps it.
    private static void AddService(int[] counts, Train train, int startMinutes, int minutes)
    {
        var from = (int)train.DriverStartTime.Value.TotalMinutes;
        var to = (int)train.DriverEndTime.Value.TotalMinutes;
        if (to <= from) return;

        AddInterval(counts, from - startMinutes, to - startMinutes, minutes);
        if (to > MinutesPerDay)
            AddInterval(counts, from - MinutesPerDay - startMinutes, to - MinutesPerDay - startMinutes, minutes);
    }

    private static void AddInterval(int[] counts, int from, int to, int minutes)
    {
        var first = Math.Max(from, 0);
        var last = Math.Min(to, minutes);
        if (last <= first) return;
        counts[first]++;
        counts[last]--;
    }
}
