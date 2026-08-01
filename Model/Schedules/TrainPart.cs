using System.Globalization;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Represents a portion of a train's journey between two station calls.
/// </summary>
/// <remarks>
/// A train part is the common geometry — a from/to span on a single train — shared by the kinds of
/// planning that attach to a segment of a train's route. <see cref="ScheduledTrainPart"/> is assigned
/// to vehicle schedules and driver duties and carries the per-part vehicle options;
/// cargo-flow parts (added separately) carry waybill-directed cargo. Geometry and equality live here;
/// the kind-specific data lives on the subclasses.
/// </remarks>
public abstract class TrainPart : IEquatable<TrainPart>
{
    /// <summary>
    /// Parameterless constructor for EF Core and JSON deserialization.
    /// </summary>
    [JsonConstructor]
    protected TrainPart()
    {
        From = default!;
        To = default!;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduledTrainPart"/> with the specified station calls.
    /// </summary>
    /// <param name="from">The departure station call.</param>
    /// <param name="to">The arrival station call.</param>
    /// <exception cref="ArgumentException">Thrown when the station calls are from different trains.</exception>
    protected TrainPart(StationCall from, StationCall to)
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
    /// Gets or sets an optional external key for this train part.
    /// </summary>
    public string? ExternalKey { get; set; }

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
    /// Gets the train this train part belongs to.
    /// </summary>
    [JsonIgnore]
    public Train Train => From.Train!;

    /// <summary>
    /// Changes the part's span in place, keeping the foreign keys in sync.
    /// </summary>
    /// <remarks>
    /// The part keeps its identity, so everything referencing it — its vehicle
    /// <see cref="ScheduledTrainPart.Schedule">schedule</see>, the driver duties working it, and its
    /// per-part options — follows the change instead of being left on a discarded object. Both calls
    /// must be calls of the part's own train; the callers in <c>ScheduleEditingExtensions</c> check that.
    /// </remarks>
    /// <param name="from">The call the part departs from.</param>
    /// <param name="to">The call the part arrives at.</param>
    internal void SetSpan(StationCall from, StationCall to)
    {
        From = from.ValueOrException(nameof(from));
        To = to.ValueOrException(nameof(to));
        FromId = From.Id;
        ToId = To.Id;
    }

    /// <summary>
    /// Gets the departure time for this train part.
    /// </summary>
    [JsonIgnore]
    public Time? Departure => From.Departure;

    /// <summary>
    /// Gets the arrival time for this train part.
    /// </summary>
    [JsonIgnore]
    public Time? Arrival => To.Arrival;

    /// <inheritdoc/>
    public bool Equals(TrainPart? other) => other != null && From.Equals(other.From) && To.Equals(other.To);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is TrainPart other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(From.GetHashCode(), To.GetHashCode());

    /// <inheritdoc/>
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "'{0}' {1} {2}->{3} {4}", Train, From.OperationLocation, From.Departure.HHMM(), To.OperationLocation, To.Arrival.HHMM());
}

/// <summary>
/// 
/// </summary>
public static class TrainPartExtensions
{
    extension(TrainPart trainPart)
    {
        /// <summary>
        /// The span for which this part ties up the vehicles and the driver working it: the running time
        /// from departure to arrival, extended at the train's origin by the preparation time and at its
        /// destination by the finishing-up time. Those two are real pieces of work — the train is being
        /// made ready, or being put away — so anything scheduled over them clashes just as much as
        /// something scheduled over the run itself.
        /// </summary>
        /// <remarks>
        /// Only the train's own ends are extended. A part that starts or ends at an intermediate call
        /// hands over to the neighbouring part of the same train there, and stretching both across that
        /// stop would turn the handover into an overlap. Where a train records no preparation or
        /// finishing-up time — its end call arrives and departs at the same minute — the span is the
        /// running time, exactly as before.
        /// </remarks>
        public (Time From, Time To) WorkingSpan => (trainPart.From.WorkStart, trainPart.To.WorkEnd);

        /// <summary>
        /// The part written with the times of its <c>WorkingSpan</c> rather than its departure and
        /// arrival, so that a reported overlap shows the times the overlap was judged on: at the train's
        /// origin the arrival, where preparing it begins, and at its destination the departure, where
        /// finishing it up ends. Anywhere else the departure and arrival are the working times already.
        /// </summary>
        public string WorkingSpanText
        {
            get
            {
                var (from, to) = trainPart.WorkingSpan;
                return string.Format(CultureInfo.CurrentCulture, "'{0}' {1} {2}->{3} {4}", trainPart.Train, trainPart.From.OperationLocation, from.HHMM(), trainPart.To.OperationLocation, to.HHMM());
            }
        }

        internal bool ContainsCall(StationCall call)
        {
            return trainPart.Train == call.Train && trainPart.From.Departure <= call.Departure && trainPart.To.Arrival >= call.Arrival;
        }

        /// <summary>
        /// Subset of train's call that are on the train part.
        /// </summary>
        public IEnumerable<StationCall> Calls =>
            trainPart.Train.Calls.Where(c => c.Departure >= trainPart.From.Departure && c.Arrival <= trainPart.To.Arrival).OrderBy(c => c.SortTime);
    }
}
