using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A <see cref="TrainPart"/> that is assigned to a vehicle <see cref="Schedule"/> and/or a
/// <see cref="DriverDuty"/>. It carries the per-part options describing how the assigned traction,
/// wagons or fixed-schedule cargo are handled over the segment. A part may carry several option kinds
/// at once; each slot is null when not applicable.
/// </summary>
public sealed class
    ScheduledTrainPart : TrainPart
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private ScheduledTrainPart() : base() { }

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduledTrainPart"/> with the specified station calls.
    /// </summary>
    /// <param name="from">The departure station call.</param>
    /// <param name="to">The arrival station call.</param>
    /// <exception cref="ArgumentException">Thrown when the station calls are from different trains.</exception>
    public ScheduledTrainPart(StationCall from, StationCall to) : base(from, to) { }

    /// <summary>
    /// Gets or sets the foreign key to the vehicle schedule. Optional.
    /// </summary>
    public int? ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle schedule this train part is assigned to.
    /// </summary>
    public Schedule? Schedule { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the driver duty. Optional.
    /// </summary>
    public int? DutyId { get; set; }

    /// <summary>
    /// Gets or sets the driver duty this train part is assigned to.
    /// </summary>
    public DriverDuty? Duty { get; set; }

    /// <summary>
    /// Options applying when this part is operated by a traction unit (locomotive or trainset).
    /// Null when not applicable. A part may carry several option kinds at once.
    /// </summary>
    public TractionOptions? TractionOptions { get; set; }

    /// <summary>
    /// Options applying when this part carries non-traction rolling stock (wagons).
    /// Null when not applicable.
    /// </summary>
    public WagonSetOptions? WagonSetOptions { get; set; }

    /// <summary>
    /// Options applying when this part is a fixed-schedule cargo-only working.
    /// Null when not applicable.
    /// </summary>
    public CargoOnlyOptions? CargoOnlyOptions { get; set; }
}

/// <summary>
/// Provides extension methods for <see cref="ScheduledTrainPart"/>.
/// </summary>
public static class ScheduledTrainPartExtensions
{
    extension(ScheduledTrainPart trainPart)
    {
        /// <summary>
        /// Determines whether this train part overlaps in time with any of the specified train parts.
        /// Overlap only applies to scheduled parts (vehicle circulations and driver duties); a cargo
        /// flow is not subject to it.
        /// </summary>
        /// <param name="otherTrainParts">The collection of train parts to check against.</param>
        /// <returns><c>true</c> if there is any overlap; otherwise, <c>false</c>.</returns>
        public bool IsOverlapping(IEnumerable<ScheduledTrainPart> otherTrainParts)
        {
            return otherTrainParts.Any(o => o.Arrival > trainPart.Departure && o.Departure < trainPart.Arrival);
        }

        /// <summary>
        /// Creates <see cref="ICallNote">notes</see> for the departure station call at the train part's start station.
        /// </summary>
        public IEnumerable<ICallNote> DepartureNotes
        {
            get
            {
                List<ICallNote> result = [];
                trainPart.AddTractionUnitDepartureNotes(result);
                trainPart.AddWagonSetDepartureNotes(result);
                result.AddRange(trainPart.From.Notes);
                return result;
            }
        }

        /// <summary>
        /// Creates <see cref="ICallNote">notes</see> for the arrival station call at the train part's end station.
        /// </summary>
        public IEnumerable<ICallNote> ArrivalNotes
        {
            get
            {
                List<ICallNote> result = [];
                trainPart.AddTractionUnitArrivalNotes(result);
                result.AddRange(trainPart.To.Notes);
                return result;
            }
        }

        private void AddTractionUnitDepartureNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.TractionOptions;
            if (options is null) return;
            if (options.FromParking)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new FromParkingNote(so)));
            }
            else if (options.HasCoupleNote)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new CoupleNote(so)));
            }
            else if (options.DisplayUseNote)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new UseNote(so)));
            }
            if (options.IsReinforcement)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new ReinforcementNote(so, trainPart) { DisplayOrder = 800 }));

            }
        }

        private void AddTractionUnitArrivalNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.TractionOptions;
            if (options is null) return;
            if (options.ToParking)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new ToParkingNote(so)));
            }
            else if (options.HasUncoupleNote)
            {
                callNotes.AddRange(trainPart.TractionUnits
                    .Select(so => new UncoupleNote(so)));
            }

        }

        private void AddWagonSetDepartureNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.WagonSetOptions;
            if (options is null) return;
            if (options.HasCoupleNote)
            {
                callNotes.AddRange(trainPart.WagonSets
                    .Select(so => new CoupleNote(so)));
            }
        }

        private IEnumerable<ScheduledObject> ScheduledObjects =>
            trainPart.Schedule?.Plan.ScheduledObjectsFor(trainPart) ?? [];

        private IEnumerable<ScheduledObject> TractionUnits =>
            trainPart.ScheduledObjects.Where(so => so.IsTraction);

        private IEnumerable<ScheduledObject> WagonSets =>
            trainPart.ScheduledObjects.Where(so => so.IsWagonSet);
    }
}
