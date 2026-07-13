namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class SessionCombinationTests
{
    // Forward 12:00 G->Snu (12:55), return 13:00 Snu->G (13:55): the return continues the forward run.
    private static Plan CreatePlan()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        return Plan.Create("Test", timetable);
    }

    private static Train Forward(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 1);
    private static Train Return(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 2);

    [TestMethod]
    public void SplitsOneScheduleWhereADailyTrainRunsBeyondTheOthersIntoTwoCards()
    {
        var plan = CreatePlan();
        Forward(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5); // runs 1-5
        Return(plan).Sessions = Sessions.All;                                 // runs every session
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        schedule.Append(Return(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, vehicle); // the whole period

        var combinations = vehicle.SessionCombinations(useDays: false, maxSessions: 7);

        Assert.HasCount(2, combinations);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, combinations[0].Sessions.Numbers);
        Assert.HasCount(2, combinations[0].Parts, "On 1-5 the vehicle works both trains.");
        CollectionAssert.AreEqual(new byte[] { 6, 7 }, combinations[1].Sessions.Numbers);
        Assert.HasCount(1, combinations[1].Parts, "On 6-7 only the daily train runs.");
        Assert.AreEqual(Return(plan), combinations[1].Parts.Single().Train);
    }

    [TestMethod]
    public void OneCardPerAssignmentWhenDailyTrainsAreSplitAcrossAlternatingSessions()
    {
        var plan = CreatePlan(); // both trains default to Sessions.All (daily)
        var odd = plan.CreateSchedule();
        odd.Append(Forward(plan).AsTrainPart);
        var even = plan.CreateSchedule();
        even.Append(Return(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(odd, vehicle, Sessions.FromSessionNumbers(1, 3, 5, 7));
        plan.AssignVehicle(even, vehicle, Sessions.FromSessionNumbers(2, 4, 6));

        var combinations = vehicle.SessionCombinations(useDays: false, maxSessions: 7);

        Assert.HasCount(2, combinations);
        CollectionAssert.AreEqual(new byte[] { 1, 3, 5, 7 }, combinations[0].Sessions.Numbers);
        Assert.AreEqual(Forward(plan), combinations[0].Parts.Single().Train);
        CollectionAssert.AreEqual(new byte[] { 2, 4, 6 }, combinations[1].Sessions.Numbers);
        Assert.AreEqual(Return(plan), combinations[1].Parts.Single().Train);
    }

    [TestMethod]
    public void OneCardWhenTheVehicleWorksTheSamePartsEveryDay()
    {
        var plan = CreatePlan(); // both daily
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        schedule.Append(Return(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, vehicle);

        var combinations = vehicle.SessionCombinations(useDays: false, maxSessions: 5);

        Assert.HasCount(1, combinations);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, combinations[0].Sessions.Numbers);
        Assert.HasCount(2, combinations[0].Parts);
    }

    [TestMethod]
    public void PartsAreOrderedAsTheVehicleWorksThem()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart); // departs 12:00
        schedule.Append(Return(plan).AsTrainPart);  // departs 13:00
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, vehicle);

        var parts = vehicle.SessionCombinations(useDays: false, maxSessions: 5).Single().Parts;

        CollectionAssert.AreEqual(new[] { Forward(plan), Return(plan) }, parts.Select(p => p.Train).ToList(),
            "The earlier-departing part comes first.");
    }

    [TestMethod]
    public void SessionsCoveredByNoAssignmentYieldNoCard()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, vehicle, Sessions.FromSessionNumbers(1, 2)); // works only 1-2

        var combinations = vehicle.SessionCombinations(useDays: false, maxSessions: 7);

        Assert.HasCount(1, combinations, "Only the assigned sessions produce a card.");
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, combinations[0].Sessions.Numbers);
    }

    [TestMethod]
    public void AnUnassignedVehicleHasNoCombinations()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);

        Assert.IsEmpty(vehicle.SessionCombinations(useDays: false, maxSessions: 7));
    }

    [TestMethod]
    public void DayLayoutSplitsIntoDayCombinationsRenderableAsWeekdays()
    {
        var plan = CreatePlan();
        Forward(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5); // first five days
        Return(plan).Sessions = Sessions.All;                                 // every day
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        schedule.Append(Return(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, vehicle);

        var combinations = vehicle.SessionCombinations(useDays: true, maxSessions: 7);

        Assert.HasCount(2, combinations);
        Assert.AreEqual(5, combinations[0].Sessions.DayCount, "Five operating days on the first card.");
        Assert.AreEqual(2, combinations[1].Sessions.DayCount, "The remaining two days on the second card.");
    }
}
