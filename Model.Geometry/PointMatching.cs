namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Matches two sets of points within a tolerance, e.g. the screw holes or track positions of two
/// facing end profiles. Each point is matched at most once.
/// </summary>
public static class PointMatching
{
    /// <summary>
    /// Pairs points of <paramref name="first"/> and <paramref name="second"/> that are at most
    /// <paramref name="tolerance"/> apart, closest pairs first, and reports the unmatched points by index.
    /// </summary>
    public static PointMatchResult Match(IReadOnlyList<Point> first, IReadOnlyList<Point> second, double tolerance)
    {
        if (tolerance < 0) throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "The tolerance must not be negative.");
        var candidates = new List<PointPair>();
        for (var i = 0; i < first.Count; i++)
        {
            for (var j = 0; j < second.Count; j++)
            {
                var distance = first[i].DistanceTo(second[j]);
                if (distance <= tolerance) candidates.Add(new(i, j, distance));
            }
        }
        var usedFirst = new bool[first.Count];
        var usedSecond = new bool[second.Count];
        var pairs = new List<PointPair>();
        foreach (var candidate in candidates.OrderBy(c => c.Distance).ThenBy(c => c.FirstIndex).ThenBy(c => c.SecondIndex))
        {
            if (usedFirst[candidate.FirstIndex] || usedSecond[candidate.SecondIndex]) continue;
            usedFirst[candidate.FirstIndex] = true;
            usedSecond[candidate.SecondIndex] = true;
            pairs.Add(candidate);
        }
        return new(
            [.. pairs.OrderBy(p => p.FirstIndex)],
            [.. Enumerable.Range(0, first.Count).Where(i => !usedFirst[i])],
            [.. Enumerable.Range(0, second.Count).Where(j => !usedSecond[j])]);
    }
}
