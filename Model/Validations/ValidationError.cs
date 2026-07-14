namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Represents a validation error with location and time information
/// for highlighting conflicts in a graphical timetable.
/// </summary>
/// <remarks>
/// If <see cref="FromTrack"/> and <see cref="ToTrack"/> are the same,
/// the conflict is at a station on that track.
/// If they are different, the conflict is on the track stretch between them.
/// </remarks>
public sealed record ValidationError
{
    /// <summary>
    /// The type of validation error.
    /// </summary>
    public required ValidationErrorType ErrorType { get; init; }

    /// <summary>
    /// The minimum departure time of objects involved in the conflict.
    /// </summary>
    public required Time FromTime { get; init; }

    /// <summary>
    /// The maximum arrival time of objects involved in the conflict.
    /// </summary>
    public required Time ToTime { get; init; }

    /// <summary>
    /// The station track where the conflict starts (or occurs if same as ToTrack).
    /// </summary>
    public required StationTrack FromTrack { get; init; }

    /// <summary>
    /// The station track where the conflict ends (or occurs if same as FromTrack).
    /// </summary>
    public required StationTrack ToTrack { get; init; }

    /// <summary>
    /// The trains involved in the conflict.
    /// </summary>
    public required IReadOnlyList<Train> Trains { get; init; }

    /// <summary>
    /// The localized message describing the error.
    /// </summary>
    public required Message Message { get; init; }

    /// <summary>
    /// True if the conflict is at a single station track.
    /// </summary>
    public bool IsStationConflict => FromTrack.Equals(ToTrack);

    /// <summary>
    /// True if the conflict spans a track stretch between stations.
    /// </summary>
    public bool IsStretchConflict => !FromTrack.Equals(ToTrack);

    /// <summary>
    /// The severity of the error, derived from its <see cref="ErrorType"/>. Centralised in
    /// <see cref="SeverityOf"/> so the GUI's colour-coding (and the toolbar icon) classifies each kind of
    /// conflict in one place.
    /// </summary>
    public Severity Severity => SeverityOf(ErrorType);

    /// <summary>
    /// Classifies a validation error type by severity. Genuine planning conflicts that must be resolved
    /// before publishing are warnings; advisory findings (speed heuristics, incomplete locomotive
    /// coverage) are informational. Adjust the split here.
    /// </summary>
    public static Severity SeverityOf(ValidationErrorType errorType) => errorType switch
    {
        ValidationErrorType.TrainSpeedTooSlow or
        ValidationErrorType.TrainSpeedTooFast or
        ValidationErrorType.LocomotiveCoverageGap or
        ValidationErrorType.LocomotiveCoverageOverlap => Severity.Information,
        _ => Severity.Warning,
    };

    /// <summary>
    /// Determines whether the given train is one of the trains involved in this conflict.
    /// Used by GUI components to highlight the offending train.
    /// </summary>
    public bool Involves(Train train) => Trains.Contains(train);

    /// <summary>
    /// Determines whether the given station track is where this conflict starts or ends.
    /// Used by GUI components to highlight the offending track.
    /// </summary>
    public bool Involves(StationTrack track) => FromTrack.Equals(track) || ToTrack.Equals(track);

    /// <summary>
    /// Determines whether this conflict's time span overlaps the given time range (inclusive).
    /// Used by GUI components (e.g. the graphical timetable) to hit-test a rendered region.
    /// </summary>
    public bool OverlapsTimeRange(Time from, Time to) => FromTime <= to && ToTime >= from;

    /// <summary>
    /// Creates a station track conflict error.
    /// </summary>
    public static ValidationError StationTrackConflict(
        StationTrack track,
        StationCall call1,
        StationCall call2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.StationTrackConflict,
            FromTrack = track,
            ToTrack = track,
            FromTime = Time.Min(call1.Departure, call2.Departure),
            ToTime = Time.Max(call1.Arrival, call2.Arrival),
            Trains = [call1.Train!, call2.Train!],
            Message = message
        };

    /// <summary>
    /// Creates a track stretch conflict error.
    /// </summary>
    public static ValidationError StretchConflict(
        StationTrack fromTrack,
        StationTrack toTrack,
        StretchPassing passing1,
        StretchPassing passing2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.TrackStretchConflict,
            FromTrack = fromTrack,
            ToTrack = toTrack,
            FromTime = Time.Min(passing1.From.Departure, passing2.From.Departure),
            ToTime = Time.Max(passing1.To.Arrival, passing2.To.Arrival),
            Trains = [passing1.Train, passing2.Train],
            Message = message
        };

    /// <summary>
    /// Creates a train time sequence error.
    /// </summary>
    public static ValidationError TrainTimeSequence(
        StationCall call1,
        StationCall call2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.TrainTimeSequence,
            FromTrack = call1.Track,
            ToTrack = call2.Track,
            FromTime = call1.Departure,
            ToTime = call2.Arrival,
            Trains = [call1.Train!],
            Message = message
        };

    /// <summary>
    /// Creates a train speed error.
    /// </summary>
    public static ValidationError TrainSpeed(
        StationCall fromCall,
        StationCall toCall,
        bool isTooSlow,
        Message message) => new()
        {
            ErrorType = isTooSlow ? ValidationErrorType.TrainSpeedTooSlow : ValidationErrorType.TrainSpeedTooFast,
            FromTrack = fromCall.Track,
            ToTrack = toCall.Track,
            FromTime = fromCall.Departure,
            ToTime = toCall.Arrival,
            Trains = [fromCall.Train!],
            Message = message
        };

    /// <summary>
    /// Creates a station call timing error (arrival after departure).
    /// </summary>
    public static ValidationError StationCallTiming(
        StationCall call,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.StationCallTiming,
            FromTrack = call.Track,
            ToTrack = call.Track,
            FromTime = call.Departure,
            ToTime = call.Arrival,
            Trains = [call.Train!],
            Message = message
        };

    /// <summary>
    /// Creates a vehicle schedule overlap error.
    /// </summary>
    public static ValidationError VehicleScheduleOverlap(
        TrainPart part1,
        TrainPart part2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.VehicleScheduleOverlap,
            FromTrack = part1.From.Track,
            ToTrack = part2.To.Track,
            FromTime = Time.Min(part1.From.Departure, part2.From.Departure),
            ToTime = Time.Max(part1.To.Arrival, part2.To.Arrival),
            Trains = [part1.Train, part2.Train],
            Message = message
        };

    /// <summary>
    /// Creates a missing track reference error.
    /// </summary>
    public static ValidationError MissingTrackReference(
        StationTrack track,
        Train train,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.MissingTrackReference,
            FromTrack = track,
            ToTrack = track,
            FromTime = train.Calls.First().Departure,
            ToTime = train.Calls.Last().Arrival,
            Trains = [train],
            Message = message
        };

    /// <summary>
    /// Creates a train with too few calls error.
    /// </summary>
    public static ValidationError TrainTooFewCalls(
        Train train,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.TrainTooFewCalls,
            FromTrack = train.Calls.FirstOrDefault()?.Track ?? StationTrack.Example,
            ToTrack = train.Calls.LastOrDefault()?.Track ?? StationTrack.Example,
            FromTime = train.Calls.FirstOrDefault()?.Departure ?? Time.Zero,
            ToTime = train.Calls.LastOrDefault()?.Arrival ?? Time.Zero,
            Trains = [train],
            Message = message
        };

    /// <summary>
    /// Creates a locomotive coverage gap error.
    /// </summary>
    public static ValidationError LocomotiveCoverageGap(
        Train train,
        StationCall gapStart,
        StationCall gapEnd,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.LocomotiveCoverageGap,
            FromTrack = gapStart.Track,
            ToTrack = gapEnd.Track,
            FromTime = gapStart.Departure,
            ToTime = gapEnd.Arrival,
            Trains = [train],
            Message = message
        };

    /// <summary>
    /// Creates a locomotive coverage overlap error.
    /// </summary>
    public static ValidationError LocomotiveCoverageOverlap(
        Train train,
        TrainPart part1,
        TrainPart part2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.LocomotiveCoverageOverlap,
            FromTrack = part1.From.Track,
            ToTrack = part2.To.Track,
            FromTime = Time.Max(part1.From.Departure, part2.From.Departure),
            ToTime = Time.Min(part1.To.Arrival, part2.To.Arrival),
            Trains = [train],
            Message = message
        };

    /// <summary>
    /// Creates a vehicle double-booked error (overlapping schedule assignments).
    /// </summary>
    public static ValidationError VehicleDoubleBooked(
        ScheduledObject vehicle,
        ScheduleAssignment assignment1,
        ScheduleAssignment assignment2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.VehicleDoubleBooked,
            FromTrack = GetFirstTrack(assignment1.Schedule) ?? StationTrack.Example,
            ToTrack = GetLastTrack(assignment2.Schedule) ?? StationTrack.Example,
            FromTime = GetFirstDeparture(assignment1.Schedule) ?? Time.Zero,
            ToTime = GetLastArrival(assignment2.Schedule) ?? Time.Zero,
            Trains = GetTrains(assignment1.Schedule, assignment2.Schedule),
            Message = message
        };

    /// <summary>
    /// Creates a duplicate train-number error: two trains share the same operating company, category
    /// and number but run on overlapping sessions (rule T4).
    /// </summary>
    public static ValidationError DuplicateTrainNumber(
        Train train1,
        Train train2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.DuplicateTrainNumber,
            FromTrack = train1.Calls.FirstOrDefault()?.Track ?? StationTrack.Example,
            ToTrack = train1.Calls.LastOrDefault()?.Track ?? StationTrack.Example,
            FromTime = train1.Calls.FirstOrDefault()?.Departure ?? Time.Zero,
            ToTime = train1.Calls.LastOrDefault()?.Arrival ?? Time.Zero,
            Trains = [train1, train2],
            Message = message
        };

    /// <summary>
    /// Creates a non-contiguous schedule error: a part does not start from the station where the
    /// previous part in the vehicle's working ended (rule S2).
    /// </summary>
    public static ValidationError ScheduleNotContiguous(
        TrainPart previous,
        TrainPart next,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.ScheduleNotContiguous,
            FromTrack = previous.To.Track,
            ToTrack = next.From.Track,
            FromTime = previous.To.Arrival,
            ToTime = next.From.Departure,
            Trains = [.. new[] { previous.Train, next.Train }.Distinct()],
            Message = message
        };

    /// <summary>
    /// Creates a non-closing schedule error: a schedule that runs the whole operating period does not
    /// return the vehicle to the station it started from (rule S3).
    /// </summary>
    public static ValidationError ScheduleNotClosed(
        Schedule schedule,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.ScheduleNotClosed,
            FromTrack = GetFirstTrack(schedule) ?? StationTrack.Example,
            ToTrack = GetLastTrack(schedule) ?? StationTrack.Example,
            FromTime = GetFirstDeparture(schedule) ?? Time.Zero,
            ToTime = GetLastArrival(schedule) ?? Time.Zero,
            Trains = [.. schedule.Parts.Select(p => p.Train).Distinct()],
            Message = message
        };

    /// <summary>
    /// Creates a non-closing session-combination error: a set of sessions on which a vehicle works a
    /// distinct set of parts does not return it to its start station (rule S5).
    /// </summary>
    public static ValidationError SessionCombinationNotClosed(
        ScheduledObject vehicle,
        SessionCombination combination,
        Message message)
    {
        var first = combination.Parts.OrderBy(p => p.From.Departure.Value).First();
        var last = combination.Parts.OrderBy(p => p.To.Arrival.Value).Last();
        return new()
        {
            ErrorType = ValidationErrorType.SessionCombinationNotClosed,
            FromTrack = first.From.Track,
            ToTrack = last.To.Track,
            FromTime = first.From.Departure,
            ToTime = last.To.Arrival,
            Trains = [.. combination.Parts.Select(p => p.Train).Distinct()],
            Message = message
        };
    }

    private static StationTrack? GetFirstTrack(Schedule schedule) =>
        schedule.Parts.OrderBy(p => p.From.Departure.Value).FirstOrDefault()?.From.Track;

    private static StationTrack? GetLastTrack(Schedule schedule) =>
        schedule.Parts.OrderBy(p => p.To.Arrival.Value).LastOrDefault()?.To.Track;

    private static Time? GetFirstDeparture(Schedule schedule) =>
        schedule.Parts.OrderBy(p => p.From.Departure.Value).FirstOrDefault()?.From.Departure;

    private static Time? GetLastArrival(Schedule schedule) =>
        schedule.Parts.OrderBy(p => p.To.Arrival.Value).LastOrDefault()?.To.Arrival;

    private static Train[] GetTrains(Schedule schedule1, Schedule schedule2) =>
        [.. schedule1.Parts.Select(p => p.Train)
            .Concat(schedule2.Parts.Select(p => p.Train))
            .Distinct()];
}

/// <summary>
/// Categories of validation errors.
/// </summary>
public enum ValidationErrorType
{
    /// <summary>Station track is referenced but not in layout.</summary>
    MissingTrackReference,

    /// <summary>Two trains conflict on the same station track.</summary>
    StationTrackConflict,

    /// <summary>Station call has arrival after departure.</summary>
    StationCallTiming,

    /// <summary>Two trains conflict on a track stretch.</summary>
    TrackStretchConflict,

    /// <summary>Train calls are not in correct time sequence.</summary>
    TrainTimeSequence,

    /// <summary>Train speed is too slow between calls.</summary>
    TrainSpeedTooSlow,

    /// <summary>Train speed is too fast between calls.</summary>
    TrainSpeedTooFast,

    /// <summary>Train must have at least two station calls.</summary>
    TrainTooFewCalls,

    /// <summary>Vehicle schedule has overlapping train parts.</summary>
    VehicleScheduleOverlap,

    /// <summary>Train has a gap in locomotive coverage.</summary>
    LocomotiveCoverageGap,

    /// <summary>Train has overlapping locomotive assignments.</summary>
    LocomotiveCoverageOverlap,

    /// <summary>Vehicle has overlapping schedule assignments (double-booked).</summary>
    VehicleDoubleBooked,

    /// <summary>Two trains share company, category and number but run on overlapping sessions.</summary>
    DuplicateTrainNumber,

    /// <summary>A vehicle schedule's parts are not geographically contiguous.</summary>
    ScheduleNotContiguous,

    /// <summary>An all-session vehicle schedule does not return to its start station.</summary>
    ScheduleNotClosed,

    /// <summary>A vehicle's session combination does not return it to its start station.</summary>
    SessionCombinationNotClosed,
}
