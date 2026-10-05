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
    // With `endToMiddle`, the second stretch is defined from End to Middle instead.
    private static Fixture CreateFixture(bool endToMiddle = false)
    {
        var layout = new Layout { Name = "Test" };
        var start = AddStation(layout, 1, "Start", "St");
        var middle = AddStation(layout, 2, "Middle", "Mi");
        var end = AddStation(layout, 3, "End", "En");
        end.Regions.Add(new Region { Id = 1, Name = "East", BackgroundColor = "#CC0000" });
        layout.Add(new TrackStretch(1, start, middle, 10));
        layout.Add(endToMiddle ? new TrackStretch(2, end, middle, 10) : new TrackStretch(2, middle, end, 10));

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

    private static ScheduledObject AddWagonset(Fixture fixture, Sessions? sessions = null, params string[] classes) =>
        AddWagonsetFrom(fixture, 0, sessions, classes);

    // A wagonset coupled to the train at its call fromIndex and carried to the end of its run.
    private static ScheduledObject AddWagonsetFrom(Fixture fixture, int fromIndex, Sessions? sessions = null, params string[] classes)
    {
        var schedule = fixture.Plan.CreateSchedule();
        var calls = fixture.Train.CallsInRunOrder;
        schedule.Add(fromIndex == 0 ? fixture.Train.AsTrainPart : new ScheduledTrainPart(calls[fromIndex], calls[^1]));
        var wagonset = fixture.Plan.CreateVehicle(ScheduledObjectType.Wagonset, "B", fixture.Plan.ScheduledObjects.Count + 1, null);
        foreach (var @class in classes) wagonset.AddWagon(@class, $"{@class}-1");
        fixture.Plan.AssignVehicle(schedule, wagonset, sessions);
        return wagonset;
    }

    // The part of the train the wagonset's schedule works: its coupling to the train.
    private static ScheduledTrainPart PartOf(Fixture fixture, ScheduledObject wagonset) =>
        wagonset.ScheduleAssignments.Single().Schedule!.Parts.Single(part => part.Train.Equals(fixture.Train));

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
    public void DestinationPositionsWithinAFlowAreSeparateRectanglesFrontFirst()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 2, new Destination { Location = fixture.End, PositionInTrain = 2 });
        AddFlow(fixture, 0, 2, 1,
            new Destination { Location = fixture.End, PositionInTrain = 0 },
            new Destination { Location = fixture.Middle, PositionInTrain = 2 },
            new Destination { Location = fixture.End, PositionInTrain = 1 });

        var groups = DeparturesAt(fixture, fixture.Start).Single().Groups.Cast<CargoPositionComposition>().ToArray();

        CollectionAssert.AreEqual(new[] { (1, 1), (1, 2), (1, 0), (2, 2) },
            groups.Select(group => (group.Position, group.DestinationPosition)).ToArray(),
            "By the flow's position first, then by the destination's within it, with anywhere last at each level.");
        CollectionAssert.AreEqual(new[] { "End", "Middle", "End", "End" },
            groups.Select(group => group.Destinations.Single().Text).ToArray());
    }

    [TestMethod]
    public void DestinationsSharingAPositionWithinAFlowShareOneRectangle()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1,
            new Destination { Location = fixture.End, PositionInTrain = 1 },
            new Destination { Location = fixture.Middle, PositionInTrain = 1 });

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        Assert.AreEqual(1, cargo.DestinationPosition);
        CollectionAssert.AreEqual(new[] { "End", "Middle" }, cargo.Destinations.Select(d => d.Text).ToArray(),
            "They are one unit of wagons, so they are listed in one rectangle.");
    }

    [TestMethod]
    public void ACargoPositionStatesItsDestinationsWithQualifiersAndColouredRegions()
    {
        var fixture = CreateFixture();
        fixture.Middle.CargoServedFrom = fixture.End;
        AddFlow(fixture, 0, 2, 1,
            new Destination { Location = fixture.End, AndRegions = true, AndLocalDestinations = true },
            new Destination { Location = fixture.Middle, AndBeyond = true });

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        CollectionAssert.AreEqual(new[] { "End, Middle", "Middle and beyond", "East" },
            cargo.Destinations.Select(destination => destination.Text).ToArray(),
            "The local destinations are named, not summed up, and the regions follow all the places.");
        Assert.Contains("background-color: #CC0000", cargo.Destinations[2].Html.Value, "The region is a coloured chip.");
    }

    [TestMethod]
    public void RegionsAreListedOnceAfterEveryPlace()
    {
        var fixture = CreateFixture();
        fixture.Middle.Regions.Add(fixture.End.Regions.Single());
        AddFlow(fixture, 0, 2, 1,
            new Destination { Location = fixture.End, AndRegions = true },
            new Destination { Location = fixture.Middle, AndRegions = true, MaxNumberOfWagons = 4 });

        var cargo = (CargoPositionComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        CollectionAssert.AreEqual(new[] { "End", "Middle 4■", "East" },
            cargo.Destinations.Select(destination => destination.Text).ToArray());
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
    public void OnlyTheCargoFlowsConnectedAtTheStationAreListed()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });     // through Middle
        AddFlow(fixture, 0, 1, 2, new Destination { Location = fixture.Middle });  // uncoupled at Middle
        AddFlow(fixture, 1, 2, 3, new Destination { Location = fixture.End });     // coupled at Middle

        var atStart = DeparturesAt(fixture, fixture.Start).Single().Groups;
        var atMiddle = DeparturesAt(fixture, fixture.Middle).Single(departure => !departure.IsArrival).Groups;

        CollectionAssert.AreEqual(new[] { 1, 2 }, atStart.Select(group => group.Position).ToArray(),
            "Both flows are gathered at Start.");
        CollectionAssert.AreEqual(new[] { 3 }, atMiddle.Select(group => group.Position).ToArray(),
            "Middle gathers the wagons of one flow; the others only arrive there or end there.");
    }

    [TestMethod]
    public void ATrainIsNotListedWhereItEndsOrRunsPast()
    {
        var fixture = CreateFixture();
        // Not uncoupled at End by any instruction, so End has no arrival to list either.
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End }).HasUncoupleNote = false;
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
    public void AWagonsetListingNoWagonsIsDrawnByItsTurnusAlone()
    {
        var fixture = CreateFixture();
        var wagonset = AddWagonset(fixture);
        wagonset.Class = "Gbs";
        wagonset.NumberOfUnits = 5;

        var group = (WagonsetComposition)DeparturesAt(fixture, fixture.Start).Single().Groups.Single();

        Assert.IsEmpty(group.Wagons, "The designation names the class; a rectangle repeating it says nothing more.");
        Assert.AreEqual(wagonset.Designation, group.Designation);
    }

    [TestMethod]
    public void AWagonsetIsListedOnlyWhereItIsCoupledToTheTrain()
    {
        var fixture = CreateFixture();
        var fromStart = AddWagonset(fixture, null, "A");
        var fromMiddle = AddWagonsetFrom(fixture, 1, null, "B");

        var atStart = DeparturesAt(fixture, fixture.Start).Single().Groups.Cast<WagonsetComposition>();
        var atMiddle = DeparturesAt(fixture, fixture.Middle).Single().Groups.Cast<WagonsetComposition>();

        CollectionAssert.AreEqual(new[] { fromStart }, atStart.Select(group => group.Wagonset).ToArray());
        CollectionAssert.AreEqual(new[] { fromMiddle }, atMiddle.Select(group => group.Wagonset).ToArray(),
            "The rake coupled at Start only runs on with the train at Middle; nothing is made up for it there.");
    }

    [TestMethod]
    public void AWagonsetStayingWithItsLocoIsListedOnlyAtTheFirstDepartureOfItsSchedule()
    {
        var fixture = CreateFixture();
        var wagonset = AddWagonset(fixture, null, "A");
        var back = AddTrain(fixture.Timetable, 2, 4712, fixture.End["2"], fixture.Middle["2"], fixture.Start["1"], Time.FromHourAndMinute(7, 0));
        var schedule = wagonset.ScheduleAssignments.Single().Schedule!;
        var returning = schedule.Add(back.AsTrainPart);

        Assert.IsEmpty(DeparturesAt(fixture, fixture.End),
            "The rake stays in the train its loco works on; nothing is coupled at End.");

        returning.WagonSetOptions = new WagonSetOptions { HasCoupleNote = true };

        Assert.AreEqual(wagonset, DeparturesAt(fixture, fixture.End).Single().Groups.Cast<WagonsetComposition>().Single().Wagonset,
            "A couple note says the rake is coupled there.");
    }

    [TestMethod]
    public void WagonsetsCoupledAtTheSameStationStandByTheirCouplingsPositions()
    {
        var fixture = CreateFixture();
        var rear = AddWagonset(fixture, null, "G");
        var front = AddWagonset(fixture, null, "H");
        PartOf(fixture, rear).WagonSetOptions = new WagonSetOptions { OrderInTrain = 2 };
        PartOf(fixture, front).WagonSetOptions = new WagonSetOptions { OrderInTrain = 1 };

        var groups = DeparturesAt(fixture, fixture.Start).Single().Groups.Cast<WagonsetComposition>().ToArray();

        CollectionAssert.AreEqual(new[] { front, rear }, groups.Select(group => group.Wagonset).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2 }, groups.Select(group => group.Position).ToArray());
    }

    [TestMethod]
    public void AWagonsetOnSomeOfTheTrainsSessionsIsMarkedWithThemBesideItsTurnus()
    {
        var fixture = CreateFixture();
        AddWagonset(fixture, Sessions.FromSessionNumbers(1, 3, 5), "A");
        AddWagonset(fixture, Sessions.FromSessionNumbers(2, 4, 6), "B");

        var departure = DeparturesAt(fixture, fixture.Start).Single();
        var groups = departure.Groups.Cast<WagonsetComposition>().ToList();

        Assert.HasCount(2, groups);
        CollectionAssert.AreEqual(new byte[] { 1, 3, 5 }, groups[0].Sessions!.Value.Numbers);
        CollectionAssert.AreEqual(new byte[] { 2, 4, 6 }, groups[1].Sessions!.Value.Numbers);
        Assert.AreEqual("1,3,5", groups[0].SessionsText);
        Assert.AreEqual("2,4,6", groups[1].SessionsText);
    }

    [TestMethod]
    public void EveryWagonsetIsNamedAndSaysNothingMoreWhereARakeRunsWithTheTrain()
    {
        var fixture = CreateFixture();
        var wagonset = AddWagonset(fixture, null, "A");
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });

        var departure = DeparturesAt(fixture, fixture.Start).Single();

        var group = departure.Groups.OfType<WagonsetComposition>().Single();
        Assert.AreEqual(wagonset.Designation, group.Designation);
        Assert.IsNull(group.SessionsText, "A rake in the train on all its sessions is named and nothing more.");
    }

    [TestMethod]
    public void TheArrivalIsEmptyWhereTheTrainStartsItsRun()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });
        AddFlow(fixture, 1, 2, 2, new Destination { Location = fixture.End });

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
        var bare = new CompositionWagon("A");
        var numbered = new CompositionWagon("Gbs", "51 74 20-70 123-4");

        Assert.AreEqual(geometry.RectangleMinWidthMm, CompositionPaginator.WidthMmOf(bare, geometry), 0.001,
            "A one-letter class still reads as a wagon.");
        Assert.AreEqual(
            geometry.WagonChromeWidthMm + (3 * geometry.WagonClassCharacterWidthMm) + geometry.WagonTextGapMm + (17 * geometry.WagonNumberCharacterWidthMm),
            CompositionPaginator.WidthMmOf(numbered, geometry), 0.001);
    }

    [TestMethod]
    public void AWagonsetIsHeadedByItsTurnusAsWideAsItsNameAndSessions()
    {
        var fixture = CreateFixture();
        var geometry = CompositionPageGeometry.A4Landscape;
        var wagonset = AddWagonset(fixture, null, "B");
        WagonsetComposition Of(string? sessions) =>
            new() { Wagonset = wagonset, Wagons = [new CompositionWagon("B")], Position = 0, SessionsText = sessions };

        Assert.AreEqual(geometry.WagonChromeWidthMm + (wagonset.Designation.Length * geometry.WagonClassCharacterWidthMm),
            CompositionPaginator.TurnusWidthMmOf(Of(null), geometry), 0.001);
        Assert.AreEqual(
            geometry.WagonChromeWidthMm + (wagonset.Designation.Length * geometry.WagonClassCharacterWidthMm) +
            geometry.WagonTextGapMm + (5 * geometry.CharacterWidthMm),
            CompositionPaginator.TurnusWidthMmOf(Of("1,3,5"), geometry), 0.001);
        Assert.AreEqual(
            CompositionPaginator.TurnusWidthMmOf(Of(null), geometry) + geometry.WagonGapMm + geometry.RectangleMinWidthMm,
            CompositionPaginator.WidthMmOf(Of(null), geometry), 0.001, "The turnus stands before the wagons, on the same line.");
    }

    [TestMethod]
    public void AWagonsetIsOneLineHighUntilItsWagonsNoLongerFitSideBySide()
    {
        var fixture = CreateFixture();
        var geometry = CompositionPageGeometry.A4Landscape;
        var wagonset = AddWagonset(fixture, null, "B");
        var width = CompositionPaginator.WidthMmOf(new CompositionWagon("B", "B-1"), geometry);
        WagonsetComposition Of(int count) => new()
        {
            Wagonset = wagonset,
            Wagons = [.. Enumerable.Range(0, count).Select(_ => new CompositionWagon("B", "B-1"))],
            Position = 0,
        };
        // The turnus stands first on the line, and the wagons fill what it leaves.
        var turnus = CompositionPaginator.TurnusWidthMmOf(Of(0), geometry);
        var perLine = (int)((geometry.CompositionWidthMm - turnus) / (width + geometry.WagonGapMm));

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
        };

        Assert.AreEqual("24● 12■ 2.5m", departure.LimitText);
        var limited = CompositionPaginator.LocoWidthMmOf(departure, geometry);
        Assert.AreEqual(geometry.WagonHeightMm + geometry.RowChromeHeightMm,
            CompositionPaginator.HeightMmOf(departure, geometry), 0.001, "On one line, it costs no height.");

        fixture.Train.Length = default;
        Assert.AreEqual(limited - geometry.WagonTextGapMm - (12 * geometry.CharacterWidthMm),
            CompositionPaginator.LocoWidthMmOf(departure, geometry), 0.001, "The limit stands last in the loco.");
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
    public void ACargoRectangleIsAsWideAsItsListOfDestinationsAndNeverNarrowerThanTwoLetters()
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
        Assert.AreEqual(geometry.CargoChromeWidthMm + (("End, ".Length + 30) * geometry.DestinationCharacterWidthMm),
            CompositionPaginator.WidthMmOf(Cargo("End", longest), geometry), 0.001, "The places run on as one list.");
        Assert.AreEqual(geometry.CargoChromeHeightMm + geometry.DestinationLineHeightMm,
            CompositionPaginator.HeightMmOf(Cargo("End", longest), geometry), 0.001,
            "A list that fits the composition takes one line.");
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
        };
        var geometry = CompositionPageGeometry.A4Landscape;
        var perPage = (int)((geometry.PrintableHeightMm - geometry.HeaderHeightMm) / CompositionPaginator.HeightMmOf(departure, geometry));
        var first = new StationCompositions(fixture.Start, [.. Enumerable.Repeat(departure, perPage + 1)]);
        var second = new StationCompositions(fixture.Middle, [departure]);
        var empty = new StationCompositions(fixture.End, []);

        var pages = CompositionPaginator.BuildPages([first, second, empty], geometry);

        Assert.HasCount(6, pages, "A station with nothing to show gets no page; every page comes twice.");
        Assert.HasCount(perPage, pages[0].Departures);
        Assert.IsFalse(pages[0].IsContinued);
        Assert.IsTrue(pages[2].IsContinued);
        Assert.AreSame(second, pages[4].Station);
        Assert.IsFalse(pages[4].IsContinued);
    }

    [TestMethod]
    public void EveryPageIsFollowedByItsMirrorImageHoldingTheSameRows()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 1, 2, 1, new Destination { Location = fixture.End });
        var station = StationCompositions.Create(fixture.Middle, fixture.Timetable.Trains, Settings, fixture.Plan, new CompositionOrientation(fixture.Timetable.Layout));

        var pages = CompositionPaginator.BuildPages([station], CompositionPageGeometry.A4Landscape);

        Assert.HasCount(2, pages);
        Assert.AreEqual(CompositionView.AsDrawn, pages[0].View);
        Assert.AreEqual(CompositionView.Mirrored, pages[1].View);
        CollectionAssert.AreEqual(pages[0].Departures.ToArray(), pages[1].Departures.ToArray());
        var departure = pages[0].Departures.Single();
        Assert.AreEqual(CompositionHeading.Rightwards, pages[0].HeadingOf(departure));
        Assert.AreEqual(CompositionHeading.Leftwards, pages[1].HeadingOf(departure), "Seen from the other side.");
        CollectionAssert.AreEqual(new[] { fixture.Start }, pages[0].LeftNeighbours.ToArray());
        CollectionAssert.AreEqual(new[] { fixture.End }, pages[0].RightNeighbours.ToArray());
        CollectionAssert.AreEqual(new[] { fixture.End }, pages[1].LeftNeighbours.ToArray(), "The ends change places too.");
        CollectionAssert.AreEqual(new[] { fixture.Start }, pages[1].RightNeighbours.ToArray());
    }

    [TestMethod]
    public void ATrainHeadsTowardsWhereItDepartsForAndAwayFromWhereItArrivesFrom()
    {
        var fixture = CreateFixture();
        var flow = AddFlow(fixture, 0, 1, 1, new Destination { Location = fixture.Middle });
        flow.HasUncoupleNote = true;
        AddFlow(fixture, 1, 2, 1, new Destination { Location = fixture.End });
        var back = AddTrain(fixture.Timetable, 2, 4712, fixture.End["1"], fixture.Middle["2"], fixture.Start["1"], Time.FromHourAndMinute(8, 0));
        var calls = back.CallsInRunOrder;
        var options = fixture.Timetable.Add(new CargoFlowOptions());
        options.Destinations.Add(new Destination { Location = fixture.Start });
        back.CreateCargoFlow(1, calls[1], calls[2], options);

        var departures = StationCompositions.Create(fixture.Middle, fixture.Timetable.Trains, Settings, fixture.Plan, new CompositionOrientation(fixture.Timetable.Layout)).Departures;

        // Both stretches run Start to Middle to End, so a train running that way travels forward.
        Assert.AreEqual(CompositionHeading.Rightwards, departures.Single(d => d.IsArrival).Heading, "Arriving from the left.");
        Assert.AreEqual(CompositionHeading.Rightwards, departures.Single(d => !d.IsArrival && d.Call.Train.Number == 4711).Heading);
        Assert.AreEqual(CompositionHeading.Leftwards, departures.Single(d => d.Call.Train.Number == 4712).Heading);
    }

    [TestMethod]
    public void TheHeadingFollowsTheDefinedDirectionOfTheTrackStretch()
    {
        var fixture = CreateFixture(endToMiddle: true);
        var flow = AddFlow(fixture, 0, 1, 1, new Destination { Location = fixture.Middle });
        flow.HasUncoupleNote = true;
        AddFlow(fixture, 1, 2, 1, new Destination { Location = fixture.End });

        var station = StationCompositions.Create(fixture.Middle, fixture.Timetable.Trains, Settings, fixture.Plan, new CompositionOrientation(fixture.Timetable.Layout));

        Assert.AreEqual(CompositionHeading.Rightwards, station.Departures.Single(d => d.IsArrival).Heading, "Arriving along Start to Middle.");
        Assert.AreEqual(CompositionHeading.Leftwards, station.Departures.Single(d => !d.IsArrival).Heading, "Departing against End to Middle.");
        // A train leaves for either of them against its stretch's direction, so both are named at the left.
        CollectionAssert.AreEquivalent(new[] { fixture.Start, fixture.End }, station.LeftNeighbours.ToArray());
        Assert.IsEmpty(station.RightNeighbours);
    }

    [TestMethod]
    public void WithNoOrientationATrainIsDrawnLocoFirstFromTheLeft()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });

        Assert.AreEqual(CompositionHeading.Leftwards, DeparturesAt(fixture, fixture.Start).Single().Heading);
    }

    [TestMethod]
    public void TheLocoStatesTheTimesWithTheDashOnTheSideTheTrainRunsOn()
    {
        var fixture = CreateFixture();
        var calls = fixture.Train.CallsInRunOrder;
        CompositionDeparture Row(int index, CompositionMovement movement = CompositionMovement.Departing) =>
            new() { Call = calls[index], Movement = movement, Groups = [], SessionsText = "1" };

        Assert.AreEqual("-06:00", Row(0).TimeText, "It starts its run here.");
        Assert.AreEqual("06:20-06:25", Row(1).TimeText);
        Assert.AreEqual("06:45-", Row(2, CompositionMovement.Arriving).TimeText, "It ends its run here.");
    }

    [TestMethod]
    public void TheLocoStandsFirstInTheCompositionAndTheRowPaysForIt()
    {
        var fixture = CreateFixture();
        var geometry = CompositionPageGeometry.A4Landscape;
        var departure = new CompositionDeparture { Call = fixture.Train.CallsInRunOrder[0], Groups = [], SessionsText = "1,3" };

        Assert.AreEqual(
            geometry.LocoChromeWidthMm + (3 * geometry.CharacterWidthMm) +
            (departure.TrainIdentity.Length * geometry.WagonClassCharacterWidthMm) +
            (6 * geometry.WagonNumberCharacterWidthMm) + (2 * geometry.WagonTextGapMm),
            CompositionPaginator.LocoWidthMmOf(departure, geometry), 0.001);
        Assert.AreEqual(geometry.WagonHeightMm + geometry.RowChromeHeightMm,
            CompositionPaginator.HeightMmOf(departure, geometry), 0.001, "A loco alone is one rectangle high.");
    }

    [TestMethod]
    public void ArrivingCargoFlowWagonsAreListedWithTheirOriginsInWagonOrder()
    {
        var fixture = CreateFixture();
        var second = AddFlow(fixture, 0, 2, 2, new Destination { Location = fixture.End });
        second.CargoFlowOptions.Origins.Add(new Origin { Location = fixture.Middle });
        AddFlow(fixture, 0, 2, 0, new Destination { Location = fixture.End });
        AddFlow(fixture, 1, 2, 1, new Destination { Location = fixture.End });

        var arrival = DeparturesAt(fixture, fixture.End).Single();

        Assert.AreEqual(CompositionMovement.Arriving, arrival.Movement);
        Assert.AreEqual(Time.FromHourAndMinute(6, 45), arrival.ArrivalTime);
        Assert.IsNull(arrival.DepartureTime, "The train ends its run here.");
        Assert.AreEqual("Start", arrival.EndName, "An arrival names where the train came from.");
        var groups = arrival.Groups.Cast<ArrivingCargoComposition>().ToArray();
        CollectionAssert.AreEqual(new[] { 1, 2 }, groups.Select(group => group.Position).ToArray(),
            "Front first; the unit at anywhere names nothing new, so it is not drawn.");
        CollectionAssert.AreEqual(new[] { "Middle", "Start" },
            groups.Select(group => string.Join(", ", group.Origins)).ToArray(),
            "The flow's own from-station, then the origins it forwards, each named only in the front-most rectangle.");
    }

    [TestMethod]
    public void OnlyFlowsWithUncoupleTickedAreListedOnArrival()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End }).HasUncoupleNote = false;

        Assert.IsEmpty(DeparturesAt(fixture, fixture.End));
    }

    [TestMethod]
    public void AtAShadowStationEveryArrivingFlowIsListed()
    {
        var fixture = CreateFixture();
        fixture.End.IsShadow = true;
        AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End }).HasUncoupleNote = false;

        var arrival = DeparturesAt(fixture, fixture.End).Single();

        Assert.IsTrue(arrival.IsArrival);
        Assert.AreEqual("Start", ((ArrivingCargoComposition)arrival.Groups.Single()).Origins.Single());
    }

    [TestMethod]
    public void AFlowBringingNoWagonsFromItsFromStationNamesOnlyItsOrigins()
    {
        var fixture = CreateFixture();
        var forwarding = AddFlow(fixture, 0, 2, 1, new Destination { Location = fixture.End });
        forwarding.BringsNoWagonsFromHere = true;
        forwarding.CargoFlowOptions.Origins.Add(new Origin { Location = fixture.Middle });
        AddFlow(fixture, 0, 2, 2, new Destination { Location = fixture.End }).BringsNoWagonsFromHere = true;

        var groups = DeparturesAt(fixture, fixture.End).Single().Groups;

        Assert.AreEqual("Middle", ((ArrivingCargoComposition)groups.Single()).Origins.Single(),
            "A flow naming no origin at all brings nothing, so it gets no rectangle.");
    }

    [TestMethod]
    public void AnArrivalIsListedBeforeTheDepartureOfTheSameCall()
    {
        var fixture = CreateFixture();
        AddFlow(fixture, 0, 1, 1, new Destination { Location = fixture.Middle });
        AddFlow(fixture, 1, 2, 1, new Destination { Location = fixture.End });

        var rows = DeparturesAt(fixture, fixture.Middle);

        CollectionAssert.AreEqual(new[] { CompositionMovement.Arriving, CompositionMovement.Departing },
            rows.Select(row => row.Movement).ToArray(), "Uncoupled before anything is coupled.");
        Assert.AreEqual(Time.FromHourAndMinute(6, 25), rows[0].DepartureTime,
            "The train runs on, so its arrival row says when it leaves: when the wagons must be off.");
        Assert.IsEmpty(rows[0].LimitText, "Nothing is made up on arrival, so no limit is printed.");
    }

    [TestMethod]
    public void AnArrivingRectangleIsAsWideAsItsListOfOrigins()
    {
        var geometry = CompositionPageGeometry.A4Landscape;
        var arriving = new ArrivingCargoComposition { Position = 1, Origins = ["Start", new string('E', 30)] };

        Assert.AreEqual(geometry.CargoChromeWidthMm + (("Start, ".Length + 30) * geometry.DestinationCharacterWidthMm),
            CompositionPaginator.WidthMmOf(arriving, geometry), 0.001);
        Assert.AreEqual(geometry.CargoChromeHeightMm + geometry.DestinationLineHeightMm,
            CompositionPaginator.HeightMmOf(arriving, geometry), 0.001, "The origins run on as one list on one line.");
    }

    [TestMethod]
    public void AListTooLongForTheCompositionWrapsWithinTheRectangle()
    {
        var geometry = CompositionPageGeometry.A4Landscape;
        var place = new string('E', 20);
        var perLine = geometry.DestinationCharactersWithin(geometry.CompositionWidthMm - geometry.CargoChromeWidthMm);
        var count = (perLine / (place.Length + 2)) + 1;
        var arriving = new ArrivingCargoComposition { Position = 1, Origins = [.. Enumerable.Range(0, count).Select(i => place + i)] };

        Assert.AreEqual(geometry.CompositionWidthMm, CompositionPaginator.WidthMmOf(arriving, geometry), 0.001);
        Assert.AreEqual(geometry.CargoChromeHeightMm + (2 * geometry.DestinationLineHeightMm),
            CompositionPaginator.HeightMmOf(arriving, geometry), 0.001, "Wrapped between the places onto a second line.");
    }
}
