using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a train, including its identification, category, company, timetable, and operational details.
/// </summary>
/// <remarks>The Train class models a scheduled train and its associated data, such as its number, operator,
/// timetable, and route calls. It supports equality comparison based on company, number, and external identifier. This
/// class is designed for use with Entity Framework Core and includes properties for related entities such as Company
/// and Timetable. Some properties are required for correct instantiation and operation.</remarks>
public class Train : IEquatable<Train>
{
    // Private parameterless constructor for EF Core
    private Train()
    {
        ExternalId = string.Empty;
        Timetable = default!;
        Groups = [];
        Calls = [];
    }

    [SetsRequiredMembers]
    public Train(int id, int number, string externalId = "")
    {
        Id = id;
        Number = number;
        ExternalId = externalId;
        Timetable = default!;
        Groups = [];
        Calls = [];
    }

    [SetsRequiredMembers]
    public Train(int id, TrainCategory category, int number, string externalId = "")
        : this(id, number, externalId)
    {
        Category = category;
        CategoryId = category.Id;
    }

    public required int Id { get; set; }
    public required int Number { get; set; }
    public string ExternalId { get; set; }
    public string? Remark { get; set; }
    public TrainLenght Length { get; set; }

    // FK property for EF Core - optional, company must exist in database
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    // FK property for EF Core - optional, category must exist in database when set
    public int? CategoryId { get; set; }
    public TrainCategory? Category { get; set; }

    public required Sessions Sessions { get; set; } = Sessions.All;
    public IList<string> Groups { get; set; }

    // FK property for EF Core - required
    public int TimetableId { get; set; }
    public Timetable Timetable { get; set; }

    public IList<StationCall> Calls { get; set; }
    public Time DriverStartTime => this[0].Arrival;
    public Time DriverEndTime => this[^1].Departure;
    public StationCall this[Index index] => Calls[index];
    internal IEnumerable<StationTrack> Tracks => Calls.OrderBy(c => c.Arrival.Value).Select(c => c.Track).Distinct();
    public Layout Layout => Calls[0].Station.Layout;
    public TrainPart AsTrainPart => this.AsTrainPart(0, Calls.Count - 1);

    public bool Equals(Train? other) =>
        other is not null &&
        (Company?.Equals(other.Company) ?? other.Company is null) &&
        Number == other.Number &&
        ExternalId.Equals(other.ExternalId, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) => obj is Train other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(CompanyId, Number, ExternalId);
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
        public string Identity => train.Category?.TrainIdentity(train.Number) ?? train.Number.ToString();

        public StationCall Add(StationCall call)
        {
            train = train.ValueOrException(nameof(train));
            call = call.ValueOrException(nameof(call));
            if (!train.Calls.Contains(call))
            {
                call.SetTrain(train);
                if (train.Calls.Count == 0)
                {
                    call.IsArrival = false;
                    call.IsDeparture = true;
                }

                train.Calls.Add(call);
            }
            return call;
        }
        public Train WithFixedSingleCallTrain()
        {
            if (train.Calls.Count == 1)
            {
                var departure = train.Calls[0];
                departure.Track.Calls.Remove(departure);
                var arrival = new StationCall(departure.Id, departure.Track, departure.Arrival, departure.Arrival);
                departure = new StationCall(departure.Id + 1, departure.Track, departure.Departure, departure.Departure);
                train.Calls.Clear();
                train.Add(arrival);
                train.Add(departure);
            }
            return train;
        }

        public Train WithFirstCallDepartureOnlyAndLastCallArrivalOnly()
        {
            train.SetFirstCallDepartureOnly();
            train.SetLastCallArrivalOnly();
            return train;
        }

        public void SetFirstCallDepartureOnly() => train.Calls.First().IsArrival = false;
        public void SetLastCallArrivalOnly() => train.Calls.Last().IsDeparture = false;
    }

    public static bool IsNullOrHasNoCalls([NotNullWhen(false)] this Train? train)
        => train is null || train.Calls.Count == 0;
}
