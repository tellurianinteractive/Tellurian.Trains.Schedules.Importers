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
}
