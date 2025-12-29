namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class SessionsTests
{
    [TestMethod]
    public void IsAllSessions()
    {
        var target = Sessions.All;
        var actual = target.Numbers;
        Assert.HasCount(14, actual);
        Assert.AreEqual(1, actual[0]);
        Assert.AreEqual(14, actual[13]);
    }

    [TestMethod]
    public void IsOddSessions()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var actual = target.Numbers;
        Assert.HasCount(7, actual);
        Assert.AreEqual(1, actual[0]);
        Assert.AreEqual(13, actual[^1]);
    }

    [TestMethod]
    public void IsEvenSessions()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var actual = target.Numbers;
        Assert.HasCount(7, actual);
        Assert.AreEqual(2, actual[0]);
        Assert.AreEqual(14, actual[^1]);
    }

    [TestMethod]
    public void TwoSessionsWithDifferentDaysCombinedIsNoDays()
    {
        var odd = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var even = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var target = odd.And(even);
        var actual = target.Numbers;
        Assert.HasCount(0, actual);
    }

    [TestMethod]
    public void TwoSessionsWithComplementaryDaysCombinedIsAllDays()
    {
        var odd = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var even = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var target = odd.Or(even);
        var actual = target.Numbers;
        Assert.HasCount(14, actual);
        Assert.AreEqual(1, actual[0]);
        Assert.AreEqual(14, actual[^1]);
    }

    [TestMethod]
    public void IsOnDemand()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even | CommonSessionPatterns.OnDemand);
        var actual = target.Numbers;
        Assert.HasCount(7, actual);
        Assert.AreEqual(2, actual[0]);
        Assert.AreEqual(14, actual[^1]);
        Assert.IsTrue(target.IsOnDemand);
    }

    [TestMethod]
    public void Daily()
    {
        var target = Sessions.FromBitPattern(CommonDayPatterns.Daily);
        var actual = target.Days;
        Assert.HasCount(7, actual);
        Assert.AreEqual(Days.Monday, actual[0], "First day in week is Monday");
        Assert.AreEqual(Days.Sunday, actual[^1], "Last day in week is Sunday");
    }

    [TestMethod]
    public void MondayWednesdayFridaySunday()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        var actual = target.Days;
        Assert.HasCount(4, actual);
        Assert.AreEqual(Days.Monday, actual[0], "First day is Monday");
        Assert.AreEqual(Days.Sunday, actual[^1], "Last day is Sunday");
    }

    [TestMethod]
    public void TuesdayThursdaySaturday()
    {
        var target = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var actual = target.Days;
        Assert.HasCount(3, actual);
        Assert.AreEqual(Days.Tuesday, actual[0], "First day is Tuesday");
        Assert.AreEqual(Days.Saturday, actual[^1], "Last day is Saturday");
    }

    [TestMethod]
    public void ConstructorSetsCorrectDays()
    {
        var target = Sessions.FromDays(Days.Monday | Days.Friday);
        var actual = target.Numbers;
        Assert.HasCount(4, actual);
        var expected = new byte[] { 1, 5, 8, 12 };
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void ConstructorSetsCorrectSessions()
    {
        var target = Sessions.FromSessionNumbers([1, 5, 8, 12]);
        var actual = target.Numbers;
        Assert.HasCount(4, actual);
        var expected = new byte[] { 1, 5, 8, 12 };
        CollectionAssert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void DayResourceNameIsConsequtive()
    {
        var target = Sessions.FromDays(Days.Tuesday | Days.Wednesday | Days.Thursday);
        var actual = target.DaysResourceKey;
        Assert.AreEqual("Tuesday-Thursday", actual);
    }
}
