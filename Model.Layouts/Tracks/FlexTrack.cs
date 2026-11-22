using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tracks;

/// <summary>
/// A track along any path of lines and arcs, e.g. a generated through track with
/// concentric arcs or reverse curves.
/// </summary>
public sealed class FlexTrack : TrackElement
{
    /// <summary>The centre line of the track, in module coordinates.</summary>
    public required PathDrawing Path { get; set; }

    /// <inheritdoc/>
    public override IReadOnlyList<SegmentPath> Routes() => [Path.ToPath()];

    /// <inheritdoc/>
    public override IReadOnlyList<Pose> ConnectionPoints()
    {
        var path = Path.ToPath();
        return [path.StartPose.Reversed(), path.EndPose];
    }
}
