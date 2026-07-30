namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

/// <summary>Which half of a call a timetable row shows.</summary>
public enum CallHalf
{
    /// <summary>Neither: the call is a single moment, and the row shows the departure time.</summary>
    None,

    /// <summary>The train pulling in.</summary>
    Arrival,

    /// <summary>The train leaving.</summary>
    Departure,
}

/// <summary>
/// One row of the train timetable: an Arr/Dep marker, the station, the track, a time, and the notes
/// belonging to that half of the call.
/// </summary>
public sealed class TimetableRow
{
    /// <summary>The call this row belongs to.</summary>
    public required StationCall Call { get; init; }

    /// <summary>Which half of the call the row shows.</summary>
    public CallHalf Half { get; init; }

    /// <summary>
    /// Whether this row is part of the driver's own work. False on the stretches of the train they do
    /// not drive, which are shown greyed and struck through.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A driver often works only a segment of a train. Printing just that segment would leave the route
    /// looking like the whole train, so the rest is shown but visibly cancelled: the point is to make
    /// unmistakable what <em>not</em> to drive. The train's full extent is also orientation — it tells
    /// the driver where the train came from and where it goes on without them.
    /// </para>
    /// <para>
    /// This is judged per <em>half</em>, never per call, because the two boundary calls are split: at
    /// the call where the driver takes over, the departure is theirs but the arrival is not; at the call
    /// where they finish, the arrival is theirs but the departure is not.
    /// </para>
    /// </remarks>
    public bool IsInPart { get; init; } = true;

    /// <summary>The time shown.</summary>
    public Time Time { get; init; }

    /// <summary>
    /// The notes for this row, in <c>DisplayOrder</c>. Never reordered to fit: note order carries
    /// meaning, and a reader who learns that the first note is the most important must not find that
    /// silently untrue on crowded calls.
    /// </summary>
    public IReadOnlyList<ICallNote> Notes { get; init; } = [];

    /// <summary>The station name, repeated on both rows of a two-row call.</summary>
    /// <remarks>
    /// Each row stays self-contained, so a time is never read against an empty station cell — cheap
    /// insurance on paper, where the eye slips between adjacent rows.
    /// </remarks>
    public string StationName => Call.OperationLocation.Name;

    /// <summary>The track the call occupies.</summary>
    public string TrackNumber => Call.Track.Number;

    /// <summary>
    /// The longest note text that may sit in the narrow note column beside the time.
    /// </summary>
    /// <remarks>
    /// Thirty is the prototype's proven value, measured the same way — on the plain text, with any
    /// markup stripped — after real use at operating sessions. The specification suggested starting at
    /// 25 and tuning; there is no reason to re-derive a number that already works.
    /// <para>
    /// It is still below the column's true capacity. The notes that occur most often are the short ones
    /// — <em>No stop</em> is seven characters and <em>No exchange</em> eleven — so the common call keeps
    /// its compact single row, while anything that might wrap is moved to a full-width row where it has
    /// the whole page to run in. Overflowing the column costs an unpredictable wrap; being too cautious
    /// costs one tidy row, so the error is biased low.
    /// </para>
    /// </remarks>
    public const int MaxCharsInNoteColumn = 30;

    /// <summary>
    /// The notes that actually put something on the page.
    /// </summary>
    /// <remarks>
    /// A note that renders to nothing still renders <em>something</em>: an empty span, which as a
    /// stacked row is a blank line the reader cannot see but the page still pays for. That is how a
    /// call came to stand taller than its neighbours for no visible reason, and it broke the even
    /// rhythm the route is read by. Filtered once, here, so it holds for the note column and the
    /// stacked rows alike and the page-height estimate counts only rows that will exist.
    /// <para>
    /// This guards the page against the note, not the note against itself: a note whose text is empty
    /// is a fault wherever it came from, and one worth tracing — but a printed booklet is the wrong
    /// place to find out about it.
    /// </para>
    /// </remarks>
    private IReadOnlyList<ICallNote> PrintingNotes =>
        [.. Notes.Where(note => note.ToText.HasValue)];

    /// <summary>
    /// The note that sits in this row's note column, or <c>null</c> when every note stacks below.
    /// Measured on <c>ToText</c>, never <c>ToHtml</c>: markup would inflate the count and push short
    /// notes out of the column for no reason.
    /// </summary>
    public ICallNote? InlineNote =>
        PrintingNotes is [{ } first, ..] && first.ToText.Length <= MaxCharsInNoteColumn ? first : null;

    /// <summary>
    /// The notes that get a full-width row of their own beneath this one, each on its own line.
    /// </summary>
    /// <remarks>
    /// One note per row rather than a paragraph of concatenated text: each note is then a discrete
    /// statement, and a full page width holds roughly twice what the note column does, so wrapping
    /// becomes the exception rather than the rule.
    /// </remarks>
    public IEnumerable<ICallNote> StackedNotes =>
        InlineNote is null ? PrintingNotes : PrintingNotes.Skip(1);

    /// <summary>
    /// Builds the rows for a train part: the <em>whole</em> train's route, with the halves the driver
    /// does not work marked so they can be shown cancelled.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The number of rows is decided by the <em>times</em>, not by whether the call is a stop: a call
    /// whose arrival and departure differ occupies the platform for a while and gets two rows, while a
    /// call whose times are equal is a single moment and gets one, with an empty Arr/Dep cell. The
    /// train's origin is a departure only and its terminus an arrival only, because neither half exists.
    /// </para>
    /// <para>
    /// The two boundary calls of the part are <em>split</em>, which is why the in-part test is per half:
    /// an arrival belongs to the driver on the calls after they take over up to and including where they
    /// finish, and a departure from where they take over up to but excluding where they finish.
    /// </para>
    /// </remarks>
    /// <param name="trainPart">The part being printed.</param>
    /// <param name="readerSessions">The duty's sessions, for qualifying meet notes.</param>
    /// <param name="settings">How sessions render inside notes.</param>
    public static IReadOnlyList<TimetableRow> Build(
        ScheduledTrainPart trainPart, Sessions readerSessions, SessionsSettings settings)
    {
        trainPart = trainPart.ValueOrException(nameof(trainPart));

        var calls = trainPart.Train.Calls.ToList();
        // By reference: StationCall equality is by value, and a train can hold two equal-valued calls.
        var fromIndex = calls.FindIndex(c => ReferenceEquals(c, trainPart.From));
        var toIndex = calls.FindIndex(c => ReferenceEquals(c, trainPart.To));
        if (fromIndex < 0 || toIndex < 0) return [];

        var rows = new List<TimetableRow>(calls.Count * 2);

        for (var i = 0; i < calls.Count; i++)
        {
            var call = calls[i];
            var arrivalIsInPart = i > fromIndex && i <= toIndex;
            var departureIsInPart = i >= fromIndex && i < toIndex;

            // A stretch the driver does not work carries no notes: the notes are work instructions, and
            // showing them against a cancelled row would say to do something that is not theirs to do.
            var notes = arrivalIsInPart || departureIsInPart
                ? call.DriverNotes(readerSessions, settings)
                : [];

            if (i == 0)
            {
                // The train originates here, so there is no arrival to show.
                rows.Add(new TimetableRow
                {
                    Call = call,
                    Half = CallHalf.Departure,
                    Time = call.Departure,
                    IsInPart = departureIsInPart,
                    Notes = [.. notes.Where(n => n.IsForDeparture)],
                });
                continue;
            }
            if (i == calls.Count - 1)
            {
                // The train terminates here, so there is no departure to show.
                rows.Add(new TimetableRow
                {
                    Call = call,
                    Half = CallHalf.Arrival,
                    Time = call.Arrival,
                    IsInPart = arrivalIsInPart,
                    Notes = [.. notes.Where(n => n.IsForArrival)],
                });
                continue;
            }

            if (call.Arrival.Equals(call.Departure))
            {
                // A single row cannot hold the arrival/departure split, so both halves' notes appear on
                // it. Without this, a note classified for the missing half would be silently dropped —
                // most damagingly for the notes that only occur on single-row calls.
                rows.Add(new TimetableRow
                {
                    Call = call,
                    Half = CallHalf.None,
                    Time = call.Departure,
                    IsInPart = arrivalIsInPart || departureIsInPart,
                    Notes = notes,
                });
                continue;
            }

            rows.Add(new TimetableRow
            {
                Call = call,
                Half = CallHalf.Arrival,
                Time = call.Arrival,
                IsInPart = arrivalIsInPart,
                Notes = [.. notes.Where(n => n.IsForArrival)],
            });
            rows.Add(new TimetableRow
            {
                Call = call,
                Half = CallHalf.Departure,
                Time = call.Departure,
                IsInPart = departureIsInPart,
                Notes = [.. notes.Where(n => n.IsForDeparture)],
            });
        }

        return rows;
    }
}
