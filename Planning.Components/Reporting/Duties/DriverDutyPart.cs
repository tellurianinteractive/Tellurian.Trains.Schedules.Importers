namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

/// <summary>
/// One train part of a duty, shaped for printing: the header facts, the three vehicle and cargo blocks,
/// and the rows of the train timetable.
/// </summary>
/// <remarks>
/// The report computes no domain facts of its own. Everything here is either stored in the model or an
/// existing model-side derivation; this type only flattens it into the shape the page renders.
/// </remarks>
public sealed class DriverDutyPart
{
    /// <summary>
    /// The stretch of the train the driver works. When the driver stays with a train over several of
    /// the duty's parts, this is one span from the first part's start to the last part's end.
    /// </summary>
    public required ScheduledTrainPart TrainPart { get; init; }

    /// <summary>
    /// The duty's own parts this printed part is made of, in running order. Empty means the one part
    /// <see cref="TrainPart"/> itself.
    /// </summary>
    /// <remarks>
    /// A train is printed once however many of the duty's parts it is cut into: where the traction unit
    /// changes, or wagons are coupled or uncoupled on the way, the driver still works one train, and a
    /// page per part made it read as several. The parts are kept because each vehicle works its own
    /// stretch of the span, and the vehicle blocks show which.
    /// </remarks>
    public IReadOnlyList<ScheduledTrainPart> Parts { get; init; } = [];

    /// <summary>The duty this part belongs to.</summary>
    public required DriverDuty Duty { get; init; }

    /// <summary>How sessions are displayed in this booklet.</summary>
    public required SessionsSettings SessionsSettings { get; init; }
}

/// <summary>
/// Projects a <see cref="DriverDutyPart"/> into the four blocks of a train part page.
/// </summary>
public static class DriverDutyPartExtensions
{
    extension(DriverDutyPart dutyPart)
    {
        /// <summary>The train being worked.</summary>
        public Train Train => dutyPart.TrainPart.Train;

        /// <summary>
        /// The traction units hauling this part, with the movement each of them works.
        /// </summary>
        public TrainPartVehicleData TractionData => new()
        {
            Vehicles =
            [
                .. dutyPart.VehicleRows.Where(row => row.Vehicle.IsTraction)
            ],
            SessionsSettings = dutyPart.SessionsSettings,
        };

        /// <summary>
        /// The wagonsets this part carries. Structurally identical to the traction block, differing only
        /// in which scheduled objects it lists — and it exists because the same train may carry different
        /// wagonsets on different sessions, which one booklet has to show all of.
        /// </summary>
        public TrainPartVehicleData WagonsetData => new()
        {
            Vehicles =
            [
                .. dutyPart.VehicleRows.Where(row => row.Vehicle.IsWagonSet)
            ],
            SessionsSettings = dutyPart.SessionsSettings,
        };

        /// <summary>
        /// The cargo wagons with waybills carried over this part, ordered by the station they are coupled
        /// at, in the order the train reaches it, and then by position in the rake. The flows coupled at
        /// one station into one position are one row (see <see cref="TrainPartCargoFlow"/>).
        /// </summary>
        public TrainPartCargoData CargoData => new()
        {
            Flows =
            [
                .. dutyPart.Train.CargoFlows
                    .Where(dutyPart.Covers)
                    .GroupBy(flow => (flow.From, flow.PositionInTrain))
                    .Select(flows => new TrainPartCargoFlow { Flows = [.. flows] })
                    .OrderBy(row => row.Flows[0].From.SortTime)
                    .ThenBy(row => row.PositionInTrain)
            ],
            SessionsSettings = dutyPart.SessionsSettings,
        };

        /// <summary>
        /// The rows of the train timetable, in the order the driver runs them.
        /// </summary>
        public IReadOnlyList<TimetableRow> TimetableRows =>
            TimetableRow.Build(dutyPart.TrainPart, dutyPart.Duty.Sessions, dutyPart.SessionsSettings, dutyPart.Duty.Plan);

        /// <summary>
        /// Whether the train carries any limit at all, and so whether the header prints a limits line.
        /// </summary>
        /// <remarks>
        /// An unset limit contributes neither figure nor label, because on paper a driver cannot tell
        /// "not restricted" from "the planner forgot" — and an absent line says the first unambiguously.
        /// The whole line disappears when nothing is set, and then costs no height.
        /// </remarks>
        public bool HasLimits =>
            dutyPart.Train.MaxSpeed is not null ||
            dutyPart.Train.Length.Axles is not null ||
            dutyPart.Train.Length.Wagons is not null ||
            dutyPart.Train.Length.Meters is not null;

        /// <summary>
        /// One row per vehicle and the stretch it works: the span as a whole, and each of the duty's
        /// parts it is made of. Ordered by where the stretch starts, then by session.
        /// </summary>
        /// <remarks>
        /// A vehicle working the whole span is matched by the span, and one working only a stretch of it
        /// by that stretch — so on a train cut into parts, the rows themselves say that the traction unit
        /// changes, or that wagons are coupled or uncoupled, at the station where one row ends and the
        /// next begins.
        /// </remarks>
        private IEnumerable<TrainPartVehicle> VehicleRows =>
            new[] { dutyPart.TrainPart }.Concat(dutyPart.Parts).Distinct()
                .SelectMany(part => dutyPart.VehiclesOn(part).Select(vehicle => dutyPart.VehicleRow(vehicle, part)))
                .OrderBy(row => row.TrainPart.StartTime)
                .ThenBy(row => row.Sessions.FirstNumber);

        /// <summary>
        /// The vehicles working one stretch, resolved through the duty's plan.
        /// </summary>
        /// <remarks>
        /// The plan is asked, not the part's own <see cref="ScheduledTrainPart.Schedule"/> back-reference,
        /// because that reference is cleared when a schedule drops a part a duty still works — and the
        /// booklet would then print no traction unit for a part the Duties editor plainly shows one for.
        /// The plan matches the part by value, so it answers in both cases. It is also the resolution the
        /// rest of the application uses; the duty always knows its plan, whereas a part need not know its
        /// schedule.
        /// </remarks>
        private IEnumerable<ScheduledObject> VehiclesOn(ScheduledTrainPart part) =>
            dutyPart.Duty.Plan is { } plan
                ? plan.ScheduledObjectsFor(part)
                : part.ScheduledObjects;

        // One row of a vehicle block: the vehicle, the stretch it works, the sessions it works it on, and
        // the tracks it is fetched from and left on.
        private TrainPartVehicle VehicleRow(ScheduledObject vehicle, ScheduledTrainPart part) => new()
        {
            Vehicle = vehicle,
            TrainPart = part,
            Sessions = vehicle.SessionsOn(part),
            DepartureTrack = vehicle.DepartureTrackOn(part),
            ArrivalTrack = vehicle.ArrivalTrackOn(part),
        };

        // A cargo flow belongs on this part's page when it is carried over any of the part's span. On a
        // shunting task there is no span to overlap — the part and every flow on it stand at the one call
        // — so the flows of the task are simply all of them, which is what the page is for.
        private bool Covers(CargoFlowTrainPart flow) =>
            dutyPart.Train.IsShuntingTask ||
            (flow.From.SortTime < dutyPart.TrainPart.To.Arrival &&
             flow.To.SortTime > dutyPart.TrainPart.From.Departure);
    }

    extension(ScheduledObject vehicle)
    {
        /// <summary>
        /// The sessions this vehicle works the given part on: the union of the sessions of the schedule
        /// assignments whose schedule holds the part. A vehicle with no assignment covering the part
        /// falls back to the train's own sessions.
        /// </summary>
        internal Sessions SessionsOn(ScheduledTrainPart trainPart)
        {
            var assigned = vehicle.ScheduleAssignments
                .Where(assignment => assignment.Schedule.Parts.Contains(trainPart))
                .Select(assignment => assignment.Sessions)
                .ToList();
            return assigned.Count == 0
                ? trainPart.Train.Sessions
                : assigned.Aggregate((left, right) => left.Or(right));
        }

        /// <summary>
        /// The track this vehicle stands on before the given part departs: the other track its own
        /// schedule fetches it from, or else the track the train departs from.
        /// </summary>
        /// <remarks>
        /// The other track is read from the vehicle's own schedule's part, not from the part the duty
        /// holds: parts are equal over the same two calls, so the duty's part may belong to another
        /// schedule — or to none — and would then send this vehicle to a track named for someone else's.
        /// </remarks>
        internal StationTrack DepartureTrackOn(ScheduledTrainPart trainPart) =>
            vehicle.OwnPartsEqualTo(trainPart).Select(part => part.OtherFromTrack).FirstOrDefault(track => track is not null)
            ?? trainPart.From.Track;

        /// <summary>
        /// The track this vehicle is left on after the given part arrives: the other track its own
        /// schedule puts it on, or else the track the train arrives at. See <c>DepartureTrackOn</c>.
        /// </summary>
        internal StationTrack ArrivalTrackOn(ScheduledTrainPart trainPart) =>
            vehicle.OwnPartsEqualTo(trainPart).Select(part => part.OtherToTrack).FirstOrDefault(track => track is not null)
            ?? trainPart.To.Track;

        private IEnumerable<ScheduledTrainPart> OwnPartsEqualTo(ScheduledTrainPart trainPart) =>
            vehicle.ScheduleAssignments
                .SelectMany(assignment => assignment.Schedule.Parts)
                .Where(part => part.Equals(trainPart));
    }
}
