namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A stretch of time during which the vehicles of a schedule stand on a station track without a train of
/// theirs calling there: between the train that brought them and the next one that takes them on, or from
/// the start of the day to their first train, or from their last train to the end of the day.
/// </summary>
/// <param name="Track">The track the vehicles stand on.</param>
/// <param name="From">When they take the track.</param>
/// <param name="To">When they leave it.</param>
/// <param name="Sessions">The sessions or days it applies to: those the trains either side of it run on.</param>
/// <param name="Schedule">The schedule whose vehicles stand there.</param>
/// <param name="Train">The train that brings the vehicles to the track, or takes them from it where it
/// is the day's first.</param>
public sealed record TrackHold(StationTrack Track, Time From, Time To, Sessions Sessions, Schedule Schedule, Train Train)
{
    /// <summary>The train that takes the vehicles on from the track, when there is one that day.</summary>
    public Train? DepartingTrain { get; init; }

    /// <summary>
    /// Whether the vehicles of the two holds arrive or depart in a common train. They then stand on the
    /// track together by design, and are no double booking of it.
    /// </summary>
    public bool SharesTrainWith(TrackHold other) =>
        (Train.Equals(other.Train)) ||
        (DepartingTrain is not null && DepartingTrain.Equals(other.DepartingTrain)) ||
        (DepartingTrain is not null && DepartingTrain.Equals(other.Train)) ||
        (other.DepartingTrain is not null && other.DepartingTrain.Equals(Train));

    /// <summary>The time the vehicles stand on the track.</summary>
    public (Time From, Time To) Span => (From, To);

    /// <summary>The vehicles standing there, as the planner names them.</summary>
    public string Vehicles
    {
        get
        {
            var names = Schedule.Vehicles.Select(v => v.Designation).ToList();
            return names.Count > 0 ? string.Join(" + ", names) : Schedule.Number.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }
    }
}

/// <summary>
/// Works out where the vehicles of the schedules stand between their trains. See
/// <see cref="TrackHold"/>.
/// </summary>
/// <remarks>
/// <para>
/// A traction unit or wagonset that has arrived stays on the track it was left on until it is moved or
/// leaves. It is left on the arrival track unless the part says it is put on another track of the
/// station, and it is taken from the departing train's track unless the part says it is fetched from
/// another one. Where it stays, the track is held from the end of the arriving train's own window to the
/// start of the next train's own window — the two spans are then contiguous, so the stay is covered
/// without either train being charged for the other's time. Where it is put on or fetched from another
/// track, that track is held from the arrival to the departure, while the arrival track is still held by
/// the arriving train's own window: both are occupied at the same time.
/// </para>
/// <para>
/// A vehicle that is moved to stabling or lifted off holds no track until it is picked up from
/// stabling or lifted on again.
/// </para>
/// <para>
/// The last arrival of a schedule holds its track for the rest of the day, and the first departure holds
/// its track from the start of the day, with the same exceptions. A time past midnight is stored on the
/// day after, so "the day" is the day the time is stored on.
/// </para>
/// </remarks>
public static class TrackHoldExtensions
{
    extension(IEnumerable<Schedule> schedules)
    {
        /// <summary>All the holds of all the schedules.</summary>
        public IReadOnlyList<TrackHold> TrackHolds => [.. schedules.SelectMany(s => s.TrackHolds)];
    }

    extension(Plan plan)
    {
        /// <summary>
        /// The holds a view should mark: all of the plan's when the plan counts a vehicle's stay as
        /// occupying the track (<see cref="Settings.ValidationSettings.ExtendTrackOccupancyByVehicleStay"/>),
        /// none otherwise.
        /// </summary>
        public IReadOnlyList<TrackHold> MarkedTrackHolds =>
            plan.Layout.Settings.Validation.ExtendTrackOccupancyByVehicleStay ? plan.Schedules.TrackHolds : [];
    }

    extension(Schedule schedule)
    {
        /// <summary>Where the vehicles of this schedule stand between its trains.</summary>
        public IReadOnlyList<TrackHold> TrackHolds
        {
            get
            {
                // A cargo flow is a circulation of wagons directed by waybills, not a working with times.
                if (schedule.IsCargoFlow) return [];
                // A shunting task without a traction unit takes no track (see StationCall.OccupiesTrack),
                // so vehicles that only appear in such a task do not stand anywhere because of it.
                var parts = schedule.Vehicles.Any(v => v.IsTraction)
                    ? schedule.OrderedParts
                    : [.. schedule.OrderedParts.Where(p => !p.Train.IsShuntingTask)];
                if (parts.Count == 0) return [];

                List<TrackHold> holds = [];
                var first = FirstHold(schedule, parts[0]);
                var last = LastHold(schedule, parts[^1]);
                // The vehicles that end the day on the track their first train leaves from the next day stand
                // there once, through the night, rather than at the end of one day and at the start of another.
                if (first is not null && last is not null && first.Track.Equals(last.Track) &&
                    TrackOccupancyExtensions.SharesSession(last.Sessions, first.Sessions, 1) &&
                    first.To.AddDays(1) > last.From)
                {
                    last = last with { To = first.To.AddDays(1), DepartingTrain = first.DepartingTrain };
                    first = null;
                }
                Add(holds, first);
                for (var i = 0; i < parts.Count - 1; i++)
                    Add(holds, HoldBetween(schedule, parts[i], parts[i + 1]));
                Add(holds, last);
                return holds;
            }
        }
    }

    private static void Add(List<TrackHold> holds, TrackHold? hold)
    {
        if (hold is not null && hold.To > hold.From) holds.Add(hold);
    }

    private static TrackHold? HoldBetween(Schedule schedule, ScheduledTrainPart arriving, ScheduledTrainPart leaving)
    {
        // A broken joint says nothing about where the vehicle stands.
        if (!arriving.To.OperationLocation.Equals(leaving.From.OperationLocation)) return null;
        // A vehicle can only link two trains that run on a common session.
        if (!arriving.Train.Sessions.Overlaps(leaving.Train.Sessions)) return null;
        var sessions = arriving.Train.Sessions.And(leaving.Train.Sessions);
        if (arriving.ArrivalLayover != TractionLayover.None || leaving.DepartureLayover != TractionLayover.None) return null;

        var track = arriving.OtherToTrack ?? arriving.To.Track;
        var from = track.Equals(arriving.To.Track) ? arriving.To.Departure : arriving.To.Arrival;
        // Up to where the next train's own window begins if the vehicles go with it from this very track;
        // otherwise they are fetched, and are on the track until the train departs.
        var to = leaving.OtherFromTrack is null && track.Equals(leaving.From.Track) ? leaving.From.Arrival : leaving.From.Departure;
        return new TrackHold(track, from, to, sessions, schedule, arriving.Train) { DepartingTrain = leaving.Train };
    }

    private static TrackHold? LastHold(Schedule schedule, ScheduledTrainPart last)
    {
        if (last.ArrivalLayover != TractionLayover.None) return null;
        var track = last.OtherToTrack ?? last.To.Track;
        var from = track.Equals(last.To.Track) ? last.To.Departure : last.To.Arrival;
        return new TrackHold(track, from, EndOfDay(from), last.Train.Sessions, schedule, last.Train);
    }

    private static TrackHold? FirstHold(Schedule schedule, ScheduledTrainPart first)
    {
        if (first.DepartureLayover != TractionLayover.None) return null;
        var track = first.OtherFromTrack ?? first.From.Track;
        var to = track.Equals(first.From.Track) ? first.From.Arrival : first.From.Departure;
        return new TrackHold(track, StartOfDay(to), to, first.Train.Sessions, schedule, first.Train) { DepartingTrain = first.Train };
    }

    // The day a time is stored on: times past midnight are on the day after.
    private static int DayOf(Time time) => (int)Math.Floor(time.Value.TotalDays);

    private static Time StartOfDay(Time time) => Time.FromDayHourMinute(DayOf(time), 0, 0);

    private static Time EndOfDay(Time time) => Time.FromDayHourMinute(DayOf(time) + 1, 0, 0);
}
