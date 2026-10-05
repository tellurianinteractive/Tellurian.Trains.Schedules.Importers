using System.Text.Json;
using Microsoft.Playwright;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Timetables;
using static Microsoft.Playwright.Assertions;

namespace Tellurian.Trains.Schedules.Planning.App.Tests;

/// <summary>
/// End-to-end cover for the language reports are printed in: the layout's default language rather than the
/// user-interface language, and with local languages chosen, each station's dispatch list in the language of
/// the station's country. The interface stays English throughout, so any Swedish or Danish on the sheets can
/// only have come from the report's own language — which also proves the other languages' translations are
/// loaded in the browser, where Blazor loads only the interface language's by itself.
/// </summary>
[TestClass]
public sealed class ReportLanguageUiTests : PlaywrightTestBase
{
    private const string ScheduleStorageKey = "planning.schedule.v2";
    private const int Denmark = 3;

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task Reports_are_printed_in_the_layouts_default_language()
    {
        await OpenTheDispatchReportAsync(useLocalLanguages: false);

        var sheets = Page.Locator(".dispatchlist");
        await Expect(sheets).ToHaveCountAsync(2);
        await Expect(sheets.Nth(0).Locator("th.track")).ToHaveTextAsync("Spår");
        await Expect(sheets.Nth(1).Locator("th.track")).ToHaveTextAsync("Spår");
        // The toolbar is interface, so it keeps the user's language.
        await Expect(Page.Locator(".report-toolbar button").First).ToHaveTextAsync("Print");
    }

    [TestMethod]
    public async Task With_local_languages_each_station_is_printed_in_the_language_of_its_country()
    {
        await OpenTheDispatchReportAsync(useLocalLanguages: true);

        var sheets = Page.Locator(".dispatchlist");
        await Expect(sheets).ToHaveCountAsync(2);
        await Expect(sheets.Nth(0).Locator("th.track")).ToHaveTextAsync("Spår");
        await Expect(sheets.Nth(1).Locator("th.track")).ToHaveTextAsync("Spor");
        // Drawn by a shared component rather than the sheet's own markup, so this is what shows it too
        // follows its sheet — once, the Swedish sheet borrowed the Danish word from the sheet after it.
        await Expect(sheets.Nth(0).Locator("td.sessions")).ToHaveTextAsync("Alla");
        await Expect(sheets.Nth(1).Locator("td.sessions")).ToHaveTextAsync("Alle");
        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("report-local-languages.png"), FullPage = true });

        // Leaving the report puts the interface back in the user's language.
        await Page.Locator(".report-toolbar button").Nth(1).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Print" })).ToHaveCountAsync(0);
        await Expect(Page.Locator("body")).Not.ToContainTextAsync("Spår", new() { Timeout = 2000 });
    }

    private async Task OpenTheDispatchReportAsync(bool useLocalLanguages)
    {
        await Page.GotoAsync("/", Idle);
        await Page.EvaluateAsync(
            """
            async ([key, json]) => {
                const module = await import('/_content/Tellurian.Trains.Schedules.Planning.Components/js/storage.js');
                await module.set(key, json);
            }
            """,
            new[] { ScheduleStorageKey, PlanJsonText(useLocalLanguages) });
        await Page.GotoAsync("/station-dispatch-report", Idle);
        await Expect(Page.Locator(".dispatchlist").First).ToBeVisibleAsync();
    }

    // A Swedish layout: Munkeröd, in the default country, and Stilkøbing, in Denmark, both manned, with one
    // train between them so each has a list to print.
    private static string PlanJsonText(bool useLocalLanguages)
    {
        var plan = PlanFactory.CreatePlan("Report languages", "sv");
        var timetable = plan.Timetable!;
        var layout = timetable.Layout;
        layout.Settings.General.UseObjectLanguageInReports = useLocalLanguages;
        var munkerod = (Station)layout.Add(NewStation(1, "Munkeröd", "Mkd"));
        var stilkobing = (Station)layout.Add(NewStation(2, "Stilkøbing", "Stk"));
        stilkobing.CountryId = Denmark;
        layout.Add(new TrackStretch(1, munkerod, stilkobing, 10));

        var train = new Train(1, 101) { Category = timetable.TrainCategories.First(c => c.IsPassenger) };
        var departure = Time.FromHourAndMinute(6, 0);
        train.Add(new StationCall(11, munkerod["1"], departure.AddMinutes(-10), departure));
        var last = train.Add(new StationCall(12, stilkobing["1"], departure.AddMinutes(20), departure.AddMinutes(25)));
        last.IsArrival = true;
        timetable.Add(train);
        plan.Reconcile();

        return JsonSerializer.Serialize(plan, PlanJson.CreateOptions());
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1") { DisplayOrder = 1 });
        return station;
    }
}
