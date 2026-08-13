using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Planning.Components.Reporting;
using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers how a printed graphical timetable is divided into sheets: the time-axis slicing (break time first,
/// then a seam with an overlap) and the packing of stretches onto a sheet — and the print scale claims the
/// slicing rests on, above all that a 14-hour operating window fits one A4 sheet at the default 18&#160;mm/h.
/// </summary>
[TestClass]
public class GraphPaginationTests
{
    private static readonly TimeSpan Six = TimeSpan.FromHours(6);
    private static readonly TimeSpan Twenty = TimeSpan.FromHours(20);

    private static GraphPageItem Item(int number, double crossMm) =>
        new(new TimetableStretch(number, number.ToString()), crossMm, 20);

    private static LayoutSettings Settings()
    {
        var settings = new LayoutSettings();
        settings.General.StartTime = Six;
        settings.General.EndTime = Twenty;
        return settings;
    }

    [TestMethod]
    public void WindowThatFitsOneSheetIsNotSliced()
    {
        var slices = GraphPaginator.TimeSlices(Six, Twenty, breakTime: null, minutesPerPage: 900, overlapMinutes: 15).ToList();

        Assert.AreEqual(1, slices.Count);
        Assert.AreEqual((Six, Twenty, false), slices[0]);
    }

    [TestMethod]
    public void LongWindowIsSlicedAndTheLastSheetIsFilledToTheEnd()
    {
        // Eight hours per sheet over a fourteen-hour window: two sheets. The last is anchored to the end of
        // the window rather than started 15 minutes back from the first, so it carries a full eight hours of
        // paper instead of leaving the right-hand third of the sheet blank.
        var slices = GraphPaginator.TimeSlices(Six, Twenty, breakTime: null, minutesPerPage: 480, overlapMinutes: 15).ToList();

        Assert.AreEqual(2, slices.Count);
        Assert.AreEqual((Six, TimeSpan.FromHours(14), false), slices[0]);
        Assert.AreEqual((TimeSpan.FromHours(12), Twenty, true), slices[1]);
    }

    [TestMethod]
    public void ARemainderOfAFewMinutesDoesNotBecomeItsOwnSheet()
    {
        // 427 minutes a sheet over 840: a third sheet would carry only the last 16 minutes. Anchoring the
        // last sheet to the end of the window absorbs the remainder into its overlap instead.
        var slices = GraphPaginator.TimeSlices(Six, Twenty, breakTime: null, minutesPerPage: 427, overlapMinutes: 15).ToList();

        Assert.AreEqual(2, slices.Count);
        Assert.AreEqual(Twenty, slices[^1].To);
        Assert.AreEqual(TimeSpan.FromMinutes(427), slices[^1].To - slices[^1].From, "The last sheet carries a full page of time.");
    }

    [TestMethod]
    public void BreakTimeIsACleanCutAndBothHalvesStartFresh()
    {
        var slices = GraphPaginator.TimeSlices(Six, Twenty, breakTime: TimeSpan.FromHours(13), minutesPerPage: 900, overlapMinutes: 15).ToList();

        Assert.AreEqual(2, slices.Count);
        Assert.AreEqual((Six, TimeSpan.FromHours(13), false), slices[0], "The first half ends at the break.");
        Assert.AreEqual((TimeSpan.FromHours(13), Twenty, false), slices[1], "The last half starts at the break, with no overlap and not marked as continued.");
    }

    [TestMethod]
    public void BreakTimeOutsideTheWindowIsIgnored()
    {
        var slices = GraphPaginator.TimeSlices(Six, Twenty, breakTime: TimeSpan.FromHours(22), minutesPerPage: 900, overlapMinutes: 15).ToList();

        Assert.AreEqual(1, slices.Count);
    }

    [TestMethod]
    public void StretchesAreStackedUntilTheSheetIsFull()
    {
        // A4 leaves 190 mm across the time axis. Two 80 mm graphs cost 6 + 80 + 5 + 6 + 80 = 177 mm and fit;
        // a third does not.
        var pages = GraphPaginator.BuildPages(
            [Item(1, 80), Item(2, 80), Item(3, 80)],
            Six, Twenty, breakTime: null, minutesPerPage: 900, GraphPageGeometry.A4);

        Assert.AreEqual(2, pages.Count);
        Assert.AreEqual(2, pages[0].Items.Count);
        Assert.AreEqual(1, pages[1].Items.Count);
    }

    [TestMethod]
    public void EverySheetShowsASingleTimeSpanAndTheTimeSlicesComeFirst()
    {
        // Two stretches too tall to share a sheet, over a window needing two slices: four sheets, ordered
        // slice by slice, so both stretches of one time span sit together in the print.
        var pages = GraphPaginator.BuildPages(
            [Item(1, 150), Item(2, 150)],
            Six, Twenty, breakTime: null, minutesPerPage: 480, GraphPageGeometry.A4);

        Assert.AreEqual(4, pages.Count);
        CollectionAssert.AreEqual(
            new[] { Six, Six, TimeSpan.FromHours(12), TimeSpan.FromHours(12) },
            pages.Select(page => page.From).ToArray());
        CollectionAssert.AreEqual(
            new[] { "1", "2", "1", "2" },
            pages.Select(page => page.Items.Single().Stretch.Number).ToArray());
    }

    [TestMethod]
    public void PrintScaleIsATrueMillimetreScale()
    {
        var print = Settings().ToPrintGraphSettings(longestSignatureLength: 5);

        // 18 mm per hour is 0.3 mm per minute, and one unit is a hundredth of a millimetre.
        Assert.AreEqual(30, print.MinuteSpacing);
        Assert.AreEqual(100, print.KilometerSpacing, "1 mm per kilometre.");
        Assert.AreEqual(2000, print.MinStationSpacing, "20 mm minimum between stations.");
        Assert.AreEqual(200, print.TrackSpacing, "2 mm between tracks.");
    }

    [TestMethod]
    public void DefaultScalePutsAFourteenHourWindowOnOneSheet()
    {
        var print = Settings().ToPrintGraphSettings(longestSignatureLength: 5);

        var minutes = print.MinutesPerPage(GraphPageGeometry.A4);

        Assert.IsTrue(minutes >= 14 * 60,
            $"A 14-hour window must fit one sheet at the default scale, but only {minutes} minutes fit.");
    }

    [TestMethod]
    public void ADoubledScaleNeedsTwoSheetsForTheSameWindow()
    {
        var settings = Settings();
        settings.GraphicTimetable.PrintHourSpacingMm = 36;
        var print = settings.ToPrintGraphSettings(longestSignatureLength: 5);

        var slices = GraphPaginator.TimeSlices(Six, Twenty, null, print.MinutesPerPage(GraphPageGeometry.A4), 15).ToList();

        Assert.AreEqual(2, slices.Count);
    }

    [TestMethod]
    public void AGraphMeasuresItsPrintedExtentInMillimetres()
    {
        // Two stations 2 km apart, two tracks each. The distance-based spacing (2 mm) is below the 20 mm floor,
        // so the extent is the hour-label gutter (4 mm) + 20 mm + the terminal fan-out (2 mm) + end margin (2 mm).
        var settings = Settings();
        var layout = new Layout { Name = "Test" };
        var a = new Station(1, "Alpha", "A");
        a.Add(new StationTrack(11, "1"));
        a.Add(new StationTrack(12, "2"));
        var b = new Station(2, "Beta", "B");
        b.Add(new StationTrack(21, "1"));
        b.Add(new StationTrack(22, "2"));
        layout.Add(a);
        layout.Add(b);
        var stretch = new TrackStretch(1, a, b, 2, 1);
        layout.Add(stretch);
        var line = new TimetableStretch(1, "1");
        line.AddLast(stretch);
        layout.Add(line);

        var graph = new GraphSchedule(line, new Timetable("Test", layout), settings.ToPrintGraphSettings(longestSignatureLength: 5));

        Assert.AreEqual(28.0, graph.CrossAxisLengthMm(), 0.001);
    }
}
