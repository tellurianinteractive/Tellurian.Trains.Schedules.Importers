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

        var schedules = plan.BuildSchedulesAutomatically();

        Assert.HasCount(1, schedules,
            "The return train starts its run at Snu, where the forward train ends, though its Snu call was not the one added first.");
        Assert.HasCount(2, schedules[0].Parts);
    }

    [TestMethod]
    public void ChainsSameCategoryContinuationIntoOneSchedule()
    {
        var plan = CreatePlanWithForwardAndReturn();

        var schedules = plan.BuildSchedulesAutomatically();

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

        var schedules = plan.BuildSchedulesAutomatically();

        Assert.IsEmpty(schedules);
    }

    [TestMethod]
    public void AlreadyAssignedTrainsAreNotScheduledAgain()
    {
        var plan = CreatePlanWithForwardAndReturn();
        plan.BuildSchedulesAutomatically();

        var second = plan.BuildSchedulesAutomatically();

        Assert.IsEmpty(second, "No unassigned trains remain after the first build.");
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
}
