using Tellurian.Trains.Schedules.Model.Timetables;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// The route of a train is the order of its calls' <c>Sequence</c>, fixed when a call joins the train, not
/// worked out from the times — so editing a time cannot move a call to another place in the route.
/// </summary>
[TestClass]
public class CallSequenceTests
{
    private static Time At(int hour, int minute) => Time.FromHourAndMinute(hour, minute);

    private static (Train Train, StationCall[] Calls) ThreeCallTrain()
    {
        var a = new Station(1, "Alpha", "A");
        a.Add(new StationTrack(11, "1"));
        var b = new Station(2, "Beta", "B");
        b.Add(new StationTrack(21, "1"));
        var c = new Station(3, "Cesar", "C");
        c.Add(new StationTrack(31, "1"));
        var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
        var train = new Train(1, category, 1) { Category = category };
        var calls = new[]
        {
            train.Add(new StationCall(1, a["1"], At(8, 0), At(8, 0))),
            train.Add(new StationCall(2, b["1"], At(8, 20), At(8, 25))),
            train.Add(new StationCall(3, c["1"], At(8, 45), At(8, 45))),
        };
        return (train, calls);
    }

    [TestMethod]
    public void CallsAreNumberedInTheOrderTheirTimesPlaceThemWhenAdded()
    {
        var (train, calls) = ThreeCallTrain();

        CollectionAssert.AreEqual(calls, train.CallsInRunOrder.ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, calls.Select(c => c.Sequence).ToArray());
    }

    [TestMethod]
    public void ACallAddedOutOfOrderGoesWhereItsTimesPutIt()
    {
        var (train, calls) = ThreeCallTrain();
        var late = train.Calls.First().Track;
        var between = train.Add(new StationCall(4, late, At(8, 30), At(8, 30)));

        CollectionAssert.AreEqual(new[] { calls[0], calls[1], between, calls[2] }, train.CallsInRunOrder.ToArray());
    }

    [TestMethod]
    public void ACallAddedLastIsLastWhateverItsTimes()
    {
        var (train, calls) = ThreeCallTrain();
        var added = train.AddLast(new StationCall(4, calls[0].Track, At(7, 0), At(7, 0)));

        Assert.AreSame(added, train.CallsInRunOrder[^1]);
    }

    [TestMethod]
    public void ChangingATimeDoesNotMoveACallInTheRoute()
    {
        var (train, calls) = ThreeCallTrain();

        calls[1].Arrival = At(9, 30);
        calls[1].Departure = At(9, 40);

        CollectionAssert.AreEqual(calls, train.CallsInRunOrder.ToArray(), "Beta is still the middle of the route.");
    }

    [TestMethod]
    public void CallsNotNumberedYetAreOrderedByTimeUntilTheTrainNumbersThem()
    {
        var (train, calls) = ThreeCallTrain();
        foreach (var call in calls) call.Sequence = 0;
        calls[0].Arrival = At(9, 0);
        calls[0].Departure = At(9, 0);

        Assert.AreSame(calls[0], train.CallsInRunOrder[^1], "Without numbers the times decide, as they did before numbers existed.");
        Assert.IsTrue(train.NumberCalls());
        calls[0].Arrival = At(7, 0);
        calls[0].Departure = At(7, 0);
        Assert.AreSame(calls[0], train.CallsInRunOrder[^1], "Numbered, the route no longer follows the times.");
        Assert.IsFalse(train.NumberCalls(), "Numbering a numbered train changes nothing.");
    }
}
