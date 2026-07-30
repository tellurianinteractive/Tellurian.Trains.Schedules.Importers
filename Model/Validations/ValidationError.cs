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
    /// The minimum departure time of objects involved in the conflict, or <c>null</c> when the conflict
    /// happens at no particular time. See <see cref="FromTrack"/>.
    /// </summary>
    public required Time? FromTime { get; init; }

    /// <summary>
    /// The maximum arrival time of objects involved in the conflict, or <c>null</c> when the conflict
    /// happens at no particular time. See <see cref="FromTrack"/>.
    /// </summary>
    public required Time? ToTime { get; init; }

    /// <summary>
    /// The station track where the conflict starts (or occurs if same as <see cref="ToTrack"/>), or
    /// <c>null</c> when the conflict has no place on the layout.
    /// </summary>
    /// <remarks>
    /// Most conflicts are between things that happen somewhere at some time, and locating them is what
    /// lets the graphical timetable highlight the offending region. A few are not: a duty numbering fault
    /// is a property of the plan's duty list, with no track and no time. Those carry <c>null</c> rather
    /// than an arbitrary track, which would highlight an innocent part of the layout. The property stays
    /// <c>required</c> so every error still has to state its location, even when the answer is "nowhere".
    /// </remarks>
    public required StationTrack? FromTrack { get; init; }

    /// <summary>
    /// The station track where the conflict ends (or occurs if same as <see cref="FromTrack"/>), or
    /// <c>null</c> when the conflict has no place on the layout.
    /// </summary>
    public required StationTrack? ToTrack { get; init; }

    /// <summary>
    /// The trains involved in the conflict.
    /// </summary>
    public required IReadOnlyList<Train> Trains { get; init; }

    /// <summary>
    /// The schedule (turnus) this error belongs to, when the error is attributed to a specific schedule
    /// (rules S1, S2, orphan S4). Lets the Schedules tab mark the exact schedule's parts column instead
    /// of matching by train intersection. Null for train-keyed schedule-scope errors (e.g. missing
    /// traction), which are located via <see cref="Trains"/>.
    /// </summary>
    public Schedule? Schedule { get; init; }

    /// <summary>
    /// The vehicle (scheduled object) this error is attributed to, for <see cref="ValidationScope.Vehicle"/>
    /// errors (double-booked, non-closing circulation). Lets the Schedules tab mark the exact vehicle chip
    /// in the Vehicle column.
    /// </summary>
    public ScheduledObject? Vehicle { get; init; }

    /// <summary>
    /// The driver duty this error is attributed to, for <see cref="ValidationScope.Duty"/> errors
    /// (a part double-assigned across duties, or parts overlapping within a duty). Lets the Duties tab
    /// mark the exact duty. Null for non-duty errors.
    /// </summary>
    public DriverDuty? Duty { get; init; }

    /// <summary>
    /// The localized message describing the error.
    /// </summary>
    public required Message Message { get; init; }

    /// <summary>
    /// Which planning surface this kind of conflict belongs to, derived from its <see cref="ErrorType"/>.
    /// Train conflicts are highlighted in the graphical timetable and the Trains tab; schedule and vehicle
    /// conflicts only in the Schedules tab (schedule → the parts column, vehicle → the Vehicle column). This
    /// targeting is centralised in <see cref="ScopeOf"/> so each surface filters to exactly its own rules.
    /// </summary>
    public ValidationScope Scope => ScopeOf(ErrorType);

    /// <summary>
    /// Classifies a validation error type by the planning surface that should highlight it. Adjust the split
    /// here. The three traction-coverage rules (missing traction, locomotive coverage gap/overlap) are keyed
    /// to a train but are resolved by assigning vehicles, so they are Schedule scope and mark the schedules
    /// that work the train.
    /// </summary>
    public static ValidationScope ScopeOf(ValidationErrorType errorType) => errorType switch
    {
        ValidationErrorType.VehicleDoubleBooked or
        ValidationErrorType.VehicleNotClosed => ValidationScope.Vehicle,

        ValidationErrorType.DutyPartDoubleAssigned or
        ValidationErrorType.DutyPartsOverlap or
        ValidationErrorType.DutyIdentityDuplicated or
        ValidationErrorType.DutyIdentityMissing or
        ValidationErrorType.DutyStaffCountOutOfRange or
        ValidationErrorType.TrainPartMissingDriverDuty => ValidationScope.Duty,

        ValidationErrorType.VehicleScheduleOverlap or
        ValidationErrorType.ScheduleNotContiguous or
        ValidationErrorType.ScheduleHasNoVehicle or
        ValidationErrorType.TrainMissingTraction or
        ValidationErrorType.LocomotiveCoverageGap or
        ValidationErrorType.LocomotiveCoverageOverlap => ValidationScope.Schedule,

        _ => ValidationScope.Train,
    };

    /// <summary>
    /// True if the conflict is at a single station track. False for a placeless conflict
    /// (see <see cref="FromTrack"/>).
    /// </summary>
    public bool IsStationConflict => FromTrack is not null && FromTrack.Equals(ToTrack);

    /// <summary>
    /// True if the conflict spans a track stretch between stations. False for a placeless conflict
    /// (see <see cref="FromTrack"/>).
    /// </summary>
    public bool IsStretchConflict => FromTrack is not null && ToTrack is not null && !FromTrack.Equals(ToTrack);

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
    /// Used by GUI components to highlight the offending track. Always false for a placeless conflict
    /// (see <see cref="FromTrack"/>).
    /// </summary>
    public bool Involves(StationTrack track) =>
        FromTrack?.Equals(track) == true || ToTrack?.Equals(track) == true;

    /// <summary>
    /// Determines whether this schedule-scope conflict should mark the given schedule's parts column.
    /// A schedule-keyed error (<see cref="Schedule"/> set) marks exactly that schedule; a train-keyed
    /// schedule-scope error (e.g. missing traction) marks every schedule that works one of its trains.
    /// Returns false for train- and vehicle-scope errors, so those never bleed into the parts column.
    /// </summary>
    public bool Involves(Schedule schedule) =>
        Scope == ValidationScope.Schedule &&
        (Schedule is not null ? Schedule.Equals(schedule) : schedule.Parts.Any(p => Involves(p.Train)));

    /// <summary>
    /// Determines whether this vehicle-scope conflict should mark the given vehicle's chip in the Vehicle
    /// column. Returns false for train- and schedule-scope errors, so those never mark a vehicle.
    /// </summary>
    public bool Involves(ScheduledObject vehicle) =>
        Scope == ValidationScope.Vehicle && Vehicle is not null && Vehicle.Equals(vehicle);

    /// <summary>
    /// Determines whether this duty-scope conflict should mark the given duty in the Duties tab. Returns
    /// false for other scopes, so those never mark a duty.
    /// </summary>
    public bool Involves(DriverDuty duty) =>
        Scope == ValidationScope.Duty && Duty is not null && Duty.Equals(duty);

    /// <summary>
    /// Determines whether this conflict's time span overlaps the given time range (inclusive).
    /// Used by GUI components (e.g. the graphical timetable) to hit-test a rendered region. Always false
    /// for a timeless conflict (see <see cref="FromTrack"/>).
    /// </summary>
    public bool OverlapsTimeRange(Time from, Time to) =>
        FromTime is { } start && ToTime is { } end && start <= to && end >= from;

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
        Schedule schedule,
        ScheduledTrainPart part1,
        ScheduledTrainPart part2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.VehicleScheduleOverlap,
            FromTrack = part1.From.Track,
            ToTrack = part2.To.Track,
            FromTime = Time.Min(part1.From.Departure, part2.From.Departure),
            ToTime = Time.Max(part1.To.Arrival, part2.To.Arrival),
            Trains = [part1.Train, part2.Train],
            Schedule = schedule,
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
        ScheduledTrainPart part1,
        ScheduledTrainPart part2,
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
            Vehicle = vehicle,
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
        Schedule schedule,
        ScheduledTrainPart previous,
        ScheduledTrainPart next,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.ScheduleNotContiguous,
            FromTrack = previous.To.Track,
            ToTrack = next.From.Track,
            FromTime = previous.To.Arrival,
            ToTime = next.From.Departure,
            Trains = [.. new[] { previous.Train, next.Train }.Distinct()],
            Schedule = schedule,
            Message = message
        };

    /// <summary>
    /// Creates a schedule-has-no-vehicle error: a schedule that runs regular sessions has no vehicle
    /// assigned at all (rule S4).
    /// </summary>
    public static ValidationError ScheduleHasNoVehicle(
        Schedule schedule,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.ScheduleHasNoVehicle,
            FromTrack = GetFirstTrack(schedule) ?? StationTrack.Example,
            ToTrack = GetLastTrack(schedule) ?? StationTrack.Example,
            FromTime = GetFirstDeparture(schedule) ?? Time.Zero,
            ToTime = GetLastArrival(schedule) ?? Time.Zero,
            Trains = [.. schedule.Parts.Select(p => p.Train).Distinct()],
            Schedule = schedule,
            Message = message
        };

    /// <summary>
    /// Creates a train-missing-traction error: a train runs on sessions for which it has no traction unit
    /// (locomotive or self-propelled trainset) assigned through any schedule (rule S4).
    /// </summary>
    public static ValidationError TrainMissingTraction(
        Train train,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.TrainMissingTraction,
            FromTrack = train.Calls.FirstOrDefault()?.Track ?? StationTrack.Example,
            ToTrack = train.Calls.LastOrDefault()?.Track ?? StationTrack.Example,
            FromTime = train.Calls.FirstOrDefault()?.Departure ?? Time.Zero,
            ToTime = train.Calls.LastOrDefault()?.Arrival ?? Time.Zero,
            Trains = [train],
            Message = message
        };

    /// <summary>
    /// Creates a non-closing traction-unit error: over the operating period the unit's movements do not
    /// balance at every station, so it does not return to a repeatable circulation (rules S3 + S5). The
    /// error spans the station it departs more often than it returns to and the station it is left at.
    /// </summary>
    public static ValidationError VehicleNotClosed(
        ScheduledObject vehicle,
        ScheduledTrainPart fromPart,
        ScheduledTrainPart toPart,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.VehicleNotClosed,
            FromTrack = fromPart.From.Track,
            ToTrack = toPart.To.Track,
            FromTime = fromPart.From.Departure,
            ToTime = toPart.To.Arrival,
            Trains = [.. new[] { fromPart.Train, toPart.Train }.Distinct()],
            Vehicle = vehicle,
            Message = message
        };

    /// <summary>
    /// Creates a duty double-assignment error: the same train part is worked by two duties whose sessions
    /// overlap, so the segment would be driven by two drivers on a common session.
    /// </summary>
    public static ValidationError DutyPartDoubleAssigned(
        DriverDuty duty,
        DriverDuty otherDuty,
        ScheduledTrainPart part,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.DutyPartDoubleAssigned,
            FromTrack = part.From.Track,
            ToTrack = part.To.Track,
            FromTime = part.From.Departure,
            ToTime = part.To.Arrival,
            Trains = [part.Train],
            Duty = duty,
            Message = message
        };

    /// <summary>
    /// Creates a duty overlap error: two parts within one duty overlap in time, so the driver would have to
    /// be in two places at once.
    /// </summary>
    public static ValidationError DutyPartsOverlap(
        DriverDuty duty,
        ScheduledTrainPart part1,
        ScheduledTrainPart part2,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.DutyPartsOverlap,
            FromTrack = part1.From.Track,
            ToTrack = part2.To.Track,
            FromTime = Time.Min(part1.From.Departure, part2.From.Departure),
            ToTime = Time.Max(part1.To.Arrival, part2.To.Arrival),
            Trains = [.. new[] { part1.Train, part2.Train }.Distinct()],
            Duty = duty,
            Message = message
        };

    /// <summary>
    /// Creates a duplicate pinned-identity error: two duties excluded from renumbering hold the same
    /// <see cref="DriverDuty.Identity"/>. No renumbering can separate them, so the pile would get two
    /// booklets with the same number — and the pile is sorted by exactly that number.
    /// </summary>
    public static ValidationError DutyIdentityDuplicated(
        DriverDuty duty,
        DriverDuty otherDuty,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.DutyIdentityDuplicated,
            FromTrack = null,
            ToTrack = null,
            FromTime = null,
            ToTime = null,
            Trains = [],
            Duty = duty,
            Message = message
        };

    /// <summary>
    /// Creates a missing pinned-identity error: a duty excluded from renumbering has an empty
    /// <see cref="DriverDuty.Identity"/>, so it is pinned to nothing and its front page carries no number
    /// for the sorter or the chooser to read.
    /// </summary>
    public static ValidationError DutyIdentityMissing(
        DriverDuty duty,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.DutyIdentityMissing,
            FromTrack = null,
            ToTrack = null,
            FromTime = null,
            ToTime = null,
            Trains = [],
            Duty = duty,
            Message = message
        };

    /// <summary>
    /// Creates a staffing range error: a duty's <see cref="DriverDuty.StaffCount"/> is outside 1–3. The
    /// editor cannot produce this, so it guards a hand-edited plan file.
    /// </summary>
    public static ValidationError DutyStaffCountOutOfRange(
        DriverDuty duty,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.DutyStaffCountOutOfRange,
            FromTrack = null,
            ToTrack = null,
            FromTime = null,
            ToTime = null,
            Trains = [],
            Duty = duty,
            Message = message
        };

    /// <summary>
    /// Creates a train-part-missing-driver-duty error: a train part with a traction unit assigned has no
    /// driver duty rostered on some of the sessions the traction assignment runs, so nobody is assigned
    /// to drive it.
    /// </summary>
    public static ValidationError TrainPartMissingDriverDuty(
        ScheduledTrainPart part,
        Message message) => new()
        {
            ErrorType = ValidationErrorType.TrainPartMissingDriverDuty,
            FromTrack = part.From.Track,
            ToTrack = part.To.Track,
            FromTime = part.From.Departure,
            ToTime = part.To.Arrival,
            Trains = [part.Train],
            Message = message
        };

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

    /// <summary>A vehicle schedule that runs regular sessions has no vehicle assigned.</summary>
    ScheduleHasNoVehicle,

    /// <summary>A train has no traction unit on some of the sessions it runs.</summary>
    TrainMissingTraction,

    /// <summary>A traction unit's circulation does not close over the operating period.</summary>
    VehicleNotClosed,

    /// <summary>The same train part is worked by two driver duties whose sessions overlap.</summary>
    DutyPartDoubleAssigned,

    /// <summary>Two train parts within one driver duty overlap in time.</summary>
    DutyPartsOverlap,

    /// <summary>Two duties excluded from renumbering hold the same identity.</summary>
    DutyIdentityDuplicated,

    /// <summary>A duty excluded from renumbering has no identity to hold.</summary>
    DutyIdentityMissing,

    /// <summary>A duty's staff count is outside the allowed range of 1–3.</summary>
    DutyStaffCountOutOfRange,

    /// <summary>A train part with a traction unit assigned has no driver duty on some of the sessions
    /// the traction assignment runs.</summary>
    TrainPartMissingDriverDuty,
}

/// <summary>
/// The planning surface a validation conflict belongs to, so feedback is targeted: each surface
/// highlights only the conflicts in its own scope.
/// </summary>
public enum ValidationScope
{
    /// <summary>A problem with a train itself (timing, speed, conflicts, numbering, missing track).
    /// Highlighted in the graphical timetable and the Trains tab.</summary>
    Train,

    /// <summary>A problem with a vehicle schedule (turnus): overlapping/non-contiguous/orphan parts, or a
    /// train left without traction. Marks the schedule's parts column in the Schedules tab.</summary>
    Schedule,

    /// <summary>A problem with a scheduled vehicle: double-booked, or a non-closing circulation. Marks the
    /// vehicle's chip in the Vehicle column of the Schedules tab.</summary>
    Vehicle,

    /// <summary>A problem with a driver duty: a part double-assigned across duties, or parts overlapping
    /// within a duty. Marks the offending duty in the Duties tab.</summary>
    Duty,
}
