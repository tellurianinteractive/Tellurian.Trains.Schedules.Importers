namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Works out from the route where a train leaves in the opposite direction to the one it arrived from,
/// which is where a locomotive that cannot reverse as it stands has to be run round.
/// </summary>
/// <remarks>
/// Direction is taken relative to each track stretch's own Start and End, the same convention the
/// timings use. Nothing here asks what traction works the train: that is
/// <c>Plan.NeedsLocoRunaround</c> for a train and <c>ScheduledTrainPart.NeedsRunaround</c>
/// for a part.
/// </remarks>
public static class RunaroundDetection
{
    // Whether the train runs forward over the stretch between the two locations; null where there is none.
    private static bool? Forward(Plan plan, OperationLocation from, OperationLocation to) =>
        plan.Layout.StretchBetween(from, to) is { } stretch ? stretch.Start.Equals(from) : null;

    // The calls either side of this one in its train's run, matched by reference because a train can hold
    // two calls that compare equal. Null where the call is the origin or the destination.
    private static (StationCall? Before, StationCall? After) Neighbours(StationCall call)
    {
        if (call.Train is not { } train) return (null, null);
        var ordered = train.CallsInRunOrder;
        for (var i = 0; i < ordered.Count; i++)
        {
            if (!ReferenceEquals(ordered[i], call)) continue;
            return (i > 0 ? ordered[i - 1] : null, i < ordered.Count - 1 ? ordered[i + 1] : null);
        }
        return (null, null);
    }

    extension(StationCall call)
    {
        /// <summary>Whether the train runs forward into this call; null at its origin or where no stretch joins it to the call before.</summary>
        internal bool? ArrivesForward(Plan plan) =>
            Neighbours(call).Before is { } before ? Forward(plan, before.OperationLocation, call.OperationLocation) : null;

        /// <summary>Whether the train runs forward out of this call; null at its destination or where no stretch joins it to the call after.</summary>
        internal bool? LeavesForward(Plan plan) =>
            Neighbours(call).After is { } after ? Forward(plan, call.OperationLocation, after.OperationLocation) : null;

        /// <summary>
        /// Whether the train leaves this call in the opposite direction to the one it arrived from. False
        /// at its origin and its destination, and wherever the route is not whole.
        /// </summary>
        /// <param name="plan">The plan whose layout holds the track stretches.</param>
        public bool ReversesDirection(Plan plan) =>
            call.ArrivesForward(plan) is { } arrives && call.LeavesForward(plan) is { } leaves && arrives != leaves;
    }

    extension(ScheduledTrainPart part)
    {
        /// <summary>
        /// The part of the same schedule that follows this one where it ends, when it begins at the same
        /// location: the working the traction goes on to. Null when the traction is not worked on from there.
        /// </summary>
        internal ScheduledTrainPart? ContinuingPart
        {
            get
            {
                if (part.Schedule is not { } schedule) return null;
                var parts = schedule.OrderedParts;
                var index = -1;
                for (var i = 0; i < parts.Count; i++)
                    if (ReferenceEquals(parts[i], part)) { index = i; break; }
                if (index < 0 || index >= parts.Count - 1) return null;
                var next = parts[index + 1];
                return next.From.OperationLocation.Equals(part.To.OperationLocation) ? next : null;
            }
        }

        /// <summary>
        /// Whether the traction leaves the end of this part in the opposite direction to the one it arrived
        /// from: the train running on in its own, or the next part of the schedule — which may be another
        /// train — setting off the way it came. False where the traction is stabled, lifted off or
        /// exchanged there, or where nothing follows.
        /// </summary>
        public bool ReversesAtEnd
        {
            get
            {
                if (part.Schedule?.Plan is not { } plan) return false;
                if (part.ContinuingPart is not { } next) return false;
                if (part.TractionOptions?.ToLayover is TractionLayover.Stabling or TractionLayover.LiftedOff) return false;
                if (next.TractionOptions?.FromLayover is TractionLayover.Stabling or TractionLayover.LiftedOff) return false;
                return part.To.ArrivesForward(plan) is { } arrives && next.From.LeavesForward(plan) is { } leaves && arrives != leaves;
            }
        }
    }
}
