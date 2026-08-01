namespace Tellurian.Trains.Schedules.Model.Validations;

/// <summary>
/// Rules for the two flags that say whether a train stops at a call, <see cref="StationCall.IsArrival"/>
/// and <see cref="StationCall.IsDeparture"/>: where they are the planner's to set, where they are forced
/// on, and where they count for nothing.
/// </summary>
/// <remarks>
/// <para>
/// Two things constrain a call. The <em>location</em> decides whether the train can stop there at all — a
/// signal controlled location never lets it, and elsewhere the train needs somewhere to exchange what it
/// carries (see <c>Train.CanStopAt</c>). That rule needs nothing applied: <see cref="StationCall.IsStop"/>
/// already answers through <c>StationCall.CanBeStop</c>, so a call at a location the train cannot stop at
/// is a pass-through however its flags stand. The flags are left alone rather than cleared, so a stop
/// planned while the location still exchanged passengers or cargo comes back if the exchange is restored;
/// the editor shows them cleared and disabled meanwhile.
/// </para>
/// <para>
/// The <em>train parts</em> constrain the other way, and this one has to be applied. A train part runs
/// from a call the train departs to a call it arrives at, so the train must stop at both ends of it. The
/// train's own run is such a part, bounded by its origin and its destination, and so are the parts a
/// vehicle schedule, a driver duty or a cargo flow is planned over. A flag holding up one of those ends
/// is forced on, and the editor offers it disabled so it cannot be taken away.
/// </para>
/// <para>
/// Where the two meet — a part ends at a location the train cannot stop at — the flag is still forced on,
/// but the call is a pass-through all the same, which is what makes the fault visible. The remedy is to
/// move the call or the part, and neither is the model's to choose.
/// </para>
/// <para>
/// <c>ApplyStopRules</c> mutates and does not persist, so it obliges the caller to save (in the app,
/// <c>ScheduleState.SaveAndNotify()</c>). Reading a plan applies it on the way in.
/// </para>
/// </remarks>
public static class StopRules
{
    extension(Plan plan)
    {
        /// <summary>
        /// Whether <see cref="StationCall.IsDeparture"/> is forced on at this call: the train starts its
        /// run here, or a train part departs from here.
        /// </summary>
        /// <param name="call">The call to test.</param>
        public bool IsDepartureRequired(StationCall call) =>
            call is not null &&
            (call.IsTrainOrigin || TrainParts(plan).Any(part => ReferenceEquals(part.From, call)));

        /// <summary>
        /// Whether <see cref="StationCall.IsArrival"/> is forced on at this call: the train ends its run
        /// here, or a train part arrives here.
        /// </summary>
        /// <param name="call">The call to test.</param>
        public bool IsArrivalRequired(StationCall call) =>
            call is not null &&
            (call.IsTrainDestination || TrainParts(plan).Any(part => ReferenceEquals(part.To, call)));

        /// <summary>
        /// Sets the stop flags that the plan's train parts depend on, so no part is left starting or
        /// ending where its train does not stop. Nothing is ever cleared. Returns the number of calls
        /// changed, so a caller can save only when something actually moved. Idempotent.
        /// </summary>
        /// <remarks>
        /// Runs over the timetable's trains, which own the calls; the per-track index is derived from them
        /// (see <c>Timetable.RebuildStationCalls</c>) and needs nothing done to it here. The parts are
        /// gathered once rather than searched per call, so a plan with many duties costs no more than one
        /// with none.
        /// </remarks>
        public int ApplyStopRules()
        {
            plan = plan.ValueOrException(nameof(plan));
            if (plan.Timetable is not { } timetable) return 0;
            var parts = TrainParts(plan).ToList();
            var departsFrom = CallSet(parts.Select(part => part.From));
            var arrivesAt = CallSet(parts.Select(part => part.To));
            var changed = 0;
            foreach (var train in timetable.Trains)
            {
                // Insertion order is not run order: the train's ends are the earliest and latest call.
                var ordered = train.CallsInRunOrder;
                if (ordered.Count == 0) continue;
                foreach (var call in ordered)
                {
                    var isArrival = call.IsArrival || ReferenceEquals(call, ordered[^1]) || arrivesAt.Contains(call);
                    var isDeparture = call.IsDeparture || ReferenceEquals(call, ordered[0]) || departsFrom.Contains(call);
                    if (isArrival == call.IsArrival && isDeparture == call.IsDeparture) continue;
                    call.IsArrival = isArrival;
                    call.IsDeparture = isDeparture;
                    changed++;
                }
            }
            return changed;
        }
    }

    // Every train part the plan holds. A part belongs to a vehicle schedule, to a driver duty (which
    // shares the schedule's parts) or, for a cargo flow, to the train itself; all three are reachable
    // from the plan and all three bind their end calls the same way. Duplicates do not matter: the
    // callers only ask whether a call ends any part at all.
    private static IEnumerable<TrainPart> TrainParts(Plan plan) =>
        plan.Schedules.SelectMany(schedule => schedule.Parts)
            .Concat<TrainPart>(plan.DriverDuties.SelectMany(duty => duty.Parts))
            .Concat(plan.Timetable is { } timetable ? timetable.Trains.SelectMany(train => train.CargoFlows) : []);

    // Calls are gathered by identity, never by value: two calls of one train can compare equal
    // (see StationCall.Equals), and it is the very call a part was planned over that is bound.
    private static HashSet<StationCall> CallSet(IEnumerable<StationCall> calls) =>
        new(calls, ReferenceEqualityComparer.Instance);
}
