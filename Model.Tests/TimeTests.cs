namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class TimeTests
{
    [TestMethod]
    public void ParsesDouble()
    {
        var actual = "0.5".ParseDays();
        Assert.AreEqual("12:00", actual.ToString());
    }

    // Time must be directly comparable so LINQ OrderBy/Max use the typed comparer rather than the
    // reflection-based ObjectComparer (which throws "does not implement IComparable").
    [TestMethod]
    public void OrdersAndTakesMaxWithTheDefaultComparer()
    {
        Time[] times = [Time.FromHourAndMinute(9, 0), Time.FromHourAndMinute(7, 30), Time.FromHourAndMinute(8, 15)];

        var ordered = times.OrderBy(t => t).ToArray();
        var max = times.Max();

        Assert.AreEqual(Time.FromHourAndMinute(7, 30), ordered[0]);
        Assert.AreEqual(Time.FromHourAndMinute(9, 0), max);
    }
}
