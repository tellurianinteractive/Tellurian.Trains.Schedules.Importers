using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PlanCreateReturnTrainTests
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

    private static Time Departure(Train train) =>
        train.CallsInRunOrder.First(c => c.IsDeparture).Departure;

    [TestMethod]
    public void ReturnRunsTheSameRouteBackwards()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);
        Assert.IsNotNull(train);

        var back = plan.CreateReturn(train);

        Assert.IsNotNull(back);
        CollectionAssert.AreEqual(
            new[] { "Hm", "E", "Lu", "M2" },
            back.Calls.Select(c => c.OperationLocation.Signature).ToArray());
    }

    [TestMethod]
    public void AsSoonAsPossibleDepartsAfterFinishingTheTrainAndPreparingTheReturn()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, preparationMinutes: 10, finishingMinutes: 15);
        Assert.IsNotNull(train);

        var back = plan.CreateReturn(train, departureTime: null, preparationMinutes: 10, finishingMinutes: 15);

        Assert.IsNotNull(back);
        var arrival = train.CallsInRunOrder[^1].Arrival;
        Assert.AreEqual(arrival.AddMinutes(15 + 10), Departure(back));
        // The return's own preparation puts its driver on duty exactly when the first train's driver finishes.
        Assert.AreEqual(train.DriverEndTime, back.DriverStartTime);
    }

    [TestMethod]
    public void AnEnteredDepartureTimeIsUsedAsGiven()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);
        Assert.IsNotNull(train);

        var back = plan.CreateReturn(train, Time.FromHourAndMinute(14, 30));

        Assert.IsNotNull(back);
        Assert.AreEqual(Time.FromHourAndMinute(14, 30), Departure(back));
    }

    [TestMethod]
    public void TheReturnMayDepartBeforeTheTrainItReturnsFrom()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);
        Assert.IsNotNull(train);

        var back = plan.CreateReturn(train, Time.FromHourAndMinute(7, 0));

        Assert.IsNotNull(back);
        Assert.AreEqual(Time.FromHourAndMinute(7, 0), Departure(back));
    }

    [TestMethod]
    public void ReturnKeepsTheCategoryAndSpeedButTakesTheOppositeDirectionNumber()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, maxSpeed: 80);
        Assert.IsNotNull(train);

        var back = plan.CreateReturn(train);

        Assert.IsNotNull(back);
        Assert.AreEqual(Passenger.Id, back.Category?.Id);
        Assert.AreEqual(80, back.MaxSpeed);
        // M2 → Hm runs upward and takes the next even number; the return runs downward and takes the next odd one.
        Assert.AreEqual(2, train.Number);
        Assert.AreEqual(1, back.Number);
    }

    [TestMethod]
    public void ReturnIsNotCreatedWhenItsDepartureFallsOutsideThePlansOperatingTimes()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);
        Assert.IsNotNull(train);

        // The plan's operating window ends at 20:00 by default.
        var back = plan.CreateReturn(train, Time.FromHourAndMinute(19, 55));

        Assert.IsNull(back);
        Assert.AreEqual(1, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void CreateWithReturnAddsBothTrains()
    {
        var plan = SimplePlan();

        var trains = plan.CreateWithReturn(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.AreEqual(2, trains.Count);
        Assert.AreEqual(2, plan.Timetable.Trains.Count);
        Assert.AreEqual("M2", trains[0].CallsInRunOrder[0].OperationLocation.Signature);
        Assert.AreEqual("Hm", trains[1].CallsInRunOrder[0].OperationLocation.Signature);
    }

    [TestMethod]
    public void CreateWithReturnAddsNothingWhenTheReturnDoesNotFit()
    {
        var plan = SimplePlan();

        var trains = plan.CreateWithReturn(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start,
            returnTime: Time.FromHourAndMinute(19, 55));

        Assert.AreEqual(0, trains.Count);
        Assert.AreEqual(0, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void CreateWithReturnAddsNothingWhenNoPathBetweenLocations()
    {
        var plan = SimplePlan();
        var same = Location(plan, "M2");

        var trains = plan.CreateWithReturn(Passenger, same, same, Start);

        Assert.AreEqual(0, trains.Count);
        Assert.AreEqual(0, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void CreateRepeatingWithReturnRepeatsBothDirections()
    {
        var plan = SimplePlan();

        // 08:00 outbound, one per two hours until 12:00 → 08, 10, 12 outbound; the return departs later,
        // so it fits one repetition fewer before the same end time.
        var trains = plan.CreateRepeatingWithReturn(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(12, 0), intervalMinutes: 120);

        Assert.AreEqual(trains.Count, plan.Timetable.Trains.Count);
        var outbound = trains.Where(t => t.CallsInRunOrder[0].OperationLocation.Signature == "M2").ToList();
        var back = trains.Where(t => t.CallsInRunOrder[0].OperationLocation.Signature == "Hm").ToList();
        Assert.AreEqual(3, outbound.Count);
        Assert.AreEqual(2, back.Count);
        CollectionAssert.AreEqual(
            new[] { 8 * 60.0, 10 * 60, 12 * 60 },
            outbound.Select(t => Departure(t).Value.TotalMinutes).ToArray());
    }

    [TestMethod]
    public void CreateRepeatingWithReturnNumbersEachDirectionPairwise()
    {
        var plan = SimplePlan();

        var trains = plan.CreateRepeatingWithReturn(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(12, 0), intervalMinutes: 120);

        // Both trains are created before either is cloned, so the outbound trains take the even numbers
        // and their returns the odd ones, pairing up as 2/1, 4/3, 6/5.
        var outbound = trains.Where(t => t.CallsInRunOrder[0].OperationLocation.Signature == "M2").ToList();
        var back = trains.Where(t => t.CallsInRunOrder[0].OperationLocation.Signature == "Hm").ToList();
        CollectionAssert.AreEqual(new[] { 2, 4, 6 }, outbound.Select(t => t.Number).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 3 }, back.Select(t => t.Number).ToArray());
    }

    [TestMethod]
    public void CreateRepeatingWithReturnAddsNothingWhenTheReturnDoesNotFit()
    {
        var plan = SimplePlan();

        var trains = plan.CreateRepeatingWithReturn(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(12, 0), intervalMinutes: 120,
            returnTime: Time.FromHourAndMinute(19, 55));

        Assert.AreEqual(0, trains.Count);
        Assert.AreEqual(0, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void CreateRepeatingWithReturnThrowsWhenIntervalIsNotPositive()
    {
        var plan = SimplePlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => plan.CreateRepeatingWithReturn(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(12, 0), intervalMinutes: 0));
    }
}
