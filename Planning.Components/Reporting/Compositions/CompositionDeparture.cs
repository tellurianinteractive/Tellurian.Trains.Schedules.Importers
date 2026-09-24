using System.Globalization;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

/// <summary>
/// One departure of a train from a station, with the composition it leaves with.
/// </summary>
/// <remarks>
/// The composition is everything the train carries over the stretch it departs onto — wagons coupled here as
/// well as those it arrived with — because that is what the staff have to check the train against, and where
/// wagons are added they have to see which positions are already taken.
/// </remarks>
public sealed class CompositionDeparture
{
    /// <summary>The call the train departs from.</summary>
    public required StationCall Call { get; init; }

    /// <summary>The groups the train leaves with, front first; never empty.</summary>
    public required IReadOnlyList<CompositionGroup> Groups { get; init; }

    /// <summary>
    /// The plain-text form of the sessions as the column shows them. Kept so the page-height estimate charges
    /// for exactly what is printed; the cell wraps rather than truncate.
    /// </summary>
    public required string SessionsText { get; init; }

    /// <summary>
    /// The plain-text form of the turnus column, one entry per line as the column prints them. Kept for the
    /// same reason as <see cref="SessionsText"/>: the cell wraps, so the estimate charges the lines.
    /// </summary>
    public required IReadOnlyList<string> TurnusTexts { get; init; }

    /// <summary>The train, named as it is announced — company signature, category prefix and number.</summary>
    public string TrainIdentity => Call.Train.Identity;

    /// <summary>The sessions or days the train runs.</summary>
    public Sessions Sessions => Call.Train.Sessions;

    /// <summary>The track the train departs from.</summary>
    public StationTrack Track => Call.Track;

    /// <summary>The departure time.</summary>
    public Time DepartureTime => Call.Departure;

    /// <summary>
    /// The arrival time, or <c>null</c> where the train starts its run here and so arrives from nowhere. The
    /// origin call does carry a time — the preparation the train is given before it moves — but printing it
    /// under an arrival heading would state an arrival that never happens.
    /// </summary>
    public Time? ArrivalTime => Call.IsTrainOrigin ? null : Call.Arrival;

    /// <summary>Where the train ends its run.</summary>
    public string? DestinationName => Call.Train.CallsInRunOrder.LastOrDefault()?.OperationLocation.Name;

    /// <summary>
    /// The most the train may be made up of. Unspecified where it is not restricted, and then nothing is
    /// printed: on paper a limit with no figure cannot be told from a forgotten one.
    /// </summary>
    /// <remarks>
    /// The load limits only. The train's maximum speed is a limit too, but it is not one the wagons are
    /// counted against, and this sheet is read while a train is made up.
    /// </remarks>
    public TrainCapacity Limit => Call.Train.Length;

    /// <summary>The plain-text form of the limit column, for the estimate. Empty where nothing limits the train.</summary>
    public string LimitText => CompactLimitText(Limit);

    /// <summary>
    /// The wagonsets the train leaves with, named by the identity written on their turnus cards, in the order
    /// their wagons stand in the composition.
    /// </summary>
    /// <remarks>
    /// The turnus is a column of its own rather than a caption over the rectangles: a caption has to fit the
    /// width of the rake it names, and with it gone every rectangle is free to be as wide as its own contents.
    /// </remarks>
    public IEnumerable<WagonsetComposition> Turnuses => Groups.OfType<WagonsetComposition>();

    /// <summary>
    /// Builds the departures one train makes from one station that leave with a composition to show.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A departure is the train's first call, or a later one where it stops; a train running past is not
    /// touched by the staff, and its last call departs nowhere. A shunting task is left out altogether: it
    /// works the station's sidings and departs onto no stretch.
    /// </para>
    /// <para>
    /// A departure is only listed when the train leaves with cargo flow wagons or with a wagonset whose
    /// wagons are listed. A wagonset that lists no wagons has no rectangles to draw, so it is not drawn.
    /// </para>
    /// </remarks>
    /// <param name="train">The train to take departures from.</param>
    /// <param name="station">The station the report page is for.</param>
    /// <param name="settings">How sessions are written, and how many there are.</param>
    /// <param name="plan">The plan whose schedules say which wagonsets work the train. Omit it and only cargo
    /// flows are shown.</param>
    public static IEnumerable<CompositionDeparture> Build(
        Train train, OperationLocation station, SessionsSettings settings, Plan? plan = null)
    {
        train = train.ValueOrException(nameof(train));
        station = station.ValueOrException(nameof(station));
        settings = settings.ValueOrException(nameof(settings));
        if (train.IsShuntingTask) yield break;

        var calls = train.CallsInRunOrder;
        for (var i = 0; i < calls.Count - 1; i++)
        {
            var call = calls[i];
            if (!call.OperationLocation.Equals(station) || (i > 0 && !call.IsStop)) continue;

            List<CompositionGroup> groups =
            [
                .. Wagonsets(train, calls, i, settings, plan),
                .. CargoPositions(train, calls, i),
            ];
            if (groups.Count == 0) continue;

            // Wagonsets before cargo at the same position: a wagonset is a fixed rake coupled as a whole,
            // and the loose wagons are marshalled around it.
            CompositionGroup[] ordered =
            [
                .. groups
                    .OrderBy(group => group.SortPosition)
                    .ThenBy(group => group is WagonsetComposition ? 0 : 1)
                    .ThenBy(group => (group as WagonsetComposition)?.Designation, StringComparer.CurrentCulture)
            ];

            yield return new CompositionDeparture
            {
                Call = call,
                Groups = ordered,
                SessionsText = train.Sessions.ToText(settings),
                // In the order the rakes stand in the composition, so the column and the rectangles read
                // together where a train carries more than one wagonset.
                TurnusTexts = [.. ordered.OfType<WagonsetComposition>().Select(wagonset => TurnusTextOf(wagonset, settings))],
            };
        }
    }

    // A limit in the compact form the report draws it in — 24●, 12■, 2.5m — as plain text, so the estimate
    // charges the column and the rectangle for what is printed in them. The marks are the same shapes the
    // MaxLoadView icons draw: a wheel and a wagon end-on.
    private static string CompactLimitText(TrainCapacity capacity)
    {
        List<string> limits = [];
        if (capacity.Axles is { } axles) limits.Add($"{axles}●");
        if (capacity.Wagons is { } wagons) limits.Add($"{wagons}■");
        if (capacity.Meters is { } metres) limits.Add($"{metres.ToString("0.0", CultureInfo.CurrentCulture)}m");
        return string.Join(" ", limits);
    }

    // One line of the turnus column: the wagonset's card identity, with the sessions it works the train on
    // where those are not all of the train's own.
    private static string TurnusTextOf(WagonsetComposition wagonset, SessionsSettings settings) =>
        wagonset.Sessions is { } sessions
            ? $"{wagonset.Designation} {sessions.ToText(settings)}"
            : wagonset.Designation;

    // The wagonsets that are in the train over the stretch departed onto from calls[index], each with the
    // sessions it is there on where those are not all the train's.
    private static IEnumerable<WagonsetComposition> Wagonsets(
        Train train, IReadOnlyList<StationCall> calls, int index, SessionsSettings settings, Plan? plan)
    {
        if (plan is null) yield break;

        var runs = train.Sessions.Capped(settings.MaxNumberOfSessions);
        foreach (var wagonset in plan.ScheduledObjects.Where(vehicle => vehicle.IsWagonSet))
        {
            var wagons = wagonset.Wagons.OrderBy(wagon => wagon.Position).ToList();
            if (wagons.Count == 0) continue;

            var covering = wagonset.ScheduleAssignments
                .Select(assignment => (assignment.Sessions, Part: (assignment.Schedule?.Parts ?? [])
                    .FirstOrDefault(part => part.Train.Equals(train) && Covers(calls, part, index))))
                .Where(assignment => assignment.Part is not null)
                .ToList();
            if (covering.Count == 0) continue;

            var assigned = covering.Aggregate(Sessions.FromBitPattern(0), (all, assignment) => all.Or(assignment.Sessions));
            var shared = assigned.And(runs);
            // Assigned only for sessions the train does not run: the wagonset is never in it.
            if (shared.Numbers.Length == 0) continue;

            yield return new WagonsetComposition
            {
                Wagonset = wagonset,
                Wagons = wagons,
                Position = covering.Select(assignment => assignment.Part!.WagonSetOptions?.OrderInTrain ?? 0).Max(),
                Sessions = shared.Numbers.SequenceEqual(runs.Numbers) ? null : shared,
            };
        }
    }

    // The cargo flow wagons in the train over the stretch departed onto from calls[index], one group per
    // position. The position is the flow's; a destination's own position is a different value.
    private static IEnumerable<CargoPositionComposition> CargoPositions(
        Train train, IReadOnlyList<StationCall> calls, int index) =>
        train.CargoFlows
            .Where(flow => flow.CargoFlowOptions is not null && Covers(calls, flow, index))
            .GroupBy(flow => flow.PositionInTrain)
            .Select(flows => new CargoPositionComposition
            {
                Position = flows.Key,
                Destinations = DestinationsOf([.. flows]),
            });

    // Where the wagons of the flows at one position go, and the most that may be brought to each. A flow to
    // all destinations says everything the others could, so it stands alone; otherwise each destination is
    // listed once, in the order the flows name them.
    private static IReadOnlyList<CompositionDestination> DestinationsOf(IReadOnlyList<CargoFlowTrainPart> flows)
    {
        if (flows.FirstOrDefault(flow => flow.CargoFlowOptions.ToAllDestinations) is { } toAll)
            return [new(toAll.ToText, new(toAll.ToHtml))];

        return
        [
            .. flows
                .SelectMany(flow => flow.CargoFlowOptions.Destinations)
                .Select(destination => new CompositionDestination(
                    Entry(destination.PlaceText, CompactLimitText(destination.MaxLoad)),
                    destination.PlaceHtml,
                    destination.MaxLoad))
                // By the whole entry, so that the same place under two different limits stays two lines: one
                // of them would otherwise be dropped and the wagons brought under a limit nobody stated.
                .DistinctBy(destination => destination.Text)
        ];
    }

    // A destination as one line of a rectangle: where the wagons go, and how many may go there.
    private static string Entry(string place, string limit) => limit.Length > 0 ? $"{place} {limit}" : place;

    // Whether a part is carried over the stretch departed onto from calls[index]: it is coupled at or before
    // that call and uncoupled after it. Positions in run order, since a train's calls are held in the order
    // they were added.
    private static bool Covers(IReadOnlyList<StationCall> calls, TrainPart part, int index)
    {
        var from = IndexOf(calls, part.From);
        var to = IndexOf(calls, part.To);
        return from >= 0 && from <= index && to > index;
    }

    private static int IndexOf(IReadOnlyList<StationCall> calls, StationCall call)
    {
        for (var i = 0; i < calls.Count; i++)
            if (calls[i].Equals(call)) return i;
        return -1;
    }
}
