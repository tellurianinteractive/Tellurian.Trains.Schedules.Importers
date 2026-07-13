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
