using Tellurian.Trains.Timetables.Importers.Model;

namespace Tellurian.Trains.Timetables.Importers.Model.Tests;

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
