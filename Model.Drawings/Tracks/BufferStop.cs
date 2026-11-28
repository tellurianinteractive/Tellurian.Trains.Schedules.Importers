using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Tracks;

/// <summary>
/// A buffer stop at the end of a track. <see cref="Pose"/> is its connection point:
/// the track end, with the heading pointing from the stop back along the track.
/// </summary>
public sealed class BufferStop : TrackElement
{
    /// <summary>The connection point at the track end, in module coordinates; the heading points from the stop back along the track.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Pose { get; set; }

    /// <inheritdoc/>
    public override IReadOnlyList<SegmentPath> Routes() => [];

    /// <inheritdoc/>
    public override IReadOnlyList<Pose> ConnectionPoints() => [Pose];
}
