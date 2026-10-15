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
        !IsIdle && !other.IsIdle && (
        (Train.Equals(other.Train)) ||
        (DepartingTrain is not null && DepartingTrain.Equals(other.DepartingTrain)) ||
        (DepartingTrain is not null && DepartingTrain.Equals(other.Train)) ||
        (other.DepartingTrain is not null && other.DepartingTrain.Equals(Train)));

    /// <summary>
    /// The vehicles of the schedule that this hold is about, when the schedule's vehicles work different
    /// sessions and so do not stand together. Empty when the schedule's vehicles all work the same ones.
    /// </summary>
    public IReadOnlyList<ScheduledObject> Group { get; init; } = [];

    /// <summary>
    /// Identifies the group of vehicles of the schedule that works the same sessions, so that two holds of
    /// one schedule are known to belong to the same vehicles or to different ones. Minus one when the
    /// schedule's vehicles are not told apart.
    /// </summary>
    public int GroupKey { get; init; } = -1;

    /// <summary>Whether the group has a traction unit; a locomotive and wagons working different sessions still stand together.</summary>
    public bool IsTractionGroup { get; init; }

    /// <summary>
    /// Whether the two holds are of the same vehicles of a schedule, which therefore cannot be in each
    /// other's way: they are of the same schedule and either the same group, or groups of different kinds
    /// (a locomotive and the wagons it works with, which stand together even if they are booked for
    /// different sessions).
    /// </summary>
    public bool IsOfSameVehiclesAs(TrackHold other) =>
        Schedule.Equals(other.Schedule) && (GroupKey == other.GroupKey || IsTractionGroup != other.IsTractionGroup);

    /// <summary>
    /// Whether the vehicles stand here because their schedule is not worked by them on these sessions
    /// (another vehicle works it), rather than between two trains of theirs. They stand where they were
    /// left, all day.
    /// </summary>
    public bool IsIdle { get; init; }

    /// <summary>
    /// For an idle hold: the sessions on which other vehicles work the schedule, and so on which the
    /// schedule's own trains may call on the track the idle vehicles stand on. <c>null</c> when none do.
    /// </summary>
    public Sessions? OtherVehicleSessions { get; init; }

    /// <summary>The time the vehicles stand on the track.</summary>
    public (Time From, Time To) Span => (From, To);

    /// <summary>The vehicles standing there, as the planner names them.</summary>
    public string Vehicles
    {
        get
        {
            var names = (Group.Count > 0 ? Group : [.. Schedule.Vehicles]).Select(v => v.Designation).ToList();
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
                var groups = VehicleGroups(schedule);
                // Vehicles that work different sessions of the schedule do not stand together: each group has
                // holds of its own, and where it is not working the schedule it stands idle.
                var told = groups.Count > 1 || groups.Any(g => g.Sessions is { } w && !schedule.IdleSessions(w).IsNone);
                return [.. groups.SelectMany(g => HoldsOf(schedule, g, groups, told))];
            }
        }

        /// <summary>
        /// The sessions of the schedule's trains that none of the vehicles in <paramref name="working"/>
        /// works, within the layout's operating period.
        /// </summary>
        internal Sessions IdleSessions(Sessions working)
        {
            var general = schedule.Plan?.Layout.Settings.General;
            var free = general is null ? new Sessions() : working.ComplementWithin(general.UseDays, general.MaxSessions);
            return free.And(schedule.EffectiveSessions);
        }
    }

    // The vehicles of a schedule, grouped by the sessions they are assigned to it for. A schedule whose
    // vehicles are not assigned (or which has no plan) is one group of no particular vehicles and sessions.
    private sealed record VehicleGroup(Sessions? Sessions, IReadOnlyList<ScheduledObject> Vehicles);

    private static List<VehicleGroup> VehicleGroups(Schedule schedule)
    {
        // Assignments may be written either as sessions or as days, so they are read within the operating period.
        var general = schedule.Plan?.Layout.Settings.General;
        var assigned = (schedule.Plan?.ScheduledObjects ?? [])
            .SelectMany(v => v.ScheduleAssignments.Where(a => schedule.Equals(a.Schedule))
                .Select(a => (Vehicle: v, Sessions: general is null ? a.Sessions : a.Sessions.WithinPeriod(general.UseDays, general.MaxSessions))))
            .ToList();
        if (assigned.Count == 0) return [new VehicleGroup(null, [])];
        return [.. assigned
            .GroupBy(x => x.Sessions.Flags)
            .Select(g => new VehicleGroup(g.First().Sessions, [.. g.Select(x => x.Vehicle)]))];
    }

    private static IEnumerable<TrackHold> HoldsOf(Schedule schedule, VehicleGroup group, List<VehicleGroup> groups, bool told)
    {
        // A shunting task without a traction unit takes no track (see StationCall.OccupiesTrack),
        // so vehicles that only appear in such a task do not stand anywhere because of it.
        var vehicles = group.Vehicles.Count > 0 ? group.Vehicles : [.. schedule.Vehicles];
        var parts = vehicles.Any(v => v.IsTraction)
            ? schedule.OrderedParts
            : [.. schedule.OrderedParts.Where(p => !p.Train.IsShuntingTask)];
        if (parts.Count == 0) return [];

        var tractionGroup = vehicles.Any(v => v.IsTraction);
        var key = told && group.Sessions is { } s ? s.Flags : -1;
        TrackHold? Mark(TrackHold? hold)
        {
            if (hold is null) return null;
            if (group.Sessions is not { } working) return hold;
            if (!hold.Sessions.Overlaps(working)) return null;
            return hold with { Sessions = hold.Sessions.And(working), Group = told ? group.Vehicles : [], GroupKey = key, IsTractionGroup = tractionGroup };
        }

        List<TrackHold> holds = [];
        var first = Mark(FirstHold(schedule, parts[0]));
        var last = Mark(LastHold(schedule, parts[^1]));
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
            Add(holds, Mark(HoldBetween(schedule, parts[i], parts[i + 1])));
        Add(holds, last);

        // Where the vehicles do not work the schedule they stay where it left them, all day.
        if (told && group.Sessions is { } working2 && (last ?? first) is { } resting)
        {
            // Vehicles that work another schedule on a session are away on it, not standing here.
            var idle = Sessions.FromBitPattern(schedule.IdleSessions(working2).Flags & ~BusyElsewhere(schedule, group.Vehicles).Flags);
            if (!idle.IsNone)
            {
                var others = groups.Where(g => g != group && g.Sessions is not null)
                    .Aggregate(new Sessions(), (union, g) => union.Or(g.Sessions!.Value)).And(idle);
                // Vehicles that take over in blocks of consecutive sessions (1-3, then 4-6) are swapped: the
                // ones leaving are implicitly taken off the tracks for the others to stand there. Only where
                // they alternate (1,3,5 and 2,4,6) do the idle ones stay put, a day at a time.
                if (!others.IsNone && groups.All(g => g.Sessions is not { } w || w.IsInBlocks(schedule.Plan?.Layout.Settings.General.MaxSessions ?? 14)))
                {
                    idle = Sessions.FromBitPattern(idle.Flags & ~others.Flags);
                    others = new Sessions();
                }
                if (idle.IsNone) return holds;
                var day = Time.FromDayHourMinute(0, 0, 0);
                holds.Add(new TrackHold(resting.Track, day, day.AddDays(1), idle, schedule, resting.Train)
                {
                    IsIdle = true,
                    Group = group.Vehicles,
                    GroupKey = key,
                    IsTractionGroup = tractionGroup,
                    OtherVehicleSessions = others.IsNone ? null : others,
                });
            }
        }
        return holds;
    }

    // The sessions on which every one of the vehicles works some other schedule than this one, so that none of
    // them is left standing where this schedule ended.
    private static Sessions BusyElsewhere(Schedule schedule, IReadOnlyList<ScheduledObject> vehicles)
    {
        if (vehicles.Count == 0) return new Sessions();
        // Assignments may be written either as sessions or as days, so compare them within the operating period.
        var general = schedule.Plan?.Layout.Settings.General;
        if (general is null) return new Sessions();
        var bits = vehicles
            .Select(v => v.ScheduleAssignments.Where(a => !schedule.Equals(a.Schedule))
                .Aggregate(0, (acc, a) => acc | a.Sessions.WithinPeriod(general.UseDays, general.MaxSessions).Flags))
            .Aggregate(-1, (acc, b) => acc & b);
        return Sessions.FromBitPattern(bits);
    }

    // Whether every session within the operating period is next to another one of the set, i.e. the set is
    // runs of two or more consecutive sessions rather than a pattern of single, alternating ones. Sessions
    // beyond the period never run, so they do not count.
    private static bool IsInBlocks(this Sessions sessions, int maxSessions)
    {
        var bits = sessions.Flags & ((1 << Math.Clamp(maxSessions, 1, 14)) - 1);
        return (bits & ((bits << 1) | (bits >> 1))) == bits;
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
