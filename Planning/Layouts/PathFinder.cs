using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Layouts;

/// <summary>
/// Finds the shortest path between two locations in a layout,
/// respecting direction change constraints at intermediate locations.
/// </summary>
public static class PathFinder
{
    /// <summary>
    /// Finds the shortest path by distance between two locations.
    /// Direction changes are only permitted at locations where
    /// <see cref="OperationLocation.IsChangingTrainDirectionPossible"/> is true.
    /// </summary>
    /// <param name="layout">The layout to search.</param>
    /// <param name="from">The origin location.</param>
    /// <param name="to">The destination location.</param>
    /// <returns>The shortest path, or null if no path exists.</returns>
    public static TrainPath? FindShortestPath(Layout layout, OperationLocation from, OperationLocation to)
    {
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (from.Equals(to)) return null;

        // Build adjacency: for each location, list of (TrackStretch, Direction, Neighbour)
        var edges = BuildEdges(layout);

        // Dijkstra with state = (location, last direction) to enforce direction change constraints.
        // last direction is null at the start (no constraint on first segment).
        var dist = new Dictionary<(string Signature, TrainPathDirection? LastDir), double>();
        var prev = new Dictionary<(string Signature, TrainPathDirection? LastDir), (string Signature, TrainPathDirection? LastDir, TrainPathSegment Segment)>();
        var queue = new PriorityQueue<(string Signature, TrainPathDirection? LastDir), double>();

        var startKey = (from.Signature, (TrainPathDirection?)null);
        dist[startKey] = 0;
        queue.Enqueue(startKey, 0);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current.Signature.Equals(to.Signature, StringComparison.OrdinalIgnoreCase))
            {
                return ReconstructPath(prev, current, startKey);
            }

            if (!edges.TryGetValue(current.Signature, out var neighbours)) continue;

            var currentDist = dist[current];

            foreach (var (stretch, direction, neighbourSig) in neighbours)
            {
                // Check direction change constraint
                if (current.LastDir.HasValue && direction != current.LastDir.Value)
                {
                    // Direction change — only allowed if current location permits it
                    var currentLocation = direction == TrainPathDirection.Forward ? stretch.Start : stretch.End;
                    if (!currentLocation.IsChangingTrainDirectionPossible) continue;
                }

                var newDist = currentDist + stretch.Distance;
                var neighbourKey = (neighbourSig, (TrainPathDirection?)direction);

                if (!dist.TryGetValue(neighbourKey, out var existingDist) || newDist < existingDist)
                {
                    dist[neighbourKey] = newDist;
                    prev[neighbourKey] = (current.Signature, current.LastDir, new TrainPathSegment(stretch, direction));
                    queue.Enqueue(neighbourKey, newDist);
                }
            }
        }

        return null; // No path found
    }

    private static Dictionary<string, List<(TrackStretch Stretch, TrainPathDirection Direction, string NeighbourSig)>> BuildEdges(Layout layout)
    {
        var edges = new Dictionary<string, List<(TrackStretch, TrainPathDirection, string)>>(StringComparer.OrdinalIgnoreCase);

        void AddEdge(string fromSig, TrackStretch stretch, TrainPathDirection dir, string toSig)
        {
            if (!edges.TryGetValue(fromSig, out var list))
            {
                list = [];
                edges[fromSig] = list;
            }
            list.Add((stretch, dir, toSig));
        }

        foreach (var stretch in layout.TrackStretches)
        {
            // Forward: Start → End
            AddEdge(stretch.Start.Signature, stretch, TrainPathDirection.Forward, stretch.End.Signature);
            // Backward: End → Start
            AddEdge(stretch.End.Signature, stretch, TrainPathDirection.Backward, stretch.Start.Signature);
        }

        return edges;
    }

    private static TrainPath ReconstructPath(
        Dictionary<(string Signature, TrainPathDirection? LastDir), (string Signature, TrainPathDirection? LastDir, TrainPathSegment Segment)> prev,
        (string Signature, TrainPathDirection? LastDir) endKey,
        (string Signature, TrainPathDirection? LastDir) startKey)
    {
        var segments = new List<TrainPathSegment>();
        var current = endKey;

        while (!current.Equals(startKey))
        {
            var (prevSig, prevDir, segment) = prev[current];
            segments.Add(segment);
            current = (prevSig, prevDir);
        }

        segments.Reverse();
        return new TrainPath(segments);
    }
}
