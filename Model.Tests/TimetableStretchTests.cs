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
