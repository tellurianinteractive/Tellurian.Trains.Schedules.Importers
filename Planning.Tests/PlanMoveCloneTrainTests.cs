using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PlanMoveCloneTrainTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true, DefaultSpeed = 100 };

    private static Plan PlanWithTrain(out Train train)
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        var plan = new Plan("Test", new Timetable("Test", layout));
        var from = plan.Layout.OperationLocations.First(l => l.Signature == "M2");
        var to = plan.Layout.OperationLocations.First(l => l.Signature == "Hm");
        train = plan.Create(Passenger, from, to, Start)!;
        Assert.IsNotNull(train);
        return plan;
    }

    [TestMethod]
    public void MoveShiftsEveryCallForwardByTheGivenMinutes()
    {
        var plan = PlanWithTrain(out var train);
        var before = train.Calls.Select(c => (c.Arrival, c.Departure)).ToList();

        var moved = plan.Move(train, 30);

        Assert.AreSame(train, moved);
        for (var i = 0; i < train.Calls.Count; i++)
        {
            Assert.AreEqual(before[i].Arrival.AddMinutes(30), train.Calls[i].Arrival);
            Assert.AreEqual(before[i].Departure.AddMinutes(30), train.Calls[i].Departure);
        }
    }

    [TestMethod]
    public void MoveShiftsEveryCallBackwardForNegativeMinutes()
    {
        var plan = PlanWithTrain(out var train);
        var before = train.Calls.Select(c => (c.Arrival, c.Departure)).ToList();

        var moved = plan.Move(train, -15);

        Assert.IsNotNull(moved);
        for (var i = 0; i < train.Calls.Count; i++)
            Assert.AreEqual(before[i].Arrival.AddMinutes(-15), train.Calls[i].Arrival);
    }

    [TestMethod]
    public void MoveReturnsNullAndLeavesTrainUnchangedWhenStartWouldFallBeforeMidnight()
    {
        var plan = PlanWithTrain(out var train);
        var before = train.Calls.Select(c => (c.Arrival, c.Departure)).ToList();

        // The train starts at about 07:50; moving 10 hours earlier would take it before 00:00.
        var moved = plan.Move(train, -10 * 60);

        Assert.IsNull(moved);
        for (var i = 0; i < train.Calls.Count; i++)
        {
            Assert.AreEqual(before[i].Arrival, train.Calls[i].Arrival);
            Assert.AreEqual(before[i].Departure, train.Calls[i].Departure);
        }
    }

    [TestMethod]
    public void CloneAddsANewTrainWithNextIdAndNumberAndShiftedTimes()
    {
        var plan = PlanWithTrain(out var train);

        var clone = plan.Clone(train, 60);

        Assert.IsNotNull(clone);
        Assert.AreNotSame(train, clone);
        Assert.IsTrue(plan.Timetable.Trains.Contains(clone));
        Assert.AreEqual(2, plan.Timetable.Trains.Count);
        Assert.AreEqual(train.Id + 1, clone.Id);
        Assert.AreEqual(train.Number + 1, clone.Number);
        Assert.AreEqual(train.Category, clone.Category);
        Assert.AreEqual(train.Sessions, clone.Sessions);

        Assert.AreEqual(train.Calls.Count, clone.Calls.Count);
        for (var i = 0; i < clone.Calls.Count; i++)
        {
            Assert.AreEqual(train.Calls[i].Arrival.AddMinutes(60), clone.Calls[i].Arrival);
            Assert.AreEqual(train.Calls[i].Departure.AddMinutes(60), clone.Calls[i].Departure);
            Assert.AreEqual(train.Calls[i].IsArrival, clone.Calls[i].IsArrival);
            Assert.AreEqual(train.Calls[i].IsDeparture, clone.Calls[i].IsDeparture);
            Assert.AreEqual(train.Calls[i].Track, clone.Calls[i].Track);
        }
    }

    [TestMethod]
    public void CloneGivesEachCallANewUniqueId()
    {
        var plan = PlanWithTrain(out var train);

        var clone = plan.Clone(train, 60);

        Assert.IsNotNull(clone);
        var originalIds = train.Calls.Select(c => c.Id).ToHashSet();
        Assert.IsFalse(clone.Calls.Any(c => originalIds.Contains(c.Id)));
    }

    [TestMethod]
    public void CloneDoesNotAlterTheOriginalTrain()
    {
        var plan = PlanWithTrain(out var train);
        var before = train.Calls.Select(c => (c.Arrival, c.Departure)).ToList();

        plan.Clone(train, 60);

        for (var i = 0; i < train.Calls.Count; i++)
        {
            Assert.AreEqual(before[i].Arrival, train.Calls[i].Arrival);
            Assert.AreEqual(before[i].Departure, train.Calls[i].Departure);
        }
    }

    [TestMethod]
    public void CloneReturnsNullAndAddsNothingWhenStartWouldFallBeforeMidnight()
    {
        var plan = PlanWithTrain(out var train);

        var clone = plan.Clone(train, -10 * 60);

        Assert.IsNull(clone);
        Assert.AreEqual(1, plan.Timetable.Trains.Count);
    }
}
