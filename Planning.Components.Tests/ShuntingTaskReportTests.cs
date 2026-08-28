using System.Net;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;
using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers how a shunting task reads in the reports: two timetable rows for its one call — the start time
/// and the end time — with the shunting instruction on the row showing the start, its cargo flows in the
/// duty page's cargo block, and no line on the graphical timetable, which is a diagram of travelling.
/// </summary>
[TestClass]
public class ShuntingTaskReportTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);
    private static readonly Time Start = Time.FromHourAndMinute(14, 0);
    private static readonly Time End = Time.FromHourAndMinute(14, 45);

    private static TrainCategory ShuntingCategory => new()
    {
        Id = 3,
        Name = "Shunting",
        Prefix = "V",
        Content = TrainContent.Cargo,
        IsShunting = true,
    };

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature);
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    // A layout with two stations and the stretch between them, a travelling train over it, and a
    // shunting task at the first station.
    private static (Timetable Timetable, Train Task, OperationLocation Station) Arrange()
    {
        var layout = new Layout { Name = "Test" };
        var first = layout.Add(NewStation(1, "Munkeröd", "Mkd"));
        var last = layout.Add(NewStation(2, "Stilkøbing", "Stk"));
        layout.Add(new TrackStretch(1, first, last, 10));
        var stretch = new TimetableStretch(1, "1");
        foreach (var ts in layout.TrackStretches) stretch.AddLast(ts);
        layout.Add(stretch);

        var timetable = new Timetable("Test", layout);

        var train = new Train(1, 1234);
        train.Add(new StationCall(1, first["1"], Start.AddMinutes(-10), Start));
        var arrival = train.Add(new StationCall(2, last["1"], Start.AddMinutes(20), Start.AddMinutes(30)));
        arrival.IsArrival = true;
        timetable.Add(train);

        var task = new Train(2, ShuntingCategory, 9001) { Sessions = Sessions.All };
        var call = task.Add(new StationCall(3, first["1"], Start, End));
        call.IsArrival = true;
        call.IsDeparture = true;
        timetable.Add(task);

        return (timetable, task, first);
    }

    private static CargoFlowTrainPart AddFlow(Timetable timetable, Train task, OperationLocation destination)
    {
        var options = timetable.Add(new CargoFlowOptions());
        options.Destinations.Add(new Destination { Location = destination });
        var call = task.Calls[0];
        return task.CreateCargoFlow(1, call, call, options);
    }

    [TestMethod]
    public void TheOneCallGivesAnArrivalRowAndADepartureRow()
    {
        var (_, task, _) = Arrange();

        var rows = TimetableRow.Build(task.AsTrainPart, Sessions.All, Settings);

        Assert.HasCount(2, rows);
        Assert.AreEqual(CallHalf.Arrival, rows[0].Half);
        Assert.AreEqual(Start, rows[0].Time, "The arrival row shows when the work starts.");
        Assert.AreEqual(CallHalf.Departure, rows[1].Half);
        Assert.AreEqual(End, rows[1].Time, "The departure row shows when it ends.");
        // Both halves are the driver's: the task is theirs from beginning to end.
        Assert.IsTrue(rows.All(r => r.IsInPart));
    }

    [TestMethod]
    public void TheShuntingInstructionSitsOnTheRowShowingTheStartTime()
    {
        var (timetable, task, station) = Arrange();
        AddFlow(timetable, task, station);

        var rows = TimetableRow.Build(task.AsTrainPart, Sessions.All, Settings);

        Assert.HasCount(1, rows[0].Notes.OfType<ShuntArrivingWagonsNote>());
        Assert.IsEmpty(rows[1].Notes.OfType<ShuntArrivingWagonsNote>());
    }

    [TestMethod]
    public void TheCargoBlockOfADutyPageListsTheTasksFlows()
    {
        var (timetable, task, station) = Arrange();
        var flow = AddFlow(timetable, task, station);
        var plan = Plan.Create("Test", timetable);
        var duty = new DriverDuty(1, "1") { Plan = plan, Sessions = Sessions.All };
        var dutyPart = new DriverDutyPart { TrainPart = task.AsTrainPart, Duty = duty, SessionsSettings = Settings };

        var cargo = dutyPart.CargoData;

        Assert.IsTrue(cargo.HasData);
        Assert.AreSame(flow, cargo.Flows.Single().Flow);
    }

    [TestMethod]
    public void TheCargoBlockDrawsAGlobeForArrivedWagonsWithNoNamedOrigin()
    {
        var (timetable, task, station) = Arrange();
        // Destination is the worked station, so these wagons arrived here and go out to its customers.
        var row = new TrainPartCargoFlow { Flow = AddFlow(timetable, task, station) };

        var html = FromCell(row);

        // The station is where the wagons stand, not where they came from — and the flow names no origin.
        Assert.Contains("fa-globe", html, "A globe says the wagons may have arrived from anywhere.");
        // The word is not printed — the column is too narrow for it — but it labels the globe.
        Assert.Contains("aria-label=\"Anywhere\"", html);
        Assert.DoesNotContain("Munkeröd", html, "The station the task works is not where the wagons came from.");
    }

    [TestMethod]
    public void TheCargoBlockNamesTheOriginsOfArrivedWagonsWhenTheFlowStatesThem()
    {
        var (timetable, task, station) = Arrange();
        var flow = AddFlow(timetable, task, station);
        flow.CargoFlowOptions.Origins.Add(new Origin { Location = timetable.Layout.OperationLocations.Last() });

        // Where the flow says where the wagons came from, that answers the column instead of the globe.
        Assert.AreEqual("Stilkøbing", FromCell(new() { Flow = flow }));
    }

    [TestMethod]
    public void TheCargoBlockNamesTheStationForWagonsFetchedForADepartingTrain()
    {
        var (timetable, task, _) = Arrange();
        // Destination elsewhere, so the task gathers wagons from this station's customers.
        var flow = AddFlow(timetable, task, timetable.Layout.OperationLocations.Last());

        Assert.AreEqual("Munkeröd", FromCell(new() { Flow = flow }));
    }

    // The From cell is markup, and the location names in it are encoded; decoding gives back the names
    // as written, so a test asserting on them does not have to spell out entities.
    private static string FromCell(TrainPartCargoFlow row) =>
        WebUtility.HtmlDecode(row.From("also shunt", "Anywhere").Value);

    [TestMethod]
    public void TheGraphicalTimetableLeavesShuntingTasksOut()
    {
        var (timetable, task, _) = Arrange();

        var graph = new GraphSchedule(timetable.Layout.TimetableStretches.First(), timetable);

        Assert.DoesNotContain(task, graph.Trains);
        Assert.HasCount(1, graph.Trains, "The travelling train is still drawn.");
    }
}
