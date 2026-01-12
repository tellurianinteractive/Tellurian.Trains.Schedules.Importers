using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a scheduled stop or passage of a train at a station track.
/// </summary>
/// <remarks>
/// A station call records the arrival and departure times of a train at a specific track,
/// along with notes and flags indicating whether it's an arrival, departure, or both.
/// </remarks>
public sealed class StationCall : IEquatable<StationCall>, IComparable<StationCall>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private StationCall()
    {
        Track = default!;
        Notes = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StationCall"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the station call.</param>
    /// <param name="track">The station track where the call occurs.</param>
    /// <param name="arrival">The arrival time.</param>
    /// <param name="departure">The departure time.</param>
    /// <param name="remark">An optional remark for the call.</param>
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

    /// <summary>
    /// Gets or sets the unique identifier for this station call.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the station track.
    /// </summary>
    public int TrackId { get; set; }

    /// <summary>
    /// Gets or sets the station track where this call occurs.
    /// </summary>
    public StationTrack Track { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the train.
    /// </summary>
    public int TrainId { get; set; }

    /// <summary>
    /// Gets or sets the train making this station call.
    /// </summary>
    public Train Train { get; set; } = default!;

    /// <summary>
    /// Gets the station where this call occurs.
    /// </summary>
    public OperationLocation Station => Track.Station;

    /// <summary>
    /// Gets or sets the arrival time at the station.
    /// </summary>
    public Time Arrival { get; set; }

    /// <summary>
    /// Gets or sets the departure time from the station.
    /// </summary>
    public Time Departure { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is an arrival (train terminates or passengers can alight).
    /// </summary>
    public bool IsArrival { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this is a departure (train originates or passengers can board).
    /// </summary>
    public bool IsDeparture { get; set; }

    /// <summary>
    /// Gets or sets the collection of notes associated with this call.
    /// </summary>
    public ICollection<CallNote> Notes { get; set; }

    /// <summary>
    /// Gets a value indicating whether this is a scheduled stop (either arrival or departure).
    /// </summary>
    public bool IsStop => IsArrival || IsDeparture;

    /// <summary>
    /// Gets the time used for sorting (departure if this is a departure, otherwise arrival).
    /// </summary>
    public Time SortTime => IsDeparture ? Departure : Arrival;

    /// <summary>
    /// Sets the train reference for this station call.
    /// </summary>
    /// <param name="train">The train to associate with this call.</param>
    internal void SetTrain(Train train)
    {
        Train = train;
        TrainId = train.Id;
    }

    /// <inheritdoc/>
    public bool Equals(StationCall? other) =>
         other != null &&
         Arrival.Equals(other.Arrival) &&
         Departure.Equals(other.Departure) &&
         Track.Equals(other.Track) &&
         Train?.Equals(other.Train) == true;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is StationCall other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Arrival, Departure, Track, Train);

    /// <inheritdoc/>
    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, Strings.CallAtStationTrackDuringTimes, Station, Track, Arrival.HHMM(), Departure.HHMM());

    /// <inheritdoc/>
    public int CompareTo([AllowNull] StationCall other) =>
        other is null ? 1 : SortTime.CompareTo(other.SortTime);

    /// <summary>
    /// Determines whether the first station call is earlier than the second.
    /// </summary>
    public static bool operator <(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) == -1;

    /// <summary>
    /// Determines whether the first station call is later than the second.
    /// </summary>
    public static bool operator >(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) == 1;

    /// <summary>
    /// Determines whether the first station call is earlier than or equal to the second.
    /// </summary>
    public static bool operator <=(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) >= 0;

    /// <summary>
    /// Determines whether the first station call is later than or equal to the second.
    /// </summary>
    public static bool operator >=(StationCall? call1, StationCall? call2) => call1?.CompareTo(call2) <= 0;
}
