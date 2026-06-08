using Microsoft.Playwright;

namespace Tellurian.Trains.Schedules.Planning.App.E2E.Tests;

/// <summary>Diagnostic capture of the app's browser console + uncaught errors at startup.</summary>
[TestClass]
public sealed class StartupDiagnostics : PlaywrightTestBase
{
    [TestMethod]
    public async Task Capture_startup_console()
    {
        var lines = new List<string>();
        Page.Console += (_, m) => lines.Add($"[{m.Type}] {m.Text}");
        Page.PageError += (_, e) => lines.Add($"[pageerror] {e}");

        await Page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.WaitForTimeoutAsync(4000);

        await File.WriteAllLinesAsync(ScreenshotPath("startup-console.txt"), lines);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("startup.png"), FullPage = true });
    }
}
