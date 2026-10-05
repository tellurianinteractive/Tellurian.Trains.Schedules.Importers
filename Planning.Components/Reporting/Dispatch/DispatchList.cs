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
/// station's own: a second sheet would have to be merged by eye while trains are moving. Rows at a remote
/// location are only listed for trains that do not also call at the station itself.
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

        var trainList = trains.ToList();
        var rows = dispatched
            .SelectMany(location => trainList
                // A train that also calls at the station itself is already in the list through its rows
                // there; its rows at a remote location would only repeat it. Only a train that passes the
                // remote location without touching the station needs rows of its own.
                .Where(train => location.Equals(station) || !train.Calls.Any(call => call.OperationLocation.Equals(station)))
                .SelectMany(train => DispatchRow.Build(train, location, settings, plan, isRemote: !location.Equals(station))))
            // Time first, because the list is worked through in time order. The tie-breaks only make the
            // order of simultaneous clearances stable from one print to the next: arrivals before
            // departures, since a train pulling in has to be cleared in before the platform is given away.
            .OrderBy(row => row.Time)
            .ThenBy(row => row.Kind is DispatchRowKind.Arrival or DispatchRowKind.ShuntingTask ? 0 : 1)
            .ThenBy(row => row.TrainIdentity, StringComparer.CurrentCulture)
            .ToList();

        // Only the ones that can be rung: see DispatchNeighbour for why a number is not optional here.
        var neighbours = layout is not null && station is Station endpoint
            ? layout.DispatchNeighboursOf(endpoint)
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
}
