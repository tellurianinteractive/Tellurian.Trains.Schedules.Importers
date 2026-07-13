namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Interactive building blocks for the Schedules editor: creating empty vehicle
/// <see cref="Schedule">schedules</see>, finding the trains that can extend a schedule (whole or in
/// part), creating and assigning <see cref="ScheduledObject">vehicles</see>, and trimming a schedule.
/// </summary>
/// <remarks>
/// Appending a (possibly partial) train goes through the guarded <see cref="VehicleScheduleExtensions.Append"/>
/// with <see cref="TrainExtensions.AsTrainPart(Train, int, int)"/>, which keeps a schedule a single
/// contiguous, non-overlapping working. Automatic building lives in
/// <see cref="PlanScheduleBuilderExtensions"/>; deletion lives in <c>DeletionRules</c>.
/// </remarks>
public static class ScheduleEditingExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Creates a new empty <see cref="Schedule"/>, adds it to the plan and returns it. Its
        /// <see cref="Schedule.Number"/> stays 0 until the first part is appended.
        /// </summary>
        public Schedule CreateSchedule()
        {
            plan = plan.ValueOrException(nameof(plan));
            var schedule = new Schedule(plan.NextScheduleId());
            plan.AddVehicleSchedule(schedule);
            return schedule;
        }

        /// <summary>
        /// Gets the trains that can extend the given schedule, whole or in part. For an empty schedule
        /// this is every schedulable train (a possible seed). For a non-empty schedule it is every train
        /// that calls at the schedule's end location — at or after its last arrival, with at least one
        /// later call — so a train may be joined <em>mid-run</em> (the case that lets one physical train be
        /// split across two schedules at, say, an electrification boundary). A train sharing none of the
        /// schedule's <c>EffectiveSessions</c> is left out, as it could
        /// never work the whole run. Assigned trains are not
        /// excluded: the same train may supply a part to more than one schedule; the overlap guard in
        /// <see cref="VehicleScheduleExtensions.Append"/> is what keeps a single schedule consistent.
        /// </summary>
        /// <param name="schedule">The schedule being built.</param>
        /// <returns>The candidate trains, ordered by the departure at which they join, then by number.</returns>
        public IReadOnlyList<Train> CandidateTrainsFor(Schedule schedule)
        {
            plan = plan.ValueOrException(nameof(plan));
            schedule = schedule.ValueOrException(nameof(schedule));
            if (schedule.EndLocation is not { } end || schedule.LastArrival is not { } lastArrival)
                return [.. plan.SchedulableTrains().OrderBy(t => t.Calls[0].Departure.Value).ThenBy(t => t.Number)];

            return
            [
                .. plan.SchedulableTrains()
                    .Select(train => (train, index: schedule.JoinCallIndexFor(train)))
                    .Where(x => x.index is not null)
                    .Where(x => schedule.EffectiveSessions.Overlaps(x.train.Sessions))
                    .OrderBy(x => x.train.Calls[x.index!.Value].Departure.Value)
                    .ThenBy(x => x.train.Number)
                    .Select(x => x.train)
            ];
        }

        /// <summary>
        /// Creates the schedule complementary to <paramref name="origin"/>: a new working for the sessions
        /// the origin does not cover. The origin operates on its
        /// <c>EffectiveSessions</c> (the sessions common to all its trains);
        /// the complement covers the remaining sessions of the layout's operating period
        /// (<see cref="GeneralSettings.MaxSessions"/>). It receives a copied part-reference for every origin
        /// part whose train also runs on one of those remaining sessions, in working order. The complement
        /// takes the origin's <see cref="Schedule.Number"/> (so it is listed just below the origin) and the
        /// origin's vehicle(s) are assigned to it for the leftover sessions, so the same vehicle works the
        /// origin on its sessions and the complement on the rest. The complement is added to the plan.
        /// Returns None when the origin is empty, already covers the whole period, or has no train running
        /// beyond it.
        /// </summary>
        /// <param name="origin">The schedule to complement.</param>
        /// <returns>A <see cref="Maybe{T}"/> with the new complementary schedule, or a message explaining
        /// why there is nothing to complement.</returns>
        public Maybe<Schedule> CreateComplementarySchedule(Schedule origin)
        {
            plan = plan.ValueOrException(nameof(plan));
            origin = origin.ValueOrException(nameof(origin));
            if (origin.Parts.Count == 0)
                return new Maybe<Schedule>("Cannot complement an empty schedule.");

            var general = plan.Layout.Settings.General;
            var complementSessions = origin.EffectiveSessions.ComplementWithin(general.UseDays, general.MaxSessions);
            if (complementSessions.Numbers.Length == 0)
                return new Maybe<Schedule>($"Schedule {origin.Number} already covers the whole operating period; nothing to complement.");

            var extending = origin.OrderedParts.Where(p => p.Train.Sessions.Overlaps(complementSessions)).ToList();
            if (extending.Count == 0)
                return new Maybe<Schedule>($"No train in schedule {origin.Number} runs beyond its operating sessions.");

            var complement = plan.CreateScheduleFrom(extending, origin.Number);
            plan.CopyVehicles(origin, complement, complementSessions);
            return new Maybe<Schedule>(complement);
        }

        /// <summary>
        /// Creates a copy of <paramref name="origin"/> and adds it to the plan: a new working with a copied
        /// part-reference for each of the origin's parts, in working order, referencing the same trains.
        /// It is a starting point for a variant working the planner then edits (add, remove or replace
        /// parts), rather than building an identical schedule from scratch. The clone takes the origin's
        /// <see cref="Schedule.Number"/> (so it is listed just below the origin) and the origin's vehicle(s)
        /// are assigned to it for the same sessions.
        /// </summary>
        /// <param name="origin">The schedule to clone.</param>
        /// <returns>The new cloned schedule (empty when the origin is empty).</returns>
        public Schedule CloneSchedule(Schedule origin)
        {
            plan = plan.ValueOrException(nameof(plan));
            origin = origin.ValueOrException(nameof(origin));
            var clone = plan.CreateScheduleFrom(origin.OrderedParts, origin.Number);
            plan.CopyVehicles(origin, clone, sessions: null);
            return clone;
        }

        // Creates a new schedule with the given number and fills it with copied part-references (same trains
        // and segments) in the given order, using the unguarded Add so a filtered, possibly non-contiguous
        // selection is kept intact (as the trusted XPLN reconstruction does). The number is taken from the
        // origin so the new schedule sorts just below it in the turn chart.
        private Schedule CreateScheduleFrom(IEnumerable<ScheduledTrainPart> parts, int number)
        {
            var schedule = plan.CreateSchedule();
            foreach (var part in parts)
                schedule.Add(new ScheduledTrainPart(part.From, part.To));
            schedule.Number = number;
            return schedule;
        }

        // Assigns each of the origin's vehicles to the target schedule too. When sessions is given every
        // vehicle is assigned to those sessions (the complement uses the origin's leftover sessions);
        // otherwise each vehicle keeps the sessions of its origin assignment (a plain clone).
        private void CopyVehicles(Schedule origin, Schedule target, Sessions? sessions)
        {
            foreach (var vehicle in origin.Vehicles.ToList())
            {
                var assignment = vehicle.ScheduleAssignments.FirstOrDefault(a => origin.Equals(a.Schedule));
                plan.AssignVehicle(target, vehicle, sessions ?? assignment?.Sessions);
            }
        }

        /// <summary>
        /// Creates a new <see cref="ScheduledObject">vehicle</see> and adds it to the plan's vehicle pool.
        /// A vehicle carrying no number falls back to its unique id (as the XPLN import does), and its
        /// external id — its identity everywhere in the app — is composed from the class and number.
        /// </summary>
        /// <param name="objectType">The kind of vehicle (locomotive, trainset, wagonset).</param>
        /// <param name="class">The vehicle class, e.g. "BR 218"; may be empty.</param>
        /// <param name="number">The vehicle number, or 0 to fall back to the vehicle's id.</param>
        /// <param name="company">The operating company, or <c>null</c>.</param>
        public ScheduledObject CreateVehicle(ScheduledObjectType objectType, string? @class, int number, Company? company)
        {
            plan = plan.ValueOrException(nameof(plan));
            var id = plan.NextScheduledObjectId();
            var vehicleNumber = number == 0 ? id : number;
            var cls = @class ?? string.Empty;
            var externalId = (string.IsNullOrWhiteSpace(cls) ? $"{objectType} {vehicleNumber}" : $"{cls} {vehicleNumber}").Trim();
            var vehicle = new ScheduledObject(id, objectType, vehicleNumber)
            {
                Class = cls,
                Company = company,
                CompanyId = company?.Id,
                ExternalId = externalId,
            };
            plan.AddVehicle(vehicle);
            return vehicle;
        }

        /// <summary>
        /// Assigns a <see cref="ScheduledObject">vehicle</see> to a <see cref="Schedule"/> for the given
        /// sessions (all sessions by default), creating the <see cref="ScheduleAssignment"/>. A vehicle
        /// already assigned to the schedule is returned unchanged rather than duplicated.
        /// </summary>
        /// <param name="schedule">The schedule to assign the vehicle to.</param>
        /// <param name="vehicle">The vehicle to assign.</param>
        /// <param name="sessions">The sessions the assignment applies to; defaults to all sessions.</param>
        /// <returns>The created (or already-present) assignment.</returns>
        public Maybe<ScheduleAssignment> AssignVehicle(Schedule schedule, ScheduledObject vehicle, Sessions? sessions = null)
        {
            plan = plan.ValueOrException(nameof(plan));
            schedule = schedule.ValueOrException(nameof(schedule));
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            if (vehicle.ScheduleAssignments.FirstOrDefault(a => schedule.Equals(a.Schedule)) is { } existing)
                return new Maybe<ScheduleAssignment>(existing);

            var assignment = new ScheduleAssignment(plan.NextScheduleAssignmentId(), vehicle, schedule, sessions ?? Sessions.All)
            {
                Number = schedule.Number,
            };
            vehicle.ScheduleAssignments.Add(assignment);
            return new Maybe<ScheduleAssignment>(assignment);
        }

        /// <summary>Trains that can take part in a schedule: those with a category and at least two calls.</summary>
        private IEnumerable<Train> SchedulableTrains() =>
            plan.Timetable.Trains.Where(t => t.Category is not null && t.Calls.Count >= 2);

        private int NextScheduleId() =>
            (plan.Schedules.Count == 0 ? 0 : plan.Schedules.Max(s => s.Id)) + 1;

        private int NextScheduledObjectId() =>
            (plan.ScheduledObjects.Count == 0 ? 0 : plan.ScheduledObjects.Max(v => v.Id)) + 1;

        private int NextScheduleAssignmentId() =>
            plan.ScheduledObjects.SelectMany(v => v.ScheduleAssignments).Select(a => a.Id).DefaultIfEmpty(0).Max() + 1;
    }

    extension(Schedule schedule)
    {
        /// <summary>
        /// Gets the index of the call at which the given train would join this (non-empty) schedule: its
        /// earliest call at the schedule's end location that departs at or after the schedule's last
        /// arrival and is followed by at least one later call. Returns <c>null</c> when the train cannot
        /// join, or when the schedule is empty (the caller then lets the planner choose the from-call).
        /// </summary>
        public int? JoinCallIndexFor(Train train)
        {
            train = train.ValueOrException(nameof(train));
            if (schedule.EndLocation is not { } end || schedule.LastArrival is not { } lastArrival) return null;
            for (var i = 0; i < train.Calls.Count - 1; i++)
            {
                var call = train.Calls[i];
                if (call.OperationLocation.Equals(end) && !(call.Departure < lastArrival)) return i;
            }
            return null;
        }

        /// <summary>
        /// Removes a single part from the schedule and detaches it, leaving every other part in place.
        /// A part not in the schedule is ignored. Removing a part from the middle of a working can leave a
        /// gap the planner then fills or trims; use <see cref="TruncateFrom"/> to drop the tail instead.
        /// </summary>
        /// <param name="part">The part to remove.</param>
        public void RemovePart(ScheduledTrainPart part)
        {
            schedule = schedule.ValueOrException(nameof(schedule));
            part = part.ValueOrException(nameof(part));
            if (!schedule.Parts.Remove(part)) return;
            part.Schedule = null;
            part.ScheduleId = null;
        }

        /// <summary>
        /// Removes the given part and every part worked after it, keeping the schedule a contiguous
        /// working. Detaches the removed parts from the schedule. A part not in the schedule is ignored.
        /// </summary>
        /// <param name="part">The first part to remove; later parts go with it.</param>
        public void TruncateFrom(ScheduledTrainPart part)
        {
            schedule = schedule.ValueOrException(nameof(schedule));
            part = part.ValueOrException(nameof(part));
            if (!schedule.Parts.Contains(part)) return;
            var from = part.From.Departure;
            foreach (var removed in schedule.Parts.Where(p => !(p.From.Departure < from)).ToList())
            {
                schedule.Parts.Remove(removed);
                removed.Schedule = null;
                removed.ScheduleId = null;
            }
        }
    }
}
