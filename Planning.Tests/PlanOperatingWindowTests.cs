using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PlanOperatingWindowTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true, DefaultSpeed = 100 };

    private static Plan SimplePlan()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        return new Plan("Test", new Timetable("Test", layout));
    }

    private static OperationLocation Location(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature);

    private static Train CreateTrain(Plan plan) =>
        plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start)!;

    private static void SetWindow(Plan plan, TimeSpan start, TimeSpan end, bool runsOverMidnight = false)
    {
        plan.Layout.Settings.General.StartTime = start;
        plan.Layout.Settings.General.EndTime = end;
        plan.Layout.Settings.General.RunsOverMidnight = runsOverMidnight;
    }

    [TestMethod]
    public void IsWrappingMidnightIsDrivenByTheRunsOverMidnightFlag()
    {
        var plan = SimplePlan();

        // A plain whole-day window does not wrap on its own: without the flag a train may not spill
        // into the next day.
        SetWindow(plan, TimeSpan.Zero, new TimeSpan(23, 59, 0));
        Assert.IsFalse(plan.IsWrappingMidnight);

        // The flag alone makes the window wrap.
        plan.Layout.Settings.General.RunsOverMidnight = true;
        Assert.IsTrue(plan.IsWrappingMidnight);
    }

    [TestMethod]
    public void CreateReturnsNullAndAddsNothingWhenTheTrainRunsPastTheEndOfTheWindow()
    {
        var plan = SimplePlan();
        // The train departs at 08:00 and runs for the better part of an hour, so a window ending at 08:00
        // cannot contain it.
        SetWindow(plan, TimeSpan.FromHours(6), TimeSpan.FromHours(8));

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNull(train);
        Assert.AreEqual(0, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void MoveIsRejectedWhenItWouldTakeTheTrainPastTheEndOfTheWindow()
    {
        var plan = SimplePlan();
        SetWindow(plan, TimeSpan.FromHours(6), TimeSpan.FromHours(20));
        var train = CreateTrain(plan);
        var before = train.Calls.Select(c => (c.Arrival, c.Departure)).ToList();

        // Shift the start to 19:55, so the train necessarily finishes after 20:00.
        var minutes = 19 * 60 + 55 - (int)train.DriverStartTime.Value.TotalMinutes;
        var moved = plan.Move(train, minutes);

        Assert.IsNull(moved);
        for (var i = 0; i < train.Calls.Count; i++)
            Assert.AreEqual(before[i].Arrival, train.Calls[i].Arrival);
    }

    [TestMethod]
    public void MoveMayTakeTheTrainPastMidnightWhenTheWindowWrapsMidnight()
    {
        var plan = SimplePlan();
        SetWindow(plan, TimeSpan.Zero, new TimeSpan(23, 59, 0), runsOverMidnight: true);
        var train = CreateTrain(plan);

        // Shift the start to 23:55; the train then runs on past midnight, which is allowed only because the
        // window wraps midnight.
        var minutes = 23 * 60 + 55 - (int)train.DriverStartTime.Value.TotalMinutes;
        var moved = plan.Move(train, minutes);

        Assert.IsNotNull(moved);
        Assert.IsTrue(train.DriverEndTime.Value >= TimeSpan.FromHours(24),
            "The moved train should spill over midnight.");
    }

    [TestMethod]
    public void CloneMayTakeTheCopyPastMidnightWhenTheWindowWrapsMidnight()
    {
        var plan = SimplePlan();
        SetWindow(plan, TimeSpan.Zero, new TimeSpan(23, 59, 0), runsOverMidnight: true);
        var train = CreateTrain(plan);

        var minutes = 23 * 60 + 55 - (int)train.DriverStartTime.Value.TotalMinutes;
        var clone = plan.Clone(train, minutes);

        Assert.IsNotNull(clone);
        Assert.AreEqual(2, plan.Timetable.Trains.Count);
        Assert.IsTrue(clone.DriverEndTime.Value >= TimeSpan.FromHours(24),
            "The cloned train should spill over midnight.");
    }

    [TestMethod]
    public void CreateExtendsTheWindowInsteadOfRejectingWhenAllowPlanTimeExtendIsSet()
    {
        var plan = SimplePlan();
        // The train departs at 08:00 and runs for the better part of an hour, so a window ending at 08:00
        // would normally reject it.
        SetWindow(plan, TimeSpan.FromHours(6), TimeSpan.FromHours(8));
        plan.Layout.Settings.General.AllowPlanTimeExtend = true;

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.AreEqual(1, plan.Timetable.Trains.Count);
        Assert.IsTrue(plan.Layout.Settings.General.EndTime >= train.DriverEndTime.Value,
            "EndTime should have been widened to cover the train.");
    }

    [TestMethod]
    public void MoveExtendsTheWindowInsteadOfRejectingWhenAllowPlanTimeExtendIsSet()
    {
        var plan = SimplePlan();
        SetWindow(plan, TimeSpan.FromHours(6), TimeSpan.FromHours(20));
        var train = CreateTrain(plan);
        plan.Layout.Settings.General.AllowPlanTimeExtend = true;

        // Shift the start to 19:55, so the train necessarily finishes after 20:00.
        var minutes = 19 * 60 + 55 - (int)train.DriverStartTime.Value.TotalMinutes;
        var moved = plan.Move(train, minutes);

        Assert.IsNotNull(moved);
        Assert.IsTrue(plan.Layout.Settings.General.EndTime >= train.DriverEndTime.Value,
            "EndTime should have been widened to cover the moved train.");
    }

    [TestMethod]
    public void AllowPlanTimeExtendDoesNotAllowATrainToStartOutsideTheCalendarDay()
    {
        var plan = SimplePlan();
        SetWindow(plan, TimeSpan.FromHours(6), TimeSpan.FromHours(20));
        var train = CreateTrain(plan);
        plan.Layout.Settings.General.AllowPlanTimeExtend = true;

        // A shift past midnight would make the train start on the next calendar day, which stays
        // rejected even when AllowPlanTimeExtend is set.
        var minutes = (int)TimeSpan.FromHours(24).TotalMinutes - (int)train.DriverStartTime.Value.TotalMinutes + 5;
        var moved = plan.Move(train, minutes);

        Assert.IsNull(moved);
    }
}
