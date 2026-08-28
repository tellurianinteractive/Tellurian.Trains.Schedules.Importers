using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// The dwell a train is given where its route reverses. A locomotive has to be run round to the other
/// end of its train there, which takes time; a trainset, and a locomotive working a reversible train,
/// simply changes cab and stands no longer than at any other stop.
/// </summary>
[TestClass]
public class PlanReversalDwellTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Freight => new() { Id = 2, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo, DefaultSpeed = 100 };

    // Malmö → Växjö reverses at Munkeröd (Mkd), the only station on the route that permits it. A start
    // time can be given where the test lengthens the run and needs room inside the operating window.
    private static (Plan Plan, Train Train) PlanWithReversingTrain(Time? start = null)
    {
        var plan = TestLayoutFactory.CreatePlan();
        var train = plan.Create(Freight,
            plan.Layout.OperationLocations.First(l => l.Signature == "M"),
            plan.Layout.OperationLocations.First(l => l.Signature == "Vö"), start ?? Start);
        Assert.IsNotNull(train);
        return (plan, train);
    }

    // Puts the whole train into a new schedule and books the given vehicle to work it.
    private static ScheduledObject Assign(Plan plan, Train train, ScheduledObjectType type, bool isReversibleTrain = false)
    {
        var schedule = plan.CreateSchedule();
        Assert.IsTrue(schedule.Append(train.AsTrainPart).HasValue);
        var vehicle = plan.CreateVehicle(type, "Test", 1, company: null);
        vehicle.IsReversibleTrain = isReversibleTrain;
        Assert.IsTrue(plan.AssignVehicle(schedule, vehicle).HasValue);
        return vehicle;
    }

    private static StationCall Reversal(Train train) =>
        train.Calls.Single(c => c.OperationLocation.Signature == "Mkd");

    private static int DwellMinutes(StationCall call) =>
        (int)(call.Departure.Value - call.Arrival.Value).TotalMinutes;

    private static int RunaroundMinutes(Plan plan)
    {
        var settings = plan.Layout.Settings.TimeAndSpeed;
        return (settings.StationTimings.LocoRunaroundRealMinutes ?? 5) * settings.FastClockSpeed;
    }

    private static int MinimumStopMinutes(Plan plan) =>
        plan.Layout.Settings.TimeAndSpeed.StationTimings.MinimumStopMinutes ?? 3;

    [TestMethod]
    public void ATrainWithNoVehicleAssignedKeepsTheRunaroundAllowance()
    {
        var (plan, train) = PlanWithReversingTrain();

        Assert.IsNotNull(plan.UpdateTimings(train));

        Assert.AreEqual(RunaroundMinutes(plan), DwellMinutes(Reversal(train)));
    }

    [TestMethod]
    public void ATrainWorkedByAPlainLocomotiveKeepsTheRunaroundAllowance()
    {
        var (plan, train) = PlanWithReversingTrain();
        Assign(plan, train, ScheduledObjectType.Locomotive);

        Assert.IsNotNull(plan.UpdateTimings(train));

        Assert.AreEqual(RunaroundMinutes(plan), DwellMinutes(Reversal(train)));
    }

    [TestMethod]
    public void ATrainWorkedByALocomotiveOnAReversibleTrainStandsOnlyTheMinimumStop()
    {
        var (plan, train) = PlanWithReversingTrain();
        Assign(plan, train, ScheduledObjectType.Locomotive, isReversibleTrain: true);

        Assert.IsNotNull(plan.UpdateTimings(train));

        Assert.AreEqual(MinimumStopMinutes(plan), DwellMinutes(Reversal(train)));
    }

    [TestMethod]
    public void ATrainWorkedByATrainsetStandsOnlyTheMinimumStop()
    {
        var (plan, train) = PlanWithReversingTrain();
        Assign(plan, train, ScheduledObjectType.Trainset);

        Assert.IsNotNull(plan.UpdateTimings(train));

        Assert.AreEqual(MinimumStopMinutes(plan), DwellMinutes(Reversal(train)));
    }

    [TestMethod]
    public void TheRunaroundAllowanceReturnsWhenTheLocomotiveNoLongerWorksAReversibleTrain()
    {
        var (plan, train) = PlanWithReversingTrain();
        var loco = Assign(plan, train, ScheduledObjectType.Locomotive, isReversibleTrain: true);
        Assert.IsNotNull(plan.UpdateTimings(train));
        Assert.AreEqual(MinimumStopMinutes(plan), DwellMinutes(Reversal(train)), "Precondition: the allowance was dropped.");

        loco.IsReversibleTrain = false;
        Assert.IsNotNull(plan.UpdateTimings(train));

        Assert.AreEqual(RunaroundMinutes(plan), DwellMinutes(Reversal(train)));
    }

    [TestMethod]
    public void AStandLongerThanTheRunaroundAllowanceIsKeptAtAReversal()
    {
        // An hour earlier than the other tests, so the lengthened run still fits the operating window.
        var (plan, train) = PlanWithReversingTrain(Time.FromHourAndMinute(7, 0));
        Assign(plan, train, ScheduledObjectType.Locomotive, isReversibleTrain: true);
        // A deliberate layover, longer than any allowance the planner would have computed here. Set the
        // way the planner sets it, so the rest of the run follows and the calls stay in run order.
        var deliberate = RunaroundMinutes(plan) + 15;
        var reversal = Reversal(train);
        Assert.IsNotNull(plan.SetDeparture(reversal, reversal.Arrival.AddMinutes(deliberate)));

        Assert.IsNotNull(plan.UpdateTimings(train));

        Assert.AreEqual(deliberate, DwellMinutes(Reversal(train)));
    }

    [TestMethod]
    public void DoubleHeadingWithOneOrdinaryLocomotiveKeepsTheRunaroundAllowance()
    {
        var (plan, train) = PlanWithReversingTrain();
        var schedule = plan.CreateSchedule();
        Assert.IsTrue(schedule.Append(train.AsTrainPart).HasValue);
        var reversible = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Test", 1, company: null);
        reversible.IsReversibleTrain = true;
        var ordinary = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Test", 2, company: null);
        Assert.IsTrue(plan.AssignVehicle(schedule, reversible).HasValue);
        Assert.IsTrue(plan.AssignVehicle(schedule, ordinary).HasValue);

        Assert.IsNotNull(plan.UpdateTimings(train));

        Assert.AreEqual(RunaroundMinutes(plan), DwellMinutes(Reversal(train)),
            "One locomotive that must be run round is enough to need the time for it.");
    }
}
