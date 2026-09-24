namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note telling the loco driver to shunt the wagonsets between the train's track and the other track they
/// stand on, where a vehicle schedule fetches them from or puts them on one. The loco driver's
/// counterpart of <see cref="FromTrackNote"/> and <see cref="ToTrackNote"/> for wagonsets.
/// </summary>
/// <remarks>
/// <para>
/// It names neither the wagonsets nor the track. The wagonset block of the driver duty booklet lists both,
/// with the tracks each wagonset is fetched from and left on, so the timetable says only that there is
/// shunting to do. The dispatcher has no such block and still reads a <see cref="FromTrackNote"/> or
/// <see cref="ToTrackNote"/> naming each wagonset and its track.
/// </para>
/// <para>
/// Naming nothing, it is one note for all the wagonsets shunted at the call, whichever schedules they
/// work: see <c>VehicleCallNoteExtensions.VehicleNotes</c>, which says it once for them all.
/// </para>
/// <para>
/// Its <see cref="GeneratedNote.Sessions"/> are those any of the wagonsets is shunted on, taken together;
/// see <see cref="SaidOnce"/>.
/// </para>
/// </remarks>
public abstract record ShuntWagonsNote : GeneratedNote
{
    /// <summary>
    /// The one note these notes of the same kind, for a call of a train running
    /// <paramref name="trainRuns"/>, say together: for every session any of them is shunted on.
    /// </summary>
    internal static IEnumerable<ShuntWagonsNote> SaidOnce(IEnumerable<ShuntWagonsNote> notes, Sessions trainRuns) =>
        notes
            .GroupBy(note => note.GetType())
            .Select(kind => kind.First() with { Sessions = kind.Select(note => note.Sessions).SharedPartOf(trainRuns) });
}

/// <summary>
/// Note telling the loco driver to shunt the wagonsets from the other track they stand on to the train's
/// departure track before the train departs. Derived from <see cref="ScheduledTrainPart.FromTrack"/>.
/// </summary>
public sealed record ShuntWagonsToDepartureTrackNote : ShuntWagonsNote;

/// <summary>
/// Note telling the loco driver to shunt the wagonsets from the train's arrival track to the other track
/// they are left on after the train has arrived. Derived from <see cref="ScheduledTrainPart.ToTrack"/>.
/// </summary>
public sealed record ShuntWagonsToArrivalTrackNote : ShuntWagonsNote;
