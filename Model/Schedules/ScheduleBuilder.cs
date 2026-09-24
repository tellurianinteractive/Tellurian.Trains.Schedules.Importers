namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Builds vehicle <see cref="Schedule">schedules</see> (turnus) from the trains of a <see cref="Plan"/>.
/// </summary>
/// <remarks>
/// Automatic building greedily chains whole trains of the same category into contiguous workings: a
/// schedule is extended by the earliest unassigned same-category train that departs where the schedule
/// currently ends, at or after that arrival, regardless of how long the vehicle waits. The schedules the
/// plan already has are offered the trains first — a vehicle that is already turning should take on more
/// work before another vehicle is put in service — and only the trains that fit none of them seed new
/// schedules. Chaining goes through <see cref="ScheduleExtensions.Append"/>, so a working whose vehicle
/// cannot change tracks on its own has its arrival tracks put where its next train departs from as the
/// chain grows (see <see cref="ScheduleTrackAlignmentExtensions.AlignArrivalTracks"/>); a working the
/// build creates has no vehicle yet, and is aligned when one is assigned to it. Trains whose category is
/// flagged
/// <see cref="TrainCategory.ExcludeFromAutomaticScheduling"/> are never used. The manual
/// <see cref="PlanScheduleBuilderExtensions.ContinuationsFor"/> query offers the trains that could
/// extend a schedule the planner is building by hand, across categories.
/// </remarks>
public static class PlanScheduleBuilderExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Builds schedules automatically from the trains not already assigned to a schedule and not
        /// excluded by their category: first by extending the schedules the plan already has, then by
        /// creating new schedules for the trains that are left over. Created schedules are added to
        /// the plan.
        /// </summary>
        /// <returns>What the build added to existing schedules and what it created.</returns>
        public ScheduleBuildResult BuildSchedulesAutomatically()
        {
            plan = plan.ValueOrException(nameof(plan));
            var used = plan.AssignedTrains();

            var extended = new List<Schedule>();
            var addedParts = 0;
            foreach (var schedule in plan.SchedulesToExtend())
            {
                var added = plan.Extend(schedule, used);
                if (added == 0) continue;
                extended.Add(schedule);
                addedParts += added;
            }

            var nextId = (plan.Schedules.Count == 0 ? 0 : plan.Schedules.Max(s => s.Id)) + 1;
            var created = new List<Schedule>();
            foreach (var seed in plan.SeedTrains(used))
            {
                if (used.Contains(seed)) continue;
                var schedule = new Schedule(nextId++);
                schedule.Append(seed.AsTrainPart);
                used.Add(seed);
                plan.Chain(schedule, used);
                plan.AddVehicleSchedule(schedule);
                created.Add(schedule);
            }
            return new ScheduleBuildResult(created, extended, addedParts);
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
        /// Gets the schedules an automatic build offers the unassigned trains to, in the order they get
        /// to pick.
        /// </summary>
        /// <remarks>
        /// A cargo-flow schedule is the circulation of a consignment and not of a turning vehicle, so it
        /// is left out: its wagons go where their waybills send them, not where a chain of trains happens
        /// to lead. A working that is already running picks before an empty schedule, which has nothing
        /// to continue and is therefore seeded from what is left over, and among the running ones the
        /// vehicle that becomes free earliest picks first.
        /// </remarks>
        private IReadOnlyList<Schedule> SchedulesToExtend() =>
            [.. plan.Schedules
                .Where(s => !s.IsCargoFlow)
                .OrderBy(s => s.Parts.Count == 0)
                .ThenBy(s => s.LastArrival)
                .ThenBy(s => s.Id)];

        /// <summary>
        /// Gives a schedule that is already in the plan as much of the unassigned work as it can take:
        /// an empty one is seeded like a new schedule, then the working is chained on as far as it
        /// reaches.
        /// </summary>
        /// <param name="schedule">The schedule to extend.</param>
        /// <param name="used">The trains already spoken for, added to as the schedule takes them.</param>
        /// <returns>The number of parts added to the schedule.</returns>
        private int Extend(Schedule schedule, HashSet<Train> used)
        {
            var added = 0;
            if (schedule.Parts.Count == 0)
            {
                if (plan.SeedTrains(used).FirstOrDefault() is not { } seed) return 0;
                if (schedule.Append(seed.AsTrainPart).IsNone) return 0;
                used.Add(seed);
                added++;
            }
            return added + plan.Chain(schedule, used);
        }

        /// <summary>
        /// Chains unassigned trains of the working's own category onto the end of the schedule for as
        /// long as one continues it.
        /// </summary>
        /// <param name="schedule">The schedule to chain onto.</param>
        /// <param name="used">The trains already spoken for, added to as the schedule takes them.</param>
        /// <returns>The number of parts chained on.</returns>
        private int Chain(Schedule schedule, HashSet<Train> used)
        {
            // The category to carry on with is the one the vehicle works where the chaining starts. An
            // imported or hand-built working may have been put together from several categories; the
            // build continues it as what it has become, and never continues a category the planner has
            // excluded from automatic scheduling.
            if (schedule.LastPart?.Train.Category is not { } category) return 0;
            if (category.ExcludeFromAutomaticScheduling) return 0;
            var added = 0;
            for (var next = plan.NextInChain(schedule, category, used);
                 next is not null;
                 next = plan.NextInChain(schedule, category, used))
            {
                if (schedule.Append(next.AsTrainPart).IsNone) break;
                used.Add(next);
                added++;
            }
            return added;
        }

        /// <summary>
        /// Gets the trains an automatic build may start a working from, earliest departure first.
        /// </summary>
        private IReadOnlyList<Train> SeedTrains(HashSet<Train> used) =>
            [.. plan.SchedulableTrains()
                .Where(t => !t.Category!.ExcludeFromAutomaticScheduling)
                .Where(t => !used.Contains(t))
                .OrderBy(t => t.ScheduleDeparture())
                .ThenBy(t => t.Number)];

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
                .Where(t => !t.Category!.ExcludeFromAutomaticScheduling)
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
