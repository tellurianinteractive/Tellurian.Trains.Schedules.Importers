using Tellurian.Trains.Schedules.Importers.Model;
using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Tests;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void ParsesTrainNumber()
    {
        Assert.AreEqual(1234, "1234".NumberOrZero);
        Assert.AreEqual(5814, "GT CL5814".NumberOrZero);
        Assert.AreEqual(8318, "GT HCR 8318".NumberOrZero);
    }

    [TestMethod]
    public void ParsesTrainCategory()
    {
        Assert.AreEqual("GT", "054738.GT CN54738".TrainCategoryPrefix);
        Assert.AreEqual("Snt", "000100.Snt100".TrainCategoryPrefix);
    }

    [TestMethod]
    public void IsTime()
    {
        Assert.IsTrue("12:34".IsTime(), "12:34");
        Assert.IsTrue("1899-12-31 12:34:00".IsTime(), "1899-12-31 12:34:00");
        Assert.IsTrue("0,22222222222646".IsTime(), "0,22222222222646");
        Assert.IsFalse("12.60".IsTime(), "12:60");
        Assert.IsFalse("X".IsTime(), "X");
        Assert.IsTrue("0".IsTime(), "0");
        Assert.IsFalse("0".IsTime(true), "0");
    }
}

