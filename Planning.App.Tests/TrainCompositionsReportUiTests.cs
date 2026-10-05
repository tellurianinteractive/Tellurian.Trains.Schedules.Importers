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
/// End-to-end cover for the Train compositions report: a page per manned station with departures to show, the
/// departures track by track, wagons and cargo positions drawn as rectangles — and, since the pages are fitted
/// by estimate rather than measured, no page running past the foot of the sheet.
/// </summary>
[TestClass]
public sealed class TrainCompositionsReportUiTests : PlaywrightTestBase
{
    private const string ScheduleStorageKey = "planning.schedule.v2";
    private const int FreightTrainCount = 20;

    private static PageGotoOptions Idle => new() { WaitUntil = WaitUntilState.NetworkIdle };

    [TestMethod]
    public async Task Each_manned_station_lists_its_departures_as_rectangles_and_no_page_overflows()
    {
        await OpenTheReportAsync();

        // Munkeröd and Stilkøbing, the manned stations; Rubjerg and Vig get no pages. Munkeröd's track 1 fills
        // its first page and track 2 its second, and Stilkøbing starts on a page of its own, continued over the
        // next because every freight also arrives there with wagons to uncouple. Every page is followed by its
        // mirror image, for the reader on the other side of the tracks.
        var headings = Page.Locator(".compositions-page h1");
        await Expect(headings).ToHaveCountAsync(8);
        await Expect(headings.Nth(0)).ToHaveTextAsync("Munkeröd");
        await Expect(headings.Nth(1)).ToHaveTextAsync("Munkeröd");
        await Expect(headings.Nth(2)).ToHaveTextAsync(new Regex(@"^Munkeröd\s*\(continued\)$"));
        await Expect(headings.Nth(4)).ToHaveTextAsync("Stilkøbing");
        await Expect(headings.Nth(6)).ToHaveTextAsync(new Regex(@"^Stilkøbing\s*\(continued\)$"));

        var first = Page.Locator(".compositions-page").First;
        var mirrored = Page.Locator(".compositions-page").Nth(1);
        // The sessions, the train, its times and its limit are in the loco rectangle, not in columns of their own.
        await Expect(first.Locator("th")).ToHaveCountAsync(3);
        await Expect(first.Locator("th").Nth(0)).ToHaveTextAsync("Track");
        await Expect(first.Locator("th").Nth(1)).ToHaveTextAsync("To/from");
        await Expect(first.Locator("th.composition")).ToContainTextAsync("Composition counted from the loco");
        // The neighbours at either end change places on the mirrored page.
        var ends = await first.Locator("th.composition .end").AllInnerTextsAsync();
        var mirroredEnds = await mirrored.Locator("th.composition .end").AllInnerTextsAsync();
        Assert.AreEqual(ends[0].Trim('◀', '▶', ' '), mirroredEnds[1].Trim('◀', '▶', ' '), $"Ends: {string.Join("|", ends)} / {string.Join("|", mirroredEnds)}");
        Assert.AreEqual(ends[1].Trim('◀', '▶', ' '), mirroredEnds[0].Trim('◀', '▶', ' '));
        // The track is named once, where its departures begin, and again at the top of the next page.
        var tracks = await first.Locator("td.track").AllInnerTextsAsync();
        Assert.AreEqual("1", tracks[0]);
        CollectionAssert.AreEqual(new[] { "1", "2" }, tracks.Where(track => track.Length > 0).ToArray(),
            $"Tracks: {string.Join(",", tracks)}");
        await Expect(Page.Locator(".compositions-page").Nth(2).Locator("td.track").First).ToHaveTextAsync("2");

        // The passenger train leaves first, with its wagonset wagon by wagon in rake order. It starts its run
        // here, so it arrives from nowhere and the arrival is left empty.
        var passenger = first.Locator("tbody tr").First;
        await Expect(passenger.Locator(".loco .times")).ToHaveTextAsync("-05:30");
        // The loco leads, at the end the train travels to: mirrored, it stands at the other end.
        var heading = await passenger.Locator(".groups").GetAttributeAsync("class");
        var mirroredHeading = await mirrored.Locator("tbody tr").First.Locator(".groups").GetAttributeAsync("class");
        Assert.AreNotEqual(heading, mirroredHeading);
        var loco = (await passenger.Locator(".loco").BoundingBoxAsync())!;
        var wagon = (await passenger.Locator(".wagon").First.BoundingBoxAsync())!;
        Assert.AreEqual(heading!.Contains("rightwards"), loco.X > wagon.X, $"The loco leads the train: {heading}");
        // The rake is named in a shaded rectangle at the front of its wagons; nothing captions them.
        await Expect(passenger.Locator(".wagonset .turnus .designation")).ToHaveTextAsync("01 B");
        // Nothing limits the passenger train, and its loco states no limit.
        await Expect(passenger.Locator(".loco .maxload")).ToHaveCountAsync(0, new() { Timeout = 2000 });
        await Expect(first.Locator(".caption")).ToHaveCountAsync(0);
        await Expect(passenger.Locator(".wagon .class")).ToHaveTextAsync(["A", "AB", "B", "B", "BF"]);
        await Expect(passenger.Locator(".wagon").First.Locator(".number")).ToHaveTextAsync("50 74 19-1");
        // Side by side on one line, and each wagon one line high with its number beside its class.
        var wagonTops = await passenger.Locator(".wagon").EvaluateAllAsync<double[]>(
            "wagons => wagons.map(wagon => Math.round(wagon.getBoundingClientRect().top))");
        Assert.AreEqual(1, wagonTops.Distinct().Count(), "The wagons of a short rake stand on one line.");
        var firstWagon = passenger.Locator(".wagon").First;
        var (classBox, numberBox) = (await firstWagon.Locator(".class").BoundingBoxAsync(), await firstWagon.Locator(".number").BoundingBoxAsync());
        Assert.IsTrue(numberBox!.X > classBox!.X + classBox.Width - 0.5, "The number stands beside the class, not beneath it.");

        // A freight train's cargo positions front first, the one without a position last. Nothing names the
        // positions: the order of the rectangles is what says where the wagons go.
        var freight = first.Locator("tbody tr").Nth(1);
        await Expect(freight.Locator(".wagonset .turnus .designation")).ToHaveCountAsync(0, new() { Timeout = 2000 });
        // The train's own limit, last in the loco: at most 24 axles and 12 wagons, each figure with its mark.
        await Expect(freight.Locator(".loco .axles")).ToHaveTextAsync("24");
        await Expect(freight.Locator(".loco .wagons")).ToHaveTextAsync("12");
        await Expect(freight.Locator(".loco > *").Last).ToHaveClassAsync(new Regex("arrow|maxload"));
        await Expect(freight.Locator(".cargo")).ToHaveCountAsync(3);
        var destination = freight.Locator(".cargo .destination").First;
        await Expect(destination).ToContainTextAsync("Stilkøbing, Vig");
        // The region follows every place in the rectangle, as a coloured chip.
        await Expect(freight.Locator(".cargo").First.Locator(".destination").Last.Locator(".region"))
            .ToHaveCSSAsync("background-color", "rgb(204, 0, 0)");
        await Expect(destination.Locator(".region")).ToHaveCountAsync(0, new() { Timeout = 2000 });
        // And the most that may be brought to that destination, after the place it belongs to.
        await Expect(destination.Locator(".wagons")).ToHaveTextAsync("5");
        await Expect(freight.Locator(".cargo").Nth(1).Locator(".destination")).ToHaveTextAsync("Rubjerg and beyond");
        await Expect(freight.Locator(".cargo").Nth(2).Locator(".destination")).ToHaveTextAsync("all destinations");
        // Each rectangle is as wide as what it holds, so the one with the longest destination is the widest.
        var widths = await freight.Locator(".cargo").EvaluateAllAsync<double[]>(
            "boxes => boxes.map(box => box.getBoundingClientRect().width)");
        CollectionAssert.AreEqual(widths.OrderByDescending(width => width).ToArray(), widths,
            $"Widest first, as the destinations are longest first: {string.Join(", ", widths)}");

        // The train that only calls here on its way through shows when it arrives as well as when it leaves;
        // it is on this sheet because it gathers wagons here, not merely because it passes.
        // Its arrival, uncoupling the wagons gathered at Stilkøbing, is a row of its own just before, with the
        // origin of those wagons in a dashed rectangle. It runs on, so it shows its departure time too.
        // Munkeröd's sheets come first; Stilkøbing lists the train once more, where it starts.
        var rows = Page.Locator("tbody tr").Filter(new() { HasTextString = "4800" });
        // Twice over: every page is printed as drawn and mirrored.
        await Expect(rows).ToHaveCountAsync(6);
        var arriving = rows.Nth(0);
        await Expect(arriving.Locator(".loco .times")).ToHaveTextAsync("05:15-05:20");
        await Expect(arriving.Locator("td.to")).ToHaveTextAsync(new Regex(@"^from\s+Stilkøbing$"));
        await Expect(arriving.Locator(".cargo.arriving")).ToHaveTextAsync("Stilkøbing");
        var stopping = rows.Nth(1);
        await Expect(stopping.Locator(".loco .times")).ToHaveTextAsync("05:15-05:20");
        await Expect(stopping.Locator("td.to")).ToHaveTextAsync(new Regex(@"^to\s+Vig$"));


        await Page.ScreenshotAsync(new PageScreenshotOptions { Path = ScreenshotPath("train-compositions.png"), FullPage = true });
        await AssertNoPageOverflowsAsync();
    }

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
        await Page.GotoAsync("/train-compositions-report", Idle);
        await Expect(Page.Locator(".compositions-page").First).ToBeVisibleAsync();
    }

    // The table on every sheet must end within the sheet's margins: what runs past them on screen runs past the
    // edge of the paper. And no rectangle may stand beyond the right edge of its composition.
    private async Task AssertNoPageOverflowsAsync()
    {
        var overflows = await Page.EvaluateAsync<string[]>(
            """
            () => [...document.querySelectorAll('.a4l')].flatMap((page, index) => {
                const box = page.getBoundingClientRect();
                const style = getComputedStyle(page);
                const bottom = box.bottom - parseFloat(style.paddingBottom);
                const table = page.querySelector('table.compositions').getBoundingClientRect();
                const wide = [...page.querySelectorAll('td.composition')].filter(cell =>
                    [...cell.querySelectorAll('.group')].some(group => group.getBoundingClientRect().right > cell.getBoundingClientRect().right + 0.5));
                return [
                    ...(table.bottom > bottom + 0.5 ? [`page ${index + 1} at the foot by ${(table.bottom - bottom).toFixed(1)} px`] : []),
                    ...(wide.length > 0 ? [`page ${index + 1} has ${wide.length} compositions wider than their cell`] : []),
                ];
            })
            """);
        Assert.AreEqual(0, overflows.Length, $"Overflowing: {string.Join(", ", overflows)}");
    }

    // Munkeröd (manned, tracks 1 and 2) → Rubjerg (not manned) → Stilkøbing (manned), with Vig (not manned)
    // beyond Munkeröd the other way. A passenger train leaves Munkeröd track 1 at 05:30 with a wagonset of five
    // wagons. Sixteen freight trains follow every ten minutes, alternately from track 1 and 2, each with wagons
    // at position 1 for Stilkøbing and its local destination Vig and its region, at position 2 for Rubjerg and beyond,
    // and without a position for all destinations. The freights take at most 24 axles and 12 wagons, and at most
    // five wagons may go to Stilkøbing, so both kinds of limit are printed. One more freight runs the other way
    // and only calls at Munkeröd on its way, which is the departure that has an arrival time.
    private static string PlanJsonText()
    {
        var plan = PlanFactory.CreatePlan("Train compositions", "en");
        var timetable = plan.Timetable!;
        var layout = timetable.Layout;
        var munkerod = (Station)layout.Add(NewStation(1, "Munkeröd", "Mkd", manned: true, "1", "2"));
        var rubjerg = (Station)layout.Add(NewStation(2, "Rubjerg", "Rbj", manned: false, "1"));
        var stilkobing = (Station)layout.Add(NewStation(3, "Stilkøbing", "Stk", manned: true, "1"));
        stilkobing.Add(layout.Regions.First(region => region.BackgroundColor == "#CC0000"));
        var vig = (Station)layout.Add(NewStation(4, "Vig", "Vig", manned: false, "1"));
        vig.CargoServedFrom = stilkobing;
        layout.Add(new TrackStretch(1, munkerod, rubjerg, 10));
        layout.Add(new TrackStretch(2, rubjerg, stilkobing, 10));
        layout.Add(new TrackStretch(3, vig, munkerod, 10));

        var toStilkobing = timetable.Add(new CargoFlowOptions { Id = 1 });
        toStilkobing.Destinations.Add(new Destination
        {
            Location = stilkobing,
            AndLocalDestinations = true,
            AndRegions = true,
            MaxNumberOfWagons = 5,
        });
        var toRubjerg = timetable.Add(new CargoFlowOptions { Id = 2 });
        toRubjerg.Destinations.Add(new Destination { Location = rubjerg, AndBeyond = true });
        var toAll = timetable.Add(new CargoFlowOptions { Id = 3, ToAllDestinations = true });

        var passenger = AddTrain(timetable, 1, 101, timetable.TrainCategories.First(c => c.IsPassenger), munkerod["1"], rubjerg, stilkobing, Time.FromHourAndMinute(5, 30));
        for (var i = 1; i <= FreightTrainCount; i++)
        {
            var track = munkerod[i % 2 == 1 ? "1" : "2"];
            var train = AddTrain(timetable, i + 1, 4700 + i, timetable.TrainCategories.First(c => c.IsFreight), track, rubjerg, stilkobing, Time.FromHourAndMinute(6, 0).AddMinutes(i * 10));
            train.Length = new TrainCapacity(24, 12, null);
            var calls = train.CallsInRunOrder;
            train.CreateCargoFlow(1, calls[0], calls[2], toStilkobing, positionInTrain: 1);
            train.CreateCargoFlow(2, calls[0], calls[1], toRubjerg, positionInTrain: 2);
            train.CreateCargoFlow(3, calls[0], calls[2], toAll, positionInTrain: 0);
        }
        // Stilkøbing 05:00 → Munkeröd 05:15-05:20 → Vig: it stops at Munkeröd rather than starting there, so its
        // row is the one with an arrival. Track 2, where it leaves before the freights that start there. It
        // gathers wagons at both stations — one flow connected at each — so both of them list it.
        var through = AddTrain(timetable, FreightTrainCount + 2, 4800, timetable.TrainCategories.First(c => c.IsFreight),
            stilkobing["1"], munkerod, vig, Time.FromHourAndMinute(5, 0), stopTrack: "2");
        var throughCalls = through.CallsInRunOrder;
        through.CreateCargoFlow(1, throughCalls[0], throughCalls[1], toAll, positionInTrain: 1);
        through.CreateCargoFlow(2, throughCalls[1], throughCalls[2], toAll, positionInTrain: 1);

        plan.Reconcile();

        var schedule = plan.CreateSchedule();
        schedule.Append(passenger.AsTrainPart);
        var wagonset = plan.CreateVehicle(ScheduledObjectType.Wagonset, "B", 1, null);
        foreach (var (@class, number) in new[] { ("A", "50 74 19-1"), ("AB", "50 74 38-2"), ("B", "50 74 20-3"), ("B", "50 74 20-4"), ("BF", "50 74 82-5") })
            wagonset.AddWagon(@class, number, isPassenger: true);
        plan.AssignVehicle(schedule, wagonset);

        return JsonSerializer.Serialize(plan, PlanJson.CreateOptions());
    }

    private static Train AddTrain(
        Timetable timetable, int id, int number, TrainCategory category, StationTrack start, Station middle, Station end,
        Time departure, string stopTrack = "1")
    {
        var train = new Train(id, number) { Category = category };
        train.Add(new StationCall(id * 10 + 1, start, departure.AddMinutes(-10), departure));
        var stop = train.Add(new StationCall(id * 10 + 2, middle[stopTrack], departure.AddMinutes(15), departure.AddMinutes(20)));
        stop.IsArrival = true;
        stop.IsDeparture = true;
        var last = train.Add(new StationCall(id * 10 + 3, end["1"], departure.AddMinutes(35), departure.AddMinutes(40)));
        last.IsArrival = true;
        timetable.Add(train);
        return train;
    }

    private static OperationLocation NewStation(int id, string name, string signature, bool manned, params string[] tracks)
    {
        var station = new Station(id, name, signature) { IsManned = manned };
        for (var i = 0; i < tracks.Length; i++)
            station.Add(new StationTrack(id * 10 + i, tracks[i]) { DisplayOrder = i + 1 });
        return station;
    }
}
