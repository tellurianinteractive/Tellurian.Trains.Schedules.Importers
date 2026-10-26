using System.Globalization;
using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a runaround is worked out from the route — a train leaving in the opposite direction to
/// the one it arrived from, without the locomotive changing — and that the planner can overrule it.
/// </summary>
[TestClass]
public class RunaroundDetectionTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // One locomotive (or trainset) working a train out and the train back, which leaves the terminus the
    // way it came. Returns the call where the first train ends.
    private static (Plan Plan, ScheduledTrainPart Out, ScheduledTrainPart Back) Shuttle(
        ScheduledObjectType vehicleType = ScheduledObjectType.Locomotive, bool isReversibleTrain = false, bool carriesWagons = true)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        var plan = Plan.Create("Test", timetable);

        var schedule = plan.CreateSchedule();
        foreach (var train in plan.Timetable.Trains.OrderBy(t => t.Number)) schedule.Append(train.AsTrainPart);
        var vehicle = plan.CreateVehicle(vehicleType, "T44", 42, null);
        vehicle.IsReversibleTrain = isReversibleTrain;
        plan.AssignVehicle(schedule, vehicle, Sessions.All);
        // A locomotive with no wagonset has no train to run round, so it works one.
        if (carriesWagons && vehicleType == ScheduledObjectType.Locomotive)
            plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 99, null), Sessions.All);
        return (plan, schedule.OrderedParts[0], schedule.OrderedParts[1]);
    }

    private static IEnumerable<ICallNote> Notes(Plan plan, StationCall call) =>
        call.DriverNotes(Sessions.All, Settings, plan);

    [TestMethod]
    public void ALocomotiveSetOffTheWayItCameIsToldToCirculateWithoutAnyoneAskingFor()
    {
        var (plan, outward, _) = Shuttle();

        Assert.ContainsSingle(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ALocomotiveWithNoWagonsetIsASingleLocoAndNeverRunsRound()
    {
        var (plan, outward, _) = Shuttle(carriesWagons: false);

        Assert.IsFalse(outward.NeedsRunaround);
        Assert.IsFalse(plan.NeedsLocoRunaround(outward.Train));
        Assert.IsEmpty(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ALocomotiveOnATrainWithACargoFlowRunsRoundEvenWithNoWagonsetAssigned()
    {
        var (plan, outward, _) = Shuttle(carriesWagons: false);
        var options = plan.Timetable.Add(new CargoFlowOptions());
        _ = outward.Train.CreateCargoFlow(1, outward.From, outward.To, options);

        Assert.IsTrue(outward.NeedsRunaround);
        Assert.IsTrue(plan.NeedsLocoRunaround(outward.Train));
        Assert.ContainsSingle(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ACirculationIsNotMadeWhereTheTrainEndsAndNothingFollows()
    {
        var (plan, outward, _) = Shuttle();

        Assert.IsEmpty(Notes(plan, outward.From).OfType<CirculateNote>());
    }

    [TestMethod]
    public void NeverTakesTheDetectedRunaroundAway()
    {
        var (plan, outward, _) = Shuttle();
        outward.TractionOptions = new TractionOptions { Runaround = RunaroundMode.Never };

        Assert.IsEmpty(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    [TestMethod]
    public void NeverOnTheCallAlsoTakesItAway()
    {
        var (plan, outward, _) = Shuttle();
        outward.To.Runaround = RunaroundMode.Never;

        Assert.IsEmpty(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    [TestMethod]
    public void NeverKeepsTheTurnWhenBothWereAsked()
    {
        var (plan, outward, _) = Shuttle();
        outward.TractionOptions = new TractionOptions { TurnLoco = true };
        outward.To.Runaround = RunaroundMode.Never;

        var notes = Notes(plan, outward.To).ToList();
        Assert.ContainsSingle(notes.OfType<TurnNote>());
        Assert.IsEmpty(notes.OfType<TurnAndCirculateNote>());
    }

    [TestMethod]
    public void AlwaysGivesARunaroundWhereTheRouteDoesNotCallForOne()
    {
        var (plan, outward, _) = Shuttle();
        outward.From.Runaround = RunaroundMode.Always;

        Assert.ContainsSingle(Notes(plan, outward.From).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ATurnAndTheDetectedRunaroundGiveOneNote()
    {
        var (plan, outward, _) = Shuttle();
        outward.TractionOptions = new TractionOptions { TurnLoco = true };

        var notes = Notes(plan, outward.To).ToList();
        Assert.ContainsSingle(notes.OfType<TurnAndCirculateNote>());
        Assert.IsEmpty(notes.OfType<CirculateNote>());
    }

    [TestMethod]
    public void ATrainsetIsNeverToldToCirculate()
    {
        var (plan, outward, _) = Shuttle(ScheduledObjectType.Trainset);

        Assert.IsEmpty(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ALocomotiveOnAReversibleTrainIsNeverToldToCirculate()
    {
        var (plan, outward, _) = Shuttle(isReversibleTrain: true);

        Assert.IsEmpty(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ALocomotiveGoingToStablingIsNotToldToCirculate()
    {
        var (plan, outward, _) = Shuttle();
        outward.TractionOptions = new TractionOptions { ToLayover = TractionLayover.Stabling };

        Assert.IsEmpty(Notes(plan, outward.To).OfType<CirculateNote>());
    }

    // A train that runs out to the middle station and back again, so it reverses there.
    private static (Plan Plan, StationCall Reversal) OutAndBack()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var stations = TestDataFactory.Stations.ToArray();
        var start = Time.FromHourAndMinute(12, 00);
        var train = new Train(1, category, 1) { Category = category };
        _ = train.Add(new StationCall(1, stations[0]["3"], start, start));
        _ = train.Add(new StationCall(2, stations[1]["2"], start.AddMinutes(25), start.AddMinutes(30)) { IsArrival = true, IsDeparture = true });
        _ = train.Add(new StationCall(3, stations[0]["1"], start.AddMinutes(55), start.AddMinutes(55)));
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(train);
        var plan = Plan.Create("Test", timetable);
        return (plan, plan.Timetable.Trains.Single().CallsInRunOrder[1]);
    }

    [TestMethod]
    public void AReversalInsideATrainGivesACirculateNote()
    {
        var (plan, reversal) = OutAndBack();

        Assert.IsTrue(reversal.ReversesDirection(plan));
        Assert.ContainsSingle(Notes(plan, reversal).OfType<CirculateNote>());
    }

    [TestMethod]
    public void NeverOnAReversalInsideATrainTakesTheNoteAway()
    {
        var (plan, reversal) = OutAndBack();
        reversal.Runaround = RunaroundMode.Never;

        Assert.IsEmpty(Notes(plan, reversal).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ATrainRunningStraightOnIsNotToldToCirculate()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        var plan = Plan.Create("Test", timetable);
        var middle = plan.Timetable.Trains.Single().CallsInRunOrder[1];

        Assert.IsFalse(middle.ReversesDirection(plan));
        Assert.IsEmpty(Notes(plan, middle).OfType<CirculateNote>());
    }

    [TestMethod]
    public void APlanSavedWithTheOldYesIsReadAsAlways()
    {
        var options = JsonSerializer.Deserialize<TractionOptions>("""{"RunaroundLoco":true}""")!;

        Assert.AreEqual(RunaroundMode.Always, options.Runaround);
    }

    [TestMethod]
    public void TheOldYesOrNoIsNotWrittenBack()
    {
        var json = JsonSerializer.Serialize(new TractionOptions { Runaround = RunaroundMode.Never });

        Assert.DoesNotContain("RunaroundLoco", json);
        Assert.AreEqual(RunaroundMode.Never, JsonSerializer.Deserialize<TractionOptions>(json)!.Runaround);
    }
}
