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
    /// Initializes a new instance of <see cref="TrainPart"/> with the specified station calls.
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
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "'{0}' {1} {2}->{3} {4}", Train, From.OperationLocation, From.Departure.HHMM(), To.OperationLocation, To.Arrival.HHMM());
}

/// <summary>
/// 
/// </summary>
public static class TrainPartExtensions
{
    extension(TrainPart trainPart)
    {
        internal bool ContainsCall(StationCall call)
        {
            return trainPart.Train == call.Train && trainPart.From.Departure <= call.Departure && trainPart.To.Arrival >= call.Arrival;
        }
    }
}
