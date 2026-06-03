using Tellurian.Trains.Schedules.Importers.Xpln.Extensions;
using Tellurian.Trains.Schedules.Model;

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
    public void ParsesLocoNumber()
    {
        Assert.AreEqual(2, "DB_GLok2".LocoNumber, "DB_GLok2");
        Assert.AreEqual(0, "DB_GLok".LocoNumber, "DB_GLok (no number)");
        Assert.AreEqual(2000, "X2000".LocoNumber, "X2000");
        Assert.AreEqual(5814, "GT CL5814".LocoNumber, "GT CL5814");
        Assert.AreEqual(218, "DB_218".LocoNumber, "DB_218");
    }

    [TestMethod]
    public void ParsesLocoOperatingCompany()
    {
        Assert.AreEqual("DB", "DB_GLok2".LocoOperatingCompanySignature, "DB_GLok2");
        Assert.AreEqual("SJ", "SJ-Rc6".LocoOperatingCompanySignature, "SJ-Rc6");
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

