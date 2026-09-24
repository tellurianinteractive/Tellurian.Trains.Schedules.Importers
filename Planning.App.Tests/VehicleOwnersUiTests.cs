using System.Text.Json;
using Microsoft.Playwright;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Timetables;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for the Vehicle Owners tab: where a vehicle is to stand, picking an owner by typing part of
/// a name, the DCC address a traction unit cannot be brought without, and a participant's name kept in one place.
/// </summary>
[TestClass]
public sealed class VehicleOwnersUiTests : PlaywrightTestBase
{
    private const string ScheduleStorageKey = "planning.schedule.v2";

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task An_owner_is_picked_by_typing_part_of_a_name_and_a_locomotive_needs_a_DCC_address()
    {
        await OpenTheTabAsync(PlanJsonText(withOwner: false));

        // The locomotive works the 08:05 from Munkeröd track 1 on session 1, which is where it is to stand.
        var loco = Page.Locator("table.owners tr.vehicle", new() { HasText = "01 Rc" });
        await Expect(loco.Locator("td.first-session svg.sessionnumber")).ToHaveAttributeAsync("aria-label", "1");
        await Expect(loco).ToContainTextAsync("Munkeröd");
        await Expect(loco).ToContainTextAsync("08:05");
        var wagons = Page.Locator("table.owners tr.vehicle", new() { HasText = "02 B" });
        await Expect(wagons).ToContainTextAsync("Not in operation");

        // A count of units is shown only where there is more than one.
        await Expect(Page.Locator("table.owners th.units")).ToHaveTextAsync("Count");
        await Expect(loco.Locator("td.units")).ToHaveTextAsync("");
        await Expect(wagons.Locator("td.units")).ToHaveTextAsync("3");

        await loco.Locator("button.toggle").ClickAsync();
        var add = Page.Locator("table.sub-grid tr.add");
        var name = add.Locator(".participant-combo input");

        // A surname finds its owner, and a name matching nobody is offered as a new participant — last.
        await name.FillAsync("berg");
        var choices = add.Locator(".participant-combo li[role=option]");
        await Expect(choices).ToHaveCountAsync(2);
        await Expect(choices.First).ToHaveTextAsync("Anna Berg");
        await Expect(choices.Last).ToContainTextAsync("New participant: berg");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("vehicle-owners-choices.png") });
        await name.PressAsync("Enter");
        await Expect(name).ToHaveValueAsync("Anna Berg");

        // Not without a DCC address.
        var addOwner = add.Locator("button", new() { HasText = "Add owner" });
        await addOwner.ClickAsync();
        await Expect(Page.Locator(".sub p.error")).ToContainTextAsync("DCC address");
        await Expect(Page.Locator("table.sub-grid tbody tr:not(.add) .participant-combo")).ToHaveCountAsync(0);

        // Zero: the owner is to provide it.
        await add.Locator("input.dcc").FillAsync("0");
        await addOwner.ClickAsync();
        var owner = Page.Locator("table.sub-grid tbody tr").First;
        await Expect(owner.Locator("td.role")).ToHaveTextAsync("Primary");
        await Expect(owner.Locator(".participant-combo input")).ToHaveValueAsync("Anna Berg");
        await Expect(owner).ToContainTextAsync("Owner to provide");
        await Expect(loco).ToContainTextAsync("Anna Berg");

        await WaitForStoredPlanAsync("\"DccAddress\":0");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("vehicle-owners.png"), FullPage = true });
    }

    [TestMethod]
    public async Task Correcting_a_participant_name_corrects_it_wherever_it_appears()
    {
        await OpenTheTabAsync(PlanJsonText(withOwner: true));

        await Page.Locator(".sub-tab", new() { HasText = "Participants" }).ClickAsync();
        var anna = Page.Locator("table.participants tbody tr", new() { HasText = "02 B" });
        await Expect(anna.Locator("button", new() { HasText = "Delete" })).ToBeDisabledAsync();

        var nameField = anna.Locator("input.name");
        await nameField.FillAsync("Anna Bergström");
        await nameField.PressAsync("Tab");
        // The stored JSON escapes the "ö", so the name is looked for up to it.
        await WaitForStoredPlanAsync("\"Anna Bergstr");

        await Page.Locator(".sub-tab", new() { HasText = "Rolling stock" }).ClickAsync();
        await Expect(Page.Locator("table.owners tr.vehicle", new() { HasText = "02 B" })).ToContainTextAsync("Anna Bergström");
    }

    private async Task OpenTheTabAsync(string planJson)
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
            new[] { ScheduleStorageKey, planJson });
        await Page.GotoAsync("/vehicle-owners", Idle);
        await Expect(Page.Locator("table.owners")).ToBeVisibleAsync();
    }

    // The plan is written to the browser store on a debounce, so an edit is only in it a moment later.
    private async Task WaitForStoredPlanAsync(params string[] texts)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            var json = await Page.EvaluateAsync<string?>(
                """
                async key => {
                    const module = await import('/_content/Tellurian.Trains.Schedules.Planning.Components/js/storage.js');
                    return await module.get(key);
                }
                """,
                ScheduleStorageKey);
            if (json is not null && texts.All(json.Contains)) return;
            await Task.Delay(200);
        }
        Assert.Fail($"The plan was not stored with {string.Join(" and ", texts)} within ten seconds.");
    }

    // A locomotive working one train from Munkeröd to Stenungsund, a wagonset of three given no work yet, and two
    // participants; with an owner, Anna Berg brings the wagonset.
    private static string PlanJsonText(bool withOwner)
    {
        var plan = PlanFactory.CreatePlan("Vehicle owners", "en");
        var timetable = plan.Timetable!;
        var layout = timetable.Layout;
        var origin = layout.Add(NewStation(1, "Munkeröd", "Mkd"));
        var terminus = layout.Add(NewStation(2, "Stenungsund", "Snu"));
        layout.Add(new TrackStretch(1, origin, terminus, 10));

        var train = new Train(1, 1234) { Category = timetable.TrainCategories.First() };
        train.Add(new StationCall(1, origin["1"], Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 5)));
        train.Add(new StationCall(2, terminus["1"], Time.FromHourAndMinute(8, 30), Time.FromHourAndMinute(8, 35)));
        timetable.Add(train);
        plan.Reconcile();

        var schedule = plan.CreateSchedule();
        schedule.Append(train.AsTrainPart);
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Rc", 1, null);
        plan.AssignVehicle(schedule, loco);
        var wagons = plan.CreateVehicle(ScheduledObjectType.Wagonset, "B", 2, null);
        wagons.NumberOfUnits = 3;
        var anna = plan.FindOrAddParticipant("Anna Berg")!;
        plan.FindOrAddParticipant("Bert Andersson");
        if (withOwner) plan.AddContributor(wagons, anna, null);

        return JsonSerializer.Serialize(plan, PlanJson.CreateOptions());
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }
}
