using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model;

public sealed class TrainPart : IEquatable<TrainPart>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private TrainPart()
    {
        From = default!;
        To = default!;
    }

    public TrainPart(StationCall from, StationCall to)
    {
        From = from.ValueOrException(nameof(from));
        To = to.ValueOrException(nameof(to));
        FromId = from.Id;
        ToId = to.Id;
        From.Train.IfNotEqualsThrow(To.Train, $"Departure {from} is not same train as arrival {to}.");
    }

    public int Id { get; set; }

    // FK property for EF Core
    public int? ScheduleId { get; set; }
    public VehicleSchedule? Schedule { get; set; }

    // FK property for EF Core
    public int? DutyId { get; set; }
    public DriverDuty? Duty { get; set; }

    // FK property for EF Core
    public int FromId { get; set; }
    public StationCall From { get; set; }

    // FK property for EF Core
    public int ToId { get; set; }
    public StationCall To { get; set; }

    public string? ExternalKey { get; set; }

    public Train Train => From.Train!;
    public Time? Departure => From.Departure;
    public Time? Arrival => To.Arrival;

    public bool Equals(TrainPart? other) => other != null && From.Equals(other.From) && To.Equals(other.To);
    public override bool Equals(object? obj) => obj is TrainPart other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(From.GetHashCode(), To.GetHashCode());
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "'{0}' {1} {2}->{3} {4}", Train, From.Station, From.Departure.HHMM(), To.Station, To.Arrival.HHMM());
}

public static class TrainPartExtensions
{
    public static TrainPart AsTrainPart(this Train? train, int fromCallIndex, int toCallIndex)
    {
        var t = train.ValueOrException(nameof(train));
        var c = t.Calls.Count;
        (fromCallIndex < 0 || fromCallIndex > c - 2).IfTrueThrows(nameof(fromCallIndex));
        (toCallIndex <= fromCallIndex || toCallIndex > c - 1).IfTrueThrows(nameof(toCallIndex));
        var calls = t.Calls.ToArray();
        return new TrainPart(calls[fromCallIndex], calls[toCallIndex]);
    }

    public static bool IsOverlapping(this TrainPart me, IEnumerable<TrainPart> other)
    {
        return other.Any(o => o.Arrival > me.Departure && o.Departure < me.Arrival);
    }
}
