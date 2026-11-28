namespace Tellurian.Trains.Schedules.Model.Tests;

// The test layout is a line Göteborg (G) – Ytterby (Yb) – Stenungsund (Snu), 10 m between each pair, with one
// timetable stretch along it, and two trains running G 12:00 → Yb 12:25/12:30 (track 2) → Snu 12:55. A newer version of the layout, as from a newer hall plan, is compared with it and the
// differences the planner confirms are applied.
[TestClass]
public class LayoutChangeRulesTests
{
    private Plan Plan = default!;
    private Layout Layout = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        Plan = Plan.Create("Test", timetable);
        Layout = timetable.Layout;
    }

    /// <summary>A newer layout: the current one's locations and stretches in the given order, changed by <paramref name="change"/>.</summary>
    private Layout Newer(string[] line, IReadOnlyDictionary<(string, string), double>? distances = null, Action<Layout>? change = null)
    {
        var newer = new Layout();
        foreach (var signature in line)
        {
            var existing = Layout.OperationLocations.FirstOrDefault(l => l.Signature == signature);
            var location = existing?.Copy() ?? new Station(90 + newer.OperationLocations.Count, $"New {signature}", signature);
            if (existing is null) location.Tracks.Add(new StationTrack(91, "1") { Station = location });
            location.Layout = newer;
            newer.OperationLocations.Add(location);
        }
        for (var i = 0; i < line.Length - 1; i++)
        {
            var start = newer.OperationLocations.Single(l => l.Signature == line[i]);
            var end = newer.OperationLocations.Single(l => l.Signature == line[i + 1]);
            var distance = distances?.GetValueOrDefault((line[i], line[i + 1]), 10) ?? 10;
            newer.TrackStretches.Add(new TrackStretch(i + 1, start, end, distance) { Speed = 100, Layout = newer });
        }
        change?.Invoke(newer);
        return newer;
    }

    private OperationLocation At(string signature) => Layout.OperationLocations.Single(l => l.Signature == signature);

    private LayoutChange Single(Layout newer, LayoutChangeKind kind) =>
        LayoutComparison.Compare(Layout, newer).Changes.Single(c => c.Kind == kind);

    /// <summary>The train that starts at <paramref name="signature"/>.</summary>
    private Train StartingAt(string signature) => Plan.Timetable.Trains.First(t => t.CallsInRunOrder[0].OperationLocation.Signature == signature);

    [TestMethod]
    public void TheCallsOnARemovedTrackGoToAnotherTrackOfTheLocation()
    {
        var newer = Newer(["G", "Yb", "Snu"], change: n =>
        {
            var yb = n.OperationLocations.Single(l => l.Signature == "Yb");
            yb.Tracks.Remove(yb.Tracks.Single(t => t.Designation == "2"));
        });
        var change = Single(newer, LayoutChangeKind.TrackRemoved);

        var preview = Plan.PreviewChange(change);
        Plan.ApplyChange(change);

        Assert.AreEqual(LayoutChangeHandling.Confirm, change.Handling);
        Assert.IsNotEmpty(preview.MovedCalls);
        Assert.IsTrue(preview.MovedCalls.All(m => m.To.Designation == "1"));
        var yb = At("Yb");
        Assert.AreEqual("1", yb.Tracks.Single().Designation);
        Assert.IsTrue(Plan.Timetable.Trains.SelectMany(t => t.Calls).Where(c => c.OperationLocation.Equals(yb)).All(c => ReferenceEquals(c.Track, yb.Tracks.Single())));
    }

    [TestMethod]
    public void ALocationWithOneTrackKeepsIt()
    {
        var yb = At("Yb");
        yb.Tracks.Remove(yb.Tracks.Single(t => t.Designation == "2"));
        var newer = Newer(["G", "Yb", "Snu"], change: n =>
        {
            var newerYb = n.OperationLocations.Single(l => l.Signature == "Yb");
            newerYb.Tracks.Remove(newerYb.Tracks.Single());
        });
        var change = Single(newer, LayoutChangeKind.TrackRemoved);

        var preview = Plan.ApplyChange(change);

        Assert.IsFalse(preview.IsAllowed);
        Assert.ContainsSingle(yb.Tracks);
    }

    [TestMethod]
    public void ALocationOfAKindWhereTrainsDontStopLosesItsStops()
    {
        foreach (var call in Plan.Timetable.Trains.SelectMany(t => t.Calls).Where(c => c.OperationLocation.Signature == "Yb"))
            (call.IsArrival, call.IsDeparture) = (true, true);
        var newer = Newer(["G", "Yb", "Snu"], change: n =>
        {
            var yb = n.OperationLocations.Single(l => l.Signature == "Yb");
            n.OperationLocations.Remove(yb);
            n.OperationLocations.Add(OperationLocationRules.Conforming(yb, OperationLocationKind.SignalControlled));
            foreach (var stretch in n.TrackStretches)
            {
                if (stretch.Start.Signature == "Yb") stretch.Start = n.OperationLocations.Last();
                if (stretch.End.Signature == "Yb") stretch.End = n.OperationLocations.Last();
            }
        });
        var change = Single(newer, LayoutChangeKind.KindChanged);

        var preview = Plan.ApplyChange(change);

        Assert.HasCount(Plan.Timetable.Trains.Count, preview.TrainsNoLongerStopping);
        var yb = At("Yb");
        Assert.IsInstanceOfType<SignalControlledLocation>(yb);
        Assert.IsTrue(Plan.Timetable.Trains.SelectMany(t => t.Calls).Where(c => c.OperationLocation.Equals(yb)).All(c => !c.IsStop));
    }

    [TestMethod]
    public void ALocationGoneFromBetweenItsNeighboursIsRemovedAndTheirStretchTakesTheNewerDistance()
    {
        var newer = Newer(["G", "Snu"], new Dictionary<(string, string), double> { [("G", "Snu")] = 18 });
        var change = Single(newer, LayoutChangeKind.LocationRemoved);

        Plan.ApplyChange(change);

        Assert.IsFalse(Layout.OperationLocations.Any(l => l.Signature == "Yb"));
        var stretch = Layout.TrackStretches.Single();
        Assert.AreEqual(18, stretch.Distance);
        Assert.AreSame(stretch, Plan.Timetable.TimetableStretches.Single().Stretches.Single());
        Assert.HasCount(2, StartingAt("G").Calls);
    }

    [TestMethod]
    public void ALocationInsertedOnAStretchSplitsItAndTheTrainsPassItOnTime()
    {
        var category = Plan.Timetable.Trains.First().Category!;
        Plan.Timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 5678, Time.FromHourAndMinute(12, 00)));
        var newer = Newer(["G", "X", "Yb", "Snu"], new Dictionary<(string, string), double> { [("G", "X")] = 4, [("X", "Yb")] = 6 });
        var change = Single(newer, LayoutChangeKind.LocationInserted);

        var preview = Plan.PreviewChange(change);
        Plan.ApplyChange(change);

        Assert.HasCount(Plan.Timetable.Trains.Count, preview.PassingTrains, "every train runs the stretch");
        var x = At("X");
        Assert.AreSame(Layout, x.Layout);
        Assert.IsTrue(x.Tracks.All(t => ReferenceEquals(t.Station, x)));
        Assert.AreEqual(4, Layout.TrackStretches.Single(s => s.Start.Signature == "G").Distance);
        Assert.AreEqual(6, Layout.TrackStretches.Single(s => s.Start.Signature == "X").Distance);
        CollectionAssert.AreEqual(new[] { "G", "X", "Yb", "Snu" }, Plan.Timetable.TimetableStretches.Single().Stations.Select(s => s.Signature).ToArray());

        var outward = StartingAt("G").CallsInRunOrder;
        CollectionAssert.AreEqual(new[] { "G", "X", "Yb", "Snu" }, outward.Select(c => c.OperationLocation.Signature).ToArray());
        Assert.AreEqual(Time.FromHourAndMinute(12, 10), outward[1].Departure, "4 of the 10 metres from 12:00 to 12:25");
        Assert.IsFalse(outward[1].IsStop);
        var back = StartingAt("Snu").CallsInRunOrder;
        CollectionAssert.AreEqual(new[] { "Snu", "Yb", "X", "G" }, back.Select(c => c.OperationLocation.Signature).ToArray());
        Assert.AreEqual(Time.FromHourAndMinute(12, 45), back[2].Departure, "6 of the 10 metres from 12:30 to 12:55");
    }

    [TestMethod]
    public void AnEnteredTimeIsSharedByDistanceWhenTheStretchIsSplit()
    {
        var stretch = Layout.TrackStretches.First(s => s.Start.Signature == "G");
        stretch.Time = 20;
        stretch.IsTimeEntered = true;
        var newer = Newer(["G", "X", "Yb", "Snu"], new Dictionary<(string, string), double> { [("G", "X")] = 4, [("X", "Yb")] = 6 });

        Plan.ApplyChange(Single(newer, LayoutChangeKind.LocationInserted));

        Assert.AreEqual(8, stretch.Time);
        Assert.AreEqual(12, Layout.TrackStretches.Single(s => s.Start.Signature == "X").Time);
    }

    [TestMethod]
    public void AnAutomaticChangeIsNotThePlannersToApply()
    {
        var newer = Newer(["G", "Yb", "Snu"], new Dictionary<(string, string), double> { [("G", "Yb")] = 12 });
        var change = Single(newer, LayoutChangeKind.StretchChanged);

        Assert.ThrowsExactly<ArgumentException>(() => Plan.PreviewChange(change));
    }
}
