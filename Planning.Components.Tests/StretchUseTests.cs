using Tellurian.Trains.Schedules.Planning.Components.Scheduling;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies that the legs the graphical timetable draws are the legs the train runs. A train's
/// <see cref="Train.Calls"/> is in insertion order, which on a hand-edited train is not the order it runs
/// them, so pairing consecutive calls from that list would draw lines the train never travels.
/// </summary>
[TestClass]
public class StretchUseTests
{
    // A —10m— B —10m— C, single track each. The train runs A 08:00 → B 08:20/08:25 → C 08:45, but its
    // calls are added in another order: the middle stop B first, then the origin A, then the terminus C.
    private static Train CreateTrainWithCallsAddedOutOfRunOrder()
    {
        var a = new Station(1, "Alpha", "A");
        a.Add(new StationTrack(11, "1"));
        var b = new Station(2, "Beta", "B");
        b.Add(new StationTrack(21, "1"));
        var c = new Station(3, "Cesar", "C");
        c.Add(new StationTrack(31, "1"));

        var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
        var train = new Train(1, category, 1) { Category = category };
        _ = train.Add(new StationCall(1, b["1"], Time.FromHourAndMinute(8, 20), Time.FromHourAndMinute(8, 25)));
        _ = train.Add(new StationCall(2, a["1"], Time.FromHourAndMinute(8, 00), Time.FromHourAndMinute(8, 00)));
        _ = train.Add(new StationCall(3, c["1"], Time.FromHourAndMinute(8, 45), Time.FromHourAndMinute(8, 45)));
        return train;
    }

    [TestMethod]
    public void StretchUsesArePairsOfCallsInRunOrder()
    {
        var train = CreateTrainWithCallsAddedOutOfRunOrder();

        var uses = train.StretchUses().ToList();

        Assert.HasCount(2, uses);
        Assert.AreEqual("A", uses[0].From.OperationLocation.Signature, "The first leg starts where the train starts its run.");
        Assert.AreEqual("B", uses[0].To.OperationLocation.Signature);
        Assert.AreEqual("B", uses[1].From.OperationLocation.Signature);
        Assert.AreEqual("C", uses[1].To.OperationLocation.Signature, "The last leg ends where the train ends its run.");
    }

    [TestMethod]
    public void EveryStretchUseRunsForwardsInTime()
    {
        var train = CreateTrainWithCallsAddedOutOfRunOrder();

        foreach (var use in train.StretchUses())
        {
            Assert.IsTrue(use.From.Departure < use.To.Arrival,
                $"A leg the train runs departs before it arrives, but {use.From} to {use.To} does not.");
        }
    }
}
