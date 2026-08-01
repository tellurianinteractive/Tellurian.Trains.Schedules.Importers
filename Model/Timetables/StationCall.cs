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
    /// Gets the station where this call occurs. Read from the track the call is on, so it is never
    /// stored separately.
    /// </summary>
    [JsonIgnore]
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
    /// A call is a stop when it arrives and/or departs <em>and</em> the train is able to stop at its
    /// location at all (see <c>CanBeStop</c>) — a train never stops at a signal-controlled location, and
    /// stops elsewhere only where it can exchange what it carries. Those location rules are built in here
    /// as the single source of truth, so nothing has to remember to apply them.
    /// It never compares the arrival and departure times: equal times are only a convention used
    /// by the XPLN import to decide whether to clear both flags (see <see cref="IsPassthrough"/>).
    /// </summary>
    /// <remarks>
    /// The flags themselves are left as the planner set them, never cleared, so a stop planned while a
    /// location still exchanged passengers or cargo comes back if the exchange is restored. Only the
    /// answer given here changes with the location.
    /// </remarks>
    [JsonIgnore]
    public bool IsStop => (IsArrival || IsDeparture) && this.CanBeStop;

    /// <summary>
    /// Gets a value indicating whether the train passes the station without stopping.
    /// A pass-through has neither <see cref="IsArrival"/> nor <see cref="IsDeparture"/> set
    /// (so <see cref="IsStop"/> is <c>false</c>), regardless of its arrival and departure times.
    /// The XPLN import expresses a pass-through this way for an intermediate call whose arrival
    /// equals its departure; manual editing does so by clearing both the Arr and Dep flags.
    /// </summary>
    [JsonIgnore]
    public bool IsPassthrough => !IsStop;

    /// <summary>
    /// Gets the time used for sorting (departure if this is a departure, otherwise arrival).
    /// </summary>
    [JsonIgnore]
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
        /// True when the train is able to stop at this call's location at all — the location is not
        /// signal controlled and it exchanges what this train carries (see <c>Train.CanStopAt</c>).
        /// Where this is false the call can only ever be a pass-through, whatever
        /// <see cref="StationCall.IsArrival"/> and <see cref="StationCall.IsDeparture"/> say, so the
        /// editor shows those two cleared and disabled.
        /// </summary>
        /// <remarks>
        /// What may be exchanged is a property of the train, so a call not yet joined to one is judged on
        /// its location alone.
        /// </remarks>
        public bool CanBeStop =>
            call.Train is { } train
                ? train.CanStopAt(call.OperationLocation)
                : call.OperationLocation is not SignalControlledLocation;

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

        /// <summary>
        /// True when this is the train's first call, where it starts its run. Nothing precedes it, so the
        /// gap from its arrival to its departure is the train's preparation time.
        /// </summary>
        /// <remarks>See the <c>IsAtTrainEnd</c> extension for how the ends are determined.</remarks>
        public bool IsTrainOrigin =>
            call.Train.IsNullOrHasNoCalls() || ReferenceEquals(call, call.Train.CallsInRunOrder[0]);

        /// <summary>
        /// True when this is the train's last call, where it ends its run. Nothing follows it, so the gap
        /// from its arrival to its departure is the train's finishing-up time.
        /// </summary>
        /// <remarks>See the <c>IsAtTrainEnd</c> extension for how the ends are determined.</remarks>
        public bool IsTrainDestination =>
            call.Train.IsNullOrHasNoCalls() || ReferenceEquals(call, call.Train.CallsInRunOrder[^1]);

        /// <summary>
        /// The time from which a working that starts at this call ties up the vehicles and the driver. At
        /// the train's origin that is the <see cref="StationCall.Arrival"/>: preparing the train is real
        /// work, done before it moves. Anywhere else the working starts when the train departs.
        /// </summary>
        public Time WorkStart => call.IsTrainOrigin ? call.Arrival : call.Departure;

        /// <summary>
        /// The time until which a working that ends at this call ties up the vehicles and the driver. At
        /// the train's destination that is the <see cref="StationCall.Departure"/>: finishing the train up
        /// is real work, done after it has stopped. Anywhere else the working ends when the train arrives.
        /// </summary>
        public Time WorkEnd => call.IsTrainDestination ? call.Departure : call.Arrival;
    }
}
