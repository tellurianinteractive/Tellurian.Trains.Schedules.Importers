using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// Regression test for the turnus cards report. Importing Rotebro2016 (12 locomotives) must
/// render 12 turnus cards. This guards against the report blanking out when a render-time
/// dependency fails to load — see the C# 14 generic-extension-member / Blazor WASM issue that
/// caused <c>Tellurian.Utilities.Printing.PaginationExtensions</c> to throw a TypeLoadException.
/// </summary>
[TestClass]
public sealed class TurnusCardsReportTests : PlaywrightTestBase
{
    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    private static string Rotebro2016Ods
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && !dir.EnumerateFiles("*.slnx").Any()) dir = dir.Parent;
            return Path.Combine(dir!.FullName, "Importers.Xpln.Tests", "Test data", "Rotebro2016.sv-SE.ods");
        }
    }

    [TestMethod]
    public async Task Importing_Rotebro2016_shows_twelve_turnus_cards()
    {
        var renderErrors = new List<string>();
        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error" && msg.Text.Contains("Unhandled exception rendering component"))
                renderErrors.Add(msg.Text);
        };

        Assert.IsTrue(File.Exists(Rotebro2016Ods), $"Missing test file {Rotebro2016Ods}");

        // Import the XPLN schedule.
        await Page.GotoAsync("/import", Idle);
        await Page.SetInputFilesAsync("input[type=file]", Rotebro2016Ods);
        await Expect(Page.Locator(".schedule-summary")).ToBeVisibleAsync(new() { Timeout = 30_000 });

        // Open the turnus cards report via the Reports menu.
        await Page.GetByRole(AriaRole.Button, new() { Name = "Reports" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Turnus cards" }).ClickAsync();

        await Expect(Page.Locator(".pocket.frame")).ToHaveCountAsync(12, new() { Timeout = 15_000 });
        Assert.AreEqual(0, renderErrors.Count, "Render errors: " + string.Join(" | ", renderErrors));
    }
}
