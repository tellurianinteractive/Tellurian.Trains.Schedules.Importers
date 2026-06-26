namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Consistency checks for the stretch graph, used by the planning UI to surface warnings while the
/// user edits track and timetable stretches.
/// </summary>
public static class LayoutStretchValidationExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// Finds groups of track stretches whose defined directions are inconsistent. All track stretches
        /// are expected to be defined in the same direction; any directed cycle in the <c>Start → End</c>
        /// graph breaks that rule. A stretch pair defined in both directions (<c>A → B</c> and
        /// <c>B → A</c>) shows up as a two-stretch cycle. Each returned list is one cycle in traversal
        /// order. An empty result means all track stretches are consistently directed.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<TrackStretch>> DirectionInconsistencies()
        {
            var adjacency = layout.TrackStretches
                .GroupBy(s => s.Start)
                .ToDictionary(g => g.Key, g => g.ToList());

            // 0 = unvisited, 1 = on the current DFS path, 2 = fully explored.
            var state = new Dictionary<OperationLocation, int>();
            var pathEdges = new List<TrackStretch>();
            var cycles = new List<IReadOnlyList<TrackStretch>>();
            var seen = new HashSet<string>();

            void Visit(OperationLocation node)
            {
                state[node] = 1;
                if (adjacency.TryGetValue(node, out var edges))
                {
                    foreach (var edge in edges)
                    {
                        var next = edge.End;
                        switch (state.GetValueOrDefault(next))
                        {
                            case 0:
                                pathEdges.Add(edge);
                                Visit(next);
                                pathEdges.RemoveAt(pathEdges.Count - 1);
                                break;
                            case 1:
                                // Back edge: a cycle runs from the first path edge that starts at 'next'
                                // through to this closing edge.
                                var startIndex = pathEdges.FindIndex(e => e.Start.Equals(next));
                                var cycle = startIndex >= 0
                                    ? pathEdges.GetRange(startIndex, pathEdges.Count - startIndex)
                                    : [];
                                cycle.Add(edge);
                                var key = string.Join("|", cycle.Select(c => c.Id).OrderBy(id => id));
                                if (seen.Add(key)) cycles.Add(cycle);
                                break;
                        }
                    }
                }
                state[node] = 2;
            }

            foreach (var node in adjacency.Keys)
                if (state.GetValueOrDefault(node) == 0) Visit(node);

            return cycles;
        }
    }
}
