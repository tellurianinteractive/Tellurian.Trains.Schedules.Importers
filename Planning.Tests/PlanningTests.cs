namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PlanningTests
{
    // Simple layout tests

    [TestMethod]
    public void SimpleLayoutHas5Stations()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        Assert.AreEqual(5, layout.OperationLocations.Count);
    }

    [TestMethod]
    public void SimpleLayoutHas4TrackStretches()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        Assert.AreEqual(4, layout.TrackStretches.Count);
    }

    [TestMethod]
    public void SimpleLayoutHas2TimetableStretches()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        Assert.AreEqual(2, layout.TimetableStretches.Count);
    }

    // Full layout tests

    [TestMethod]
    public void LayoutHas28Locations()
    {
        var layout = TestLayoutFactory.CreateLayout();
        Assert.AreEqual(28, layout.OperationLocations.Count);
    }

    [TestMethod]
    public void LayoutHas16Stations()
    {
        var layout = TestLayoutFactory.CreateLayout();
        Assert.AreEqual(16, layout.OperationLocations.OfType<Station>().Count());
    }

    [TestMethod]
    public void LayoutHas6SignalControlledLocations()
    {
        var layout = TestLayoutFactory.CreateLayout();
        Assert.AreEqual(5, layout.OperationLocations.OfType<SignalControlledLocation>().Count());
    }

    [TestMethod]
    public void LayoutHas7OtherLocations()
    {
        var layout = TestLayoutFactory.CreateLayout();
        Assert.AreEqual(7, layout.OperationLocations.OfType<OtherLocation>().Count());
    }

    [TestMethod]
    public void LayoutHas27TrackStretches()
    {
        var layout = TestLayoutFactory.CreateLayout();
        Assert.AreEqual(27, layout.TrackStretches.Count);
    }

    [TestMethod]
    public void LayoutHas5TimetableStretches()
    {
        var layout = TestLayoutFactory.CreateLayout();
        Assert.AreEqual(5, layout.TimetableStretches.Count);
    }
}
