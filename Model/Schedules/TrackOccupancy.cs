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
/// The vehicles then extend it: a traction unit or a wagonset stands where it was left until it is moved
/// or leaves with its next train. That stay is not a call and is described by a <see cref="TrackHold"/>,
/// which is also what the graph draws as a dashed line.
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
        /// The span for which this call occupies its track by itself: its arrival to its departure. What
        /// the vehicles standing there add before and after is a <see cref="TrackHold"/>.
        /// </summary>
        public (Time From, Time To) TrackOccupancy => (call.Arrival, call.Departure);
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

    /// <summary>
    /// The number of days the other span has to be moved for the two to be on the track together, when
    /// they are (see <see cref="ConflictsInTime(ValueTuple{Time, Time}, Sessions, ValueTuple{Time, Time}, Sessions, int)"/>
    /// with no required gap); <c>null</c> when they are not. Lets a caller tell which of two spans comes
    /// first once a time past midnight has been put on the day it belongs to.
    /// </summary>
    internal static int? OverlapDayShift(this (Time From, Time To) span, Sessions sessions, (Time From, Time To) other, Sessions otherSessions) =>
        DayShifts.Where(d => RunTogether(sessions, otherSessions, d) && span.ConflictsInTime(other.Shifted(d), 0)).Select(d => (int?)d).FirstOrDefault();

    /// <summary>
    /// The location a train comes from to this call (<paramref name="before"/>) or goes on to from it,
    /// read in run order; <c>null</c> where the train begins or ends its run here, or the neighbouring
    /// call is at the same location. It tells which end of a track a train enters or leaves by: trains
    /// from the same neighbour come in at the same end.
    /// </summary>
    internal static int? NeighbourLocation(this StationCall call, bool before)
    {
        if (call.Train is not { } train) return null;
        var calls = train.CallsInRunOrder;
        var index = -1;
        for (var i = 0; i < calls.Count; i++)
            if (ReferenceEquals(calls[i], call)) { index = i; break; }
        var neighbour = before ? index - 1 : index + 1;
        if (index < 0 || neighbour < 0 || neighbour >= calls.Count) return null;
        var location = calls[neighbour].OperationLocation;
        return location.Equals(call.OperationLocation) ? null : location.Id;
    }

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
        SharesSession(sessions.Flags & SessionBits, other.Flags & SessionBits, dayShift);

    /// <summary>The bits of <see cref="Sessions.Flags"/> that stand for sessions 1 to 14.</summary>
    internal const int SessionBits = 0x3FFF;

    /// <summary>
    /// <see cref="SharesSession(Sessions, Sessions, int)"/> on session bits already masked with
    /// <see cref="SessionBits"/>: session s is bit s-1, so moving the other bits <paramref name="dayShift"/>
    /// places session s + dayShift under s.
    /// </summary>
    internal static bool SharesSession(int sessionBits, int otherBits, int dayShift) =>
        (sessionBits & (dayShift >= 0 ? otherBits >> dayShift : otherBits << -dayShift) & SessionBits) != 0;

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
