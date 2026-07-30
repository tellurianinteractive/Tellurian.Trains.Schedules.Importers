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
            if (options.ValidateSchedules) result.AddRange(plan.Schedules.SelectMany(l => l.ValidateOverlappingParts()));
            if (options.ValidateSchedules) result.AddRange(plan.Schedules.SelectMany(l => l.ValidateContiguity()));
            if (options.ValidateSchedules) result.AddRange(plan.ValidateTractionCoverage());
            if (options.ValidateSchedules) result.AddRange(plan.ValidateVehicleClosure());
            if (options.ValidateSchedules) result.AddRange(plan.ValidateVehicleDoubleBooking());
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
            if (options.ValidateTrainNumbers) result.AddRange(timetable.ValidateTrainNumbers());
            if (options.ValidateStationTracks) result.AddRange(timetable.Stations().SelectMany(s => s.Tracks).SelectMany(t => t.GetValidationErrors(plan.Schedules, options.ExtendTrackOccupancyByVehicleStay)));
            if (options.ValidateStationCalls) result.AddRange(timetable.Stations().SelectMany(s => s.Calls()).SelectMany(c => c.GetValidationErrors()));
            if (options.ValidateStretches) result.AddRange(timetable.Layout.TrackStretches.SelectMany(ss => ss.GetConflictingTrains()).Distinct());
            if (options.ValidateTrainSpeed) result.AddRange(timetable.CheckTrainSpeed(options.MinTrainSpeedMetersPerClockMinute, options.MaxTrainSpeedMetersPerClockMinute));
            return result;
        }

        /// <summary>
        /// Validates that all trains have complete locomotive coverage without gaps or overlaps.
        /// </summary>
        internal IEnumerable<ValidationError> ValidateLocomotiveCoverage()
        {
            var errors = new List<ValidationError>();

            // Get all traction vehicle schedules (locomotives and self-propelled railcars)
            var locomotiveSchedules = plan.ScheduledObjects
                .Where(v => v.ObjectType is ScheduledObjectType.Locomotive or ScheduledObjectType.Trainset)
                .SelectMany(v => v.ScheduleAssignments)
                .Select(a => a.Schedule)
                .Distinct()
                .ToList();

            // Group by train
            foreach (var train in plan.Timetable.Trains)
            {
                // Get all train parts for this specific train run from locomotive schedules.
                // Match by Id, not value equality: several runs can share the same category and number
                // (e.g. a clock-face service), and value equality would merge their parts and emit the
                // same gap/overlap warnings once per run.
                var locomotiveParts = locomotiveSchedules
                    .SelectMany(ls => ls.Parts)
                    .Where(p => p.Train.Id == train.Id)
                    .OrderBy(p => p.From.Departure.Value)
                    .ToList();

                if (locomotiveParts.Count == 0)
                {
                    // No locomotive assigned at all - report gap for entire train
                    if (train.Calls.Count >= 2)
                    {
                        var message = Message.Information(Strings.TrainHasNoLocomotiveAssigned, train);
                        errors.Add(ValidationError.LocomotiveCoverageGap(train, train.Calls.First(), train.Calls.Last(), message));
                    }
                    continue;
                }

                // Check for gaps
                errors.AddRange(CheckLocomotiveCoverageGaps(train, locomotiveParts));

                // Check for overlaps
                errors.AddRange(CheckLocomotiveCoverageOverlaps(train, locomotiveParts));
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

            // Two schedules overlap in time when any part of one runs at the same clock time as any part
            // of the other (the standard half-open interval overlap used throughout these validations).
            static bool SchedulesOverlapInTime(Schedule s1, Schedule s2)
            {
                foreach (var p1 in s1.Parts)
                    foreach (var p2 in s2.Parts)
                        if (p1.To.Arrival > p2.From.Departure && p1.From.Departure < p2.To.Arrival)
                            return true;
                return false;
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

            // Two parts within one duty overlapping in time.
            foreach (var duty in duties)
            {
                var parts = duty.OrderedParts;
                for (var i = 0; i < parts.Count - 1; i++)
                {
                    for (var j = i + 1; j < parts.Count; j++)
                    {
                        var p1 = parts[i];
                        var p2 = parts[j];
                        if (p1.To.Arrival > p2.From.Departure && p1.From.Departure < p2.To.Arrival)
                        {
                            var message = Message.Information(Strings.DutyHasOverlappingParts, duty.Identity, p1, p2);
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
        /// <item>Every train that operates must be hauled by a traction unit (a locomotive or a
        /// self-propelled trainset) on <em>every</em> session it runs. The traction may be assigned through
        /// any schedule that works the train — a wagonset has its own turnus with no traction of its own and
        /// is hauled by the locomotive's separate turnus, so coverage is judged per train, not per
        /// schedule.</item>
        /// </list>
        /// On-demand trains are exempt (they run only when needed), as are cargo flows.
        /// </summary>
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

            // Every train must have a traction unit on every session it runs, provided through any schedule
            // that works it.
            var tractionAssignments = plan.ScheduledObjects
                .Where(v => v.IsTraction)
                .SelectMany(v => v.ScheduleAssignments)
                .ToList();
            foreach (var train in plan.Timetable.Trains)
            {
                if (train.Calls.Count < 2) continue;
                if (train.Sessions.IsOnDemand) continue;

                var tractionSessions = tractionAssignments
                    .Where(a => a.Schedule is not null && a.Schedule.Parts.Any(p => p.Train.Equals(train)))
                    .Aggregate(new Sessions(), (acc, a) => acc.Or(a.Sessions));

                var missing = new List<int>();
                for (var number = 1; number <= periodMax; number++)
                    if (train.Sessions.Includes(number) && !tractionSessions.Includes(number)) missing.Add(number);

                if (missing.Count > 0)
                {
                    var missingSessions = SessionsExtensions.FromPeriodNumbers(missing, general.UseDays);
                    var message = Message.Information(Strings.TrainMissingTraction, train, missingSessions.SessionsNumbers);
                    errors.Add(ValidationError.TrainMissingTraction(train, message));
                }
            }
            return errors;
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
            var parts = schedule.Parts.ToArray();
            for (var i = 0; i < parts.Length - 1; i++)
                for (var j = i + 1; j < parts.Length; j++)
                    if (parts[i].To.Arrival > parts[j].From.Departure && parts[i].From.Departure < parts[j].To.Arrival)
                        return true;
            return false;
        }

        private List<ValidationError> ValidateOverlappingParts()
        {
            var errors = new List<ValidationError>();
            var parts = schedule.Parts.ToArray();
            for (var i = 0; i < parts.Length - 1; i++)
            {
                for (var j = i + 1; j < parts.Length; j++)
                {
                    var p1 = parts[i];
                    var p2 = parts[j];
                    if (p1.To.Arrival > p2.From.Departure && p1.From.Departure < p2.To.Arrival)
                    {
                        var message = Message.Information(string.Format(CultureInfo.CurrentCulture, Strings.VehicleScheduleContainsOverlappingTrainParts, schedule.Id, p1, p2));
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
            return result;
        }

        private IEnumerable<ValidationError> CheckLocomotiveCoverageGaps(List<ScheduledTrainPart> locomotiveParts)
        {
            var errors = new List<ValidationError>();
            var calls = train.Calls.ToArray();
            if (calls.Length < 2) yield break;

            // Check if first call is covered
            // Skip if at same station (locomotive may start later at same location)
            var firstCall = calls[0];
            var firstPart = locomotiveParts.FirstOrDefault();
            if (firstPart != null && firstPart.From.Departure > firstCall.Departure &&
                !firstCall.OperationLocation.Equals(firstPart.From.OperationLocation))
            {
                var message = Message.Information(Strings.TrainHasLocomotiveCoverageGap, train, firstCall.OperationLocation, firstPart.From.OperationLocation);
                errors.Add(ValidationError.LocomotiveCoverageGap(train, firstCall, firstPart.From, message));
            }

            // Check for gaps between consecutive locomotive parts
            for (var i = 0; i < locomotiveParts.Count - 1; i++)
            {
                var currentPart = locomotiveParts[i];
                var nextPart = locomotiveParts[i + 1];

                // There's a gap if the next part starts after the current part ends
                // BUT not if they're at the same station (locomotive change at same location is valid)
                if (nextPart.From.Departure > currentPart.To.Arrival &&
                    !currentPart.To.OperationLocation.Equals(nextPart.From.OperationLocation))
                {
                    var message = Message.Information(Strings.TrainHasLocomotiveCoverageGap, train, currentPart.To.OperationLocation, nextPart.From.OperationLocation);
                    errors.Add(ValidationError.LocomotiveCoverageGap(train, currentPart.To, nextPart.From, message));
                }
            }

            // Check if last call is covered
            // Skip if at same station (locomotive may end earlier at same location)
            var lastCall = calls[^1];
            var lastPart = locomotiveParts.LastOrDefault();
            if (lastPart != null && lastPart.To.Arrival < lastCall.Arrival &&
                !lastPart.To.OperationLocation.Equals(lastCall.OperationLocation))
            {
                var message = Message.Information(Strings.TrainHasLocomotiveCoverageGap, train, lastPart.To.OperationLocation, lastCall.OperationLocation);
                errors.Add(ValidationError.LocomotiveCoverageGap(train, lastPart.To, lastCall, message));
            }

            foreach (var error in errors) yield return error;
        }

        private IEnumerable<ValidationError> CheckLocomotiveCoverageOverlaps(List<ScheduledTrainPart> locomotiveParts)
        {
            for (var i = 0; i < locomotiveParts.Count - 1; i++)
            {
                for (var j = i + 1; j < locomotiveParts.Count; j++)
                {
                    var part1 = locomotiveParts[i];
                    var part2 = locomotiveParts[j];

                    // Check for overlap: part1 ends after part2 starts AND part1 starts before part2 ends
                    if (part1.To.Arrival > part2.From.Departure && part1.From.Departure < part2.To.Arrival)
                    {
                        var message = Message.Information(Strings.TrainHasLocomotiveCoverageOverlap, train, part1, part2);
                        yield return ValidationError.LocomotiveCoverageOverlap(train, part1, part2, message);
                    }
                }
            }
        }

        private List<ValidationError> CheckTrainSpeed(double minTrainSpeedMetersPerClockMinute, double maxTrainSpeedMetersPerClockMinute)
        {
            var result = new List<ValidationError>();
            var calls = train.Calls.ToArray();
            for (var i = 0; i < calls.Length - 2; i++)
            {
                var c1 = calls[i];
                var c2 = calls[i + 1];
                var maybeStretch = train.Layout.TrackStretch(c1.OperationLocation, c2.OperationLocation);
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

        private List<(StationCall one, StationCall another)> GetCallConflicts()
        {
            var result = new List<(StationCall, StationCall)>();
            if (train.Calls.Count == 2 && train.Calls.First().OperationLocation.Equals(train.Calls.Last().OperationLocation))
            {
                var c1 = train.Calls.First();
                var c2 = train.Calls.Last();
                if (c1.Arrival > c2.Departure) result.Add((c1, c2));
                else if (c1.Arrival > c2.Arrival) result.Add((c1, c2));
                else if (c1.Departure > c2.Arrival) result.Add((c1, c2));
                else if (c1.Departure > c2.Departure) result.Add((c1, c2));

                return result;
            }
            var calls = train.Calls.ToArray();

            for (var i = 0; i < calls.Length - 1; i++)
            {
                var c1 = calls[i];
                var c2 = calls[i + 1];
                if (c2 != null)
                {
                    if (c1.Arrival > c2.Departure) result.Add((c1, c2));
                    else if (c1.Arrival > c2.Arrival) result.Add((c1, c2));
                    else if (c1.Departure > c2.Arrival) result.Add((c1, c2));
                    else if (c1.Departure > c2.Departure) result.Add((c1, c2));
                }
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

        internal List<(StationCall one, StationCall other)> GetConflictsWithRemaning(IEnumerable<StationCall> remaining, IEnumerable<Schedule> vehicleSchedules, bool extendByVehicleStay = false)
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
                r.TrackOccupancy(occupancySchedules).OverlapsInTime(mine) &&
                !vehicleSchedules.HasSameVehicle(r, stationCall)).ToList();
            result.AddRange(conflictingWithMe.Select(c => (stationCall, c)));
            if (remaining.Count() > 1) result.AddRange(remaining.First().GetConflictsWithRemaning(remaining.Skip(1), vehicleSchedules, extendByVehicleStay));
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
        /// <returns>The validation errors found.</returns>
        public IEnumerable<ValidationError> GetValidationErrors(IEnumerable<Schedule> vehicleSchedules, bool extendOccupancyByVehicleStay = false) =>
            stationTrack is null ? [] :
            stationTrack.GetConflicts(vehicleSchedules, extendOccupancyByVehicleStay).Select(c =>
            {
                // Same occupancy the conflict was detected with, not each call's own arrival/departure,
                // so a conflict caused by a traction unit's stay (rather than the calls' own times) still
                // shows the span that actually overlaps.
                var occupancySchedules = extendOccupancyByVehicleStay ? vehicleSchedules : null;
                var oneSpan = c.one.TrackOccupancySpanText(occupancySchedules);
                var anotherSpan = c.another.TrackOccupancySpanText(occupancySchedules);
                var message = Message.Information(Strings.CallAtStationOverlapsInTimeWithOtherCall, c.one.Train!, oneSpan, c.another.Train!, anotherSpan);
                return ValidationError.StationTrackConflict(stationTrack, c.one, c.another, message);
            });

        private IEnumerable<(StationCall one, StationCall another)> GetConflicts(IEnumerable<Schedule> vehicleSchedules, bool extendOccupancyByVehicleStay)
        {
            if (stationTrack.Calls.Count < 2) return [];
            var result = GetConflictsWithRemaning(stationTrack.Calls.First(), stationTrack.Calls.Skip(1), vehicleSchedules, extendOccupancyByVehicleStay);
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
