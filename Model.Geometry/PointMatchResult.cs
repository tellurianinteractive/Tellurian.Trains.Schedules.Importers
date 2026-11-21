namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>The result of <see cref="PointMatching.Match"/>.</summary>
public sealed record PointMatchResult(
    IReadOnlyList<PointPair> Pairs,
    IReadOnlyList<int> UnmatchedFirst,
    IReadOnlyList<int> UnmatchedSecond)
{
    /// <summary>True when every point in both sets has a partner.</summary>
    public bool IsComplete => UnmatchedFirst.Count == 0 && UnmatchedSecond.Count == 0;
}
