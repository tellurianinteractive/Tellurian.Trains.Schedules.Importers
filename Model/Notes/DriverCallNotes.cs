namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Assembles the notes a loco driver reads at one call: the persisted and generated notes already on the
/// call, the two derived from whether the train stops, and the crossings and overtakings.
/// </summary>
public static class DriverCallNoteExtensions
{
    extension(StationCall call)
    {
        /// <summary>
        /// The notes shown to the driver at this call, sorted by <see cref="ICallNote.DisplayOrder"/>.
        /// </summary>
        /// <remarks>
        /// Filtered by <see cref="ICallNote.IsDriverNote"/>: the same call carries notes for station
        /// staff and shunters that must not appear in a driver's booklet. Both note families are
        /// included, so a note a planner typed and one the model derived combine into a single list.
        /// </remarks>
        /// <param name="readerSessions">The sessions the reader's duty runs, used to decide whether a
        /// meet needs a session qualifier.</param>
        /// <param name="settings">How session qualifiers are rendered.</param>
        public IReadOnlyList<ICallNote> DriverNotes(Sessions readerSessions, SessionsSettings settings)
        {
            call = call.ValueOrException(nameof(call));
            return
            [
                .. call.Notes
                    .Cast<ICallNote>()
                    .Concat(call.StopNotes)
                    .Concat(call.MeetNotes(readerSessions, settings))
                    .Where(note => note.IsDriverNote)
                    .OrderBy(note => note.DisplayOrder)
            ];
        }

        /// <summary>
        /// The notes derived from <see cref="StationCall.IsStop"/> combined with the times.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The two facts are both needed: the times say whether the train stands, and <c>IsStop</c> says
        /// whether standing means work. A train may stand three minutes for a meet yet exchange nothing,
        /// which reads <em>No exchange</em> rather than <em>No stop</em>.
        /// </para>
        /// <para>
        /// An ordinary working stop produces neither — only the <em>absence</em> of a stop needs saying.
        /// A stop with equal times is a real operating case, a stop too brief to schedule a dwell for,
        /// and it also produces nothing.
        /// </para>
        /// </remarks>
        public IEnumerable<ICallNote> StopNotes
        {
            get
            {
                call = call.ValueOrException(nameof(call));
                if (call.IsStop) yield break;
                yield return call.Arrival.Equals(call.Departure)
                    // A pass-through renders as a single row showing the departure time.
                    ? new NoStopNote { IsForDeparture = true, DisplayOrder = 100 }
                    // Answers the question the driver asks on pulling in: do I have work here?
                    : new NoExchangeNote { IsForArrival = true, DisplayOrder = 100 };
            }
        }
    }
}
