namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// A joint in a vehicle's working: the point between two consecutive parts of a <see cref="Schedule"/>,
/// before its first part, or after its last. A joint is where the vehicle stands between two trains, and
/// therefore where another train can be worked in.
/// </summary>
/// <remarks>
/// A joint is derived from the schedule as it stands (see <c>Schedule.Joints</c>) and is
/// never stored: it is the handle an editor uses to say <em>where</em> a part is to be inserted, since a
/// schedule's parts are held in an unordered collection and ordered by departure.
/// </remarks>
/// <param name="Previous">The part worked before the joint, or <c>null</c> at the start of the working.</param>
/// <param name="Next">The part worked after the joint, or <c>null</c> at the end of the working.</param>
public sealed record ScheduleJoint(ScheduledTrainPart? Previous, ScheduledTrainPart? Next);

/// <summary>
/// Provides extension members for <see cref="ScheduleJoint"/>.
/// </summary>
public static class ScheduleJointExtensions
{
    extension(ScheduleJoint joint)
    {
        /// <summary>True at the start of the working, where a part can only be prepended.</summary>
        public bool IsStart => joint.Previous is null;

        /// <summary>True at the end of the working, where a part is appended.</summary>
        public bool IsEnd => joint.Next is null;

        /// <summary>The location the vehicle arrives at, or <c>null</c> at the start of the working.</summary>
        public OperationLocation? From => joint.Previous is { } previous ? previous.To.OperationLocation : null;

        /// <summary>The location the vehicle leaves from next, or <c>null</c> at the end of the working.</summary>
        public OperationLocation? To => joint.Next is { } next ? next.From.OperationLocation : null;

        /// <summary>
        /// The time from which the vehicle is free, or <c>null</c> at the start of the working (where it is
        /// free from the beginning of the day). It is the end of the previous part's <c>WorkingSpan</c>, so
        /// the finishing-up time at a train's destination counts as still occupied.
        /// </summary>
        public Time? WindowStart => joint.Previous is { } previous ? previous.WorkingSpan.To : null;

        /// <summary>
        /// The time by which the vehicle must be free again, or <c>null</c> at the end of the working. It is
        /// the start of the next part's <c>WorkingSpan</c>, so the preparation time at a train's origin
        /// counts as already occupied.
        /// </summary>
        public Time? WindowEnd => joint.Next is { } next ? next.WorkingSpan.From : null;

        /// <summary>
        /// How long the vehicle stands at this joint, or <c>null</c> when the working is open at either end
        /// or the parts meet without any time to spare.
        /// </summary>
        public Time? Layover =>
            joint.WindowStart is { } start && joint.WindowEnd is { } end && end > start ? end.Subtract(start) : null;

        /// <summary>
        /// True when the working is broken here: the vehicle arrives at one location and the next part
        /// leaves from another. Such a joint is reported by the contiguity validation (rule S2) and is
        /// filled by working in the train (or trains) that bridge it.
        /// </summary>
        public bool IsBroken =>
            joint.From is { } from && joint.To is { } to && !from.Equals(to);

        /// <summary>
        /// True when there is somewhere to work a train in: at either open end, at a broken joint, or where
        /// the vehicle has time to spare. A joint whose parts meet exactly has no room for anything.
        /// </summary>
        public bool HasRoom =>
            joint.IsStart || joint.IsEnd || joint.IsBroken || joint.Layover is not null;

        /// <summary>
        /// Finds the run of the given train that best fills this joint, as indices into the train's
        /// <c>CallsInRunOrder</c> — the order it works its calls — or <c>null</c> when the train has no run
        /// that fits.
        /// </summary>
        /// <remarks>
        /// A run fits when the vehicle is free for the whole of it — it starts no earlier than the vehicle
        /// comes free and ends no later than it is needed again — and when it touches the joint: it must
        /// leave from where the vehicle stands, except at the start of a working or at a broken joint, where
        /// a run arriving where the working continues is what is wanted instead. Of the fitting runs, one
        /// that both leaves from and arrives at the joint is preferred (it closes the working in one), then
        /// the earliest departure, then the longest run.
        /// </remarks>
        /// <param name="train">The train to fit into the joint.</param>
        public (int From, int To)? FittingCallsFor(Train train)
        {
            train = train.ValueOrException(nameof(train));
            var calls = train.CallsInRunOrder;
            var from = joint.From;
            var to = joint.To;
            // The vehicle stands at one place, so a run has to leave from there. Only where the working has
            // no part before this joint, or is broken across it, is a run that merely arrives of any use.
            var acceptsArrivalOnly = joint.IsStart || joint.IsBroken;

            // A shunting task has one call, which is both where the vehicle takes the task up and where it
            // is free again, so its only run is that call to itself. It fits when the task stands where the
            // vehicle does and falls wholly inside the time the vehicle is free.
            if (train.IsShuntingTask && calls.Count == 1)
            {
                var only = calls[0];
                if (joint.WindowStart is { } free && only.WorkStart < free) return null;
                if (joint.WindowEnd is { } needed && only.WorkEnd > needed) return null;
                var atJoint =
                    (from is { } leaves && only.OperationLocation.Equals(leaves)) ||
                    (acceptsArrivalOnly && to is { } arrives && only.OperationLocation.Equals(arrives));
                return atJoint ? (0, 0) : null;
            }

            (int From, int To)? best = null;
            var bestBridges = false;
            for (var i = 0; i < calls.Count - 1; i++)
            {
                if (joint.WindowStart is { } start && calls[i].WorkStart < start) continue;
                var leavesJoint = from is { } f && calls[i].OperationLocation.Equals(f);
                for (var j = i + 1; j < calls.Count; j++)
                {
                    if (joint.WindowEnd is { } end && calls[j].WorkEnd > end) continue;
                    var arrivesJoint = to is { } t && calls[j].OperationLocation.Equals(t);
                    if (!leavesJoint && !(acceptsArrivalOnly && arrivesJoint)) continue;
                    var bridges = leavesJoint && arrivesJoint;
                    if (best is not { } current || IsBetter((i, j), bridges, current, bestBridges))
                    {
                        best = (i, j);
                        bestBridges = bridges;
                    }
                }
            }
            return best;
        }
    }

    // Ranks two fitting runs: one that bridges the joint outright beats one that does not, then the earlier
    // departure (the vehicle takes up work as soon as it is free), then the longer run.
    private static bool IsBetter((int From, int To) candidate, bool bridges, (int From, int To) current, bool currentBridges) =>
        bridges != currentBridges ? bridges
        : candidate.From != current.From ? candidate.From < current.From
        : candidate.To > current.To;
}
