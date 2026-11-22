using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tracks;

/// <summary>A curved track from <see cref="Start"/>, turning <see cref="Angle"/> degrees (positive left).</summary>
public sealed class CurvedTrack : TrackElement
{
    /// <summary>Start of the track, in module coordinates, heading into the curve.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Start { get; set; }
    /// <summary>Radius of the curve centre line, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Radius { get; set; }
    /// <summary>How far the curve turns, degrees; positive turns left (counter-clockwise), negative right.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Angle { get; set; }

    /// <inheritdoc/>
    public override IReadOnlyList<SegmentPath> Routes() => [new PathBuilder(Start).Curve(Radius, Angle).Build()];

    /// <inheritdoc/>
    public override IReadOnlyList<Pose> ConnectionPoints() => [Start.Reversed(), Routes()[0].EndPose];
}
