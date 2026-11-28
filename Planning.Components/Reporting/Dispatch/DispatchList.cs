using Tellurian.Trains.Schedules.Model.Dispatching;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

/// <summary>
/// A neighbouring station a dispatcher can ring, with the number they reach it on.
/// </summary>
/// <remarks>
/// The number is not optional: the heading exists so a call can actually be made, and a name with no
/// number beside it is something to read past rather than something to use. A neighbour whose number
/// nobody recorded is therefore left out of the list entirely rather than carried with a blank.
/// </remarks>
/// <param name="Name">The neighbour's name.</param>
/// <param name="PhoneNumber">The number it is reached on.</param>
public sealed record DispatchNeighbour(string Name, int PhoneNumber);

/// <summary>
/// One station's complete dispatch list: every clearance its dispatcher gives over the whole operating
/// day, in time order, with the neighbours they clear trains to and from.
/// </summary>
/// <remarks>
/// A dispatcher who also works locations from afar — a junction or an unmanned station controlled from
/// here — clears the trains there too, so their rows are in the same list, in time order among the
/// station's own: a second sheet would have to be merged by eye while trains are moving. A remote call
/// directly beside the train's own call at the station is left out, since the rows there already cover it.
/// </remarks>
/// <param name="Station">The station the list is for.</param>
/// <param name="Rows">Every clearance, ascending by time.</param>
/// <param name="Neighbours">The dispatch stretches' far ends that can actually be rung.</param>
public sealed record DispatchList(
    OperationLocation Station,
    IReadOnlyList<DispatchRow> Rows,
    IReadOnlyList<DispatchNeighbour> Neighbours)
{
    /// <summary>
    /// The locations this station's dispatcher works from afar that have rows in the list.
    /// </summary>
    public IReadOnlyList<OperationLocation> RemoteLocations { get; init; } = [];

    /// <summary>
    /// Where the list's direction leads — the neighbours beyond it, "Avesta, Fors" — or <c>null</c> for a
    /// list that covers the whole station. See <see cref="CreateAll"/>.
    /// </summary>
    public string? Direction { get; init; }

    /// <summary>
    /// The list's heading: the station, followed by each location it dispatches remotely that has rows
    /// in the list — "Alpha + Bravo".
    /// </summary>
    public string Title =>
        string.Join(" + ", RemoteLocations.Select(location => location.Name).Prepend(Station.Name));

    /// <summary>Builds the dispatch list for one station from the trains of a timetable.</summary>
    /// <remarks>
    /// Rows come from the trains rather than from the station's own track index: a train owns its calls,
    /// and the per-track index is a derived one that a plan can be loaded without. Reading the trains
    /// therefore cannot produce a list missing the calls of a train whose index was never rebuilt.
    /// </remarks>
    /// <param name="station">The station to build the list for.</param>
    /// <param name="trains">The timetable's trains.</param>
    /// <param name="settings">How sessions are rendered inside notes.</param>
    /// <param name="plan">The plan whose vehicle schedules say what is done with the vehicles at this
    /// station. Omit it and the rows carry no vehicle instructions.</param>
    public static DispatchList Create(
        OperationLocation station, IEnumerable<Train> trains, SessionsSettings settings, Plan? plan = null)
    {
        station = station.ValueOrException(nameof(station));
        trains = trains.ValueOrException(nameof(trains));

        var layout = station.Layout;
        var dispatched = layout is not null && station is Station dispatcher
            ? layout.LocationsDispatchedBy(dispatcher)
            : [station];
        // The clearances, their order and which remote calls are left out are the model's rules (Clearances).
        var rows = station.Clearances(trains, settings, plan)
            .Select(clearance => DispatchRow.FromClearance(clearance, settings, plan))
            .ToList();
        // Only the ones that can be rung: see DispatchNeighbour for why a number is not optional here.
        var neighbours = TimetableOf(plan, trains) is { } timetable && station is Station endpoint
            ? timetable.DispatchNeighboursOf(endpoint)
                .Select(neighbour => (neighbour.Name, timetable.PlanningOf(neighbour).PhoneNumber))
                .Where(neighbour => neighbour.PhoneNumber.HasValue)
                .Select(neighbour => new DispatchNeighbour(neighbour.Name, neighbour.PhoneNumber!.Value))
                .ToList()
            : [];

        // A remote location without rows has nothing on the sheet, so it is left out of the heading too.
        var remoteLocations = dispatched
            .Skip(1)
            .Where(location => rows.Any(row => row.IsRemote && row.Call.OperationLocation.Equals(location)))
            .ToList();

        return new DispatchList(station, rows, neighbours) { RemoteLocations = remoteLocations };
    }

    /// <summary>
    /// The lists a station hands out: one for the whole station, or — where the station is set to divide
    /// its dispatch by direction and has more than one — one per direction.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A row goes to the list of the direction it is cleared to or from. An arrival belongs to the
    /// direction the train came from and a departure to the direction it goes to, so a train running
    /// through is cleared in on one list and out on the other. It is one row only where it comes and goes
    /// the same way. A call at a location the station dispatches from afar belongs to the direction that
    /// location lies in.
    /// </para>
    /// <para>
    /// A shunting task has no direction and is on the first list, and so is a clearance to or from
    /// somewhere the network does not connect to the station.
    /// </para>
    /// </remarks>
    /// <param name="station">The station to build the lists for.</param>
    /// <param name="trains">The timetable's trains.</param>
    /// <param name="settings">How sessions are rendered inside notes.</param>
    /// <param name="plan">See <see cref="Create"/>.</param>
    public static IReadOnlyList<DispatchList> CreateAll(
        OperationLocation station, IEnumerable<Train> trains, SessionsSettings settings, Plan? plan = null)
    {
        var whole = Create(station, trains, settings, plan);
        if (station is not Station dispatcher || TimetableOf(plan, trains) is not { } timetable || !timetable.PlanningOf(dispatcher).SplitDispatchByDirection || station.Layout is not { } layout)
            return [whole];
        var directions = layout.DispatchDirectionsAt(station);
        if (directions.Count < 2) return [whole];

        int SideOf(OperationLocation? location)
        {
            if (location is null) return 0;
            var side = directions.ToList().FindIndex(direction => direction.Reach.Contains(location));
            return Math.Max(side, 0);
        }

        var rows = directions.Select(_ => new List<DispatchRow>()).ToList();
        foreach (var row in whole.Rows)
        {
            if (row.IsRemote)
            {
                rows[SideOf(row.Call.OperationLocation)].Add(row);
                continue;
            }

            var calls = row.Call.Train.CallsInRunOrder;
            var index = -1;
            for (var i = 0; i < calls.Count; i++) if (calls[i] == row.Call) index = i;
            var before = index > 0 ? calls[index - 1].OperationLocation : null;
            var after = index >= 0 && index < calls.Count - 1 ? calls[index + 1].OperationLocation : null;

            switch (row.Kind)
            {
                case DispatchRowKind.Arrival:
                    rows[SideOf(before ?? after)].Add(row);
                    break;
                case DispatchRowKind.Departure:
                    rows[SideOf(after ?? before)].Add(row);
                    break;
                case DispatchRowKind.PassThrough when SideOf(before) != SideOf(after):
                    var (arrival, departure) = row.SplitPassThrough();
                    rows[SideOf(before)].Add(arrival);
                    rows[SideOf(after)].Add(departure);
                    break;
                case DispatchRowKind.PassThrough:
                    rows[SideOf(before)].Add(row);
                    break;
                default:
                    rows[0].Add(row);
                    break;
            }
        }

        var neighbours = timetable.DispatchNeighboursOf(dispatcher);
        return
        [
            .. directions.Select((direction, side) =>
            {
                var beyond = neighbours.Where(neighbour => SideOf(neighbour) == side).ToList();
                var names = beyond.Count > 0 ? beyond.Select(neighbour => neighbour.Name) : direction.Adjacent.Select(location => location.Name);
                return whole with
                {
                    Rows = rows[side],
                    // A spur folded into the direction is rung like a neighbour when it has a number.
                    Neighbours = [.. beyond.Cast<OperationLocation>()
                        .Concat(direction.Spurs.OrderBy(spur => spur.Name, StringComparer.CurrentCulture))
                        .Select(neighbour => (neighbour.Name, timetable.PlanningOf(neighbour).PhoneNumber))
                        .Where(neighbour => neighbour.PhoneNumber.HasValue)
                        .Select(neighbour => new DispatchNeighbour(neighbour.Name, neighbour.PhoneNumber!.Value))],
                    RemoteLocations = [.. whole.RemoteLocations.Where(location => rows[side].Any(row => row.IsRemote && row.Call.OperationLocation.Equals(location)))],
                    Direction = string.Join(", ", names),
                };
            }),
        ];
    }

    // The timetable the station's planning is kept in: the plan's, else that of the trains listed.
    private static Timetable? TimetableOf(Plan? plan, IEnumerable<Train> trains) =>
        plan?.Timetable ?? trains.Select(train => (Timetable?)train.Timetable).FirstOrDefault(timetable => timetable is not null);
}
