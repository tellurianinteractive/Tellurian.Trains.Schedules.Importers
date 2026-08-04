namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class TrackStretchTests
{
    [TestMethod]
    public void ReturnsPassings()
    {
        TestDataFactory.Init();
        var target = TestDataFactory.CreateTimetable();
        foreach (var stretch in target.Layout.TrackStretches)
        {
            Assert.AreEqual(2, stretch.Passings.Count());
        }
    }

    private static (Layout layout, Station a, Station b, Station c) LayoutWithStretchFromAToB()
    {
        var layout = new Layout { Name = "Test" };
        var a = (Station)layout.Add(new Station(1, "Alpha", "A"));
        var b = (Station)layout.Add(new Station(2, "Beta", "B"));
        var c = (Station)layout.Add(new Station(3, "Gamma", "C"));
        layout.Add(new TrackStretch(1, a, b, 10));
        return (layout, a, b, c);
    }

    [TestMethod]
    public void StretchBetweenFindsStretchDefinedInSameDirection()
    {
        var (layout, a, b, _) = LayoutWithStretchFromAToB();

        Assert.IsNotNull(layout.StretchBetween(a, b));
    }

    [TestMethod]
    public void StretchBetweenFindsStretchDefinedInOppositeDirection()
    {
        var (layout, a, b, _) = LayoutWithStretchFromAToB();

        Assert.IsNotNull(layout.StretchBetween(b, a), "A stretch is bidirectional, so B–A is the same connection as A–B.");
    }

    [TestMethod]
    public void StretchBetweenFindsNothingForUnjoinedLocations()
    {
        var (layout, a, _, c) = LayoutWithStretchFromAToB();

        Assert.IsNull(layout.StretchBetween(a, c));
    }

    [TestMethod]
    public void StretchBetweenDisregardsTheStretchBeingEdited()
    {
        var (layout, a, b, _) = LayoutWithStretchFromAToB();
        var editing = layout.TrackStretches.Single();

        Assert.IsNull(layout.StretchBetween(a, b, excluding: editing), "A stretch is not a duplicate of itself.");
        Assert.IsNull(layout.StretchBetween(b, a, excluding: editing), "Nor when its endpoints are swapped.");
    }
}
