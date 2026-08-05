using Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers how a station's dispatch list is broken into printed pages: what a row costs, where the break
/// falls, and the rule that no page ever carries two stations.
/// </summary>
[TestClass]
public class DispatchPaginationTests
{
    // Short forms, as every printed report asks for: the sessions column then holds "All" rather than
    // "All sessions", which in a 14 mm column wraps and would make these tests about that instead of
    // about the notes they are measuring.
    private static readonly SessionsSettings Settings =
        new() { MaxNumberOfSessions = 4, UseShortWeekdayNames = true };
    private static readonly int SessionCount = SessionsFormatting.PositionsOf(Settings).Count;

    private static string LanguageCode => System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

    // Room for exactly five note-free rows, so a break is reached without building a hundred trains.
    // Round numbers, so a test states what it means rather than the calibrated millimetres.
    private static readonly DispatchPageGeometry Geometry = DispatchPageGeometry.A4Landscape with
    {
        PrintableHeightMm = 40,
        HeadingHeightMm = 10,
        ColumnHeaderHeightMm = 5,
        RowHeightMm = 5,
        NoteLineHeightMm = 4,
    };

    // A timetable of one two-station layout with the given number of trains, each running from the first
    // station to the second. Every train therefore contributes exactly one row to each station: a
    // departure where it originates and an arrival where it terminates.
    private static Timetable CreateTimetable(int trainCount)
    {
        var layout = new Layout { Name = "Test" };
        var first = layout.Add(NewStation(1, "Munkeröd", "Mkd"));
        var last = layout.Add(NewStation(2, "Slokärr", "Slk"));
        layout.Add(new TrackStretch(1, first, last, 10));

        var timetable = new Timetable("Test", layout);
        for (var i = 0; i < trainCount; i++)
        {
            var start = Time.FromHourAndMinute(8, 0).AddMinutes(i * 30);
            var train = new Train(i + 1, 1000 + i);
            train.Add(new StationCall((i * 10) + 1, first["1"], start.AddMinutes(-10), start));
            var arrival = start.AddMinutes(15);
            var lastCall = train.Add(new StationCall((i * 10) + 2, last["1"], arrival, arrival.AddMinutes(5)));
            lastCall.IsArrival = true;
            timetable.Add(train);
        }
        return timetable;
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    private static DispatchList ListFor(Timetable timetable, string name) =>
        DispatchList.Create(
            timetable.Layout.OperationLocations.Single(l => l.Name == name), timetable.Trains, Settings);

    private static IReadOnlyList<DispatchPage> Paginate(params DispatchList[] lists) =>
        DispatchPaginator.BuildPages(lists, SessionCount, Geometry);

    [TestMethod]
    public void ARowWithAtMostOneShortNoteCostsOneLine()
    {
        var timetable = CreateTimetable(1);
        var list = ListFor(timetable, "Munkeröd");
        var bare = DispatchPaginator.HeightMmOf(list.Rows.Single(), SessionCount, Geometry);

        timetable.Trains.Single().Calls[0].Notes.Add(new TextCallNote("short", LanguageCode) { IsForDeparture = true });
        var withOne = DispatchPaginator.HeightMmOf(
            ListFor(timetable, "Munkeröd").Rows.Single(), SessionCount, Geometry);

        // The first note sits on the row's own line, so it costs nothing extra.
        Assert.AreEqual(Geometry.RowHeightMm, bare);
        Assert.AreEqual(bare, withOne);
    }

    [TestMethod]
    public void EachNoteBeyondTheFirstCostsAnotherLine()
    {
        var timetable = CreateTimetable(1);
        var call = timetable.Trains.Single().Calls[0];
        call.Notes.Add(new TextCallNote("one", LanguageCode, 1) { IsForDeparture = true });
        call.Notes.Add(new TextCallNote("two", LanguageCode, 2) { IsForDeparture = true });
        call.Notes.Add(new TextCallNote("three", LanguageCode, 3) { IsForDeparture = true });

        var height = DispatchPaginator.HeightMmOf(
            ListFor(timetable, "Munkeröd").Rows.Single(), SessionCount, Geometry);

        // The first note shares the row's own line; the other two each add a note line.
        Assert.AreEqual(Geometry.RowHeightMm + (2 * Geometry.NoteLineHeightMm), height);
    }

    [TestMethod]
    public void ANoteTooLongForItsColumnIsChargedForTheLinesItWrapsOnto()
    {
        var timetable = CreateTimetable(1);
        var perLine = Geometry.CharactersPerNoteLine(SessionCount);
        timetable.Trains.Single().Calls[0].Notes.Add(
            new TextCallNote(new string('x', (perLine * 2) + 1), LanguageCode) { IsForDeparture = true });

        var height = DispatchPaginator.HeightMmOf(
            ListFor(timetable, "Munkeröd").Rows.Single(), SessionCount, Geometry);

        // A note too long for the column takes the extra lines whether or not the estimate expected it,
        // and a page that overflows loses the rows off the foot of it without a word.
        Assert.AreEqual(Geometry.RowHeightMm + (2 * Geometry.NoteLineHeightMm), height);
    }

    [TestMethod]
    public void ARowIsAsTallAsItsTallestCellNotJustItsNotes()
    {
        var timetable = CreateTimetable(1);
        // Narrow enough that even the short "All" wraps.
        var narrow = Geometry with { SessionsColumnWidthMm = Geometry.CellPaddingWidthMm + Geometry.NoteCharacterWidthMm };
        var row = ListFor(timetable, "Munkeröd").Rows.Single();

        var height = DispatchPaginator.HeightMmOf(row, SessionCount, narrow);

        // The sessions column wraps rather than truncate, so its extra lines have to be paid for even on
        // a row carrying no notes at all.
        Assert.IsEmpty(row.PrintingNotes, "The fixture needs a row whose height can only come from its sessions.");
        Assert.IsGreaterThan(narrow.CharactersPerSessionsLine, row.SessionsText.Length, "It must wrap.");
        Assert.IsGreaterThan(Geometry.RowHeightMm, height);
    }

    [TestMethod]
    public void WrappingIsCountedByWordsNotByDividingTheLength()
    {
        // Six characters is what a 14 mm sessions column fits. Dividing 19 by 6 gives three lines, but
        // the words do not pack that way — it renders on four, and the line nobody charged for is a row
        // that falls off the foot of the page.
        Assert.AreEqual(4, DispatchPaginator.LinesOf("None On demand only", 6));
        Assert.AreEqual(1, DispatchPaginator.LinesOf("All", 6));

        // A word too long for the line breaks across lines of its own rather than looping forever.
        Assert.AreEqual(3, DispatchPaginator.LinesOf(new string('x', 13), 6));
    }

    [TestMethod]
    public void TheCellPaddingIsTakenOffTheWidthTextWrapsWithin()
    {
        // 2 mm of a 14 mm column is a sixth of it; measuring against the column rather than the text
        // width is what let a value wrap onto an uncharged line.
        var geometry = DispatchPageGeometry.A4Landscape;

        Assert.AreEqual(
            (int)((geometry.SessionsColumnWidthMm - geometry.CellPaddingWidthMm) / geometry.NoteCharacterWidthMm),
            geometry.CharactersPerSessionsLine);
    }

    [TestMethod]
    public void ALongOperatingPeriodNarrowsTheNotesAndWrapsThemSooner()
    {
        var geometry = DispatchPageGeometry.A4Landscape;

        // Every tick-off column is taken off the notes, which take whatever the grid leaves.
        Assert.IsTrue(geometry.CharactersPerNoteLine(14) < geometry.CharactersPerNoteLine(4));
    }

    [TestMethod]
    public void RowsFillAPageBeforeSpillingOntoTheNext()
    {
        // Five trains departing Munkeröd: five rows, exactly the page budget.
        var pages = Paginate(ListFor(CreateTimetable(5), "Munkeröd"));

        Assert.HasCount(1, pages);
        Assert.HasCount(5, pages.Single().Rows);
    }

    [TestMethod]
    public void TheRowThatWouldOverflowStartsANewPage()
    {
        var pages = Paginate(ListFor(CreateTimetable(6), "Munkeröd"));

        Assert.HasCount(2, pages);
        Assert.HasCount(5, pages[0].Rows);
        Assert.HasCount(1, pages[1].Rows);
    }

    [TestMethod]
    public void PagesAreNumberedAcrossTheWholeReport()
    {
        // Six rows at each station: two pages each, numbered straight through rather than restarting.
        var timetable = CreateTimetable(6);
        var pages = Paginate(ListFor(timetable, "Munkeröd"), ListFor(timetable, "Slokärr"));

        Assert.AreSequenceEqual(new[] { 1, 2, 3, 4 }, pages.Select(p => p.PageNumber).ToArray());
    }

    [TestMethod]
    public void EveryStationStartsOnItsOwnPage()
    {
        var timetable = CreateTimetable(2);
        var pages = Paginate(ListFor(timetable, "Munkeröd"), ListFor(timetable, "Slokärr"));

        // The pile is torn apart and handed out, so a page carrying the tail of one station and the head
        // of another would belong to neither — even though these two would fit on one sheet together.
        Assert.HasCount(2, pages);
        Assert.AreEqual("Munkeröd", pages[0].Station.Name);
        Assert.AreEqual("Slokärr", pages[1].Station.Name);
    }

    [TestMethod]
    public void AStationWithNoTrainsStillGetsAPage()
    {
        var timetable = CreateTimetable(0);

        // A dispatcher handed nothing cannot tell an empty list from a list that was never printed.
        var page = Paginate(ListFor(timetable, "Munkeröd")).Single();
        Assert.IsEmpty(page.Rows);
        Assert.IsNull(page.FirstTime);
    }

    [TestMethod]
    public void APageKnowsTheIntervalItCovers()
    {
        var page = Paginate(ListFor(CreateTimetable(3), "Munkeröd")).Single();

        // The heading states it, which is how a reader finds the right sheet in a station's pile.
        Assert.AreEqual("08:00", page.FirstTime?.HHMM());
        Assert.AreEqual("09:00", page.LastTime?.HHMM());
    }
}
