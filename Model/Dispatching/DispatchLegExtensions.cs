using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Model.Dispatching;

/// <summary>
/// How a train's run is divided into parts cleared as one, and who has to agree on each.
/// </summary>
/// <remarks>
/// <list type="number">
/// <item>On a dispatch stretch with different dispatchers at its ends, they agree between them
/// (<see cref="DispatchLegKind.BetweenDispatchers"/>). A dispatcher working a junction with signals from afar can
/// accept trains towards it while holding the junction at stop for others.</item>
/// <item>On a dispatch stretch with the same dispatcher at both ends, that dispatcher clears the train alone
/// (<see cref="DispatchLegKind.OneDispatcher"/>).</item>
/// <item>On a dispatch stretch with a dispatcher at one end only, the driver calls that dispatcher
/// (<see cref="DispatchLegKind.DriverCalls"/>).</item>
/// <item>Places along a dispatch stretch without a dispatcher of their own, such as an unmanned station or a
/// junction without signals, are part of the line: a train starting, ending, joining or leaving there is cleared
/// between the dispatchers at the ends of the stretch.</item>
/// <item>A train whose first or last place is on no dispatch stretch, such as a branch to an industrial area, is
/// cleared by the dispatcher at the place next to it: the driver calls (<see cref="DispatchLegKind.DriverCalls"/>).</item>
/// </list>
/// A part of the run with no dispatcher at either end is not cleared by anybody and has no leg.
/// </remarks>
public static class DispatchLegExtensions
{
    extension(Train train)
    {
        /// <summary>
        /// The parts of the train's run that are cleared as one, in run order. See <see cref="DispatchLegExtensions"/>.
        /// </summary>
        public IReadOnlyList<DispatchLeg> DispatchLegs
        {
            get
            {
                var calls = train.CallsInRunOrder;
                if (calls.Count < 2) return [];
                var stretches = train.Timetable?.DispatchStretches.ToList() ?? [];

                var points = new SortedSet<int> { 0, calls.Count - 1 };
                for (var i = 1; i < calls.Count - 1; i++)
                    if (calls[i].OperationLocation.IsDispatchEndpoint) points.Add(i);

                // Where the run between two clearance places is on no single dispatch stretch, it joins or leaves one
                // at a place along it: that place is cleared too.
                foreach (var (a, b) in Pairs([.. points]))
                {
                    if (StretchWith(stretches, calls[a].OperationLocation, calls[b].OperationLocation) is not null) continue;
                    if (EndOfRunAlong(stretches, calls, a, b, step: 1) is { } exit) points.Add(exit);
                    if (EndOfRunAlong(stretches, calls, b, a, step: -1) is { } entry) points.Add(entry);
                }

                return [.. Pairs([.. points]).Select(pair => Leg(stretches, calls[pair.Item1], calls[pair.Item2])).OfType<DispatchLeg>()];
            }
        }
    }

    private static IEnumerable<(int, int)> Pairs(IReadOnlyList<int> points) =>
        points.Zip(points.Skip(1));

    private static DispatchLeg? Leg(IReadOnlyList<DispatchStretch> stretches, StationCall from, StationCall to)
    {
        var stretch = StretchWith(stretches, from.OperationLocation, to.OperationLocation);
        Station? fromDispatcher, toDispatcher;
        if (stretch is null)
        {
            fromDispatcher = from.OperationLocation.Dispatcher;
            toDispatcher = to.OperationLocation.Dispatcher;
        }
        else
        {
            var places = PlacesAlong(stretch);
            var isForward = places.IndexOf(from.OperationLocation) < places.IndexOf(to.OperationLocation);
            fromDispatcher = (isForward ? stretch.From : stretch.To).Dispatcher;
            toDispatcher = (isForward ? stretch.To : stretch.From).Dispatcher;
        }

        DispatchLegKind? kind = (fromDispatcher, toDispatcher) switch
        {
            (null, null) => null,
            (null, _) or (_, null) => DispatchLegKind.DriverCalls,
            _ when fromDispatcher!.Equals(toDispatcher) => DispatchLegKind.OneDispatcher,
            _ => DispatchLegKind.BetweenDispatchers,
        };
        return kind is { } k
            ? new DispatchLeg(from, to, k) { Stretch = stretch, FromDispatcher = fromDispatcher, ToDispatcher = toDispatcher }
            : null;
    }

    // From the clearance place at index from, follows the run towards index until (exclusive) while the places are
    // along a dispatch stretch that has the place at from as one end, and gives the furthest such index, if any.
    private static int? EndOfRunAlong(IReadOnlyList<DispatchStretch> stretches, IReadOnlyList<StationCall> calls, int from, int until, int step)
    {
        var place = calls[from].OperationLocation;
        int? furthest = null;
        foreach (var stretch in stretches.Where(s => s.From.Equals(place) || s.To.Equals(place)))
        {
            var along = PlacesAlong(stretch);
            for (var i = from + step; i != until && along.Contains(calls[i].OperationLocation); i += step)
            {
                if (furthest is null || Math.Abs(i - from) > Math.Abs(furthest.Value - from)) furthest = i;
            }
        }
        return furthest;
    }

    private static DispatchStretch? StretchWith(IEnumerable<DispatchStretch> stretches, OperationLocation a, OperationLocation b) =>
        stretches.FirstOrDefault(stretch => PlacesAlong(stretch) is var places && places.Contains(a) && places.Contains(b));

    private static List<OperationLocation> PlacesAlong(DispatchStretch stretch) =>
        [stretch.From, .. stretch.IntermediateLocations, stretch.To];
}
