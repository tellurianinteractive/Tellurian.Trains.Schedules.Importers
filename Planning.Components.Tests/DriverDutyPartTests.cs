using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the vehicle blocks of a duty's train part page: the traction unit the driver is handed and
/// the wagonsets it hauls.
/// </summary>
[TestClass]
public class DriverDutyPartTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    private sealed record Fixture(Plan Plan, DriverDuty Duty, Schedule Schedule, ScheduledObject Loco);

    // A duty of one train part, worked by a schedule with one locomotive assigned to it. The train runs
    // on track 1 at both stations; each has a track 2 for vehicles fetched or put elsewhere.
    private static Fixture CreateFixture()
    {
        var layout = new Layout { Name = "Test" };
        var stations = new List<OperationLocation>();
        for (var i = 0; i < 2; i++)
        {
            var station = new Station(i + 1, $"Station{i + 1}", $"S{i + 1}");
            station.Add(new StationTrack((i + 1) * 10, "1"));
            station.Add(new StationTrack((i + 1) * 10 + 1, "2"));
            stations.Add(layout.Add(station));
        }
        layout.Add(new TrackStretch(1, stations[0], stations[1], 10));

        var timetable = new Timetable("Test", layout);
        var plan = Plan.Create("Test", timetable);

        var train = new Train(1, 100);
        var start = Time.FromHourAndMinute(6, 00);
        for (var c = 0; c < 2; c++)
        {
            var call = train.Add(new StationCall(c + 1, stations[c]["1"], start.AddMinutes(c * 5), start.AddMinutes(c * 5 + 1)));
            call.IsArrival = true;
            call.IsDeparture = true;
        }
        timetable.Add(train);

        var schedule = plan.CreateSchedule();
        schedule.Add(train.AsTrainPart);
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, loco);

        var duty = plan.CreateDriverDuty();
        duty.Add(schedule.OrderedParts[0]);

        return new Fixture(plan, duty, schedule, loco);
    }

    private static DriverDutyPart PartOf(Fixture fixture) => new()
    {
        TrainPart = fixture.Duty.OrderedParts[0],
        Duty = fixture.Duty,
        SessionsSettings = Settings,
    };

    [TestMethod]
    public void TheTractionBlockListsTheAssignedLocomotive()
    {
        var fixture = CreateFixture();

        var traction = PartOf(fixture).TractionData;

        Assert.IsTrue(traction.HasData, "The traction block must show the locomotive assigned to the part's schedule.");
        Assert.AreEqual(fixture.Loco.Designation, traction.Vehicles[0].Designation);
    }

    [TestMethod]
    public void TheTractionBlockListsTheLocomotiveForAPartThatDoesNotKnowItsSchedule()
    {
        // Plans stored before the Job import was made to share the schedule's part instances hold a
        // private copy in the duty, whose Schedule back-reference is null. The plan still matches it by
        // value — and the Duties editor resolves the vehicle that way — so the booklet must too, rather
        // than printing a part with no traction unit.
        var fixture = CreateFixture();
        var owned = fixture.Schedule.OrderedParts[0];
        var detached = new ScheduledTrainPart(owned.From, owned.To);
        fixture.Duty.Parts.Clear();
        fixture.Duty.Parts.Add(detached);

        var part = new DriverDutyPart { TrainPart = detached, Duty = fixture.Duty, SessionsSettings = Settings };

        Assert.IsNull(detached.Schedule, "The part in this case does not know its schedule.");
        Assert.IsTrue(part.TractionData.HasData, "The traction block must resolve the vehicle through the plan.");
        Assert.AreEqual(fixture.Loco.Designation, part.TractionData.Vehicles[0].Designation);
    }

    [TestMethod]
    public void TheTractionBlockSurvivesTheStoredPlanRoundTrip()
    {
        // The report reads the plan restored from browser storage, not the one just built in memory.
        var fixture = CreateFixture();
        var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, MaxDepth = 256 };
        var json = JsonSerializer.Serialize(fixture.Plan, options);
        var restored = JsonSerializer.Deserialize<Plan>(json, options)!;

        var duty = restored.DriverDuties.Single();
        var part = new DriverDutyPart
        {
            TrainPart = duty.OrderedParts[0],
            Duty = duty,
            SessionsSettings = Settings,
        };

        Assert.IsNotNull(part.TrainPart.Schedule, "A restored duty part must still know the schedule that owns it.");
        Assert.IsTrue(part.TractionData.HasData, "The traction block must survive the storage round trip.");
    }

    [TestMethod]
    public void AVehicleRowNamesTheTrainsTracksWhereNoOtherTrackIsGiven()
    {
        var fixture = CreateFixture();

        var row = PartOf(fixture).TractionData.Vehicles[0];

        Assert.AreEqual("1", row.DepartureTrack.Number);
        Assert.AreEqual("1", row.ArrivalTrack.Number);
    }

    [TestMethod]
    public void AVehicleRowNamesTheTracksItsScheduleFetchesItFromAndPutsItOn()
    {
        var fixture = CreateFixture();
        var owned = fixture.Schedule.OrderedParts[0];
        owned.FromTrack = owned.From.OperationLocation["2"];
        owned.ToTrack = owned.To.OperationLocation["2"];

        var row = PartOf(fixture).TractionData.Vehicles[0];

        Assert.AreEqual("2", row.DepartureTrack.Number);
        Assert.AreEqual("2", row.ArrivalTrack.Number);
    }

    [TestMethod]
    public void AVehicleRowIgnoresTheOtherTrackOfAnotherVehiclesSchedule()
    {
        // Parts are equal over the same two calls, so a wagonset's schedule covering the loco's part is
        // found for the loco's part too. Its fetch-from track concerns the wagonset only.
        var fixture = CreateFixture();
        var train = fixture.Schedule.OrderedParts[0].Train;
        var wagonSchedule = fixture.Plan.CreateSchedule();
        wagonSchedule.Add(train.AsTrainPart);
        var wagonset = fixture.Plan.CreateVehicle(ScheduledObjectType.Wagonset, "Bm", 1, null);
        fixture.Plan.AssignVehicle(wagonSchedule, wagonset);
        var wagonPart = wagonSchedule.OrderedParts[0];
        wagonPart.FromTrack = wagonPart.From.OperationLocation["2"];

        var part = PartOf(fixture);

        Assert.AreEqual("1", part.TractionData.Vehicles[0].DepartureTrack.Number, "The loco stands on the train's track.");
        Assert.AreEqual("2", part.WagonsetData.Vehicles[0].DepartureTrack.Number, "The wagonset is fetched from its own track.");
    }

    [TestMethod]
    public void PartsOfOneTrainWorkedInARowPrintAsOneTrain()
    {
        // The train is cut at the middle station: one loco to it and another from it, one wagonset
        // uncoupled there and another running through. The driver stays on the train throughout.
        var (duty, stations) = CreateDutyOverACutTrain();

        var printed = DutyPagination.PrintedParts(duty, Settings);

        Assert.HasCount(1, printed, "The two parts of one train must print as one train.");
        Assert.AreSame(stations[0], printed[0].TrainPart.From.OperationLocation);
        Assert.AreSame(stations[2], printed[0].TrainPart.To.OperationLocation);
    }

    [TestMethod]
    public void TheVehicleBlocksOfAJoinedTrainShowWhereEachVehicleIsChangedOrUncoupled()
    {
        var (duty, stations) = CreateDutyOverACutTrain();

        var printed = DutyPagination.PrintedParts(duty, Settings)[0];

        var traction = printed.TractionData.Vehicles;
        Assert.HasCount(2, traction);
        Assert.AreEqual("A", traction[0].Vehicle.Class);
        Assert.AreSame(stations[1], traction[0].TrainPart.To.OperationLocation, "The first loco is changed at the middle station.");
        Assert.AreEqual("B", traction[1].Vehicle.Class);
        Assert.AreSame(stations[1], traction[1].TrainPart.From.OperationLocation, "The second loco takes over at the middle station.");

        var wagonsets = printed.WagonsetData.Vehicles;
        Assert.HasCount(2, wagonsets);
        var uncoupled = wagonsets.Single(w => w.Vehicle.Number == 1);
        var through = wagonsets.Single(w => w.Vehicle.Number == 2);
        Assert.AreSame(stations[1], uncoupled.TrainPart.To.OperationLocation, "This wagonset is uncoupled at the middle station.");
        Assert.AreSame(stations[2], through.TrainPart.To.OperationLocation, "This wagonset runs the whole train.");
    }

    [TestMethod]
    public void TheTimetableOfAJoinedTrainIsWorkedThroughTheStationWhereItWasCut()
    {
        var (duty, _) = CreateDutyOverACutTrain();

        var rows = DutyPagination.PrintedParts(duty, Settings)[0].TimetableRows;

        Assert.IsTrue(rows.All(row => row.IsInPart), "No stretch of the train may show as not the driver's.");
    }

    // A three-station train cut into two parts at the middle station, both in one duty.
    private static (DriverDuty Duty, IReadOnlyList<OperationLocation> Stations) CreateDutyOverACutTrain()
    {
        var layout = new Layout { Name = "Test" };
        var stations = new List<OperationLocation>();
        for (var i = 0; i < 3; i++)
        {
            var station = new Station(i + 1, $"Station{i + 1}", $"S{i + 1}");
            station.Add(new StationTrack(i + 1, "1"));
            stations.Add(layout.Add(station));
        }
        layout.Add(new TrackStretch(1, stations[0], stations[1], 10));
        layout.Add(new TrackStretch(2, stations[1], stations[2], 10));

        var timetable = new Timetable("Test", layout);
        var plan = Plan.Create("Test", timetable);

        var train = new Train(1, 100);
        var start = Time.FromHourAndMinute(6, 00);
        for (var c = 0; c < 3; c++)
        {
            var call = train.Add(new StationCall(c + 1, stations[c]["1"], start.AddMinutes(c * 10), start.AddMinutes(c * 10 + 5)));
            call.IsArrival = true;
            call.IsDeparture = true;
        }
        timetable.Add(train);

        Schedule ScheduleFor(ScheduledTrainPart part, ScheduledObjectType type, string @class, int number)
        {
            var schedule = plan.CreateSchedule();
            schedule.Add(part);
            plan.AssignVehicle(schedule, plan.CreateVehicle(type, @class, number, null));
            return schedule;
        }

        var first = ScheduleFor(train.AsTrainPart(0, 1), ScheduledObjectType.Locomotive, "A", 1);
        var second = ScheduleFor(train.AsTrainPart(1, 2), ScheduledObjectType.Locomotive, "B", 2);
        ScheduleFor(train.AsTrainPart(0, 1), ScheduledObjectType.Wagonset, "Bm", 1);
        ScheduleFor(train.AsTrainPart(0, 2), ScheduledObjectType.Wagonset, "Bm", 2);

        var duty = plan.CreateDriverDuty();
        duty.Add(first.OrderedParts[0]);
        duty.Add(second.OrderedParts[0]);
        return (duty, stations);
    }
}
