using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Provides validation extension methods for schedules, timetables, trains, station tracks and station calls.
/// </summary>
public static class ValidationExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Validates a complete schedule, including its timetable, vehicle schedules and locomotive coverage.
        /// </summary>
        /// <param name="options">The options controlling which validations run.</param>
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> GetValidationErrors(ValidationSettings options)
        {
            plan = plan.ValueOrException(nameof(plan));
            options = options.ValueOrException(nameof(options));
            var result = new List<ValidationError>();
            result.AddRange(plan.GetTimetableValidationErrors(options));
            result.AddRange(plan.ValidateLockKeys());
            if (options.ValidateSchedules) result.AddRange(plan.Schedules.SelectMany(l => l.ValidateOverlappingParts()));
            if (options.ValidateSchedules) result.AddRange(plan.Schedules.SelectMany(l => l.ValidateContiguity()));
            if (options.ValidateSchedules) result.AddRange(plan.ValidateTractionCoverage());
            if (options.ValidateSchedules) result.AddRange(plan.ValidateVehicleClosure());
            if (options.ValidateSchedules) result.AddRange(plan.ValidateVehicleDoubleBooking());
            if (options.ValidateSchedules) result.AddRange(plan.ValidateVehicleIdentities());
            if (options.ValidateDriverDuties) result.AddRange(plan.ValidateDriverDuties());
            if (options.ValidateDriverDuties) result.AddRange(plan.ValidateDriverDutyCoverage());
            if (options.ValidateLocomotiveCoverage) result.AddRange(plan.ValidateLocomotiveCoverage());
            return result;
        }
        /// <summary>
        /// Validates a timetable within its schedule (station tracks, station calls, stretches and train speed).
        /// </summary>
        /// <param name="options">The options controlling which validations run.</param>
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> GetTimetableValidationErrors(ValidationSettings options)
        {
            options = options.ValueOrException(nameof(options));
            var result = new List<ValidationError>();
            var timetable = plan.Timetable;
            result.AddRange(timetable.EnsureStationHasTrack());
            result.AddRange(timetable.Trains.SelectMany(t => t.CheckTrainTimeSequence()));
            if (options.ValidateRouteContinuity) result.AddRange(timetable.Trains.SelectMany(t => t.CheckRouteContinuity()));
            if (options.ValidateTrainNumbers) result.AddRange(timetable.ValidateTrainNumbers());
            if (options.ValidateStationTracks) result.AddRange(timetable.Stations().SelectMany(s => s.Tracks).SelectMany(t => t.GetValidationErrors(plan.Schedules, options.ExtendTrackOccupancyByVehicleStay, options.MinMinutesBetweenTrackUsage)));
            if (options.ValidateStationCalls) result.AddRange(timetable.Stations().SelectMany(s => s.Calls()).SelectMany(c => c.GetValidationErrors()));
            if (options.ValidateStretches) result.AddRange(timetable.Layout.TrackStretches.SelectMany(ss => ss.GetConflictingTrains()).Distinct());
            if (options.ValidateTrainSpeed) result.AddRange(timetable.CheckTrainSpeed(options.MinTrainSpeedMetersPerClockMinute, options.MaxTrainSpeedMetersPerClockMinute));
            return result;
        }

        /// <summary>
        /// Validates the lock keys the layout's operation locations carry (rule L4): a key is in force
        /// only where the location still needs one and the station holding it is still manned. A key the
        /// manning has left meaningless is kept but ignored, and reported here — silently dropping the
        /// notes it produced would leave the planner wondering where they went.
        /// </summary>
        /// <remarks>
        /// Always run, like the other checks for a model that contradicts itself: this is not a planning
        /// preference to be switched off, and a key nobody can fetch is a fault in the layout however the
        /// plan is being validated.
        /// </remarks>
        internal IEnumerable<ValidationError> ValidateLockKeys()
        {
            foreach (var location in plan.Layout.OperationLocations)
            {
                if (location.LockKey is not { HeldAt: { } holder }) continue;
                var message = location.LockKeyFault switch
                {
                    LockKeyFault.LocationIsManned => Strings.LockKeyIgnoredAtMannedLocation,
                    LockKeyFault.LocationExchangesNoCargo => Strings.LockKeyIgnoredWithoutCargoExchange,
                    LockKeyFault.HolderIsNotManned => Strings.LockKeyIgnoredWhenHolderIsNotManned,
                    _ => null,
                };
                if (message is null) continue;
                yield return ValidationError.LockKeyIgnored(Message.Warning(message, location, holder));
            }
        }

        /// <summary>
        /// Validates that no train is assigned two traction units over the same stretch of its run
        /// (rule P4).
        /// </summary>
        /// <remarks>
        /// Coverage <em>gaps</em> are not checked here: <see cref="ValidateTractionCoverage"/> (S4) judges
        /// the same thing per leg and per session, and correctly allows a traction change at a station,
        /// so a second time-based gap check would only report each gap twice.
        /// </remarks>
        internal IEnumerable<ValidationError> ValidateLocomotiveCoverage()
        {
            var errors = new List<ValidationError>();

            // The workings traction is booked on, each with the sessions it is booked for: the union over
            // the schedule's traction assignments. Grouped by schedule, so a schedule two locomotives
            // share counts once — a double-headed working is one claim on the train, not two — and so
            // that a schedule worked by one locomotive on the odd sessions and another on the even is
            // booked, as it reads, for both.
            var tractionSchedules = plan.ScheduledObjects
                .Where(v => v.IsTraction)
                .SelectMany(v => v.ScheduleAssignments)
                .Where(a => a.Schedule is not null)
                .GroupBy(a => a.Schedule)
                .Select(g => (Schedule: g.Key, Sessions: g.Aggregate(new Sessions(), (union, a) => union.Or(a.Sessions))))
                .ToList();

            foreach (var train in plan.Timetable.Trains)
            {
                // An on-demand train runs on no numbered session, so there is nothing to narrow the
                // bookings by; theirs stand on their own.
                var runsOnNumberedSessions = train.Sessions.Numbers.Length > 0;

                // Every part of this train that traction is booked on, with the sessions it is hauled on:
                // the booking narrowed to the sessions the train itself runs.
                // Match by Id, not value equality: several runs can share the same category and number
                // (e.g. a clock-face service), and value equality would merge their parts and emit the
                // same overlap warning once per run.
                // In the order the locomotive starts work on each part — the times an overlap message
                // shows — so a reported pair reads earliest first.
                var hauledParts = tractionSchedules
                    .SelectMany(s => s.Schedule.Parts
                        .Where(p => p.Train.Id == train.Id)
                        .Select(p => (Part: p, Sessions: runsOnNumberedSessions ? s.Sessions.And(train.Sessions) : s.Sessions)))
                    .OrderBy(x => x.Part.WorkingSpan.From)
                    .ToList();

                if (hauledParts.Count < 2) continue; // no traction at all, or one working: S4's concern

                errors.AddRange(CheckLocomotiveCoverageOverlaps(train, hauledParts));
            }

            return errors;
        }

        /// <summary>
        /// Validates that no vehicle is double-booked: assigned to two schedules that run on the same
        /// session/day AND overlap in clock time, so the vehicle would have to be in two places at once.
        /// Two schedules on the same day at different times (for example a morning turn and an afternoon
        /// turn) are a normal roster and are not a conflict.
        /// </summary>
        internal IEnumerable<ValidationError> ValidateVehicleDoubleBooking()
        {
            foreach (var vehicle in plan.ScheduledObjects)
            {
                var assignments = vehicle.ScheduleAssignments.ToArray();
                for (var i = 0; i < assignments.Length - 1; i++)
                {
                    for (var j = i + 1; j < assignments.Length; j++)
                    {
                        var a1 = assignments[i];
                        var a2 = assignments[j];

                        if (a1.Sessions.Overlaps(a2.Sessions) && SchedulesOverlapInTime(a1.Schedule, a2.Schedule))
                        {
                            var message = Message.Information(Strings.VehicleIsDoubleBooked, vehicle.Designation, a1.Sessions.SessionsNumbers, a2.Sessions.SessionsNumbers);
                            yield return ValidationError.VehicleDoubleBooked(vehicle, a1, a2, message);
                        }
                    }
                }
            }

            // Two schedules overlap in time when any part of one is worked at the same clock time as any
            // part of the other. Each part is taken over its WorkingSpan — running time plus the
            // preparation and finishing-up time at the train's ends — as everywhere else overlap is
            // judged.
            static bool SchedulesOverlapInTime(Schedule s1, Schedule s2)
            {
                var spans2 = s2.Parts.Select(p => p.WorkingSpan).ToArray();
                foreach (var p1 in s1.Parts)
                {
                    var span1 = p1.WorkingSpan;
                    foreach (var span2 in spans2)
                        if (span1.OverlapsInTime(span2)) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Validates that no two vehicles share a <see cref="VehicleIdentity"/> on a common session
        /// (rule P5). An identity names one physical vehicle — the external id a vehicle was imported
        /// under, or, for a vehicle carrying none, its operator and number (the number alone with no
        /// operator) — so on any one session it may belong to only one of the plan's vehicles.
        /// </summary>
        /// <remarks>
        /// The identity spans every vehicle type: a wagonset and a locomotive may not share one either, so
        /// the vehicles are grouped by identity alone. Two vehicles may reuse an identity as long as the
        /// sessions they work are strictly disjoint — the meeting where one is present and the other is
        /// not. A vehicle assigned nowhere claims every session (see <c>ClaimedSessions</c>), so an unused
        /// duplicate in the pool is reported too. Cargo flows are left out; they carry a synthesised
        /// identifier standing for a group of wagons, not a vehicle's identity.
        /// <para>
        /// Older plans were built before the rule existed, so this is where their duplicates surface. Each
        /// duplicate is reported <em>once</em>, against the first earlier vehicle of its identity it
        /// clashes with, rather than once per pair: a plan can hold many vehicles under one identity, and
        /// the pairs of such a group would bury every other conflict in the list. One error per vehicle
        /// still names every vehicle that has to be given an identity of its own.
        /// </para>
        /// </remarks>
        internal IEnumerable<ValidationError> ValidateVehicleIdentities()
        {
            var duplicates = plan.ScheduledObjects
                .Where(v => v.HasVehicleIdentity)
                .GroupBy(v => v.Identity)
                .Where(g => g.Count() > 1);
            foreach (var group in duplicates)
            {
                var vehicles = group.ToArray();
                for (var i = 1; i < vehicles.Length; i++)
                {
                    var vehicle = vehicles[i];
                    var claimed = vehicle.ClaimedSessions;
                    for (var j = 0; j < i; j++)
                    {
                        var overlap = claimed.And(vehicles[j].ClaimedSessions);
                        if (overlap.Flags == 0) continue;
                        // Two vehicles sharing an external id have the same Designation, so naming them
                        // both would say the same thing twice; the shared id and the sessions are the news.
                        var message = group.Key.IsExternalId
                            ? Message.Information(Strings.VehiclesShareExternalId,
                                vehicle.IdentityText, overlap.SessionsNumbers)
                            : Message.Information(Strings.VehiclesShareOperatorAndNumber,
                                vehicle.Designation, vehicles[j].Designation, vehicle.IdentityText, overlap.SessionsNumbers);
                        yield return ValidationError.VehicleIdentityDuplicated(vehicle, vehicles[j], message);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Validates driver duties: a train part may not be worked by two duties whose sessions overlap
        /// (the segment would need two drivers on a common session), and the parts within one duty may not
        /// overlap in time (a driver cannot be in two places at once). Also checks the identities that
        /// renumbering cannot fix and the staffing range.
        /// </summary>
        internal IEnumerable<ValidationError> ValidateDriverDuties()
        {
            var duties = plan.DriverDuties.ToArray();

            // A pinned identity renumbering cannot repair: empty, or shared with another pinned duty.
            // Both end as two booklets the pile cannot be sorted by, which is the one thing its number
            // is for.
            var pinned = duties.Where(d => d.IsExcludedFromRenumbering).ToArray();
            foreach (var duty in pinned.Where(d => string.IsNullOrWhiteSpace(d.Identity)))
            {
                var message = Message.Information(Strings.DutyHasNoIdentityToHold);
                yield return ValidationError.DutyIdentityMissing(duty, message);
            }
            for (var i = 0; i < pinned.Length - 1; i++)
            {
                for (var j = i + 1; j < pinned.Length; j++)
                {
                    var (d1, d2) = (pinned[i], pinned[j]);
                    if (string.IsNullOrWhiteSpace(d1.Identity)) continue;
                    if (!d1.Identity.Equals(d2.Identity, StringComparison.OrdinalIgnoreCase)) continue;
                    var message = Message.Information(Strings.DutyIdentityIsHeldByTwoDuties, d1.Identity);
                    yield return ValidationError.DutyIdentityDuplicated(d1, d2, message);
                }
            }

            // The editor offers 1–3 only, so this guards a hand-edited plan file.
            foreach (var duty in duties.Where(d => d.StaffCount is < 1 or > 3))
            {
                var message = Message.Information(Strings.DutyStaffCountIsOutOfRange, duty.Identity, duty.StaffCount);
                yield return ValidationError.DutyStaffCountOutOfRange(duty, message);
            }

            // A part shared by two duties whose sessions overlap.
            for (var i = 0; i < duties.Length - 1; i++)
            {
                for (var j = i + 1; j < duties.Length; j++)
                {
                    var d1 = duties[i];
                    var d2 = duties[j];
                    if (!d1.Sessions.Overlaps(d2.Sessions)) continue;
                    foreach (var part in d1.Parts.Where(p => d2.Parts.Contains(p)))
                    {
                        var message = Message.Information(Strings.DutyPartIsDoubleAssigned, part, d1.Identity, d2.Identity);
                        yield return ValidationError.DutyPartDoubleAssigned(d1, d2, part, message);
                    }
                }
            }

            // Two parts within one duty overlapping in time, counting the preparation and finishing-up
            // time at the trains' ends: a driver still making one train ready cannot be driving another.
            foreach (var duty in duties)
            {
                // In the order the driver starts work on each part — the times the message shows — so a
                // reported pair reads earliest first.
                var parts = duty.Parts.OrderBy(p => p.WorkingSpan.From).ToList();
                var spans = parts.Select(p => p.WorkingSpan).ToArray();
                for (var i = 0; i < parts.Count - 1; i++)
                {
                    for (var j = i + 1; j < parts.Count; j++)
                    {
                        var p1 = parts[i];
                        var p2 = parts[j];
                        if (spans[i].OverlapsInTime(spans[j]))
                        {
                            var message = Message.Information(Strings.DutyHasOverlappingParts, duty.Identity, p1.WorkingSpanText, p2.WorkingSpanText);
                            yield return ValidationError.DutyPartsOverlap(duty, p1, p2, message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates driver duty coverage: every train part hauled by a traction unit must also be
        /// covered by a driver duty on every session that traction assignment actually runs — otherwise
        /// nobody is rostered to drive it. A part with no traction assigned is out of scope here (that
        /// gap is <see cref="ValidateTractionCoverage"/>'s concern); a part with traction but only partial
        /// duty coverage is reported for exactly the sessions the duty gap leaves open.
        /// </summary>
        internal IEnumerable<ValidationError> ValidateDriverDutyCoverage()
        {
            var general = plan.Layout.Settings.General;
            var periodMax = Math.Clamp(general.MaxSessions, 1, general.UseDays ? 7 : 14);
            var errors = new List<ValidationError>();

            var tractionAssignments = plan.ScheduledObjects
                .Where(v => v.IsTraction)
                .SelectMany(v => v.ScheduleAssignments)
                .ToList();
            var duties = plan.DriverDuties.ToArray();

            // The sessions each traction-assigned part actually runs traction on: the assignment's
            // sessions, narrowed to the sessions the train itself runs. Several assignments (different
            // traction units taking over on different sessions) can cover the same part, so their
            // sessions are unioned.
            var tractionSessionsByPart = new Dictionary<ScheduledTrainPart, Sessions>();
            foreach (var assignment in tractionAssignments)
            {
                foreach (var part in assignment.Schedule?.Parts ?? [])
                {
                    var sessions = assignment.Sessions.And(part.Train.Sessions);
                    tractionSessionsByPart[part] = tractionSessionsByPart.TryGetValue(part, out var existing)
                        ? existing.Or(sessions)
                        : sessions;
                }
            }

            foreach (var (part, tractionSessions) in tractionSessionsByPart)
            {
                var dutySessions = duties
                    .Where(d => d.Parts.Contains(part))
                    .Aggregate(new Sessions(), (acc, d) => acc.Or(d.Sessions));

                var missing = new List<int>();
                for (var number = 1; number <= periodMax; number++)
                    if (tractionSessions.Includes(number) && !dutySessions.Includes(number)) missing.Add(number);

                if (missing.Count > 0)
                {
                    var missingSessions = SessionsExtensions.FromPeriodNumbers(missing, general.UseDays);
                    var message = Message.Information(Strings.TrainPartHasNoDriverDuty, part.Train, missingSessions.SessionsNumbers, part.From.OperationLocation, part.From.Departure.HHMM(), part.To.OperationLocation, part.To.Arrival.HHMM());
                    errors.Add(ValidationError.TrainPartMissingDriverDuty(part, message));
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates traction coverage (rule S4). Two things must hold:
        /// <list type="bullet">
        /// <item>A schedule (turnus) that runs regular sessions must have at least one vehicle assigned; an
        /// orphan working with no vehicle is reported.</item>
        /// <item>Every leg a train runs must be hauled by a traction unit (a locomotive or a self-propelled
        /// trainset) on <em>every</em> session the train runs. The traction may be assigned through any
        /// schedule that works the train — a wagonset has its own turnus with no traction of its own and is
        /// hauled by the locomotive's separate turnus, so coverage is judged per train, not per
        /// schedule.</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// Coverage is judged leg by leg, not by whether the train appears in some turnus at all: shortening
        /// a turnus part leaves the rest of the train unworked, and the train still has a part. The legs are
        /// the pairs of calls in run order (CallsInRunOrder) — insertion order would pair up calls the train
        /// does not run one after the other. Consecutive legs missing the same sessions are reported as one
        /// span, so a train with no traction at all gives one error over its whole run rather than one per
        /// leg. On-demand trains are exempt (they run only when needed), as are cargo flows.
        /// </remarks>
        internal IEnumerable<ValidationError> ValidateTractionCoverage()
        {
            var general = plan.Layout.Settings.General;
            var periodMax = Math.Clamp(general.MaxSessions, 1, general.UseDays ? 7 : 14);
            var errors = new List<ValidationError>();

            // A turnus that runs regular sessions but has no vehicle assigned at all is an orphan working.
            foreach (var schedule in plan.Schedules)
            {
                if (schedule.Parts.Count == 0) continue;
                if (schedule.IsCargoFlow) continue;
                if (schedule.Parts.All(p => p.Train.Sessions.IsOnDemand)) continue;
                if (schedule.Vehicles.Any()) continue;
                var message = Message.Information(Strings.VehicleScheduleHasNoVehicle, schedule.Number);
                errors.Add(ValidationError.ScheduleHasNoVehicle(schedule, message));
            }

            // Every leg a train runs must have a traction unit on every session it runs it, provided through
            // any schedule that works it.
            var tractionAssignments = plan.ScheduledObjects
                .Where(v => v.IsTraction)
                .SelectMany(v => v.ScheduleAssignments)
                .ToList();
            foreach (var train in plan.Timetable.Trains)
            {
                if (train.Calls.Count < 2) continue;
                if (train.Sessions.IsOnDemand) continue;

                var calls = train.CallsInRunOrder;
                var legCount = calls.Count - 1;

                // The sessions each leg is hauled on, unioned over every traction assignment whose schedule
                // has a part spanning that leg. Parts are matched to the train by Id, not value equality:
                // several runs can share category and number (a clock-face service), and value equality
                // would credit one run's traction to another's legs.
                var hauled = new Sessions[legCount];
                foreach (var assignment in tractionAssignments)
                {
                    foreach (var part in assignment.Schedule?.Parts ?? [])
                    {
                        if (part.Train.Id != train.Id) continue;
                        var from = IndexOfCall(calls, part.From);
                        var to = IndexOfCall(calls, part.To);
                        if (from < 0 || to < 0) continue;
                        for (var leg = from; leg < to; leg++) hauled[leg] = hauled[leg].Or(assignment.Sessions);
                    }
                }

                var missingPerLeg = new List<int>[legCount];
                for (var leg = 0; leg < legCount; leg++)
                {
                    missingPerLeg[leg] = [];
                    // Two successive calls at the same operating location travel no stretch — a train
                    // changing track there — so nothing has to haul it, exactly as CheckRouteContinuity
                    // does not call that pair a gap in the route.
                    if (calls[leg].OperationLocation.Equals(calls[leg + 1].OperationLocation)) continue;
                    for (var number = 1; number <= periodMax; number++)
                        if (train.Sessions.Includes(number) && !hauled[leg].Includes(number)) missingPerLeg[leg].Add(number);
                }

                for (var leg = 0; leg < legCount;)
                {
                    if (missingPerLeg[leg].Count == 0) { leg++; continue; }

                    // Run on over the following legs missing exactly the same sessions, so an unworked
                    // stretch is one error naming where it starts and ends.
                    var last = leg;
                    while (last + 1 < legCount && missingPerLeg[last + 1].SequenceEqual(missingPerLeg[leg])) last++;

                    var (from, to) = (calls[leg], calls[last + 1]);
                    var missingSessions = SessionsExtensions.FromPeriodNumbers(missingPerLeg[leg], general.UseDays);
                    var message = Message.Information(Strings.TrainMissingTraction,
                        train, from.OperationLocation, from.Departure.HHMM(), to.OperationLocation, to.Arrival.HHMM(), missingSessions.SessionsNumbers);
                    errors.Add(ValidationError.TrainMissingTraction(train, from, to, message));
                    leg = last + 1;
                }
            }
            return errors;

            static int IndexOfCall(IReadOnlyList<StationCall> calls, StationCall call)
            {
                for (var i = 0; i < calls.Count; i++) if (calls[i].Equals(call)) return i;
                return -1;
            }
        }

        /// <summary>
        /// Validates vehicle circulation closure (rules S3 + S5): over the whole operating period a
        /// vehicle's movements must balance at every station — it must depart each station as often
        /// as it arrives there — so the layout's vehicle distribution repeats and the working can run
        /// again. Movements are counted per session worked, so a unit that runs a leg on more sessions than
        /// it runs the return is caught. A unit that works both a forward and a return leg is closed and is
        /// not reported, even when the legs run on different sessions and even when they are split across
        /// several schedules (the rotation case). Applies to traction units, wagonsets and cargo-only units,
        /// each of which turns on its own working. On-demand-only units and cargo flows (freight directed by
        /// waybills, not a turning vehicle) are exempt.
        /// </summary>
        internal IEnumerable<ValidationError> ValidateVehicleClosure()
        {
            var general = plan.Layout.Settings.General;
            var periodMax = Math.Clamp(general.MaxSessions, 1, general.UseDays ? 7 : 14);
            var errors = new List<ValidationError>();
            foreach (var vehicle in plan.ScheduledObjects)
            {
                if (!(vehicle.IsTraction || vehicle.IsWagonSet || vehicle.IsCargoOnly)) continue;

                // Flow conservation over every session worked: +1 where the unit departs, -1 where it
                // arrives. A closed circulation nets to zero at every location.
                var net = new Dictionary<OperationLocation, int>();
                var departFrom = new Dictionary<OperationLocation, ScheduledTrainPart>();
                var arriveAt = new Dictionary<OperationLocation, ScheduledTrainPart>();
                var worksScheduled = false;
                for (var number = 1; number <= periodMax; number++)
                {
                    foreach (var assignment in vehicle.ScheduleAssignments.Where(a => a.Sessions.Includes(number)))
                    {
                        foreach (var part in assignment.Schedule?.Parts ?? [])
                        {
                            if (!part.Train.Sessions.Includes(number)) continue;
                            worksScheduled = true;
                            var from = part.From.OperationLocation;
                            var to = part.To.OperationLocation;
                            net[from] = net.GetValueOrDefault(from) + 1;
                            net[to] = net.GetValueOrDefault(to) - 1;
                            departFrom.TryAdd(from, part);
                            arriveAt.TryAdd(to, part);
                        }
                    }
                }
                if (!worksScheduled) continue; // no regular working (e.g. on-demand only)

                var start = net.FirstOrDefault(kv => kv.Value > 0).Key; // departs more often than it returns
                var end = net.FirstOrDefault(kv => kv.Value < 0).Key;   // arrives more often than it leaves
                if (start is null && end is null) continue;             // balanced: the circulation closes

                var fromPart = start is not null ? departFrom[start] : arriveAt[end!];
                var toPart = end is not null ? arriveAt[end] : departFrom[start!];
                var message = Message.Information(Strings.VehicleDoesNotReturnToStart, vehicle.Designation, (start ?? end)!, (end ?? start)!);
                errors.Add(ValidationError.VehicleNotClosed(vehicle, fromPart, toPart, message));
            }
            return errors;
        }
    }

    extension(Schedule schedule)
    {
        /// <summary>
        /// Validates that the schedule's parts are geographically contiguous (rule S2): each part, in
        /// working order, starts from the operation location where the previous part ended. Schedules
        /// built through <see cref="ScheduleExtensions.Append"/> are contiguous by construction; this
        /// catches schedules assembled unconditionally (e.g. reconstructed from an XPLN import).
        /// </summary>
        /// <remarks>
        /// A schedule whose parts overlap in time is not a single vehicle's working (for example two
        /// vehicles that an import merged under one identifier). Its overlap is already reported by S1
        /// (<see cref="ValidateOverlappingParts"/>) and ordering its parts to test contiguity would only
        /// produce misleading cascades, so such a schedule is skipped here.
        /// </remarks>
        private List<ValidationError> ValidateContiguity()
        {
            var errors = new List<ValidationError>();
            if (schedule.HasOverlappingParts()) return errors;
            var parts = schedule.OrderedParts;
            for (var i = 0; i < parts.Count - 1; i++)
            {
                var previous = parts[i];
                var next = parts[i + 1];
                if (!next.From.OperationLocation.Equals(previous.To.OperationLocation))
                {
                    var message = Message.Information(Strings.ScheduleIsNotContiguous, schedule.Number, next, previous.To.OperationLocation);
                    errors.Add(ValidationError.ScheduleNotContiguous(schedule, previous, next, message));
                }
            }
            return errors;
        }

        /// <summary>
        /// Determines whether any two of the schedule's parts overlap in time (one vehicle cannot be in
        /// two places at once). Used to gate the contiguity check (S2), which is meaningful only for a
        /// schedule that could be a single vehicle's working.
        /// </summary>
        private bool HasOverlappingParts()
        {
            var spans = schedule.Parts.Select(p => p.WorkingSpan).ToArray();
            for (var i = 0; i < spans.Length - 1; i++)
                for (var j = i + 1; j < spans.Length; j++)
                    if (spans[i].OverlapsInTime(spans[j]))
                        return true;
            return false;
        }

        /// <summary>
        /// Reports the schedule's parts that overlap in time (rule S1). Each part is taken over its
        /// <c>WorkingSpan</c>, so the preparation time at a train's origin and the finishing-up time at
        /// its destination count as occupied: the vehicle is being made ready or put away, and cannot at
        /// the same time be working another train.
        /// </summary>
        /// <remarks>
        /// The parts are taken in the order the vehicle starts work on them — the same times the message
        /// shows — so each reported pair reads earliest first and the overlap between the two is easy to
        /// see.
        /// </remarks>
        private List<ValidationError> ValidateOverlappingParts()
        {
            var errors = new List<ValidationError>();
            var parts = schedule.Parts.OrderBy(p => p.WorkingSpan.From).ToList();
            var spans = parts.Select(p => p.WorkingSpan).ToArray();
            for (var i = 0; i < parts.Count - 1; i++)
            {
                for (var j = i + 1; j < parts.Count; j++)
                {
                    var p1 = parts[i];
                    var p2 = parts[j];
                    if (spans[i].OverlapsInTime(spans[j]))
                    {
                        var message = Message.Information(string.Format(CultureInfo.CurrentCulture, Strings.VehicleScheduleContainsOverlappingTrainParts, schedule.Id, p1.WorkingSpanText, p2.WorkingSpanText));
                        errors.Add(ValidationError.VehicleScheduleOverlap(schedule, p1, p2, message));
                    }
                }
            }
            return errors;
        }
    }



    extension(Timetable timetable)
    {
        internal IEnumerable<ValidationError> EnsureStationHasTrack()
        {
            var result = new List<ValidationError>();
            foreach (var train in timetable.Trains)
            {
                foreach (var track in train.Tracks)
                {
                    if (!timetable.Layout.HasTrack(track))
                    {
                        var message = Message.Information(Strings.TrackInStationReferredInTrainIsNotInLayout, track, track.Station, train);
                        result.Add(ValidationError.MissingTrackReference(track, train, message));
                    }
                }
            }
            return result;
        }

        internal IEnumerable<ValidationError> CheckTrainSpeed(double minTrainSpeedMetersPerClockMinute, double maxTrainSpeedMetersPerClockMinute)
        {
            var result = new List<ValidationError>();
            foreach (var train in timetable.Trains)
            {
                result.AddRange(train.CheckTrainSpeed(minTrainSpeedMetersPerClockMinute, maxTrainSpeedMetersPerClockMinute));
            }
            return result;
        }

        /// <summary>
        /// Validates that trains sharing the same operating company, category and number run on
        /// non-overlapping sessions (rule T4), so that a given train identity is never scheduled to run
        /// twice at once. Trains that differ in company or category may reuse a number freely.
        /// </summary>
        internal IEnumerable<ValidationError> ValidateTrainNumbers()
        {
            var result = new List<ValidationError>();
            var duplicates = timetable.Trains
                .GroupBy(t => (Company: t.EffectiveCompany?.Id, t.CategoryId, t.Number))
                .Where(g => g.Count() > 1);
            foreach (var group in duplicates)
            {
                var trains = group.ToArray();
                for (var i = 0; i < trains.Length - 1; i++)
                    for (var j = i + 1; j < trains.Length; j++)
                    {
                        if (trains[i].Sessions.Overlaps(trains[j].Sessions))
                        {
                            var overlap = trains[i].Sessions.And(trains[j].Sessions);
                            var message = Message.Information(Strings.TrainsShareNumberOnOverlappingSessions, trains[i], trains[j], overlap.SessionsNumbers);
                            result.Add(ValidationError.DuplicateTrainNumber(trains[i], trains[j], message));
                        }
                    }
            }
            return result;
        }
    }

    extension(Train train)
    {
        /// <summary>
        /// Validates a single train's speed and call time sequence.
        /// </summary>
        /// <param name="options">The options controlling the speed thresholds.</param>
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> GetValidationErrors(ValidationSettings options)
        {
            train = train.ValueOrException(nameof(train));
            options = options.ValueOrException(nameof(options));
            List<ValidationError> result = [];
            result.AddRange(train.CheckTrainSpeed(options.MinTrainSpeedMetersPerClockMinute, options.MaxTrainSpeedMetersPerClockMinute));
            result.AddRange(train.CheckTrainTimeSequence());
            if (options.ValidateRouteContinuity) result.AddRange(train.CheckRouteContinuity());
            return result;
        }

        /// <summary>
        /// Checks that the train's route is continuous: every leg it runs — each pair of calls it runs one
        /// after the other — is a track stretch of the layout (rule T5).
        /// </summary>
        /// <remarks>
        /// A train travels a stretch by departing its start and arriving at its end, so it must call at both
        /// ends of every stretch on its way; a pair of successive calls with no stretch between them is a
        /// route that jumps a location, which no train can run. This is why a call in the middle of a route
        /// may not be deleted, only one at either end (see <c>DeletionRules.MayDelete(StationCall)</c>).
        /// <para>
        /// Two successive calls at the same operating location travel no stretch (a train changing track
        /// there, for instance), so they are not a gap. Connectivity is judged by whether any stretch joins
        /// the two locations, in either direction — a stretch is bidirectional.
        /// </para>
        /// </remarks>
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> CheckRouteContinuity()
        {
            var calls = train.CallsInRunOrder;
            if (calls.Count < 2) return [];
            // Connectivity is a question only the layout can answer; a train whose locations are not on one
            // (a fragment under construction) is left to the referential-integrity rules.
            if (train.Layout is not { } layout) return [];
            var result = new List<ValidationError>();
            for (var i = 0; i < calls.Count - 1; i++)
            {
                var from = calls[i];
                var to = calls[i + 1];
                if (from.OperationLocation.Equals(to.OperationLocation)) continue;
                if (layout.IsConnected(from.OperationLocation, to.OperationLocation)) continue;
                var message = Message.Information(Strings.TrainHasNoStretchBetweenCalls,
                    train, from.OperationLocation, from.Departure.HHMM(), to.OperationLocation, to.Arrival.HHMM());
                result.Add(ValidationError.TrainRouteNotConnected(from, to, message));
            }
            return result;
        }

        private IEnumerable<ValidationError> CheckLocomotiveCoverageOverlaps(List<(ScheduledTrainPart Part, Sessions Sessions)> hauledParts)
        {
            // Each part is taken over its WorkingSpan: the running time plus the preparation time at the
            // train's origin and the finishing-up time at its destination, which are as much a claim on
            // the locomotive as the run between them.
            var spans = hauledParts.Select(x => x.Part.WorkingSpan).ToArray();
            for (var i = 0; i < hauledParts.Count - 1; i++)
            {
                for (var j = i + 1; j < hauledParts.Count; j++)
                {
                    var (part1, sessions1) = hauledParts[i];
                    var (part2, sessions2) = hauledParts[j];

                    if (!spans[i].OverlapsInTime(spans[j])) continue;

                    // Two workings that share no session are a rotation, not a conflict: one locomotive
                    // takes the train on the odd sessions and another on the even, and the two are never
                    // at the meeting on the same day. Only where the sessions meet is the train hauled
                    // twice over, and then it is hauled twice over on exactly those sessions.
                    if (!sessions1.Overlaps(sessions2)) continue;
                    var shared = sessions1.And(sessions2);

                    // Named by their traction, not by their train: the train is {0} already, and the
                    // two parts can otherwise read alike down to the last minute, leaving the planner
                    // no way to see which locomotives are the doubled ones.
                    // The sessions are stated only where they are a subset of the ones the train runs.
                    // Doubled whenever it runs — the ordinary case, where both bookings are for every
                    // session — there is no subset to point at, and naming them all would only be noise.
                    var everySessionTheTrainRuns = shared.Numbers.SequenceEqual(train.Sessions.Numbers);
                    var message = everySessionTheTrainRuns
                        ? Message.Information(Strings.TrainHasLocomotiveCoverageOverlap,
                            train, part1.TractionWorkingSpanText, part2.TractionWorkingSpanText)
                        : Message.Information(Strings.TrainHasLocomotiveCoverageOverlapOnSessions,
                            train, part1.TractionWorkingSpanText, part2.TractionWorkingSpanText, shared.SessionsNumbers);
                    yield return ValidationError.LocomotiveCoverageOverlap(train, part1, part2, message);
                }
            }
        }

        // Every leg the train runs is checked, the last one included, and the legs are the pairs of calls in
        // run order (CallsInRunOrder) — insertion order would pair up calls the train does not run one after
        // the other.
        private List<ValidationError> CheckTrainSpeed(double minTrainSpeedMetersPerClockMinute, double maxTrainSpeedMetersPerClockMinute)
        {
            var result = new List<ValidationError>();
            var calls = train.CallsInRunOrder;
            // A train with no calls belongs to no layout, and so has no stretches to measure a speed on.
            if (train.Layout is not { } layout) return result;
            for (var i = 0; i < calls.Count - 1; i++)
            {
                var c1 = calls[i];
                var c2 = calls[i + 1];
                var maybeStretch = layout.TrackStretch(c1.OperationLocation, c2.OperationLocation);
                if (maybeStretch.HasValue)
                {
                    var time = c2.Arrival.Subtract(c1.Departure);
                    var length = maybeStretch.Value.Distance < 2 ? 2 : maybeStretch.Value.Distance;

                    var speed = time.TotalMinutes == 0 ? 0 : length / time.TotalMinutes;
                    if (speed == 0) continue;
                    if (speed < minTrainSpeedMetersPerClockMinute)
                    {
                        var message = Message.Information(Strings.TrainSpeedBetweenCallsIsTooSlow, c1.Train!, c1.OperationLocation, c1.Departure.HHMM(), c2.OperationLocation, c2.Arrival.HHMM(), length);
                        result.Add(ValidationError.TrainSpeed(c1, c2, isTooSlow: true, message));
                    }
                    if (speed > maxTrainSpeedMetersPerClockMinute)
                    {
                        var message = Message.Information(Strings.TrainSpeedBetweenCallsIsTooFast, c1.Train!, c1.OperationLocation, c1.Departure.HHMM(), c2.OperationLocation, c2.Arrival.HHMM(), length);
                        result.Add(ValidationError.TrainSpeed(c1, c2, isTooSlow: false, message));
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Checks that a train's station calls form a valid, non-decreasing time sequence.
        /// </summary>
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> CheckTrainTimeSequence()
        {
            var result = new List<ValidationError>();
            if (train.Calls.Count < 1)
            {
                var message = Message.Information(Strings.TrainMustHaveMinimumTwoCalls, train);
                result.Add(ValidationError.TrainTooFewCalls(train, message));
            }
            else
            {
                var conflicts = train.GetCallConflicts();
                if (conflicts.Count > 0)
                {
                    result.AddRange(conflicts.Select(c =>
                    {
                        var message = Message.Information(Strings.TrainHasCallsOverlappingInTime, train, c.one, c.another);
                        return ValidationError.TrainTimeSequence(c.one, c.another, message);
                    }));
                }
            }
            return result;
        }

        // Successive calls are the pairs in run order (CallsInRunOrder). Insertion order is not run order —
        // a call added last can be timed first — and pairing calls the train does not run one after the
        // other reports a conflict that is not one. What remains a conflict after ordering is a train whose
        // times contradict themselves, above all one that reaches the next location before it has left the
        // previous one.
        private List<(StationCall one, StationCall another)> GetCallConflicts()
        {
            var result = new List<(StationCall, StationCall)>();
            var calls = train.CallsInRunOrder;
            for (var i = 0; i < calls.Count - 1; i++)
            {
                var c1 = calls[i];
                var c2 = calls[i + 1];
                if (c1.Arrival > c2.Departure) result.Add((c1, c2));
                else if (c1.Arrival > c2.Arrival) result.Add((c1, c2));
                else if (c1.Departure > c2.Arrival) result.Add((c1, c2));
                else if (c1.Departure > c2.Departure) result.Add((c1, c2));
            }
            return result;
        }
    }

    extension(StationCall stationCall)
    {
        /// <summary>
        /// Validates a station call's timing (that arrival is not after departure).
        /// </summary>
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> GetValidationErrors()
        {
            stationCall = stationCall.ValueOrException(nameof(stationCall));
            var result = new List<ValidationError>();
            if (stationCall.Arrival > stationCall.Departure)
            {
                var message = Message.Information(Strings.ArrivalIsAfterDeparture, stationCall.Track.Station.Name, stationCall.Arrival.HHMM(), stationCall.Departure.HHMM());
                result.Add(ValidationError.StationCallTiming(stationCall, message));
            }
            return result;
        }

        internal List<(StationCall one, StationCall other)> GetConflictsWithRemaning(IEnumerable<StationCall> remaining, IEnumerable<Schedule> vehicleSchedules, bool extendByVehicleStay = false, int minMinutesBetweenTrackUsage = 0)
        {
            var result = new List<(StationCall, StationCall)>();
            // The schedules are still needed when the extension is off: they are what tells two calls of
            // the same vehicle apart from two vehicles contending for the track.
            var occupancySchedules = extendByVehicleStay ? vehicleSchedules : null;
            var mine = stationCall.TrackOccupancy(occupancySchedules);
            var conflictingWithMe = remaining.Where(r =>
                r.Track.Equals(stationCall.Track) &&
                !r.Train!.Equals(stationCall.Train) &&
                // Trains that never run on a common session are never there together, so they cannot
                // contend for the track — the same rule the stretch capacity check already applies.
                r.Train!.Sessions.Overlaps(stationCall.Train!.Sessions) &&
                r.TrackOccupancy(occupancySchedules).ConflictsInTime(mine, minMinutesBetweenTrackUsage) &&
                !vehicleSchedules.HasSameVehicle(r, stationCall)).ToList();
            result.AddRange(conflictingWithMe.Select(c => (stationCall, c)));
            if (remaining.Count() > 1) result.AddRange(remaining.First().GetConflictsWithRemaning(remaining.Skip(1), vehicleSchedules, extendByVehicleStay, minMinutesBetweenTrackUsage));
            return result;
        }

        /// <summary>
        /// Describes the span this call actually occupies its track for, rather than the call's own
        /// arrival and departure — the two differ when a traction unit staying on for the next train
        /// extends the occupancy (<see cref="TrackOccupancyExtensions.TrackOccupancy"/>), which is the
        /// span a reported conflict needs to show to make sense of a meet the calls' own times do not.
        /// </summary>
        internal string TrackOccupancySpanText(IEnumerable<Schedule>? vehicleSchedules)
        {
            var (from, to) = stationCall.TrackOccupancy(vehicleSchedules);
            return string.Format(CultureInfo.CurrentCulture, Strings.CallAtStationTrackOccupiedDuringTimes, stationCall.OperationLocation, stationCall.Track, from.HHMM(), to.HHMM());
        }
    }

    extension(StationTrack stationTrack)
    {
        /// <summary>
        /// Validates a station track for conflicting calls, using the schedule's vehicle schedules to tell whether conflicting calls share a vehicle.
        /// </summary>
        /// <param name="vehicleSchedules">The vehicle schedules used to determine whether conflicting calls share a vehicle.</param>
        /// <param name="extendOccupancyByVehicleStay">Whether a traction unit waiting between two trains
        /// counts as occupying the track; see <see cref="ValidationSettings.ExtendTrackOccupancyByVehicleStay"/>.</param>
        /// <param name="minMinutesBetweenTrackUsage">The free time the track needs between two occupancies,
        /// in fast-clock minutes; see <see cref="ValidationSettings.MinMinutesBetweenTrackUsage"/>. At zero
        /// only overlapping occupancies conflict.</param>
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> GetValidationErrors(IEnumerable<Schedule> vehicleSchedules, bool extendOccupancyByVehicleStay = false, int minMinutesBetweenTrackUsage = 0) =>
            stationTrack is null ? [] :
            stationTrack.GetConflicts(vehicleSchedules, extendOccupancyByVehicleStay, minMinutesBetweenTrackUsage).Select(c =>
            {
                // Same occupancy the conflict was detected with, not each call's own arrival/departure,
                // so a conflict caused by a traction unit's stay (rather than the calls' own times) still
                // shows the span that actually overlaps.
                var occupancySchedules = extendOccupancyByVehicleStay ? vehicleSchedules : null;
                // The call that takes the track first is named first, so the two spans in the message read
                // in time order and the overlap between them is easy to see.
                var (first, second) = c.one.TrackOccupancy(occupancySchedules).From <= c.another.TrackOccupancy(occupancySchedules).From ? (c.one, c.another) : (c.another, c.one);
                var firstSpan = first.TrackOccupancySpanText(occupancySchedules);
                var secondSpan = second.TrackOccupancySpanText(occupancySchedules);
                // Two occupancies that do not overlap are only in conflict because the required free time
                // between them is missing, and saying they overlap would then be plainly wrong: the times
                // in the message do not, and the planner needs to be told how short the gap actually is.
                var free = second.TrackOccupancy(occupancySchedules).FreeMinutesBetween(first.TrackOccupancy(occupancySchedules));
                var message = free < 0
                    ? Message.Information(Strings.CallAtStationOverlapsInTimeWithOtherCall, first.Train!, firstSpan, second.Train!, secondSpan)
                    : Message.Information(Strings.CallAtStationTooCloseInTimeToOtherCall, first.Train!, firstSpan, second.Train!, secondSpan, free, minMinutesBetweenTrackUsage);
                return ValidationError.StationTrackConflict(stationTrack, first, second, message);
            });

        private IEnumerable<(StationCall one, StationCall another)> GetConflicts(IEnumerable<Schedule> vehicleSchedules, bool extendOccupancyByVehicleStay, int minMinutesBetweenTrackUsage)
        {
            if (stationTrack.Calls.Count < 2) return [];
            var result = GetConflictsWithRemaning(stationTrack.Calls.First(), stationTrack.Calls.Skip(1), vehicleSchedules, extendOccupancyByVehicleStay, minMinutesBetweenTrackUsage);
            return result.Distinct();
        }

        internal (bool, IEnumerable<StationCall>?) GetConflicts2(StationCall call, IEnumerable<StationCall> withCalls, IEnumerable<Schedule> vehicleSchedules)
        {
            if (stationTrack.Calls.Count == 0) return (false, null);
            if (stationTrack.Calls.Count == 2)
            {
                if (stationTrack.Calls.First().OperationLocation.Equals(stationTrack.Calls.Last().OperationLocation))
                    return (false, null);
            }
            var conflictingCalls = withCalls
                .Where(c => !vehicleSchedules.HasSameVehicle(call, c) && (
                    (call.Departure > c.Arrival && call.Departure <= c.Departure) ||
                    (call.Arrival >= c.Arrival && call.Arrival < c.Departure)));
            if (conflictingCalls.Any())
                return (true, conflictingCalls);
            return (false, null);
        }
    }

    extension(TrackStretch trackStretch)
    {
        /// <summary>
        /// Finds trains that exceed the stretch's capacity: on a stretch with <c>TracksCount</c> tracks,
        /// up to that many trains may occupy it at once, so a conflict is the <c>(TracksCount + 1)</c>-th
        /// train sharing the stretch at the same time. Two trains only meet if they run on a common
        /// operating session — a train working sessions 1,3,5 never shares the stretch with one working
        /// 2,4,6 — so capacity is judged per session: the conflict is more than <c>TracksCount</c> trains
        /// present at one instant that all run one common session. A train occupies the stretch for the
        /// half-open interval from when it departs the first station until it arrives at the second, so a
        /// train arriving exactly as another departs (a meet at the station) does not count. Concurrency
        /// only rises when a train departs, so testing every departure instant catches every peak.
        /// </summary>
        internal IEnumerable<ValidationError> GetConflictingTrains()
        {
            var tracks = trackStretch.TracksCount;
            var passings = trackStretch.Passings.OrderBy(p => p.Departure.Value).ToArray();
            if (passings.Length <= tracks) return [];

            var result = new List<ValidationError>();
            var reported = new HashSet<(int, int)>();
            for (var q = 0; q < passings.Length; q++)
            {
                var entering = passings[q];
                var instant = entering.Departure;
                // The passings occupying the stretch at the instant this one departs, in departure order.
                var active = new List<int>();
                for (var k = 0; k < passings.Length; k++)
                    if (passings[k].Departure <= instant && passings[k].Arrival > instant) active.Add(k);

                for (var session = 1; session <= 14; session++)
                {
                    if (!entering.Train.Sessions.Includes(session)) continue;
                    var onSession = active.Where(k => passings[k].Train.Sessions.Includes(session)).ToList();
                    if (onSession.Count <= tracks) continue;

                    // Over capacity on this session. Report the earliest-departing other train meeting the
                    // one that tips the stretch over; the pair carries the span and trains for the GUI, and
                    // is reported once however many sessions or instants surface it.
                    var other = onSession.FirstOrDefault(k => passings[k].Train.Number != entering.Train.Number, -1);
                    if (other < 0) continue;
                    var key = other < q ? (other, q) : (q, other);
                    if (!reported.Add(key)) continue;

                    var first = passings[key.Item1];
                    var second = passings[key.Item2];
                    var message = Message.Information(Strings.TrainBetweenPassingOverlapsInTimeWithTrainBetweenPassing, first.Train.Identity, first.SpanText, second.Train.Identity, second.SpanText);
                    result.Add(ValidationError.StretchConflict(first.From.Track, first.To.Track, first, second, message));
                }
            }
            return result;
        }
    }

}

/// <summary>
/// Extension methods for converting ValidationError to strings for display.
/// </summary>
public static class ValidationErrorDisplayExtensions
{
    extension(IEnumerable<ValidationError> errors)
    {
        /// <summary>
        /// Converts a collection of validation errors to their message strings.
        /// </summary>
        public IEnumerable<string> ToStrings() =>
            errors.Select(e => e.Message.ToString());
    }

}
