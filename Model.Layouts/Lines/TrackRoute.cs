using Tellurian.Trains.Schedules.Model.Layouts.Tracks;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The shortest way along the drawn tracks from the centre of one location to the centre of the next
/// (<c>LayoutsLayerSurvey.md</c> 3.3, step 2, and 3.4), as pieces in order. A train can run it without reversing.
/// </summary>
/// <param name="Pieces">The pieces in order from the start.</param>
public sealed record TrackRoute(IReadOnlyList<TrackRoutePiece> Pieces)
{
    /// <summary>The length of the route, metres.</summary>
    public double Distance => Pieces.Sum(p => p.Length) / StationTrackMeasurement.MillimetresPerMetre;

    /// <summary>The smallest radius along the route, mm; null when it is straight all the way.</summary>
    public double? SmallestRadius => Pieces.Min(p => p.Radius);

    /// <summary>The length of the route in each module, mm, in the order the route runs through them.</summary>
    public IReadOnlyList<(int ModuleId, double Length)> LengthPerModule =>
        [.. Pieces.GroupBy(p => p.ModuleId).Select(g => (g.Key, g.Sum(p => p.Length)))];
}
