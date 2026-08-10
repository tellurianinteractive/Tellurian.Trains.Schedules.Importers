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
/// <param name="Station">The station the list is for.</param>
/// <param name="Rows">Every clearance, ascending by time.</param>
/// <param name="Neighbours">The dispatch stretches' far ends that can actually be rung.</param>
public sealed record DispatchList(
    OperationLocation Station,
    IReadOnlyList<DispatchRow> Rows,
    IReadOnlyList<DispatchNeighbour> Neighbours)
{
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

        var rows = trains
            .SelectMany(train => DispatchRow.Build(train, station, settings, plan))
            // Time first, because the list is worked through in time order. The tie-breaks only make the
            // order of simultaneous clearances stable from one print to the next: arrivals before
            // departures, since a train pulling in has to be cleared in before the platform is given away.
            .OrderBy(row => row.Time)
            .ThenBy(row => row.Kind == DispatchRowKind.Arrival ? 0 : 1)
            .ThenBy(row => row.TrainIdentity, StringComparer.CurrentCulture)
            .ToList();

        // Only the ones that can be rung: see DispatchNeighbour for why a number is not optional here.
        var neighbours = station.Layout is { } layout && station is Station endpoint
            ? layout.DispatchNeighboursOf(endpoint)
                .Where(neighbour => neighbour.PhoneNumber.HasValue)
                .Select(neighbour => new DispatchNeighbour(neighbour.Name, neighbour.PhoneNumber!.Value))
                .ToList()
            : [];

        return new DispatchList(station, rows, neighbours);
    }
}
