namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// How long a station track is actually occupied — which is longer than the trains calling there say on
/// their own.
/// </summary>
/// <remarks>
/// <para>
/// A train occupies its track for its whole call window, from the arrival to the departure, including
/// the two times at the ends of a run that are not movements at all: the driver reports before the train
/// leaves its origin, and stands down after it has arrived at its destination. The train is physically
/// standing there throughout, so for occupancy that span is real — the exact opposite of a meet, which
/// happens only while a train is on its way and is therefore suppressed at those two calls.
/// </para>
/// <para>
/// The traction unit then extends it. A unit that arrives with one train and leaves with the next stands
/// on the track for the whole time between, so the track is not free in that gap even though no train's
/// own window covers it.
/// </para>
/// <para>
/// Unless it is stabled or lifted off. Where the unit is driven to stabling or lifted off on arrival, or
/// comes from stabling or is lifted on before departing, it is not standing on this track between the two trains, and occupancy falls back to what
/// each train occupies by itself. The same holds where it is put on another track of the station on
/// arrival, or fetched from another track before departing.
/// </para>
/// </remarks>
public static class TrackOccupancyExtensions
{
    extension(StationCall call)
    {
        /// <summary>
        /// Whether this call occupies its track at all. Every call of a travelling train does. The call of
        /// a shunting task does only when a traction unit is assigned to it: without one the task is
        /// worked over the whole station — by a train loco already standing there, an unmodelled station
        /// pilot or by hand — so no track is taken, and the track and times it names are ignored.
        /// </summary>
        /// <param name="vehicleSchedules">The schedules that assign vehicles to train parts.</param>
        public bool OccupiesTrack(IEnumerable<Schedule>? vehicleSchedules) =>
            !call.Train.IsShuntingTask ||
            (vehicleSchedules ?? []).Any(s => s.Parts.Any(p => p.ContainsCall(call)) && s.Vehicles.Any(v => v.IsTraction));

        /// <summary>
        /// The span for which this call occupies its track, given the vehicle schedules that may extend
        /// it beyond the call's own times.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The extension runs only to where the next train's <em>own</em> window begins, not to its
        /// departure. The two spans are then contiguous rather than nested, so between them they cover
        /// the unit's whole stay while a third train standing in the gap conflicts with exactly one of
        /// them — one clash reported once, instead of the same clash reported against both trains.
        /// </para>
        /// <para>
        /// Only a continuation onto the <em>same track</em> extends anything. A unit that leaves on a
        /// train departing from another track has moved between the two, and nothing in the data says
        /// when — so the gap is left unclaimed rather than attributed to a track it may not be on. The
        /// same goes for a unit put on another track after arriving. One fetched from this track for a
        /// train departing from another stays here until then, so that one does extend it.
        /// </para>
        /// </remarks>
        /// <param name="vehicleSchedules">The schedules that assign vehicles to train parts. Occupancy
        /// falls back to the call's own window when none of them covers this call.</param>
        public (Time From, Time To) TrackOccupancy(IEnumerable<Schedule>? vehicleSchedules)
        {
            call = call.ValueOrException(nameof(call));
            var own = (From: call.Arrival, To: call.Departure);
            if (vehicleSchedules is null) return own;

            foreach (var schedule in vehicleSchedules)
            {
                var parts = schedule.OrderedParts;
                for (var i = 0; i < parts.Count - 1; i++)
                {
                    // By reference: StationCall equality is by value and a train may hold two calls that
                    // compare equal, so only identity says this is the call the unit arrives on.
                    if (!ReferenceEquals(parts[i].To, call)) continue;

                    var arriving = parts[i];
                    var leaving = parts[i + 1];
                    // Stabled or lifted off, the unit is not on this track between the two trains.
                    if (arriving.TractionOptions is { ToLayover: not TractionLayover.None }) return own;
                    if (leaving.TractionOptions is { FromLayover: not TractionLayover.None }) return own;
                    // Where the vehicles are put after arriving and where they are fetched from before
                    // leaving: the other track a part names, or else the train's own. Only a stay on this
                    // call's track at both ends keeps this track occupied.
                    var leftOn = arriving.OtherToTrack ?? call.Track;
                    var fetchedFrom = leaving.OtherFromTrack ?? leaving.From.Track;
                    if (!leftOn.Equals(call.Track) || !fetchedFrom.Equals(call.Track)) return own;

                    return leaving.From.Arrival > own.To ? (own.From, leaving.From.Arrival) : own;
                }
            }
            return own;
        }
    }

    /// <summary>
    /// Whether two occupancy spans are on the track at the same time. Open at both ends, so one train
    /// arriving exactly as another leaves is a handover rather than a conflict.
    /// </summary>
    public static bool OverlapsInTime(this (Time From, Time To) span, (Time From, Time To) other) =>
        span.ConflictsInTime(other, 0);

    /// <summary>
    /// Whether two occupancy spans leave less than <paramref name="minMinutesBetween"/> fast-clock
    /// minutes of free track between them — either because they overlap, or because they follow each
    /// other too closely for the track to be considered free in between.
    /// </summary>
    /// <remarks>
    /// The overlap case is this rule with no required gap: at zero the spans conflict only where they
    /// actually cover the same time, and a train arriving exactly as another leaves is still a handover.
    /// Above zero the same test asks for that much free time as well, so exactly the required number of
    /// minutes is enough and one minute less is a conflict.
    /// </remarks>
    /// <param name="span">The occupancy span to test.</param>
    /// <param name="other">The occupancy span to test it against.</param>
    /// <param name="minMinutesBetween">The free time the track needs between two occupancies, in
    /// fast-clock minutes; see <see cref="Settings.ValidationSettings.MinMinutesBetweenTrackUsage"/>.</param>
    public static bool ConflictsInTime(this (Time From, Time To) span, (Time From, Time To) other, int minMinutesBetween) =>
        span.From < other.To.AddMinutes(minMinutesBetween) && span.To.AddMinutes(minMinutesBetween) > other.From;

    /// <summary>
    /// The same test for spans of trains running on given sessions. A time past midnight is stored on the
    /// next day, so a span is the same time on the clock as one a day earlier or later — but only on the
    /// session that day belongs to: when the other span is moved <c>d</c> days, its train must run on a
    /// session <c>d</c> later than the session the first runs on. Two spans a day apart therefore conflict
    /// only where the sessions line up that way, not wherever both trains run.
    /// </summary>
    public static bool ConflictsInTime(this (Time From, Time To) span, Sessions sessions, (Time From, Time To) other, Sessions otherSessions, int minMinutesBetween) =>
        DayShifts.Any(d => RunTogether(sessions, otherSessions, d) && span.ConflictsInTime(other.Shifted(d), minMinutesBetween));

    private static (Time From, Time To) Shifted(this (Time From, Time To) span, int days) => (span.From.AddDays(days), span.To.AddDays(days));

    /// <summary>The day shifts worth trying: a train spans less than a day, so only the neighbouring days can meet.</summary>
    internal static readonly int[] DayShifts = [-1, 0, 1];

    // Same day: the trains' sessions overlap (on-demand trains included). A day apart: SharesSession.
    private static bool RunTogether(Sessions sessions, Sessions other, int dayShift) =>
        dayShift == 0 ? sessions.Overlaps(other) : SharesSession(sessions, other, dayShift);

    /// <summary>
    /// Whether some session of <paramref name="sessions"/> has a session of <paramref name="other"/>
    /// <paramref name="dayShift"/> days later.
    /// </summary>
    internal static bool SharesSession(Sessions sessions, Sessions other, int dayShift) =>
        Enumerable.Range(1, 14).Any(s => s + dayShift is >= 1 and <= 14 && sessions.Includes(s) && other.Includes(s + dayShift));

    /// <summary>
    /// The free time between two occupancy spans that do not overlap, in whole fast-clock minutes.
    /// Negative where they do overlap.
    /// </summary>
    public static int FreeMinutesBetween(this (Time From, Time To) span, (Time From, Time To) other) =>
        (int)(span.From >= other.To ? span.From.Subtract(other.To).TotalMinutes : other.From.Subtract(span.To).TotalMinutes);

    /// <summary>
    /// The free time between spans of trains running on given sessions: the least over the day shifts the
    /// sessions allow (see <see cref="ConflictsInTime(ValueTuple{Time, Time}, Sessions, ValueTuple{Time, Time}, Sessions, int)"/>).
    /// </summary>
    public static int FreeMinutesBetween(this (Time From, Time To) span, Sessions sessions, (Time From, Time To) other, Sessions otherSessions) =>
        DayShifts.Where(d => RunTogether(sessions, otherSessions, d)).Select(d => span.FreeMinutesBetween(other.Shifted(d))).DefaultIfEmpty(int.MaxValue).Min();
}
