using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end tests for the error report indicator: an error kept from an earlier run (as after a crash and a
/// reload) shows the (!) button, whose panel offers a mail without recipient and the report to copy.
/// </summary>
[TestClass]
public sealed class ErrorReportUiTests : PlaywrightTestBase
{
    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    private const string StoredReport = """
        [{"Time":"2026-11-26T10:15:03+00:00","Source":"Renderer","Message":"Unhandled","Details":"System.InvalidOperationException: Boom\n   at Somewhere.Method()"}]
        """;

    [TestMethod]
    public async Task No_indicator_without_errors()
    {
        await Page.GotoAsync("/", Idle);
        await Expect(Page.Locator(".top-bar-actions")).ToBeVisibleAsync();
        await Expect(Page.Locator(".error-report")).ToHaveCountAsync(0);
    }

    [TestMethod]
    public async Task A_kept_error_can_be_mailed_copied_and_cleared()
    {
        await Page.GotoAsync("/", Idle);
        await Page.EvaluateAsync("r => localStorage.setItem('tellurian.planner.errorReports', r)", StoredReport);
        await Page.ReloadAsync(new() { WaitUntil = WaitUntilState.NetworkIdle });

        var button = Page.Locator(".error-report .indicator-btn");
        await Expect(button).ToBeVisibleAsync();
        await button.ClickAsync();

        var panel = Page.Locator(".error-report .panel");
        await Expect(panel.Locator(".details")).ToContainTextAsync("System.InvalidOperationException: Boom");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("error-report-panel.png") });

        var href = await panel.Locator("a.action").GetAttributeAsync("href");
        Assert.IsNotNull(href);
        Assert.StartsWith("mailto:?subject=Error%20report", href);
        Assert.Contains("InvalidOperationException", Uri.UnescapeDataString(href));

        await panel.Locator("button.clear").ClickAsync();
        await Expect(Page.Locator(".error-report")).ToHaveCountAsync(0);
        Assert.IsNull(await Page.EvaluateAsync<string?>("() => localStorage.getItem('tellurian.planner.errorReports')"));
    }
}
