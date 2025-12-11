namespace Tellurian.Trains.Timetables.Importers.Model.Tests;

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
}
