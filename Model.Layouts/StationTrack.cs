using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A track of an operation location, or of plain line, as train planning sees it. Lengths are in metres of the model.
/// </summary>
public sealed class StationTrack : LayoutPart, IEquatable<StationTrack>
{
    /// <summary>
    /// The platform length in metres given to the tracks of a location that exchanges passengers but has no platform
    /// recorded anywhere: a plan written before platforms existed.
    /// </summary>
    public const double DefaultPlatformLength = 1;

    /// <summary>Creates a track with no designation yet, to be given with an object initializer. It is scheduled.</summary>
    [JsonConstructor]
    public StationTrack()
    {
        Designation = string.Empty;
    }

    /// <summary>Creates a scheduled main track.</summary>
    /// <param name="id">The identity of the track.</param>
    /// <param name="designation">What the track is called, e.g. "1".</param>
    public StationTrack(int id, string designation) : this(id, designation, true, true) { }

    /// <summary>Creates a track.</summary>
    /// <param name="id">The identity of the track.</param>
    /// <param name="designation">What the track is called, e.g. "1".</param>
    /// <param name="isMain">Whether it is a main track.</param>
    /// <param name="isScheduled">Whether trains are planned to use it.</param>
    public StationTrack(int id, string designation, bool isMain, bool isScheduled)
    {
        Id = id;
        Designation = designation;
        IsMain = isMain;
        IsScheduled = isScheduled;
    }

    /// <summary>
    /// The identity of the track, which a station call refers to. It stays when the designation is changed. Unique
    /// within the module in a module file, and within the layout in a layout.
    /// </summary>
    public int Id { get; set; }

    /// <summary>What the track is called, e.g. "1" or "2a". Unique within its location, without regard to case.</summary>
    public string Designation { get; set; }

    /// <summary>The place of the track in lists, lowest first.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Trains are planned to use the track.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool IsScheduled { get; set; } = true;

    /// <summary>A main track, as opposed to a siding.</summary>
    public bool IsMain { get; set; }

    /// <summary>
    /// The length a train can use, in metres. At a turnout the track ends at the fouling point, where a train on the
    /// other track can still pass freely, not at the points.
    /// </summary>
    public double Length { get; set; }

    /// <summary>
    /// The <see cref="Length"/> was entered by the user and is kept; otherwise it follows the module's drawing, when it
    /// has one (<c>StationTrackMeasurement</c>).
    /// </summary>
    public bool IsLengthEntered { get; set; }

    /// <summary>
    /// The track is entered from one end only, a stub or siding: every vehicle goes in and out at the same end, so the
    /// last one in is the first that can leave.
    /// </summary>
    public bool IsStub { get; set; }

    /// <summary>
    /// Whether the track is a stub (<see cref="IsStub"/>) was chosen by the user and is kept; otherwise it follows the
    /// module's drawing, when it has one (<c>StationTrackMeasurement</c>).
    /// </summary>
    public bool IsStubEntered { get; set; }

    /// <summary>
    /// The length in metres of the platform along the track; zero where it has none. Passengers are exchanged only at a
    /// track with a platform.
    /// </summary>
    public double PlatformLength { get; set; }

    /// <summary>
    /// A train standing on the track can be locked in: kept there, switches locked, so that other trains may pass or
    /// meet it on the line.
    /// </summary>
    public bool IsLockable { get; set; }

    /// <summary>The name of the key that locks the track, e.g. "K1"; null when none is given. Only for a lockable track.</summary>
    public string? KeyName { get; set; }

    /// <summary>What the track is used for, e.g. "Loading".</summary>
    public string Usage { get; set; } = string.Empty;

    /// <summary>The direction trains run on the track, as seen from the viewing side of the module.</summary>
    public TrackDirection Direction { get; set; }

    /// <summary>The speed limit on the track, km/h; null when it has none of its own.</summary>
    public int? SpeedLimit { get; set; }

    /// <summary>
    /// The <see cref="OperationLocation.Id"/> of the location a train comes from for this to be its track, or
    /// <c>null</c> where the track says nothing about where a train comes from. With <see cref="NextLocationId"/>, it
    /// says which way through the location the track is for.
    /// </summary>
    public int? PreviousLocationId { get; set; }

    /// <summary>The <see cref="OperationLocation.Id"/> of the location a train goes on to for this to be its track; see <see cref="PreviousLocationId"/>.</summary>
    public int? NextLocationId { get; set; }

    /// <summary>The track's route counts the other way round as well: a track for trains from A to B is then equally the track for trains from B to A.</summary>
    public bool AppliesInBothDirections { get; set; }

    /// <summary>The foreign key of the location the track belongs to. Not written: the location owns the track.</summary>
    [JsonIgnore]
    public int StationId { get; set; }

    /// <summary>The location the track belongs to. Not written: the location owns the track.</summary>
    [JsonIgnore]
    public OperationLocation Station { get; set; } = default!;

    /// <summary>A copy of the track, still of the same location until the location's copy takes it.</summary>
    public override StationTrack Copy() => (StationTrack)base.Copy();

    /// <inheritdoc/>
    public bool Equals(StationTrack? other) => other is not null && Designation.Equals(other.Designation, StringComparison.OrdinalIgnoreCase) && Equals(Station, other.Station);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is StationTrack other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Designation.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Designation;
}

/// <summary>What follows from the properties of a <see cref="StationTrack"/>.</summary>
public static class StationTrackFacts
{
    extension(StationTrack track)
    {
        /// <summary>The track has a platform: its <see cref="StationTrack.PlatformLength"/> is greater than zero.</summary>
        public bool HasPlatform => track.PlatformLength > 0;
    }
}
