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

    // A duty of one train part, worked by a schedule with one locomotive assigned to it.
    private static Fixture CreateFixture()
    {
        var layout = new Layout { Name = "Test" };
        var stations = new List<OperationLocation>();
        for (var i = 0; i < 2; i++)
        {
            var station = new Station(i + 1, $"Station{i + 1}", $"S{i + 1}");
            station.Add(new StationTrack((i + 1) * 10, "1"));
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
}
