using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Locations dispatched from afar: Alpha (manned) works the junction Bj, where the lines from Alpha and
/// Delta join towards Charlie (both manned).
/// </summary>
[TestClass]
public class RemoteDispatchTests
{
    private sealed record Junction(Timetable Timetable, Station A, OperationLocation B, Station C, Station D);

    private static Junction NewJunction(OperationLocation? junction = null, bool controlled = true, bool withBranch = true)
    {
        var layout = new Layout { Name = "Test" };
        var a = (Station)layout.Add(new Station(1, "Alpha", "A") { IsManned = true });
        var b = layout.Add(junction ?? new SignalControlledLocation(2, "Bravo junction", "Bj"));
        var c = (Station)layout.Add(new Station(3, "Charlie", "C") { IsManned = true });
        var d = (Station)layout.Add(new Station(4, "Delta", "D") { IsManned = true });
        if (controlled) b.ControlledBy = a;
        layout.Add(new TrackStretch(1, a, b, 10));
        layout.Add(new TrackStretch(2, b, c, 10));
        if (withBranch) layout.Add(new TrackStretch(3, d, b, 10));
        var timetable = new Timetable("Test", layout);
        timetable.DispatchStretches = layout.CreateDispatchStretches();
        timetable.EnsurePlanningOf(a).PhoneNumber = 11;
        timetable.EnsurePlanningOf(c).PhoneNumber = 13;
        timetable.EnsurePlanningOf(d).PhoneNumber = 14;
        return new Junction(timetable, a, b, c, d);
    }

    private static string[] Stretches(Timetable timetable) =>
        [.. timetable.DispatchStretches.Select(d => d.ToString()).Order()];

    [TestMethod]
    public void AControlledJunctionIsADispatchEndpoint()
    {
        var j = NewJunction();

        Assert.IsTrue(j.B.IsDispatchEndpoint);
        Assert.AreEqual(j.A, j.B.Dispatcher);
        Assert.AreSequenceEqual(new[] { "Alpha-Bravo junction", "Bravo junction-Charlie", "Delta-Bravo junction" }, Stretches(j.Timetable));
    }

    [TestMethod]
    public void AControlledBlockPostStaysPartOfTheLine()
    {
        var j = NewJunction(withBranch: false);

        Assert.IsTrue(j.B.IsBlockPost);
        Assert.IsFalse(j.B.IsDispatchEndpoint, "A block post has no route of its own to clear.");
        Assert.IsTrue(j.B.IsControlPoint);
        Assert.AreSequenceEqual(new[] { "Alpha-Charlie" }, Stretches(j.Timetable));
    }

    [TestMethod]
    public void AControlledCrossingPlaceIsADispatchEndpoint()
    {
        var loop = new SignalControlledLocation(2, "Bravo loop", "Bj") { TrainsCanCross = true };
        var j = NewJunction(loop, withBranch: false);

        Assert.IsFalse(j.B.IsBlockPost);
        Assert.IsTrue(j.B.IsDispatchEndpoint);
        Assert.AreEqual(j.A, j.B.Dispatcher);
        Assert.AreSequenceEqual(new[] { "Alpha-Bravo loop", "Bravo loop-Charlie" }, Stretches(j.Timetable));
    }

    [TestMethod]
    public void TwoTracksDoNotMakeACrossingPlace()
    {
        // A junction has two tracks to know which way a train goes, whether or not trains can cross, so
        // the tracks say nothing: without the mark, a location that is no junction is a block post.
        var post = new SignalControlledLocation(2, "Bravo", "Bj");
        post.Add(new StationTrack(21, "1"));
        post.Add(new StationTrack(22, "2"));
        var j = NewJunction(post, withBranch: false);

        Assert.IsTrue(j.B.IsBlockPost);
        Assert.IsFalse(j.B.IsDispatchEndpoint);
        Assert.IsFalse(j.B.AllowsMeets);
    }

    [TestMethod]
    public void AJunctionWhereTrainsCannotCrossIsStillADispatchEndpoint()
    {
        var j = NewJunction();

        Assert.IsFalse(j.B.IsBlockPost);
        Assert.IsTrue(j.B.IsDispatchEndpoint);
    }

    [TestMethod]
    public void ACrossingPlaceMissingItsControllerStaysAMeetingPlaceOnTheLine()
    {
        var loop = new SignalControlledLocation(2, "Bravo loop", "Bj") { TrainsCanCross = true };
        var j = NewJunction(loop, controlled: false, withBranch: false);

        Assert.IsFalse(j.B.IsDispatchEndpoint);
        Assert.IsTrue(j.B.IsControlPoint);
        Assert.IsTrue(j.B.AllowsMeets);
    }

    [TestMethod]
    public void AJunctionNobodyControlsIsNoDispatchEndpoint()
    {
        var j = NewJunction(controlled: false);

        Assert.IsFalse(j.B.IsDispatchEndpoint);
        Assert.IsNull(j.B.Dispatcher);
    }

    [TestMethod]
    public void AControlledUnmannedStationIsADispatchEndpoint()
    {
        var j = NewJunction(new Station(2, "Bravo", "Bj"), withBranch: false);

        Assert.IsTrue(j.B.IsDispatchEndpoint);
        Assert.AreSequenceEqual(new[] { "Alpha-Bravo", "Bravo-Charlie" }, Stretches(j.Timetable));
    }

    [TestMethod]
    public void OnlyAStationWithADispatcherCanDispatchRemotely()
    {
        var j = NewJunction(controlled: false);
        j.B.ControlledBy = new Station(9, "Unmanned", "U");

        Assert.IsFalse(j.B.IsRemotelyDispatched);
    }

    [TestMethod]
    public void TheControllerRingsTheDispatchersBeyondTheLocationsItWorks()
    {
        var j = NewJunction();

        Assert.AreSequenceEqual(new[] { j.C, j.D }, j.Timetable.DispatchNeighboursOf(j.A).ToArray());
        Assert.AreSequenceEqual(new[] { j.A }, j.Timetable.DispatchNeighboursOf(j.C).ToArray(), "Charlie rings Alpha about the junction.");
        Assert.AreSequenceEqual(new[] { j.A }, j.Timetable.DispatchNeighboursOf(j.D).ToArray());
    }

    [TestMethod]
    public void AJunctionEndpointAndItsControllerSurviveSaving()
    {
        var j = NewJunction();
        var plan = Plan.Create("Test", j.Timetable);

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, PlanJson.CreateOptions()), PlanJson.CreateOptions())!;
        var junction = restored.Layout.OperationLocations.Single(l => l.Signature == "Bj");

        Assert.IsInstanceOfType<SignalControlledLocation>(junction);
        Assert.AreEqual("A", junction.ControlledBy?.Signature);
        Assert.AreSequenceEqual(Stretches(j.Timetable), Stretches(restored.Timetable));
        Assert.IsTrue(restored.Timetable.DispatchStretches.Any(d => ReferenceEquals(d.From, junction) || ReferenceEquals(d.To, junction)),
            "The endpoints are the layout's own locations, not copies.");
    }

    [TestMethod]
    public void TheControllerDispatchesItselfAndTheLocationsItWorks()
    {
        var j = NewJunction();

        Assert.AreSequenceEqual(new[] { j.A, j.B }, j.Timetable.Layout.LocationsDispatchedBy(j.A).ToArray());
        Assert.AreSequenceEqual(new[] { j.A, j.C, j.D }, j.Timetable.Layout.Dispatchers.ToArray());
    }
}
