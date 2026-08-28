using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// Creating a shunting task: a train of a shunting category with the one call that carries the span of
/// work, at one operating location and travelling nowhere (<c>Plan.CreateShuntingTask</c>).
/// </summary>
[TestClass]
public class PlanCreateShuntingTaskTests
{
    private static readonly Time Start = Time.FromHourAndMinute(14, 0);

    private static TrainCategory Shunting => new()
    {
        Id = 3,
        Name = "Shunting",
        Prefix = "V",
        Content = TrainContent.Cargo,
        IsShunting = true,
        StartNumber = 9000,
    };

    private static TrainCategory Freight => new() { Id = 2, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo, StartNumber = 5000 };

    private static Plan SimplePlan()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        var plan = new Plan("Test", new Timetable("Test", layout));
        plan.Layout.Settings.General.AllowPlanTimeExtend = true;
        return plan;
    }

    private static OperationLocation Location(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature);

    [TestMethod]
    public void TheTaskHasOneCallSpanningTheGivenDuration()
    {
        var plan = SimplePlan();

        var task = plan.CreateShuntingTask(Shunting, Location(plan, "Lu"), Start, 45);

        Assert.IsNotNull(task);
        var call = task.Calls.Single();
        Assert.AreEqual(Start, call.Arrival, "The arrival is when the work starts.");
        Assert.AreEqual(Start.AddMinutes(45), call.Departure, "The departure is when it ends.");
        Assert.IsTrue(call.IsArrival, "The call is the task's origin as well as its destination.");
        Assert.IsTrue(call.IsDeparture);
        Assert.AreEqual("Lu", call.OperationLocation.Signature);
        Assert.IsTrue(task.IsShuntingTask);
        Assert.Contains(task, plan.Timetable.Trains);
    }

    [TestMethod]
    public void TheTaskTakesTheNextFreeNumberInItsCategorysBandWhateverItsParity()
    {
        var plan = SimplePlan();
        var category = Shunting;

        var first = plan.CreateShuntingTask(category, Location(plan, "Lu"), Start, 30);
        var second = plan.CreateShuntingTask(category, Location(plan, "E"), Start, 30);

        // Every number of the band is free to a task: parity encodes a travelling direction, which a
        // task worked at one place does not have.
        Assert.AreEqual(9000, first!.Number);
        Assert.AreEqual(9001, second!.Number);
    }

    [TestMethod]
    public void ACategoryThatIsNotAShuntingOneIsRefused()
    {
        var plan = SimplePlan();

        Assert.IsNull(plan.CreateShuntingTask(Freight, Location(plan, "Lu"), Start, 30));
        Assert.IsEmpty(plan.Timetable.Trains);
    }

    [TestMethod]
    public void ATaskOutsideTheOperatingWindowIsNotAdded()
    {
        var plan = SimplePlan();
        plan.Layout.Settings.General.AllowPlanTimeExtend = false;
        plan.Layout.Settings.General.StartTime = TimeSpan.FromHours(6);
        plan.Layout.Settings.General.EndTime = TimeSpan.FromHours(22);

        var task = plan.CreateShuntingTask(Shunting, Location(plan, "Lu"), Time.FromHourAndMinute(21, 45), 60);

        Assert.IsNull(task);
        Assert.IsEmpty(plan.Timetable.Trains);
    }

    [TestMethod]
    public void ADurationOfZeroOrLessIsRejected()
    {
        var plan = SimplePlan();

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => plan.CreateShuntingTask(Shunting, Location(plan, "Lu"), Start, 0));
    }
}
