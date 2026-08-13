namespace Tellurian.Trains.Schedules.Planning.Components.Shared;

/// <summary>
/// Which side of its circle a location's signature is printed on. Over the circle is the house style and
/// is used wherever it is clear; the other three are what keep a signature off the track, and the side
/// ones are the only answer where track runs both up and down from the same location.
/// </summary>
public enum TopologyLabelSide
{
    /// <summary>Over the circle — the house style, used wherever no track runs through it.</summary>
    Above,

    /// <summary>Under the circle.</summary>
    Below,

    /// <summary>To the right of the circle, reading away from it.</summary>
    Right,

    /// <summary>To the left of the circle, reading towards it.</summary>
    Left,
}

/// <summary>
/// An operation location drawn as a circle, at the one place it appears in the diagram. A location is a
/// node whatever it is to the timetable stretches running through it — a terminus, a through station, a
/// junction where three lines meet — so it is drawn once and once only, however many stretches claim it.
/// <see cref="LabelSide"/> says where its signature goes; <see cref="LabelX"/>, <see cref="LabelY"/> and
/// <see cref="LabelAnchor"/> are that decision turned into what the renderer needs, so the two cannot
/// drift apart.
/// </summary>
public sealed record TopologyNode(
    int LocationId, double X, double Y, string Signature, TopologyLabelSide LabelSide = TopologyLabelSide.Above)
{
    /// <summary>Where the signature's text is anchored horizontally.</summary>
    public double LabelX => LabelSide switch
    {
        TopologyLabelSide.Right => X + TopologyDiagram.LabelOffset,
        TopologyLabelSide.Left => X - TopologyDiagram.LabelOffset,
        _ => X,
    };

    /// <summary>The baseline the signature sits on.</summary>
    public double LabelY => LabelSide switch
    {
        TopologyLabelSide.Above => Y - TopologyDiagram.LabelRise,
        TopologyLabelSide.Below => Y + TopologyDiagram.LabelDrop,
        // Beside the circle the signature reads best level with it, which puts the baseline a third of
        // the way below its centre rather than on it.
        _ => Y + (TopologyDiagram.FontSize / 3.0),
    };

    /// <summary>The SVG text anchor that puts the signature on <see cref="LabelSide"/>.</summary>
    public string LabelAnchor => LabelSide switch
    {
        TopologyLabelSide.Right => "start",
        TopologyLabelSide.Left => "end",
        _ => "middle",
    };
}

/// <summary>
/// The track between two neighbouring operation locations — one track stretch of the layout — drawn as a
/// straight line between where the two are, at whatever angle that turns out to be.
/// </summary>
/// <param name="Tracks">
/// How many parallel tracks the stretch has. A double track is drawn as two lines either side of the
/// centre, which is why track several timetable stretches run over may not be: see <paramref name="Colors"/>.
/// </param>
/// <param name="Colors">
/// The colours of the timetable stretches running over this track stretch, in stretch order. The first is
/// the colour the track itself is drawn in; the renderer lays the rest over it as dashes rather than as
/// further parallel lines, which would read as extra tracks. Empty where no timetable stretch runs over
/// the stretch at all, which the renderer draws in a neutral grey so the gap can be seen.
/// </param>
public sealed record TopologySection(
    double FromX, double FromY, double ToX, double ToY, int Tracks, IReadOnlyList<string> Colors)
{
    /// <summary>Whether no timetable stretch runs over this piece of track.</summary>
    public bool IsUncovered => Colors.Count == 0;
}

/// <summary>
/// A laid-out topology of a layout's track: one node per operation location and one section per track
/// stretch. Coordinates are in SVG user units and are the coordinates a moved location is saved in, so
/// the drawing is framed by the view box (<see cref="MinX"/>, <see cref="MinY"/>, <see cref="Width"/>,
/// <see cref="Height"/>) rather than by shifting everything into a fixed corner — a saved position has to
/// mean the same thing next time.
/// </summary>
public sealed record TopologyDiagram(
    double MinX,
    double MinY,
    double Width,
    double Height,
    IReadOnlyList<TopologyNode> Nodes,
    IReadOnlyList<TopologySection> Sections)
{
    /// <summary>
    /// The size, in SVG user units, that station signatures are drawn at. The layout reserves label space
    /// from it, so the renderer must draw at this size for the two to agree.
    /// </summary>
    public const double FontSize = 16.5;

    /// <summary>The radius of the circle a location is drawn as, and half the thickness of a single track.</summary>
    public const double NodeRadius = 5.0;

    /// <summary>How far above a location's centre its signature's baseline sits when printed over it.</summary>
    public const double LabelRise = 10.0;

    /// <summary>How far below a location's centre its signature's baseline sits when printed under it.</summary>
    public const double LabelDrop = 22.0;

    /// <summary>How far to the side of a location's centre its signature begins when printed beside it.</summary>
    public const double LabelOffset = 12.0;

    /// <summary>
    /// The vertical step a location snaps to when dragged. It is the spacing the automatic layout puts
    /// between its rows, so a location dropped anywhere lands level with the rows around it and the track
    /// between them stays straight.
    /// </summary>
    public const double SnapY = 72.0;

    /// <summary>The horizontal step a location snaps to when dragged — fine enough to place it exactly.</summary>
    public const double SnapX = 8.0;

    /// <summary>The vertical coordinate of the topmost row of the automatic layout.</summary>
    public const double TopRow = 40.0;

    private const double LeftMargin = 56.0;
    private const double TargetSpan = 640.0;
    private const double EdgePadding = 12.0;

    // Empty space kept around the drawing while it can be arranged, so there is somewhere to move a
    // location to. Without it the drawing is framed tight against its own outermost locations, and the
    // frame is held still for the length of a drag — so a location dragged off the top row would be
    // carried outside the frame and clipped away mid-drag. It still landed where it was dropped, but it
    // could not be seen going there, which is indistinguishable from it refusing to go. A whole row up
    // and down, so a location moved one row is in clear space the whole way.
    private const double ArrangingRoomY = SnapY;
    private const double ArrangingRoomX = SnapX * 6;

    // Label space is estimated, never measured: the diagram is laid out where no browser text metrics are
    // available. A signature is set from its baseline, so its extent is an ascender up and a descender
    // down from there.
    private const double GlyphWidth = FontSize * 0.62;   // average advance of a semibold letter or digit
    private const double LabelGap = 10.0;                // clear space kept between two neighbouring labels
    private const double Ascent = FontSize * 0.8;        // how far a signature reaches above its baseline
    private const double Descent = FontSize * 0.2;       // and below it
    private const double TrackHalfWidth = 5.0;           // half the width a piece of track is drawn at

    // A route as it is walked: each location along it, with the distance from the one before it.
    private sealed record Step(OperationLocation Location, double Distance);

    /// <summary>
    /// Lays out the layout's track. Every operation location the track reaches is placed exactly once: the
    /// longest timetable stretch runs left to right along the top row, and everything else hangs from what
    /// is already placed — a branch reaching on past a location it leaves, a second route between two of
    /// them spread across the gap — each on the nearest row where it runs into nothing already drawn. A
    /// location the planner has moved (see <see cref="Model.Layouts.TopologyPosition"/>) goes where they
    /// put it, which is the answer for the triangles, balloon loops and other cycles that no rule reading
    /// the track alone gets right. A timetable stretch shows only as the colour its track is drawn in: it
    /// is not numbered, the diagram being a picture of the railway rather than of the timetable.
    /// </summary>
    /// <param name="layout">The layout to draw.</param>
    /// <param name="withRoomToArrange">
    /// Whether to keep an empty row above and below the drawing. Pass <c>true</c> wherever the planner
    /// can drag a location, so there is visible space to move one into — above the topmost line most of
    /// all, where a diagram framed against its own content leaves nowhere to go. Off for the printed
    /// booklet, which is looked at rather than arranged and should not waste the paper.
    /// </param>
    public static TopologyDiagram Build(Layout layout, bool withRoomToArrange = false)
    {
        layout = layout.ValueOrException(nameof(layout));
        var track = GatherTrack(layout);
        if (track.Count == 0) return new TopologyDiagram(0.0, 0.0, LeftMargin, TopRow, [], []);

        var placed = AutoArrange(layout, track);
        foreach (var saved in layout.TopologyPositions.Where(p => placed.ContainsKey(p.LocationId)))
            placed[saved.LocationId] = (saved.X, saved.Y);

        var sections = track
            .Select(t => new TopologySection(
                placed[t.From.Id].X, placed[t.From.Id].Y, placed[t.To.Id].X, placed[t.To.Id].Y,
                Math.Max(1, t.Tracks), t.Colors))
            .ToList();

        return Frame(BuildNodes(track, placed), sections, withRoomToArrange);
    }

    /// <summary>
    /// The nearest point to (<paramref name="x"/>, <paramref name="y"/>) that a dragged location may be
    /// dropped at: the horizontal grid, and the same row spacing the automatic layout uses, so a location
    /// moved by hand still lines up with the ones it was not moved past.
    /// </summary>
    /// <param name="x">Where it was dropped, horizontally, in diagram units.</param>
    /// <param name="y">Where it was dropped, vertically, in diagram units.</param>
    public static (double X, double Y) Snap(double x, double y) =>
        (Math.Round(x / SnapX) * SnapX, TopRow + (Math.Round((y - TopRow) / SnapY) * SnapY));

    // One piece of the layout's track, between two neighbouring locations, with the timetable stretches
    // running over it. The layout may hold the same pair more than once — the data is not always tidy —
    // so track is gathered by the pair it joins, whichever way round each copy was recorded.
    private sealed class Track
    {
        public required OperationLocation From { get; init; }
        public required OperationLocation To { get; init; }
        public double Distance { get; set; }
        public int Tracks { get; set; }
        public List<TimetableStretch> Used { get; } = [];
        public IReadOnlyList<string> Colors => [.. Used.OrderBy(u => u.Id).Select(u => u.Color)];
    }

    private static (int, int) Between(OperationLocation a, OperationLocation b) =>
        a.Id <= b.Id ? (a.Id, b.Id) : (b.Id, a.Id);

    // The layout's track, one entry per pair of locations joined by it, in the order the layout records
    // them. Copies of one piece should agree; where they do not, the longer distance and the wider
    // formation are kept, so track is never shown as shorter or narrower than the data claims.
    private static List<Track> GatherTrack(Layout layout)
    {
        var track = new Dictionary<(int, int), Track>();
        var order = new List<Track>();

        Track Piece(TrackStretch stretch)
        {
            var between = Between(stretch.Start, stretch.End);
            if (!track.TryGetValue(between, out var piece))
            {
                track[between] = piece = new Track { From = stretch.Start, To = stretch.End };
                order.Add(piece);
            }
            piece.Distance = Math.Max(piece.Distance, stretch.Distance);
            piece.Tracks = Math.Max(piece.Tracks, stretch.TracksCount);
            return piece;
        }

        foreach (var stretch in layout.TrackStretches.Where(s => !s.Start.Equals(s.End))) Piece(stretch);

        // A timetable stretch may name track the layout itself no longer lists; it is still track a train
        // runs over, so it is drawn rather than silently dropped.
        foreach (var timetableStretch in layout.TimetableStretches.OrderBy(s => s.Id))
            foreach (var stretch in timetableStretch.Stretches.Where(s => !s.Start.Equals(s.End)))
            {
                var piece = Piece(stretch);
                if (!piece.Used.Any(u => u.Id == timetableStretch.Id)) piece.Used.Add(timetableStretch);
            }

        return order;
    }

    // The routes the automatic layout follows: each timetable stretch as it runs, longest first so the
    // line most of the layout hangs from is the one drawn along the top row, and then every remaining
    // piece of track as a route of its own, so track no timetable stretch covers is placed too. A stretch
    // that does not continue from where the one before it ended starts a fresh route rather than being
    // walked as though the two were joined.
    private static List<List<Step>> RoutesOf(Layout layout, List<Track> track)
    {
        var byPair = track.ToDictionary(t => Between(t.From, t.To));
        var routes = new List<List<Step>>();
        var covered = new HashSet<(int, int)>();

        foreach (var stretch in layout.TimetableStretches
            .Where(s => s.Stretches.Count > 0)
            .OrderByDescending(s => s.Stretches.Sum(t => t.Distance))
            .ThenBy(s => s.Id))
        {
            List<Step>? route = null;
            OperationLocation? at = null;
            foreach (var stretchTrack in stretch.Stretches.Where(t => !t.Start.Equals(t.End)))
            {
                var between = Between(stretchTrack.Start, stretchTrack.End);
                covered.Add(between);
                if (at is null || !at.Equals(stretchTrack.Start))
                {
                    if (route is { Count: > 1 }) routes.Add(route);
                    route = [new Step(stretchTrack.Start, 0.0)];
                }
                route!.Add(new Step(stretchTrack.End, byPair[between].Distance));
                at = stretchTrack.End;
            }
            if (route is { Count: > 1 }) routes.Add(route);
        }

        foreach (var piece in track.Where(t => !covered.Contains(Between(t.From, t.To))))
            routes.Add([new Step(piece.From, 0.0), new Step(piece.To, piece.Distance)]);

        return routes;
    }

    // Where every location goes when nothing has been moved by hand. Routes are taken one at a time, each
    // time the first that reaches somewhere already placed, so the diagram grows outwards from the line
    // drawn first instead of starting again in mid-air. What a route reaches that is already placed
    // anchors it; each run of locations it reaches that is not goes on a row of its own.
    private static Dictionary<int, (double X, double Y)> AutoArrange(Layout layout, List<Track> track)
    {
        var pending = RoutesOf(layout, track);
        var span = 0.0;
        foreach (var route in pending) span = Math.Max(span, route.Sum(step => step.Distance));
        var scale = span > 0.0 ? TargetSpan / span : 1.0;

        var placed = new Dictionary<int, (double X, int Row)>();
        var rows = new Dictionary<int, List<(double From, double To)>>();

        while (pending.Count > 0)
        {
            var index = pending.FindIndex(r => r.Any(s => placed.ContainsKey(s.Location.Id)));
            var route = pending[index < 0 ? 0 : index];
            pending.Remove(route);

            for (var at = 0; at < route.Count;)
            {
                if (placed.ContainsKey(route[at].Location.Id)) { at++; continue; }
                var beyond = at;
                while (beyond < route.Count && !placed.ContainsKey(route[beyond].Location.Id)) beyond++;
                PlaceRun(route, at - 1, beyond, scale, placed, rows);
                at = beyond;
            }
        }

        return placed.ToDictionary(p => p.Key, p => (p.Value.X, TopRow + (p.Value.Row * SnapY)));
    }

    // Places route[before + 1 .. beyond - 1], none of which is placed yet. It is anchored by route[before]
    // and route[beyond] where those exist: by both, and it is a second route between two locations already
    // drawn, spread across the gap; by one, and it reaches on past that location; by neither, and it is a
    // piece of the layout nothing drawn so far touches, which starts afresh at the left margin.
    private static void PlaceRun(
        IReadOnlyList<Step> route,
        int before,
        int beyond,
        double scale,
        Dictionary<int, (double X, int Row)> placed,
        Dictionary<int, List<(double From, double To)>> rows)
    {
        var first = before + 1;
        var last = beyond - 1;
        (double X, int Row)? start = before >= 0 && placed.TryGetValue(route[before].Location.Id, out var a) ? a : null;
        (double X, int Row)? end = beyond < route.Count && placed.TryGetValue(route[beyond].Location.Id, out var b) ? b : null;

        // How far each location lies along the route, counted from the anchor before the run.
        var from = Math.Max(before, 0);
        var along = new double[route.Count];
        for (var i = from + 1; i < route.Count; i++) along[i] = along[i - 1] + route[i].Distance;

        var x = new double[last - first + 1];
        if (start is { } left && end is { } right && along[beyond] > 0.0)
        {
            for (var i = first; i <= last; i++)
                x[i - first] = left.X + (along[i] / along[beyond] * (right.X - left.X));
        }
        else if (start is { } anchor)
        {
            // On past where the anchor's own line came from, so a branch carries on rather than doubling
            // back over the line it leaves.
            var direction = before > 0 && placed.TryGetValue(route[before - 1].Location.Id, out var back)
                && Math.Abs(anchor.X - back.X) > 0.5 ? Math.Sign(anchor.X - back.X) : 1;
            for (var i = first; i <= last; i++) x[i - first] = anchor.X + (direction * along[i] * scale);
        }
        else if (end is { } into)
        {
            // The run leads into the anchor, so it lies behind it along the way the route goes on from there.
            var direction = beyond + 1 < route.Count && placed.TryGetValue(route[beyond + 1].Location.Id, out var onward)
                && Math.Abs(onward.X - into.X) > 0.5 ? Math.Sign(onward.X - into.X) : 1;
            for (var i = first; i <= last; i++) x[i - first] = into.X - (direction * (along[beyond] - along[i]) * scale);
        }
        else
        {
            for (var i = first; i <= last; i++) x[i - first] = LeftMargin + (along[i] * scale);
        }

        Spread(route, first, x);

        var reachLeft = x[0] - HalfWidth(route[first].Location.Signature) - LabelGap;
        var reachRight = x[^1] + HalfWidth(route[last].Location.Signature) + LabelGap;
        if (reachLeft > reachRight) (reachLeft, reachRight) = (reachRight, reachLeft);
        var row = FreeRow(rows, reachLeft, reachRight, start?.Row ?? end?.Row ?? 0);

        if (!rows.TryGetValue(row, out var taken)) rows[row] = taken = [];
        taken.Add((reachLeft, reachRight));
        for (var i = first; i <= last; i++) placed[route[i].Location.Id] = (x[i - first], row);
    }

    // Pushes apart locations that distance alone would print on top of each other: on a stretch mixing a
    // long haul with a couple of neighbouring yards, true scale prints the two yards' signatures over one
    // another. Everything past the pair moves along with it rather than the next gap being squeezed to pay
    // for it, so the hauls that were already far enough apart keep their true proportions. The order along
    // the run is kept, so spreading never turns a run back on itself.
    private static void Spread(IReadOnlyList<Step> route, int first, double[] x)
    {
        if (x.Length < 2) return;
        var rightwards = x[^1] >= x[0];
        var shift = 0.0;
        for (var i = 1; i < x.Length; i++)
        {
            var least = HalfWidth(route[first + i - 1].Location.Signature)
                + HalfWidth(route[first + i].Location.Signature) + LabelGap;
            var wanted = x[i] + shift;
            var nearest = rightwards ? x[i - 1] + least : x[i - 1] - least;
            if (rightwards ? wanted < nearest : wanted > nearest)
            {
                shift += nearest - wanted;
                wanted = nearest;
            }
            x[i] = wanted;
        }
    }

    // The row a run goes on: the one it hangs from where that is clear — a branch reaching on beyond the
    // line it leaves belongs in line with it — and otherwise the first row below that is. Only downwards,
    // which is the natural reading order and, more to the point, the predictable one: the planner drags
    // what the placement got wrong, and a placement that quietly moves a branch to the other side of the
    // line it leaves is harder to correct than one that always puts it on the same side.
    private static int FreeRow(Dictionary<int, List<(double From, double To)>> rows, double left, double right, int near)
    {
        var row = near;
        while (!IsFree(rows, row, left, right)) row++;
        return row;
    }

    private static bool IsFree(Dictionary<int, List<(double From, double To)>> rows, int row, double left, double right) =>
        !rows.TryGetValue(row, out var taken) || !taken.Any(t => left < t.To && right > t.From);

    // A signature is printed over its circle where nothing is in the way, and moved to whichever of the
    // four sides is clearest where something is. Over the circle is exactly where track climbing away
    // from that location runs, under it is where track falling away runs, and a location with track doing
    // both — the middle of a run drawn up and down the page — has no clear side at all unless the
    // signature can go beside the circle. That is the case the old rule, which only ever chose between
    // over and under, had no answer for.
    private static List<TopologyNode> BuildNodes(List<Track> track, Dictionary<int, (double X, double Y)> placed)
    {
        var lines = track
            .Select(piece => (From: placed[piece.From.Id], To: placed[piece.To.Id]))
            .ToList();

        var nodes = new List<TopologyNode>();
        var taken = new List<Box>();
        foreach (var location in track
            .SelectMany(piece => new[] { piece.From, piece.To })
            .DistinctBy(location => location.Id)
            .OrderBy(location => location.Id))
        {
            var at = placed[location.Id];
            var node = new TopologyNode(location.Id, at.X, at.Y, location.Signature, ClearestSide(at, location.Signature, lines, taken));
            nodes.Add(node);
            taken.Add(LabelBox(node));
        }
        return nodes;
    }

    // Tried in this order, so the house style wins wherever it is as clear as anything else.
    private static readonly TopologyLabelSide[] LabelSides =
        [TopologyLabelSide.Above, TopologyLabelSide.Below, TopologyLabelSide.Right, TopologyLabelSide.Left];

    // The side whose signature the least runs through, weighing every piece of track — not merely the
    // ones reaching this location — and the signatures already placed, so moving one off the track does
    // not put it over a neighbour's instead.
    //
    // Two levels, because they are not the same fault. Track actually crossing the letters is the fault
    // being fixed here; track merely passing close is only untidy, since track is measured along its
    // centreline and is drawn half its width to either side of it, so a line clearing the letters by less
    // than that still has its edge over them. A crossing is therefore always worth trading for a near
    // miss, but never the other way round. Sides otherwise equal are settled by the order above, so the
    // house style is never given up for a little more room. Where a location has track on all four sides
    // nothing is clear and the least bad side is the answer: no arrangement leaves that signature alone.
    private static TopologyLabelSide ClearestSide(
        (double X, double Y) at,
        string signature,
        IReadOnlyList<((double X, double Y) From, (double X, double Y) To)> lines,
        IReadOnlyList<Box> taken)
    {
        var best = TopologyLabelSide.Above;
        var fewestStruck = int.MaxValue;
        var fewestClose = int.MaxValue;

        foreach (var side in LabelSides)
        {
            var box = LabelBox(at.X, at.Y, signature, side);
            var struck = 0;
            var close = 0;
            foreach (var (from, to) in lines)
            {
                var distance = Distance(from.X, from.Y, to.X, to.Y, box);
                if (distance <= 0.0) struck++;
                else if (distance < TrackHalfWidth) close++;
            }
            foreach (var other in taken)
                if (Overlaps(box, other)) struck++;

            if (struck < fewestStruck || (struck == fewestStruck && close < fewestClose))
                (best, fewestStruck, fewestClose) = (side, struck, close);
            if (fewestStruck == 0 && fewestClose == 0) break;
        }
        return best;
    }

    // The space a signature takes, from the baseline the renderer puts it on. Width is estimated from the
    // average glyph, as everywhere else here; height reaches an ascender above the baseline and a
    // descender below it.
    private static Box LabelBox(TopologyNode node) => LabelBox(node.X, node.Y, node.Signature, node.LabelSide);

    private static Box LabelBox(double x, double y, string signature, TopologyLabelSide side)
    {
        var node = new TopologyNode(0, x, y, signature, side);
        var width = Math.Max(2 * NodeRadius, signature.Length * GlyphWidth);
        var (left, right) = side switch
        {
            TopologyLabelSide.Right => (node.LabelX, node.LabelX + width),
            TopologyLabelSide.Left => (node.LabelX - width, node.LabelX),
            _ => (node.LabelX - (width / 2), node.LabelX + (width / 2)),
        };
        return new Box(left, right, node.LabelY - Ascent, node.LabelY + Descent);
    }

    private static bool Overlaps(Box a, Box b) =>
        a.Left < b.Right && a.Right > b.Left && a.Top < b.Bottom && a.Bottom > b.Top;

    // The space a signature or a stretch number takes on the page.
    private readonly record struct Box(double Left, double Right, double Top, double Bottom);

    // The shortest distance from a segment to a box, zero where the segment enters it.
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

    // The shortest distance between two segments, zero where they cross.
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
        var ox = px - ax - (t * vx);
        var oy = py - ay - (t * vy);
        return Math.Sqrt((ox * ox) + (oy * oy));
    }

    // How far a location's signature reaches to either side of its circle, the label being centred on it.
    private static double HalfWidth(string signature) =>
        Math.Max(NodeRadius, signature.Length * GlyphWidth / 2.0);

    // The window the drawing is seen through, taken from the signatures rather than from the circles they
    // belong to, so no signature is cut off at an edge. Nothing is moved to make it fit: the coordinates
    // are the ones a moved location is saved in, and must mean the same next time.
    private static TopologyDiagram Frame(
        List<TopologyNode> nodes, List<TopologySection> sections, bool withRoomToArrange)
    {
        var minX = double.MaxValue;
        var maxX = double.MinValue;
        var minY = double.MaxValue;
        var maxY = double.MinValue;

        void Include(double left, double right, double top, double bottom)
        {
            minX = Math.Min(minX, left);
            maxX = Math.Max(maxX, right);
            minY = Math.Min(minY, top);
            maxY = Math.Max(maxY, bottom);
        }

        foreach (var node in nodes)
        {
            // The circle and its signature, wherever that ended up: a signature moved beside its circle
            // reaches further sideways than one over it, and the frame has to follow or it is cut off.
            Include(node.X - NodeRadius, node.X + NodeRadius, node.Y - NodeRadius, node.Y + NodeRadius);
            var box = LabelBox(node);
            Include(box.Left, box.Right, box.Top, box.Bottom);
        }
        foreach (var section in sections)
            Include(
                Math.Min(section.FromX, section.ToX), Math.Max(section.FromX, section.ToX),
                Math.Min(section.FromY, section.ToY), Math.Max(section.FromY, section.ToY));

        var roomX = EdgePadding + (withRoomToArrange ? ArrangingRoomX : 0.0);
        var roomY = EdgePadding + (withRoomToArrange ? ArrangingRoomY : 0.0);
        return new TopologyDiagram(
            minX - roomX, minY - roomY,
            maxX - minX + (2 * roomX), maxY - minY + (2 * roomY),
            nodes, sections);
    }
}
