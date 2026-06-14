using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model;

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
    public NonTractionOptions? NonTractionOptions { get; set; }

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
    /// <summary>
    /// Creates a train part from the train between the specified call indices.
    /// </summary>
    /// <param name="train">The train.</param>
    /// <param name="fromCallIndex">The index of the departure call.</param>
    /// <param name="toCallIndex">The index of the arrival call.</param>
    /// <returns>A new train part.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the indices are invalid.</exception>
    public static TrainPart AsTrainPart(this Train? train, int fromCallIndex, int toCallIndex)
    {
        var t = train.ValueOrException(nameof(train));
        var c = t.Calls.Count;
        (fromCallIndex < 0 || fromCallIndex > c - 2).IfTrueThrows(nameof(fromCallIndex));
        (toCallIndex <= fromCallIndex || toCallIndex > c - 1).IfTrueThrows(nameof(toCallIndex));
        var calls = t.Calls.ToArray();
        return new TrainPart(calls[fromCallIndex], calls[toCallIndex]);
    }

    /// <summary>
    /// Determines whether this train part overlaps with any of the specified train parts.
    /// </summary>
    /// <param name="me">The train part to check.</param>
    /// <param name="other">The collection of train parts to check against.</param>
    /// <returns><c>true</c> if there is any overlap; otherwise, <c>false</c>.</returns>
    public static bool IsOverlapping(this TrainPart me, IEnumerable<TrainPart> other)
    {
        return other.Any(o => o.Arrival > me.Departure && o.Departure < me.Arrival);
    }
}
