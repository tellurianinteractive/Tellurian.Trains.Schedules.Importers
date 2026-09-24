using System.Text.Json;
using Microsoft.Playwright;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Notes;
using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Timetables;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for the remark on a call in the Trains tab: which half of the call a note may be
/// written for, and that a call the train runs past is offered none.
/// </summary>
[TestClass]
public sealed class CallNoteUiTests : PlaywrightTestBase
{
    private const string ScheduleStorageKey = "planning.schedule.v2";

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task A_remark_is_offered_for_the_halves_of_the_call_the_train_makes()
    {
        await OpenTheTrainsCallsAsync();
        var rows = Page.Locator("table.sub-grid tbody tr");
        await Expect(rows).ToHaveCountAsync(4);

        // The origin: the train starts here, so there is only a departure to write for.
        var origin = rows.Nth(0).Locator("td.note select.target");
        await Expect(origin).ToBeDisabledAsync();
        await Expect(origin).ToHaveValueAsync("Departure");
        await Expect(origin.Locator("option")).ToHaveCountAsync(1);

        // An intermediate stop: both halves, and a note goes to the departure until it is said otherwise.
        var stop = rows.Nth(1).Locator("td.note select.target");
        await Expect(stop).ToBeEnabledAsync();
        await Expect(stop.Locator("option")).ToHaveCountAsync(2);
        await Expect(stop).ToHaveValueAsync("Departure");

        // A pass-through: the train stands for neither half, so there is no note to be written.
        await Expect(rows.Nth(2).Locator("td.note select.target")).ToHaveCountAsync(0);
        await Expect(rows.Nth(2).Locator("td.note span.target.none")).ToBeVisibleAsync();
        await Expect(rows.Nth(2).Locator("td.note .rendered.readonly")).ToBeVisibleAsync();

        // The terminus: the train ends here, so there is only an arrival to write for.
        var terminus = rows.Nth(3).Locator("td.note select.target");
        await Expect(terminus).ToBeDisabledAsync();
        await Expect(terminus).ToHaveValueAsync("Arrival");

        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("call-notes.png"), FullPage = true });
    }

    [TestMethod]
    public async Task A_remark_keeps_the_half_it_was_written_for()
    {
        await OpenTheTrainsCallsAsync();
        var note = Page.Locator("table.sub-grid tbody tr").Nth(1).Locator("td.note");

        await note.Locator(".rendered").ClickAsync();
        await note.Locator("input").FillAsync("Wait for the branch train");
        await note.Locator("input").PressAsync("Enter");
        await Expect(note.Locator(".callnote")).ToHaveTextAsync("Wait for the branch train");

        await note.Locator("select.target").SelectOptionAsync(new SelectOptionValue { Value = "Arrival" });

        // Read back from the stored plan, so what is checked is what was written and not what was typed.
        // The app saves on a debounce, so the reload waits for both the text and the half it was given
        // to have reached the store.
        await WaitForStoredPlanAsync("Wait for the branch train", "\"IsForArrival\":true");
        await Page.GotoAsync("/trains", Idle);
        await ExpandTheTrainAsync();
        var stored = Page.Locator("table.sub-grid tbody tr").Nth(1).Locator("td.note");
        await Expect(stored.Locator(".callnote")).ToHaveTextAsync("Wait for the branch train");
        await Expect(stored.Locator("select.target")).ToHaveValueAsync("Arrival");
    }

    private async Task OpenTheTrainsCallsAsync()
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
        await Page.GotoAsync("/trains", Idle);
        await ExpandTheTrainAsync();
    }

    // The plan is written to the browser store on a debounce, so an edit is only in it a moment later.
    // Polled from here rather than in the page, so the promise the store answers with is really awaited.
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

    // Both the category and the train are collapsed when the tab opens; the calls are under the train.
    private async Task ExpandTheTrainAsync()
    {
        await Page.Locator("tr.category button.toggle").First.ClickAsync();
        await Page.Locator("tr.train button.toggle").First.ClickAsync();
        await Expect(Page.Locator("table.sub-grid")).ToBeVisibleAsync();
    }

    // A train over four locations: it starts at the first, stops at the second, runs past the third and
    // ends at the fourth — one call for each half a note can be written for, and one for neither.
    private static string PlanJsonText()
    {
        var plan = PlanFactory.CreatePlan("Call notes", "en");
        var timetable = plan.Timetable!;
        var layout = timetable.Layout;
        var origin = layout.Add(NewStation(1, "Munkeröd", "Mkd"));
        var stop = layout.Add(NewStation(2, "Slokärr", "Slk"));
        var passed = layout.Add(NewStation(3, "Ytterby", "Yb"));
        var terminus = layout.Add(NewStation(4, "Stenungsund", "Snu"));
        layout.Add(new TrackStretch(1, origin, stop, 10));
        layout.Add(new TrackStretch(2, stop, passed, 10));
        layout.Add(new TrackStretch(3, passed, terminus, 10));

        var train = new Train(1, 1234);
        var time = Time.FromHourAndMinute(8, 0);
        foreach (var location in new[] { origin, stop, passed, terminus })
        {
            train.Add(new StationCall(location.Id, location["1"], time, time.AddMinutes(2)));
            time = time.AddMinutes(20);
        }
        // Only the middle stop is set by hand: the origin's departure and the terminus's arrival are
        // required of the train's own run, and the third call is left as a pass-through.
        train.Calls[1].IsArrival = true;
        train.Calls[1].IsDeparture = true;
        timetable.Add(train);
        plan.Reconcile();

        return JsonSerializer.Serialize(plan, PlanJson.CreateOptions());
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }
}
