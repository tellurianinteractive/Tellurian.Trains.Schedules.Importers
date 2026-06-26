namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class TrackStretchDirectionTests
{
    private static Layout LayoutWith(params (int id, string from, string to)[] stretches)
    {
        var layout = new Layout { Name = "Test" };
        Station Get(string sig) =>
            (Station)(layout.OperationLocations.FirstOrDefault(s => s.Signature == sig)
                ?? layout.Add(new Station(sig[0], sig, sig)));
        foreach (var (id, from, to) in stretches)
            layout.Add(new TrackStretch(id, Get(from), Get(to), 10));
        return layout;
    }

    [TestMethod]
    public void ConsistentlyDirectedStretchesHaveNoInconsistencies()
    {
        // A->B B->C D->C C->E E->F E->G : a branching/merging but acyclic (same-direction) graph.
        var layout = LayoutWith((1, "A", "B"), (2, "B", "C"), (3, "D", "C"), (4, "C", "E"), (5, "E", "F"), (6, "E", "G"));

        Assert.AreEqual(0, layout.DirectionInconsistencies().Count);
    }

    [TestMethod]
    public void ReversedPairIsReportedAsATwoStretchCycle()
    {
        var layout = LayoutWith((1, "A", "B"), (2, "B", "A"));

        var inconsistencies = layout.DirectionInconsistencies();

        Assert.AreEqual(1, inconsistencies.Count);
        Assert.AreEqual(2, inconsistencies[0].Count);
    }

    [TestMethod]
    public void DirectedCycleIsReported()
    {
        var layout = LayoutWith((1, "A", "B"), (2, "B", "C"), (3, "C", "A"));

        var inconsistencies = layout.DirectionInconsistencies();

        Assert.AreEqual(1, inconsistencies.Count);
        Assert.AreEqual(3, inconsistencies[0].Count);
    }
}
