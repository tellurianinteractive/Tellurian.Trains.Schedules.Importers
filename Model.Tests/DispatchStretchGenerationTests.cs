namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class DispatchStretchGenerationTests
{
    private static Station Manned(int id, string name, string sig) => new(id, name, sig) { IsManned = true };
    private static Station Shadow(int id, string name, string sig) => new(id, name, sig) { IsShadow = true };
    private static Station Unmanned(int id, string name, string sig) => new(id, name, sig);

    [TestMethod]
    public void AdjacentMannedStationsBecomeOneDispatchStretch()
    {
        var layout = new Layout { Name = "Test" };
        var a = (Station)layout.Add(Manned(1, "Alpha", "A"));
        var b = (Station)layout.Add(Manned(2, "Beta", "B"));
        layout.Add(new TrackStretch(1, a, b, 10));

        var dispatch = layout.CreateDispatchStretches();

        Assert.AreEqual(1, dispatch.Count);
        var d = dispatch.Single();
        Assert.AreEqual(a, d.From);
        Assert.AreEqual(b, d.To);
        Assert.AreEqual(1, d.Stretches.Count);
        Assert.IsFalse(d.IntermediateLocations.Any());
    }

    [TestMethod]
    public void UnmannedStationIsTraversedAndRecordedAsIntermediate()
    {
        // A (manned) - U (unmanned) - B (manned): one dispatch stretch A-B spanning both track stretches.
        var layout = new Layout { Name = "Test" };
        var a = (Station)layout.Add(Manned(1, "Alpha", "A"));
        var u = (Station)layout.Add(Unmanned(2, "Unmanned", "U"));
        var b = (Station)layout.Add(Manned(3, "Beta", "B"));
        layout.Add(new TrackStretch(1, a, u, 10));
        layout.Add(new TrackStretch(2, u, b, 10));

        var dispatch = layout.CreateDispatchStretches();

        Assert.AreEqual(1, dispatch.Count);
        var d = dispatch.Single();
        Assert.AreEqual(a, d.From);
        Assert.AreEqual(b, d.To);
        Assert.AreEqual(2, d.Stretches.Count);
        CollectionAssert.AreEqual(new[] { (OperationLocation)u }, d.IntermediateLocations.ToArray());
        Assert.AreEqual(1, d.Id, "Id is the first comprising track stretch's id.");
        Assert.IsFalse(dispatch.Any(x => x.From.Equals(u) || x.To.Equals(u)), "An unmanned station is never a dispatch endpoint.");
    }

    [TestMethod]
    public void ShadowStationIsADispatchEndpoint()
    {
        var layout = new Layout { Name = "Test" };
        var a = (Station)layout.Add(Manned(1, "Alpha", "A"));
        var s = (Station)layout.Add(Shadow(2, "Shadow", "S"));
        layout.Add(new TrackStretch(1, a, s, 10));

        var dispatch = layout.CreateDispatchStretches();

        Assert.AreEqual(1, dispatch.Count);
        Assert.AreEqual(s, dispatch.Single().To);
    }
}
