using System.Globalization;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

/// <summary>
/// One departure of a train from a station, with the composition it leaves with — or one arrival, with the
/// cargo flow wagons uncoupled there.
/// </summary>
/// <remarks>
/// A cargo flow stands on the sheet of the station it is connected at and of no other: that station gathers
/// the wagons and marshals them into the train, while a station the flow only runs through has nothing to
/// make up and no wagons of its own to find. A wagonset stands likewise only where it is coupled: at the
/// first departure of its schedule, and later only where it is coupled explicitly — with a couple note,
/// fetched from another track, or coupled to a train already running. A rake that stays with its loco from
/// one train to the next is nothing the station has to make up.
/// An arrival is a row of its own, showing only the cargo flow wagons uncoupled there, each unit with where
/// its wagons came from: the station takes them off the train and sorts them on, and the origins tell the
/// units apart.
/// </remarks>
public sealed class CompositionDeparture
{
    /// <summary>The call the train departs from, or arrives at.</summary>
    public required StationCall Call { get; init; }

    /// <summary>Whether the row makes up a departing train or uncouples wagons from an arriving one.</summary>
    public CompositionMovement Movement { get; init; } = CompositionMovement.Departing;

    /// <summary>Whether this row is an arrival uncoupling cargo flow wagons.</summary>
    public bool IsArrival => Movement == CompositionMovement.Arriving;

    /// <summary>The groups the train leaves with, front first; never empty.</summary>
    public required IReadOnlyList<CompositionGroup> Groups { get; init; }

    /// <summary>
    /// The plain-text form of the sessions as the column shows them. Kept so the page-height estimate charges
    /// for exactly what is printed; the cell wraps rather than truncate.
    /// </summary>
    public required string SessionsText { get; init; }

    /// <summary>
    /// Which way the train travels past the station on this row: rightwards where it runs in the direction of
    /// the track stretch to the location it departs for, or from the one it arrives from. The loco is drawn
    /// at that end.
    /// </summary>
    public CompositionHeading Heading { get; init; } = CompositionHeading.Leftwards;

    /// <summary>The train, named as it is announced — company signature, category prefix and number.</summary>
    public string TrainIdentity => Call.Train.Identity;

    /// <summary>The sessions or days the train runs.</summary>
    public Sessions Sessions => Call.Train.Sessions;

    /// <summary>The track the train departs from.</summary>
    public StationTrack Track => Call.Track;

    /// <summary>
    /// The departure time, or <c>null</c> where the train ends its run here. An arrival row of a train that
    /// runs on shows it too: it is the time the wagons must be off the train by.
    /// </summary>
    public Time? DepartureTime => Call.IsTrainDestination ? null : Call.Departure;

    /// <summary>The time the row is sorted by: when its wagons are coupled or uncoupled.</summary>
    public Time Time => IsArrival ? Call.Arrival : Call.Departure;

    /// <summary>
    /// The arrival time, or <c>null</c> where the train starts its run here and so arrives from nowhere. The
    /// origin call does carry a time — the preparation the train is given before it moves — but printing it
    /// under an arrival heading would state an arrival that never happens.
    /// </summary>
    public Time? ArrivalTime => Call.IsTrainOrigin ? null : Call.Arrival;

    /// <summary>
    /// The train's times here as the loco rectangle prints them: "06:00-06:45" where it arrives and departs,
    /// "-06:45" where it starts its run here and "06:00-" where it ends it. The dash stays on its side, so a
    /// lone time is never read as the other one.
    /// </summary>
    public string TimeText => $"{ArrivalTime?.HHMM()}-{DepartureTime?.HHMM()}";

    /// <summary>
    /// The far end of the train's run as the row sees it: where it ends its run on a departure, where it
    /// started it on an arrival. The column marks which, so it is never read the wrong way round.
    /// </summary>
    public string? EndName => (IsArrival
        ? Call.Train.CallsInRunOrder.FirstOrDefault()
        : Call.Train.CallsInRunOrder.LastOrDefault())?.OperationLocation.Name;

    /// <summary>
    /// The most the train may be made up of. Unspecified where it is not restricted, and then nothing is
    /// printed: on paper a limit with no figure cannot be told from a forgotten one.
    /// </summary>
    /// <remarks>
    /// The load limits only. The train's maximum speed is a limit too, but it is not one the wagons are
    /// counted against, and this sheet is read while a train is made up.
    /// </remarks>
    public TrainCapacity Limit => Call.Train.Length;

    /// <summary>
    /// The plain-text form of the limit column, for the estimate. Empty where nothing limits the train, and on
    /// an arrival row, which prints no limit: nothing is made up there.
    /// </summary>
    public string LimitText => IsArrival ? string.Empty : CompactLimitText(Limit);

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
    /// An arrival is listed where cargo flow wagons are uncoupled, for the flows whose uncouple note is ticked:
    /// those are the ones the station is told to take off. At a shadow station every arriving flow is listed,
    /// ticked or not, because every train ends its run there and all its wagons come off.
    /// </para>
    /// <para>
    /// A departure is only listed when cargo flow wagons are connected here or the train leaves with a
    /// wagonset.
    /// </para>
    /// </remarks>
    /// <param name="train">The train to take departures from.</param>
    /// <param name="station">The station the report page is for.</param>
    /// <param name="settings">How sessions are written, and how many there are.</param>
    /// <param name="plan">The plan whose schedules say which wagonsets work the train. Omit it and only cargo
    /// flows are shown.</param>
    /// <param name="orientation">Where the locations lie left and right, which sets each row's heading. Omit
    /// it and every train travels leftwards.</param>
    public static IEnumerable<CompositionDeparture> Build(
        Train train, OperationLocation station, SessionsSettings settings, Plan? plan = null,
        CompositionOrientation? orientation = null)
    {
        train = train.ValueOrException(nameof(train));
        station = station.ValueOrException(nameof(station));
        settings = settings.ValueOrException(nameof(settings));
        if (train.IsShuntingTask) yield break;

        var calls = train.CallsInRunOrder;
        for (var i = 0; i < calls.Count; i++)
        {
            var call = calls[i];
            if (!call.OperationLocation.Equals(station)) continue;

            // Uncoupled before anything is coupled, so the arrival comes first where the train does both.
            if (i > 0 && ArrivingCargo(train, calls, i) is { Count: > 0 } arriving)
                yield return new CompositionDeparture
                {
                    Call = call,
                    Movement = CompositionMovement.Arriving,
                    Heading = orientation?.HeadingFrom(calls[i - 1].OperationLocation, station) ?? CompositionHeading.Leftwards,
                    Groups = arriving,
                    SessionsText = train.Sessions.ToText(settings),
                };

            if (i == calls.Count - 1 || (i > 0 && !call.IsStop)) continue;

            List<CompositionGroup> groups =
            [
                .. Wagonsets(train, calls, i, settings, plan),
                .. CargoPositions(train, calls, i),
            ];
            if (groups.Count == 0) continue;

            // Wagonsets before cargo at the same position: a wagonset is a fixed rake coupled as a whole,
            // and the loose wagons are marshalled around it. Cargo units then stand in the order their
            // destinations give them within the flow's position.
            CompositionGroup[] ordered =
            [
                .. groups
                    .OrderBy(group => group.SortPosition)
                    .ThenBy(group => group is WagonsetComposition ? 0 : 1)
                    .ThenBy(group => (group as CargoPositionComposition)?.SortDestinationPosition ?? 0)
                    .ThenBy(group => (group as WagonsetComposition)?.Designation, StringComparer.CurrentCulture)
            ];

            yield return new CompositionDeparture
            {
                Call = call,
                Heading = orientation?.HeadingFrom(station, calls[i + 1].OperationLocation) ?? CompositionHeading.Leftwards,
                Groups = ordered,
                SessionsText = train.Sessions.ToText(settings),
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

    // The wagonsets coupled to the train at calls[index], each with the sessions it is coupled on where those
    // are not all the train's. See IsCoupling for which parts count as a coupling.
    private static IEnumerable<WagonsetComposition> Wagonsets(
        Train train, IReadOnlyList<StationCall> calls, int index, SessionsSettings settings, Plan? plan)
    {
        if (plan is null) yield break;

        var runs = train.Sessions.Capped(settings.MaxNumberOfSessions);
        foreach (var wagonset in plan.ScheduledObjects.Where(vehicle => vehicle.IsWagonSet))
        {
            var covering = wagonset.ScheduleAssignments
                .Select(assignment => (assignment.Sessions, Part: (assignment.Schedule?.Parts ?? [])
                    .FirstOrDefault(part => part.Train.Equals(train) && IsCoupledAt(calls, part, index) &&
                        IsCoupling(assignment.Schedule!, part, index))))
                .Where(assignment => assignment.Part is not null)
                .ToList();
            if (covering.Count == 0) continue;

            var assigned = covering.Aggregate(Sessions.FromBitPattern(0), (all, assignment) => all.Or(assignment.Sessions));
            var shared = assigned.And(runs);
            // Assigned only for sessions the train does not run: the wagonset is never in it.
            if (shared.Numbers.Length == 0) continue;

            var sessions = shared.Numbers.SequenceEqual(runs.Numbers) ? (Sessions?)null : shared;
            yield return new WagonsetComposition
            {
                Wagonset = wagonset,
                Wagons = WagonsOf(wagonset),
                Position = covering.Select(assignment => assignment.Part!.WagonSetOptions?.OrderInTrain ?? 0).Max(),
                Sessions = sessions,
                SessionsText = sessions?.ToText(settings),
            };
        }
    }

    // The rectangles a wagonset is drawn as: wagon by wagon in rake order, with class and number. One that
    // lists no wagons is drawn by its turnus rectangle alone, since its designation already names its class.
    private static IReadOnlyList<CompositionWagon> WagonsOf(ScheduledObject wagonset) =>
        [.. wagonset.Wagons.OrderBy(wagon => wagon.Position).Select(wagon => new CompositionWagon(wagon.Class, wagon.Number))];

    // The cargo flow wagons connected at calls[index], grouped into the units they are marshalled as: the
    // flow's position in the train, and within it the position its destinations give the wagons.
    private static IEnumerable<CargoPositionComposition> CargoPositions(
        Train train, IReadOnlyList<StationCall> calls, int index) =>
        train.CargoFlows
            .Where(flow => flow.CargoFlowOptions is not null && ConnectsAt(calls, flow, index))
            .GroupBy(flow => flow.PositionInTrain)
            .SelectMany(flows => UnitsAt(flows.Key, [.. flows]));

    // The units of wagons the flows at one position in the train bring. Each position the destinations name
    // within the flow is a unit of its own, standing in its own place, and so gets a rectangle of its own.
    // A flow to all destinations says everything the others at the position could, so it stands alone; it
    // names no destination, and so nothing tells its wagons apart within the position either.
    private static IEnumerable<CargoPositionComposition> UnitsAt(int position, IReadOnlyList<CargoFlowTrainPart> flows)
    {
        if (flows.FirstOrDefault(flow => flow.CargoFlowOptions.ToAllDestinations) is { } toAll)
        {
            yield return new CargoPositionComposition
            {
                Position = position,
                Destinations = [new(toAll.ToText, new(toAll.ToHtml))],
            };
            yield break;
        }

        var units = flows
            .SelectMany(flow => flow.StatedDestinations)
            .GroupBy(destination => destination.PositionInTrain);
        foreach (var unit in units)
            yield return new CargoPositionComposition
            {
                Position = position,
                DestinationPosition = unit.Key,
                Destinations = DestinationsOf([.. unit]),
            };
    }

    // The cargo flow wagons uncoupled at calls[index], one unit per position the flows take in the train, front
    // first, each naming where its wagons came from.
    private static List<CompositionGroup> ArrivingCargo(Train train, IReadOnlyList<StationCall> calls, int index)
    {
        var isShadow = calls[index].OperationLocation is Station { IsShadow: true };
        var units = train.CargoFlows
            .Where(flow => flow.CargoFlowOptions is not null && (isShadow || flow.HasUncoupleNote) &&
                IndexOf(calls, flow.To) == index)
            .GroupBy(flow => flow.PositionInTrain)
            .Select(flows => (Position: flows.Key, Origins: flows.SelectMany(OriginsOf).ToList()))
            // A unit naming no origin brings no wagons: none from its from-station, and none forwarded.
            .Where(unit => unit.Origins.Count > 0)
            .OrderBy(unit => unit.Position > 0 ? unit.Position : int.MaxValue);

        // Each origin is named once in the row, in the front-most rectangle its wagons stand in: repeated, it
        // only lengthens the rectangles behind. A rectangle left with nothing to name is not drawn.
        var named = new HashSet<string>(StringComparer.CurrentCulture);
        return
        [
            .. units
                .Select(unit => new ArrivingCargoComposition
                {
                    Position = unit.Position,
                    Origins = [.. unit.Origins.Where(named.Add)],
                })
                .Where(unit => unit.Origins.Count > 0)
        ];
    }

    // Where a flow's wagons came from: its own from-station, unless it brings no wagons from there, and the
    // origins it forwards.
    private static IEnumerable<string> OriginsOf(CargoFlowTrainPart flow)
    {
        if (!flow.BringsNoWagonsFromHere) yield return flow.From.OperationLocation.Name;
        foreach (var origin in flow.CargoFlowOptions.Origins) yield return origin.Location.Name;
    }

    // Where the wagons of one unit go, and the most that may be brought to each. Each place is listed once, in
    // the order the flows name them, and the regions the destinations include follow all the places, each once:
    // a region is the widest of destinations, and among the places it would read as one more station.
    private static IReadOnlyList<CompositionDestination> DestinationsOf(IReadOnlyList<Destination> destinations) =>
    [
        .. destinations
            .Select(destination => new CompositionDestination(
                Entry(destination.PlaceTextWithoutRegions, CompactLimitText(destination.MaxLoad)),
                destination.PlaceHtmlWithoutRegions,
                destination.MaxLoad))
            // By the whole entry, so that the same place under two different limits stays two entries: one of
            // them would otherwise be dropped and the wagons brought under a limit nobody stated.
            .DistinctBy(destination => destination.Text),
        .. destinations
            .SelectMany(destination => destination.StatedRegions)
            .DistinctBy(region => region.Name)
            .Select(region => new CompositionDestination(region.Name, region.ToHtml)),
    ];

    // A destination as one line of a rectangle: where the wagons go, and how many may go there.
    private static string Entry(string place, string limit) => limit.Length > 0 ? $"{place} {limit}" : place;

    // Whether a cargo flow's wagons are connected at calls[index], which is the flow's from-call. A flow that
    // is only carried past this call brings the station no wagons to gather, so it is not on its sheet.
    private static bool ConnectsAt(IReadOnlyList<StationCall> calls, CargoFlowTrainPart flow, int index) =>
        IndexOf(calls, flow.From) == index;

    // Whether a part is coupled to the train at calls[index] and carried on from there. Positions in run
    // order, since a train's calls are held in the order they were added.
    private static bool IsCoupledAt(IReadOnlyList<StationCall> calls, TrainPart part, int index) =>
        IndexOf(calls, part.From) == index && IndexOf(calls, part.To) > index;

    // Whether the wagonset is coupled to the train at the start of a part, rather than staying in the train
    // its loco works on from the previous part: the schedule starts there, the part says the wagonset is to be
    // coupled or fetched from another track, or the train is already running and so arrived without it.
    private static bool IsCoupling(Schedule schedule, ScheduledTrainPart part, int fromIndex) =>
        ReferenceEquals(part, schedule.FirstPart) ||
        part.WagonSetOptions?.HasCoupleNote == true ||
        part.OtherFromTrack is not null ||
        fromIndex > 0;

    private static int IndexOf(IReadOnlyList<StationCall> calls, StationCall call)
    {
        for (var i = 0; i < calls.Count; i++)
            if (calls[i].Equals(call)) return i;
        return -1;
    }
}

/// <summary>Whether a composition row makes up a departing train or uncouples wagons from an arriving one.</summary>
public enum CompositionMovement
{
    /// <summary>The train leaves with the composition shown.</summary>
    Departing,

    /// <summary>The cargo flow wagons shown are uncoupled from the arriving train.</summary>
    Arriving,
}
