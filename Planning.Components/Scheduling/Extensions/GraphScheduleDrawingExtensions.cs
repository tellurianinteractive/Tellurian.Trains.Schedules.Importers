using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

public static class GraphScheduleDrawingExtensions
{
    public static string OrientationCss(this TimeAxisDirection axisDirection, string classes) =>
        axisDirection == TimeAxisDirection.Horisontal ? $"{classes} horizontal".TrimStart() :
        axisDirection == TimeAxisDirection.Vertical ? $"{classes} vertical".TrimStart() :
        string.Empty;

    public static IEnumerable<(Offset Start, Offset End)> TrainBetweenStationsLine(this GraphSchedule me, int departureStationIndex, int departureTrackIndex, int arrivalStationIndex, int arrivalTrackIndex, Time departure, Time arrival)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var yd = me.Y(departureStationIndex, departureTrackIndex);
            var ya = me.Y(arrivalStationIndex, arrivalTrackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(departure.Value, yd, arrival.Value, ya)))
                yield return (new(me.TimeOffset(piece.FromTime).X, piece.FromCross), new(me.TimeOffset(piece.ToTime).X, piece.ToCross));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var xd = me.X(departureStationIndex, departureTrackIndex);
            var xa = me.X(arrivalStationIndex, arrivalTrackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(departure.Value, xd, arrival.Value, xa)))
                yield return (new(piece.FromCross, me.TimeOffset(piece.FromTime).Y), new(piece.ToCross, me.TimeOffset(piece.ToTime).Y));
        }
        else throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static IEnumerable<(Offset Start, Offset End)> TrainAtStationLine(this GraphSchedule me, int stationIndex, int trackIndex, StationCall stationCall)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var y = me.Y(stationIndex, trackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(stationCall.Arrival.Value, y, stationCall.Departure.Value, y)))
                yield return (new(me.TimeOffset(piece.FromTime).X, piece.FromCross), new(me.TimeOffset(piece.ToTime).X, piece.ToCross));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, trackIndex);
            foreach (var piece in me.ClipToWindow(WrapPieces(stationCall.Arrival.Value, x, stationCall.Departure.Value, x)))
                yield return (new(piece.FromCross, me.TimeOffset(piece.FromTime).Y), new(piece.ToCross, me.TimeOffset(piece.ToTime).Y));
        }
        else throw new NotSupportedException(me.AxisDirection.ToString());
    }

    private static readonly TimeSpan OneDay = TimeSpan.FromHours(24);

    /// <summary>Reduces a time into the current day ([0, 24:00)), so a label for an after-midnight call
    /// (e.g. 24:05) is positioned at its wrapped place on the axis (00:05). A no-op for normal times.</summary>
    private static TimeSpan WrapTime(TimeSpan time) =>
        TimeSpan.FromMinutes(((time.TotalMinutes % OneDay.TotalMinutes) + OneDay.TotalMinutes) % OneDay.TotalMinutes);

    /// <summary>
    /// Splits a train line running from <paramref name="fromTime"/>/<paramref name="fromCross"/> to
    /// <paramref name="toTime"/>/<paramref name="toCross"/> at every 24-hour boundary, so the part at or
    /// past 24:00 wraps back to the start of the axis. Each returned piece has both times reduced into
    /// the current day ([0, 24:00]); the cross-axis position (station/track) is interpolated at the
    /// boundary so the line leaves the right edge and re-enters the left edge at the same position.
    /// When nothing crosses 24:00 (the normal case), a single unchanged piece is returned.
    /// </summary>
    private static IEnumerable<(TimeSpan FromTime, int FromCross, TimeSpan ToTime, int ToCross)> WrapPieces(TimeSpan fromTime, int fromCross, TimeSpan toTime, int toCross)
    {
        var totalMinutes = (toTime - fromTime).TotalMinutes;
        var cursorTime = fromTime;
        var cursorCross = (double)fromCross;
        while (true)
        {
            var dayIndex = (int)Math.Floor((cursorTime.TotalMinutes / OneDay.TotalMinutes) + 1e-6);
            var boundary = OneDay * (dayIndex + 1);
            var shift = OneDay * dayIndex;
            if (totalMinutes <= 0 || toTime <= boundary)
            {
                yield return (cursorTime - shift, (int)Math.Round(cursorCross), toTime - shift, toCross);
                yield break;
            }
            var boundaryCross = fromCross + ((toCross - fromCross) * ((boundary - fromTime).TotalMinutes / totalMinutes));
            yield return (cursorTime - shift, (int)Math.Round(cursorCross), boundary - shift, (int)Math.Round(boundaryCross));
            cursorTime = boundary;
            cursorCross = boundaryCross;
        }
    }

    /// <summary>
    /// Clips each piece to the visible time window [<see cref="GraphSchedule.StartTime"/>,
    /// <see cref="GraphSchedule.EndTime"/>]. A piece reaching outside the window is shortened to the
    /// window edge with its cross-axis position (station/track) interpolated at that edge; a piece
    /// wholly outside is dropped. Without this, an out-of-window time yields <see cref="Offset.Invalid"/>
    /// (0,0) from <see cref="TimeOffset"/>, drawing a stray line to the axis origin. Pieces already
    /// inside the window pass through unchanged. Each piece runs forward in time (FromTime ≤ ToTime).
    /// </summary>
    private static IEnumerable<(TimeSpan FromTime, int FromCross, TimeSpan ToTime, int ToCross)> ClipToWindow(
        this GraphSchedule me, IEnumerable<(TimeSpan FromTime, int FromCross, TimeSpan ToTime, int ToCross)> pieces)
    {
        var start = me.StartTime;
        var end = me.EndTime;
        foreach (var piece in pieces)
        {
            if (piece.ToTime < start || piece.FromTime > end) continue; // wholly outside the window

            var span = (piece.ToTime - piece.FromTime).TotalMinutes;
            int CrossAt(TimeSpan time) => span <= 0
                ? piece.FromCross
                : (int)Math.Round(piece.FromCross + ((piece.ToCross - piece.FromCross) * ((time - piece.FromTime).TotalMinutes / span)));

            var fromTime = piece.FromTime < start ? start : piece.FromTime;
            var toTime = piece.ToTime > end ? end : piece.ToTime;
            yield return (fromTime, CrossAt(fromTime), toTime, CrossAt(toTime));
        }
    }

    /// <summary>
    /// Enumerates every arrival/departure minute label to draw, each already positioned by a corner anchored
    /// at its call point. Intended for a single final pass over all trains so the labels sit above every graph
    /// line. A label is produced only for a stop (<see cref="StationCall.IsStop"/>, never a pass-through) on the matching side
    /// (departure where the train departs, arrival where it arrives) when that minute kind is enabled and the
    /// time is inside the visible window. Uses whose endpoints are not adjacent stations on this schedule are
    /// omitted, matching the lines actually drawn.
    /// </summary>
    public static IEnumerable<MinuteLabel> MinuteLabels(this GraphSchedule me)
    {
        var trackArrays = new StationTrack[me.Stations.Length][];
        for (var i = 0; i < me.Stations.Length; i++) trackArrays[i] = [.. me.Stations[i].Tracks];

        int StationIndexOf(OperationLocation station)
        {
            for (var i = 0; i < me.Stations.Length; i++)
                if (me.Stations[i].Equals(station)) return i;
            return -1;
        }

        foreach (var train in me.Trains)
        {
            foreach (var use in train.StretchUses())
            {
                var fromStationIndex = StationIndexOf(use.From.Track.Station);
                var toStationIndex = StationIndexOf(use.To.Track.Station);
                if (fromStationIndex < 0 || toStationIndex < 0 || Math.Abs(fromStationIndex - toStationIndex) != 1) continue;

                var fromTrackIndex = Array.IndexOf(trackArrays[fromStationIndex], use.From.Track);
                var toTrackIndex = Array.IndexOf(trackArrays[toStationIndex], use.To.Track);
                if (fromTrackIndex < 0 || toTrackIndex < 0) continue;

                // The line descends on screen (cross-axis grows with time) when it runs to a later station.
                var descending = toStationIndex > fromStationIndex;

                if (me.GraphSettings.ShowDepartureMinutes && use.From.IsDeparture && use.From.IsStop && me.IsTimeVisible(use.From.Departure.Value))
                {
                    var point = me.MinuteAt(fromStationIndex, fromTrackIndex, use.From.Departure.Value);
                    yield return me.MinuteLabelAt(point, use.From.Departure.MinutesText(), isDeparture: true, descending);
                }

                if (me.GraphSettings.ShowArrivalMinutes && use.To.IsArrival && use.To.IsStop && me.IsTimeVisible(use.To.Arrival.Value))
                {
                    var point = me.MinuteAt(toStationIndex, toTrackIndex, use.To.Arrival.Value);
                    yield return me.MinuteLabelAt(point, use.To.Arrival.MinutesText(), isDeparture: false, descending);
                }
            }
        }
    }

    // Chooses which corner of the minute text touches the call point, following the requested placement:
    // on a descending (down-slope) line the departure tucks its upper-left corner to its point and the
    // arrival its lower-right; on an ascending (up-slope) line the departure uses its lower-right corner and
    // the arrival its upper-right. The corner is expressed as the screen direction the text extends from the
    // point (right/left, down/up); for a vertical time axis the two axes are transposed. The label is drawn
    // semi-transparent in the final pass, so a corner sitting on a line stays readable without hiding it.
    private static MinuteLabel MinuteLabelAt(this GraphSchedule me, Offset point, string text, bool isDeparture, bool descending)
    {
        bool extendRight, extendDown;
        if (isDeparture)
        {
            // descending: upper-left corner at point → text extends right+down; ascending: lower-right → left+up.
            extendRight = descending;
            extendDown = descending;
        }
        else
        {
            // descending: lower-right corner at point → text extends left+up; ascending: upper-right → left+down.
            extendRight = false;
            extendDown = !descending;
        }

        if (me.AxisDirection == TimeAxisDirection.Vertical) (extendRight, extendDown) = (extendDown, extendRight);

        var anchor = extendRight ? "start" : "end";
        var baseline = extendDown ? "text-before-edge" : "text-after-edge";
        return new MinuteLabel(point, text, anchor, baseline);
    }

    public static string MinutesText(this Time time) => time.Value.Minutes.ToString("D2");

    /// <summary>
    /// Returns the two endpoints of the inter-station train line running from
    /// <paramref name="fromStationIndex"/>/<paramref name="fromTrackIndex"/> to
    /// <paramref name="toStationIndex"/>/<paramref name="toTrackIndex"/> (in screen coordinates), together
    /// with the fraction (0–1) along that line at which to centre the identity label. The label is rendered
    /// along this segment with an SVG <c>&lt;textPath&gt;</c>, so it follows the line's real slope and sits
    /// cleanly to one side of it for both travel directions — no manual rotation or perpendicular offset,
    /// which previously made downward (forward) labels overlap their own line. The start is the departure
    /// point, so the segment always runs forward in time and the text reads upright.
    /// <para>
    /// <paramref name="Offset"/> is not a plain 50%: with many tracks the section midpoint lands inside a
    /// station band, so the label is instead centred on the open corridor between the two bands — halfway
    /// between the stations' facing track edges. Because the segment is straight, the cross-axis position is
    /// linear along it, so the corridor-centre fraction is simply where the line crosses that midpoint.
    /// </para>
    /// Returns <c>null</c> when either endpoint time falls outside the visible window (the label would land
    /// off-canvas).
    /// </summary>
    public static (Offset Start, Offset End, double Offset)? TrainLabelPath(
        this GraphSchedule me,
        int fromStationIndex,
        int fromTrackIndex,
        int toStationIndex,
        int toTrackIndex,
        TimeSpan departure,
        TimeSpan arrival)
    {
        var wrappedDep = WrapTime(departure);
        var wrappedArr = WrapTime(arrival);
        if (!me.IsTimeVisible(wrappedDep) || !me.IsTimeVisible(wrappedArr)) return null;

        // The corridor sits between the two stations' facing track edges: the station higher on the axis
        // (lower index) faces the gap with its last track, the lower station (higher index) with its first
        // track. This is independent of travel direction, so it is derived from station order, not the
        // from/to (departure/arrival) roles.
        var fromFacingTrack = fromStationIndex < toStationIndex ? me.Stations[fromStationIndex].Tracks.Count - 1 : 0;
        var toFacingTrack = toStationIndex < fromStationIndex ? me.Stations[toStationIndex].Tracks.Count - 1 : 0;

        Offset start, end;
        double crossFrom, crossTo, crossEnter, crossExit;
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            crossFrom = me.Y(fromStationIndex, fromTrackIndex);
            crossTo = me.Y(toStationIndex, toTrackIndex);
            crossEnter = me.Y(fromStationIndex, fromFacingTrack);
            crossExit = me.Y(toStationIndex, toFacingTrack);
            start = new Offset(me.TimeOffset(wrappedDep).X, (int)crossFrom);
            end = new Offset(me.TimeOffset(wrappedArr).X, (int)crossTo);
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            crossFrom = me.X(fromStationIndex, fromTrackIndex);
            crossTo = me.X(toStationIndex, toTrackIndex);
            crossEnter = me.X(fromStationIndex, fromFacingTrack);
            crossExit = me.X(toStationIndex, toFacingTrack);
            start = new Offset((int)crossFrom, me.TimeOffset(wrappedDep).Y);
            end = new Offset((int)crossTo, me.TimeOffset(wrappedArr).Y);
        }
        else throw new NotSupportedException(me.AxisDirection.ToString());

        // Fraction along the segment where the cross-axis reaches the corridor midpoint. Linear because the
        // segment is straight; clamped so a degenerate case never sends the label off the path.
        var corridorMid = (crossEnter + crossExit) / 2.0;
        var crossSpan = crossTo - crossFrom;
        var offset = crossSpan == 0 ? 0.5 : (corridorMid - crossFrom) / crossSpan;
        offset = Math.Clamp(offset, 0.0, 1.0);
        return (start, end, offset);
    }

    /// <summary>
    /// Enumerates every train identity label to draw, each with the line segment its text follows. One label
    /// is produced per train use of a stretch between two adjacent stations on this schedule, from the real
    /// departure/arrival tracks via <see cref="TrainLabelPath"/>. Intended to be rendered in a single final
    /// pass (each as an invisible path plus a <c>&lt;textPath&gt;</c>) so all labels sit on top of the graph
    /// lines. Uses whose endpoints are not adjacent stations on this schedule (e.g. a skipped station) are
    /// omitted, matching the lines actually drawn.
    /// </summary>
    public static IEnumerable<(Train Train, Offset Start, Offset End, double Offset)> TrainLabels(this GraphSchedule me)
    {
        var trackArrays = new StationTrack[me.Stations.Length][];
        for (var i = 0; i < me.Stations.Length; i++) trackArrays[i] = [.. me.Stations[i].Tracks];

        int StationIndexOf(OperationLocation station)
        {
            for (var i = 0; i < me.Stations.Length; i++)
                if (me.Stations[i].Equals(station)) return i;
            return -1;
        }

        foreach (var train in me.Trains)
        {
            foreach (var use in train.StretchUses())
            {
                var fromStationIndex = StationIndexOf(use.From.Track.Station);
                var toStationIndex = StationIndexOf(use.To.Track.Station);
                if (fromStationIndex < 0 || toStationIndex < 0 || Math.Abs(fromStationIndex - toStationIndex) != 1) continue;

                var fromTrackIndex = Array.IndexOf(trackArrays[fromStationIndex], use.From.Track);
                var toTrackIndex = Array.IndexOf(trackArrays[toStationIndex], use.To.Track);
                if (fromTrackIndex < 0 || toTrackIndex < 0) continue;

                if (me.TrainLabelPath(fromStationIndex, fromTrackIndex, toStationIndex, toTrackIndex, use.From.Departure.Value, use.To.Arrival.Value) is { } path)
                    yield return (train, path.Start, path.End, path.Offset);
            }
        }
    }

    /// <summary>
    /// The train identity labels from <see cref="TrainLabels"/> with overlapping ones thinned out, so what is
    /// drawn stays legible when many trains cross. Every train keeps its first and last label where possible;
    /// once that is honoured, labels are dropped from the trains involved in the most overlaps, fairly: a train
    /// is not thinned a second time until every other conflicting train has lost a label too (round-robin). A
    /// train is never reduced below one visible label. Removal continues until no two visible labels overlap
    /// (or only sole-survivor labels remain in conflict).
    /// <para>
    /// Overlap is judged from an oriented bounding box per label — centred on the label's anchor along its
    /// segment, as wide as the estimated text and as tall as the font — tested with the separating-axis
    /// theorem. The text width is estimated from glyph count and font size (no browser metrics are available
    /// server-side), so <see cref="TrainLabelBoxPaddingFactor"/> pads it to stay conservative.
    /// </para>
    /// </summary>
    public static IEnumerable<(Train Train, Offset Start, Offset End, double Offset)> VisibleTrainLabels(this GraphSchedule me)
    {
        var raw = me.TrainLabels().ToList();
        if (raw.Count == 0) yield break;

        var perTrainCount = raw.GroupBy(l => l.Train).ToDictionary(g => g.Key, g => g.Count());
        var textLength = new Dictionary<Train, int>();
        var indexInTrain = new Dictionary<Train, int>();

        var nodes = new List<LabelNode>(raw.Count);
        foreach (var l in raw)
        {
            if (!textLength.TryGetValue(l.Train, out var length))
                textLength[l.Train] = length = l.Train.GraphLabel(me.GraphSettings).Length;
            var index = indexInTrain.TryGetValue(l.Train, out var i) ? i : 0;
            indexInTrain[l.Train] = index + 1;
            nodes.Add(new LabelNode(l.Train, l.Start, l.End, l.Offset, index, perTrainCount[l.Train], LabelBox(l.Start, l.End, l.Offset, length)));
        }

        // The conflict graph is static: a label's box never moves when another is removed, so overlaps are
        // computed once and removal just hides nodes, dropping their live edges.
        var neighbours = new List<int>[nodes.Count];
        for (var i = 0; i < nodes.Count; i++) neighbours[i] = [];
        for (var i = 0; i < nodes.Count; i++)
            for (var j = i + 1; j < nodes.Count; j++)
                if (Overlaps(nodes[i].Box, nodes[j].Box)) { neighbours[i].Add(j); neighbours[j].Add(i); }

        var removed = perTrainCount.Keys.ToDictionary(t => t, _ => 0);
        var visibleCount = new Dictionary<Train, int>(perTrainCount);

        int LiveDegree(int i)
        {
            var degree = 0;
            foreach (var j in neighbours[i]) if (nodes[j].Visible) degree++;
            return degree;
        }

        while (true)
        {
            var conflicting = new List<int>();
            for (var i = 0; i < nodes.Count; i++)
                if (nodes[i].Visible && LiveDegree(i) > 0) conflicting.Add(i);
            if (conflicting.Count == 0) break;

            // Round-robin gate: only thin trains that currently have the fewest removals among those in conflict.
            var minRemoved = conflicting.Min(i => removed[nodes[i].Train]);

            bool Removable(int i, bool allowEndpoint) =>
                removed[nodes[i].Train] == minRemoved
                && visibleCount[nodes[i].Train] > 1
                && (allowEndpoint || !nodes[i].IsEndpoint);

            // Prefer dropping interior labels; only sacrifice a first/last when interior ones cannot resolve it.
            var pool = conflicting.Where(i => Removable(i, allowEndpoint: false)).ToList();
            if (pool.Count == 0) pool = conflicting.Where(i => Removable(i, allowEndpoint: true)).ToList();
            // Every remaining conflict is between sole-survivor labels: thinning further would erase a train, so stop.
            if (pool.Count == 0) break;

            // Resolve the most overlaps per removal; tie-break toward the most central label so survivors spread out.
            var pick = pool
                .OrderByDescending(LiveDegree)
                .ThenBy(i => Math.Abs(nodes[i].Index - (nodes[i].Count - 1) / 2.0))
                .First();

            nodes[pick].Visible = false;
            removed[nodes[pick].Train]++;
            visibleCount[nodes[pick].Train]--;
        }

        foreach (var node in nodes)
            if (node.Visible) yield return (node.Train, node.Start, node.End, node.Offset);
    }

    // Padding on the estimated label width/height, since server-side text size is estimated, not measured.
    private const double TrainLabelBoxPaddingFactor = 1.1;
    private const double PointsToPixels = 96.0 / 72.0;
    private const double TrainLabelFontPoints = 6.0;          // matches .trainlabel font-size in the editor CSS
    private const double TrainLabelGlyphWidthEm = 0.62;       // average advance of a bold digit/letter
    private const double TrainLabelLiftPixels = 2.0;          // matches LabelOffsetDy in the editor

    // An oriented bounding box: centre, the unit direction of its long (text) axis, and the half-extents along
    // that axis (Hu) and the perpendicular (Hv).
    private readonly record struct LabelObb(double Cx, double Cy, double Ux, double Uy, double Hu, double Hv);

    private sealed class LabelNode(Train train, Offset start, Offset end, double offset, int index, int count, LabelObb box)
    {
        public Train Train { get; } = train;
        public Offset Start { get; } = start;
        public Offset End { get; } = end;
        public double Offset { get; } = offset;
        public int Index { get; } = index;
        public int Count { get; } = count;
        public LabelObb Box { get; } = box;
        public bool Visible { get; set; } = true;
        public bool IsEndpoint => Index == 0 || Index == Count - 1;
    }

    // Builds the oriented box a label occupies: centred on its anchor point along the segment (lifted off the
    // line like the rendered text), as wide as the estimated text and as tall as the font.
    private static LabelObb LabelBox(Offset start, Offset end, double offset, int textLength)
    {
        var fontPixels = TrainLabelFontPoints * PointsToPixels;
        var halfWidth = Math.Max(1, textLength) * TrainLabelGlyphWidthEm * fontPixels * TrainLabelBoxPaddingFactor / 2.0;
        var halfHeight = fontPixels * TrainLabelBoxPaddingFactor / 2.0;

        double sx = end.X - start.X, sy = end.Y - start.Y;
        var length = Math.Sqrt(sx * sx + sy * sy);
        var (ux, uy) = length < 1e-6 ? (1.0, 0.0) : (sx / length, sy / length);
        double nx = -uy, ny = ux; // unit normal

        var anchorX = start.X + offset * sx;
        var anchorY = start.Y + offset * sy;
        return new LabelObb(anchorX + nx * -TrainLabelLiftPixels, anchorY + ny * -TrainLabelLiftPixels, ux, uy, halfWidth, halfHeight);
    }

    // Separating-axis test for two oriented boxes: they overlap unless some axis (the four box edge normals)
    // separates them. The axes need not be unit vectors — the scale cancels between projection and distance.
    private static bool Overlaps(in LabelObb a, in LabelObb b) =>
        OverlapsOnAxis(a, b, a.Ux, a.Uy)
        && OverlapsOnAxis(a, b, -a.Uy, a.Ux)
        && OverlapsOnAxis(a, b, b.Ux, b.Uy)
        && OverlapsOnAxis(a, b, -b.Uy, b.Ux);

    private static bool OverlapsOnAxis(in LabelObb a, in LabelObb b, double ax, double ay)
    {
        var ra = a.Hu * Math.Abs(a.Ux * ax + a.Uy * ay) + a.Hv * Math.Abs(-a.Uy * ax + a.Ux * ay);
        var rb = b.Hu * Math.Abs(b.Ux * ax + b.Uy * ay) + b.Hv * Math.Abs(-b.Uy * ax + b.Ux * ay);
        var distance = Math.Abs((b.Cx - a.Cx) * ax + (b.Cy - a.Cy) * ay);
        return distance <= ra + rb;
    }

    // The point on the train's own track line at the given time, so a minute label is placed beside the
    // line the train actually runs on rather than above or below the whole station band. After-midnight
    // times are wrapped into the current day to match how the line is positioned on the axis.
    private static Offset MinuteAt(this GraphSchedule me, int stationIndex, int trackIndex, TimeSpan time)
    {
        time = WrapTime(time);
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var x = me.TimeOffset(time).X;
            var y = me.Y(stationIndex, trackIndex);
            return new Offset(x, y);
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, trackIndex);
            var y = me.TimeOffset(time).Y;
            return new Offset(x, y);
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static (Offset Start, Offset End) TrackLine(this GraphSchedule me, int stationIndex, int trackIndex)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var y = me.Y(stationIndex, trackIndex);
            return (new(me.GraphSettings.KilometerAxisSpacing.X, y), new(me.MaxTimeOffset().X, y));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var x = me.X(stationIndex, trackIndex);
            return (new(x, me.GraphSettings.KilometerAxisSpacing.Y), new(x, me.MaxTimeOffset().Y));
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static (Offset Start, Offset End) TimeLine(this GraphSchedule me, TimeSpan time)
    {
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var x = me.TimeOffset(time).X;
            return (new(x, me.GraphSettings.TimeAxisSpacing.Y), new(x, me.MaxTrackOffset().Y));
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var y = me.TimeOffset(time).Y;
            return (new(me.GraphSettings.TimeAxisSpacing.X, y), new(me.MaxTrackOffset().X, y));
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static int Height(this GraphSchedule me) =>
        me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => me.MaxTrackOffset().Y + me.GraphSettings.EndMargin,
            TimeAxisDirection.Vertical => me.MaxTimeOffset().Y + me.GraphSettings.EndMargin,
            _ => 0
        };

    public static int Width(this GraphSchedule me) =>
        me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => me.MaxTimeOffset().X + me.GraphSettings.EndMargin,
            TimeAxisDirection.Vertical => me.MaxTrackOffset().X + me.GraphSettings.EndMargin,
            _ => 0
        };

    public static Offset TimeAxisLabelOffset(this GraphSchedule me, TimeSpan time) =>
       me.AxisDirection switch
       {
           TimeAxisDirection.Horisontal => me.TimeLine(time).Start - new Offset(5, 5),
           TimeAxisDirection.Vertical => me.TimeLine(time).Start - new Offset(me.GraphSettings.TimeAxisSpacing.X - 5, 0),
           _ => throw new NotSupportedException()
       };

    public static Offset StationLabelOffset(this GraphSchedule me, int stationIndex)
    {
        var offset = me.Stations[stationIndex].Tracks.Count / 2 * me.GraphSettings.TrackSpacing;
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(5, me.Y(stationIndex, 0) + offset),
            TimeAxisDirection.Vertical => new(me.X(stationIndex, 0) + offset, 25),
            _ => Offset.Invalid
        };
    }

    public static Offset KmLabelOffset(this GraphSchedule me, int stationIndex)
    {
        var offset = me.Stations[stationIndex].Tracks.Count / 2 * me.GraphSettings.TrackSpacing;
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(me.GraphSettings.KilometerAxisSpacing.X - 15, me.Y(stationIndex, 0) + offset),
            TimeAxisDirection.Vertical => new(me.X(stationIndex, 0) + offset, me.GraphSettings.KilometerAxisSpacing.Y - 15),
            _ => Offset.Invalid
        };
    }

    public static Offset TrackNumberOffset(this GraphSchedule me, int stationIndex, int trackIndex)
    {
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(me.GraphSettings.KilometerAxisSpacing.X - 2, me.Y(stationIndex, trackIndex) + 3),
            TimeAxisDirection.Vertical => new(me.X(stationIndex, trackIndex) + 0, me.GraphSettings.KilometerAxisSpacing.Y - 2),
            _ => Offset.Invalid
        };
    }

    public static Offset TimeOffset(this GraphSchedule me, TimeSpan time)
    {
        if (time < me.StartTime || time > me.EndTime) return Offset.Invalid;
        if (me.AxisDirection == TimeAxisDirection.Horisontal)
        {
            var x = me.GraphSettings.KilometerAxisSpacing.X + (me.GraphSettings.MinuteSpacing * (int)(time - me.StartTime).TotalMinutes);
            return new(x, 0);
        }
        else if (me.AxisDirection == TimeAxisDirection.Vertical)
        {
            var y = me.GraphSettings.KilometerAxisSpacing.Y + (me.GraphSettings.MinuteSpacing * (int)(time - me.StartTime).TotalMinutes);
            return new(0, y);
        }
        throw new NotSupportedException(me.AxisDirection.ToString());
    }

    public static Offset MaxTimeOffset(this GraphSchedule me) =>
        TimeOffset(me, me.EndTime);

    /// <summary>Whether the given call time falls inside the visible time window, so its minute label
    /// has a valid position. After-midnight times are wrapped into the current day first, matching how
    /// they are positioned on the axis.</summary>
    public static bool IsTimeVisible(this GraphSchedule me, TimeSpan time)
    {
        var wrapped = WrapTime(time);
        return wrapped >= me.StartTime && wrapped <= me.EndTime;
    }

    public static int X(this GraphSchedule me, int stationIndex, int trackIndex) =>
        me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => TrackOffset(me, stationIndex, trackIndex).Y,
            TimeAxisDirection.Vertical => TrackOffset(me, stationIndex, trackIndex).X,
            _ => 0
        };

    public static int Y(this GraphSchedule me, int stationIndex, int trackIndex) =>
         me.AxisDirection switch
         {
             TimeAxisDirection.Horisontal => TrackOffset(me, stationIndex, trackIndex).Y,
             TimeAxisDirection.Vertical => TrackOffset(me, stationIndex, trackIndex).X,
             _ => 0
         };

    public static Offset TrackOffset(this GraphSchedule me, int stationIndex, int trackIndex)
    {
        var tracks = me.Stations[stationIndex].Tracks.ToArray();
        var x = me.GraphSettings.TimeAxisSpacing.X;
        var y = me.GraphSettings.TimeAxisSpacing.Y;
        if (stationIndex == 0)
        {
            y += me.GraphSettings.TrackSpacing * trackIndex;
            x += me.GraphSettings.TrackSpacing * trackIndex;
        }
        else
        {
            for (var i = 0; i < stationIndex; i++)
            {
                var stretch = me.TrackStretches[i];
                var fromTrackCount = me.Stations[i].Tracks.Count;
                var Δ1 = Math.Max(me.GraphSettings.MinStationSpacing, ((fromTrackCount - 1) * me.GraphSettings.TrackSpacing) + (int)Math.Round(me.GraphSettings.KilometerSpacing * stretch.Distance));
                x += Δ1;
                y += Δ1;
            }
            var Δ2 = trackIndex * me.GraphSettings.TrackSpacing;
            x += Δ2;
            y += Δ2;
        }
        return me.AxisDirection switch
        {
            TimeAxisDirection.Horisontal => new(0, y),
            TimeAxisDirection.Vertical => new(x, 0),
            _ => Offset.Invalid
        };
    }

    // The distance-axis extent is the position of the last station's last track. This must use the same
    // accumulation as TrackOffset (which positions every drawn track): the hour-display margin
    // (TimeAxisSpacing) plus the per-stretch spacing, plus the last station's track fan-out added
    // *outside* the MinStationSpacing floor. Re-deriving it with a different formula under-reserved the
    // fan-out when KilometerSpacing was low, clipping the last station. Height()/Width() add EndMargin.
    public static Offset MaxTrackOffset(this GraphSchedule me)
    {
        var lastStationIndex = me.Stations.Length - 1;
        var lastTrackIndex = me.Stations[lastStationIndex].Tracks.Count - 1;
        return me.TrackOffset(lastStationIndex, lastTrackIndex);
    }
}
