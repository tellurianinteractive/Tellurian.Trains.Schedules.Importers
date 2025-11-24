using TimetablePlanning.Importers.Xpln.Extensions;

namespace TimetablePlanning.Importers.Xpln.Tests;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void ParsesTrainNumber()
    {
        Assert.AreEqual("1234", "1234".TrainNumber());
        Assert.AreEqual("5814", "GT CL5814".TrainNumber());
        Assert.AreEqual("8318", "GT HCR 8318".TrainNumber());
    }

    [TestMethod]
    public void ParsesTrainCategory()
    {
        Assert.AreEqual("GT", "054738.GT CN54738".TrainCategory());
        Assert.AreEqual("Snt", "000100.Snt100".TrainCategory());
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

