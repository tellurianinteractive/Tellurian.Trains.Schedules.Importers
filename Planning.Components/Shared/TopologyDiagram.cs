namespace Tellurian.Trains.Schedules.Planning.Components.Shared;

/// <summary>
/// A station drawn as a circle on a timetable-stretch line, positioned proportionally to its distance
/// from the start of the stretch. A <see cref="Hidden"/> node is a junction shared with the line this one
/// hangs from: it still anchors this line's end, but its circle and label are drawn on that line only,
/// not repeated here.
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
/// A 45° line linking a branching stretch to the junction it diverges from (or merges into), drawn in the
/// branching (child) stretch's colour. It runs from the junction to <see cref="LineX"/>, where the branch's
/// own horizontal line begins, and then continues a short way along that line to <see cref="StubX"/>. The
/// stub is there so the renderer can draw the corner as a single joined path: a diagonal and a horizontal
/// stroke merely butted together leave a notch — a small white triangle — on the outside of the 135° corner
/// between them.
/// <para>
/// A branch normally hangs below the line it leaves, so the connector usually falls; it climbs instead when
/// the branch had to be drawn above. The junction is normally a station on the parent line, but when a
/// sibling branch already reaches that very station in the same direction, it is that sibling's corner
/// instead, so the two connectors chain end to end rather than being drawn one on top of the other.
/// </para>
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
    /// <summary>
    /// The size, in SVG user units, that station signatures and line numbers are drawn at. The layout
    /// reserves label space from it, so the renderer must draw at this size for the two to agree.
    /// </summary>
    public const double FontSize = 16.5;

    /// <summary>
    /// How far to the left of a line's first station the renderer must end the line's number. The layout
    /// keeps that much room clear, so a line packed onto the same row cannot run into the number.
    /// </summary>
    public const double NumberOffset = 20.0;

    private const double LeftMargin = 56.0;
    private const double TopMargin = 40.0;
    private const double BottomMargin = 28.0;
    private const double RowGap = 72.0;
    private const double TargetSpan = 640.0;
    private const double EdgePadding = 8.0;

    // Label space is estimated, never measured: the diagram is laid out where no browser text metrics are
    // available. A signature is centred above its station, a line number sits to the left of its first one.
    private const double GlyphWidth = FontSize * 0.62;   // average advance of a semibold letter or digit
    private const double NodeRadius = 5.0;               // the station circle, and half the line's thickness
    private const double LabelGap = 10.0;                // clear space kept between two neighbouring labels
    private const double LabelRise = LabelGap + FontSize; // how far a signature reaches above its line
    private const double LabelDrop = LabelGap;           // how far a line number reaches below its line

    // How far a diagonal connector must stay from anything else it is not joined to, and how far it runs
    // on along the branch's own line so that the corner between them is a join rather than a butt.
    private const double ConnectorClearance = 14.0;
    private const double StubLength = 10.0;

    // Two points count as the same junction — and so may touch — when they are this close.
    private const double JoinTolerance = 0.5;

    // Which end of a branching stretch sits on the line it hangs from: its start (it leads out, so its own
    // line runs on beyond the corner) or its end (it leads into, so its line arrives at the corner).
    private enum JunctionSide { Start, End }

    // Which way a branch grows away from the line it leaves. Below is the natural reading order and is
    // always tried first; above is what saves a diagram whose lower side is full.
    private const int Below = 1;
    private const int Above = -1;

    // A stretch that already has a row, and where its stations ended up, so a branch off it can find the
    // x-coordinate of the junction it leaves from. Growth is the side of its own parent it grew out on;
    // its children try that same side first, so a subtree keeps growing away from the trunk.
    private sealed record PlacedLine(int Row, double Y, int Growth, IReadOnlyDictionary<string, double> NodeX);

    // The point a branch's connector starts from: a junction on the line it leaves, or the corner of a
    // sibling that already reaches that same station on the same side and in the same direction.
    private sealed record Anchor(double X, double Y, int Row, int Line);

    // The space a drawn line claims: its own extent widened by the room its number and signatures need,
    // and tall enough to cover the signatures printed above it. Connectors must stay clear of it.
    private readonly record struct Box(double Left, double Right, double Top, double Bottom);

    /// <summary>
    /// Lays out the layout's timetable stretches. A stretch that diverges from another is linked to it by a
    /// diagonal connector and placed on the topmost free row below it — or, when nothing below can be
    /// reached without cutting across another line or another connector, above it instead. Every stretch
    /// takes the first row on which it overlaps nothing already drawn, so unrelated lines share a row and
    /// the diagram stays as compact as it can.
    /// </summary>
    public static TopologyDiagram Build(Layout layout)
    {
        var all = layout.TimetableStretches.Where(s => s.Stretches.Count > 0).OrderBy(s => s.Id).ToList();
        if (all.Count == 0) return new TopologyDiagram(LeftMargin + EdgePadding, TopMargin + BottomMargin, [], []);

        var parents = BuildParentMap(all);
        var order = OrderDepthFirst(all, parents);

        var maxSpan = all.Max(s => s.Stretches.Sum(t => t.Distance));
        var pxPerUnit = maxSpan > 0 ? TargetSpan / maxSpan : 1.0;

        var placed = new Dictionary<int, PlacedLine>(order.Count);
        var rows = new Dictionary<int, List<(double From, double To)>>();
        var boxes = new List<(int Line, Box Box)>(order.Count);
        var connectors = new List<(int Line, TopologyConnector Connector)>();
        var chains = new Dictionary<(int Parent, string Station, JunctionSide Side, int Growth), Anchor>();
        var lines = new List<TopologyLine>(order.Count);
        var lowest = 0;
        var highest = 0;

        foreach (var stretch in order)
        {
            var stations = stretch.Stations.ToList();
            var track = stretch.Stretches.ToList();

            // The line this one hangs from and the station they share. A stretch whose parent has not been
            // drawn, or whose shared station did not end up on it, is laid out as a line of its own.
            var hasParent = parents.TryGetValue(stretch.Id, out var link);
            var side = hasParent ? link.Side : JunctionSide.Start;
            var junctionStation = hasParent ? (side == JunctionSide.Start ? stretch.Starts : stretch.Ends).Signature : string.Empty;
            var parent = hasParent && placed.TryGetValue(link.Parent.Id, out var found) && found.NodeX.ContainsKey(junctionStation)
                ? found
                : null;

            // The station a branch shares with the line it hangs from is drawn there only; here it is merely
            // the corner where the connector meets this line, so it is hidden and claims no label space.
            var hidden = parent is null ? -1 : side == JunctionSide.Start ? 0 : stations.Count - 1;

            // Stations sit as far apart as their distance makes them, but never closer than their signatures
            // need: on a stretch that mixes a long haul with a couple of neighbouring yards, distance alone
            // would print the two yards' signatures on top of each other.
            var offset = new double[stations.Count];
            for (var i = 1; i < stations.Count; i++)
                offset[i] = offset[i - 1] + Math.Max(
                    track[i - 1].Distance * pxPerUnit,
                    HalfWidth(stations[i - 1].Signature, i - 1 == hidden) + HalfWidth(stations[i].Signature, i == hidden) + LabelGap);
            var span = offset[^1];

            // What the line claims beyond its own stations, and must therefore keep clear of its row
            // neighbours: its number to the left of the first station, and both end signatures.
            var leftReach = Math.Max(NumberReach(stretch.Number), HalfWidth(stations[0].Signature, hidden == 0)) + LabelGap;
            var rightReach = HalfWidth(stations[^1].Signature, hidden == stations.Count - 1) + LabelGap;

            // Where the stretch would sit were it hung from a given anchor on a given row, along with the
            // connector back to that anchor. A branch is pushed as far sideways as it is pushed away, its
            // connector running at 45°; which way it is pushed is the side the junction sits on.
            (double StartX, TopologyConnector? Connector) At(Anchor? anchor, int row)
            {
                if (anchor is null) return (LeftMargin, null);
                var run = Math.Abs(row - anchor.Row) * RowGap;
                var y = RowY(row);
                if (side == JunctionSide.Start)
                {
                    // Leads out: its start is the junction, so the branch leaves the anchor to the right.
                    var startX = anchor.X + run;
                    return (startX, LinkTo(anchor, startX, y, span, stretch.Color));
                }
                // Leads into: its end is the junction, so the branch reaches the anchor from the left.
                var endX = anchor.X - run;
                return (endX - span, LinkTo(anchor, endX, y, -span, stretch.Color));
            }

            Box BoxAt(double startX, int row) =>
                new(startX - leftReach, startX + span + rightReach, RowY(row) - LabelRise, RowY(row) + LabelDrop);

            // A row works when the line itself fits on it and its connector reaches it without touching
            // anything: no line it would cut across on the way, and no connector it would cross.
            bool Fits(Anchor? anchor, double startX, TopologyConnector? connector, int row) =>
                IsRowFree(rows, row, startX - leftReach, startX + span + rightReach)
                && IsClear(boxes, connectors, anchor, connector, BoxAt(startX, row));

            Anchor? chosenAnchor = null;
            var chosenRow = 0;
            var settled = false;

            if (parent is null)
            {
                // A line that hangs from nothing starts at the left margin, on the topmost row it fits on.
                for (var row = 0; ; row++)
                    if (Fits(null, LeftMargin, null, row)) { chosenRow = row; break; }
            }
            else
            {
                Anchor? fallbackAnchor = null;
                var fallbackRow = 0;
                var fallbackFound = false;

                // Away from the trunk first, then back over it. Both are searched outwards from the anchor,
                // so a branch stays as close to the line it leaves as it can.
                foreach (var growth in new[] { parent.Growth, -parent.Growth })
                {
                    var anchor = chains.TryGetValue((link.Parent.Id, junctionStation, side, growth), out var chained)
                        ? chained
                        : new Anchor(parent.NodeX[junctionStation], parent.Y, parent.Row, link.Parent.Id);

                    // One row past everything drawn in this direction is always free of other lines, so the
                    // search is bounded; beyond it there is nothing left to gain.
                    var last = growth == Below ? Math.Max(highest, anchor.Row) + 1 : Math.Min(lowest, anchor.Row) - 1;
                    for (var row = anchor.Row + growth; growth == Below ? row <= last : row >= last; row += growth)
                    {
                        var (startX, connector) = At(anchor, row);
                        if (!IsRowFree(rows, row, startX - leftReach, startX + span + rightReach)) continue;
                        if (!fallbackFound) (fallbackAnchor, fallbackRow, fallbackFound) = (anchor, row, true);
                        if (!IsClear(boxes, connectors, anchor, connector, BoxAt(startX, row))) continue;
                        (chosenAnchor, chosenRow, settled) = (anchor, row, true);
                        break;
                    }
                    if (settled) break;
                }

                // Pushing a branch further out does not move its connector where it passes the rows in
                // between, so when neither side offers a clear path we settle for the nearest row the line
                // itself fits on, and accept that the connector shows a crossing.
                if (!settled)
                {
                    chosenAnchor = fallbackAnchor;
                    chosenRow = fallbackRow;
                }
            }

            var (finalStartX, finalConnector) = At(chosenAnchor, chosenRow);
            var y = RowY(chosenRow);
            var nodes = new List<TopologyNode>(stations.Count);
            var nodeX = new Dictionary<string, double>(stations.Count);
            for (var i = 0; i < stations.Count; i++)
            {
                var x = finalStartX + offset[i];
                nodes.Add(new TopologyNode(x, y, stations[i].Signature, i == hidden));
                nodeX[stations[i].Signature] = x;
            }

            if (!rows.TryGetValue(chosenRow, out var taken)) rows[chosenRow] = taken = [];
            taken.Add((finalStartX - leftReach, finalStartX + span + rightReach));
            boxes.Add((stretch.Id, BoxAt(finalStartX, chosenRow)));
            lowest = Math.Min(lowest, chosenRow);
            highest = Math.Max(highest, chosenRow);

            var grew = chosenAnchor is null ? Below : Math.Sign(chosenRow - chosenAnchor.Row);
            if (finalConnector is not null)
            {
                connectors.Add((stretch.Id, finalConnector));
                // The next branch reaching this same station from this same side and direction hangs off
                // this line's corner rather than off the junction, so their connectors chain end to end
                // instead of the deeper one being drawn straight over the shallower one.
                chains[(link.Parent.Id, junctionStation, side, grew)] = new Anchor(finalConnector.LineX, y, chosenRow, stretch.Id);
            }

            placed[stretch.Id] = new PlacedLine(chosenRow, y, grew == 0 ? Below : grew, nodeX);
            lines.Add(new TopologyLine(stretch.Number, stretch.Description, stretch.Color, y, nodes));
        }

        return Normalize(lines, [.. connectors.Select(c => c.Connector)], lowest, highest);
    }

    private static double RowY(int row) => TopMargin + (row * RowGap);

    // How far a station's signature reaches to either side of its circle, the label being centred on it.
    // A hidden node has no signature drawn, so it takes no more room than the line running through it.
    private static double HalfWidth(string signature, bool hidden) =>
        hidden ? NodeRadius : Math.Max(NodeRadius, signature.Length * GlyphWidth / 2.0);

    // How far a line's number reaches to the left of the line's first station.
    private static double NumberReach(string number) => NumberOffset + (number.Length * GlyphWidth);

    // The connector from an anchor to a branch, continued a short way along the branch's own line. Reach
    // says where that line runs from the corner: to the right when positive, to the left when negative. A
    // line shorter than the stub is followed only to its far end, so the stub never sticks out beyond it.
    private static TopologyConnector LinkTo(Anchor anchor, double lineX, double lineY, double reach, string color)
    {
        var stub = reach == 0.0 ? StubLength : Math.Min(StubLength, Math.Abs(reach));
        return new TopologyConnector(anchor.X, anchor.Y, lineX, lineY, lineX + (reach < 0.0 ? -stub : stub), color);
    }

    // Whether the horizontal space a line would take on a row is free of the lines already on it.
    private static bool IsRowFree(Dictionary<int, List<(double From, double To)>> rows, int row, double from, double to) =>
        !rows.TryGetValue(row, out var taken) || !taken.Any(t => from < t.To && to > t.From);

    // Whether a candidate placement touches nothing it is not joined to: its connector must clear every
    // line it passes and every other connector, and the line itself must clear every connector already
    // drawn. The line the connector hangs from is left out — the two meet at the junction by design, and
    // the connector leaves it at 45° so it departs at once.
    private static bool IsClear(
        List<(int Line, Box Box)> boxes,
        List<(int Line, TopologyConnector Connector)> connectors,
        Anchor? anchor,
        TopologyConnector? connector,
        Box box)
    {
        foreach (var (line, other) in connectors)
            if (Distance(box, other) < ConnectorClearance) return false;
        if (connector is null) return true;

        foreach (var (line, other) in boxes)
        {
            if (line == anchor?.Line) continue;
            if (Distance(connector.JunctionX, connector.JunctionY, connector.LineX, connector.LineY, other) < ConnectorClearance) return false;
        }
        foreach (var (_, other) in connectors)
        {
            // Two connectors that start at the same junction, or one that starts where another ends, are
            // branches of the same station: they are meant to meet, and only their paths beyond must part.
            if (Joined(connector, other)) continue;
            if (Distance(
                    connector.JunctionX, connector.JunctionY, connector.LineX, connector.LineY,
                    other.JunctionX, other.JunctionY, other.LineX, other.LineY) < ConnectorClearance) return false;
        }
        return true;
    }

    // Whether two connectors share an end, and so are joined at a station rather than crossing at one.
    private static bool Joined(TopologyConnector a, TopologyConnector b) =>
        Near(a.JunctionX, a.JunctionY, b.JunctionX, b.JunctionY)
        || Near(a.JunctionX, a.JunctionY, b.LineX, b.LineY)
        || Near(a.LineX, a.LineY, b.JunctionX, b.JunctionY);

    private static bool Near(double ax, double ay, double bx, double by) =>
        Math.Abs(ax - bx) < JoinTolerance && Math.Abs(ay - by) < JoinTolerance;

    // The shortest distance from a box to a connector's diagonal. The stub is left out: it lies along the
    // branch's own line, which the row bookkeeping already keeps clear.
    private static double Distance(Box box, TopologyConnector connector) =>
        Distance(connector.JunctionX, connector.JunctionY, connector.LineX, connector.LineY, box);

    // The shortest distance from a segment to a box, zero when the segment enters it.
    private static double Distance(double ax, double ay, double bx, double by, Box box)
    {
        if (Inside(ax, ay, box) || Inside(bx, by, box)) return 0.0;
        return Math.Min(
            Math.Min(
                Distance(ax, ay, bx, by, box.Left, box.Top, box.Right, box.Top),
                Distance(ax, ay, bx, by, box.Left, box.Bottom, box.Right, box.Bottom)),
            Math.Min(
                Distance(ax, ay, bx, by, box.Left, box.Top, box.Left, box.Bottom),
                Distance(ax, ay, bx, by, box.Right, box.Top, box.Right, box.Bottom)));
    }

    private static bool Inside(double x, double y, Box box) =>
        x >= box.Left && x <= box.Right && y >= box.Top && y <= box.Bottom;

    // The shortest distance between two segments, zero when they cross.
    private static double Distance(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy)
    {
        if (Crosses(ax, ay, bx, by, cx, cy, dx, dy)) return 0.0;
        return Math.Min(
            Math.Min(Distance(ax, ay, cx, cy, dx, dy), Distance(bx, by, cx, cy, dx, dy)),
            Math.Min(Distance(cx, cy, ax, ay, bx, by), Distance(dx, dy, ax, ay, bx, by)));
    }

    private static bool Crosses(double ax, double ay, double bx, double by, double cx, double cy, double dx, double dy)
    {
        var d1 = Side(cx, cy, dx, dy, ax, ay);
        var d2 = Side(cx, cy, dx, dy, bx, by);
        var d3 = Side(ax, ay, bx, by, cx, cy);
        var d4 = Side(ax, ay, bx, by, dx, dy);
        return ((d1 > 0.0 && d2 < 0.0) || (d1 < 0.0 && d2 > 0.0))
            && ((d3 > 0.0 && d4 < 0.0) || (d3 < 0.0 && d4 > 0.0));
    }

    private static double Side(double ax, double ay, double bx, double by, double px, double py) =>
        ((bx - ax) * (py - ay)) - ((by - ay) * (px - ax));

    // The shortest distance from a point to a segment.
    private static double Distance(double px, double py, double ax, double ay, double bx, double by)
    {
        var vx = bx - ax;
        var vy = by - ay;
        var length = (vx * vx) + (vy * vy);
        var t = length <= 0.0 ? 0.0 : Math.Clamp((((px - ax) * vx) + ((py - ay) * vy)) / length, 0.0, 1.0);
        return Math.Sqrt(Squared(px - ax - (t * vx)) + Squared(py - ay - (t * vy)));
    }

    private static double Squared(double value) => value * value;

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
            .ToDictionary(g => g.Key, g => g.OrderBy(s => Outermost(s, parent[s.Id])).ThenBy(s => s.Id).ToList());

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

    // Where a branch leaves its parent, ordered so that the branch furthest along the direction it moves
    // away in comes first. Every branch leaves at the same 45°, so a branch and a line it cannot clear at
    // one row cannot clear it at any row either: pushing both further out moves them equally far sideways.
    // Taking the outermost branch first keeps that from happening — its neighbours nearer the parent's
    // other end then fall past the outside of it, where there is nothing in the way.
    private static double Outermost(TimetableStretch stretch, (TimetableStretch Parent, JunctionSide Side) link)
    {
        var atStart = link.Side == JunctionSide.Start;
        var distance = link.Parent.DistanceToStation(atStart ? stretch.Starts : stretch.Ends) ?? 0.0;
        return atStart ? -distance : distance;
    }

    // A branch may end up left of the margin or above the top row; shift everything so the whole diagram
    // sits within its bounds, then size the canvas to fit. Horizontal bounds are taken from the labels
    // rather than the stations they belong to, so no signature or line number is cut off at an edge.
    private static TopologyDiagram Normalize(List<TopologyLine> lines, List<TopologyConnector> connectors, int lowest, int highest)
    {
        var minX = double.MaxValue;
        var maxX = double.MinValue;
        foreach (var line in lines) minX = Math.Min(minX, line.StartX - NumberReach(line.Number));
        foreach (var node in lines.SelectMany(l => l.Nodes))
        {
            var half = HalfWidth(node.Signature, node.Hidden);
            minX = Math.Min(minX, node.X - half);
            maxX = Math.Max(maxX, node.X + half);
        }
        foreach (var c in connectors)
        {
            minX = Math.Min(minX, Math.Min(c.JunctionX, Math.Min(c.LineX, c.StubX)));
            maxX = Math.Max(maxX, Math.Max(c.JunctionX, Math.Max(c.LineX, c.StubX)));
        }

        var shiftX = minX < EdgePadding ? EdgePadding - minX : 0.0;
        var shiftY = -lowest * RowGap;
        var width = maxX + shiftX + EdgePadding;
        var height = TopMargin + ((highest - lowest) * RowGap) + BottomMargin;
        if (shiftX == 0.0 && shiftY == 0.0) return new TopologyDiagram(width, height, lines, connectors);

        var shiftedLines = lines
            .Select(l => l with { Y = l.Y + shiftY, Nodes = l.Nodes.Select(n => n with { X = n.X + shiftX, Y = n.Y + shiftY }).ToList() })
            .ToList();
        var shiftedConnectors = connectors
            .Select(c => c with
            {
                JunctionX = c.JunctionX + shiftX,
                JunctionY = c.JunctionY + shiftY,
                LineX = c.LineX + shiftX,
                LineY = c.LineY + shiftY,
                StubX = c.StubX + shiftX,
            })
            .ToList();
        return new TopologyDiagram(width, height, shiftedLines, shiftedConnectors);
    }
}
