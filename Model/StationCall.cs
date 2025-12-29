using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

public sealed record StationCall : IEquatable<StationCall>, IComparable<StationCall>
{
    public Train Train { get => _train; init => _train = value; }
    private Train _train = default!;
    public int Id { get; init; }
    public OperationLocation Station => Track.Station;
    public StationTrack Track { get; init; }
    public Time Arrival { get; init; }
    public Time Departure { get; init; }
    public bool IsArrival { get; set; }
    public bool IsDeparture { get; set; }
    public ICollection<Note> Notes { get; }
    public bool IsStop => IsArrival || IsDeparture;
    public Time SortTime => IsDeparture ? Departure : Arrival;
    internal void SetTrain(Train train) => _train = train;

    public StationCall(int id, StationTrack track, Time arrival, Time departure, string? remark = null)
    {
        Id = id;
        Track = track.ValueOrException(nameof(track));
        Track.Add(this);
        Arrival = arrival;
        Departure = departure;
        Notes = [];
        if (!string.IsNullOrWhiteSpace(remark))
        {
            Notes.Add(new Note() { IsDriverNote = true, IsShuntingNote = true, IsStationNote = true, Text = remark });
        }
    }

    public bool Equals(StationCall? other) =>
         other != null &&
         Arrival.Equals(other.Arrival) &&
         Departure.Equals(other.Departure) &&
         Track.Equals(other.Track) &&
         Train?.Equals(other.Train) == true;

    public override int GetHashCode() => HashCode.Combine(Arrival, Departure, Track, Train);

    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, Strings.CallAtStationTrackDuringTimes, Station, Track, Arrival.HHMM(), Departure.HHMM());

    public int CompareTo([AllowNull] StationCall other) =>
        other is null ? 1 : SortTime.CompareTo(other.SortTime);

    public static bool operator <(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) == -1;
    public static bool operator >(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) == 1;
    public static bool operator <=(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) >= 0;
    public static bool operator >=(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) <= 0;
}
