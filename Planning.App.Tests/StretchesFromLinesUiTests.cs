using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Playwright;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for a hall plan whose track stretches are worked out from its lines (<c>ModelHarmonisation.md</c>
/// decision 15): they are made when the hall plan is read, the stretches editor shows the measured distance and the
/// modules' speed, and timetable stretches are suggested from the lines only when the planner asks.
/// </summary>
[TestClass]
public sealed class StretchesFromLinesUiTests : PlaywrightTestBase
{
    private static readonly Guid HallPlanId = Guid.Parse("70000000-0000-0000-0000-000000000007");

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task Track_stretches_are_worked_out_from_the_lines_and_timetable_stretches_suggested()
    {
        await Page.GotoAsync("/import", Idle);
        await Page.SetInputFilesAsync("input[type=file]", new FilePayload { Name = "lined.json", MimeType = "application/json", Buffer = HallPlanFile(LinedHallPlan()) });
        await Expect(Page.Locator(".import-messages")).ToContainTextAsync("new layout made with 3 operation locations and 2 track stretches.");

        await Page.GotoAsync("/stretches", Idle);
        var grid = Page.Locator(".stretch-editor table");
        await Expect(grid.Locator("th", new() { HasText = "Measured (m)" })).ToHaveCountAsync(1);
        var first = grid.Locator("tbody tr").Filter(new() { HasText = "Tellby" });
        await Expect(first).ToContainTextAsync("3.0");
        await Expect(first).ToContainTextAsync("70");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("stretches-from-lines.png"), FullPage = true });

        await first.Locator("button", new() { HasText = "Edit" }).ClickAsync();
        var form = Page.Locator(".stretch-editor .edit");
        await Expect(form).ToContainTextAsync("Measured in the hall plan: 3.0 m");
        await Expect(form).ToContainTextAsync("From the modules: 70 km/h");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("stretch-edit-from-lines.png"), FullPage = true });
        await form.Locator("button", new() { HasText = "Cancel" }).ClickAsync();

        await Page.Locator(".sub-tab", new() { HasText = "Timetable stretches" }).ClickAsync();
        var timetableEditor = Page.Locator(".stretch-editor");
        await Expect(timetableEditor.Locator("table.grid tbody tr")).ToHaveCountAsync(0);
        await timetableEditor.Locator("button", new() { HasText = "Suggest from lines" }).ClickAsync();
        var dialog = Page.Locator(".confirm-dialog").Filter(new() { HasText = "Timetable stretches from the hall plan's lines:" });
        await Expect(dialog).ToContainTextAsync("Main line");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("suggested-timetable-stretches.png"), FullPage = true });
        await dialog.Locator("button", new() { HasText = "Add" }).ClickAsync();
        await Expect(timetableEditor.Locator("table.grid tbody tr")).ToHaveCountAsync(1);
        await Expect(timetableEditor.Locator("table.grid tbody tr")).ToContainTextAsync("Main line");
    }

    /// <summary>
    /// Tellby – plain line – Ytterby – plain line – Kode, the stations 2000 mm long and the plain-line modules 1000 mm,
    /// one at 70 km/h and one at 100 km/h; no track stretches of its own.
    /// </summary>
    private static Layout LinedHallPlan()
    {
        var layout = new Layout { Name = "Lined meeting", SourceId = HallPlanId, SourceRevision = 1 };
        var tellby = layout.Place(Drawn(Station("Tellby", "Tby"), 2000));
        var first = layout.Place(Drawn(new Module { Name = "First", SpeedLimit = 70 }, 1000));
        var ytterby = layout.Place(Drawn(Station("Ytterby", "Yb"), 2000));
        var second = layout.Place(Drawn(new Module { Name = "Second", SpeedLimit = 100 }, 1000));
        var kode = layout.Place(Drawn(Station("Kode", "Kd"), 2000));
        layout.Add(new Line
        {
            Name = "Main line",
            Entries = [.. new[] { tellby, first, ytterby, second, kode }.Select(m => new LineEntry { ModuleId = m.Id })],
        });
        return layout;
    }

    private static Module Station(string name, string signature)
    {
        var station = new Station(0, name, signature) { IsManned = true };
        station.Tracks.Add(new StationTrack(1, "1") { IsMain = true, PlatformLength = 100 });
        return new Module { Name = name, OperationLocation = station };
    }

    private static Module Drawn(Module module, double length)
    {
        module.Drawing.MainPath = PathDrawing.Polyline([new(0, 0), new(length, 0)]);
        return module;
    }

    private static byte[] HallPlanFile(Layout layout)
    {
        var file = new JsonObject
        {
            ["format"] = Model.Layouts.HallPlanFile.Format,
            ["kind"] = Model.Layouts.HallPlanFile.Kind,
            ["schemaVersion"] = Model.Layouts.HallPlanFile.NewestSchemaVersion,
            ["content"] = new JsonObject { ["layout"] = JsonNode.Parse(JsonSerializer.Serialize(layout, Layout.JsonSerializerOptions)) },
        };
        return Encoding.UTF8.GetBytes(file.ToJsonString());
    }
}
