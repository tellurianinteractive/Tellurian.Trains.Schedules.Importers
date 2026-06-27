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
    public void WagonGroupIsDeleted()
    {
        var calls = Train.Calls.ToList();
        var wagonGroup = new WagonGroup
        {
            Id = 1,
            FromStationCall = calls[0],
            FromStationCallId = calls[0].Id,
            ToStationCall = calls[^1],
            ToStationCallId = calls[^1].Id,
            PositionInTrain = 1,
        };
        Train.Add(wagonGroup);

        var result = Plan.TryDelete(wagonGroup);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(Train.WagonGroups.Contains(wagonGroup));
    }

    [TestMethod]
    public void DeletingStationCallRemovesDependentWagonGroups()
    {
        var calls = Train.Calls.ToList();
        var wagonGroup = new WagonGroup
        {
            Id = 1,
            FromStationCall = calls[0],
            FromStationCallId = calls[0].Id,
            ToStationCall = calls[^1],
            ToStationCallId = calls[^1].Id,
            PositionInTrain = 1,
        };
        Train.Add(wagonGroup);

        var result = Plan.TryDelete(calls[0]);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(Train.Calls.Contains(calls[0]));
        Assert.IsFalse(Train.WagonGroups.Contains(wagonGroup), "A wagon group that attached at the deleted call is removed.");
    }
}
