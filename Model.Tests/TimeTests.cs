using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Model.Tests;

[TestClass]
public class TimeTests
{
    [TestMethod]
    public void ParsesDouble()
    {
        var actual = "0.5".ParseDays();
        Assert.AreEqual("12:00", actual.ToString());
    }
}
