namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Which half of a call a manual note may be written for — and the repair that gives a note saved
/// without a half the one its call implies.
/// </summary>
/// <remarks>
/// <para>
/// A note is printed against one of a call's two events: the reports that show an arrival row and a
/// departure row take only the notes for that half (see the station dispatch list and the driver duty
/// timetable). A note belonging to neither is therefore printed nowhere the two are told apart, which
/// is why writing one is only offered where the train arrives or departs, and why the planner is asked
/// which of the two it is for.
/// </para>
/// <para>
/// The halves on offer are the call's own <see cref="StationCall.IsArrival"/> and
/// <see cref="StationCall.IsDeparture"/>, so what the Arr and Dep flags say is exactly what a note may
/// be written for. A pass-through offers neither: the train stands for nothing there, and the single
/// row a pass-through prints as shows whatever notes it already carries whole.
/// </para>
/// </remarks>
public static class ManualNoteRules
{
    extension(StationCall call)
    {
        /// <summary>
        /// The halves of this call a manual note may be written for: both where the train arrives and
        /// departs, one where it does only one of them, and none at a pass-through.
        /// </summary>
        public IReadOnlyList<CallNoteTarget> ManualNoteTargets
        {
            get
            {
                call = call.ValueOrException(nameof(call));
                // IsStop carries the location rule as well as the two flags: where the train cannot stop
                // at all, the flags are shown cleared and count for nothing (see StationCall.CanBeStop).
                if (!call.IsStop) return [];
                return (call.IsArrival, call.IsDeparture) switch
                {
                    (true, true) => [CallNoteTarget.Arrival, CallNoteTarget.Departure],
                    (true, _) => [CallNoteTarget.Arrival],
                    (_, true) => [CallNoteTarget.Departure],
                    _ => [],
                };
            }
        }

        /// <summary>
        /// Whether a manual note may be written at this call at all.
        /// </summary>
        public bool CanHaveManualNote => call.ManualNoteTargets.Count > 0;

        /// <summary>
        /// The half a note gets where the planner has not chosen one, or <c>null</c> at a call that may
        /// carry no manual note.
        /// </summary>
        /// <remarks>
        /// The departure wherever there is a choice: what is written at a stop is most often an
        /// instruction for leaving it, and it is the half the XPLN import stamps its own remarks with.
        /// </remarks>
        public CallNoteTarget? DefaultManualNoteTarget
        {
            get
            {
                var targets = call.ManualNoteTargets;
                if (targets.Contains(CallNoteTarget.Departure)) return CallNoteTarget.Departure;
                return targets.Count > 0 ? targets[0] : null;
            }
        }
    }

    extension(CallNote note)
    {
        /// <summary>
        /// The half of the call this note belongs to, or <c>null</c> when it belongs to neither — or to
        /// both, which no manual note does.
        /// </summary>
        public CallNoteTarget? Target =>
            (note.ValueOrException(nameof(note)).IsForArrival, note.IsForDeparture) switch
            {
                (true, false) => CallNoteTarget.Arrival,
                (false, true) => CallNoteTarget.Departure,
                _ => null,
            };

        /// <summary>
        /// Puts the note on one half of its call and takes it off the other, so it is never left
        /// claiming both.
        /// </summary>
        /// <param name="target">The half the note is for.</param>
        public void SetTarget(CallNoteTarget target)
        {
            note = note.ValueOrException(nameof(note));
            note.IsForArrival = target == CallNoteTarget.Arrival;
            note.IsForDeparture = target == CallNoteTarget.Departure;
        }
    }

    extension(Plan plan)
    {
        /// <summary>
        /// Gives every note saved without a half the one its call implies, and returns the number of
        /// notes changed so a caller can save only when something actually moved. Idempotent.
        /// </summary>
        /// <remarks>
        /// Notes were written before a note said which half it was for, and the XPLN import leaves a
        /// call remark that way still; unchanged, such a note is dropped from every report that tells
        /// the two halves apart. A pass-through is left alone: it has no half to be given, and the one
        /// row it prints as carries its notes whole anyway.
        /// </remarks>
        public int ApplyManualNoteTargetRules()
        {
            plan = plan.ValueOrException(nameof(plan));
            if (plan.Timetable is not { } timetable) return 0;
            var changed = 0;
            // The trains own the calls; the per-track index is derived from them (see
            // Timetable.RebuildStationCalls) and needs nothing done to it here.
            foreach (var call in timetable.Trains.SelectMany(train => train.Calls))
            {
                if (call.DefaultManualNoteTarget is not { } target) continue;
                foreach (var note in call.Notes.Where(note => !note.IsForArrival && !note.IsForDeparture))
                {
                    note.SetTarget(target);
                    changed++;
                }
            }
            return changed;
        }
    }
}
