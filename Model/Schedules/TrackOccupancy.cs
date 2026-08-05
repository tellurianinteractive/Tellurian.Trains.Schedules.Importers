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
/// Unless it is parked. Where the unit is booked to parking on arrival, or comes from parking before
/// departing, it is not standing on this track between the two trains, and occupancy falls back to what
/// each train occupies by itself.
/// </para>
/// </remarks>
public static class TrackOccupancyExtensions
{
    extension(StationCall call)
    {
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
        /// when — so the gap is left unclaimed rather than attributed to a track it may not be on.
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
                    if (arriving.TractionOptions?.ToParking == true) return own;
                    if (leaving.TractionOptions?.FromParking == true) return own;
                    if (!leaving.From.Track.Equals(call.Track)) return own;

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
    /// The free time between two occupancy spans that do not overlap, in whole fast-clock minutes.
    /// Negative where they do overlap.
    /// </summary>
    public static int FreeMinutesBetween(this (Time From, Time To) span, (Time From, Time To) other) =>
        (int)(span.From >= other.To ? span.From.Subtract(other.To).TotalMinutes : other.From.Subtract(span.To).TotalMinutes);
}
