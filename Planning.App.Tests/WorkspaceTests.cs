using System.Text.Json;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>End-to-end tests for the dockable Workspace, driven through a real headless browser.</summary>
[TestClass]
public sealed class WorkspaceTests : PlaywrightTestBase
{
    private const string StorageKey = "planning.dock.layout.v1";

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task App_loads_without_uncaught_errors()
    {
        var pageErrors = new List<string>();
        Page.PageError += (_, error) => pageErrors.Add(error);

        await Page.GotoAsync("/", Idle);

        await Expect(Page.Locator(".app-title")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Workspace" })).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("home.png"), FullPage = true });

        Assert.AreEqual(0, pageErrors.Count, "Uncaught JS errors: " + string.Join(" | ", pageErrors));
    }

    [TestMethod]
    public async Task Workspace_starts_empty_with_placeholder()
    {
        await Page.GotoAsync("/workspace", Idle);

        await Expect(Page.GetByText("Drag a view here")).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("workspace-empty.png"), FullPage = true });
    }

    [TestMethod]
    public async Task Persisted_layout_is_restored()
    {
        await SeedLayoutAsync(
            "{\"$type\":\"Split\",\"Orientation\":0,\"Ratio\":0.5," +
            "\"First\":{\"$type\":\"Leaf\",\"ViewId\":\"graphical-timetable\"}," +
            "\"Second\":{\"$type\":\"Leaf\",\"ViewId\":\"settings\"}}");

        await Page.GotoAsync("/workspace", Idle);

        await Expect(Page.Locator(".dock-pane")).ToHaveCountAsync(2);
        await Expect(PaneHeader("Graphical Timetable")).ToBeVisibleAsync();
        await Expect(PaneHeader("Settings")).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("workspace-restored.png"), FullPage = true });
    }

    [TestMethod]
    public async Task Dragging_a_tab_onto_a_pane_creates_a_split()
    {
        await SeedLayoutAsync("{\"$type\":\"Leaf\",\"ViewId\":\"settings\"}");

        await Page.GotoAsync("/workspace", Idle);
        await Expect(Page.Locator(".dock-pane")).ToHaveCountAsync(1);

        // Drag the "Graphical Timetable" tab onto the right edge of the Settings pane.
        // Blazor's drag handlers require a DataTransfer on the event, so create one and pass it.
        var dataTransfer = await Page.EvaluateHandleAsync("() => new DataTransfer()");
        var initiator = new Dictionary<string, object> { ["dataTransfer"] = dataTransfer };

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Graphical Timetable" })
            .DispatchEventAsync("dragstart", initiator);

        var rightZone = Page.Locator(".drop-zones .zone.right").First;
        await rightZone.WaitForAsync();
        await rightZone.DispatchEventAsync("dragover", initiator);
        await rightZone.DispatchEventAsync("drop", initiator);

        await Expect(Page.Locator(".dock-pane")).ToHaveCountAsync(2);
        await Expect(PaneHeader("Graphical Timetable")).ToBeVisibleAsync();
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("workspace-after-drag.png"), FullPage = true });
    }

    /// <summary>Seeds the persisted layout into localStorage before the app boots.</summary>
    private async Task SeedLayoutAsync(string layoutJson)
    {
        var literal = JsonSerializer.Serialize(layoutJson); // safe JS string literal
        await Context.AddInitScriptAsync($"window.localStorage.setItem('{StorageKey}', {literal});");
    }

    private ILocator PaneHeader(string label) =>
        Page.Locator(".dock-pane-header", new PageLocatorOptions { HasTextString = label });
}
