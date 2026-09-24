using System.Globalization;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the Train compositions report: which departures a station's sheet lists, in which order, what each
/// train leaves with — its cargo flow positions with their destinations and its wagonsets wagon by wagon —
/// what its columns say, and how the rows are fitted to pages.
/// </summary>
[TestClass]
public class TrainCompositionTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(6);

    private sealed record Fixture(Plan Plan, Timetable Timetable, Station Start, Station Middle, Station End, Train Train);

    [TestInitialize]
    public void UseEnglish()
    {
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
    }

    // Three stations in a line, each with tracks 1 and 2. The train leaves Start 06:00 from track 2, stops at
    // Middle 06:20-06:25 and ends at End 06:45. End stands for a region.
    private static Fixture CreateFixture()
    {
        var layout = new Layout { Name = "Test" };
        var start = AddStation(layout, 1, "Start", "St");
        var middle = AddStation(layout, 2, "Middle", "Mi");
        var end = AddStation(layout, 3, "End", "En");
        end.Regions.Add(new Region { Id = 1, Name = "East", BackgroundColor = "#CC0000" });
        layout.Add(new TrackStretch(1, start, middle, 10));
        layout.Add(new TrackStretch(2, middle, end, 10));

        var timetable = new Timetable("Test", layout);
        var train = AddTrain(timetable, 1, 4711, start["2"], middle["1"], end["1"], Time.FromHourAndMinute(6, 0));
        return new Fixture(Plan.Create("Test", timetable), timetable, start, middle, end, train);
    }

    private static Station AddStation(Layout layout, int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10 + 1, "1") { DisplayOrder = 1 });
        station.Add(new StationTrack(id * 10 + 2, "2") { DisplayOrder = 2 });
        layout.Add(station);
        return station;
    }

    private static Train AddTrain(
        Timetable timetable, int id, int number, StationTrack first, StationTrack second, StationTrack third, Time departure)
    {
        var train = new Train(id, number);
        train.Add(new StationCall(id * 10 + 1, first, departure.AddMinutes(-10), departure));
        var stop = train.Add(new StationCall(id * 10 + 2, second, departure.AddMinutes(20), departure.AddMinutes(25)));
        stop.IsArrival = true;
        stop.IsDeparture = true;
        var last = train.Add(new StationCall(id * 10 + 3, third, departure.AddMinutes(45), departure.AddMinutes(55)));
        last.IsArrival = true;
        timetable.Add(train);
        return train;
    }

    private static CargoFlowTrainPart AddFlow(
        Fixture fixture, int fromIndex, int toIndex, int position, params Destination[] destinations)
    {
        var options = fixture.Timetable.Add(new CargoFlowOptions());
        foreach (var destination in destinations) options.Destinations.Add(destination);
        var calls = fixture.Train.CallsInRunOrder;
        return fixture.Train.CreateCargoFlow(fixture.Train.CargoFlows.Count + 1, calls[fromIndex], calls[toIndex], options, position);
    }

    private static ScheduledObject AddWagonset(Fixture fixture, Sessions? sessions = null, params string[] classes)
    {
        var schedule = fixture.Plan.CreateSchedule();
        schedule.Add(fixture.Train.AsTrainPart);
        var wagonset = fixture.Plan.CreateVehicle(ScheduledObjectType.Wagonset, "B", fixture.Plan.ScheduledObjects.Count + 1, null);
        foreach (var @class in classes) wagonset.AddWagon(@class, $"{@class}-1");
        fixture.Plan.AssignVehicle(schedule, wagonset, sessions);
        return wagonset;
    }

    private static IReadOnlyList<CompositionDeparture> DeparturesAt(Fixture fixture, OperationLocation station) =>
        StationCompositions.Create(station, fixture.Timetable.Trains, Settings, fixture.Plan).Departures;

    [TestMethod]
    public void ATrainWithNothingToShowIsNotListed()
    {
        var fixture = CreateFixture();

        Assert.IsEmpty(DeparturesAt(fixture, fixture.Start));
    }

    [TestMethod]
    public void CargoPositionsAreDrawnFrontFirstWithAnywhereLast()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 0, new Destination { Location = fixture.End });
        AddFlow(fixture, 0, 2, 2, new Destination { Location = fixture.End });
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.Middle });

        var groups = DeparturesAt(fixture, fixture.Start).Single().Groups;

        CollectionAssert.AreEqual(new[] { 1, 2, 0 }, groups.Select(group => group.Position).ToArray());
    }

    [TestMethod]
    public void ACargoPositionStatesItsDestinationsWithQualifiersAndColouredRegions()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1,
            new Destination { Location = fixture.End, AndRegions = true, AndLocalDestinations = true },
            new Destination { Location = fixture.Middle, AndBeyond = true });

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        Assert.HasCount(2, cargo.Destinations, "One line per destination.");
        Assert.AreEqual("End and local destinations, East", cargo.Destinations[0].Text);
        Assert.Contains("background-color: #CC0000", cargo.Destinations[0].Html.Value, "The region is a coloured chip.");
        Assert.AreEqual("Middle and beyond", cargo.Destinations[1].Text);
    }

    [TestMethod]
    public void FlowsSharingAPositionShareOneRectangle()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End }, new Destination { Location = fixture.Middle });

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        CollectionAssert.AreEqual(new[] { "End", "Middle" }, cargo.Destinations.Select(d => d.Text).ToArray(),
            "A destination named by both flows is listed once.");
    }

    [TestMethod]
    public void AFlowToAllDestinationsStandsAloneAtItsPosition()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });
        AddFlow(fixture, 0, 2, 1).CargoFlowOptions.ToAllDestinations = true;

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        Assert.AreEqual("all destinations", cargo.Destinations.Single().Text);
    }

    [TestMethod]
    public void TheCompositionIncludesWagonsCoupledEarlierAndLeavesOutThoseUncoupledHere()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });     // through
        AddFlow(fixture, 0, 1, 2, new Destination { Location = fixture.Middle });  // uncoupled at Middle
        AddFlow(fixture, 1, 2, 3, new Destination { Location = fixture.End });     // coupled at Middle

        var atMiddle = DeparturesAt(fixture, fixture.Middle).Single().Groups;

        CollectionAssert.AreEqual(new[] { 1, 3 }, atMiddle.Select(group => group.Position).ToArray());
    }

    [TestMethod]
    public void ATrainIsNotListedWhereItEndsOrRunsPast()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });
        var middle = fixture.Train.CallsInRunOrder[1];
        middle.IsArrival = false;
        middle.IsDeparture = false;

        Assert.IsEmpty(DeparturesAt(fixture, fixture.Middle), "Nobody touches a train running past.");
        Assert.IsEmpty(DeparturesAt(fixture, fixture.End), "The last call departs nowhere.");
    }

    [TestMethod]
    public void AWagonsetIsDrawnWagonByWagonInRakeOrder()
    {
        var fixture = CreateFixture();
        var wagonset = AddWagonset(fixture, null, "A", "AB", "B");
        // Moved to the rear after being added first: the rake order is the positions, not the order added.
        wagonset.Wagons.First().Position = 4;

        var group = (WagonsetComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        CollectionAssert.AreEqual(new[] { "AB", "B", "A" }, group.Wagons.Select(wagon => wagon.Class).ToArray());
        Assert.AreEqual("B-1", group.Wagons[1].Number);
        Assert.IsNull(group.Sessions, "A wagonset in the train on all its sessions says nothing more.");
    }

    [TestMethod]
    public void AWagonsetListingNoWagonsIsNotDrawn()
    {
        var fixture = CreateFixture();
        AddWagonset(fixture);

        Assert.IsEmpty(DeparturesAt(fixture, fixture.Start));
    }

    [TestMethod]
    public void AWagonsetOnSomeOfTheTrainsSessionsIsMarkedWithThemInTheTurnusColumn()
    {
        var fixture = CreateFixture();
        AddWagonset(fixture, Sessions.FromSessionNumbers(1, 3, 5), "A");
        AddWagonset(fixture, Sessions.FromSessionNumbers(2, 4, 6), "B");

        var departure = DeparturesAt(fixture, fixture.Start).Single();
        var groups = departure.Groups.Cast<WagonsetComposition>().ToList();

        Assert.HasCount(2, groups);
        CollectionAssert.AreEqual(new byte[] { 1, 3, 5 }, groups[0].Sessions!.Value.Numbers);
        CollectionAssert.AreEqual(new byte[] { 2, 4, 6 }, groups[1].Sessions!.Value.Numbers);
        Assert.HasCount(2, departure.TurnusTexts, "One line of the turnus column per rake, in the order they stand.");
        Assert.EndsWith(" 1,3,5", departure.TurnusTexts[0]);
        Assert.EndsWith(" 2,4,6", departure.TurnusTexts[1]);
    }

    [TestMethod]
    public void TheTurnusColumnNamesEveryWagonsetAndSaysNothingWhereARakeRunsWithTheTrain()
    {
        var fixture = CreateFixture();
        var wagonset = AddWagonset(fixture, null, "A");
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });

        var departure = DeparturesAt(fixture, fixture.Start).Single();

        Assert.AreEqual(wagonset.Designation, departure.Turnuses.Single().Designation);
        Assert.AreEqual(wagonset.Designation, departure.TurnusTexts.Single(),
            "A rake in the train on all its sessions is named and nothing more; the cargo positions name no turnus.");
    }

    [TestMethod]
    public void TheArrivalIsEmptyWhereTheTrainStartsItsRun()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });

        Assert.IsNull(DeparturesAt(fixture, fixture.Start).Single().ArrivalTime, "It arrives from nowhere.");
        Assert.AreEqual(Time.FromHourAndMinute(6, 20), DeparturesAt(fixture, fixture.Middle).Single().ArrivalTime);
    }

    [TestMethod]
    public void WagonsetsComeBeforeCargoAtTheSamePosition()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 0, new Destination { Location = fixture.End });
        AddWagonset(fixture, null, "Gbs");

        var groups = DeparturesAt(fixture, fixture.Start).Single().Groups;

        Assert.IsInstanceOfType<WagonsetComposition>(groups[0]);
        Assert.IsInstanceOfType<CargoPositionComposition>(groups[1]);
    }

    [TestMethod]
    public void DeparturesAreInTrackOrderThenTimeOrder()
    {
        var fixture = CreateFixture();
        var destination = new Destination { Location = fixture.End };
        AddFlow(fixture, 0, 2, 1, destination);
        foreach (var (id, number, track, hour) in new[] { (2, 2, "1", 8), (3, 3, "1", 7) })
        {
            var train = AddTrain(fixture.Timetable, id, number, fixture.Start[track], fixture.Middle["1"], fixture.End["1"], Time.FromHourAndMinute(hour, 0));
            var calls = train.CallsInRunOrder;
            train.CreateCargoFlow(1, calls[0], calls[2], fixture.Timetable.CargoFlowOptions.First());
        }

        var departures = DeparturesAt(fixture, fixture.Start);

        // Track 1 before track 2, where the 06:00 train leaves from; on track 1 the 07:00 before the 08:00.
        CollectionAssert.AreEqual(new[] { 3, 2, 4711 }, departures.Select(d => d.Call.Train.Number).ToArray());
    }

    [TestMethod]
    public void AShuntingTaskIsNotADeparture()
    {
        var fixture = CreateFixture();
        var category = new TrainCategory { Id = 1, Name = "Shunting", Prefix = "V", Content = TrainContent.Cargo, IsShunting = true };
        var task = new Train(9, category, 9001) { Sessions = Sessions.All };
        var call = task.Add(new StationCall(91, fixture.Start["1"], Time.FromHourAndMinute(9, 0), Time.FromHourAndMinute(9, 30)));
        call.IsArrival = true;
        call.IsDeparture = true;
        fixture.Timetable.Add(task);
        var options = fixture.Timetable.Add(new CargoFlowOptions());
        options.Destinations.Add(new Destination { Location = fixture.Start });
        task.CreateCargoFlow(1, call, call, options);

        Assert.IsEmpty(DeparturesAt(fixture, fixture.Start));
    }

    [TestMethod]
    public void GroupsWrapOntoAnotherLineWhereTheNextDoesNotFit()
    {
        var geometry = CompositionPageGeometry.A4Landscape;
        var destinations = new[] { new CompositionDestination("End", new("End")) };
        CompositionGroup Cargo(int position) => new CargoPositionComposition { Position = position, Destinations = destinations };
        var width = CompositionPaginator.WidthMmOf(Cargo(1), geometry);
        var perLine = (int)((geometry.CompositionWidthMm + geometry.GroupGapWidthMm) / (width + geometry.GroupGapWidthMm));

        var one = CompositionPaginator.CompositionHeightMmOf([.. Enumerable.Range(1, perLine).Select(Cargo)], geometry);
        var two = CompositionPaginator.CompositionHeightMmOf([.. Enumerable.Range(1, perLine + 1).Select(Cargo)], geometry);

        Assert.AreEqual(one * 2 + geometry.GroupGapHeightMm, two, 0.001);
    }

    [TestMethod]
    public void AWagonRectangleIsAsWideAsItsClassAndNumberSideBySide()
    {
        var geometry = CompositionPageGeometry.A4Landscape;
        var bare = new Wagon { Class = "A" };
        var numbered = new Wagon { Class = "Gbs", Number = "51 74 20-70 123-4" };

        Assert.AreEqual(geometry.RectangleMinWidthMm, CompositionPaginator.WidthMmOf(bare, geometry), 0.001,
            "A one-letter class still reads as a wagon.");
        Assert.AreEqual(
            geometry.WagonChromeWidthMm + (3 * geometry.WagonClassCharacterWidthMm) + geometry.WagonTextGapMm + (17 * geometry.WagonNumberCharacterWidthMm),
            CompositionPaginator.WidthMmOf(numbered, geometry), 0.001);
    }

    [TestMethod]
    public void AWagonsetIsOneLineHighUntilItsWagonsNoLongerFitSideBySide()
    {
        var fixture = CreateFixture();
        var geometry = CompositionPageGeometry.A4Landscape;
        var wagonset = AddWagonset(fixture, null, "B");
        var width = CompositionPaginator.WidthMmOf(wagonset.Wagons.Single(), geometry);
        var perLine = (int)((geometry.CompositionWidthMm + geometry.WagonGapMm) / (width + geometry.WagonGapMm));
        WagonsetComposition Of(int count) => new()
        {
            Wagonset = wagonset,
            Wagons = [.. Enumerable.Range(0, count).Select(_ => new Wagon { Class = "B", Number = "B-1" })],
            Position = 0,
        };

        Assert.AreEqual(geometry.WagonHeightMm, CompositionPaginator.HeightMmOf(Of(perLine), geometry), 0.001);
        Assert.AreEqual(geometry.CompositionWidthMm, CompositionPaginator.WidthMmOf(Of(perLine + 1), geometry), 0.001,
            "A wagonset too long for a line takes the whole width.");
        Assert.AreEqual((2 * geometry.WagonHeightMm) + geometry.WagonGapMm,
            CompositionPaginator.HeightMmOf(Of(perLine + 1), geometry), 0.001);
    }

    [TestMethod]
    public void ADestinationSaysTheMostThatMayBeBroughtThere()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1,
            new Destination { Location = fixture.End, MaxNumberOfAxles = 16, MaxNumberOfWagons = 12 },
            new Destination { Location = fixture.Middle });

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        Assert.AreEqual("End 16● 12■", cargo.Destinations[0].Text, "The rectangle is sized for the limit too.");
        Assert.AreEqual(16, cargo.Destinations[0].Limit.Axles);
        Assert.AreEqual(12, cargo.Destinations[0].Limit.Wagons);
        Assert.AreEqual("Middle", cargo.Destinations[1].Text, "A destination that takes any number says nothing.");
    }

    [TestMethod]
    public void OnePlaceUnderTwoDifferentLimitsStaysTwoLines()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End, MaxNumberOfWagons = 5 });
        AddFlow(fixture, 0, 2, 1,
            new Destination { Location = fixture.End, MaxNumberOfWagons = 8 },
            new Destination { Location = fixture.End, MaxNumberOfWagons = 5 });

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        CollectionAssert.AreEqual(new[] { "End 5■", "End 8■" }, cargo.Destinations.Select(d => d.Text).ToArray(),
            "The same place under the same limit is listed once; under another limit it is a line of its own.");
    }

    [TestMethod]
    public void ATrainsOwnLimitIsStatedAndTheRowPaysForIt()
    {
        var fixture = CreateFixture();
        var geometry = CompositionPageGeometry.A4Landscape;
        fixture.Train.Length = new TrainCapacity(24, 12, 2.5);
        var departure = new CompositionDeparture
        {
            Call = fixture.Train.CallsInRunOrder[0],
            Groups = [new CargoPositionComposition { Position = 1, Destinations = [new("End", new("End"))] }],
            SessionsText = "1",
            TurnusTexts = [],
        };

        Assert.AreEqual("24● 12■ 2.5m", departure.LimitText);
        Assert.AreEqual((2 * geometry.TextLineHeightMm) + geometry.RowChromeHeightMm,
            CompositionPaginator.HeightMmOf(departure, geometry), 0.001,
            "Three limits wrap onto two lines, and the row is as tall as its tallest column.");
    }

    [TestMethod]
    public void ATrainThatIsNotRestrictedStatesNoLimit()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });

        var departure = DeparturesAt(fixture, fixture.Start).Single();

        Assert.IsEmpty(departure.LimitText, "A limit with no figure cannot be told from a forgotten one.");
    }

    [TestMethod]
    public void ACargoRectangleIsAsWideAsItsLongestDestinationAndNeverNarrowerThanTwoLetters()
    {
        var geometry = CompositionPageGeometry.A4Landscape;
        CompositionGroup Cargo(params string[] destinations) => new CargoPositionComposition
        {
            Position = 1,
            Destinations = [.. destinations.Select(destination => new CompositionDestination(destination, new(destination)))],
        };
        var longest = new string('E', 30);

        Assert.AreEqual(geometry.RectangleMinWidthMm, CompositionPaginator.WidthMmOf(Cargo("En"), geometry), 0.001,
            "A two-letter signature still reads as a rectangle.");
        Assert.AreEqual(geometry.CargoChromeWidthMm + (30 * geometry.DestinationCharacterWidthMm),
            CompositionPaginator.WidthMmOf(Cargo("End", longest), geometry), 0.001);
        Assert.AreEqual(geometry.CargoChromeHeightMm + (2 * geometry.DestinationLineHeightMm),
            CompositionPaginator.HeightMmOf(Cargo("End", longest), geometry), 0.001,
            "Sized to the longest, the others take one line each.");
        Assert.AreEqual(geometry.CompositionWidthMm,
            CompositionPaginator.WidthMmOf(Cargo(new string('E', 400)), geometry), 0.001,
            "A rectangle wider than the composition is brought back within it, and its text wraps.");
    }

    [TestMethod]
    public void EveryStationStartsOnAPageOfItsOwnAndALongOneContinues()
    {
        var fixture = CreateFixture();
        var departure = new CompositionDeparture
        {
            Call = fixture.Train.CallsInRunOrder[0],
            Groups = [new CargoPositionComposition { Position = 1, Destinations = [new("End", new("End"))] }],
            SessionsText = "All",
            TurnusTexts = [],
        };
        var geometry = CompositionPageGeometry.A4Landscape;
        var perPage = (int)((geometry.PrintableHeightMm - geometry.HeaderHeightMm) / CompositionPaginator.HeightMmOf(departure, geometry));
        var first = new StationCompositions(fixture.Start, [.. Enumerable.Repeat(departure, perPage + 1)]);
        var second = new StationCompositions(fixture.Middle, [departure]);
        var empty = new StationCompositions(fixture.End, []);

        var pages = CompositionPaginator.BuildPages([first, second, empty], geometry);

        Assert.HasCount(3, pages, "A station with nothing to show gets no page.");
        Assert.HasCount(perPage, pages[0].Departures);
        Assert.IsFalse(pages[0].IsContinued);
        Assert.IsTrue(pages[1].IsContinued);
        Assert.AreSame(second, pages[2].Station);
        Assert.IsFalse(pages[2].IsContinued);
    }
}
