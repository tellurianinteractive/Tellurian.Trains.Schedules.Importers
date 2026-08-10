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

public sealed record TimetableTableColumn(string Header, bool IsPassenger, Sessions Sessions, string Color = "#000000");

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

    /// <summary>
    /// Whether the operating sessions/days are shown as session numbers (e.g. <c>1-5</c>) or as short
    /// weekday names (e.g. <c>Mo-Fr</c>) in the sessions row. Mirrors the layout's <c>General.UseDays</c>.
    /// </summary>
    public bool UseDays { get; init; }

    /// <summary>The number of operating sessions/days in the layout's period; higher session bits are ignored
    /// when formatting the sessions row. Mirrors the layout's <c>General.MaxSessions</c> setting.</summary>
    public int MaxSessions { get; init; } = 14;

    /// <summary>The first weekday of the operating week, used to pick which weekdays fall within the period
    /// when the sessions row shows days. Mirrors the layout's <c>General.StartDay</c> setting.</summary>
    public DayOfWeek StartDay { get; init; } = DayOfWeek.Monday;

    /// <summary>
    /// True when at least one train in this table does not operate every session/day, so a row listing
    /// each train's operating sessions/days is shown directly below the train-number header row.
    /// </summary>
    public bool ShowSessionsRow { get; init; }

    public static TimetableTable Create(TimetableStretch stretch, IEnumerable<Train> trains, TrainGraphDirection direction, bool useDays = false, int maxSessions = 14, DayOfWeek startDay = DayOfWeek.Monday)
    {
        // SortedTrainSegments splits overtaken trains into multiple segments for the graph.
        // In the table each train occupies exactly one column, so deduplicate by train,
        // keeping the first occurrence to preserve column order.
        var seenTrains = new HashSet<Train>(ReferenceEqualityComparer.Instance);
        var tableTrains = stretch.SortedTrainSegments(trains, direction)
            .Select(s => s.Train)
            .Where(seenTrains.Add)
            .ToList();

        var stretchStations = stretch.Stations.ToHashSet();

        // Only the locations somebody stops at earn a row. Judged from every train running the stretch,
        // not just this direction's, so the stretch's up and down tables list the same locations.
        var stoppingStations = StoppingStations(stretch, trains);
        var stations = (direction == TrainGraphDirection.Upward ? stretch.Stations : stretch.Stations.Reverse())
            .Where(stoppingStations.Contains)
            .ToList();

        var columns = tableTrains
            .Select(t => new TimetableTableColumn(t.Identity, t.IsPassenger, t.Sessions, t.Category?.Color ?? "#000000"))
            .ToList();

        // A sessions row is shown only when at least one train does not operate every session/day within the
        // layout's period; a table where every train runs the full pattern carries no information worth a row.
        var fullDays = Math.Min(maxSessions, 7);
        var showSessionsRow = columns.Any(c =>
        {
            var capped = c.Sessions.CappedForDisplay(useDays, maxSessions);
            return useDays ? capped.DayCount != fullDays : capped.Numbers.Length != maxSessions;
        });

        var lastIndex = stations.Count - 1;
        var rows = stations.Select((station, index) =>
        {
            // The displayed km carries the divergence offset, so a branch continues counting from its
            // junction on the line it leaves.
            var km = stretch.DisplayedDistanceToStation(station).ToString("F0", CultureInfo.InvariantCulture);
            // A train only shows departure-only where it truly originates and arrival-only where it truly
            // terminates (its absolute first/last call). At a boundary station where it continues to or
            // from another stretch — now visible as a connection row — it reads as a normal through-stop
            // with both times.
            var cells = (IReadOnlyList<TimetableTimeCell>)[.. tableTrains.Select(t => BuildCell(t, station))];
            return new TimetableTableRow(km, station.Name, cells)
            {
                IsFirst = index == 0,
                IsLast = index == lastIndex,
            };
        }).ToList();

        var (originRows, terminalRows) = BuildConnectionRows(tableTrains, stretch, stretchStations);

        return new TimetableTable
        {
            Title = direction == TrainGraphDirection.Upward ? stretch.ForwardDescription : stretch.BackwardDescription,
            Columns = columns,
            Rows = [.. originRows, .. rows, .. terminalRows],
            TableNumber = stretch.Id,
            UseDays = useDays,
            MaxSessions = maxSessions,
            StartDay = startDay,
            ShowSessionsRow = showSessionsRow,
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
        IReadOnlyList<Train> trains, TimetableStretch stretch, HashSet<OperationLocation> stretchStations)
    {
        // Which off-stretch stations get a connection row, and an ordering step (call distance from the
        // stretch boundary) taken from the trains that actually start/end there.
        var originSteps = new Dictionary<OperationLocation, int>();
        var terminalSteps = new Dictionary<OperationLocation, int>();
        foreach (var train in trains)
        {
            // In run order: how many calls the train makes before it reaches the stretch and after it
            // leaves it, which is the step used to order the connection rows.
            var calls = train.CallsInRunOrder;
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

    /// <summary>
    /// The locations along <paramref name="stretch"/> that earn a row: those where at least one train
    /// running the stretch actually stops (<see cref="StationCall.IsStop"/>). A location every train runs
    /// past has no times to print, only a column of pass-through marks, so it is left out — and the list
    /// grows by itself as trains that do stop there are added.
    /// </summary>
    /// <remarks>
    /// A signal controlled location is never a stop for any train, so it never appears. Both directions
    /// are judged together, which keeps the stretch's up and down tables listing the same locations.
    /// </remarks>
    private static HashSet<OperationLocation> StoppingStations(TimetableStretch stretch, IEnumerable<Train> trains) =>
        [.. stretch.RunningTrains(trains)
            .SelectMany(t => t.Calls)
            .Where(c => c.IsStop)
            .Select(c => c.OperationLocation)];

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
    // is decided solely by StationCall.IsStop, never by comparing arrival and departure times. Used for both the
    // stretch's own rows and (via ProjectConnectionCell) the borrowed connection rows, so a boundary
    // station reads consistently whichever table it appears in.
    private static TimetableTimeCell BuildCell(Train train, OperationLocation station)
    {
        // In run order, so "first call" and "last call" are where the train starts and ends its run.
        var calls = train.CallsInRunOrder;
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
}
