namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Builds vehicle <see cref="Schedule">schedules</see> (turnus) from the trains of a <see cref="Plan"/>.
/// </summary>
/// <remarks>
/// Automatic building greedily chains whole trains of the same category into contiguous workings: a
/// schedule is seeded from the earliest unassigned train, then extended by the earliest same-category
/// train that departs where the schedule currently ends, at or after that arrival, regardless of how
/// long the vehicle waits. Trains whose category is flagged
/// <see cref="TrainCategory.ExcludeFromAutomaticScheduling"/> are never used. The manual
/// <see cref="PlanScheduleBuilderExtensions.ContinuationsFor"/> query offers the trains that could
/// extend a schedule the planner is building by hand, across categories.
/// </remarks>
public static class PlanScheduleBuilderExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Builds schedules automatically for all trains not already assigned to a schedule and not
        /// excluded by their category, adds them to the plan, and returns them. Existing schedules are
        /// left untouched.
        /// </summary>
        /// <returns>The schedules created, in build order.</returns>
        public IReadOnlyList<Schedule> BuildSchedulesAutomatically()
        {
            plan = plan.ValueOrException(nameof(plan));
            var used = plan.AssignedTrains();
            var seeds = plan.SchedulableTrains()
                .Where(t => !t.Category!.ExcludeFromAutomaticScheduling)
                .Where(t => !used.Contains(t))
                .OrderBy(t => t.ScheduleDeparture())
                .ThenBy(t => t.Number)
                .ToList();

            var nextId = (plan.Schedules.Count == 0 ? 0 : plan.Schedules.Max(s => s.Id)) + 1;
            var created = new List<Schedule>();
            foreach (var seed in seeds)
            {
                if (used.Contains(seed)) continue;
                var schedule = new Schedule(nextId++);
                schedule.Append(seed.AsTrainPart);
                used.Add(seed);
                for (var next = plan.NextInChain(schedule, seed.Category!, used);
                     next is not null;
                     next = plan.NextInChain(schedule, seed.Category!, used))
                {
                    if (schedule.Append(next.AsTrainPart).IsNone) break;
                    used.Add(next);
                }
                plan.AddVehicleSchedule(schedule);
                created.Add(schedule);
            }
            return created;
        }

        /// <summary>
        /// Gets the trains that could extend the given schedule when building it manually: unassigned
        /// trains (of any category) that depart the schedule's end location at or after its last
        /// arrival, ordered by departure. For an empty schedule this is every unassigned train, i.e.
        /// the possible starting trains.
        /// </summary>
        /// <param name="schedule">The schedule being built.</param>
        /// <returns>The candidate continuation trains, ordered by departure then number.</returns>
        public IReadOnlyList<Train> ContinuationsFor(Schedule schedule)
        {
            plan = plan.ValueOrException(nameof(plan));
            schedule = schedule.ValueOrException(nameof(schedule));
            var used = plan.AssignedTrains();
            var candidates = plan.SchedulableTrains().Where(t => !used.Contains(t));
            if (schedule.EndLocation is { } endLocation && schedule.LastArrival is { } lastArrival)
            {
                candidates = candidates.Where(t =>
                    t.ScheduleStart().Equals(endLocation) &&
                    t.ScheduleDeparture() >= lastArrival);
            }
            return [.. candidates.OrderBy(t => t.ScheduleDeparture()).ThenBy(t => t.Number)];
        }

        /// <summary>
        /// Gets the trains already assigned to any schedule in the plan.
        /// </summary>
        private HashSet<Train> AssignedTrains() =>
            [.. plan.Schedules.SelectMany(s => s.Parts).Select(p => p.Train)];

        /// <summary>
        /// Gets the trains that can take part in a schedule: those with a category and at least two
        /// station calls (a from/to span).
        /// </summary>
        private IEnumerable<Train> SchedulableTrains() =>
            plan.Timetable.Trains.Where(t => t.Category is not null && t.Calls.Count >= 2);

        /// <summary>
        /// Finds the earliest unassigned train of the given category that continues the schedule.
        /// </summary>
        private Train? NextInChain(Schedule schedule, TrainCategory category, HashSet<Train> used) =>
            plan.SchedulableTrains()
                .Where(t => !used.Contains(t))
                .Where(t => category.Equals(t.Category))
                .Where(t => schedule.EndLocation is { } end && t.ScheduleStart().Equals(end))
                .Where(t => schedule.LastArrival is { } arrival && t.ScheduleDeparture() >= arrival)
                .OrderBy(t => t.ScheduleDeparture())
                .ThenBy(t => t.Number)
                .FirstOrDefault();
    }

    extension(Train train)
    {
        /// <summary>
        /// Gets the call the train runs first. <see cref="Train.Calls"/> is in insertion order, which on
        /// a hand-edited train is not the order it runs them, so the origin is the earliest call rather
        /// than the first one added.
        /// </summary>
        private StationCall ScheduleOrigin() => train.Calls.MinBy(c => c.SortTime)!;

        /// <summary>Gets the location the train starts from (its first call).</summary>
        private OperationLocation ScheduleStart() => train.ScheduleOrigin().OperationLocation;

        /// <summary>Gets the train's departure time from its origin (its first call).</summary>
        private Time ScheduleDeparture() => train.ScheduleOrigin().Departure;
    }
}
