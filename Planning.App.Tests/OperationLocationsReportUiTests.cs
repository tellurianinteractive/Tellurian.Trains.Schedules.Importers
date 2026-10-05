using System.Text.Json;
using Microsoft.Playwright;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Schedules;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for the Operation locations report: one sheet per location, listing the properties that
/// apply to its kind, its regions, its instructions and its tracks.
/// </summary>
[TestClass]
public sealed class OperationLocationsReportUiTests : PlaywrightTestBase
{
    private const string ScheduleStorageKey = "planning.schedule.v2";

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task Each_location_gets_a_sheet_with_its_properties_instructions_and_tracks()
    {
        await OpenTheReportAsync();

        // In alphabetical order, not the order they were added to the layout in.
        var sheets = Page.Locator(".location-sheet");
        await Expect(sheets).ToHaveCountAsync(3);
        await Expect(sheets.Locator("h1")).ToHaveTextAsync([
            new System.Text.RegularExpressions.Regex(@"^Bro\s+Bro$"),
            new System.Text.RegularExpressions.Regex(@"^Munkeröd\s+Mkd$"),
            new System.Text.RegularExpressions.Regex(@"^Rubjerg\s+Rbj$"),
        ]);

        // The manned station: its own minimum stop, the layout's runaround time, its region, its instructions,
        // and its tracks in display order with the route one of them is for.
        var munkerod = sheets.Nth(1);
        await Expect(Property(munkerod, "Type")).ToHaveTextAsync("Station");
        await Expect(Property(munkerod, "Manned?")).ToHaveTextAsync("Yes");
        await Expect(Property(munkerod, "Owner")).ToHaveTextAsync("Anna");
        await Expect(Property(munkerod, "Minimum stop (fast-clock min)")).ToHaveTextAsync("4");
        await Expect(Property(munkerod, "Loco runaround (real min)")).ToHaveTextAsync("5");
        await Expect(munkerod.Locator(".regions .region")).ToHaveCountAsync(1);
        await Expect(munkerod.Locator(".instructions-body h2")).ToHaveTextAsync("Shunting");
        await Expect(munkerod.Locator(".tracks td.number")).ToHaveTextAsync(["1", "2", "3"]);
        await Expect(munkerod.Locator(".tracks tbody tr").Nth(1).Locator("td.route")).ToHaveTextAsync("Bro → Rbj");
        await Expect(munkerod.Locator(".tracks tbody tr").Nth(2).Locator("td.usage")).ToHaveTextAsync("Goods shed");
        // A manned station works its own switches, so it is offered no lock key; and nothing serves it cargo,
        // so no such line is printed.
        await Expect(Property(munkerod, "Lock key held at")).ToHaveCountAsync(0);
        await Expect(Property(munkerod, "Cargo served from")).ToHaveCountAsync(0);
        await Expect(Property(munkerod, "Train clearance (real min)")).ToHaveTextAsync("1");

        // The unmanned station: served and unlocked from Munkeröd, and the key named.
        var rubjerg = sheets.Nth(2);
        await Expect(Property(rubjerg, "Cargo served from")).ToHaveTextAsync("Munkeröd (Mkd)");
        await Expect(Property(rubjerg, "Lock key held at")).ToHaveTextAsync("Munkeröd (Mkd)");
        await Expect(Property(rubjerg, "Lock key name")).ToHaveTextAsync("Key R");
        // A station still has the tracks to run a loco around, but nobody on duty to clear a train.
        await Expect(Property(rubjerg, "Loco runaround (real min)")).ToHaveTextAsync("5");
        await Expect(Property(rubjerg, "Train clearance (real min)")).ToHaveCountAsync(0);

        // The signal controlled location: none of the exchange properties it can never have.
        var bro = sheets.Nth(0);
        await Expect(Property(bro, "Type")).ToHaveTextAsync("Signal-controlled location");
        await Expect(Property(bro, "Passengers?")).ToHaveCountAsync(0);
        await Expect(Property(bro, "Manned?")).ToHaveCountAsync(0);
        await Expect(Property(bro, "Controlled from")).ToHaveTextAsync("Munkeröd (Mkd)");
        await Expect(Property(bro, "Loco runaround (real min)")).ToHaveCountAsync(0);
        await Expect(Property(bro, "Train clearance (real min)")).ToHaveCountAsync(0);

        // Nothing wider than the sheet it is on.
        var wide = await Page.EvaluateAsync<int>(
            """
            () => [...document.querySelectorAll('.location-sheet')].filter(sheet =>
                [...sheet.querySelectorAll('table, dl, .instructions-body')].some(part =>
                    part.getBoundingClientRect().right > sheet.getBoundingClientRect().right - parseFloat(getComputedStyle(sheet).paddingRight) + 0.5)).length
            """);
        Assert.AreEqual(0, wide, "A sheet has something wider than its margins.");

        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("operation-locations.png"), FullPage = true });
    }

    // The value of the property labelled exactly so on a sheet.
    private static ILocator Property(ILocator sheet, string label) =>
        sheet.Locator(".property").Filter(new() { Has = sheet.Page.Locator("dt", new() { HasTextRegex = new($"^{System.Text.RegularExpressions.Regex.Escape(label)}$") }) }).Locator("dd");

    private async Task OpenTheReportAsync()
    {
        await Page.GotoAsync("/", Idle);
        await Page.EvaluateAsync(
            """
            async ([key, json]) => {
                const module = await import('/_content/Tellurian.Trains.Schedules.Planning.Components/js/storage.js');
                await module.set(key, json);
            }
            """,
            new[] { ScheduleStorageKey, PlanJsonText() });
        await Page.GotoAsync("/operation-locations-report", Idle);
        await Expect(Page.Locator(".location-sheet").First).ToBeVisibleAsync();
    }

    // Bro (signal controlled, controlled from Munkeröd) — Munkeröd (manned, three tracks) — Rubjerg (unmanned,
    // cargo served from and lock key held at Munkeröd).
    private static string PlanJsonText()
    {
        var plan = PlanFactory.CreatePlan("Operation locations", "en");
        var layout = plan.Timetable!.Layout;
        var munkerod = (Station)layout.Add(new Station(1, "Munkeröd", "Mkd") { IsManned = true, Owner = "Anna" });
        munkerod.Timings.MinimumStopMinutes = 4;
        munkerod.Add(layout.Regions.First());
        munkerod.Instructions = "## Shunting\n\nThe goods shed is shunted from track 3.";
        var rubjerg = (Station)layout.Add(new Station(2, "Rubjerg", "Rbj") { IsManned = false });
        var bro = (SignalControlledLocation)layout.Add(new SignalControlledLocation(3, "Bro", "Bro"));
        bro.ControlledBy = munkerod;
        rubjerg.CargoServedFrom = munkerod;
        rubjerg.LockKey = new LockKey { Name = "Key R", HeldAt = munkerod };

        munkerod.Add(new StationTrack(11, "1") { DisplayOrder = 1, IsMain = true, PlatformLength = 120 });
        munkerod.Add(new StationTrack(12, "2") { DisplayOrder = 2, PlatformLength = 80, PreviousLocationId = 3, NextLocationId = 2 });
        munkerod.Add(new StationTrack(13, "3") { DisplayOrder = 3, IsScheduled = false, Usage = "Goods shed" });
        rubjerg.Add(new StationTrack(21, "1") { DisplayOrder = 1, IsMain = true });
        bro.Add(new StationTrack(31, "1") { DisplayOrder = 1, IsMain = true });
        layout.Add(new TrackStretch(1, bro, munkerod, 10));
        layout.Add(new TrackStretch(2, munkerod, rubjerg, 10));

        return JsonSerializer.Serialize(plan, PlanJson.CreateOptions());
    }
}
