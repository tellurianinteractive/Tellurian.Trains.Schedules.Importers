namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Derives, for one station call, the notes generated from the vehicle schedules that begin or end a
/// part there: which locomotive to use, what to couple and uncouple, the moves to and from a parking
/// track, and what has to be done with the traction so the train can leave the other way.
/// </summary>
/// <remarks>
/// <para>
/// These notes are described on the <see cref="ScheduledTrainPart"/>, because it is the part's options
/// that say what happens — but they are <em>read</em> per call, in a driver's booklet or a station's
/// dispatch list. Nothing on a <see cref="StationCall"/> leads back to the schedules that work it, so
/// the plan holding them is passed in, exactly as the reader's sessions and the session formatting are.
/// </para>
/// <para>
/// A null plan yields nothing rather than throwing. A call is perfectly readable without one — that is
/// what a timetable printed before any vehicle has been scheduled is — and the notes are then simply
/// not among the things there are to say about it.
/// </para>
/// </remarks>
public static class VehicleCallNoteExtensions
{
    extension(StationCall call)
    {
        /// <summary>
        /// The notes the vehicle schedules generate at this call: the arrival notes of every part that
        /// <em>ends</em> here and the departure notes of every part that <em>begins</em> here.
        /// </summary>
        /// <remarks>
        /// Identical notes are collapsed. Two schedules can work the same part — a locomotive's and a
        /// wagonset's — and the notes that name no vehicle would otherwise be said once per schedule.
        /// The notes that do name one stay distinct, which is what double-headed traction wants.
        /// </remarks>
        /// <param name="plan">The plan whose vehicle schedules are read, or null when there is none.</param>
        public IEnumerable<ICallNote> VehicleNotes(Plan? plan)
        {
            call = call.ValueOrException(nameof(call));
            if (plan is null) return [];

            List<ICallNote> notes = [];
            foreach (var part in plan.Schedules.SelectMany(schedule => schedule.Parts))
            {
                // By reference, not by value: a train can hold two calls that compare equal, and a part
                // is bounded by the very call instances the train owns (see TrainExtensions.AsTrainPart).
                if (ReferenceEquals(part.To, call)) notes.AddRange(part.GeneratedArrivalNotes);
                if (ReferenceEquals(part.From, call)) notes.AddRange(part.GeneratedDepartureNotes);
            }
            return notes.Distinct();
        }
    }
}
