namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

/// <summary>
/// Page geometry for a station dispatch list. Measurements are millimetres; the values are deliberately
/// deterministic so pagination is pure arithmetic (no DOM measuring) and unit-testable.
/// </summary>
/// <remarks>
/// <para>
/// The CSS in <c>StationDispatchTable.razor.css</c> pins the font size, line height and column widths so
/// these estimates match the printed result — calibrate the constants there and here together. A constant
/// that no longer matches the stylesheet does not print short: it prints past the foot of the page, where
/// the row that fell off is a train nobody was told about.
/// </para>
/// <para>
/// The defaults were measured off the rendered report rather than derived from the type size, because
/// deriving them got the row height wrong: a row sets a taller line box than its font alone implies. The
/// stylesheet states one geometry for screen and paper alike, so the on-screen page is a true preview and
/// remeasuring means reading a page in the browser, not printing one.
/// </para>
/// </remarks>
public sealed record DispatchPageGeometry
{
    /// <summary>Printable height of one page (A4 landscape: 210 − 2 × 10 mm margin).</summary>
    public double PrintableHeightMm { get; init; } = 190;

    /// <summary>Printable width of one page (A4 landscape: 297 − 2 × 10 mm margin).</summary>
    public double PrintableWidthMm { get; init; } = 277;

    /// <summary>
    /// The heading line: station name, the interval this page covers, and the neighbours with their phone
    /// numbers. Printed by every page, because a sheet handed to a station has to identify itself whichever
    /// one it is — and with no page footer, it is the page's only identification.
    /// </summary>
    public double HeadingHeightMm { get; init; } = 15.5;

    /// <summary>The column-header row, repeated on every page.</summary>
    public double ColumnHeaderHeightMm { get; init; } = 4.75;

    /// <summary>Heading block plus column headers: what every page spends before its first row.</summary>
    public double HeaderHeightMm => HeadingHeightMm + ColumnHeaderHeightMm;

    /// <summary>Height of a row occupying a single line — no notes, or one note that does not wrap.</summary>
    /// <remarks>
    /// A row is taller than the note lines beneath it because its own line carries the whole row, session
    /// circles included, and those set a taller line box than plain text does.
    /// </remarks>
    public double RowHeightMm { get; init; } = 5.9;

    /// <summary>Height each further line of notes adds to a row.</summary>
    public double NoteLineHeightMm { get; init; } = 5.1;

    /// <summary>Combined width of the columns that do not depend on the number of sessions.</summary>
    /// <remarks>
    /// Train 20, sessions 14, track 8, arrival 13, origin 34, departure 13, destination 34.
    /// </remarks>
    public double FixedColumnsWidthMm { get; init; } = 136;

    /// <summary>Width of the operating sessions/days column, part of <see cref="FixedColumnsWidthMm"/>.</summary>
    /// <remarks>
    /// Stated separately because that column wraps rather than truncates: clipping "on demand only" off
    /// the end of a value would drop operating information without the reader ever knowing it was there.
    /// Wide enough that the everyday values — "All", "None" and a short run of session circles — stay on
    /// one line with room to spare, because an estimate that wrongly charged a second line would charge
    /// it on every single row and empty half the page.
    /// </remarks>
    public double SessionsColumnWidthMm { get; init; } = 14;

    /// <summary>Width of one tick-off column in the sessions grid.</summary>
    public double SessionColumnWidthMm { get; init; } = 4;

    /// <summary>
    /// Width one character of note text takes, biased high so a note is over-charged rather than
    /// under-charged: notes are full of times, train numbers and capitals, all wider than lower-case prose.
    /// </summary>
    /// <remarks>
    /// Over-charging is cheap here because most rows carry a short note or none, so the error costs a
    /// little slack at the foot of a page rather than a line on every row.
    /// </remarks>
    public double NoteCharacterWidthMm { get; init; } = 1.75;

    /// <summary>
    /// Total horizontal padding inside a cell, taken off a column's width to leave the width text
    /// actually wraps within.
    /// </summary>
    /// <remarks>
    /// Small, and easy to forget, but it is 2 mm of a 14 mm column — a sixth of it. Estimating against
    /// the column instead of the text width is what let a value wrap onto a line nobody had paid for.
    /// </remarks>
    public double CellPaddingWidthMm { get; init; } = 2;

    /// <summary>Default geometry for an A4 landscape page with 10 mm margins.</summary>
    public static DispatchPageGeometry A4Landscape { get; } = new();

    /// <summary>
    /// Characters that fit on one line of the notes column, which takes whatever width the tick-off grid
    /// leaves. A long operating period therefore narrows the notes and makes them wrap sooner — the
    /// estimate follows that rather than assuming a fixed column.
    /// </summary>
    /// <param name="sessionCount">The number of tick-off columns, one per session or day of the period.</param>
    public int CharactersPerNoteLine(int sessionCount)
    {
        var width = PrintableWidthMm - FixedColumnsWidthMm - (sessionCount * SessionColumnWidthMm);
        return CharactersWithin(width);
    }

    /// <summary>Characters that fit on one line of the operating sessions/days column.</summary>
    public int CharactersPerSessionsLine => CharactersWithin(SessionsColumnWidthMm);

    private int CharactersWithin(double columnWidthMm) =>
        Math.Max(1, (int)((columnWidthMm - CellPaddingWidthMm) / NoteCharacterWidthMm));
}

/// <summary>
/// One printed page of a station's dispatch list.
/// </summary>
/// <param name="List">The list this page belongs to; its station and neighbours head every page.</param>
/// <param name="Rows">The rows on this page, in time order.</param>
/// <param name="PageNumber">The page's number within the whole report, from 1.</param>
public sealed record DispatchPage(DispatchList List, IReadOnlyList<DispatchRow> Rows, int PageNumber)
{
    /// <summary>The station the page is for.</summary>
    public OperationLocation Station => List.Station;

    /// <summary>The time of the first clearance on this page, or <c>null</c> when it has no rows.</summary>
    public Time? FirstTime => Rows.Count == 0 ? null : Rows[0].Time;

    /// <summary>The time of the last clearance on this page, or <c>null</c> when it has no rows.</summary>
    public Time? LastTime => Rows.Count == 0 ? null : Rows[^1].Time;
}

/// <summary>
/// Splits station dispatch lists into printed pages.
/// </summary>
/// <remarks>
/// Every station starts on a fresh page: the sheets are torn apart and handed to different people, so a
/// page carrying the tail of one station and the head of another belongs to neither. A station with no
/// trains at all still gets its page, which says so — a dispatcher handed nothing cannot tell an empty
/// list from a list that was never printed.
/// </remarks>
public static class DispatchPaginator
{
    /// <summary>Builds the pages of the whole report, numbered from 1 across all stations.</summary>
    /// <param name="lists">The stations' lists, in the order they are to be printed.</param>
    /// <param name="sessionCount">The number of tick-off columns, which decides how wide the notes are.</param>
    /// <param name="geometry">The page geometry to estimate against.</param>
    public static IReadOnlyList<DispatchPage> BuildPages(
        IEnumerable<DispatchList> lists, int sessionCount, DispatchPageGeometry geometry)
    {
        lists = lists.ValueOrException(nameof(lists));
        geometry = geometry.ValueOrException(nameof(geometry));

        var pages = new List<DispatchPage>();
        foreach (var list in lists)
        {
            foreach (var rows in SplitIntoPages(list.Rows, sessionCount, geometry))
                pages.Add(new DispatchPage(list, rows, pages.Count + 1));
        }
        return pages;
    }

    /// <summary>
    /// The height of one row: its own line, plus a line for each further line its tallest cell needs.
    /// </summary>
    /// <remarks>
    /// A row is as tall as its tallest cell, which is the notes column or — rarely — the sessions column.
    /// The first line of each sits on the row's own line, so a row with one short note is no taller than a
    /// row with none. Wrapping is charged as well as counted, because a value too long for its column
    /// takes the extra line whether or not the estimate expected it.
    /// </remarks>
    /// <param name="row">The row to measure.</param>
    /// <param name="sessionCount">The number of tick-off columns, which decides how wide the notes are.</param>
    /// <param name="geometry">The page geometry to estimate against.</param>
    public static double HeightMmOf(DispatchRow row, int sessionCount, DispatchPageGeometry geometry)
    {
        row = row.ValueOrException(nameof(row));
        geometry = geometry.ValueOrException(nameof(geometry));

        var perNoteLine = geometry.CharactersPerNoteLine(sessionCount);
        var noteLines = row.PrintingNotes.Sum(note => LinesOf(note.ToText, perNoteLine));
        var sessionsLines = LinesOf(row.SessionsText, geometry.CharactersPerSessionsLine);

        var lines = Math.Max(Math.Max(noteLines, sessionsLines), 1);
        return geometry.RowHeightMm + ((lines - 1) * geometry.NoteLineHeightMm);
    }

    /// <summary>
    /// How many lines a value wraps onto in a column that fits <paramref name="charactersPerLine"/>
    /// characters.
    /// </summary>
    /// <remarks>
    /// Wrapping happens at word boundaries, so dividing the length by the line width under-counts —
    /// badly in a narrow column, where one long word can push the rest of a phrase onto a line of its
    /// own. "None, on demand only" in a 14&#160;mm column takes four lines, not the three that division
    /// gives, and the line nobody charged for is a row that falls off the foot of the page. A word too
    /// long for the line breaks across lines of its own rather than looping forever.
    /// </remarks>
    /// <param name="text">The value to measure.</param>
    /// <param name="charactersPerLine">Characters the column fits on one line.</param>
    public static int LinesOf(string text, int charactersPerLine)
    {
        if (string.IsNullOrWhiteSpace(text)) return 1;

        var lines = 1;
        var used = 0;
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var withWord = used == 0 ? word.Length : used + 1 + word.Length;
            if (used > 0 && withWord > charactersPerLine)
            {
                lines++;
                used = word.Length;
            }
            else
            {
                used = withWord;
            }
            while (used > charactersPerLine)
            {
                lines++;
                used -= charactersPerLine;
            }
        }
        return lines;
    }

    // Fills pages with rows until the next row would overflow, always placing at least one row per page
    // so a row taller than the page still gets printed rather than looping forever.
    private static IEnumerable<IReadOnlyList<DispatchRow>> SplitIntoPages(
        IReadOnlyList<DispatchRow> rows, int sessionCount, DispatchPageGeometry geometry)
    {
        var available = geometry.PrintableHeightMm - geometry.HeaderHeightMm;
        var current = new List<DispatchRow>();
        var used = 0.0;

        foreach (var row in rows)
        {
            var height = HeightMmOf(row, sessionCount, geometry);
            if (current.Count > 0 && used + height > available)
            {
                yield return current;
                current = [];
                used = 0;
            }
            current.Add(row);
            used += height;
        }

        // A station with no trains still gets one (empty) page; see the type's remarks.
        yield return current;
    }
}
