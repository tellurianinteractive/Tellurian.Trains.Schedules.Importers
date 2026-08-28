using Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the booklet page pipeline: packing parts onto pages, the spread rule for a part too tall to
/// fit one, blank padding, and the saddle-stitch imposition.
/// </summary>
[TestClass]
public class DutyPaginationTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    // A duty of `partCount` short train parts, each on its own two-call train.
    private static DriverDuty CreateDuty(int partCount, int callsPerTrain = 2)
    {
        var layout = new Layout { Name = "Test" };
        var stations = new List<OperationLocation>();
        for (var i = 0; i < callsPerTrain; i++)
        {
            var station = new Station(i + 1, $"Station{i + 1}", $"S{i + 1}");
            station.Add(new StationTrack((i + 1) * 10, "1"));
            stations.Add(layout.Add(station));
        }
        for (var i = 0; i < stations.Count - 1; i++)
            layout.Add(new TrackStretch(i + 1, stations[i], stations[i + 1], 10));

        var timetable = new Timetable("Test", layout);
        var plan = Plan.Create("Test", timetable);
        var duty = plan.CreateDriverDuty();

        for (var p = 0; p < partCount; p++)
        {
            var train = new Train(p + 1, 100 + p);
            var start = Time.FromHourAndMinute(6 + p, 00);
            for (var c = 0; c < callsPerTrain; c++)
            {
                var call = train.Add(new StationCall(c + 1, stations[c]["1"], start.AddMinutes(c * 5), start.AddMinutes(c * 5 + 1)));
                call.IsArrival = true;
                call.IsDeparture = true;
            }
            timetable.Add(train);
            var schedule = plan.CreateSchedule();
            duty.Add(schedule.Add(train.AsTrainPart));
        }
        return duty;
    }

    private static IReadOnlyList<DutyPage> Pages(DriverDuty duty) =>
        DutyPagination.BuildPages(duty, Settings);

    [TestMethod]
    public void AMinimalBookletIsFourPagesEndingWithTheOverview()
    {
        var pages = Pages(CreateDuty(partCount: 1));

        // Front, one part page, one blank, overview.
        Assert.HasCount(4, pages);
        Assert.AreEqual(DutyPageKind.Front, pages[0].Kind);
        Assert.AreEqual(DutyPageKind.Part, pages[1].Kind);
        Assert.AreEqual(DutyPageKind.Blank, pages[2].Kind);
        Assert.AreEqual(DutyPageKind.Overview, pages[^1].Kind);
    }

    [TestMethod]
    public void ThePageCountIsAlwaysAMultipleOfFour()
    {
        // Without this a print-dialogue page range could not isolate one duty, because the last sheet
        // of one booklet would carry the first pages of the next.
        foreach (var partCount in new[] { 1, 2, 3, 5, 8, 13 })
        {
            var pages = Pages(CreateDuty(partCount));
            Assert.AreEqual(0, pages.Count % 4, $"{partCount} parts produced {pages.Count} pages.");
        }
    }

    [TestMethod]
    public void TheOverviewPageIsAlwaysTheLastPage()
    {
        // A driver finds it by turning to the back, without knowing the page count — so the blank
        // padding sits before it rather than at the very end.
        foreach (var partCount in new[] { 1, 2, 3, 5, 8, 13 })
        {
            var pages = Pages(CreateDuty(partCount));
            Assert.AreEqual(DutyPageKind.Overview, pages[^1].Kind);
            Assert.HasCount(1, pages.Where(p => p.Kind == DutyPageKind.Overview));
        }
    }

    [TestMethod]
    public void PagesAreNumberedInReadingOrderFromOne()
    {
        var pages = Pages(CreateDuty(partCount: 5));

        for (var i = 0; i < pages.Count; i++)
            Assert.AreEqual(i + 1, pages[i].PageNumber);
    }

    [TestMethod]
    public void PartsArePackedGreedilyRatherThanOnePerPage()
    {
        var pages = Pages(CreateDuty(partCount: 3));

        var partPages = pages.Where(p => p.Kind == DutyPageKind.Part).ToList();
        Assert.IsTrue(partPages.Sum(p => p.Parts.Count) == 3, "Every part is placed exactly once.");
        Assert.IsTrue(partPages.Count < 3, "Short parts share a page rather than each taking one.");
    }

    [TestMethod]
    public void APartTooTallForOnePageSplitsOntoAFacingPair()
    {
        // A long route makes the timetable block outgrow a page on its own.
        var duty = CreateDuty(partCount: 1, callsPerTrain: 40);
        var pages = Pages(duty);

        var tables = pages.Single(p => p.IsSplitFirstHalf);
        var timetable = pages.Single(p => p.IsTimetableContinuation);

        // The driver consulting the timetable must see the tables it points at, so the two must be a
        // spread — and in a saddle-stitch booklet the spreads are (even, even+1).
        Assert.AreEqual(0, tables.PageNumber % 2, "A split part must begin on an even page.");
        Assert.AreEqual(tables.PageNumber + 1, timetable.PageNumber);
    }

    [TestMethod]
    public void ASplitPartsFirstPageCarriesNoContinuationMarker()
    {
        var duty = CreateDuty(partCount: 1, callsPerTrain: 40);
        var pages = Pages(duty);

        // The part is not finished there and its timetable is on the facing page, in view without
        // turning, so there is nothing to warn about.
        Assert.IsTrue(pages.Single(p => p.IsSplitFirstHalf).IsSplitFirstHalf);
        Assert.IsFalse(pages.Single(p => p.IsTimetableContinuation).IsSplitFirstHalf);
    }

    // The formula must reproduce the four page counts the prototype hardcoded, term for term.
    [DataTestMethod]
    [DataRow(4, "4,1,2,3")]
    [DataRow(8, "8,1,2,7,6,3,4,5")]
    [DataRow(12, "12,1,2,11,10,3,4,9,8,5,6,7")]
    [DataRow(16, "16,1,2,15,14,3,4,13,12,5,6,11,10,7,8,9")]
    public void BookletOrderReproducesTheProvenTables(int pageCount, string expected)
    {
        Assert.AreEqual(expected, string.Join(",", DutyPagination.BookletPageOrder(pageCount)));
    }

    [TestMethod]
    public void BookletOrderExtendsBeyondSixteenPages()
    {
        // The prototype's hardcoded table threw above sixteen; the formula does not.
        var order = DutyPagination.BookletPageOrder(20).ToList();

        Assert.HasCount(20, order);
        Assert.AreEqual(20, order[0]);
        Assert.AreEqual(1, order[1]);
    }

    [TestMethod]
    public void BookletOrderIsAPermutationOfEveryPage()
    {
        foreach (var count in new[] { 4, 8, 12, 16, 20, 40 })
        {
            var order = DutyPagination.BookletPageOrder(count).ToList();
            Assert.AreEqual(string.Join(",", Enumerable.Range(1, count)), string.Join(",", order.Order()),
                $"Every page must appear exactly once for a {count}-page booklet.");
        }
    }

    [TestMethod]
    public void ImpositionGroupsPagesIntoSheetSidesOfTwo()
    {
        var pages = Pages(CreateDuty(partCount: 3));

        var sides = DutyPagination.Impose(pages);

        Assert.AreEqual(pages.Count / 2, sides.Count);
        // The outermost sheet's front carries the last page on the left and the first on the right.
        Assert.AreEqual(pages.Count, sides[0].Left.PageNumber);
        Assert.AreEqual(1, sides[0].Right.PageNumber);
    }

    [TestMethod]
    public void NoPageIsPackedBeyondTheBudget()
    {
        // The invariant the whole estimate exists for. A page packed past the budget does not print
        // short: the page body is overflow: hidden, so the surplus is taken away silently, and a driver
        // is handed a duty whose last train is missing.
        foreach (var callsPerTrain in new[] { 2, 4, 6, 9 })
        {
            var pages = Pages(CreateDuty(partCount: 5, callsPerTrain));
            foreach (var page in pages.Where(p => p.Parts.Count > 0 && !p.IsSplitFirstHalf && !p.IsTimetableContinuation))
            {
                var used = page.Parts.Sum(p => DutyPagination.HeightOf(p));
                Assert.IsTrue(used <= DutyPagination.PageBudget,
                    $"Page {page.PageNumber} holds {used:F2} row units of a {DutyPagination.PageBudget} budget.");
            }
        }
    }

    [TestMethod]
    public void TwoPartsWithFullTimetablesDoNotShareAPage()
    {
        // The case that overflowed in print: a part of a six-call train is 16 row units, so a second
        // cannot follow it onto a page holding 28. This failed while the budget was counted in 16 px
        // lines and the rows charged against it were 24 px table rows — each part then measured 20
        // units against a budget of 43, and both were packed onto a page that fits neither.
        var pages = Pages(CreateDuty(partCount: 2, callsPerTrain: 6));

        Assert.HasCount(2, pages.Where(p => p.Parts.Count == 1));
    }

    [TestMethod]
    public void ASuppressedBlockCostsNoHeight()
    {
        var duty = CreateDuty(partCount: 1);
        var part = new DriverDutyPart
        {
            TrainPart = duty.OrderedParts[0],
            Duty = duty,
            SessionsSettings = Settings,
        };

        // The part has no wagonsets and no cargo, so only the header and the timetable are charged.
        var expected = DutyPagination.HeaderHeight - 1 + DutyPagination.TimetableHeight(part);
        Assert.AreEqual(expected, DutyPagination.HeightOf(part), 0.01,
            "An empty block is omitted entirely and costs nothing in page fitting.");
    }
}
