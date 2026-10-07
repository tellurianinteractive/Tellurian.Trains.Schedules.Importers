using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Timetables;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for the Vehicle contributors report: the three arrangements, a station or owner always
/// starting a page of their own, and — since the pages are fitted by estimate rather than measured — no page
/// running past the foot of the sheet.
/// </summary>
[TestClass]
public sealed class VehicleContributorsReportUiTests : PlaywrightTestBase
{
    private const string ScheduleStorageKey = "planning.schedule.v2";
    private const int LocomotiveCount = 30;

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task Each_arrangement_starts_every_station_or_owner_on_a_page_and_no_page_overflows()
    {
        await OpenTheReportAsync();

        // By operation location: thirty locomotives set up at Munkeröd, and a spare, take two pages, then Stenungsund's one.
        var headings = Page.Locator(".contributors-page h1");
        await Expect(headings).ToHaveCountAsync(4);
        await Expect(headings.Nth(0)).ToHaveTextAsync("Munkeröd");
        await Expect(headings.Nth(1)).ToHaveTextAsync(Continued("Munkeröd"));
        await Expect(headings.Nth(2)).ToHaveTextAsync("Stenungsund");
        await Expect(headings.Nth(3)).ToHaveTextAsync("Not in operation");
        await Expect(Page.Locator(".contributors-page").First.Locator("th")).ToHaveTextAsync(
            ["First session", "Track", "Departure", "Type", "Turnus", "Count", "Owner", "Role", "DCC", "Note"]);
        // Bert's spare of the first locomotive on a row of its own right below Anna's, shaded grey as a spare.
        var munkerod = Page.Locator(".contributors-page").First.Locator("tbody tr");
        await Expect(munkerod.Nth(0).Locator("td.role")).ToHaveTextAsync("Primary");
        await Expect(munkerod.Nth(1).Locator("td.owner")).ToHaveTextAsync("Bert Andersson");
        await Expect(munkerod.Nth(1).Locator("td.role")).ToHaveTextAsync("Spare");
        await Expect(munkerod.Nth(1)).ToHaveCSSAsync("background-color", "rgb(227, 227, 227)");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("vehicle-contributors-by-location.png"), FullPage = true });
        await AssertNoPageOverflowsAsync();

        // By owner: what nobody has booked first; Anna brings all thirty locomotives; Bert the wagonset he sets up
        // himself before his spare.
        await Page.Locator(".report-options a", new() { HasText = "By owner" }).ClickAsync();
        await Expect(headings.Nth(0)).ToHaveTextAsync("Not yet booked");
        await Expect(headings.Nth(1)).ToHaveTextAsync("Anna Berg");
        await Expect(headings.Nth(2)).ToHaveTextAsync(Continued("Anna Berg"));
        await Expect(headings.Nth(3)).ToHaveTextAsync("Bert Andersson");
        var bert = Page.Locator(".contributors-page").Nth(3).Locator("tbody tr");
        await Expect(bert.Locator("td.role")).ToHaveTextAsync(["Primary", "Spare"]);
        // The wagonset takes no address; the spare locomotive's is still to be provided, which is missing, in red.
        await Expect(bert.Locator("td.dcc-address")).ToHaveTextAsync(["", "Missing"]);
        await Expect(bert.Last.Locator("td.dcc-address")).ToHaveCSSAsync("color", "rgb(192, 0, 0)");
        await Expect(bert.Last.Locator("td.station")).ToHaveTextAsync("Munkeröd");
        // A spare is shaded grey; a locomotive in operation on every session stays white.
        await Expect(bert.Last).ToHaveCSSAsync("background-color", "rgb(227, 227, 227)");
        await Expect(Page.Locator(".contributors-page").Nth(1).Locator("tbody tr").First).ToHaveCSSAsync("background-color", "rgb(255, 255, 255)");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("vehicle-contributors-by-owner.png"), FullPage = true });
        await AssertNoPageOverflowsAsync();

        // By DCC address: one list of every unit, lowest address first and the missing one after.
        await Page.Locator(".report-options a", new() { HasText = "By DCC address" }).ClickAsync();
        await Expect(headings.First).ToHaveTextAsync("DCC addresses");
        var addresses = await Page.Locator("table.contributors td.dcc-address").AllInnerTextsAsync();
        Assert.AreEqual("1", addresses[0]);
        Assert.AreEqual("Missing", addresses[LocomotiveCount]);
        // Only what a DCC address applies to: thirty locomotives and Bert's spare, and no wagonset.
        Assert.AreEqual(LocomotiveCount + 1, addresses.Count);
        await Expect(Page.Locator("table.contributors td.type", new() { HasText = "Wagonset" })).ToHaveCountAsync(0);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("vehicle-contributors-by-dcc.png"), FullPage = true });
        await AssertNoPageOverflowsAsync();
    }

    // The heading of a group's later page; how much space separates the name from the marker is styling.
    private static Regex Continued(string name) => new($@"^{Regex.Escape(name)}\s*\(continued\)$");

    private async Task OpenTheReportAsync()
    {
        // The app boots on its own plan, so the one under test is put in the store it reads.
        await Page.GotoAsync("/", Idle);
        await Page.EvaluateAsync(
            """
            async ([key, json]) => {
                const module = await import('/_content/Tellurian.Trains.Schedules.Planning.Components/js/storage.js');
                await module.set(key, json);
            }
            """,
            new[] { ScheduleStorageKey, PlanJsonText() });
        await Page.GotoAsync("/vehicle-contributors-report", Idle);
        await Expect(Page.Locator(".contributors-page").First).ToBeVisibleAsync();
    }

    // The table on every sheet must end within the sheet's margins: what runs past them on screen runs past the
    // edge of the paper. And only the notes wrap, so every other cell is one line, as wide as its column: a text,
    // heading or value, wider than the room inside its cell has been cut short.
    private async Task AssertNoPageOverflowsAsync()
    {
        var cutShort = await Page.EvaluateAsync<string[]>(
            """
            () => [...document.querySelectorAll('table.contributors th, table.contributors td:not(.note)')]
                .filter(cell => {
                    const range = document.createRange();
                    range.selectNodeContents(cell);
                    const style = getComputedStyle(cell);
                    const room = cell.clientWidth - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight);
                    return range.getBoundingClientRect().width > room + 0.5;
                })
                .map(cell => `${cell.className}: ${cell.textContent.trim()}`)
            """);
        Assert.AreEqual(0, cutShort.Length, $"Cut short: {string.Join(", ", cutShort)}");

        var overflows = await Page.EvaluateAsync<string[]>(
            """
            () => [...document.querySelectorAll('.a4l')].flatMap((page, index) => {
                const box = page.getBoundingClientRect();
                const style = getComputedStyle(page);
                const bottom = box.bottom - parseFloat(style.paddingBottom);
                const right = box.right - parseFloat(style.paddingRight);
                const table = page.querySelector('table.contributors').getBoundingClientRect();
                return [
                    ...(table.bottom > bottom + 0.5 ? [`page ${index + 1} at the foot by ${(table.bottom - bottom).toFixed(1)} px`] : []),
                    ...(table.right > right + 0.5 ? [`page ${index + 1} at the side by ${(table.right - right).toFixed(1)} px`] : []),
                ];
            })
            """);
        Assert.AreEqual(0, overflows.Length, $"Overflowing: {string.Join(", ", overflows)}");
    }

    // Thirty locomotives each working a train from Munkeröd, and a wagonset working one from Stenungsund. Anna Berg
    // brings every locomotive, addresses 1 to 30; Bert Andersson a spare of the first, his address to come; nobody
    // brings an unassigned wagonset. Notes are short, as they are expected to be.
    private static string PlanJsonText()
    {
        var plan = PlanFactory.CreatePlan("Vehicle contributors", "en");
        var timetable = plan.Timetable!;
        var layout = timetable.Layout;
        var munkerod = layout.Add(NewStation(1, "Munkeröd", "Mkd"));
        var stenungsund = layout.Add(NewStation(2, "Stenungsund", "Snu"));
        layout.Add(new TrackStretch(1, munkerod, stenungsund, 10));

        var trains = Enumerable.Range(1, LocomotiveCount + 1)
            .Select(number =>
            {
                var (from, to) = number <= LocomotiveCount ? (munkerod, stenungsund) : (stenungsund, munkerod);
                var departure = Time.FromHourAndMinute(6, 0).AddMinutes(number * 10);
                var train = new Train(number, 1000 + number) { Category = timetable.TrainCategories.First() };
                train.Add(new StationCall(number * 10 + 1, from["1"], departure.AddMinutes(-5), departure));
                train.Add(new StationCall(number * 10 + 2, to["1"], departure.AddMinutes(20), departure.AddMinutes(25)));
                timetable.Add(train);
                return train;
            })
            .ToList();
        plan.Reconcile();

        var anna = plan.FindOrAddParticipant("Anna Berg")!;
        var bert = plan.FindOrAddParticipant("Bert Andersson")!;
        for (var number = 1; number <= LocomotiveCount; number++)
        {
            var loco = Assigned(plan, ScheduledObjectType.Locomotive, "Rc", number, trains[number - 1]);
            plan.AddContributor(loco, anna, number);
            if (number % 7 == 0)
                plan.SetContributionNote(loco, "Brings its own sound decoder");
            if (number == 1) plan.AddContributor(loco, bert, DccAddresses.ToBeProvided, "Arrives on Saturday");
        }
        var goods = Assigned(plan, ScheduledObjectType.Wagonset, "Gbs", LocomotiveCount + 1, trains[^1]);
        plan.AddContributor(goods, bert, null);
        plan.CreateVehicle(ScheduledObjectType.Wagonset, "B", LocomotiveCount + 2, null).NumberOfUnits = 3;

        return JsonSerializer.Serialize(plan, PlanJson.CreateOptions());
    }

    private static ScheduledObject Assigned(Plan plan, ScheduledObjectType type, string @class, int number, Train train)
    {
        var schedule = plan.CreateSchedule();
        schedule.Append(train.AsTrainPart);
        var vehicle = plan.CreateVehicle(type, @class, number, null);
        plan.AssignVehicle(schedule, vehicle);
        return vehicle;
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }
}
