using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Timetables;

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
        // A call does not register itself on the track here. The single owner of a call is its train
        // (see Train.Add), which registers the call on its track when the call joins the train; the
        // per-track index is otherwise rebuilt from the timetable's trains (Timetable.RebuildStationCalls).
        // Registering at construction would put a call on a track before (or without) it ever joining a
        // train, which is exactly how orphaned track calls arise.
        Arrival = arrival;
        Departure = departure;
        Notes = [];
        if (!string.IsNullOrWhiteSpace(remark))
        {
            Notes.Add(new TextCallNote(remark, "") { IsDriverNote = true, IsShuntingNote = true, IsStationNote = true });
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
    public OperationLocation OperationLocation => Track.Station;

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
    /// Gets a value indicating whether the train stops here, as opposed to passing through.
    /// A call is a stop when it arrives and/or departs <em>and</em> its location is not a
    /// <see cref="Layouts.SignalControlledLocation"/> — a train never stops at a signal-controlled
    /// location, so the location override is built in here as the single source of truth.
    /// It never compares the arrival and departure times: equal times are only a convention used
    /// by the XPLN import to decide whether to clear both flags (see <see cref="IsPassthrough"/>).
    /// </summary>
    public bool IsStop => (IsArrival || IsDeparture) && OperationLocation is not SignalControlledLocation;

    /// <summary>
    /// Gets a value indicating whether the train passes the station without stopping.
    /// A pass-through has neither <see cref="IsArrival"/> nor <see cref="IsDeparture"/> set
    /// (so <see cref="IsStop"/> is <c>false</c>), regardless of its arrival and departure times.
    /// The XPLN import expresses a pass-through this way for an intermediate call whose arrival
    /// equals its departure; manual editing does so by clearing both the Arr and Dep flags.
    /// </summary>
    public bool IsPassthrough => !IsStop;

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
        string.Format(CultureInfo.CurrentCulture, Strings.CallAtStationTrackDuringTimes, OperationLocation, Track, Arrival.HHMM(), Departure.HHMM());

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

/// <summary>
/// 
/// </summary>
public static class StationCallExtensions
{
    extension(StationCall call)
    {
        /// <summary>
        /// True then this is a call where trains stops and with an arrival time less that departure time.
        /// </summary>
        public bool HasDifferentArrivalAndDepartureTimes => call.IsStop && call.Arrival < call.Departure;

        /// <summary>
        /// True when this is the train's first or last call, where it starts or ends its run. The calls
        /// in between are the operating locations the train passes on the way, which it cannot skip.
        /// </summary>
        /// <remarks>
        /// The ends are the first and last of <c>Train.CallsInRunOrder</c>, not of
        /// <see cref="Train.Calls"/>, which is in insertion order. Calls are matched by identity, because
        /// two calls of the same train can compare equal.
        /// </remarks>
        public bool IsAtTrainEnd
        {
            get
            {
                if (call.Train.IsNullOrHasNoCalls()) return true;
                var ordered = call.Train.CallsInRunOrder;
                return ReferenceEquals(call, ordered[0]) || ReferenceEquals(call, ordered[^1]);
            }
        }
    }
}
