using System.Globalization;
using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

public sealed record TimetableTimeCell
{
    public static readonly TimetableTimeCell Empty = new();
    public static readonly TimetableTimeCell PassThrough = new() { IsPassThrough = true };
    public static readonly TimetableTimeCell NotStopping = new() { IsNotStopping = true };

    public string? Arrival { get; init; }
    public string? Departure { get; init; }
    public bool IsPassThrough { get; init; }
    public bool IsNotStopping { get; init; }

    public bool HasContent => Arrival is not null || Departure is not null || IsPassThrough;
    // True when the train stops here: both arrival and departure are present
    public bool HasArrivalAndDeparture => Arrival is not null && Departure is not null;
}

public sealed record TimetableTableColumn(string Header, bool IsPassenger, string Color = "#000000");

/// <summary>
/// Distinguishes ordinary stretch rows from <em>connection</em> rows that show where the table's
/// trains start or end on a neighbouring timetable stretch.
/// </summary>
public enum TimetableRowKind
{
    /// <summary>A station on this timetable stretch.</summary>
    Normal,
    /// <summary>A station on another stretch where one or more trains in this table originate
    /// (shown as a departure row above the ordinary rows).</summary>
    OriginConnection,
    /// <summary>A station on another stretch where one or more trains in this table terminate
    /// (shown as an arrival row below the ordinary rows).</summary>
    TerminalConnection,
}

public sealed record TimetableTableRow(string Km, string StationName, IReadOnlyList<TimetableTimeCell> Cells)
{
    // When any train stops at this station, split into an arrival row and a departure row
    public bool IsSplit => Cells.Any(c => c.HasArrivalAndDeparture);
    public bool IsFirst { get; init; }
    public bool IsLast { get; init; }

    /// <summary>Whether this is an ordinary stretch row or a connection row, and which kind.</summary>
    public TimetableRowKind Kind { get; init; } = TimetableRowKind.Normal;

    /// <summary>True for origin/terminal connection rows borrowed from a neighbouring stretch.</summary>
    public bool IsConnection => Kind != TimetableRowKind.Normal;
}

public sealed class TimetableTable
{
    public required string Title { get; init; }
    public required IReadOnlyList<TimetableTableColumn> Columns { get; init; }
    public required IReadOnlyList<TimetableTableRow> Rows { get; init; }

    /// <summary>
    /// Identifies the timetable stretch this table belongs to (its <see cref="TimetableStretch.Id"/>).
    /// Both directions of one stretch share the same value, so pagination can force a hard page
    /// break only when the stretch changes while still fitting both directions on one page.
    /// </summary>
    public required int TableNumber { get; init; }

    public static TimetableTable Create(TimetableStretch stretch, IEnumerable<Train> trains, TrainGraphDirection direction)
    {
        // SortedTrainSegments splits overtaken trains into multiple segments for the graph.
        // In the table each train occupies exactly one column, so deduplicate by train
        // (keeping the first occurrence to preserve column order) and restore the full
        // call range, which TrainSpansKm uses to detect SCL pass-throughs.
        var seenTrains = new HashSet<Train>(ReferenceEqualityComparer.Instance);
        var segments = stretch.SortedTrainSegments(trains, direction)
            .Where(s => seenTrains.Add(s.Train))
            .Select(s => new GraphicalTrainSegment(s.Train, 0, s.Train.Calls.Count - 1))
            .ToList();

        var stations = direction == TrainGraphDirection.Upward
            ? stretch.Stations.ToList()
            : stretch.Stations.Reverse().ToList();

        var stretchStations = stretch.Stations.ToHashSet();

        var columns = segments
            .Select(s => new TimetableTableColumn(s.Train.Identity, s.Train.IsPassenger, s.Train.Category?.Color ?? "#000000"))
            .ToList();

        var lastIndex = stations.Count - 1;
        var rows = stations.Select((station, index) =>
        {
            var stationKm = stretch.DistanceToStation(station) ?? 0.0;
            var km = stationKm.ToString("F1", CultureInfo.InvariantCulture);
            var cells = (IReadOnlyList<TimetableTimeCell>)segments
                .Select(s =>
                {
                    // A train only shows departure-only where it truly originates and arrival-only where it
                    // truly terminates (its absolute first/last call). At a boundary station where it
                    // continues to or from another stretch — now visible as a connection row — it reads as a
                    // normal through-stop with both times.
                    var cell = BuildCell(s.Train, station);
                    if (station is SignalControlledLocation && cell == TimetableTimeCell.Empty)
                        cell = TrainSpansKm(s, stationKm, stretch, stretchStations)
                            ? TimetableTimeCell.NotStopping
                            : TimetableTimeCell.Empty;
                    return cell;
                })
                .ToList();
            return new TimetableTableRow(km, station.Name, cells)
            {
                IsFirst = index == 0,
                IsLast = index == lastIndex,
            };
        }).ToList();

        var (originRows, terminalRows) = BuildConnectionRows(segments, stretch, stretchStations);

        return new TimetableTable
        {
            Title = direction == TrainGraphDirection.Upward ? stretch.ForwardDescription : stretch.BackwardDescription,
            Columns = columns,
            Rows = [.. originRows, .. rows, .. terminalRows],
            TableNumber = stretch.Id,
        };
    }

    /// <summary>
    /// Builds the <em>connection</em> rows for a table. A train that starts or ends off this stretch
    /// borrows the row of its true origin (placed above the ordinary rows) or true terminal (placed
    /// below). Which stations get a row is decided by where trains start and end; each such row is then
    /// filled for <em>every</em> column with the time that train has at that station. Connection rows
    /// carry a single time: terminal rows below show only <em>arrivals</em>, origin rows above show only
    /// <em>departures</em> — a <c>|</c> where the train passes without stopping, blank where its route
    /// never reaches the station. So a train ending at Stilkøbing still shows its Padby arrival in the
    /// Padby row, but never a departure.
    /// <para>
    /// Connection rows are grouped by the neighbouring timetable stretch (table number) the connection
    /// station belongs to, and within a group ordered by call-distance from this stretch. Origin rows go
    /// table-number ascending then furthest-first; terminal rows are the exact reverse (table-number
    /// descending, nearest-first). For stretch 2 that gives origins Ingerslev, Rotebro (table 1), Grenåbro
    /// (table 3) and terminals Grenåbro, Rotebro, Ingerslev.
    /// </para>
    /// </summary>
    private static (List<TimetableTableRow> Origins, List<TimetableTableRow> Terminals) BuildConnectionRows(
        IReadOnlyList<GraphicalTrainSegment> segments, TimetableStretch stretch, HashSet<OperationLocation> stretchStations)
    {
        var trains = segments.Select(s => s.Train).ToList();

        // Which off-stretch stations get a connection row, and an ordering step (call distance from the
        // stretch boundary) taken from the trains that actually start/end there.
        var originSteps = new Dictionary<OperationLocation, int>();
        var terminalSteps = new Dictionary<OperationLocation, int>();
        foreach (var train in trains)
        {
            var calls = train.Calls;
            var firstOn = -1;
            var lastOn = -1;
            for (var i = 0; i < calls.Count; i++)
            {
                if (!stretchStations.Contains(calls[i].OperationLocation)) continue;
                if (firstOn < 0) firstOn = i;
                lastOn = i;
            }
            if (firstOn < 0) continue;
            if (firstOn > 0) AddMinStep(originSteps, calls[0].OperationLocation, firstOn);
            if (lastOn < calls.Count - 1) AddMinStep(terminalSteps, calls[^1].OperationLocation, calls.Count - 1 - lastOn);
        }

        // The table number (stretch id) of the neighbouring stretch a connection station belongs to,
        // cached because the same station can appear for several trains. int.MaxValue sorts a station
        // with no other owning stretch last.
        var ownerTable = new Dictionary<OperationLocation, int>();
        int OwnerOf(OperationLocation station)
        {
            if (!ownerTable.TryGetValue(station, out var id))
            {
                id = station.Layout.TimetableStretches
                    .Where(s => s.Id != stretch.Id && s.Stations.Contains(station))
                    .Select(s => (int?)s.Id).Min() ?? int.MaxValue;
                ownerTable[station] = id;
            }
            return id;
        }

        TimetableTableRow Row(OperationLocation station, TimetableRowKind kind)
        {
            var arrivals = kind == TimetableRowKind.TerminalConnection;
            var cells = (IReadOnlyList<TimetableTimeCell>)[.. trains.Select(t => ProjectConnectionCell(BuildCell(t, station), arrivals))];
            return new TimetableTableRow("", station.Name, cells) { Kind = kind };
        }

        // Origins ascending by table number then furthest-first; terminals the exact reverse.
        var originRows = (List<TimetableTableRow>)[.. originSteps
            .OrderBy(kv => OwnerOf(kv.Key)).ThenByDescending(kv => kv.Value).ThenBy(kv => kv.Key.Name, StringComparer.CurrentCulture)
            .Select(kv => Row(kv.Key, TimetableRowKind.OriginConnection))];
        var terminalRows = (List<TimetableTableRow>)[.. terminalSteps
            .OrderByDescending(kv => OwnerOf(kv.Key)).ThenBy(kv => kv.Value).ThenByDescending(kv => kv.Key.Name, StringComparer.CurrentCulture)
            .Select(kv => Row(kv.Key, TimetableRowKind.TerminalConnection))];

        return (originRows, terminalRows);
    }

    private static void AddMinStep(Dictionary<OperationLocation, int> steps, OperationLocation station, int step)
    {
        if (!steps.TryGetValue(station, out var existing) || step < existing) steps[station] = step;
    }

    // Keeps only the side a connection row shows — arrivals below, departures above — so connection rows
    // are always single-line. A pass-through pipe is kept as is; a cell that lacks the wanted side
    // (for example a departure-only origin call appearing in a terminal row) collapses to Empty.
    private static TimetableTimeCell ProjectConnectionCell(TimetableTimeCell cell, bool arrival)
    {
        if (cell.IsNotStopping) return TimetableTimeCell.NotStopping;
        if (arrival) return cell.Arrival is { } a ? new TimetableTimeCell { Arrival = a } : TimetableTimeCell.Empty;
        return cell.Departure is { } d ? new TimetableTimeCell { Departure = d } : TimetableTimeCell.Empty;
    }

    // The time a train shows at a station, judged by the train's absolute first/last call: departure only
    // where it truly originates, arrival only where it truly terminates, both where it stops in passing, a
    // pass-through pipe where it does not stop, and Empty where it never calls there. Stop versus pass-through
    // is decided solely by StationCall.IsStop, never by comparing arrival and departure times — except that a
    // SignalControlledLocation always reads as a pass-through, because a train never stops there. Used for both the
    // stretch's own rows and (via ProjectConnectionCell) the borrowed connection rows, so a boundary
    // station reads consistently whichever table it appears in.
    private static TimetableTimeCell BuildCell(Train train, OperationLocation station)
    {
        var calls = train.Calls;
        for (var i = 0; i < calls.Count; i++)
        {
            var call = calls[i];
            if (!call.OperationLocation.Equals(station)) continue;
            if (!call.IsStop) return TimetableTimeCell.NotStopping;

            var isFirst = i == 0;
            var isLast = i == calls.Count - 1;
            if (isLast && !isFirst) return new TimetableTimeCell { Arrival = call.Arrival.HHMM() };
            if (isFirst && !isLast) return new TimetableTimeCell { Departure = call.Departure.HHMM() };
            return new TimetableTimeCell { Arrival = call.Arrival.HHMM(), Departure = call.Departure.HHMM() };
        }
        return TimetableTimeCell.Empty;
    }

    // Returns true when the segment has stretch calls on both sides of the given km value,
    // meaning the train passes through that point even without an explicit station call there.
    private static bool TrainSpansKm(GraphicalTrainSegment segment, double km, TimetableStretch stretch, HashSet<OperationLocation> stretchStations)
    {
        var calls = segment.Train.Calls;
        var before = false;
        var after = false;
        for (var i = segment.FromCallIndex; i <= segment.ToCallIndex; i++)
        {
            if (!stretchStations.Contains(calls[i].OperationLocation)) continue;
            var d = stretch.DistanceToStation(calls[i].OperationLocation);
            if (d is null) continue;
            if (d < km) before = true;
            else if (d > km) after = true;
            if (before && after) return true;
        }
        return false;
    }
}
