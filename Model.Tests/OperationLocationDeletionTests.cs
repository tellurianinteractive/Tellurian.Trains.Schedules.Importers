using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

// The test layout is a line Göteborg (G) – Ytterby (Yb) – Stenungsund (Snu), with one timetable stretch
// along it, and two trains running its whole length G 12:00 → Yb 12:25/12:30 → Snu 12:55.
[TestClass]
public class OperationLocationDeletionTests
{
    private Plan Plan = default!;
    private Layout Layout = default!;
    private OperationLocation G = default!, Yb = default!, Snu = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        Plan = Plan.Create("Test", timetable);
        Layout = timetable.Layout;
        G = Layout.Station("G").Value;
        Yb = Layout.Station("Yb").Value;
        Snu = Layout.Station("Snu").Value;
    }

    [TestMethod]
    public void LocationOnALineIsReplacedByAStretchJoiningItsNeighbours()
    {
        var result = Plan.TryDelete(Yb);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(Layout.OperationLocations.Contains(Yb));
        var stretch = Layout.TrackStretches.Single();
        Assert.AreEqual(G, stretch.Start);
        Assert.AreEqual(Snu, stretch.End);
        Assert.AreEqual(20, stretch.Distance, "As long as the two stretches it replaces together.");
    }

    [TestMethod]
    public void TrainsRunningThroughTheLocationKeepTheirOtherCallsAndTimes()
    {
        Plan.TryDelete(Yb);

        foreach (var train in Plan.Timetable.Trains)
        {
            var calls = train.CallsInRunOrder;
            Assert.HasCount(2, calls);
            Assert.AreEqual(G, calls[0].OperationLocation);
            Assert.AreEqual(Time.FromHourAndMinute(12, 00), calls[0].Departure);
            Assert.AreEqual(Snu, calls[1].OperationLocation);
            Assert.AreEqual(Time.FromHourAndMinute(12, 55), calls[1].Arrival);
        }
        Assert.IsTrue(Yb.Tracks.All(t => t.Calls.Count == 0), "The removed calls leave their tracks too.");
    }

    [TestMethod]
    public void TimetableStretchRunsOverTheReplacement()
    {
        Plan.TryDelete(Yb);

        var route = Layout.TimetableStretches.Single();
        Assert.AreEqual(G, route.Starts);
        Assert.AreEqual(Snu, route.Ends);
        Assert.HasCount(1, route.Stretches);
        Assert.AreSame(Layout.TrackStretches.Single(), route.Stretches.Single());
    }

    [TestMethod]
    public void StretchAlreadyJoiningTheNeighboursIsReusedRatherThanDuplicated()
    {
        var existing = Layout.Add(new TrackStretch(9, G, Snu, 15));

        var preview = Plan.PreviewDelete(Yb);
        Plan.TryDelete(Yb);

        Assert.IsFalse(preview.IsReplacementNew);
        Assert.AreSame(existing, Layout.TrackStretches.Single());
        Assert.AreSame(existing, Layout.TimetableStretches.Single().Stretches.Single());
    }

    [TestMethod]
    public void PreviewChangesNothing()
    {
        var preview = Plan.PreviewDelete(Yb);

        Assert.IsTrue(preview.IsAllowed);
        Assert.HasCount(2, preview.Trains);
        Assert.IsTrue(preview.Trains.All(t => t.Kind == TrainCallRemovalKind.Intermediate));
        Assert.HasCount(2, preview.RemovedStretches);
        Assert.IsTrue(preview.IsReplacementNew);
        Assert.HasCount(1, preview.ChangedTimetableStretches);
        Assert.IsTrue(Layout.OperationLocations.Contains(Yb));
        Assert.HasCount(2, Layout.TrackStretches);
        Assert.IsTrue(Plan.Timetable.Trains.All(t => t.Calls.Count == 3));
    }

    [TestMethod]
    public void TrainStartingAtTheLocationNowStartsAtItsNextCall()
    {
        var train = Plan.Timetable.Trains.First();
        var first = train.CallsInRunOrder[0];
        first.Arrival = Time.FromHourAndMinute(11, 50);

        var result = Plan.TryDelete(G);

        Assert.IsTrue(result.IsSuccess);
        var newFirst = train.CallsInRunOrder[0];
        Assert.AreEqual(Yb, newFirst.OperationLocation);
        Assert.IsFalse(newFirst.IsArrival);
        Assert.AreEqual(Time.FromHourAndMinute(12, 20), newFirst.Arrival, "The ten minutes of preparation move to the new origin.");
        Assert.AreEqual(Yb, Layout.TrackStretches.Single().Start, "A terminus has no neighbours to join, so its stretch just goes.");
        Assert.AreEqual(Yb, Layout.TimetableStretches.Single().Starts);
    }

    [TestMethod]
    public void TrainLeftWithASingleCallIsDeleted()
    {
        Plan.TryDelete(Snu);
        var preview = Plan.PreviewDelete(Yb);

        var result = Plan.TryDelete(Yb);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(preview.Trains.All(t => t.IsDeleted));
        Assert.IsEmpty(Plan.Timetable.Trains);
        Assert.IsEmpty(Layout.TrackStretches);
        Assert.IsEmpty(Layout.TimetableStretches, "A timetable stretch with no route left goes too.");
    }

    [TestMethod]
    public void VehicleScheduleStartingAtTheLocationBlocksTheDelete()
    {
        var schedule = new Schedule(1);
        schedule.Add(Plan.Timetable.Trains.First().AsTrainPart);
        Plan.AddVehicleSchedule(schedule);

        var result = Plan.TryDelete(G);

        Assert.IsInstanceOfType<DeletionResult.Failure>(result);
        Assert.IsTrue(Layout.OperationLocations.Contains(G), "The layout is left untouched.");
        Assert.HasCount(3, Plan.Timetable.Trains.First().Calls);
    }

    [TestMethod]
    public void VehicleScheduleRunningThroughTheLocationDoesNotBlockIt()
    {
        var schedule = new Schedule(1);
        schedule.Add(Plan.Timetable.Trains.First().AsTrainPart);
        Plan.AddVehicleSchedule(schedule);

        Assert.IsTrue(Plan.TryDelete(Yb).IsSuccess);
    }

    [TestMethod]
    public void TrainReversingAtTheLocationBlocksTheDelete()
    {
        var train = new Train(99, 99);
        _ = train.Add(new StationCall(1, G["1"], Time.FromHourAndMinute(14, 00), Time.FromHourAndMinute(14, 00)));
        _ = train.Add(new StationCall(2, Yb["1"], Time.FromHourAndMinute(14, 20), Time.FromHourAndMinute(14, 40)));
        _ = train.Add(new StationCall(3, G["2"], Time.FromHourAndMinute(15, 00), Time.FromHourAndMinute(15, 00)));
        Plan.Timetable.Add(train);

        var preview = Plan.PreviewDelete(Yb);

        Assert.IsFalse(preview.IsAllowed);
        Assert.IsTrue(preview.Trains.Single(t => t.Train.Equals(train)).IsBlocked);
        Assert.IsTrue(Plan.TryDelete(Yb).IsDenied);
    }

    [TestMethod]
    public void TrackRoutesAtTheNeighboursNowNameTheLocationBeyond()
    {
        var track = G["1"];
        track.NextLocationId = Yb.Id;

        Plan.TryDelete(Yb);

        Assert.AreEqual(Snu.Id, track.NextLocationId);
    }

    [TestMethod]
    public void LocationsControlledFromTheDeletedOneAreLeftUncontrolled()
    {
        var controlled = new SignalControlledLocation(10, "Block", "Bl") { ControlledBy = (Station)Yb };
        Layout.Add(controlled);

        var preview = Plan.PreviewDelete(Yb);
        Plan.TryDelete(Yb);

        Assert.Contains(controlled, preview.NoLongerControlled);
        Assert.IsNull(controlled.ControlledBy);
    }

    [TestMethod]
    public void DispatchStretchesAreGeneratedAgain()
    {
        foreach (var station in Layout.OperationLocations.OfType<Station>()) station.IsManned = true;
        Layout.DispatchStretches = Layout.CreateDispatchStretches();

        Plan.TryDelete(Yb);

        Assert.IsTrue(Layout.DispatchStretches.All(d => !d.From.Equals(Yb) && !d.To.Equals(Yb)));
        Assert.IsTrue(Layout.DispatchStretches.Any(d => d.From.Equals(G) && d.To.Equals(Snu)));
    }
}
