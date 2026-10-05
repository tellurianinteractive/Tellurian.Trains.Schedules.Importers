using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Duties;

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
/// <para>
/// A driver duty also occupies its driver between the trains it works: from one part's last-call
/// departure to the next part's first-call arrival the driver is waiting or walking, and cannot work
/// anything else. So does an overridden duty start before the first train, or end after the last one.
/// That time is added on top of the trains.
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
        /// layout's period counts on every session, so it is never silently dropped. The gaps within a driver
        /// duty count on the duty's sessions, by the same rule.
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
        /// <param name="duties">The plan's driver duties, whose gaps between parts, and overridden start and end
        /// times, also occupy a driver; none when omitted.</param>
        public int[] RequiredLocoDriversPerMinute(TimeSpan start, TimeSpan end, bool useDays, int maxSessions, IEnumerable<DriverDuty>? duties = null)
        {
            var minutes = (int)(end - start).TotalMinutes;
            if (minutes <= 0) return [];

            var periodLength = Math.Clamp(maxSessions, 1, useDays ? 7 : 14);

            // Every interval occupying a driver, with the sessions it applies to, restricted to the layout's
            // period. An empty set means no in-period session, and the interval then counts on every session
            // rather than disappearing.
            byte[] InPeriod(Sessions sessions) => [.. sessions.Numbers.Where(n => n >= 1 && n <= periodLength)];
            var occupancies = timetable.Trains
                .Where(t => t.Calls.Count > 0)
                .Select(t => new Occupancy(t.DriverStartTime.Value, t.DriverEndTime.Value, InPeriod(t.Sessions)))
                .Concat((duties ?? []).SelectMany(d => Gaps(d).Select(g => new Occupancy(g.From, g.To, InPeriod(d.Sessions)))))
                .ToArray();
            if (occupancies.Length == 0) return new int[minutes];

            var startMinutes = (int)start.TotalMinutes;
            var required = new int[minutes];
            var counts = new int[minutes + 1]; // difference array, reused per session

            for (var session = 1; session <= periodLength; session++)
            {
                Array.Clear(counts);
                foreach (var occupancy in occupancies)
                {
                    if (occupancy.Sessions.Length > 0 && !occupancy.Sessions.Contains((byte)session)) continue;
                    AddService(counts, occupancy, startMinutes, minutes);
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

    private sealed record Occupancy(TimeSpan From, TimeSpan To, byte[] Sessions);

    // The time a duty occupies its driver beyond the trains it works, measured the way a train's service
    // window is. Between consecutive parts it runs from the earlier part's last-call departure to the next
    // part's first-call arrival; consecutive parts of the same train share a call, so they leave no gap. An
    // overridden start earlier than the first part's first-call arrival, or an overridden end later than the
    // last part's last-call departure, adds the time before or after too. An override moving the start later
    // or the end earlier adds nothing, since the trains themselves still need a driver.
    private static IEnumerable<(TimeSpan From, TimeSpan To)> Gaps(DriverDuty duty)
    {
        var ordered = duty.OrderedParts;
        if (ordered.Count == 0) yield break;

        if (duty.OverriddenStartTime is { } start && duty.DefaultStartTime is { } defaultStart && start.Value < defaultStart.Value)
            yield return (start.Value, defaultStart.Value);

        for (var i = 0; i < ordered.Count - 1; i++)
        {
            var from = ordered[i].To.Departure.Value;
            var to = ordered[i + 1].From.Arrival.Value;
            if (to > from) yield return (from, to);
        }

        if (duty.OverriddenEndTime is { } end && duty.DefaultEndTime is { } defaultEnd && end.Value > defaultEnd.Value)
            yield return (defaultEnd.Value, end.Value);
    }

    // Marks the occupied interval in the difference array, clipped to the visible window. An interval
    // reaching past 24:00 is also added shifted one day earlier, so its after-midnight part lands at the start
    // of the axis exactly where the graphical timetable wraps it.
    private static void AddService(int[] counts, Occupancy occupancy, int startMinutes, int minutes)
    {
        var from = (int)occupancy.From.TotalMinutes;
        var to = (int)occupancy.To.TotalMinutes;
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
