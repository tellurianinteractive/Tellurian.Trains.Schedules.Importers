using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class DeletionRulesTests
{
    private Plan Plan = default!;
    private Train Train = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        Plan = Plan.Create("Test", timetable);
        Train = timetable.Trains.First();
    }

    [TestMethod]
    public void TrainIsDeletedWhenNotReferenced()
    {
        var result = Plan.TryDelete(Train);
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(Plan.Timetable.Trains.Contains(Train));
    }

    [TestMethod]
    public void DeletingTrainRemovesItsCallsFromTheirTracks()
    {
        var call = Train.Calls.First();
        var track = call.Track;
        Assert.IsTrue(track.Calls.Contains(call), "Precondition: the track holds the call.");

        Plan.TryDelete(Train);

        Assert.IsFalse(track.Calls.Contains(call));
    }

    [TestMethod]
    public void TrainCannotBeDeletedWhenUsedByAVehicleSchedule()
    {
        var schedule = new Schedule(1);
        schedule.Add(Train.AsTrainPart);
        Plan.AddVehicleSchedule(schedule);

        var result = Plan.TryDelete(Train);

        Assert.IsInstanceOfType<DeletionResult.Failure>(result);
        Assert.IsTrue(Plan.Timetable.Trains.Contains(Train), "The train is left untouched when referenced.");
    }

    [TestMethod]
    public void CallInTheMiddleOfTheRouteCannotBeDeleted()
    {
        var middle = OrderedCalls()[1];

        var result = Plan.TryDelete(middle);

        Assert.IsInstanceOfType<DeletionResult.NotAllowed>(result);
        Assert.HasCount(3, Train.Calls, "The train keeps a call at every operation location on its route.");
        Assert.IsTrue(middle.Track.Calls.Contains(middle), "The track keeps the call too.");
    }

    [TestMethod]
    public void EndCallCannotBeDeletedWhenUsedByAVehicleSchedule()
    {
        var schedule = new Schedule(1);
        schedule.Add(Train.AsTrainPart);
        Plan.AddVehicleSchedule(schedule);

        var result = Plan.TryDelete(OrderedCalls()[0]);

        Assert.IsInstanceOfType<DeletionResult.Failure>(result);
        Assert.HasCount(3, Train.Calls, "The call is left untouched when referenced.");
    }

    [TestMethod]
    public void DeletingFirstCallMakesTheNextOneTheOriginAndMovesThePreparationTimeToIt()
    {
        var calls = OrderedCalls();
        // Ten minutes to prepare the train before it departs at 12:00.
        calls[0].Arrival = Time.FromHourAndMinute(11, 50);
        calls[0].Departure = Time.FromHourAndMinute(12, 00);
        var newFirst = calls[1];

        var result = Plan.TryDelete(calls[0]);

        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, Train.Calls);
        Assert.IsFalse(calls[0].Track.Calls.Contains(calls[0]), "The deleted call leaves its track.");
        Assert.IsTrue(newFirst.IsDeparture);
        Assert.IsFalse(newFirst.IsArrival, "The train now starts here, so it only departs.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 30), newFirst.Departure, "The departure is the anchor and stays put.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 20), newFirst.Arrival, "Preparation now starts ten minutes before this departure.");
    }

    [TestMethod]
    public void DeletingLastCallMakesThePreviousOneTheTerminusAndMovesTheFinishingTimeToIt()
    {
        var calls = OrderedCalls();
        // Ten minutes to finish the train up after it arrives at 12:55.
        calls[2].Arrival = Time.FromHourAndMinute(12, 55);
        calls[2].Departure = Time.FromHourAndMinute(13, 05);
        var newLast = calls[1];

        var result = Plan.TryDelete(calls[2]);

        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, Train.Calls);
        Assert.IsFalse(calls[2].Track.Calls.Contains(calls[2]), "The deleted call leaves its track.");
        Assert.IsTrue(newLast.IsArrival);
        Assert.IsFalse(newLast.IsDeparture, "The train now ends here, so it only arrives.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 25), newLast.Arrival, "The arrival is the anchor and stays put.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 35), newLast.Departure, "Finishing up now ends ten minutes after this arrival.");
    }

    [TestMethod]
    public void ARouteCanBeShortenedFromTheEndUntilNoCallIsLeft()
    {
        var calls = OrderedCalls();

        Plan.TryDelete(calls[2]);
        Plan.TryDelete(calls[1]);
        var result = Plan.TryDelete(calls[0]);

        Assert.IsTrue(result.IsSuccess, "The last remaining call is both the first and the last.");
        Assert.IsEmpty(Train.Calls);
    }

    [TestMethod]
    public void CargoFlowOptionsCannotBeDeletedWhenReferencedByACargoFlow()
    {
        var description = Plan.Timetable.Add(new CargoFlowOptions { OnlyWagonClasses = "Coal" });
        var calls = Train.Calls.OrderBy(c => c.SortTime).ToList();
        Train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        var result = Plan.TryDelete(description);

        Assert.IsInstanceOfType<DeletionResult.Failure>(result);
        Assert.IsTrue(Plan.Timetable.CargoFlowOptions.Contains(description), "The description is left untouched when referenced.");
    }

    [TestMethod]
    public void CargoFlowOptionsIsDeletedWhenNotReferenced()
    {
        var description = Plan.Timetable.Add(new CargoFlowOptions { OnlyWagonClasses = "Coal" });

        var result = Plan.TryDelete(description);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(Plan.Timetable.CargoFlowOptions.Contains(description));
    }

    [TestMethod]
    public void CargoFlowTrainPartIsDeleted()
    {
        var description = Plan.Timetable.Add(new CargoFlowOptions { OnlyWagonClasses = "Coal" });
        var calls = Train.Calls.OrderBy(c => c.SortTime).ToList();
        var cargoFlow = Train.CreateCargoFlow(1, calls.First(), calls.Last(), description);

        var result = Plan.TryDelete(cargoFlow);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(Train.CargoFlows.Contains(cargoFlow));
    }

    // Train.Calls is in insertion order; the route order is by SortTime.
    private List<StationCall> OrderedCalls() => [.. Train.Calls.OrderBy(c => c.SortTime)];
}
