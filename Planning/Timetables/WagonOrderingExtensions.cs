using Tellurian.Trains.Schedules.Planning.Layouts;

namespace Tellurian.Trains.Schedules.Planning.Timetables;

/// <summary>
/// One segment of a wagonset's journey over which the wagon order is constant: a maximal run in one
/// travel direction within a single train part. The <see cref="Order"/> tells whether the rake is shown
/// as arranged or reversed, and <see cref="Wagons"/> is the rake already in presentation order.
/// </summary>
/// <param name="Part">The train part this leg belongs to.</param>
/// <param name="From">The station call the leg starts from.</param>
/// <param name="To">The station call the leg ends at.</param>
/// <param name="Direction">The travel direction relative to the track stretch, when it could be resolved.</param>
/// <param name="Order">Whether the rake is shown as arranged or reversed on this leg.</param>
/// <param name="Wagons">The rake in presentation order for this leg.</param>
public record ScheduleWagonLeg(
    ScheduledTrainPart Part,
    StationCall From,
    StationCall To,
    TrainPathDirection? Direction,
    UnitOrder Order,
    IReadOnlyList<ScheduledUnit> Wagons);

/// <summary>
/// Computes the direction-correct wagon order of a wagonset's rake over the legs of the schedule it works.
/// </summary>
public static class WagonOrderingExtensions
{
    extension(ScheduledObject wagonset)
    {
        /// <summary>
        /// Splits the wagonset's journey (its schedule's parts, in working order) into legs and gives, per
        /// leg, the rake in presentation order. The order is seeded <see cref="UnitOrder.AsArranged"/> at the
        /// first leg — the order the wagons are arranged in the first train — and flips at every direction
        /// change, treating the ordered parts as one continuous journey (so a reversal between two trains
        /// flips the order just like one within a train). A new leg is also started at each part boundary so
        /// every leg belongs to a single train. Empty when the vehicle has no wagons or works no parts.
        /// </summary>
        /// <param name="orderedParts">The parts the wagonset works, in working (departure) order.</param>
        public IReadOnlyList<ScheduleWagonLeg> WagonOrderByLeg(IReadOnlyList<ScheduledTrainPart> orderedParts)
        {
            var arranged = wagonset.Units.OrderBy(w => w.Position).ToList();
            if (arranged.Count == 0 || orderedParts.Count == 0) return [];
            var plan = wagonset.Plan;

            // 1. Expand every part into its consecutive station-to-station steps across the whole journey,
            //    resolving each step's travel direction relative to the track stretch (null when unknown).
            var steps = new List<(ScheduledTrainPart Part, StationCall From, StationCall To, TrainPathDirection? Direction)>();
            foreach (var part in orderedParts)
            {
                var calls = part.Train.Calls.OrderBy(c => c.SortTime).ToList();
                var fromIndex = calls.FindIndex(c => ReferenceEquals(c, part.From));
                var toIndex = calls.FindIndex(c => ReferenceEquals(c, part.To));
                if (fromIndex < 0 || toIndex <= fromIndex)
                {
                    // Can't expand into intermediate calls; treat the part as a single step.
                    steps.Add((part, part.From, part.To, LegDirection(plan, part.From, part.To)));
                    continue;
                }
                for (var i = fromIndex; i < toIndex; i++)
                    steps.Add((part, calls[i], calls[i + 1], LegDirection(plan, calls[i], calls[i + 1])));
            }
            if (steps.Count == 0) return [];

            // 2. Group steps into legs, bounded by direction changes and part boundaries. The presentation
            //    order flips only on a direction change; a plain part boundary keeps it.
            var legs = new List<ScheduleWagonLeg>();
            var order = UnitOrder.AsArranged;
            var runStart = 0;
            var lastDirection = steps[0].Direction;
            for (var i = 1; i <= steps.Count; i++)
            {
                var directionChanged = false;
                var boundary = i == steps.Count;
                if (!boundary)
                {
                    var direction = steps[i].Direction;
                    directionChanged = direction.HasValue && lastDirection.HasValue && direction.Value != lastDirection.Value;
                    var partChanged = !ReferenceEquals(steps[i].Part, steps[i - 1].Part);
                    boundary = directionChanged || partChanged;
                    if (direction.HasValue) lastDirection = direction;
                }
                if (!boundary) continue;

                var wagons = order == UnitOrder.AsArranged
                    ? (IReadOnlyList<ScheduledUnit>)arranged
                    : [.. Enumerable.Reverse(arranged)];
                legs.Add(new ScheduleWagonLeg(
                    steps[runStart].Part, steps[runStart].From, steps[i - 1].To, steps[runStart].Direction, order, wagons));

                if (directionChanged) order = order == UnitOrder.AsArranged ? UnitOrder.Reversed : UnitOrder.AsArranged;
                runStart = i;
            }
            return legs;
        }
    }

    // The travel direction of a single leg relative to its track stretch, or null when no stretch joins the
    // two locations. Forward means Start->End; the convention matches the path finder and timing code.
    private static TrainPathDirection? LegDirection(Plan plan, StationCall from, StationCall to)
    {
        var fromLocation = from.OperationLocation;
        if (plan.StretchBetween(fromLocation, to.OperationLocation) is not { } stretch) return null;
        return stretch.Start.Equals(fromLocation) ? TrainPathDirection.Forward : TrainPathDirection.Backward;
    }
}
