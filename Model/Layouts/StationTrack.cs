using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Represents a track at a station where trains can stop or pass.
/// </summary>
public sealed class StationTrack : IEquatable<StationTrack>
{
    /// <summary>
    /// The platform length in metres given to the tracks of a location that exchanges passengers but has
    /// no platform recorded anywhere — a plan written before platforms existed. See
    /// <c>Layout.EnsurePlatforms</c>.
    /// </summary>
    public const double DefaultPlatformLength = 1;

    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private StationTrack()
    {
        Number = string.Empty;
        Calls = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="StationTrack"/> with the specified id and number.
    /// </summary>
    /// <param name="id">The unique identifier for the track.</param>
    /// <param name="number">The track number or designation.</param>
    public StationTrack(int id, string number) : this(id, number, true, true) { }

    /// <summary>
    /// Initializes a new instance of <see cref="StationTrack"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the track.</param>
    /// <param name="number">The track number or designation.</param>
    /// <param name="isMain">Whether this is a main track.</param>
    /// <param name="isScheduled">Whether this track is included in scheduling.</param>
    public StationTrack(int id, string number, bool isMain, bool isScheduled)
    {
        Id = id;
        Number = number;
        IsMain = isMain;
        IsScheduled = isScheduled;
        Calls = [];
    }

    /// <summary>
    /// Gets or sets the unique identifier for this track.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the track number or designation.
    /// </summary>
    public string Number { get; set; }

    /// <summary>
    /// Gets or sets the display order of this track relative to other tracks at the same station.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this track is included in scheduling.
    /// </summary>
    public bool IsScheduled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this is a main track.
    /// </summary>
    public bool IsMain { get; set; }

    /// <summary>
    /// Gets or sets the length of this track in meters.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// Gets or sets the length in metres, to one decimal, of the platform along this track; zero where
    /// the track has no platform. Only meaningful where the location exchanges passengers.
    /// </summary>
    /// <remarks>
    /// Passengers can be exchanged only at a track with a platform (see <c>HasPassengerExchange</c>), so
    /// this is what decides which of a location's tracks a passenger train is put on. A passenger train
    /// may still stand on a track without one — it just exchanges nothing there, as when it meets another
    /// train at a location that exchanges no passengers at all.
    /// </remarks>
    public double PlatformLength { get; set; }

    /// <summary>
    /// Gets or sets the usage description for this track.
    /// </summary>
    public string Usage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the <see cref="OperationLocation.Id"/> of the location a train comes from for this
    /// to be its track, or <c>null</c> where the track says nothing about where a train comes from.
    /// </summary>
    /// <remarks>
    /// This and <see cref="NextLocationId"/> are how the planner says which way through the location a
    /// track is for — the down track of a double line, say, or the platform road a branch train uses.
    /// They only mean anything where the location has more than one track to choose between, which is
    /// why they are offered nowhere else. See <c>OperationLocation.PreferredTrack</c>, which is what
    /// reads them.
    /// </remarks>
    public int? PreviousLocationId { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="OperationLocation.Id"/> of the location a train goes on to for this
    /// to be its track, or <c>null</c> where the track says nothing about where a train goes on to.
    /// See <see cref="PreviousLocationId"/>.
    /// </summary>
    public int? NextLocationId { get; set; }

    /// <summary>
    /// Gets or sets whether the track's route counts the other way round as well: a track for trains
    /// from A to B is then equally the track for trains from B to A. Leave it off where the two
    /// directions have a track each, as on a double line.
    /// </summary>
    public bool AppliesInBothDirections { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the station.
    /// </summary>
    public int StationId { get; set; }

    /// <summary>
    /// Gets or sets the station this track belongs to.
    /// </summary>
    public OperationLocation Station { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collection of station calls on this track.
    /// </summary>
    public ICollection<StationCall> Calls { get; set; }

    /// <inheritdoc/>
    public bool Equals(StationTrack? other) => Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase) && Station.Equals(other?.Station);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is StationTrack other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Number;

    /// <summary>
    /// Gets an example station track for testing purposes.
    /// </summary>
    public static StationTrack Example { get { return new StationTrack(1, "1") { Station = OperationLocation.Example }; } }
}

/// <summary>
/// Provides extension methods for <see cref="StationTrack"/>.
/// </summary>
public static class StationTrackExtensions
{
    extension(StationTrack track)
    {
        /// <summary>
        /// Whether this track has a platform alongside it, which is what
        /// <see cref="StationTrack.PlatformLength"/> being greater than zero says.
        /// </summary>
        public bool HasPlatform => track.PlatformLength > 0;

        /// <summary>
        /// Whether passengers can be exchanged at this track: the location exchanges passengers at all
        /// (see <see cref="OperationLocation.HasPassengerExchange"/>) and this track has a platform for
        /// them to get on and off at.
        /// </summary>
        /// <remarks>
        /// This is a property of the track, not of the train standing on it. A passenger train may
        /// perfectly well stand at a track where this is false — meeting another train at a location
        /// that exchanges no passengers, for one — it simply exchanges nothing while it is there.
        /// </remarks>
        public bool HasPassengerExchange =>
            track.Station is { HasPassengerExchange: true } && track.HasPlatform;

        /// <summary>
        /// Whether this track is meant for trains running a particular way through the location: it names
        /// where such a train comes from (<see cref="StationTrack.PreviousLocationId"/>), where it goes on
        /// to (<see cref="StationTrack.NextLocationId"/>), or both.
        /// </summary>
        public bool HasRoute =>
            track.PreviousLocationId.HasValue || track.NextLocationId.HasValue;

        /// <summary>
        /// How well this track's route fits a train that comes from <paramref name="previousLocationId"/>
        /// and goes on to <paramref name="nextLocationId"/> — either of them <c>null</c> where the train
        /// starts or ends its run here. The bigger the number the better the fit, and <c>null</c> says the
        /// train's route contradicts one of the track's named locations, so this is not its track at all.
        /// </summary>
        /// <remarks>
        /// A named location the train's route confirms is what makes a track the one to use, so it weighs
        /// more than anything else; a named location the train has none of — it starts or ends its run
        /// here — counts a little against the track, leaving it behind one that names only what the train
        /// does confirm. A track naming no location comes to zero: it fits every train, claiming and
        /// contradicting none.
        /// </remarks>
        /// <param name="previousLocationId">The <see cref="OperationLocation.Id"/> the train comes from, or <c>null</c> where it starts here.</param>
        /// <param name="nextLocationId">The <see cref="OperationLocation.Id"/> the train goes on to, or <c>null</c> where it ends here.</param>
        public int? RouteMatch(int? previousLocationId, int? nextLocationId)
        {
            var asNamed = Fit(previousLocationId, nextLocationId);
            if (!track.AppliesInBothDirections) return asNamed;
            var reversed = Fit(nextLocationId, previousLocationId);
            return asNamed is null ? reversed :
                reversed is null ? asNamed :
                Math.Max(asNamed.Value, reversed.Value);

            // Both ends have to hold at once, so a fit is the two sides added together — and nothing at
            // all as soon as either side is contradicted.
            int? Fit(int? from, int? to) =>
                Side(track.PreviousLocationId, from) is { } fromSide && Side(track.NextLocationId, to) is { } toSide
                    ? fromSide + toSide
                    : null;

            // What one side of the track's route is worth against the train's.
            static int? Side(int? named, int? actual) =>
                named is null ? 0 :     // the track says nothing about this side: neither for nor against
                actual is null ? -1 :   // the train has no location here, starting or ending its run
                named == actual ? 2 :   // the train's route confirms the track's
                null;                   // the route contradicts it: not this train's track
        }
    }

    /// <summary>
    /// Adds a station call to the track.
    /// </summary>
    /// <param name="me">The track to add the call to.</param>
    /// <param name="call">The station call to add.</param>
    /// <returns>The added station call.</returns>
    internal static StationCall Add(this StationTrack me, StationCall call)
    {
        ArgumentNullException.ThrowIfNull(call);
        if (!me.Calls.Contains(call))
        {
            me.Calls.Add(call);
        }
        return call;
    }
}
