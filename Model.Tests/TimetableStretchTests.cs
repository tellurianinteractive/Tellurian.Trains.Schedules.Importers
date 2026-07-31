using Tellurian.Utilities.Web;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class TimetableStretchTests
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private TimetableStretch Target { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        Target = new TimetableStretch(10, "10", "Ten");
    }

    [TestMethod]
    public void NullNumberThrows()
    {
        Assert.Throws<ArgumentNullException>(() => new TimetableStretch(1, null));
    }

    [TestMethod]
    public void PropertiesAreSet()
    {
        Assert.AreEqual("10", Target.Number);
        Assert.AreEqual("Ten", Target.Description);
    }

    [TestMethod]
    public void DefaultColorIsFirstPaletteEntry()
    {
        Assert.AreEqual(TimetableStretch.Palette[0], Target.Color);
    }

    [TestMethod]
    public void EveryPaletteColorIsDark()
    {
        // Lines are drawn thin against a white background, so every choice must stay legible.
        foreach (var color in TimetableStretch.Palette)
            Assert.IsTrue(color.IsDark, $"{color} is not dark.");
    }

    [TestMethod]
    public void EqualsWithSameNumber()
    {
        var other = new TimetableStretch(10, "10");
        Assert.AreEqual(Target, other);
    }

    [TestMethod]
    public void GetStationOnRevisitedStationReturnsFirstAndDoesNotThrow()
    {
        // A reversing line A->B->A visits A twice, so A occurs more than once in Stations.
        // GetStation/DistanceToStation must not throw (regression: previously used SingleOrDefault).
        var layout = new Layout { Name = "Test" };
        var a = new Station(101, "Alpha", "A");
        a.Add(new StationTrack(1, "1"));
        var b = new Station(102, "Beta", "B");
        b.Add(new StationTrack(1, "1"));
        layout.Add(a);
        layout.Add(b);
        layout.Add(new TrackStretch(1, a, b, 10));
        layout.Add(new TrackStretch(2, b, a, 10));
        var stretch = new TimetableStretch(20, "20");
        foreach (var ts in layout.TrackStretches) stretch.AddLast(ts);

        var found = stretch.GetStation(a);

        Assert.IsFalse(found.IsNone, "Station A should be found on the stretch.");
        Assert.AreEqual(0.0, stretch.DistanceToStation(a), "Distance to the start station.");
        Assert.AreEqual(10.0, stretch.DistanceToStation(b), "Distance to B.");
    }

    [TestMethod]
    public void TerminalStartHasZeroKilometreOffset()
    {
        var (layout, a, b, c, _, _) = BuildBranchingLayout();
        var main = layout.TimetableStretches.Single(s => s.Number == "main");

        Assert.AreEqual(0.0, main.StartKilometer, "A terminal start counts from zero.");
        Assert.AreEqual(0.0, main.DisplayedDistanceToStation(a));
        Assert.AreEqual(10.0, main.DisplayedDistanceToStation(b));
        Assert.AreEqual(25.0, main.DisplayedDistanceToStation(c));
    }

    [TestMethod]
    public void DivergingBranchCountsFromItsJunctionKilometre()
    {
        var (layout, _, b, _, d, _) = BuildBranchingLayout();
        var branch = layout.TimetableStretches.Single(s => s.Number == "branch");

        // The branch starts at B, which lies 10 km along the main line, so its km continue from there.
        Assert.AreEqual(10.0, branch.StartKilometer, "Offset is the junction's km on the main line.");
        Assert.AreEqual(10.0, branch.DisplayedDistanceToStation(b), "Branch start = junction km.");
        Assert.AreEqual(17.0, branch.DisplayedDistanceToStation(d), "Junction km plus B->D distance.");
    }

    [TestMethod]
    public void ChainedDivergenceAccumulatesTheOffset()
    {
        var (layout, _, _, _, d, e) = BuildBranchingLayout();
        var subBranch = layout.TimetableStretches.Single(s => s.Number == "subbranch");

        // subbranch starts at D (branch km 17), which itself starts at B (main km 10): the offsets accumulate.
        Assert.AreEqual(17.0, subBranch.StartKilometer);
        Assert.AreEqual(20.0, subBranch.DisplayedDistanceToStation(e), "17 km junction plus D->E distance.");
    }

    [TestMethod]
    public void DistanceFactorScalesDisplayedButNotRawDistance()
    {
        var (layout, a, b, c, _, _) = BuildBranchingLayout();
        var main = layout.TimetableStretches.Single(s => s.Number == "main");
        layout.Settings.TimeAndSpeed.DistanceFactor = 2.0;

        Assert.AreEqual(25.0, main.DistanceToStation(c), "Raw distance stays in metres, unscaled.");
        Assert.AreEqual(50.0, main.DisplayedDistanceToStation(c), "Displayed km is raw metres times the factor.");
        Assert.AreEqual(20.0, main.DisplayedDistanceToStation(b));
    }

    [TestMethod]
    public void DisplayedKilometreIsRoundedToAWholeNumber()
    {
        var (layout, a, b, c, d, e) = BuildBranchingLayout();
        var main = layout.TimetableStretches.Single(s => s.Number == "main");
        var branch = layout.TimetableStretches.Single(s => s.Number == "branch");
        var subBranch = layout.TimetableStretches.Single(s => s.Number == "subbranch");
        layout.Settings.TimeAndSpeed.DistanceFactor = 0.75;

        Assert.AreEqual(0.0, main.DisplayedDistanceToStation(a));
        Assert.AreEqual(8.0, main.DisplayedDistanceToStation(b), "7.5 km rounds upwards.");
        Assert.AreEqual(19.0, main.DisplayedDistanceToStation(c), "18.75 km rounds to 19.");

        // The junction must read the same on both lines, so the branch's offset is added before scaling.
        Assert.AreEqual(8.0, branch.StartKilometer, "Same rounded km as B on the main line.");
        Assert.AreEqual(8.0, branch.DisplayedDistanceToStation(b));
        Assert.AreEqual(13.0, branch.DisplayedDistanceToStation(d), "17 metres scaled to 12.75 rounds to 13.");
        Assert.AreEqual(13.0, subBranch.StartKilometer, "Same rounded km as D on the branch.");
        Assert.AreEqual(15.0, subBranch.DisplayedDistanceToStation(e), "20 metres scaled to 15.");
    }

    [TestMethod]
    public void ReverseTurnsEveryTrackStretchAroundAndReversesTheirOrder()
    {
        var (layout, a, b, c, _, _) = BuildBranchingLayout();
        var main = layout.TimetableStretches.Single(s => s.Number == "main");

        Assert.IsTrue(main.CanReverse, "No other timetable stretch uses A-B or B-C.");
        main.Reverse();

        Assert.AreEqual(c, main.Starts, "The stretch now starts where it ended.");
        Assert.AreEqual(a, main.Ends, "The stretch now ends where it started.");
        CollectionAssert.AreEqual(new[] { c, b, a }, main.Stations.ToArray(), "Stations in reversed order.");
        Assert.IsTrue(main.IsContiguous, "The reversed route is still contiguous.");
        Assert.AreEqual(25.0, main.DistanceToStation(a), "Distance is now measured from C.");
    }

    [TestMethod]
    public void ReverseIsRepeatableAndRestoresTheOriginalRoute()
    {
        var (layout, a, b, c, _, _) = BuildBranchingLayout();
        var main = layout.TimetableStretches.Single(s => s.Number == "main");

        main.Reverse();
        main.Reverse();

        CollectionAssert.AreEqual(new[] { a, b, c }, main.Stations.ToArray());
    }

    [TestMethod]
    public void StretchSharingATrackStretchWithAnotherCannotBeReversed()
    {
        var (layout, a, b, _, _, _) = BuildBranchingLayout();
        var main = layout.TimetableStretches.Single(s => s.Number == "main");
        // A second line reusing A-B, the first track stretch of the main line.
        var shared = new TimetableStretch(4, "shared");
        shared.AddLast(layout.TrackStretches.Single(s => s.Start.Equals(a) && s.End.Equals(b)));
        layout.TimetableStretches.Add(shared);

        Assert.IsFalse(main.CanReverse, "A-B is used by another timetable stretch.");
        Assert.AreEqual(1, main.SharedStretches().Count, "Only A-B is shared.");

        main.Reverse();

        Assert.AreEqual(a, main.Starts, "Reversing a shared stretch does nothing.");
    }

    // A main line A-B-C (terminal A), a branch B-D diverging at B, and a sub-branch D-E diverging at D.
    private static (Layout Layout, Station A, Station B, Station C, Station D, Station E) BuildBranchingLayout()
    {
        var layout = new Layout { Name = "Branching" };
        Station Add(int id, string name, string signature)
        {
            var s = new Station(id, name, signature);
            s.Add(new StationTrack(1, "1"));
            layout.Add(s);
            return s;
        }
        var a = Add(201, "Alpha", "A");
        var b = Add(202, "Beta", "B");
        var c = Add(203, "Gamma", "C");
        var d = Add(204, "Delta", "D");
        var e = Add(205, "Epsilon", "E");

        var ab = new TrackStretch(1, a, b, 10);
        var bc = new TrackStretch(2, b, c, 15);
        var bd = new TrackStretch(3, b, d, 7);
        var de = new TrackStretch(4, d, e, 3);
        foreach (var ts in new[] { ab, bc, bd, de }) layout.Add(ts);

        var main = new TimetableStretch(1, "main");
        main.AddLast(ab);
        main.AddLast(bc);
        var branch = new TimetableStretch(2, "branch");
        branch.AddLast(bd);
        var subBranch = new TimetableStretch(3, "subbranch");
        subBranch.AddLast(de);
        foreach (var s in new[] { main, branch, subBranch }) layout.TimetableStretches.Add(s);

        return (layout, a, b, c, d, e);
    }
}
