using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Tellurian.Trains.Schedules.Importers.Model;

[method: SetsRequiredMembers]
public class Train(int id, OperatingCompany @operator, TrainCategory category, int number, string externalId = "") : IEquatable<Train>
{
    [method: SetsRequiredMembers]
    public Train(int id, string identity) : this(id, OperatingCompany.None, TrainCategory.Unknown, identity.NumberOrZero, identity) { }
    public required int Id { get; init; } = id;
    public required int Number { get; init; } = number;
    public string ExtenalId { get; } = externalId;
    public string? Remark { get; init; }
    public required OperatingCompany Operator { get; init; } = @operator;
    public required TrainCategory Category { get; init; } = category;
    public Timetable? Timetable { get; internal set; }
    public IList<StationCall> Calls { get; } = [];
    public StationCall this[int index] => Calls[index];
    internal IEnumerable<StationTrack> Tracks => Calls.OrderBy(c => c.Arrival.Value).Select(c => c.Track).Distinct();
    public Layout Layout => Calls[0].Station.Layout;
    public TrainPart AsTrainPart => this.AsTrainPart(0, Calls.Count - 1);

    public bool Equals(Train? other) =>
        other is not null && Operator.Equals(other.Operator) &&
        Number == other.Number &&
        ExtenalId.Equals(other?.ExtenalId, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is Train other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Operator, Number, ExtenalId);
    public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{0} {1}", Category, Number);
}

public static class TrainExtensions
{

    extension(string value)
    {
        public string Prefix =>
            string.IsNullOrWhiteSpace(value) ? "" :
            new([.. value.TakeWhile(c => char.IsLetter(c))]);

        public string NumberPart =>
            string.IsNullOrWhiteSpace(value) ? "" :
            new([.. value.SkipWhile(c => char.IsLetter(c) || char.IsWhiteSpace(c)).TakeWhile(c => char.IsDigit(c))]);

        public int NumberOrZero =>
            int.TryParse(value.NumberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number) ? number : 0;
    }

    extension(Train train)
    {
        public StationCall Add(StationCall call)
        {
            train = train.ValueOrException(nameof(train));
            call = call.ValueOrException(nameof(call));
            if (!train.Calls.Contains(call))
            {
                call.Train = train;
                train.Calls.Add(call);
            }
            return call;
        }
    }

    public static bool IsNullOrHasNoCalls([NotNullWhen(true)] this Train? train) => train is null || train.Calls.Count == 0;

    public static Train WithFixedFirstAndLastCall(this Train train)
    {
        train.Calls.First().IsArrival = false;
        train.Calls.Last().IsDeparture = false;
        return train;
    }
    public static Train WithFixedSingleCallTrain(this Train train)
    {
        if (train.Calls.Count == 1)
        {
            var departure = train.Calls[0];
            departure.Track.Calls.Remove(departure);
            var arrival = new StationCall(departure.Track, departure.Arrival, departure.Arrival);
            departure = new StationCall(departure.Track, departure.Departure, departure.Departure);
            train.Calls.Clear();
            train.Add(arrival);
            train.Add(departure);
        }
        return train;
    }

}
