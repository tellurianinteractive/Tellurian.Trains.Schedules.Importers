namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Interactive building blocks for the Schedules editor: creating empty vehicle
/// <see cref="Schedule">schedules</see>, finding the trains that can extend a schedule (whole or in
/// part), creating and assigning <see cref="ScheduledObject">vehicles</see>, and trimming a schedule.
/// </summary>
/// <remarks>
/// Appending a (possibly partial) train goes through the guarded <see cref="ScheduleExtensions.Append"/>
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
        /// the structural base is every schedulable train (a possible seed). For a non-empty schedule it is
        /// every train that calls at the schedule's end location — at or after its last arrival, with at
        /// least one later call — so a train may be joined <em>mid-run</em> (the case that lets one physical
        /// train be split across two schedules at, say, an electrification boundary). A train sharing none of
        /// the schedule's <c>EffectiveSessions</c> is left out, as it could never work the whole run.
        /// <para>
        /// Allocation is only filtered once a vehicle is assigned, because only then is the schedule's role
        /// known: a train fully worked by a locomotive is still free for a wagonset, so while the schedule
        /// has no vehicle the base list is returned whole. With a vehicle assigned, a train is left out when
        /// it cannot share a session with the vehicle, or when its continuation is <em>fully allocated</em> —
        /// already worked all the way to its last call, on every session this vehicle would work it, by other
        /// schedules whose vehicles share the assigned vehicle's <see cref="VehicleRole"/>. A train left
        /// unworked on some session (the complementary-schedule case) or with an unworked tail (the
        /// boundary-split case) stays a candidate. The overlap guard in
        /// <see cref="ScheduleExtensions.Append"/> keeps a single schedule consistent.
        /// </para>
        /// </summary>
        /// <param name="schedule">The schedule being built.</param>
        /// <returns>The candidate trains, ordered by the departure at which they join, then by number.</returns>
        public IReadOnlyList<Train> CandidateTrainsFor(Schedule schedule)
        {
            plan = plan.ValueOrException(nameof(plan));
            schedule = schedule.ValueOrException(nameof(schedule));

            // The structural base: (train, join-call) pairs that can continue the schedule. The join index is
            // 0 for an empty schedule (a seed may start anywhere) or the mid-run join call for a non-empty one.
            IEnumerable<(Train train, int index)> candidates =
                schedule.EndLocation is null || schedule.LastArrival is null
                    ? plan.SchedulableTrains().Select(train => (train, index: 0))
                    : plan.SchedulableTrains()
                        .Select(train => (train, index: schedule.JoinCallIndexFor(train)))
                        .Where(x => x.index is not null)
                        .Where(x => schedule.EffectiveSessions.Overlaps(x.train.Sessions))
                        .Select(x => (x.train, x.index!.Value));

            // Only with a vehicle assigned is the schedule's allocation role known; until then every
            // continuing train is offered (see remarks). With a vehicle we drop trains it shares no session
            // with and trains already fully worked, on those sessions, by other schedules of the same role.
            var roles = schedule.Vehicles.Select(v => v.Role).ToHashSet();
            if (roles.Count > 0)
            {
                var vehicleSessions = schedule.AssignedSessions;
                candidates = candidates.Where(x =>
                {
                    var want = schedule.EffectiveSessions.And(vehicleSessions).And(x.train.Sessions);
                    return want.Flags != 0 && !plan.IsContinuationFullyAllocated(schedule, x.train, x.index, roles, want);
                });
            }

            return
            [
                .. candidates
                    .OrderBy(x => x.train.CallsInRunOrder[x.index].Departure.Value)
                    .ThenBy(x => x.train.Number)
                    .Select(x => x.train)
            ];
        }

        /// <summary>
        /// Gets the trains that can be worked into the given <see cref="ScheduleJoint">joint</see> of a
        /// schedule: those with a run that fits the time the vehicle stands free there and that touches the
        /// joint (see <c>ScheduleJoint.FittingCallsFor</c>). A train that cannot share a session with the
        /// rest of the working is left out, as the vehicle could never work it.
        /// </summary>
        /// <remarks>
        /// This is the gap-filling counterpart of <see cref="CandidateTrainsFor"/>, which offers the trains
        /// that continue a working at its end. Here the window is closed at both ends, so a train is offered
        /// only for the run it could actually make in the time available; a run that leaves the working
        /// broken is still offered, since an out-and-back trip is worked in a leg at a time (see
        /// <see cref="ScheduleExtensions.Insert"/>).
        /// </remarks>
        /// <param name="schedule">The schedule being built.</param>
        /// <param name="joint">The joint of that schedule to fill.</param>
        /// <returns>The candidate trains, ordered by the departure they would leave the joint on, then by
        /// number.</returns>
        public IReadOnlyList<Train> CandidateTrainsInJoint(Schedule schedule, ScheduleJoint joint)
        {
            plan = plan.ValueOrException(nameof(plan));
            schedule = schedule.ValueOrException(nameof(schedule));
            joint = joint.ValueOrException(nameof(joint));

            return
            [
                .. plan.SchedulableTrains()
                    .Where(train => schedule.EffectiveSessions.Overlaps(train.Sessions))
                    .Select(train => (train, run: joint.FittingCallsFor(train)))
                    .Where(x => x.run is not null)
                    .OrderBy(x => x.train.CallsInRunOrder[x.run!.Value.From].Departure.Value)
                    .ThenBy(x => x.train.Number)
                    .Select(x => x.train)
            ];
        }

        // True when the continuation this schedule would take from the candidate train — from its join call
        // through to its last call — is already worked, on every session in want, by one or more OTHER
        // schedules whose vehicles share one of the given roles. Such a train is fully allocated for that
        // role: adding it here would double-book it. Coverage counts only same-role assignments' booked
        // sessions, so a train left unworked on some session — the complementary-schedule case — or with an
        // unworked tail — the boundary-split case — or worked only in another role, stays a candidate.
        private bool IsContinuationFullyAllocated(Schedule schedule, Train train, int joinIndex, IReadOnlySet<VehicleRole> roles, Sessions want)
        {
            if (want.Flags == 0) return false;
            var calls = train.CallsInRunOrder;
            var joinDeparture = calls[joinIndex].Departure;
            var lastArrival = calls[^1].Arrival;
            var covered = new Sessions();
            foreach (var other in plan.Schedules)
            {
                if (schedule.Equals(other)) continue;
                if (!other.Parts.Any(p => p.Train.Equals(train) && p.From.Departure <= joinDeparture && p.To.Arrival >= lastArrival))
                    continue;
                var roleSessions = plan.ScheduledObjects
                    .SelectMany(v => v.ScheduleAssignments)
                    .Where(a => other.Equals(a.Schedule) && roles.Contains(a.ScheduledObject.Role))
                    .Select(a => a.Sessions)
                    .Aggregate(new Sessions(), (union, s) => union.Or(s));
                covered = covered.Or(roleSessions);
            }
            return covered.Includes(want);
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
        /// A vehicle carrying no number falls back to its unique id (as the XPLN import does).
        /// </summary>
        /// <remarks>
        /// The vehicle is given no <see cref="ScheduledObject.ExternalId"/>: that is the identifier a
        /// vehicle was imported under, and the planner does not invent one. Its
        /// <see cref="ScheduledObject.Designation"/> is therefore composed from the operator signature,
        /// class and number, and it is the operator and number that identify it (see
        /// <see cref="VehicleIdentity"/>). Use <see cref="VehicleClaiming"/> first where the caller must
        /// not create a vehicle whose identity is already taken.
        /// </remarks>
        /// <param name="objectType">The kind of vehicle (locomotive, trainset, wagonset).</param>
        /// <param name="class">The vehicle class, e.g. "BR 218"; may be empty.</param>
        /// <param name="number">The vehicle number, or 0 to fall back to the vehicle's id.</param>
        /// <param name="company">The operating company, or <c>null</c>.</param>
        public ScheduledObject CreateVehicle(ScheduledObjectType objectType, string? @class, int number, Company? company)
        {
            plan = plan.ValueOrException(nameof(plan));
            var id = plan.NextScheduledObjectId();
            var vehicle = new ScheduledObject(id, objectType, number == 0 ? id : number)
            {
                Class = @class ?? string.Empty,
                Company = company,
                CompanyId = company?.Id,
            };
            plan.AddVehicle(vehicle);
            return vehicle;
        }

        /// <summary>
        /// Updates an existing <see cref="ScheduledObject">vehicle</see> in place: its type, external id,
        /// class, number and company. The <see cref="ScheduledObject.ExternalId"/> is taken as given (a
        /// blank value clears it, so the vehicle's displayed <see cref="ScheduledObject.Designation"/> then
        /// falls back to its company signature, class and number). A number of 0 falls back to the vehicle's
        /// id, matching <see cref="CreateVehicle"/>.
        /// </summary>
        /// <param name="vehicle">The vehicle to update.</param>
        /// <param name="objectType">The kind of vehicle (locomotive, trainset, wagonset).</param>
        /// <param name="externalId">The external id (identity/display override); blank clears it.</param>
        /// <param name="class">The vehicle class, e.g. "BR 218"; may be empty.</param>
        /// <param name="number">The vehicle number, or 0 to fall back to the vehicle's id.</param>
        /// <param name="company">The operating company, or <c>null</c>.</param>
        /// <param name="tractionType">The traction type; applied only to traction units (a non-traction
        /// vehicle's <see cref="ScheduledObject.TractionType"/> is always <see cref="TractionType.None"/>).
        /// <c>null</c> leaves it unchanged.</param>
        /// <param name="numberOfUnits">The number of units making up the vehicle — locomotives in a consist,
        /// cars in a multiple unit, or wagons in a wagonset. Values below 1 are clamped to 1. <c>null</c>
        /// leaves it unchanged.</param>
        /// <param name="isReversibleTrain">Whether the locomotive works a reversible train
        /// (<see cref="ScheduledObject.IsReversibleTrain"/>). Applied only to a locomotive — anything
        /// else is left without the flag, since only a locomotive can be spared the runaround by one.
        /// <c>null</c> leaves it unchanged.</param>
        /// <returns>The updated vehicle.</returns>
        public ScheduledObject UpdateVehicle(ScheduledObject vehicle, ScheduledObjectType objectType, string? externalId, string? @class, int number, Company? company, TractionType? tractionType = null, int? numberOfUnits = null, bool? isReversibleTrain = null)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            // Set the type first: its setter resets TractionType to None for non-traction vehicles and to
            // Undefined for traction ones, so any explicit tractionType below overrides that default.
            vehicle.ObjectType = objectType;
            vehicle.ExternalId = string.IsNullOrWhiteSpace(externalId) ? null : externalId.Trim();
            vehicle.Class = @class ?? string.Empty;
            vehicle.Number = number == 0 ? vehicle.Id : number;
            vehicle.Company = company;
            vehicle.CompanyId = company?.Id;
            if (tractionType is { } traction && vehicle.IsTraction) vehicle.TractionType = traction;
            if (numberOfUnits is { } units) vehicle.NumberOfUnits = Math.Max(1, units);
            // Only a locomotive is spared the runaround by working a reversible train; a trainset reverses
            // freely anyway and nothing else is traction, so neither keeps the flag.
            if (isReversibleTrain is { } reversible)
                vehicle.IsReversibleTrain = reversible && objectType == ScheduledObjectType.Locomotive;
            // The individual-wagon rake belongs only to a wagonset; clear it when the type is anything else.
            if (!objectType.IsWagonSet) vehicle.Units.Clear();
            return vehicle;
        }

        /// <summary>
        /// Finds the vehicle already in the pool that claims the given <see cref="VehicleIdentity"/> on at
        /// least one of the given sessions — the vehicle a new or edited vehicle with that identity would
        /// clash with. Returns <c>null</c> when the identity is free, which is what lets an editor refuse
        /// the edit before it is made rather than leaving the plan to be reported by the identity
        /// validation (rule P5).
        /// </summary>
        /// <remarks>
        /// The identity spans every vehicle type, so a wagonset is found when a locomotive is being given
        /// the same identity. Cargo flows are left out (see
        /// <see cref="ScheduledObjectExtensions.get_HasVehicleIdentity(ScheduledObject)"/>), and a vehicle
        /// assigned nowhere claims every session, so its identity is taken even before it is given work.
        /// </remarks>
        /// <param name="identity">The identity wanted.</param>
        /// <param name="sessions">The sessions the identity is wanted for.</param>
        /// <param name="excluding">A vehicle to disregard — the one being edited, which may of course keep
        /// its own identity.</param>
        public ScheduledObject? VehicleClaiming(VehicleIdentity identity, Sessions sessions, ScheduledObject? excluding = null)
        {
            plan = plan.ValueOrException(nameof(plan));
            return plan.ScheduledObjects.FirstOrDefault(vehicle =>
                vehicle.HasVehicleIdentity &&
                !ReferenceEquals(vehicle, excluding) &&
                vehicle.Identity == identity &&
                vehicle.ClaimedSessions.Overlaps(sessions));
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
        /// The index is a position in the train's calls <em>in run order</em>, as
        /// <see cref="TrainExtensions.AsTrainPart(Train, int, int)"/> takes them.
        /// </summary>
        public int? JoinCallIndexFor(Train train)
        {
            train = train.ValueOrException(nameof(train));
            if (schedule.EndLocation is not { } end || schedule.LastArrival is not { } lastArrival) return null;
            var calls = train.CallsInRunOrder;
            for (var i = 0; i < calls.Count - 1; i++)
            {
                var call = calls[i];
                if (call.OperationLocation.Equals(end) && !(call.Departure < lastArrival)) return i;
            }
            return null;
        }

        /// <summary>
        /// Changes the span of a part already in the schedule, adapting the neighbouring part it meets so
        /// the vehicle's working still hangs together, and returns what the edit did.
        /// </summary>
        /// <remarks>
        /// The part keeps its train and its identity — only its from- and to-calls change — so the driver
        /// duties working it follow the change. Where the edited part meets a neighbour today, the
        /// neighbour is adapted to the new joint: changing the start moves the previous part's arrival,
        /// changing the end moves the next part's departure (a neighbour may be extended as well as
        /// shortened). The adaptation is one step only and never reaches past the neighbour, whose own far
        /// end is untouched. It applies only when the neighbour's own train calls at the new joint
        /// location and only where the parts already met: a working that is broken at that joint (as an
        /// import may be) keeps its gap rather than being silently rewritten. Anything the edit leaves
        /// inconsistent — a gap the neighbour could not follow, or a joint the vehicle cannot make in time
        /// — is applied as asked and reported by the schedule validations (S1, S2).
        /// </remarks>
        /// <param name="part">The part to change; it must be in this schedule.</param>
        /// <param name="from">The call the part is to depart from.</param>
        /// <param name="to">The call the part is to arrive at.</param>
        /// <returns>A <see cref="Maybe{T}"/> with the applied <see cref="PartEdit"/>, or a message
        /// explaining why the edit was rejected.</returns>
        public Maybe<PartEdit> EditPart(ScheduledTrainPart part, StationCall from, StationCall to)
        {
            var planned = schedule.PlanPartEdit(part, from, to);
            if (planned.IsNone) return planned;
            var edit = planned.Value;
            edit.Part.SetSpan(edit.From, edit.To);
            if (edit is { Previous: { } previous, PreviousTo: { } previousTo }) previous.SetSpan(previous.From, previousTo);
            if (edit is { Next: { } next, NextFrom: { } nextFrom }) next.SetSpan(nextFrom, next.To);
            return planned;
        }

        /// <summary>
        /// Works out what <see cref="EditPart"/> would do, changing nothing. An editor uses it to show the
        /// resulting spans — and what the edit would leave inconsistent — before the planner commits.
        /// </summary>
        /// <param name="part">The part to change; it must be in this schedule.</param>
        /// <param name="from">The call the part is to depart from.</param>
        /// <param name="to">The call the part is to arrive at.</param>
        /// <returns>A <see cref="Maybe{T}"/> with the planned <see cref="PartEdit"/>, or a message
        /// explaining why the edit would be rejected.</returns>
        public Maybe<PartEdit> PlanPartEdit(ScheduledTrainPart part, StationCall from, StationCall to)
        {
            schedule = schedule.ValueOrException(nameof(schedule));
            part = part.ValueOrException(nameof(part));
            from = from.ValueOrException(nameof(from));
            to = to.ValueOrException(nameof(to));

            var parts = schedule.OrderedParts;
            // Identity, not equality: only the very object in the schedule can be edited in place, so that
            // the duties referencing it follow the change.
            var index = parts.TakeWhile(p => !ReferenceEquals(p, part)).Count();
            if (index == parts.Count)
                return new Maybe<PartEdit>($"Part {part} is not in schedule {schedule.Number}.");
            if (!part.Train.Calls.Contains(from) || !part.Train.Calls.Contains(to))
                return new Maybe<PartEdit>($"Both calls must be calls of train {part.Train}.");
            if (!(from.SortTime < to.SortTime))
                return new Maybe<PartEdit>($"A train part must run from {from} to a later call, but {to} is not later.");
            if (parts.Any(p => !ReferenceEquals(p, part) && p.From.Equals(from) && p.To.Equals(to)))
                return new Maybe<PartEdit>($"Schedule {schedule.Number} already works {part.Train} from {from} to {to}.");

            var previous = index > 0 ? parts[index - 1] : null;
            var next = index < parts.Count - 1 ? parts[index + 1] : null;
            return new Maybe<PartEdit>(new PartEdit(
                part, from, to,
                previous, AdaptedArrival(previous, part.From, from),
                next, AdaptedDeparture(next, part.To, to)));
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

    // The call the previous part is adapted to arrive at, so the vehicle hands over to the edited part at
    // its new start location: the latest call there the vehicle still reaches in time, or — when every such
    // call is too late — the earliest one, leaving the timing to be reported as a conflict. Null when there
    // is nothing to adapt: no previous part, a joint that is already broken (an existing gap is kept rather
    // than silently closed), an unchanged start location, or a train that does not call there after the
    // previous part's own start (adapting it would leave that part with no run at all).
    private static StationCall? AdaptedArrival(ScheduledTrainPart? previous, StationCall oldFrom, StationCall newFrom)
    {
        if (previous is null) return null;
        if (!previous.To.OperationLocation.Equals(oldFrom.OperationLocation)) return null;
        if (newFrom.OperationLocation.Equals(oldFrom.OperationLocation)) return null;
        var candidates = previous.Train.CallsInRunOrder
            .Where(c => c.SortTime > previous.From.SortTime && c.OperationLocation.Equals(newFrom.OperationLocation))
            .ToList();
        return candidates.LastOrDefault(c => !(c.Arrival > newFrom.Departure)) ?? candidates.FirstOrDefault();
    }

    // The mirror of AdaptedArrival: the call the next part is adapted to depart from, so the vehicle takes
    // it over where the edited part now ends. The earliest call there the vehicle can still catch, or — when
    // every such call is too early — the latest one. Null when there is nothing to adapt (see above).
    private static StationCall? AdaptedDeparture(ScheduledTrainPart? next, StationCall oldTo, StationCall newTo)
    {
        if (next is null) return null;
        if (!next.From.OperationLocation.Equals(oldTo.OperationLocation)) return null;
        if (newTo.OperationLocation.Equals(oldTo.OperationLocation)) return null;
        var candidates = next.Train.CallsInRunOrder
            .Where(c => c.SortTime < next.To.SortTime && c.OperationLocation.Equals(newTo.OperationLocation))
            .ToList();
        return candidates.FirstOrDefault(c => !(c.Departure < newTo.Arrival)) ?? candidates.LastOrDefault();
    }
}
