namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

/// <summary>Which clearance a dispatch-list row stands for.</summary>
public enum DispatchRowKind
{
    /// <summary>A train pulling in, cleared into the station.</summary>
    Arrival,

    /// <summary>A train leaving, cleared on to the next station.</summary>
    Departure,

    /// <summary>
    /// A train running past without standing: one moment, and so one clearance rather than two.
    /// </summary>
    PassThrough,
}

/// <summary>
/// One line of a station's dispatch list: a single clearance the person on duty gives, with everything
/// they need to give it.
/// </summary>
/// <remarks>
/// A standing train produces two rows, one per clearance, because clearing a train <em>in</em> and
/// clearing it <em>onward</em> are separate actions taken minutes apart — and between them other trains
/// may pass, which is why the list is ordered by time rather than by train.
/// </remarks>
public sealed class DispatchRow
{
    /// <summary>The call this row is a clearance of.</summary>
    public required StationCall Call { get; init; }

    /// <summary>Which clearance the row stands for.</summary>
    public required DispatchRowKind Kind { get; init; }

    /// <summary>The time the clearance is given: the arrival on an arrival row, the departure otherwise.</summary>
    public required Time Time { get; init; }

    /// <summary>
    /// The notes for this row, in <c>DisplayOrder</c>. Never reordered: note order carries meaning.
    /// </summary>
    public IReadOnlyList<ICallNote> Notes { get; init; } = [];

    /// <summary>The train, named as it is announced — company signature, category prefix and number.</summary>
    public string TrainIdentity => Call.Train.Identity;

    /// <summary>The sessions or days the train operates.</summary>
    public Sessions Sessions => Call.Train.Sessions;

    /// <summary>
    /// The plain-text form of the sessions as the column shows them — without the on-demand marker,
    /// which is carried as a note instead. Kept so the page-height estimate charges for exactly what is
    /// printed; the cell wraps rather than truncate, so an over-long value costs lines rather than
    /// silently losing its tail.
    /// </summary>
    public required string SessionsText { get; init; }

    /// <summary>The sessions as the column shows them; see <see cref="SessionsText"/>.</summary>
    public Sessions DisplayedSessions => Sessions.WithoutOnDemand;

    /// <summary>The track the train occupies here.</summary>
    public string TrackNumber => Call.Track.Number;

    /* What the four time and place cells hold is a property of the CALL, not of which clearance the row
       stands for: every row states when the train got here and where from, and when it leaves and where
       to. Each row is then self-contained, which matters because a standing train's two rows are not
       adjacent — other trains fall between them in time order — and because a reader scanning the Arr
       column for arrivals must find every train that arrives, including the ones that only pass through.
       Which clearance a row is for is carried by the emphasis instead; see IsSoleRow. */

    /// <summary>The arrival time, or <c>null</c> where the train did not arrive here.</summary>
    /// <remarks>
    /// Blank at the train's origin: the arrival recorded on that call is when preparing the train begins,
    /// and showing it under <em>Arr</em> would state a movement that never happened.
    /// </remarks>
    public string? ArrivalTime => Call.IsTrainOrigin ? null : Call.Arrival.HHMM();

    /// <summary>The departure time, or <c>null</c> where the train does not go on from here.</summary>
    /// <remarks>
    /// Blank at the train's destination, where the departure recorded on the call is the time the train
    /// is finished up rather than a departure anybody clears.
    /// </remarks>
    public string? DepartureTime => Call.IsTrainDestination ? null : Call.Departure.HHMM();

    /// <summary>Where the train started its run, or <c>null</c> where it started here.</summary>
    public string? OriginName =>
        Call.IsTrainOrigin ? null : NameOf(Call.Train.CallsInRunOrder.FirstOrDefault());

    /// <summary>Where the train ends its run, or <c>null</c> where it ends here.</summary>
    public string? DestinationName =>
        Call.IsTrainDestination ? null : NameOf(Call.Train.CallsInRunOrder.LastOrDefault());

    /// <summary>
    /// True when this is the train's only row here, so it stands for everything done with that train at
    /// this station rather than for one half of it.
    /// </summary>
    /// <remarks>
    /// Both pairs are then emphasised, because both are what the reader acts on. A row that is one of a
    /// pair emphasises only its own clearance: the other pair is there for reference, and emphasising it
    /// too would leave nothing saying which of the two this row is.
    /// </remarks>
    public required bool IsSoleRow { get; init; }

    /// <summary>The notes that actually put something on the page.</summary>
    /// <remarks>
    /// A note rendering to nothing still renders an empty element, which costs a blank line the reader
    /// cannot see but the page still pays for. Filtered once, here, so the rendering and the page-height
    /// estimate count the same notes.
    /// </remarks>
    public IReadOnlyList<ICallNote> PrintingNotes => [.. Notes.Where(note => note.ToText.HasValue)];

    private static string? NameOf(StationCall? call) => call?.OperationLocation.Name;

    /// <summary>
    /// Builds the rows one train contributes to one station's dispatch list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// How many rows a call produces is decided by the <em>times</em>, not by whether the train stops:
    /// what the list schedules is the dispatcher's clearances, and a train standing at the platform needs
    /// two of those whether or not anyone gets on. A call whose arrival equals its departure is a single
    /// moment and gets one row; the train's first call has no arrival to clear and its last no departure.
    /// </para>
    /// <para>
    /// Trains running past are included, and deliberately regardless of the location's <c>HidePassings</c>
    /// setting: that flag suppresses passings where they are noise, but a dispatcher clears every train
    /// through their station, so on this list a missing one is a train nobody is expecting.
    /// </para>
    /// </remarks>
    /// <param name="train">The train to take calls from.</param>
    /// <param name="station">The station the list is for.</param>
    /// <param name="settings">How sessions are rendered inside notes.</param>
    public static IEnumerable<DispatchRow> Build(Train train, OperationLocation station, SessionsSettings settings)
    {
        train = train.ValueOrException(nameof(train));
        station = station.ValueOrException(nameof(station));

        var calls = train.CallsInRunOrder;
        for (var i = 0; i < calls.Count; i++)
        {
            var call = calls[i];
            if (!call.OperationLocation.Equals(station)) continue;

            // The reader is the station, present at every session; what needs qualifying is when a meet
            // does not happen on all the sessions this train runs, so the train's own sessions are the
            // context — the row states one train's working.
            var notes = call.StationNotes(train.Sessions, settings);

            // Without the on-demand marker: that is stated as a note, and saying it in both places
            // would spend four lines of a narrow column repeating what the notes already say.
            var sessionsText = train.Sessions.WithoutOnDemand.ToText(settings);

            if (i == 0)
            {
                yield return Row(call, DispatchRowKind.Departure, call.Departure, notes, sessionsText, sole: true);
                continue;
            }
            if (i == calls.Count - 1)
            {
                yield return Row(call, DispatchRowKind.Arrival, call.Arrival, notes, sessionsText, sole: true);
                continue;
            }
            if (call.Arrival.Equals(call.Departure))
            {
                // One row cannot hold the arrival/departure split, so both halves' notes appear on it.
                // Without this a note classified for the missing half would be silently dropped — most
                // damagingly for the notes that only ever occur on a train running past.
                yield return Row(call, DispatchRowKind.PassThrough, call.Departure, notes, sessionsText, sole: true);
                continue;
            }

            yield return Row(call, DispatchRowKind.Arrival, call.Arrival, notes, sessionsText, sole: false);
            yield return Row(call, DispatchRowKind.Departure, call.Departure, notes, sessionsText, sole: false);
        }
    }

    private static DispatchRow Row(
        StationCall call, DispatchRowKind kind, Time time, IReadOnlyList<ICallNote> notes,
        string sessionsText, bool sole) =>
        new()
        {
            Call = call,
            Kind = kind,
            Time = time,
            SessionsText = sessionsText,
            IsSoleRow = sole,
            Notes = kind switch
            {
                DispatchRowKind.Arrival => [.. notes.Where(note => note.IsForArrival)],
                DispatchRowKind.Departure => [.. notes.Where(note => note.IsForDeparture)],
                _ => notes,
            },
        };
}
