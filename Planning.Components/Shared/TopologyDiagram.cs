namespace Tellurian.Trains.Schedules.Planning.Components.Shared;

/// <summary>
/// A station drawn as a circle on a timetable-stretch line, positioned proportionally to its distance
/// from the start of the stretch. A <see cref="Hidden"/> node is a junction shared with a parent line: it
/// still anchors this line's end, but its circle and label are drawn on the parent only, not repeated here.
/// </summary>
public sealed record TopologyNode(double X, double Y, string Signature, bool Hidden = false);

/// <summary>
/// A timetable stretch drawn as a horizontal line with its stations as evenly-distance-spaced nodes.
/// </summary>
public sealed record TopologyLine(string Number, string Description, string Color, double Y, IReadOnlyList<TopologyNode> Nodes)
{
    /// <summary>The x-coordinate of the first station on the line.</summary>
    public double StartX => Nodes.Count > 0 ? Nodes[0].X : 0.0;

    /// <summary>The x-coordinate of the last station on the line.</summary>
    public double EndX => Nodes.Count > 0 ? Nodes[^1].X : 0.0;
}

/// <summary>
/// A 45°/-45° line linking a branching stretch to the junction it diverges from (or merges into) on its
/// parent line, drawn in the branching (child) stretch's colour. It runs from the junction on the parent
/// down to <see cref="LineX"/>, where the branch's own horizontal line begins, and then continues a short
/// way along that line to <see cref="StubX"/>. The stub is there so the renderer can draw the corner as a
/// single joined path: a diagonal and a horizontal stroke merely butted together leave a notch — a small
/// white triangle — on the outside of the 135° corner between them.
/// </summary>
public sealed record TopologyConnector(double JunctionX, double JunctionY, double LineX, double LineY, double StubX, string Color);

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

    // What a line occupies beyond its own stations and must therefore keep clear of its row neighbours:
    // its number label to the left of the first station, and the last station's signature to the right.
    private const double LabelGutter = 56.0;
    private const double SignatureGutter = 24.0;

    // How far a diagonal connector must stay from a line it passes on its way down, and how far it runs
    // on along the branch's own line so that the corner between them is a join rather than a butt.
    private const double ConnectorClearance = 16.0;
    private const double StubLength = 10.0;

    // Which end of a branching stretch sits on its parent line: its start (it leads out, drawn +45°)
    // or its end (it leads into, drawn -45°).
    private enum JunctionSide { Start, End }

    // A stretch that already has a row, and where its stations ended up, so a branch off it can find the
    // x-coordinate of the junction it leaves from.
    private sealed record PlacedLine(int Row, double Y, IReadOnlyDictionary<string, double> NodeX);

    /// <summary>
    /// Lays out the layout's timetable stretches. A stretch that diverges from another is placed below the
    /// line it leaves and linked by a diagonal connector; every stretch takes the topmost row on which it
    /// overlaps nothing already drawn, so unrelated lines share a row and the diagram stays as low as it can.
    /// </summary>
    public static TopologyDiagram Build(Layout layout)
    {
        var all = layout.TimetableStretches.Where(s => s.Stretches.Count > 0).OrderBy(s => s.Id).ToList();
        if (all.Count == 0) return new TopologyDiagram(LeftMargin + RightMargin, TopMargin + BottomMargin, [], []);

        var parents = BuildParentMap(all);
        var order = OrderDepthFirst(all, parents);

        var maxSpan = all.Max(s => s.Stretches.Sum(t => t.Distance));
        var pxPerUnit = maxSpan > 0 ? TargetSpan / maxSpan : 1.0;

        var placed = new Dictionary<int, PlacedLine>(order.Count);
        var rows = new List<List<(double From, double To)>>();
        var crossings = new List<(int Row, double X)>();
        var lines = new List<TopologyLine>(order.Count);
        var connectors = new List<TopologyConnector>();

        foreach (var stretch in order)
        {
            var stations = stretch.Stations.ToList();
            var track = stretch.Stretches.ToList();

            var distance = new double[stations.Count];
            for (var i = 1; i < stations.Count; i++) distance[i] = distance[i - 1] + track[i - 1].Distance;
            var span = distance[^1] * pxPerUnit;

            var hasParent = parents.TryGetValue(stretch.Id, out var link);
            var junction = hasParent && placed.TryGetValue(link.Parent.Id, out var parentLine) ? parentLine : null;

            // Where the stretch would sit were it put on a given row, along with the station it shares with
            // its parent — drawn on the parent only, so hidden here — and the connector up to that parent.
            // A branch is pushed as far to the right as it is pushed down, its connector being drawn at 45°.
            (double StartX, int Hidden, TopologyConnector? Connector) At(int row)
            {
                if (junction is null) return (LeftMargin, -1, null);
                var run = (row - junction.Row) * RowGap;
                if (link.Side == JunctionSide.Start && junction.NodeX.TryGetValue(stretch.Starts.Signature, out var divergeX))
                {
                    // Leads out: its start is the junction, so the branch drops away from the parent at +45°.
                    var startX = divergeX + run;
                    return (startX, 0, LinkTo(divergeX, junction.Y, startX, RowY(row), span, stretch.Color));
                }
                if (link.Side == JunctionSide.End && junction.NodeX.TryGetValue(stretch.Ends.Signature, out var mergeX))
                {
                    // Leads into: its end is the junction, so the branch climbs to the parent at -45°.
                    var endX = mergeX - run;
                    return (endX - span, stations.Count - 1, LinkTo(mergeX, junction.Y, endX, RowY(row), -span, stretch.Color));
                }
                return (LeftMargin, -1, null);
            }

            var chosen = -1;
            var firstFree = -1;
            for (var row = junction is null ? 0 : junction.Row + 1; row <= rows.Count; row++)
            {
                var candidate = At(row);
                if (!IsRowFree(rows, crossings, row, candidate.StartX - LabelGutter, candidate.StartX + span + SignatureGutter)) continue;
                if (firstFree < 0) firstFree = row;
                if (IsPathFree(rows, candidate.Connector, junction?.Row ?? row, row)) { chosen = row; break; }
            }
            // Pushing a branch further down does not move the connector where it crosses the rows above it,
            // so when every reachable row is crossed we settle for the topmost row the line itself fits on.
            if (chosen < 0) chosen = firstFree < 0 ? rows.Count : firstFree;

            var (startX, hidden, connector) = At(chosen);
            var y = RowY(chosen);
            var nodes = new List<TopologyNode>(stations.Count);
            var nodeX = new Dictionary<string, double>(stations.Count);
            for (var i = 0; i < stations.Count; i++)
            {
                var x = startX + distance[i] * pxPerUnit;
                nodes.Add(new TopologyNode(x, y, stations[i].Signature, i == hidden));
                nodeX[stations[i].Signature] = x;
            }

            while (rows.Count <= chosen) rows.Add([]);
            rows[chosen].Add((startX - LabelGutter, startX + span + SignatureGutter));
            if (connector is not null)
            {
                connectors.Add(connector);
                for (var crossed = junction!.Row + 1; crossed < chosen; crossed++)
                    crossings.Add((crossed, CrossingX(connector, crossed)));
            }

            placed[stretch.Id] = new PlacedLine(chosen, y, nodeX);
            lines.Add(new TopologyLine(stretch.Number, stretch.Description, stretch.Color, y, nodes));
        }

        return Normalize(lines, connectors, rows.Count);
    }

    private static double RowY(int row) => TopMargin + row * RowGap;

    // The connector down to a branch, continued a short way along the branch's own line. Reach says where
    // that line runs from the corner: to the right when positive, to the left when negative. A line shorter
    // than the stub is followed only to its far end, so the stub never sticks out beyond it.
    private static TopologyConnector LinkTo(double junctionX, double junctionY, double lineX, double lineY, double reach, string color)
    {
        var stub = reach == 0.0 ? StubLength : Math.Min(StubLength, Math.Abs(reach));
        return new TopologyConnector(junctionX, junctionY, lineX, lineY, lineX + (reach < 0.0 ? -stub : stub), color);
    }

    // Where a connector passes through one of the rows between its junction and its branch.
    private static double CrossingX(TopologyConnector connector, int row) =>
        connector.JunctionX + ((RowY(row) - connector.JunctionY) / (connector.LineY - connector.JunctionY) * (connector.LineX - connector.JunctionX));

    // Whether the horizontal space a line would take on a row is free: no other line there, and no
    // connector coming down through it.
    private static bool IsRowFree(List<List<(double From, double To)>> rows, List<(int Row, double X)> crossings, int row, double from, double to) =>
        (row >= rows.Count || !rows[row].Any(taken => from < taken.To && to > taken.From))
        && !crossings.Any(c => c.Row == row && c.X > from - ConnectorClearance && c.X < to + ConnectorClearance);

    // Whether a connector clears the lines on the rows it passes through on its way down to its branch.
    private static bool IsPathFree(List<List<(double From, double To)>> rows, TopologyConnector? connector, int junctionRow, int row)
    {
        if (connector is null) return true;
        for (var crossed = junctionRow + 1; crossed < row && crossed < rows.Count; crossed++)
        {
            var x = CrossingX(connector, crossed);
            if (rows[crossed].Any(taken => x > taken.From - ConnectorClearance && x < taken.To + ConnectorClearance)) return false;
        }
        return true;
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
    // so a branch is always laid out after the line it leaves.
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
            minX = Math.Min(minX, Math.Min(c.JunctionX, Math.Min(c.LineX, c.StubX)));
            maxX = Math.Max(maxX, Math.Max(c.JunctionX, Math.Max(c.LineX, c.StubX)));
        }

        var shift = minX < LeftMargin ? LeftMargin - minX : 0.0;
        var width = maxX + shift + RightMargin;
        var height = TopMargin + ((rowCount - 1) * RowGap) + BottomMargin;
        if (shift == 0.0) return new TopologyDiagram(width, height, lines, connectors);

        var shiftedLines = lines
            .Select(l => l with { Nodes = l.Nodes.Select(n => n with { X = n.X + shift }).ToList() })
            .ToList();
        var shiftedConnectors = connectors
            .Select(c => c with { JunctionX = c.JunctionX + shift, LineX = c.LineX + shift, StubX = c.StubX + shift })
            .ToList();
        return new TopologyDiagram(width, height, shiftedLines, shiftedConnectors);
    }
}
