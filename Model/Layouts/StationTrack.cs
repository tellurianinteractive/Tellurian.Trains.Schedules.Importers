using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Represents a track at a station where trains can stop or pass.
/// </summary>
public sealed class StationTrack : IEquatable<StationTrack>
{
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
    /// Gets or sets the usage description for this track.
    /// </summary>
    public string Usage { get; set; } = string.Empty;

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
