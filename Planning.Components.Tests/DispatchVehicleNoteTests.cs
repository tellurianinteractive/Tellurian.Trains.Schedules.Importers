using Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers what the vehicle schedules and the cargo flows put on a station's dispatch rows: which
/// traction unit to use, what is coupled and uncoupled here, and where the freight wagons are bound —
/// each on the clearance it belongs to, and qualified by the sessions it holds on.
/// </summary>
/// <remarks>
/// The model's own suites settle what each note says (<c>VehicleNoteSessionsTests</c>,
/// <c>ShuntingTaskTests</c>); what is checked here is that the dispatch list asks for them and sorts them
/// onto the right row, which is the half a reader actually sees.
/// </remarks>
[TestClass]
public class DispatchVehicleNoteTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    private const string Origin = "Munkeröd";
    private const string Terminus = "Stilkøbing";

    [TestInitialize]
    public void UseInvariantCulture()
    {
        System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
        System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    }

    // Two manned stations and one train running the whole way between them, in a plan ready to have
    // vehicles and cargo hung on it.
    private static (Plan Plan, Train Train) Arrange()
    {
        var layout = new Layout { Name = "Test" };
        var first = layout.Add(NewStation(1, Origin, "Mkd"));
        var last = layout.Add(NewStation(2, Terminus, "Stk"));
        layout.Add(new TrackStretch(1, first, last, 10));

        var timetable = new Timetable("Test", layout);
        var train = new Train(1, 1234);
        var start = Time.FromHourAndMinute(12, 00);
        train.Add(new StationCall(10, first["1"], start.AddMinutes(-60), start));
        var end = start.AddMinutes(20);
        var lastCall = train.Add(new StationCall(11, last["1"], end, end.AddMinutes(20)));
        lastCall.IsArrival = true;
        timetable.Add(train);

        return (Plan.Create("Test", timetable), train);
    }

    private static OperationLocation NewStation(int id, string name, string signature)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        station.Add(new StationTrack(id * 10, "1"));
        return station;
    }

    private static IReadOnlyList<DispatchRow> RowsAt(Plan plan, string stationName) =>
        DispatchList.Create(
            plan.Layout.OperationLocations.Single(location => location.Name == stationName),
            plan.Timetable.Trains, Settings, plan).Rows;

    private static IEnumerable<string> NoteTextsAt(Plan plan, string stationName) =>
        RowsAt(plan, stationName).SelectMany(row => row.Notes).Select(note => note.ToText);

    [TestMethod]
    public void TheDispatcherIsToldWhichLocomotiveIsCoupledAndUncoupledHere()
    {
        var (plan, train) = Arrange();
        var schedule = plan.CreateSchedule();
        var part = schedule.Append(train.AsTrainPart).Value;
        part.TractionOptions = new TractionOptions();
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 42, null);
        plan.AssignVehicle(schedule, loco, Sessions.All);

        Assert.Contains($"Couple {loco} to train.", NoteTextsAt(plan, Origin));
        Assert.Contains($"Uncouple {loco} from train.", NoteTextsAt(plan, Terminus));
    }

    [TestMethod]
    public void AnInstructionForSomeSessionsOnlyLeadsWithThem()
    {
        // The whole reason a dispatch row carries the qualifier: the person on duty is there every
        // session, and the locomotive is not.
        var (plan, train) = Arrange();
        var schedule = plan.CreateSchedule();
        var part = schedule.Append(train.AsTrainPart).Value;
        part.TractionOptions = new TractionOptions();
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 42, null);
        plan.AssignVehicle(schedule, loco, Sessions.FromSessionNumbers(1, 3, 5));

        Assert.Contains($"1,3,5: Couple {loco} to train.", NoteTextsAt(plan, Origin));
        Assert.Contains($"1,3,5: Uncouple {loco} from train.", NoteTextsAt(plan, Terminus));
    }

    [TestMethod]
    public void AWagonsetIsCoupledWhereItsPartStartsAndUncoupledWhereItEnds()
    {
        var (plan, train) = Arrange();
        var schedule = plan.CreateSchedule();
        var part = schedule.Append(train.AsTrainPart).Value;
        part.WagonSetOptions = new WagonSetOptions();
        var wagons = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Bo", 7, null);
        plan.AssignVehicle(schedule, wagons, Sessions.All);

        Assert.Contains($"Couple {wagons} to train.", NoteTextsAt(plan, Origin));
        Assert.Contains($"Uncouple {wagons} from train.", NoteTextsAt(plan, Terminus));
    }

    [TestMethod]
    public void ACargoFlowSaysWhereItsWagonsGoAtBothOfItsEnds()
    {
        var (plan, train) = Arrange();
        var elsewhere = plan.Layout.OperationLocations.Single(l => l.Name == Terminus);
        var options = plan.Timetable.Add(new CargoFlowOptions());
        options.Destinations.Add(new Destination { Location = elsewhere });
        var calls = train.CallsInRunOrder;
        train.CreateCargoFlow(1, calls[0], calls[^1], options);

        Assert.Contains($"Brings wagons to {elsewhere.Name}", NoteTextsAt(plan, Origin));
        Assert.Contains($"Uncouple wagons for {elsewhere.Name}.", NoteTextsAt(plan, Terminus));
    }

    [TestMethod]
    public void TheCargoNotesSitOnTheClearanceTheyBelongTo()
    {
        // The wagons go on before the train leaves and come off after it pulls in, so a station with two
        // rows for the train must not show either instruction against the wrong one.
        var (plan, train) = Arrange();
        var options = plan.Timetable.Add(new CargoFlowOptions { ToAllDestinations = true });
        var calls = train.CallsInRunOrder;
        train.CreateCargoFlow(1, calls[0], calls[^1], options);

        var origin = RowsAt(plan, Origin).Single();
        var terminus = RowsAt(plan, Terminus).Single();

        Assert.ContainsSingle(origin.Notes.OfType<CargoFlowDestinationNote>());
        Assert.IsEmpty(origin.Notes.OfType<CargoFlowUncoupleNote>());
        Assert.ContainsSingle(terminus.Notes.OfType<CargoFlowUncoupleNote>());
        Assert.IsEmpty(terminus.Notes.OfType<CargoFlowDestinationNote>());
    }
}
