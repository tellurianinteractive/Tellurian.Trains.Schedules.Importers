using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// Comparing the layout a timetable is planned on with a newer version of it, and taking in what changes only
/// distances and times (<see cref="LayoutComparison"/>, <see cref="LayoutUpdate"/>).
/// </summary>
[TestClass]
public class LayoutComparisonTests
{
    /// <summary>A line of stations in order, a stretch of <paramref name="distance"/> metres between each pair.</summary>
    private static Layout Line(double distance, params string[] signatures)
    {
        var layout = new Layout { SourceId = Guid.Parse("10000000-0000-0000-0000-000000000001") };
        foreach (var signature in signatures)
        {
            // An id belongs to one location for good, in every version of the hall plan, as it does in a real one.
            var id = signature[0] - 'A' + 1;
            var station = new Station(id, signature, signature) { Layout = layout };
            station.Tracks.Add(new StationTrack(id * 10 + 1, "1") { Station = station, Length = 2.0 });
            layout.OperationLocations.Add(station);
        }
        var locations = layout.OperationLocations.ToList();
        for (var i = 0; i < locations.Count - 1; i++)
            layout.TrackStretches.Add(new TrackStretch(i + 1, locations[i], locations[i + 1], distance) { Speed = 100, Layout = layout });
        return layout;
    }

    private static OperationLocation At(Layout layout, string signature) => layout.OperationLocations.Single(l => l.Signature == signature);

    private static TrackStretch Between(Layout layout, string a, string b) =>
        layout.TrackStretches.Single(s => (s.Start.Signature, s.End.Signature) == (a, b) || (s.Start.Signature, s.End.Signature) == (b, a));

    [TestMethod]
    public void TheSameLayoutHasNoDifferences()
    {
        var comparison = LayoutComparison.Compare(Line(10, "A", "B", "C"), Line(10, "A", "B", "C"));

        Assert.IsTrue(comparison.IsUnchanged);
        Assert.IsTrue(comparison.IsOrderIntact);
    }

    [TestMethod]
    public void ALongerStretchIsTakenWithItsRunningTime()
    {
        var current = Line(10, "A", "B", "C");
        var source = Line(10, "A", "B", "C");
        Between(source, "A", "B").Distance = 14.4;

        var comparison = LayoutComparison.Compare(current, source);
        var change = comparison.Changes.Single();

        Assert.AreEqual(LayoutChangeKind.StretchChanged, change.Kind);
        Assert.AreEqual(LayoutChangeHandling.Automatic, change.Handling);
        CollectionAssert.AreEqual(new[] { nameof(TrackStretch.Distance) }, change.Properties.ToArray());

        LayoutUpdate.ApplyAutomatic(comparison);
        var stretch = Between(current, "A", "B");
        Assert.AreEqual(14.4, stretch.Distance, "the running time is the planner's to work out again");
    }

    [TestMethod]
    public void AnEnteredDistanceOrTimeIsKept()
    {
        var current = Line(10, "A", "B", "C");
        Between(current, "A", "B").IsDistanceEntered = true;
        var bc = Between(current, "B", "C");
        bc.Time = 7;
        bc.IsTimeEntered = true;
        var source = Line(20, "A", "B", "C");

        LayoutUpdate.ApplyAutomatic(LayoutComparison.Compare(current, source));

        Assert.AreEqual(10, Between(current, "A", "B").Distance, "the planner's distance stays");
        Assert.AreEqual(20, bc.Distance);
        Assert.AreEqual(7, bc.Time, "the planner's time stays");
    }

    [TestMethod]
    public void WhatALocationIsIsTakenAndWhatIsDecidedForItStays()
    {
        var current = Line(10, "A", "B");
        var a = (Station)At(current, "A");
        a.IsManned = true;
        var source = Line(10, "A", "B");
        var sourceA = (Station)At(source, "A");
        sourceA.Name = "Aby";
        sourceA.HasTurntable = true;

        var comparison = LayoutComparison.Compare(current, source);
        LayoutUpdate.ApplyAutomatic(comparison);

        CollectionAssert.AreEquivalent(new[] { nameof(OperationLocation.Name), nameof(Station.HasTurntable) }, comparison.Changes.Single().Properties.ToArray());
        Assert.AreEqual("Aby", a.Name);
        Assert.IsTrue(a.HasTurntable);
        Assert.IsTrue(a.IsManned);
    }

    [TestMethod]
    public void TracksAddedAndChangedAreTakenAndARemovedTrackNeedsTheTimetable()
    {
        var current = Line(10, "A", "B");
        var a = At(current, "A");
        a.Tracks.Add(new StationTrack(99, "9") { Station = a });
        var source = Line(10, "A", "B");
        var sourceA = At(source, "A");
        sourceA.Tracks.Single().Length = 3.5;
        sourceA.Tracks.Add(new StationTrack(12, "2") { Station = sourceA });

        var comparison = LayoutComparison.Compare(current, source);
        LayoutUpdate.ApplyAutomatic(comparison);

        Assert.AreEqual(LayoutChangeHandling.Confirm, comparison.Changes.Single(c => c.Kind == LayoutChangeKind.TrackRemoved).Handling);
        Assert.AreEqual("9", comparison.Changes.Single(c => c.Kind == LayoutChangeKind.TrackRemoved).Track!.Designation);
        Assert.AreEqual(3.5, a.Tracks.First().Length);
        Assert.IsTrue(a.Tracks.Any(t => t.Designation == "2" && ReferenceEquals(t.Station, a)), "the new track is the location's");
        Assert.IsTrue(a.Tracks.Any(t => t.Designation == "9"), "a removed track stays until the planner agrees");
    }

    [TestMethod]
    public void ALocationOnAStretchIsAnInsertion()
    {
        var current = Line(10, "A", "B");
        var source = Line(5, "A", "X", "B");

        var change = LayoutComparison.Compare(current, source).Changes.Single(c => c.Handling != LayoutChangeHandling.Automatic);

        Assert.AreEqual(LayoutChangeKind.LocationInserted, change.Kind);
        Assert.AreEqual(LayoutChangeHandling.Confirm, change.Handling);
        Assert.AreEqual("X", change.SourceLocation!.Signature);
        Assert.AreSame(Between(current, "A", "B"), change.Stretch);
        Assert.HasCount(2, change.Stretches);
    }

    [TestMethod]
    public void ALocationGoneFromBetweenTwoIsARemoval()
    {
        var current = Line(5, "A", "X", "B");
        var source = Line(10, "A", "B");

        var comparison = LayoutComparison.Compare(current, source);
        var change = comparison.Changes.Single();

        Assert.AreEqual(LayoutChangeKind.LocationRemoved, change.Kind);
        Assert.AreEqual(LayoutChangeHandling.Confirm, change.Handling);
        Assert.AreEqual("X", change.Location!.Signature);
        Assert.AreSame(Between(source, "A", "B"), change.SourceStretch);
        Assert.IsTrue(comparison.IsOrderIntact);
    }

    [TestMethod]
    public void AnotherOrderIsLeftToThePlanner()
    {
        var comparison = LayoutComparison.Compare(Line(10, "A", "B", "C"), Line(10, "A", "C", "B"));

        Assert.IsFalse(comparison.IsOrderIntact);
        Assert.IsTrue(comparison.Changes.Any(c => c.Kind == LayoutChangeKind.StretchAdded && c.Handling == LayoutChangeHandling.Manual));
        Assert.IsTrue(comparison.Changes.Any(c => c.Kind == LayoutChangeKind.StretchRemoved && c.Handling == LayoutChangeHandling.Manual));
    }

    [TestMethod]
    public void ALocationIsFoundByItsExternalReferenceAlsoWithAnotherSignature()
    {
        var current = Line(10, "A", "B");
        var source = Line(10, "Q", "B");
        var reference = JsonDocument.Parse("""{ "stationId": 1234 }""").RootElement;
        At(current, "A").ExternalReferences = new() { ["moduleRegistry"] = reference };
        At(source, "Q").ExternalReferences = new() { ["moduleRegistry"] = reference };

        var comparison = LayoutComparison.Compare(current, source);

        Assert.IsTrue(comparison.IsOrderIntact);
        Assert.AreEqual(LayoutChangeKind.LocationChanged, comparison.Changes.Single().Kind);
        Assert.IsTrue(comparison.Changes.Single().Properties.Contains(nameof(OperationLocation.Signature)));
    }

    [TestMethod]
    public void AStretchTheOtherWayRoundGetsItsSidesTurned()
    {
        var current = Line(10, "A", "B");
        var source = Line(12, "A", "B");
        var stretch = Between(source, "A", "B");
        stretch.Reverse();
        stretch.StartSide = ExitSide.Left;
        stretch.EndSide = ExitSide.Right;

        LayoutUpdate.ApplyAutomatic(LayoutComparison.Compare(current, source));

        var updated = Between(current, "A", "B");
        Assert.AreEqual("A", updated.Start.Signature);
        Assert.AreEqual(ExitSide.Right, updated.StartSide, "A was the end of the newer stretch");
        Assert.AreEqual(ExitSide.Left, updated.EndSide);
    }

    [TestMethod]
    public void TheModulesOfTheHallPlanReferToTheCurrentLocations()
    {
        var current = Line(10, "A", "B");
        var source = Line(10, "A", "B");
        source.SourceRevision = 3;
        At(source, "A").Id = 77; // the newer layout's ids need not be the current one's
        source.TrackStretches.Single().StartId = 77;
        source.Modules.Add(new LayoutModule { Id = 1, Title = "A", OperationLocationId = 77 });
        source.Modules.Add(new LayoutModule { Id = 2, Title = "Curve" });
        source.StretchLines.Add(new StretchLine { Id = 1, Legs = [new(77, At(source, "B").Id)] });

        LayoutUpdate.ApplyAutomatic(LayoutComparison.Compare(current, source));

        Assert.AreEqual(3, current.SourceRevision);
        Assert.AreEqual(At(current, "A").Id, current.Modules.First().OperationLocationId);
        Assert.AreEqual("Curve", current.Modules.Last().Title);
        Assert.IsNull(current.Modules.Last().OperationLocationId);
        Assert.IsTrue(current.StretchLines.Single().Legs.Single().Joins(At(current, "A").Id, At(current, "B").Id), "the line refers to the current locations");
    }
}
