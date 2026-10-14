namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// What a start label says: the card stood on a train's start track, read by whoever puts the train out
/// and by the loco driver who takes it.
/// </summary>
/// <param name="Train">The train that starts.</param>
/// <param name="Departure">The call the train starts from.</param>
/// <param name="Duties">The identities of the driver duties working the train's first part, in order.
/// Empty where no duty is planned for it.</param>
/// <param name="ArrivedAs">The trains the traction unit working this one arrived as, where it arrived at
/// the start location from an earlier train in its schedule. Empty for a unit that starts its day here.</param>
public sealed record StartLabel(Train Train, StationCall Departure, IReadOnlyList<string> Duties, IReadOnlyList<Train> ArrivedAs)
{
    /// <summary>The location the train starts at.</summary>
    public OperationLocation Location => Departure.OperationLocation;

    /// <summary>The track the train starts from.</summary>
    public StationTrack Track => Departure.Track;

    /// <summary>The time the train departs.</summary>
    public Time Time => Departure.Departure;
}

/// <summary>
/// Which trains get a start label, and what is on it. See <see cref="StartLabelPrinting"/>.
/// </summary>
public static class StartLabelExtensions
{
    extension(StartLabelPrinting printing)
    {
        /// <summary>
        /// The narrower of this choice and <paramref name="other"/>: None beats First beats All. A label is
        /// printed only where the category and the station both want it, and where one says First the
        /// other's All does not widen it.
        /// </summary>
        /// <param name="other">The other party's choice.</param>
        public StartLabelPrinting Narrower(StartLabelPrinting other) =>
            (StartLabelPrinting)Math.Min((int)printing, (int)other);
    }

    extension(Plan plan)
    {
        /// <summary>
        /// The start labels to print: one for each train that starts at a station and that the train's
        /// category and the station both want a label for (see <see cref="StartLabelPrinting"/>), ordered by
        /// station name, then by departure time, then by train number.
        /// </summary>
        /// <remarks>
        /// A shunting task has no start — it works in place — and so has no label. <c>First</c> means the
        /// train is the first one a traction unit works (<c>ScheduledObject.Start</c>), which takes the
        /// meeting's first session start into account: a train that left before it was never worked from the
        /// meeting's beginning, and the unit starts at its next train instead.
        /// </remarks>
        public IReadOnlyList<StartLabel> StartLabels()
        {
            plan = plan.ValueOrException(nameof(plan));
            var layout = plan.Layout;
            var settings = layout.Settings.General.SessionSettings();
            var firstSessionStart = layout.Settings.General.FirstSessionStart;
            var firstTrains = plan.ScheduledObjects
                .Where(vehicle => vehicle.IsPowered)
                .Select(vehicle => vehicle.Start(settings.UseDaysInsteadOfSessionNumbers, settings.MaxNumberOfSessions, firstSessionStart))
                .OfType<VehicleStart>()
                .Select(start => start.FirstPart.From)
                .ToHashSet();

            return [.. plan.Timetable.Trains
                .Where(train => !train.IsShuntingTask)
                .Select(train => train.CallsInRunOrder.FirstOrDefault())
                .OfType<StationCall>()
                .Where(call => call.OperationLocation is Station && Printing(call) != StartLabelPrinting.None)
                .Where(call => Printing(call) == StartLabelPrinting.All || firstTrains.Contains(call))
                .OrderBy(call => call.OperationLocation.Name, StringComparer.CurrentCulture)
                .ThenBy(call => call.Departure)
                .ThenBy(call => call.Train.Number)
                .Select(call => LabelFor(plan, call))];
        }
    }

    // What the train's category and the station it starts at jointly allow.
    private static StartLabelPrinting Printing(StationCall call) =>
        (call.Train.Category?.StartLabels ?? StartLabelPrinting.None)
            .Narrower(call.OperationLocation is Station station ? station.StartLabels : StartLabelPrinting.None);

    private static StartLabel LabelFor(Plan plan, StationCall call)
    {
        var parts = plan.Schedules.SelectMany(schedule => schedule.Parts)
            .Where(part => part.From.Equals(call))
            .ToList();
        var duties = plan.DriverDuties
            .Where(duty => duty.Parts.Any(part => part.From.Equals(call)))
            .Select(duty => duty.Identity)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.CurrentCulture)
            .ToList();
        var arrivedAs = parts
            .Select(ArrivalBefore)
            .OfType<Train>()
            .Distinct()
            .OrderBy(train => train.Number)
            .ToList();
        return new StartLabel(call.Train, call, duties, arrivedAs);
    }

    // The train the schedule of the part has the traction unit arrive as at the part's start location:
    // the latest part of the schedule that ends there before this one starts.
    private static Train? ArrivalBefore(ScheduledTrainPart part) =>
        part.Schedule?.Parts
            .Where(other => !other.Equals(part)
                && other.To.OperationLocation.Equals(part.From.OperationLocation)
                && other.EndTime <= part.StartTime)
            .MaxBy(other => other.EndTime)?.Train;
}
