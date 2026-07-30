using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// Verifies that a schedule change made in one browser window is picked up by another window on
/// the same origin, via the <c>BroadcastChannel</c>-backed cross-window sync (see
/// <c>CrossWindowSyncService</c> and <c>ScheduleStateService</c>). Both pages share a single
/// <see cref="IBrowserContext"/> (one storage partition) rather than each getting its own, as
/// <see cref="PlaywrightTestBase"/> does — this mirrors two windows of the same real browser.
/// </summary>
[TestClass]
public sealed class CrossWindowSyncTests
{
    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task Creating_a_layout_in_one_window_appears_in_another()
    {
        await using var context = await AppFixture.Browser.NewContextAsync(new BrowserNewContextOptions { BaseURL = AppFixture.BaseUrl });

        var windowA = await context.NewPageAsync();
        var windowB = await context.NewPageAsync();

        await windowA.GotoAsync("/", Idle);
        await windowB.GotoAsync("/", Idle);

        await Expect(windowB.Locator(".active-plan")).ToContainTextAsync("No active document");

        await windowA.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "New layout" }).ClickAsync();
        await Expect(windowA.Locator(".active-plan")).Not.ToContainTextAsync("No active document");
        var planName = await windowA.Locator(".active-plan span").InnerTextAsync();

        // No reload in window B — the update must arrive purely via the cross-window broadcast.
        await Expect(windowB.Locator(".active-plan span")).ToHaveTextAsync(planName, new LocatorAssertionsToHaveTextOptions { Timeout = 5000 });

        await windowA.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPathFor("cross-window-a.png"), FullPage = true });
        await windowB.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPathFor("cross-window-b.png"), FullPage = true });
    }

    private static string ScreenshotPathFor(string fileName)
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "screenshots");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, fileName);
    }
}
