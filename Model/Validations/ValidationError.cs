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
        Vehicle vehicle,
        VehicleScheduleAssignment assignment1,
        VehicleScheduleAssignment assignment2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.VehicleDoubleBooked,
            FromTrack = GetFirstTrack(assignment1.VehicleSchedule) ?? StationTrack.Example,
            ToTrack = GetLastTrack(assignment2.VehicleSchedule) ?? StationTrack.Example,
            FromTime = GetFirstDeparture(assignment1.VehicleSchedule) ?? Time.Zero,
            ToTime = GetLastArrival(assignment2.VehicleSchedule) ?? Time.Zero,
            Trains = GetTrains(assignment1.VehicleSchedule, assignment2.VehicleSchedule),
            Message = message
        };

    private static StationTrack? GetFirstTrack(VehicleSchedule schedule) =>
        schedule.Parts.OrderBy(p => p.From.Departure.Value).FirstOrDefault()?.From.Track;

    private static StationTrack? GetLastTrack(VehicleSchedule schedule) =>
        schedule.Parts.OrderBy(p => p.To.Arrival.Value).LastOrDefault()?.To.Track;

    private static Time? GetFirstDeparture(VehicleSchedule schedule) =>
        schedule.Parts.OrderBy(p => p.From.Departure.Value).FirstOrDefault()?.From.Departure;

    private static Time? GetLastArrival(VehicleSchedule schedule) =>
        schedule.Parts.OrderBy(p => p.To.Arrival.Value).LastOrDefault()?.To.Arrival;

    private static Train[] GetTrains(VehicleSchedule schedule1, VehicleSchedule schedule2) =>
        schedule1.Parts.Select(p => p.Train)
            .Concat(schedule2.Parts.Select(p => p.Train))
            .Distinct()
            .ToArray();
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
}
