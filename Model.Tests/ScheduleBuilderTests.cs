namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class ScheduleBuilderTests
{
    private static Plan CreatePlanWithForwardAndReturn(bool excludeCategory = false)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P", ExcludeFromAutomaticScheduling = excludeCategory };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        // Forward 12:00→12:55 (G→Snu), return 13:00→13:55 (Snu→G): the return continues the forward run.
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        return Plan.Create("Test", timetable);
    }

    // The return run Snu 13:00 → Yb 13:25/13:30 → G 13:55, with its calls added in another order than it
    // runs them: the intermediate stop first, then the origin, then the terminus.
    private static Train CreateReturnWithCallsAddedOutOfRunOrder(TrainCategory category)
    {
        var stations = TestDataFactory.Stations.ToArray();
        var train = new Train(2, category, 2) { Category = category };
        _ = train.Add(new StationCall(1, stations[1]["1"], Time.FromHourAndMinute(13, 25), Time.FromHourAndMinute(13, 30)));
        _ = train.Add(new StationCall(2, stations[2]["2"], Time.FromHourAndMinute(13, 00), Time.FromHourAndMinute(13, 00)));
        _ = train.Add(new StationCall(3, stations[0]["3"], Time.FromHourAndMinute(13, 55), Time.FromHourAndMinute(13, 55)));
        return train;
    }

    [TestMethod]
    public void ATrainIsChainedByWhereItRunsFromNotByItsFirstAddedCall()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00))); // G 12:00 → Snu 12:55
        timetable.Add(CreateReturnWithCallsAddedOutOfRunOrder(category));
        var plan = Plan.Create("Test", timetable);

        var schedules = plan.BuildSchedulesAutomatically().Created;

        Assert.HasCount(1, schedules,
            "The return train starts its run at Snu, where the forward train ends, though its Snu call was not the one added first.");
        Assert.HasCount(2, schedules[0].Parts);
    }

    [TestMethod]
    public void ChainsSameCategoryContinuationIntoOneSchedule()
    {
        var plan = CreatePlanWithForwardAndReturn();

        var schedules = plan.BuildSchedulesAutomatically().Created;

        Assert.HasCount(1, schedules);
        Assert.HasCount(2, schedules[0].Parts);
        Assert.AreEqual(1, schedules[0].Number, "Number defaults to the first train's number.");
    }

    [TestMethod]
    public void AddsBuiltSchedulesToThePlan()
    {
        var plan = CreatePlanWithForwardAndReturn();

        plan.BuildSchedulesAutomatically();

        Assert.HasCount(1, plan.Schedules);
    }

    [TestMethod]
    public void ExcludedCategoryTrainsAreNotScheduled()
    {
        var plan = CreatePlanWithForwardAndReturn(excludeCategory: true);

        var schedules = plan.BuildSchedulesAutomatically().Created;

        Assert.IsEmpty(schedules);
    }

    [TestMethod]
    public void AlreadyAssignedTrainsAreNotScheduledAgain()
    {
        var plan = CreatePlanWithForwardAndReturn();
        plan.BuildSchedulesAutomatically();

        var second = plan.BuildSchedulesAutomatically();

        Assert.IsEmpty(second.Created, "No unassigned trains remain after the first build.");
        Assert.AreEqual(0, second.AddedParts, "There is nothing left to give the schedules built by the first run.");
    }

    [TestMethod]
    public void ContinuationsForEmptyScheduleReturnsAllUnassignedTrains()
    {
        var plan = CreatePlanWithForwardAndReturn();

        var continuations = plan.ContinuationsFor(new Schedule(1));

        Assert.HasCount(2, continuations);
    }

    [TestMethod]
    public void ContinuationsForScheduleReturnsOnlyTrainsThatContinueIt()
    {
        var plan = CreatePlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First();
        var schedule = new Schedule(1);
        schedule.Add(forward.AsTrainPart);
        plan.AddVehicleSchedule(schedule);

        var continuations = plan.ContinuationsFor(schedule);

        Assert.HasCount(1, continuations);
        Assert.AreEqual(2, continuations[0].Number, "Only the return train continues from the end station.");
    }

    [TestMethod]
    public void ExtendsAnExistingScheduleInsteadOfCreatingANewOne()
    {
        var plan = CreatePlanWithForwardAndReturn();
        var schedule = ScheduleWorking(plan, trainNumber: 1);

        var built = plan.BuildSchedulesAutomatically();

        Assert.IsEmpty(built.Created, "The return train continues the existing working, so no new schedule is needed.");
        Assert.HasCount(1, built.Extended);
        Assert.AreEqual(1, built.AddedParts);
        Assert.HasCount(2, schedule.Parts);
    }

    [TestMethod]
    public void FillsAnEmptyExistingScheduleBeforeCreatingANewOne()
    {
        var plan = CreatePlanWithForwardAndReturn();
        var schedule = plan.CreateSchedule();

        var built = plan.BuildSchedulesAutomatically();

        Assert.IsEmpty(built.Created, "The empty schedule the planner made takes the work instead.");
        Assert.AreEqual(2, built.AddedParts);
        Assert.HasCount(2, schedule.Parts);
        Assert.HasCount(1, plan.Schedules);
    }

    [TestMethod]
    public void ATrainThatNoExistingScheduleCanTakeStillGetsItsOwn()
    {
        var plan = CreatePlanWithForwardAndReturn();
        // An earlier forward run G 09:00 → Snu 09:55: it departs long before the existing working arrives
        // back at G, so that working cannot take it however much it would like to.
        plan.Timetable.Add(TestDataFactory.CreateTrainInForwardDirection(
            plan.Timetable.Trains.First().Category!, 3, Time.FromHourAndMinute(09, 00)));
        var schedule = ScheduleWorking(plan, trainNumber: 1);

        var built = plan.BuildSchedulesAutomatically();

        Assert.HasCount(2, schedule.Parts, "The existing working picks first and takes the return train.");
        Assert.HasCount(1, built.Created);
        Assert.HasCount(1, built.Created[0].Parts, "Only the early train is left to start a new working from.");
    }

    [TestMethod]
    public void AnExistingScheduleIsNotExtendedWithAnotherCategory()
    {
        var plan = CreatePlanWithForwardAndFreightReturn();
        var schedule = ScheduleWorking(plan, trainNumber: 1);

        var built = plan.BuildSchedulesAutomatically();

        Assert.HasCount(1, schedule.Parts, "A passenger working is not continued as a freight train.");
        Assert.AreEqual(0, built.AddedParts);
        Assert.HasCount(1, built.Created, "The freight train starts a working of its own instead.");
    }

    [TestMethod]
    public void AnExistingScheduleIsNotExtendedWithAnExcludedCategory()
    {
        var plan = CreatePlanWithForwardAndReturn(excludeCategory: true);
        var schedule = ScheduleWorking(plan, trainNumber: 1);

        var built = plan.BuildSchedulesAutomatically();

        Assert.HasCount(1, schedule.Parts);
        Assert.AreEqual(0, built.AddedParts, "The category is excluded from automatic scheduling.");
        Assert.IsEmpty(built.Created);
    }

    // A schedule already in the plan, working the given train whole.
    private static Schedule ScheduleWorking(Plan plan, int trainNumber)
    {
        var schedule = new Schedule(1);
        schedule.Add(plan.Timetable.Trains.Single(t => t.Number == trainNumber).AsTrainPart);
        plan.AddVehicleSchedule(schedule);
        return schedule;
    }

    // Forward 12:00→12:55 (G→Snu) as a passenger train, return 13:00→13:55 (Snu→G) as a freight train: the
    // return continues the forward run in time and place, but not as the same kind of train.
    private static Plan CreatePlanWithForwardAndFreightReturn()
    {
        TestDataFactory.Init();
        var passenger = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var freight = new TrainCategory { Id = 2, Name = "G", Prefix = "G" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(passenger, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(freight, 2, Time.FromHourAndMinute(13, 00)));
        return Plan.Create("Test", timetable);
    }
}
