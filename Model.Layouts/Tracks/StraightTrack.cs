using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tracks;

/// <summary>A straight track from <see cref="Start"/>, in its heading.</summary>
public sealed class StraightTrack : TrackElement
{
    /// <summary>Start of the track, in module coordinates, heading along the track.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Start { get; set; }
    /// <summary>Length of the track, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Length { get; set; }

    /// <inheritdoc/>
    public override IReadOnlyList<SegmentPath> Routes() => [new PathBuilder(Start).Straight(Length).Build()];

    /// <inheritdoc/>
    public override IReadOnlyList<Pose> ConnectionPoints() => [Start.Reversed(), Start.Advanced(Length)];
}
