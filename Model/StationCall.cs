using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

public sealed class StationCall : IEquatable<StationCall>, IComparable<StationCall>
{
    // Private parameterless constructor for EF Core
    private StationCall()
    {
        Track = default!;
        Notes = [];
    }

    public StationCall(int id, StationTrack track, Time arrival, Time departure, string? remark = null)
    {
        Id = id;
        Track = track.ValueOrException(nameof(track));
        TrackId = track.Id;
        Track.Add(this);
        Arrival = arrival;
        Departure = departure;
        Notes = [];
        if (!string.IsNullOrWhiteSpace(remark))
        {
            Notes.Add(new TextCallNote(remark) { IsDriverNote = true, IsShuntingNote = true, IsStationNote = true });
        }
    }

    public int Id { get; set; }

    // FK property for EF Core
    public int TrackId { get; set; }
    public StationTrack Track { get; set; }

    // FK property for EF Core
    public int TrainId { get; set; }
    public Train Train { get; set; } = default!;

    public OperationLocation Station => Track.Station;
    public Time Arrival { get; set; }
    public Time Departure { get; set; }
    public bool IsArrival { get; set; }
    public bool IsDeparture { get; set; }
    public ICollection<CallNote> Notes { get; set; }
    public bool IsStop => IsArrival || IsDeparture;
    public Time SortTime => IsDeparture ? Departure : Arrival;

    internal void SetTrain(Train train)
    {
        Train = train;
        TrainId = train.Id;
    }

    public bool Equals(StationCall? other) =>
         other != null &&
         Arrival.Equals(other.Arrival) &&
         Departure.Equals(other.Departure) &&
         Track.Equals(other.Track) &&
         Train?.Equals(other.Train) == true;

    public override bool Equals(object? obj) => obj is StationCall other && Equals(other);
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
