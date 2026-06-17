using System.Globalization;
using System.Text.Json.Serialization;
using NoteResources = Tellurian.Trains.Schedules.Model.Resources.Notes;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Represents a portion of a train's journey between two station calls.
/// </summary>
/// <remarks>
/// Train parts are used to assign vehicles or drivers to specific segments of a train's route,
/// rather than the entire journey.
/// </remarks>
public sealed class TrainPart : IEquatable<TrainPart>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private TrainPart()
    {
        From = default!;
        To = default!;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="TrainPart"/> with the specified station calls.
    /// </summary>
    /// <param name="from">The departure station call.</param>
    /// <param name="to">The arrival station call.</param>
    /// <exception cref="ArgumentException">Thrown when the station calls are from different trains.</exception>
    public TrainPart(StationCall from, StationCall to)
    {
        From = from.ValueOrException(nameof(from));
        To = to.ValueOrException(nameof(to));
        FromId = from.Id;
        ToId = to.Id;
        From.Train.IfNotEqualsThrow(To.Train, $"Departure {from} is not same train as arrival {to}.");
    }

    /// <summary>
    /// Gets or sets the unique identifier for this train part.
    /// </summary>
    public int Id { get; set; }

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
    /// Gets or sets the foreign key to the departure station call.
    /// </summary>
    public int FromId { get; set; }

    /// <summary>
    /// Gets or sets the departure station call.
    /// </summary>
    public StationCall From { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the arrival station call.
    /// </summary>
    public int ToId { get; set; }

    /// <summary>
    /// Gets or sets the arrival station call.
    /// </summary>
    public StationCall To { get; set; }

    /// <summary>
    /// Gets or sets an optional external key for this train part.
    /// </summary>
    public string? ExternalKey { get; set; }

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
    /// Options applying when this part participates in a cargo flow directed by waybills.
    /// Null when not applicable.
    /// </summary>
    public CargoFlowOptions? CargoFlowOptions { get; set; }

    /// <summary>
    /// Options applying when this part is a fixed-schedule cargo-only working.
    /// Null when not applicable.
    /// </summary>
    public CargoOnlyOptions? CargoOnlyOptions { get; set; }

    /// <summary>
    /// Gets the train this train part belongs to.
    /// </summary>
    public Train Train => From.Train!;

    /// <summary>
    /// Gets the departure time for this train part.
    /// </summary>
    public Time? Departure => From.Departure;

    /// <summary>
    /// Gets the arrival time for this train part.
    /// </summary>
    public Time? Arrival => To.Arrival;

    /// <inheritdoc/>
    public bool Equals(TrainPart? other) => other != null && From.Equals(other.From) && To.Equals(other.To);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TrainPart other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(From.GetHashCode(), To.GetHashCode());

    /// <inheritdoc/>
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "'{0}' {1} {2}->{3} {4}", Train, From.Station, From.Departure.HHMM(), To.Station, To.Arrival.HHMM());
}

/// <summary>
/// Provides extension methods for <see cref="TrainPart"/>.
/// </summary>
public static class TrainPartExtensions
{
    extension(TrainPart trainPart)
    {
        /// <summary>
        /// Determines whether this train part overlaps with any of the specified train parts.
        /// </summary>
        /// <param name="otherTrainParts">The collection of train parts to check against.</param>
        /// <returns><c>true</c> if there is any overlap; otherwise, <c>false</c>.</returns>
        public bool IsOverlapping(IEnumerable<TrainPart> otherTrainParts)
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
                trainPart.AddCargoFlowDepartureNotes(result);
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

        private void AddCargoFlowDepartureNotes(List<ICallNote> callNotes)
        {
            var options = trainPart.CargoFlowOptions;
            if (options is null) return;
            if (options.HasCoupleNote)
            {
                callNotes.AddRange(trainPart.CargoFlows
                    .Select(cf => new CargoFlowDestinationNote(cf, trainPart)));
            }
        }

        private IEnumerable<ScheduledObject> ScheduledObjects =>
            trainPart.Schedule?.Plan.ScheduledObjectsFor(trainPart) ?? [];

        private IEnumerable<ScheduledObject> TractionUnits =>
            trainPart.ScheduledObjects.Where(so => so.IsTraction);

        private IEnumerable<ScheduledObject> WagonSets =>
            trainPart.ScheduledObjects.Where(so => so.IsWagonSet);

        private IEnumerable<ScheduledObject> CargoFlows =>
            trainPart.ScheduledObjects.Where(so => so.IsCargoFlow);

        internal string CargoFlowDestinationsText
        {
            get
            {
                var options = trainPart.CargoFlowOptions!;
                return options.ToAllDestinations ? NoteResources.AllDestinations :
                        string.Join(", ", options.Destinations.Select(d => d.ToString()));
            }
        }

        internal string CargoFlowDestinationsHtml
        {
            get
            {
                var options = trainPart.CargoFlowOptions!;
                return options.ToAllDestinations ? NoteResources.AllDestinations : string.Join(", ", options.Destinations.Select(d => d.Display.Value));
            }
        }
    }




}

