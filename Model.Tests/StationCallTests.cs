namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class StationCallTests
{
    [TestMethod]
    public void EqualsWorks()
    {
        TestDataFactory.Init();
        var station = TestDataFactory.CreateStation1();
        var train = TestDataFactory.CreateTrain1();
        var one = new StationCall(1, station.Tracks.First(), Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 00)) { Train = train };
        var another = new StationCall(2, station.Tracks.First(), Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 00)) { Train = train };
        Assert.AreEqual(one, another);
    }
}
