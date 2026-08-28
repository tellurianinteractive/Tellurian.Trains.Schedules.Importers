using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// Covers copying a train that already exists: backwards along its own route, and repeatedly.
/// The plain same-direction copy is covered by <see cref="PlanMoveCloneTrainTests"/>.
/// </summary>
[TestClass]
public class PlanCloneTrainTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger, DefaultSpeed = 100 };

    private static Plan SimplePlan()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        return new Plan("Test", new Timetable("Test", layout));
    }

    private static OperationLocation Location(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature);

    private static Train TrainFromMalmö(Plan plan, int preparationMinutes = 10, int finishingMinutes = 10)
    {
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, preparationMinutes, finishingMinutes);
        Assert.IsNotNull(train);
        return train;
    }

    private static double SpanMinutes(Train train) =>
        (train.DriverEndTime.Value - train.DriverStartTime.Value).TotalMinutes;

    private static Time Departure(Train train) =>
        train.CallsInRunOrder.First(c => c.IsDeparture).Departure;

    [TestMethod]
    public void OppositeCloneRunsTheSameRouteBackwards()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);

        var clone = plan.Clone(train, 30, CloneDirection.Opposite);

        Assert.IsNotNull(clone);
        CollectionAssert.AreEqual(
            new[] { "M2", "Lu", "E", "Hm" },
            train.CallsInRunOrder.Select(c => c.OperationLocation.Signature).ToArray());
        CollectionAssert.AreEqual(
            new[] { "Hm", "E", "Lu", "M2" },
            clone.CallsInRunOrder.Select(c => c.OperationLocation.Signature).ToArray());
    }

    [TestMethod]
    public void OppositeCloneDepartsTheGivenMinutesAfterTheTrainsLastDeparture()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);

        var clone = plan.Clone(train, 30, CloneDirection.Opposite);

        Assert.IsNotNull(clone);
        Assert.AreEqual(train.DriverEndTime.AddMinutes(30), Departure(clone));
    }

    [TestMethod]
    public void OppositeCloneMirrorsEveryRunAndDwellSoItTakesAsLongAsTheTrain()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan, preparationMinutes: 10, finishingMinutes: 25);

        var clone = plan.Clone(train, 30, CloneDirection.Opposite);

        Assert.IsNotNull(clone);
        Assert.AreEqual(SpanMinutes(train), SpanMinutes(clone));

        // Each leg of the clone takes as long as the same leg took the other way round.
        var trainCalls = train.CallsInRunOrder;
        var cloneCalls = clone.CallsInRunOrder;
        for (var i = 1; i < cloneCalls.Count; i++)
        {
            var source = trainCalls[^(i + 1)];
            var previous = trainCalls[^i];
            Assert.AreEqual(
                (previous.Arrival.Value - source.Departure.Value).TotalMinutes,
                (cloneCalls[i].Arrival.Value - cloneCalls[i - 1].Departure.Value).TotalMinutes);
        }
    }

    [TestMethod]
    public void OppositeCloneKeepsThePreparationAndFinishingTimesAtItsOwnEnds()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan, preparationMinutes: 10, finishingMinutes: 25);

        var clone = plan.Clone(train, 30, CloneDirection.Opposite);

        Assert.IsNotNull(clone);
        // The times belong to the train, not to the stations: the clone is prepared where the train ended
        // and put away where the train began.
        Assert.AreEqual(10, clone.PreparationMinutes);
        Assert.AreEqual(25, clone.FinishingMinutes);
    }

    [TestMethod]
    public void OppositeCloneStartsWithADepartureAndEndsWithAnArrival()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);

        var clone = plan.Clone(train, 30, CloneDirection.Opposite);

        Assert.IsNotNull(clone);
        var calls = clone.CallsInRunOrder;
        Assert.IsFalse(calls[0].IsArrival);
        Assert.IsTrue(calls[0].IsDeparture);
        Assert.IsTrue(calls[^1].IsArrival);
        Assert.IsFalse(calls[^1].IsDeparture);
    }

    [TestMethod]
    public void OppositeCloneTakesTheOppositeDirectionsNumberAndKeepsTheTrainsParticulars()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, maxSpeed: 80);
        Assert.IsNotNull(train);

        var clone = plan.Clone(train, 30, CloneDirection.Opposite);

        Assert.IsNotNull(clone);
        // M2 → Hm runs upward and takes the next even number; running it backwards takes the next odd one.
        Assert.AreEqual(2, train.Number);
        Assert.AreEqual(1, clone.Number);
        Assert.AreEqual(train.Id + 1, clone.Id);
        Assert.AreEqual(train.Category, clone.Category);
        Assert.AreEqual(80, clone.MaxSpeed);
        Assert.AreEqual(train.Sessions, clone.Sessions);

        var originalCallIds = train.Calls.Select(c => c.Id).ToHashSet();
        Assert.IsFalse(clone.Calls.Any(c => originalCallIds.Contains(c.Id)));
    }

    [TestMethod]
    public void OppositeCloneIsNotAddedWhenItWouldFallOutsideTheOperatingWindow()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);

        // The plan's operating window ends at 20:00 by default.
        var clone = plan.Clone(train, 12 * 60, CloneDirection.Opposite);

        Assert.IsNull(clone);
        Assert.AreEqual(1, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void OppositeCloneIsNotMadeOfATrainWithNoRouteToRunBackwards()
    {
        var plan = SimplePlan();
        var train = new Train(1, Passenger, 1) { Sessions = Sessions.All };
        plan.Timetable.Add(train);
        var track = Location(plan, "M2").Tracks.First();
        train.Add(new StationCall(1, track, Start, Start));

        var clone = plan.Clone(train, 30, CloneDirection.Opposite);

        Assert.IsNull(clone);
        Assert.AreEqual(1, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void RepeatingCloneAddsTheFirstAtTheGivenOffsetAndOneEveryIntervalAfterIt()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);

        // 08:00 train, first copy at +30 = 08:30, then every hour up to 11:00 → 08:30, 09:30, 10:30.
        var clones = plan.CloneRepeating(train, 30, Time.FromHourAndMinute(11, 0), intervalMinutes: 60);

        Assert.AreEqual(3, clones.Count);
        CollectionAssert.AreEqual(
            new[] { 8 * 60.0 + 30, 9 * 60 + 30, 10 * 60 + 30 },
            clones.Select(t => Departure(t).Value.TotalMinutes).ToArray());
        // The train itself is not one of them, so the timetable holds it and its copies.
        Assert.AreEqual(4, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void RepeatingCloneAddsNothingWhenTheFirstCopyWouldDepartAfterEndTime()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);

        var clones = plan.CloneRepeating(train, 90, Time.FromHourAndMinute(9, 0), intervalMinutes: 60);

        Assert.AreEqual(0, clones.Count);
        Assert.AreEqual(1, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void RepeatingCloneInTheOppositeDirectionRepeatsFromTheTrainsLastDeparture()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);
        var back = train.DriverEndTime;

        var clones = plan.CloneRepeating(train, 20, back.AddMinutes(90), intervalMinutes: 60, CloneDirection.Opposite);

        Assert.AreEqual(2, clones.Count);
        CollectionAssert.AreEqual(
            new[] { back.AddMinutes(20), back.AddMinutes(80) },
            clones.Select(Departure).ToArray());
        // Every copy runs the train's route backwards.
        Assert.IsTrue(clones.All(c => c.CallsInRunOrder[0].OperationLocation.Signature == "Hm"));
    }

    [TestMethod]
    public void RepeatingCloneThrowsWhenIntervalIsNotPositive()
    {
        var plan = SimplePlan();
        var train = TrainFromMalmö(plan);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            plan.CloneRepeating(train, 30, Time.FromHourAndMinute(11, 0), intervalMinutes: 0));
    }
}
