namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class OperatingWindowTests
{
    private static Timetable TimetableWithForwardTrainStartingAt(Time startTime)
    {
        TestDataFactory.Init();
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, startTime));
        return timetable;
    }

    [TestMethod]
    public void UsesServiceSpanWhenAllCallsAreWithinTheDay()
    {
        // Forward train calls at 08:10, 08:35/08:40, 09:05 -> floored start 08:00, ceiled end 10:00.
        var timetable = TimetableWithForwardTrainStartingAt(Time.FromHourAndMinute(8, 10));
        var (start, end) = timetable.OperatingWindow();
        Assert.AreEqual(TimeSpan.FromHours(8), start);
        Assert.AreEqual(TimeSpan.FromHours(10), end);
    }

    [TestMethod]
    public void SpansFullDayWhenATrainRunsPastMidnight()
    {
        // Forward train starting 23:50 has later calls at 24:15/24:20 and 24:45 (past midnight).
        var timetable = TimetableWithForwardTrainStartingAt(Time.FromHourAndMinute(23, 50));
        var (start, end) = timetable.OperatingWindow();
        Assert.AreEqual(TimeSpan.Zero, start);
        Assert.AreEqual(TimeSpan.FromHours(24), end);
    }
}
