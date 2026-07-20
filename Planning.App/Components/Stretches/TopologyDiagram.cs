namespace Tellurian.Trains.Schedules.Planning.App.Components.Stretches;

/// <summary>
/// A station drawn as a circle on a timetable-stretch line, positioned proportionally to its distance
/// from the start of the stretch. A <see cref="Hidden"/> node is a junction shared with a parent line: it
/// still anchors this line's end, but its circle and label are drawn on the parent only, not repeated here.
/// </summary>
public sealed record TopologyNode(double X, double Y, string Signature, bool Hidden = false);

/// <summary>
/// A timetable stretch drawn as a horizontal line with its stations as evenly-distance-spaced nodes.
/// </summary>
public sealed record TopologyLine(string Number, string Description, double Y, IReadOnlyList<TopologyNode> Nodes)
{
    /// <summary>The x-coordinate of the first station on the line.</summary>
    public double StartX => Nodes.Count > 0 ? Nodes[0].X : 0.0;

    /// <summary>The x-coordinate of the last station on the line.</summary>
    public double EndX => Nodes.Count > 0 ? Nodes[^1].X : 0.0;
}

/// <summary>
/// A 45°/-45° line linking a branching stretch to the junction it diverges from (or merges into) on its parent line.
/// </summary>
public sealed record TopologyConnector(double X1, double Y1, double X2, double Y2);

/// <summary>
/// A laid-out topology of a layout's timetable stretches: horizontal lines with station circles, and
/// diagonal connectors where one stretch branches off another. Coordinates are in SVG user units.
/// </summary>
public sealed record TopologyDiagram(
    double Width,
    double Height,
    IReadOnlyList<TopologyLine> Lines,
    IReadOnlyList<TopologyConnector> Connectors)
{
    private const double LeftMargin = 56.0;
    private const double RightMargin = 32.0;
    private const double TopMargin = 40.0;
    private const double BottomMargin = 28.0;
    private const double RowGap = 72.0;
    private const double TargetSpan = 640.0;

    // Which end of a branching stretch sits on its parent line: its start (it leads out, drawn +45°)
    // or its end (it leads into, drawn -45°).
    private enum JunctionSide { Start, End }

    /// <summary>
    /// Lays out the layout's timetable stretches. Root lines are stacked from the top; a stretch that
    /// diverges from another is placed on a row below the line it leaves and linked by a diagonal connector.
    /// </summary>
    public static TopologyDiagram Build(global::Tellurian.Trains.Schedules.Model.Layouts.Layout layout)
    {
        var all = layout.TimetableStretches.Where(s => s.Stretches.Count > 0).OrderBy(s => s.Id).ToList();
        if (all.Count == 0) return new TopologyDiagram(LeftMargin + RightMargin, TopMargin + BottomMargin, [], []);

        var parent = BuildParentMap(all);
        var order = OrderDepthFirst(all, parent);

        var maxSpan = all.Max(s => s.Stretches.Sum(t => t.Distance));
        var pxPerUnit = maxSpan > 0 ? TargetSpan / maxSpan : 1.0;

        var lineY = new Dictionary<int, double>();
        var nodeX = new Dictionary<int, Dictionary<string, double>>();
        var lines = new List<TopologyLine>(order.Count);
        var connectors = new List<TopologyConnector>();

        for (var row = 0; row < order.Count; row++)
        {
            var stretch = order[row];
            var y = TopMargin + row * RowGap;
            var stations = stretch.Stations.ToList();
            var track = stretch.Stretches.ToList();

            var distance = new double[stations.Count];
            for (var i = 1; i < stations.Count; i++) distance[i] = distance[i - 1] + track[i - 1].Distance;
            var span = distance[^1] * pxPerUnit;

            var startX = LeftMargin;
            // The station shared with the parent line (a branch's junction) is drawn on the parent only,
            // so hide it here: the start for a leads-out branch, the end for a leads-into branch.
            var hiddenIndex = -1;
            if (parent.TryGetValue(stretch.Id, out var link) && nodeX.TryGetValue(link.Parent.Id, out var parentNodes))
            {
                var parentY = lineY[link.Parent.Id];
                var dy = y - parentY;
                if (link.Side == JunctionSide.Start && parentNodes.TryGetValue(stretch.Starts.Signature, out var junctionX))
                {
                    // Leads out: start sits on the parent line, branch drops away at +45° (dx == dy).
                    startX = junctionX + dy;
                    hiddenIndex = 0;
                    connectors.Add(new TopologyConnector(junctionX, parentY, startX, y));
                }
                else if (link.Side == JunctionSide.End && parentNodes.TryGetValue(stretch.Ends.Signature, out var junctionEndX))
                {
                    // Leads into: end sits on the parent line, branch climbs to it at -45° (dx == dy).
                    var endX = junctionEndX - dy;
                    startX = endX - span;
                    hiddenIndex = stations.Count - 1;
                    connectors.Add(new TopologyConnector(endX, y, junctionEndX, parentY));
                }
            }

            var nodes = new List<TopologyNode>(stations.Count);
            var map = new Dictionary<string, double>(stations.Count);
            for (var i = 0; i < stations.Count; i++)
            {
                var x = startX + distance[i] * pxPerUnit;
                nodes.Add(new TopologyNode(x, y, stations[i].Signature, i == hiddenIndex));
                map[stations[i].Signature] = x;
            }

            lineY[stretch.Id] = y;
            nodeX[stretch.Id] = map;
            lines.Add(new TopologyLine(stretch.Number, stretch.Description, y, nodes));
        }

        return Normalize(lines, connectors, order.Count);
    }

    // Links each stretch to the one it branches from: a stretch diverges when its start is a through
    // station of another line (drawn as leading out), or otherwise merges when its end is (leading into).
    // The lowest-id candidate wins, matching how the model attributes a shared station to its owning stretch.
    private static Dictionary<int, (TimetableStretch Parent, JunctionSide Side)> BuildParentMap(IReadOnlyList<TimetableStretch> all)
    {
        var parent = new Dictionary<int, (TimetableStretch, JunctionSide)>();
        foreach (var stretch in all)
        {
            if (DivergesFrom(stretch, stretch.Starts, all) is { } startParent)
                parent[stretch.Id] = (startParent, JunctionSide.Start);
            else if (DivergesFrom(stretch, stretch.Ends, all) is { } endParent)
                parent[stretch.Id] = (endParent, JunctionSide.End);
        }
        BreakCycles(all, parent);
        return parent;
    }

    private static TimetableStretch? DivergesFrom(TimetableStretch stretch, OperationLocation at, IReadOnlyList<TimetableStretch> all) =>
        all.Where(p => !ReferenceEquals(p, stretch) && p.DistanceToStation(at) is > 0.0)
           .OrderBy(p => p.Id)
           .FirstOrDefault();

    // Mutually diverging stretches could form a cycle; drop the parent edge of any stretch whose ancestor
    // chain loops back to itself so the depth-first walk terminates.
    private static void BreakCycles(IReadOnlyList<TimetableStretch> all, Dictionary<int, (TimetableStretch Parent, JunctionSide Side)> parent)
    {
        foreach (var stretch in all)
        {
            var seen = new HashSet<int> { stretch.Id };
            var current = stretch.Id;
            while (parent.TryGetValue(current, out var link))
            {
                if (!seen.Add(link.Parent.Id)) { parent.Remove(stretch.Id); break; }
                current = link.Parent.Id;
            }
        }
    }

    // Roots first (lowest id), each immediately followed by the subtree of stretches that branch off it,
    // so a branch always lands on a row below the line it leaves.
    private static List<TimetableStretch> OrderDepthFirst(IReadOnlyList<TimetableStretch> all, Dictionary<int, (TimetableStretch Parent, JunctionSide Side)> parent)
    {
        var children = all.Where(s => parent.ContainsKey(s.Id))
            .GroupBy(s => parent[s.Id].Parent.Id)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Id).ToList());

        var order = new List<TimetableStretch>(all.Count);
        void Visit(TimetableStretch stretch)
        {
            order.Add(stretch);
            if (children.TryGetValue(stretch.Id, out var kids))
                foreach (var kid in kids) Visit(kid);
        }
        foreach (var root in all.Where(s => !parent.ContainsKey(s.Id))) Visit(root);
        return order;
    }

    // A stretch that leads into another can be positioned left of the margin; shift everything right so
    // the whole diagram sits within its bounds, then size the canvas to fit.
    private static TopologyDiagram Normalize(List<TopologyLine> lines, List<TopologyConnector> connectors, int rowCount)
    {
        var minX = double.MaxValue;
        var maxX = double.MinValue;
        foreach (var node in lines.SelectMany(l => l.Nodes))
        {
            minX = Math.Min(minX, node.X);
            maxX = Math.Max(maxX, node.X);
        }
        foreach (var c in connectors)
        {
            minX = Math.Min(minX, Math.Min(c.X1, c.X2));
            maxX = Math.Max(maxX, Math.Max(c.X1, c.X2));
        }

        var shift = minX < LeftMargin ? LeftMargin - minX : 0.0;
        var width = maxX + shift + RightMargin;
        var height = TopMargin + (rowCount - 1) * RowGap + BottomMargin;
        if (shift == 0.0) return new TopologyDiagram(width, height, lines, connectors);

        var shiftedLines = lines
            .Select(l => l with { Nodes = l.Nodes.Select(n => n with { X = n.X + shift }).ToList() })
            .ToList();
        var shiftedConnectors = connectors
            .Select(c => c with { X1 = c.X1 + shift, X2 = c.X2 + shift })
            .ToList();
        return new TopologyDiagram(width, height, shiftedLines, shiftedConnectors);
    }
}
