using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Planning.Components.Reporting;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// Runs the whole printed-graph pipeline — print scale, measurement, squeeze, time slicing, packing — against a
/// real plan, and checks the result against the paper it is meant to come out on. The unit tests in
/// <c>GraphPaginationTests</c> cover the arithmetic; this covers whether the arithmetic describes a real layout.
/// </summary>
[TestClass]
public sealed class GraphicalTimetablePrintTests
{
    private const string JsonFilePath = @"C:\Users\Stefan\Downloads\Givskud-Modern-2025.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    public TestContext TestContext { get; set; } = default!;

    private Plan _plan = default!;

    [TestInitialize]
    public void Setup()
    {
        if (!File.Exists(JsonFilePath))
            Assert.Inconclusive($"Test data file not found: {JsonFilePath}");
        _plan = JsonSerializer.Deserialize<Plan>(File.ReadAllText(JsonFilePath), JsonOptions)!;
    }

    private GraphReport Report() =>
        GraphReport.Create(
            _plan.Timetable,
            _plan.Timetable.Layout.TimetableStretches.ToList(),
            _plan.Timetable.Layout.Settings,
            GraphPageGeometry.A4);

    [TestMethod]
    public void NoGraphIsWiderThanTheSheet()
    {
        var geometry = GraphPageGeometry.A4;
        var report = Report();
        var failures = new List<string>();

        foreach (var page in report.Pages)
            foreach (var item in page.Items)
            {
                var length = report.Graph(item, page).TimeAxisLengthMm();
                if (length > geometry.UsableTimeLengthMm)
                    failures.Add($"{item.Stretch.ForwardDescription} {page.From:hh\\:mm}–{page.To:hh\\:mm}: {length:F1} mm along the time axis, {geometry.UsableTimeLengthMm:F1} mm available.");
            }

        Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures));
    }

    [TestMethod]
    public void NoSheetIsOverfilledAcrossTheTimeAxis()
    {
        var geometry = GraphPageGeometry.A4;
        var report = Report();
        var failures = new List<string>();

        foreach (var page in report.Pages)
        {
            // A stretch that is taller than a whole sheet even at the smallest station spacing is printed on a
            // sheet of its own and slightly clipped: there is nothing else to do with it, so it is not a failure.
            if (page.Items.Count == 1) continue;

            var used = page.Items.Sum(item => geometry.HeadingHeightMm + item.CrossLengthMm)
                     + (geometry.GraphGapMm * (page.Items.Count - 1));
            if (used > geometry.UsableCrossLengthMm)
                failures.Add($"{page.From:hh\\:mm}–{page.To:hh\\:mm} with {page.Items.Count} graphs: {used:F1} mm stacked, {geometry.UsableCrossLengthMm:F1} mm available.");
        }

        Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures));
    }

    [TestMethod]
    public void EveryStretchIsPrintedOnEveryTimeSlice()
    {
        var report = Report();
        var stretches = _plan.Timetable.Layout.TimetableStretches.Count;
        var slices = report.Pages.Select(page => (page.From, page.To)).Distinct().Count();

        var printed = report.Pages.SelectMany(page => page.Items.Select(item => (page.From, page.To, item.Stretch))).ToList();

        Assert.AreEqual(stretches * slices, printed.Count, "Every stretch must appear once on every time slice.");
        Assert.AreEqual(printed.Count, printed.Distinct().Count(), "No stretch may be printed twice for the same time slice.");
    }

    [TestMethod]
    public void ReportsWhatThePrintWouldLookLike()
    {
        var geometry = GraphPageGeometry.A4;
        var report = Report();
        var settings = _plan.Timetable.Layout.Settings;

        TestContext.WriteLine($"Layout: {_plan.Timetable.Layout.Name}");
        TestContext.WriteLine($"Scale: {settings.GraphicTimetable.PrintHourSpacingMm} mm/h, {settings.GraphicTimetable.PrintKilometerSpacingMm} mm/km, station floor {settings.GraphicTimetable.PrintStationSpacingMm} mm");
        TestContext.WriteLine($"Orientation: {report.PrintSettings.AxisDirection} → {(report.IsVertical ? "A4 portrait" : "A4 landscape")}");
        TestContext.WriteLine($"Minutes per sheet: {report.PrintSettings.MinutesPerPage(geometry)} ({report.PrintSettings.MinutesPerPage(geometry) / 60.0:F1} h)");
        TestContext.WriteLine($"Sheets: {report.Pages.Count}");

        foreach (var page in report.Pages)
        {
            var used = page.Items.Sum(item => geometry.HeadingHeightMm + item.CrossLengthMm)
                     + (geometry.GraphGapMm * (page.Items.Count - 1));
            TestContext.WriteLine($"  {page.From:hh\\:mm}–{page.To:hh\\:mm}{(page.IsContinued ? " (continued)" : "")}: {used:F1} of {geometry.UsableCrossLengthMm:F1} mm — {string.Join(", ", page.Items.Select(item => $"{item.Stretch.Number} ({item.CrossLengthMm:F1} mm)"))}");
        }

        Assert.IsTrue(report.Pages.Count > 0);
    }
}
