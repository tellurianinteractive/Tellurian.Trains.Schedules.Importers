using Tellurian.Trains.Schedules.Model.Dispatching;

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

    /// <summary>
    /// A shunting task: work at the station over a span of time rather than a movement through it, so
    /// one row at the time the work starts, carrying every note of the call.
    /// </summary>
    ShuntingTask,
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
    /// <remarks>
    /// The signature is the one the train actually runs under: its own company where it has one, otherwise
    /// its category's. A layout with a single operator names no company anywhere and the column then reads
    /// as it always did, so the signature appears exactly where it distinguishes one operator's train from
    /// another's.
    /// </remarks>
    public string TrainIdentity => Call.Train.IdentityWithCompany;

    /// <summary>
    /// The clock time of the clearance, without the day it falls on. The list is worked through by the
    /// clock, so this is what orders it: a clearance after midnight sorts among the early hours.
    /// </summary>
    public Time TimeOfDay => Time.FromHourAndMinute(Time.Value.Hours, Time.Value.Minutes);

    /// <summary>
    /// The sessions or days the clearance is given on. A clearance after midnight belongs to the session
    /// or day after the one the train started on, so these are the train's own moved on by the days its
    /// time has passed.
    /// </summary>
    public required Sessions Sessions { get; init; }

    /// <summary>
    /// The plain-text form of the sessions as the column shows them — without the on-demand marker,
    /// which is carried as a note instead. Kept so the page-height estimate charges for exactly what is
    /// printed; the cell wraps rather than truncate, so an over-long value costs lines rather than
    /// silently losing its tail.
    /// </summary>
    public required string SessionsText { get; init; }

    /// <summary>The sessions as the column shows them; see <see cref="SessionsText"/>.</summary>
    public Sessions DisplayedSessions => Sessions.WithoutOnDemand;

    /// <summary>
    /// Whether the call takes its track at all. A shunting task with no traction unit assigned does not:
    /// it is worked over the whole station, so the track it names means nothing and is left out.
    /// </summary>
    public bool OccupiesTrack { get; init; } = true;

    /// <summary>The track the train occupies here; empty where it occupies none.</summary>
    public string TrackNumber => OccupiesTrack ? Call.Track.Number : string.Empty;

    /// <summary>
    /// True when the call is at a location the list's dispatcher works from afar rather than at the
    /// station itself.
    /// </summary>
    public bool IsRemote { get; init; }

    /// <summary>
    /// The track as the column shows it: prefixed with the location's signature on a remote row, so
    /// a clearance at the junction is never taken for one at the station — "Bj 2".
    /// </summary>
    public string TrackText => IsRemote && OccupiesTrack ? $"{Call.OperationLocation.Signature} {TrackNumber}" : TrackNumber;

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

    /// <summary>
    /// True when the first session or day is over before this clearance could be worked: the meeting
    /// begins later than the operating window does, and either the train started before it began or the
    /// clearance falls after midnight, on the first session only because the period wraps round.
    /// </summary>
    /// <remarks>The first session's tick box is then greyed, as for a session the train does not run.</remarks>
    public bool IsOutsideFirstSession { get; init; }

    /// <summary>The notes that actually put something on the page.</summary>
    /// <remarks>
    /// A note rendering to nothing still renders an empty element, which costs a blank line the reader
    /// cannot see but the page still pays for. Filtered once, here, so the rendering and the page-height
    /// estimate count the same notes.
    /// </remarks>
    public IReadOnlyList<ICallNote> PrintingNotes => [.. Notes.Where(note => note.ToText.HasValue)];

    private static string? NameOf(StationCall? call) => call?.OperationLocation.Name;

    /// <summary>
    /// Divides a pass-through into its two clearances: the arrival, notes for arriving only, and the
    /// departure, notes for departing only. They are the same moment but are given to different neighbours
    /// when the train runs on to a different direction than it came from, so a list divided by direction
    /// cannot hold the one row both ways. Neither is the train's only row any more.
    /// </summary>
    /// <returns>The arrival row and the departure row.</returns>
    public (DispatchRow Arrival, DispatchRow Departure) SplitPassThrough() =>
        (CopyAs(DispatchRowKind.Arrival, note => note.IsForArrival),
         CopyAs(DispatchRowKind.Departure, note => note.IsForDeparture));

    private DispatchRow CopyAs(DispatchRowKind kind, Func<ICallNote, bool> keepNote) => new()
    {
        Call = Call,
        Kind = kind,
        Time = Time,
        Sessions = Sessions,
        SessionsText = SessionsText,
        IsSoleRow = false,
        IsOutsideFirstSession = IsOutsideFirstSession,
        IsRemote = IsRemote,
        OccupiesTrack = OccupiesTrack,
        Notes = [.. Notes.Where(keepNote)],
    };

    /// <summary>
    /// Builds the rows one train contributes to one station's dispatch list: one per clearance
    /// (see <c>ClearancesAt</c> in the model, which holds the rules).
    /// </summary>
    /// <param name="train">The train to take calls from.</param>
    /// <param name="station">The station the list is for.</param>
    /// <param name="settings">How sessions are rendered inside notes.</param>
    /// <param name="plan">The plan whose vehicle schedules say what is done with the vehicles here, and whose
    /// driver duties decide which trains run in the first session. Omit it and the rows carry no vehicle
    /// instructions.</param>
    /// <param name="isRemote">Whether <paramref name="station"/> is worked from afar by the dispatcher
    /// the list is for; see <see cref="IsRemote"/>.</param>
    public static IEnumerable<DispatchRow> Build(
        Train train, OperationLocation station, SessionsSettings settings, Plan? plan = null, bool isRemote = false)
    {
        train = train.ValueOrException(nameof(train));
        station = station.ValueOrException(nameof(station));
        return train.ClearancesAt(station, settings, plan, isRemote).Select(clearance => FromClearance(clearance, settings, plan));
    }

    /// <summary>
    /// The row for one clearance, with the notes and track that go with it.
    /// </summary>
    /// <param name="clearance">The clearance the row stands for.</param>
    /// <param name="settings">How sessions are rendered inside notes.</param>
    /// <param name="plan">See <see cref="Build"/>.</param>
    public static DispatchRow FromClearance(Clearance clearance, SessionsSettings settings, Plan? plan = null)
    {
        var call = clearance.Call;
        // The reader is the station, present at every session; what needs qualifying is when a meet
        // does not happen on all the sessions this train runs, so the train's own sessions are the
        // context — the row states one train's working.
        var notes = call.StationNotes(call.Train.Sessions, settings, plan);
        var kind = clearance.Kind switch
        {
            ClearanceKind.Arrival => DispatchRowKind.Arrival,
            ClearanceKind.Departure => DispatchRowKind.Departure,
            ClearanceKind.PassThrough => DispatchRowKind.PassThrough,
            _ => DispatchRowKind.ShuntingTask,
        };
        return new()
        {
            IsOutsideFirstSession = clearance.IsOutsideFirstSession,
            IsRemote = clearance.IsRemote,
            OccupiesTrack = plan is null || call.OccupiesTrack(plan.Schedules),
            Call = call,
            Kind = kind,
            Time = clearance.Time,
            Sessions = clearance.Sessions,
            // Without the on-demand marker: that is stated as a note, and saying it in both places
            // would spend four lines of a narrow column repeating what the notes already say.
            SessionsText = clearance.Sessions.WithoutOnDemand.ToText(settings),
            IsSoleRow = clearance.IsSole,
            // A pass-through or shunting task is one row that cannot hold the arrival/departure split, so both
            // halves' notes appear on it: without this a note classified for the missing half would be dropped.
            Notes = kind switch
            {
                DispatchRowKind.Arrival => [.. notes.Where(note => note.IsForArrival)],
                DispatchRowKind.Departure => [.. notes.Where(note => note.IsForDeparture)],
                _ => notes,
            },
        };
    }
}
