using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;
using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Provides validation extension methods for schedules, timetables, trains, station tracks and station calls.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates a complete schedule, including its timetable, vehicle schedules and locomotive coverage.
    /// </summary>
    /// <param name="schedule">The schedule to validate.</param>
    /// <param name="options">The options controlling which validations run.</param>
    /// <returns>The validation errors found.</returns>
    public static IEnumerable<ValidationError> GetValidationErrors(this Plan schedule, ValidationSettings options)
    {
        schedule = schedule.ValueOrException(nameof(schedule));
        options = options.ValueOrException(nameof(options));
        var result = new List<ValidationError>();
        result.AddRange(schedule.Timetable.GetValidationErrors(schedule, options));
        if (options.ValidateSchedules) result.AddRange(schedule.Schedules.SelectMany(l => l.ValidateOverlappingParts()));
        if (options.ValidateSchedules) result.AddRange(schedule.ValidateVehicleDoubleBooking());
        if (options.ValidateLocomotiveCoverage) result.AddRange(schedule.ValidateLocomotiveCoverage());
        return result;
    }

    /// <summary>
    /// Validates a timetable within its schedule (station tracks, station calls, stretches and train speed).
    /// </summary>
    /// <param name="timetable">The timetable to validate.</param>
    /// <param name="schedule">The schedule the timetable belongs to.</param>
    /// <param name="options">The options controlling which validations run.</param>
    /// <returns>The validation errors found.</returns>
    public static IEnumerable<ValidationError> GetValidationErrors(this Timetable timetable, Plan schedule, ValidationSettings options)
    {
        timetable = timetable.ValueOrException(nameof(timetable));
        options = options.ValueOrException(nameof(options));
        var result = new List<ValidationError>();
        result.AddRange(timetable.EnsureStationHasTrack());
        result.AddRange(timetable.Trains.SelectMany(t => t.CheckTrainTimeSequence()));
        if (options.ValidateStationTracks) result.AddRange(timetable.Stations().SelectMany(s => s.Tracks).SelectMany(t => t.GetValidationErrors(schedule.Schedules)));
        if (options.ValidateStationCalls) result.AddRange(timetable.Stations().SelectMany(s => s.Calls()).SelectMany(c => c.GetValidationErrors()));
        if (options.ValidateStretches) result.AddRange(timetable.Layout.TrackStretches.SelectMany(ss => ss.GetValidationErrors()).Distinct());
        if (options.ValidateTrainSpeed) result.AddRange(timetable.CheckTrainSpeed(options.MinTrainSpeedMetersPerClockMinute, options.MaxTrainSpeedMetersPerClockMinute));
        return result;
    }

    #region Station
    internal static IEnumerable<ValidationError> EnsureStationHasTrack(this Timetable me)
    {
        var result = new List<ValidationError>();
        foreach (var train in me.Trains)
        {
            foreach (var track in train.Tracks)
            {
                if (!me.Layout.HasTrack(track))
                {
                    var message = Message.Information(Strings.TrackInStationReferredInTrainIsNotInLayout, track, track.Station, train);
                    result.Add(ValidationError.MissingTrackReference(track, train, message));
                }
            }
        }
        return result;
    }

    #endregion

    #region StationTrack
    /// <summary>
    /// Validates a station track for conflicting calls, using the schedule's vehicle schedules to tell whether conflicting calls share a vehicle.
    /// </summary>
    /// <param name="me">The station track to validate.</param>
    /// <param name="vehicleSchedules">The vehicle schedules used to determine whether conflicting calls share a vehicle.</param>
    /// <returns>The validation errors found.</returns>
    public static IEnumerable<ValidationError> GetValidationErrors(this StationTrack me, IEnumerable<Schedule> vehicleSchedules) =>
        me is null ? [] :
        me.GetConflicts(vehicleSchedules).Select(c =>
        {
            var message = Message.Information(Strings.CallAtStationHasConflictsWithOtherCall, c.one.Train!, c.one, c.another.Train!, c.another);
            return ValidationError.StationTrackConflict(me, c.one, c.another, message);
        });

    private static IEnumerable<(StationCall one, StationCall another)> GetConflicts(this StationTrack me, IEnumerable<Schedule> vehicleSchedules)
    {
        if (me.Calls.Count < 2) return [];
        var result = GetConflicts(me.Calls.First(), me.Calls.Skip(1), vehicleSchedules);
        return result.Distinct();
    }

    private static List<(StationCall one, StationCall other)> GetConflicts(this StationCall me, IEnumerable<StationCall> remaining, IEnumerable<Schedule> vehicleSchedules)
    {
        var result = new List<(StationCall, StationCall)>();
        var conflictingWithMe = remaining.Where(r => r.Track.Equals(me.Track) && !r.Train!.Equals(me.Train) && r.Arrival > me.Departure && r.Departure < me.Arrival && !vehicleSchedules.HasSameVehicle(r, me)).ToList();
        result.AddRange(conflictingWithMe.Select(c => (me, c)));
        if (remaining.Count() > 1) result.AddRange(GetConflicts(remaining.First(), remaining.Skip(1), vehicleSchedules));
        return result;
    }

    internal static (bool, IEnumerable<StationCall>?) GetConflicts(this StationTrack me, StationCall call, IEnumerable<StationCall> withCalls, IEnumerable<Schedule> vehicleSchedules)
    {
        if (me.Calls.Count == 0) return (false, null);
        if (me.Calls.Count == 2)
        {
            if (me.Calls.First().Station.Equals(me.Calls.Last().Station))
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
    #endregion

    #region StationCall
    /// <summary>
    /// Validates a station call's timing (that arrival is not after departure).
    /// </summary>
    /// <param name="stationCall">The station call to validate.</param>
    /// <returns>The validation errors found.</returns>
    public static IEnumerable<ValidationError> GetValidationErrors(this StationCall stationCall)
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
    #endregion

    #region TrackStretch
    internal static IEnumerable<ValidationError> GetValidationErrors(this TrackStretch me)
    {
        var result = new List<ValidationError>();
        var passings = me.Passings.OrderBy(p => p.Departure.Value).ToArray();
        for (var i = 0; i < passings.Length - me.TracksCount; i++)
        {
            var first = passings[i];
            var second = passings[i + me.TracksCount];
            if (first.To.Train!.Number != second.To.Train!.Number && first.To.Arrival > second.From.Departure)
            {
                var message = Message.Information(Strings.TrainBetweenPassingIsConflictingWithTrainBetweenPassing, first.From.Train!.Number, first, second.To.Train!.Number, second);
                // Use the tracks from the station calls for the conflict location
                result.Add(ValidationError.StretchConflict(first.From.Track, first.To.Track, first, second, message));
            }
        }
        return result;
    }

    #endregion

    #region Train
    /// <summary>
    /// Validates a single train's speed and call time sequence.
    /// </summary>
    /// <param name="train">The train to validate.</param>
    /// <param name="options">The options controlling the speed thresholds.</param>
    /// <returns>The validation errors found.</returns>
    public static IEnumerable<ValidationError> GetValidationErrors(this Train train, ValidationSettings options)
    {
        train = train.ValueOrException(nameof(train));
        options = options.ValueOrException(nameof(options));
        var result = new List<ValidationError>();
        result.AddRange(train.CheckTrainSpeed(options.MinTrainSpeedMetersPerClockMinute, options.MaxTrainSpeedMetersPerClockMinute));
        result.AddRange(train.CheckTrainTimeSequence());
        return result;
    }

    internal static IEnumerable<ValidationError> CheckTrainSpeed(this Timetable me, double minTrainSpeedMetersPerClockMinute, double maxTrainSpeedMetersPerClockMinute)
    {
        var result = new List<ValidationError>();
        foreach (var train in me.Trains)
        {
            result.AddRange(train.CheckTrainSpeed(minTrainSpeedMetersPerClockMinute, maxTrainSpeedMetersPerClockMinute));
        }
        return result;
    }

    private static List<ValidationError> CheckTrainSpeed(this Train me, double minTrainSpeedMetersPerClockMinute, double maxTrainSpeedMetersPerClockMinute)
    {
        var result = new List<ValidationError>();
        var calls = me.Calls.ToArray();
        for (var i = 0; i < calls.Length - 2; i++)
        {
            var c1 = calls[i];
            var c2 = calls[i + 1];
            var maybeStretch = me.Layout.TrackStretch(c1.Station, c2.Station);
            if (maybeStretch.HasValue)
            {
                var time = c2.Arrival.Subtract(c1.Departure);
                var length = maybeStretch.Value.Distance < 2 ? 2 : maybeStretch.Value.Distance;

                var speed = time.TotalMinutes == 0 ? 0 : length / time.TotalMinutes;
                if (speed == 0) continue;
                if (speed < minTrainSpeedMetersPerClockMinute)
                {
                    var message = Message.Information(Strings.TrainSpeedBetweenCallsIsTooSlow, c1.Train!, c1.Station, c1.Departure.HHMM(), c2.Station, c2.Arrival.HHMM(), length);
                    result.Add(ValidationError.TrainSpeed(c1, c2, isTooSlow: true, message));
                }
                if (speed > maxTrainSpeedMetersPerClockMinute)
                {
                    var message = Message.Information(Strings.TrainSpeedBetweenCallsIsTooFast, c1.Train!, c1.Station, c1.Departure.HHMM(), c2.Station, c2.Arrival.HHMM(), length);
                    result.Add(ValidationError.TrainSpeed(c1, c2, isTooSlow: false, message));
                }
            }
        }
        return result;
    }

    /// <summary>
    /// Checks that a train's station calls form a valid, non-decreasing time sequence.
    /// </summary>
    /// <param name="me">The train to check.</param>
    /// <returns>The validation errors found.</returns>
    public static IEnumerable<ValidationError> CheckTrainTimeSequence(this Train me)
    {
        var result = new List<ValidationError>();
        if (me.Calls.Count < 1)
        {
            var message = Message.Information(Strings.TrainMustHaveMinimumTwoCalls, me);
            result.Add(ValidationError.TrainTooFewCalls(me, message));
        }
        else
        {
            var conflicts = me.GetConflicts();
            if (conflicts.Count > 0)
            {
                result.AddRange(conflicts.Select(c =>
                {
                    var message = Message.Information(Strings.TrainHasConflictingCalls, me, c.one, c.another);
                    return ValidationError.TrainTimeSequence(c.one, c.another, message);
                }));
            }
        }
        return result;
    }

    private static List<(StationCall one, StationCall another)> GetConflicts(this Train me)
    {
        var result = new List<(StationCall, StationCall)>();
        if (me.Calls.Count == 2 && me.Calls.First().Station.Equals(me.Calls.Last().Station))

        {
            var c1 = me.Calls.First();
            var c2 = me.Calls.Last();
            if (c1.Arrival > c2.Departure) result.Add((c1, c2));
            else if (c1.Arrival > c2.Arrival) result.Add((c1, c2));
            else if (c1.Departure > c2.Arrival) result.Add((c1, c2));
            else if (c1.Departure > c2.Departure) result.Add((c1, c2));

            return result;
        }
        var calls = me.Calls.ToArray();

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
    #endregion

    #region VehicleSchedule

    private static List<ValidationError> ValidateOverlappingParts(this Schedule me)
    {
        var errors = new List<ValidationError>();
        var parts = me.Parts.ToArray();
        for (var i = 0; i < parts.Length - 1; i++)
        {
            for (var j = i + 1; j < parts.Length; j++)
            {
                var p1 = parts[i];
                var p2 = parts[j];
                if (p1.To.Arrival > p2.From.Departure && p1.From.Departure < p2.To.Arrival)
                {
                    var message = Message.Information(string.Format(CultureInfo.CurrentCulture, Strings.VehicleScheduleContainsOverlappingTrainParts, me.Id, p1, p2));
                    errors.Add(ValidationError.VehicleScheduleOverlap(p1, p2, message));
                }
            }
        }
        return errors;
    }
    #endregion

    #region Locomotive Coverage

    /// <summary>
    /// Validates that all trains have complete locomotive coverage without gaps or overlaps.
    /// </summary>
    internal static IEnumerable<ValidationError> ValidateLocomotiveCoverage(this Plan schedule)
    {
        var errors = new List<ValidationError>();

        // Get all traction vehicle schedules (locomotives and self-propelled railcars)
        var locomotiveSchedules = schedule.ScheduledObjects
            .Where(v => v.ObjectType is ScheduledObjectType.Locomotive or ScheduledObjectType.Railcar)
            .SelectMany(v => v.ScheduleAssignments)
            .Select(a => a.Schedule)
            .Distinct()
            .ToList();

        // Group by train
        foreach (var train in schedule.Timetable.Trains)
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

    private static IEnumerable<ValidationError> CheckLocomotiveCoverageGaps(Train train, List<TrainPart> locomotiveParts)
    {
        var errors = new List<ValidationError>();
        var calls = train.Calls.ToArray();
        if (calls.Length < 2) yield break;

        // Check if first call is covered
        // Skip if at same station (locomotive may start later at same location)
        var firstCall = calls[0];
        var firstPart = locomotiveParts.FirstOrDefault();
        if (firstPart != null && firstPart.From.Departure > firstCall.Departure &&
            !firstCall.Station.Equals(firstPart.From.Station))
        {
            var message = Message.Information(Strings.TrainHasLocomotiveCoverageGap, train, firstCall.Station, firstPart.From.Station);
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
                !currentPart.To.Station.Equals(nextPart.From.Station))
            {
                var message = Message.Information(Strings.TrainHasLocomotiveCoverageGap, train, currentPart.To.Station, nextPart.From.Station);
                errors.Add(ValidationError.LocomotiveCoverageGap(train, currentPart.To, nextPart.From, message));
            }
        }

        // Check if last call is covered
        // Skip if at same station (locomotive may end earlier at same location)
        var lastCall = calls[^1];
        var lastPart = locomotiveParts.LastOrDefault();
        if (lastPart != null && lastPart.To.Arrival < lastCall.Arrival &&
            !lastPart.To.Station.Equals(lastCall.Station))
        {
            var message = Message.Information(Strings.TrainHasLocomotiveCoverageGap, train, lastPart.To.Station, lastCall.Station);
            errors.Add(ValidationError.LocomotiveCoverageGap(train, lastPart.To, lastCall, message));
        }

        foreach (var error in errors) yield return error;
    }

    private static IEnumerable<ValidationError> CheckLocomotiveCoverageOverlaps(Train train, List<TrainPart> locomotiveParts)
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
    #endregion

    #region Vehicle Double Booking

    /// <summary>
    /// Validates that no vehicle has overlapping schedule assignments (double-booked).
    /// </summary>
    internal static IEnumerable<ValidationError> ValidateVehicleDoubleBooking(this Plan schedule)
    {
        foreach (var vehicle in schedule.ScheduledObjects)
        {
            var assignments = vehicle.ScheduleAssignments.ToArray();
            for (var i = 0; i < assignments.Length - 1; i++)
            {
                for (var j = i + 1; j < assignments.Length; j++)
                {
                    var a1 = assignments[i];
                    var a2 = assignments[j];

                    if (a1.Sessions.Overlaps(a2.Sessions))
                    {
                        var message = Message.Information(Strings.VehicleIsDoubleBooked, vehicle.Number, a1.Sessions.SessionsNumbers, a2.Sessions.SessionsNumbers);
                        yield return ValidationError.VehicleDoubleBooked(vehicle, a1, a2, message);
                    }
                }
            }
        }
    }
    #endregion

    internal static bool HasSameVehicle(this IEnumerable<Schedule> me, StationCall one, StationCall another)
    {
        var foundOne = me.FindVehicleSchedule(one);
        var foundOther = me.FindVehicleSchedule(another);
        return foundOne is not null && foundOther is not null && foundOne == foundOther;
    }

    internal static Schedule? FindVehicleSchedule(this IEnumerable<Schedule> schedules, StationCall call)
    {
        if (schedules == null) return null;
        foreach (var schedule in schedules)
        {
            foreach (var part in schedule.Parts)
            {
                if (part.ContainsCall(call)) return schedule;
            }
        }
        return null;
    }

    internal static bool ContainsCall(this TrainPart me, StationCall call)
    {
        return me.Train == call.Train && me.From.Departure <= call.Departure && me.To.Arrival >= call.Arrival;
    }
}

/// <summary>
/// Extension methods for converting ValidationError to strings for display.
/// </summary>
public static class ValidationErrorDisplayExtensions
{
    /// <summary>
    /// Converts a collection of validation errors to their message strings.
    /// </summary>
    public static IEnumerable<string> ToStrings(this IEnumerable<ValidationError> errors) =>
        errors.Select(e => e.Message.ToString());
}
