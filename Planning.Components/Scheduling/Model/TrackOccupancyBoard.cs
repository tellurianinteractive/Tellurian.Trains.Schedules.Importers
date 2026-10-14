using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

/// <summary>What a block on the track occupancy board stands for.</summary>
public enum OccupancyKind
{
    /// <summary>A train standing at the track, from its arrival to its departure.</summary>
    Call,

    /// <summary>Vehicles standing at the track between their trains (see <see cref="TrackHold"/>).</summary>
    Hold,
}

/// <summary>
/// One block on a track of the board: a train's call or the stay of some vehicles. It is drawn once for
/// each of its <see cref="Segments"/>, which is two when it runs over the end of the day and wraps to the
/// start of the axis.
/// </summary>
public sealed class OccupancyBlock
{
    internal OccupancyBlock(OccupancyKind kind, StationTrack track, string label, string title, string? conflict,
        IReadOnlyList<(TimeSpan Start, TimeSpan End)> segments, StationCall? call = null, TrackHold? hold = null, string? color = null)
    {
        Kind = kind;
        Track = track;
        Label = label;
        Title = title;
        Conflict = conflict;
        Segments = segments;
        Call = call;
        Hold = hold;
        Color = color;
    }

    public OccupancyKind Kind { get; }
    public StationTrack Track { get; }

    /// <summary>The call this block is, when it is a <see cref="OccupancyKind.Call"/>. Only a call can be moved.</summary>
    public StationCall? Call { get; }

    /// <summary>The stay this block is, when it is a <see cref="OccupancyKind.Hold"/>.</summary>
    public TrackHold? Hold { get; }

    /// <summary>The train label or the names of the vehicles.</summary>
    public string Label { get; }

    /// <summary>What to show when pointing at the block: who, where and when.</summary>
    public string Title { get; }

    /// <summary>The category colour of a train; <see langword="null"/> for a stay.</summary>
    public string? Color { get; }

    /// <summary>The text of the conflicts this block is part of, or <see langword="null"/> when it is in none.</summary>
    public string? Conflict { get; }

    /// <summary>The parts of the day the block covers, on the axis, in the order they are travelled.</summary>
    public IReadOnlyList<(TimeSpan Start, TimeSpan End)> Segments { get; }

    /// <summary>The line of the track the block is stacked on, counted from the top. Blocks that are on the
    /// track at the same time are on different lines, so none hides another.</summary>
    public int Lane { get; internal set; }
}

/// <summary>One track of a station on the board, with everything standing on it.</summary>
public sealed class TrackOccupancyRow
{
    internal TrackOccupancyRow(StationTrack track, IReadOnlyList<OccupancyBlock> blocks)
    {
        Track = track;
        Blocks = blocks;
        Lanes = Math.Max(1, blocks.Select(b => b.Lane + 1).DefaultIfEmpty(1).Max());
    }

    public StationTrack Track { get; }
    public IReadOnlyList<OccupancyBlock> Blocks { get; }

    /// <summary>How many lines the blocks are stacked in: at least one, so an empty track still shows.</summary>
    public int Lanes { get; }
}

/// <summary>A station on the board and its tracks.</summary>
public sealed class TrackOccupancyStation(OperationLocation location, IReadOnlyList<TrackOccupancyRow> rows)
{
    public OperationLocation Location { get; } = location;
    public IReadOnlyList<TrackOccupancyRow> Rows { get; } = rows;
}

/// <summary>
/// The track occupancy board: for every track of every station, the trains standing at it and the vehicles
/// left standing between their trains, laid out on one time axis.
/// </summary>
/// <remarks>
/// Blocks that are on a track at the same time are stacked on separate lines, whatever sessions they run
/// on, so the board is always readable; whether two of them really are in each other's way is what the
/// validation says, and a block it names is marked as a conflict. A time past midnight is stored on the day
/// after (see <c>Time</c>), so a block over the end of the day is split and its latter part wraps to the
/// start of the axis, as in the graphical timetable.
/// </remarks>
public sealed class TrackOccupancyBoard
{
    private static readonly TimeSpan OneDay = TimeSpan.FromHours(24);

    // A pass-through call has no time on the track at all. It gets this long for the sake of drawing and
    // of stacking, so it is a mark one can see and point at.
    private static readonly TimeSpan MinimumVisible = TimeSpan.FromMinutes(1);

    private TrackOccupancyBoard(TimeSpan start, TimeSpan end, IReadOnlyList<TrackOccupancyStation> stations)
    {
        Start = start;
        End = end;
        Stations = stations;
    }

    /// <summary>Where the time axis starts.</summary>
    public TimeSpan Start { get; }

    /// <summary>Where the time axis ends.</summary>
    public TimeSpan End { get; }

    public IReadOnlyList<TrackOccupancyStation> Stations { get; }

    /// <summary>
    /// Lays out the board for the plan.
    /// </summary>
    /// <param name="plan">The plan whose tracks to show.</param>
    /// <param name="settings">How trains are labelled, and the hours of the day.</param>
    /// <param name="errors">The conflicts found by the validation; the blocks they name are marked.</param>
    /// <param name="include">Which locations to put on the board; all of those with tracks when omitted.</param>
    public static TrackOccupancyBoard Create(Plan plan, GraphSettings settings, IReadOnlyList<ValidationError> errors,
        Func<OperationLocation, bool>? include = null)
    {
        var timetable = plan.Timetable;
        var holds = plan.MarkedTrackHolds;
        var trackErrors = errors
            .Where(e => e.ErrorType is ValidationErrorType.StationTrackConflict or ValidationErrorType.StationTrackHeldByVehicles)
            .ToList();

        var crossesMidnight = GraphSchedule.CrossesMidnight(timetable.Trains.SelectMany(t => t.Calls));
        var (start, end) = GraphSchedule.TimeWindow(settings, crossesMidnight, GraphHalf.Whole);

        var stations = new List<TrackOccupancyStation>();
        foreach (var location in timetable.Stations().Where(l => l.Tracks.Count > 0 && (include?.Invoke(l) ?? true)))
        {
            var rows = new List<TrackOccupancyRow>();
            foreach (var track in location.TracksInDisplayOrder)
            {
                var blocks = new List<OccupancyBlock>();
                foreach (var call in track.Calls.Where(c => c.Train is not null && c.OccupiesTrack(plan.Schedules)))
                    AddIfVisible(blocks, CallBlock(call, settings, trackErrors), start, end);
                foreach (var hold in holds.Where(h => h.Track.Equals(track)))
                    AddIfVisible(blocks, HoldBlock(hold, trackErrors), start, end);
                rows.Add(new TrackOccupancyRow(track, Stack(blocks)));
            }
            stations.Add(new TrackOccupancyStation(location, rows));
        }
        return new TrackOccupancyBoard(start, end, stations);
    }

    private static OccupancyBlock CallBlock(StationCall call, GraphSettings settings, List<ValidationError> trackErrors)
    {
        var train = call.Train;
        var label = train.GraphLabel(settings);
        var messages = trackErrors
            // A stay that is in the way marks the call that waits for the track (the first train); the train
            // that left the vehicles is shown by the stay instead.
            .Where(e => e.Hold is null || e.Trains[0].Equals(train))
            .Where(e => e.Involves(train) && e.Involves(call.Track) && e.OverlapsTimeRange(call.Arrival, call.Departure))
            .Select(e => e.Message.Text)
            .Distinct()
            .ToList();
        var title = $"{label}  {call.Track.Station.Name} {call.Track.Number}  {call.Arrival.HHMM()}–{call.Departure.HHMM()}";
        return new OccupancyBlock(OccupancyKind.Call, call.Track, label, title, Join(messages),
            Segments(call.Arrival, call.Departure), call: call, color: train.Category?.Color ?? "#000000");
    }

    private static OccupancyBlock HoldBlock(TrackHold hold, List<ValidationError> trackErrors)
    {
        var messages = trackErrors.Where(e => e.Hold is { } h && h.Equals(hold)).Select(e => e.Message.Text).Distinct().ToList();
        var title = $"{hold.Vehicles}  {hold.Track.Station.Name} {hold.Track.Number}  {hold.From.HHMM()}–{hold.To.HHMM()}";
        return new OccupancyBlock(OccupancyKind.Hold, hold.Track, hold.Vehicles, title, Join(messages),
            Segments(hold.From, hold.To), hold: hold);
    }

    private static string? Join(List<string> messages) => messages.Count > 0 ? string.Join("\n", messages) : null;

    private static void AddIfVisible(List<OccupancyBlock> blocks, OccupancyBlock block, TimeSpan start, TimeSpan end)
    {
        var visible = Clip(block.Segments, start, end);
        if (visible.Count == 0) return;
        blocks.Add(new OccupancyBlock(block.Kind, block.Track, block.Label, block.Title, block.Conflict, visible,
            block.Call, block.Hold, block.Color));
    }

    /// <summary>
    /// The parts of the axis a span covers. A span lies within the day its start is stored on; where it
    /// runs past 24:00 the rest wraps to the start of the axis.
    /// </summary>
    internal static IReadOnlyList<(TimeSpan Start, TimeSpan End)> Segments(Time from, Time to)
    {
        var days = Math.Floor(from.Value.TotalDays);
        var start = from.Value - TimeSpan.FromDays(days);
        var end = to.Value - TimeSpan.FromDays(days);
        if (end < start) end = start;
        if (end - start < MinimumVisible) end = start + MinimumVisible;
        if (end <= OneDay) return [(start, end)];
        // Never wrap further than where the span began: it is shorter than a day.
        var wrapped = TimeSpan.FromTicks(Math.Min(end.Ticks - OneDay.Ticks, start.Ticks));
        return wrapped > TimeSpan.Zero ? [(start, OneDay), (TimeSpan.Zero, wrapped)] : [(start, OneDay)];
    }

    private static List<(TimeSpan Start, TimeSpan End)> Clip(IReadOnlyList<(TimeSpan Start, TimeSpan End)> segments, TimeSpan start, TimeSpan end) =>
    [
        .. segments
            .Where(s => s.End > start && s.Start < end)
            .Select(s => (Start: s.Start < start ? start : s.Start, End: s.End > end ? end : s.End)),
    ];

    /// <summary>
    /// Puts each block on the first line where it overlaps none of those already there, earliest first.
    /// Blocks that merely meet — one arriving as the other leaves — share a line.
    /// </summary>
    internal static IReadOnlyList<OccupancyBlock> Stack(List<OccupancyBlock> blocks)
    {
        List<List<OccupancyBlock>> lanes = [];
        foreach (var block in blocks.OrderBy(b => b.Segments.Min(s => s.Start)).ThenBy(b => b.Kind))
        {
            var lane = lanes.FindIndex(l => l.All(other => !Overlap(block, other)));
            if (lane < 0)
            {
                lanes.Add([]);
                lane = lanes.Count - 1;
            }
            lanes[lane].Add(block);
            block.Lane = lane;
        }
        return blocks;
    }

    private static bool Overlap(OccupancyBlock one, OccupancyBlock other) =>
        one.Segments.Any(a => other.Segments.Any(b => a.Start < b.End && b.Start < a.End));
}
