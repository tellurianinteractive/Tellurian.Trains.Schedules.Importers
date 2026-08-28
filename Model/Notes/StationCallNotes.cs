namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Assembles the notes the person on duty at a station reads for one call: the persisted and generated
/// notes already on the call, the two derived from whether the train stops, the lock keys handed out and
/// taken back here, and the crossings and overtakings.
/// </summary>
/// <remarks>
/// The same families as <c>DriverNotes</c>, filtered by <see cref="ICallNote.IsStationNote"/> instead of
/// <see cref="ICallNote.IsDriverNote"/>. The two audiences overlap heavily but are not the same: a note
/// written for the driver alone must not reach the station's list, and a note about handing a key over
/// concerns whoever hands it over as much as whoever collects it.
/// </remarks>
public static class StationCallNoteExtensions
{
    extension(StationCall call)
    {
        /// <summary>
        /// The notes shown to the station's dispatcher at this call, sorted by
        /// <see cref="ICallNote.DisplayOrder"/>.
        /// </summary>
        /// <param name="readerSessions">The sessions the reader has this call in view for, used to decide
        /// whether a meet needs a session qualifier. On a station dispatch list that is the sessions of the
        /// train the row belongs to, because a row states one train's working.</param>
        /// <param name="settings">How session qualifiers are rendered.</param>
        /// <param name="plan">The plan whose vehicle schedules say what to do with the vehicles here.
        /// Omit it where there is none; the vehicle notes are then simply absent (see
        /// <c>VehicleCallNoteExtensions</c>).</param>
        public IReadOnlyList<ICallNote> StationNotes(Sessions readerSessions, SessionsSettings settings, Plan? plan = null)
        {
            call = call.ValueOrException(nameof(call));
            return
            [
                .. call.Notes
                    .Cast<ICallNote>()
                    .Concat(call.StopNotes)
                    .Concat(call.OnDemandNotes)
                    .Concat(call.LockKeyNotes)
                    .Concat(call.MeetNotes(readerSessions, settings))
                    .Concat(call.VehicleNotes(plan))
                    .Concat(call.CargoFlowNotes)
                    .Where(note => note.IsStationNote)
                    .OrderBy(note => note.DisplayOrder)
            ];
        }

        /// <summary>
        /// The note saying the train runs only when it is called for, where its sessions say so.
        /// </summary>
        /// <remarks>
        /// Derived from the train rather than the call, so it repeats on every row that train has here.
        /// That is deliberate: each row of a dispatch list is read on its own, and whether the train runs
        /// at all is not something to learn from the row above. It sorts before the other notes for the
        /// same reason — it qualifies everything else said about the train.
        /// </remarks>
        public IEnumerable<ICallNote> OnDemandNotes
        {
            get
            {
                call = call.ValueOrException(nameof(call));
                if (call.Train?.Sessions.IsOnDemand != true) yield break;
                yield return new OnDemandNote { IsForArrival = true, IsForDeparture = true, DisplayOrder = 50 };
            }
        }
    }
}
