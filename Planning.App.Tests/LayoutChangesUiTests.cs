using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Playwright;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.PlanFiles;
using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Timetables;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for loading a newer revision of the hall plan a plan was made from: distances are taken at once,
/// and the changes that move or add calls are listed on the Import tab, reviewed and applied one by one.
/// </summary>
[TestClass]
public sealed class LayoutChangesUiTests : PlaywrightTestBase
{
    private const string ScheduleStorageKey = "planning.schedule.v2";
    private static readonly Guid HallPlanId = Guid.Parse("60000000-0000-0000-0000-000000000006");

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task The_changes_of_a_newer_hall_plan_are_reviewed_and_applied_one_by_one()
    {
        await OpenImportTabAsync(PlanJsonText());
        await OpenNewerRevisionAsync();

        var choice = Page.Locator(".revision-choice");
        await Expect(choice).ToContainTextAsync("The file is revision 2 of the layout the open plan 'Club meeting' was made from; the plan has revision 1.");
        await choice.Locator("button", new() { HasText = "Update the open plan" }).ClickAsync();

        await Expect(Page.Locator(".import-messages")).ToContainTextAsync("Hall plan from Module Draw, revision 2: 1 changes applied, 2 to confirm");
        var changes = Page.Locator(".layout-changes");
        await Expect(changes.Locator("h3")).ToHaveTextAsync("Changes from Hall plan from Module Draw, revision 2");
        var rows = changes.Locator("table.changes tr");
        await Expect(rows).ToHaveCountAsync(2);
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("layout-changes.png"), FullPage = true });

        // The track the train stops at goes: its call moves to the other track.
        var trackRemoved = rows.Filter(new() { HasText = "Ytterby: track 2 is removed" });
        await trackRemoved.Locator("button", new() { HasText = "Review" }).ClickAsync();
        var review = Page.Locator(".layout-change-review");
        await Expect(review).ToContainTextAsync("These calls move to another track:");
        await Expect(review).ToContainTextAsync("to track 1");
        await review.Locator("button", new() { HasText = "Apply" }).ClickAsync();
        await Expect(rows).ToHaveCountAsync(1);

        // A location on the stretch the train runs: the train passes it.
        await rows.Filter(new() { HasText = "Kärra is inserted between Ytterby and Stenungsund" }).Locator("button", new() { HasText = "Review" }).ClickAsync();
        await Expect(review).ToContainTextAsync("These trains will get a call passing 'Kärra':");
        await review.Locator("button", new() { HasText = "Apply" }).ClickAsync();

        await Expect(changes).ToContainTextAsync("All changes are applied.");
        await WaitForStoredPlanAsync("\"Kärra\"");
        await changes.Locator("button", new() { HasText = "Close" }).ClickAsync();
        await Expect(changes).ToHaveCountAsync(0);
    }

    [TestMethod]
    public async Task A_newer_hall_plan_can_be_made_a_new_plan_instead()
    {
        await OpenImportTabAsync(PlanJsonText());
        await OpenNewerRevisionAsync();

        await Page.Locator(".revision-choice button", new() { HasText = "Make a new plan" }).ClickAsync();

        await Expect(Page.Locator(".import-messages")).ToContainTextAsync("Hall plan from Module Draw: new layout made with 4 operation locations and 3 track stretches.");
        await Expect(Page.Locator(".schedule-summary")).ToContainTextAsync("0 Trains");
        await Expect(Page.Locator(".layout-changes")).ToHaveCountAsync(0);
    }

    private Task OpenNewerRevisionAsync() =>
        Page.SetInputFilesAsync("input[type=file]", new FilePayload
        {
            Name = "meeting.json",
            MimeType = "application/json",
            Buffer = HallPlanFile(Newer()),
        });

    private async Task OpenImportTabAsync(string planJson)
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
        await Page.GotoAsync("/import", Idle);
        await Expect(Page.Locator(".schedule-summary")).ToBeVisibleAsync();
    }

    private async Task WaitForStoredPlanAsync(string text)
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
            // The stored JSON escapes non-ASCII letters, so both forms are looked for.
            if (json is not null && (json.Contains(text) || json.Contains(JsonSerializer.Serialize(text.Trim('"'))))) return;
            await Task.Delay(200);
        }
        Assert.Fail($"The plan was not stored with {text} within ten seconds.");
    }

    // Tellby – Ytterby – Stenungsund, two tracks each; revision 1 of the hall plan.
    private static Layout HallPlan(int revision, Action<Layout>? change = null)
    {
        var layout = new Layout { Name = "Club meeting", SourceId = HallPlanId, SourceRevision = revision };
        Add(layout, 1, "Tellby", "Tby");
        Add(layout, 2, "Ytterby", "Yb");
        Add(layout, 3, "Stenungsund", "Snu");
        Connect(layout, 1, 1, 2, 10);
        Connect(layout, 2, 2, 3, 10);
        change?.Invoke(layout);
        return layout;
    }

    // Revision 2: the stretch Tellby – Ytterby is longer, Ytterby has lost track 2, and Kärra lies between Ytterby and
    // Stenungsund.
    private static Layout Newer() => HallPlan(2, layout =>
    {
        layout.TrackStretches.Single(s => s.Id == 1).Distance = 14;
        var ytterby = layout.OperationLocations.Single(l => l.Id == 2);
        ytterby.Tracks.Remove(ytterby.Tracks.Single(t => t.Designation == "2"));
        layout.TrackStretches.Remove(layout.TrackStretches.Single(s => s.Id == 2));
        Add(layout, 4, "Kärra", "Kä");
        Connect(layout, 2, 2, 4, 4);
        Connect(layout, 3, 4, 3, 6);
    });

    private static void Add(Layout layout, int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true, Layout = layout };
        station.Tracks.Add(new StationTrack(id * 10 + 1, "1") { Station = station, IsMain = true, PlatformLength = 100 });
        station.Tracks.Add(new StationTrack(id * 10 + 2, "2") { Station = station, PlatformLength = 100 });
        layout.OperationLocations.Add(station);
    }

    private static void Connect(Layout layout, int id, int from, int to, double distance) =>
        layout.TrackStretches.Add(new TrackStretch(id, layout.OperationLocations.Single(l => l.Id == from), layout.OperationLocations.Single(l => l.Id == to), distance) { Layout = layout });

    // A plan made from revision 1, with a train Tellby – Ytterby (track 2) – Stenungsund.
    private static string PlanJsonText()
    {
        var plan = PlanFactory.CreatePlan(HallPlan(1), "en");
        var timetable = plan.Timetable!;
        var locations = plan.Layout.OperationLocations.ToList();
        var train = new Train(1, 1234) { Category = timetable.TrainCategories.First() };
        train.Add(new StationCall(1, locations[0]["1"], Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 0)));
        train.Add(new StationCall(2, locations[1]["2"], Time.FromHourAndMinute(8, 10), Time.FromHourAndMinute(8, 12)));
        train.Add(new StationCall(3, locations[2]["1"], Time.FromHourAndMinute(8, 22), Time.FromHourAndMinute(8, 22)));
        timetable.Add(train);
        plan.Reconcile();
        return PlanFile.Write(plan);
    }

    private static byte[] HallPlanFile(Layout layout)
    {
        var file = new JsonObject
        {
            ["format"] = Model.Drawings.HallPlanFile.Format,
            ["kind"] = Model.Drawings.HallPlanFile.Kind,
            ["schemaVersion"] = Model.Drawings.HallPlanFile.NewestSchemaVersion,
            ["content"] = new JsonObject { ["layout"] = JsonNode.Parse(JsonSerializer.Serialize(layout, Layout.JsonSerializerOptions)) },
        };
        return Encoding.UTF8.GetBytes(file.ToJsonString());
    }
}
