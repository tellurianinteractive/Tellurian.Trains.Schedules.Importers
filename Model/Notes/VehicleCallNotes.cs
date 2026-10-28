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
    // Where the call comes in the train's run, matched by reference because a train can hold two calls
    // that compare equal. A call the train does not hold is before everything (-1).
    private static int RunPosition(Train train, StationCall call)
    {
        var ordered = train.CallsInRunOrder;
        for (var i = 0; i < ordered.Count; i++)
            if (ReferenceEquals(ordered[i], call)) return i;
        return -1;
    }

    // A wagonset fetched from or put on another track is named, with its remark, by that note, so a separate
    // "in the train" remark beside it would say the same twice — and read as a contradiction, the wagonset being
    // in the train and on the track at once. The remark note stays for any session the track note leaves out.
    private static List<ICallNote> WithoutRemarksToldByTrackNotes(List<ICallNote> notes, Sessions trainRuns)
    {
        var told = notes.OfType<GeneratedNote>().Where(note => note is FromTrackNote or ToTrackNote).ToList();
        return [.. notes.Where(note => note is not WagonsetRemarkNote remark
            || !told.Any(track => remark.ScheduledObject.Equals(VehicleOf(track))
                && (track.Sessions ?? trainRuns).Includes(remark.Sessions ?? trainRuns)))];

        static ScheduledObject? VehicleOf(GeneratedNote note) => note switch
        {
            FromTrackNote(var vehicle, _) => vehicle,
            ToTrackNote(var vehicle, _) => vehicle,
            _ => null,
        };
    }

    // Notes that read the same but for the sessions they hold on — "Mon-Wed: fetch the train from
    // track 18" and "Thu-Sun: fetch the train from track 18" — are one note for the sessions of both,
    // and say nothing of sessions where those together are every one the train runs. The first of
    // them keeps its place.
    private static IEnumerable<ICallNote> SaidOncePerWording(IEnumerable<ICallNote> notes, Sessions trainRuns) =>
        notes
            .GroupBy(note => note is GeneratedNote generated
                ? (object)((generated with { Sessions = null }).ToText, generated.IsDriverNote, generated.IsStationNote,
                    generated.IsShuntingNote, generated.IsForArrival, generated.IsForDeparture, generated.DisplayOrder)
                : note)
            .Select(group => group.First() is GeneratedNote first && group.Count() > 1
                ? first with { Sessions = group.Cast<GeneratedNote>().Select(note => note.Sessions).SharedPartOf(trainRuns) }
                : group.First());

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
        /// <para>
        /// The loco driver's notes to shunt wagons name no wagonset either, but they can differ in the
        /// sessions they are for, when the schedules shunt their wagonsets on different ones. Each kind is
        /// said once, for every session any of the schedules shunts on.
        /// </para>
        /// </remarks>
        /// <param name="plan">The plan whose vehicle schedules are read, or null when there is none.</param>
        public IEnumerable<ICallNote> VehicleNotes(Plan? plan)
        {
            call = call.ValueOrException(nameof(call));
            // Pushing wagons is the call's own say-so and needs no schedule, so it is given without a plan.
            // Nothing departs from the end of the train's run, so nothing is pushed from there.
            var push = call.PushWagons && !call.IsTrainDestination ? call.PushNote(plan) : null;
            if (plan is null) return push is null ? [] : [push];

            List<ICallNote> notes = push is null ? [] : [push];
            foreach (var part in plan.Schedules.SelectMany(schedule => schedule.Parts))
            {
                // By reference, not by value: a train can hold two calls that compare equal, and a part
                // is bounded by the very call instances the train owns (see TrainExtensions.AsTrainPart).
                if (ReferenceEquals(part.To, call)) notes.AddRange(part.GeneratedArrivalNotes);
                if (ReferenceEquals(part.From, call)) notes.AddRange(part.GeneratedDepartureNotes);
            }
            notes.AddRange(call.WagonsetRemarkNotes(plan));
            notes = call.WithTurningAsked(plan, notes);
            if (call.Train is { } running) notes = WithoutRemarksToldByTrackNotes(notes, running.Sessions);
            if (call.Train is not { } train) return notes.Distinct();
            return
            [
                .. SaidOncePerWording(notes.Where(note => note is not ShuntWagonsNote).Distinct(), train.Sessions),
                .. ShuntWagonsNote.SaidOnce(notes.OfType<ShuntWagonsNote>(), train.Sessions),
            ];
        }

        /// <summary>
        /// The remarks of the wagonsets the train carries through this call: those of every part of a schedule that
        /// begins, ends or runs on past it, so the station sees them whether the wagonsets arrive, leave or merely
        /// stand in the train. Positions are by <c>CallsInRunOrder</c>, never by <c>Train.Calls</c>.
        /// </summary>
        private IEnumerable<ICallNote> WagonsetRemarkNotes(Plan plan)
        {
            if (call.Train is not { } train) return [];
            var here = RunPosition(train, call);
            return plan.Schedules.SelectMany(schedule => schedule.Parts)
                .Where(part => ReferenceEquals(part.From.Train, train)
                    && RunPosition(train, part.From) <= here && here <= RunPosition(train, part.To))
                .SelectMany(part => part.WagonsetRemarkNotes);
        }

        /// <summary>
        /// The departure note for pushing wagons from this call, naming where the wagons are bound: the
        /// destinations of the train's cargo flows that still have a way to go, otherwise the places the
        /// wagonsets' parts end — those parts that run on past this call. Where neither says anything the
        /// note is the bare instruction.
        /// </summary>
        /// <remarks>
        /// Positions are by <c>CallsInRunOrder</c>, never by <c>Train.Calls</c>, which is in insertion
        /// order. The wagonsets need the plan; the cargo flows belong to the train and do not.
        /// </remarks>
        private PushWagonsNote PushNote(Plan? plan)
        {
            var train = call.Train;
            if (train is null) return new PushWagonsNote { IsForDeparture = true };
            var here = RunPosition(train, call);

            var flows = train.CargoFlows
                .Where(flow => RunPosition(train, flow.To) > here && flow.HasStatedDestinations)
                .ToList();
            if (flows.Count > 0) return new PushWagonsNote(flows, []) { IsForDeparture = true };

            var places = (plan?.Schedules ?? []).SelectMany(schedule => schedule.Parts)
                .Where(part => ReferenceEquals(part.From.Train, train)
                    && RunPosition(train, part.From) <= here && RunPosition(train, part.To) > here
                    && part.WagonSets.Any())
                .Select(part => part.To.OperationLocation.Name)
                .Distinct()
                .ToList();
            return new PushWagonsNote([], places) { IsForDeparture = true };
        }

        /// <summary>
        /// Adds what the call itself asks for — <see cref="StationCall.TurnLoco"/> and
        /// <see cref="StationCall.Runaround"/> — to the turning notes already derived from the parts
        /// ending here, as the one note the two together give.
        /// </summary>
        /// <remarks>
        /// Unlike the part's options, the call's are not tied to where a schedule part ends, so they work
        /// anywhere in the train. They are dropped where every traction unit working the train reverses
        /// as it stands (a trainset, a push-pull train): there is then nothing to turn round or run round.
        /// <para>
        /// A runaround is worked out from the route unless the call says otherwise: where the train
        /// leaves in the opposite direction to the one it arrived from, and no part of a schedule with
        /// traction ends — where one does, it decides for itself, since the traction may be exchanged
        /// there. Whether the call is flagged as a stop does not matter: a train cannot reverse without
        /// standing, so a reversal on a call marked as a pass-through is still a runaround. <see cref="RunaroundMode.Never"/> takes the runaround away, a part's included.
        /// </para>
        /// </remarks>
        internal List<ICallNote> WithTurningAsked(Plan plan, List<ICallNote> notes)
        {
            if (call.Train is not { } train || !plan.NeedsLocoRunaround(train)) return notes;

            var fromParts = notes.Any(n => n is CirculateNote or TurnAndCirculateNote);
            var circulate = call.Runaround switch
            {
                RunaroundMode.Always => true,
                RunaroundMode.Never => false,
                _ => fromParts || (call.ReversesDirection(plan) && !call.EndsAPartWorkedByTraction(plan)),
            };
            if (!call.TurnLoco && circulate == fromParts) return notes;

            var turn = call.TurnLoco || notes.Any(n => n is TurnNote or TurnAndCirculateNote);
            GeneratedNote? merged = (turn, circulate) switch
            {
                (true, true) => new TurnAndCirculateNote { IsForArrival = true },
                (true, false) => new TurnNote { IsForArrival = true },
                (false, true) => new CirculateNote { IsForArrival = true },
                _ => null,
            };
            var rest = notes.Where(n => n is not (TurnNote or CirculateNote or TurnAndCirculateNote));
            return merged is null ? [.. rest] : [.. rest, merged];
        }

        // Whether a part of a schedule that has traction ends at this very call, by reference.
        private bool EndsAPartWorkedByTraction(Plan plan) =>
            plan.Schedules
                .Where(schedule => schedule.Vehicles.Any(vehicle => vehicle.IsTraction))
                .SelectMany(schedule => schedule.Parts)
                .Any(part => ReferenceEquals(part.To, call));
    }
}
