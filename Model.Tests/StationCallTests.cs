using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Model.Tests;

[TestClass]
public class StationCallTests
{
    [TestMethod]
    public void EqualsWorks()
    {
        TestDataFactory.Init();
        var station = TestDataFactory.CreateStation1();
        var train = TestDataFactory.CreateTrain1();
        var one = new StationCall(station.Tracks.First(), Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 00)) { Train = train };
        var another = new StationCall(station.Tracks.First(), Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 00)) { Train = train };
        Assert.AreEqual(one, another);
    }
}
