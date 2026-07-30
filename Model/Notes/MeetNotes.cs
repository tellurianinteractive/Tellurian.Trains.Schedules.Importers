namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// One other train present at a call at the same time as the reader's own train.
/// </summary>
/// <param name="Other">The train being met.</param>
/// <param name="From">When both trains are first present together.</param>
/// <param name="To">When the first of them leaves.</param>
/// <param name="Sessions">The sessions both trains run, or <c>null</c> when that is every session the
/// reader's duty runs and the qualifier would be redundant.</param>
public sealed record Meet(Train Other, Time From, Time To, Sessions? Sessions);

/// <summary>
/// Every other train that passes the driver's train in the <em>opposite</em> direction at this
/// location, at most one per call: a call can be crossed by several trains at once, and they read as
/// one aggregated note rather than one row each.
/// </summary>
/// <param name="Meets">The trains being crossed, ordered by <see cref="Meet.From"/>, then
/// <see cref="Meet.To"/>, then the other train's number — the order the driver reads them in.</param>
/// <param name="Settings">How each meet's <see cref="Meet.Sessions"/> qualifier is rendered.</param>
public sealed record CrossingNote(
    IReadOnlyList<Meet> Meets,
    SessionsSettings? Settings) : GeneratedNote;

/// <summary>
/// Every other train the driver's train overtakes here — it was already standing when this train
/// arrived and is still standing when it leaves — at most one per call: see <see cref="CrossingNote"/>
/// for why these aggregate rather than repeat.
/// </summary>
/// <param name="Meets">The trains being overtaken, ordered by <see cref="Meet.From"/>, then
/// <see cref="Meet.To"/>, then the other train's number.</param>
/// <param name="Settings">How each meet's <see cref="Meet.Sessions"/> qualifier is rendered.</param>
public sealed record OvertakesNote(
    IReadOnlyList<Meet> Meets,
    SessionsSettings? Settings) : GeneratedNote;

/// <summary>
/// Every other train that overtakes the driver's train here — it arrives after this train and leaves
/// before it — at most one per call: see <see cref="CrossingNote"/> for why these aggregate rather
/// than repeat.
/// </summary>
/// <param name="Meets">The trains doing the overtaking, ordered by <see cref="Meet.From"/>, then
/// <see cref="Meet.To"/>, then the other train's number.</param>
/// <param name="Settings">How each meet's <see cref="Meet.Sessions"/> qualifier is rendered.</param>
public sealed record IsOvertakenNote(
    IReadOnlyList<Meet> Meets,
    SessionsSettings? Settings) : GeneratedNote;

/// <summary>
/// The train runs through this location without stopping.
/// </summary>
public sealed record NoStopNote : GeneratedNote;

/// <summary>
/// The train stands here — for a meet, a signal or a crossing — but nothing is picked up or set down.
/// </summary>
public sealed record NoExchangeNote : GeneratedNote;

/// <summary>
/// Which way a train runs along a <see cref="TrackStretch"/>.
/// </summary>
/// <remarks>
/// Nothing in the model states a train's direction, but a stretch has a <c>Start</c> and an <c>End</c>,
/// and the sense in which a train traverses it is exactly what tells a crossing from an overtaking.
/// </remarks>
public enum TravelDirection
{
    /// <summary>Along the stretch from its <c>Start</c> towards its <c>End</c>.</summary>
    Forward,

    /// <summary>Along the stretch from its <c>End</c> towards its <c>Start</c>.</summary>
    Backward,
}

/// <summary>
/// Derives the crossing and overtaking notes for a train's calls.
/// </summary>
/// <remarks>
/// <para>
/// The relative direction decides which question is asked. Trains running <em>towards</em> each other
/// cross whenever they overlap — each arrives before the other departs. Trains running the
/// <em>same</em> way only meet when one of them passes the other: the overtaken train must arrive
/// before the overtaking one and leave after it. Merely overlapping in the same direction is no event
/// at all — both simply stand here for a while — and produces no note. The same containment predicate
/// answers both same-direction cases with its arguments swapped, which keeps the pair consistent by
/// construction.
/// </para>
/// <para>
/// Comparing directions across <em>different</em> stretches only means anything because all track
/// stretches are expected to be defined in the same direction, which <c>Layout.DirectionInconsistencies</c>
/// already validates. A layout with inconsistent stretch orientation produces <em>wrong</em> crossing and
/// overtaking notes rather than missing ones, which makes that existing check a precondition of these
/// notes rather than a tidiness aid.
/// </para>
/// </remarks>
public static class MeetNoteExtensions
{
    extension(StationCall call)
    {
        /// <summary>
        /// The crossing and overtaking notes for this call: one per other train that crosses this one,
        /// or passes it, here — naming that train and the window in which both are present.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both attach to the arrival, because they answer <em>who am I meeting here</em> — which the
        /// driver asks on pulling in, not on leaving. A location whose
        /// <see cref="OperationLocation.HideMeets"/> is set produces neither: where such events are
        /// routine, one flag silences both at once.
        /// </para>
        /// <para>
        /// A train's origin and terminus produce neither either — neither its own, nor the other
        /// train's — and not merely because a meet there would be uninteresting. At those two calls one
        /// of the times is not a movement at all: the first call's arrival is when the driver reports,
        /// before the train departs, and the last call's departure is when they stand down, after it has
        /// arrived — the two times <c>DriverDuty</c> derives its start and end from. Testing presence
        /// across that span invents meets in time the train is not running: one arriving at 18:00 whose
        /// duty ends at 18:10 was met by everything that called there in between. A meet is a meet only
        /// while both trains are on their way.
        /// </para>
        /// </remarks>
        /// <param name="readerSessions">The sessions the reader's duty runs. A meet restricted to
        /// exactly these needs no session qualifier and gets none.</param>
        /// <param name="settings">How session qualifiers are rendered.</param>
        public IEnumerable<ICallNote> MeetNotes(Sessions readerSessions, SessionsSettings settings)
        {
            call = call.ValueOrException(nameof(call));
            if (call.OperationLocation.HideMeets) yield break;
            if (call.Train is not { } train) yield break;
            if (call.IsTrainOriginOrTerminus) yield break;

            var direction = call.DirectionInto;
            if (direction is null) yield break;

            var crossings = new List<Meet>();
            var overtakes = new List<Meet>();
            var isOvertaken = new List<Meet>();

            foreach (var other in call.OperationLocation.Calls())
            {
                if (other.Train is not { } otherTrain) continue;
                if (otherTrain.Equals(train)) continue;

                // The same rule read from the other side: a train that begins or ends its run here is
                // not running, so it can be neither crossed nor passed. One event has two readings, and
                // both must agree — otherwise a train would be told it overtook one that never moved.
                if (other.IsTrainOriginOrTerminus) continue;

                var otherDirection = other.DirectionInto;
                if (otherDirection is null) continue;

                // Which of the three events this is — or none, when two trains going the same way
                // merely share the location for a while without either passing the other.
                var meets =
                    otherDirection != direction ? Overlaps(call, other) ? crossings : null :
                    IsOvertaken(other, call) ? overtakes :
                    IsOvertaken(call, other) ? isOvertaken :
                    null;
                if (meets is null) continue;

                // The trains must also share at least one session to meet at all.
                var shared = train.Sessions.And(otherTrain.Sessions);
                if (shared.Flags == 0) continue;

                // The window in which the other train is actually there to be met — not either train's
                // own dwell, which is not what the driver needs to know.
                var from = Time.Max(call.Arrival, other.Arrival);
                var to = Time.Min(call.Departure, other.Departure);

                // A qualifier repeating the duty's own sessions says nothing, so it is omitted.
                var qualifier = shared.Flags == readerSessions.Flags ? (Sessions?)null : shared;

                meets.Add(new Meet(otherTrain, from, to, qualifier));
            }

            // At most one note per kind, aggregating every train met — not one row each — ordered the
            // way the driver reads down the page: by when a meet starts, then when it ends, then the
            // other train's number, which is deterministic even when two meets start and end together.
            if (crossings.Count > 0)
                yield return new CrossingNote(Order(crossings), settings) { IsForArrival = true };
            if (overtakes.Count > 0)
                yield return new OvertakesNote(Order(overtakes), settings) { IsForArrival = true };
            if (isOvertaken.Count > 0)
                yield return new IsOvertakenNote(Order(isOvertaken), settings) { IsForArrival = true };
        }

        /// <summary>
        /// Whether this call is where its train begins or ends its run.
        /// </summary>
        /// <remarks>
        /// Compared by reference, not by value: a train may hold two calls that compare equal, and only
        /// identity says which of them this one is.
        /// </remarks>
        public bool IsTrainOriginOrTerminus
        {
            get
            {
                call = call.ValueOrException(nameof(call));
                if (call.Train is not { Calls.Count: > 0 } train) return false;
                var calls = train.Calls;
                return ReferenceEquals(calls[0], call) || ReferenceEquals(calls[^1], call);
            }
        }

        /// <summary>
        /// The sense in which the train runs along the stretch it <em>arrives</em> on, or <c>null</c>
        /// when no stretch connects this call to its neighbours.
        /// </summary>
        /// <remarks>
        /// Taking the inbound stretch gives one unambiguous answer even where a train reverses at a
        /// station that permits it — and it is the right one, since what the driver meets is the train
        /// that came towards them, whatever it does afterwards. A train that originates here has no
        /// inbound stretch, so its outbound one is used: its direction of travel is still well defined
        /// by where it is going.
        /// </remarks>
        public TravelDirection? DirectionInto
        {
            get
            {
                call = call.ValueOrException(nameof(call));
                if (call.Train is not { } train) return null;
                var calls = train.Calls;
                var index = calls.IndexOf(call);
                if (index < 0) return null;

                if (index > 0 && DirectionBetween(calls[index - 1], call) is { } inbound) return inbound;
                if (index < calls.Count - 1 && DirectionBetween(call, calls[index + 1]) is { } outbound) return outbound;
                return null;
            }
        }
    }

    // Two trains occupy a location together when each arrives before the other departs.
    private static bool Overlaps(StationCall mine, StationCall other) =>
        other.Arrival < mine.Departure && other.Departure > mine.Arrival;

    // One train passes another going the same way only when it arrives after the other and leaves
    // before it: the overtaken train is standing here for the whole of the overtaking train's visit,
    // which is what lets the second get ahead of the first. Equal times at either end are not enough —
    // then neither is unambiguously in front — and share the fate of a plain overlap: no note.
    private static bool IsOvertaken(StationCall overtaken, StationCall overtaker) =>
        overtaken.Arrival < overtaker.Arrival && overtaken.Departure > overtaker.Departure;

    // The order the driver reads a group of simultaneous meets in: by start, then end, then the other
    // train's number, so the order is deterministic even when two meets start and end together.
    private static IReadOnlyList<Meet> Order(List<Meet> meets) =>
        [.. meets.OrderBy(m => m.From).ThenBy(m => m.To).ThenBy(m => m.Other.Number)];

    // Which way a train runs over the stretch joining two consecutive calls.
    private static TravelDirection? DirectionBetween(StationCall from, StationCall to)
    {
        var layout = from.OperationLocation.Layout;
        if (layout is null) return null;
        var (start, end) = (from.OperationLocation, to.OperationLocation);
        foreach (var stretch in layout.TrackStretches)
        {
            if (stretch.Start.Equals(start) && stretch.End.Equals(end)) return TravelDirection.Forward;
            if (stretch.Start.Equals(end) && stretch.End.Equals(start)) return TravelDirection.Backward;
        }
        return null;
    }
}
