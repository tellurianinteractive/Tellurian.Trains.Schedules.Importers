namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// A plan edited before times were entered on the right day holds a train's after-midnight times on the
/// first day; reading it puts them on the next day, so they sort after the times before midnight
/// (<c>Timetable.PlaceTimesAfterMidnight</c>).
/// </summary>
[TestClass]
public class PlaceTimesAfterMidnightTests
{
    // The forward train calls at start, +25/+30 and +55 minutes.
    private static (Timetable Timetable, Train Train) TimetableWithForwardTrainStartingAt(Time startTime, bool runsOverMidnight = true)
    {
        TestDataFactory.Init();
        var layout = TestDataFactory.Layout();
        layout.Settings.General.RunsOverMidnight = runsOverMidnight;
        var timetable = new Timetable("Test", layout);
        var category = new TrainCategory { Id = 1, Prefix = "G", Name = "G" };
        var train = TestDataFactory.CreateTrainInForwardDirection(category, 1, startTime);
        timetable.Add(train);
        return (timetable, train);
    }

    private static Time OnTheFirstDay(Time time) => Time.FromTimeSpan(TimeSpan.FromTicks(time.Value.Ticks % TimeSpan.TicksPerDay));

    private static (Time Arrival, Time Departure)[] TimesOf(Train train) =>
        [.. train.Calls.Select(c => (c.Arrival, c.Departure))];

    [TestMethod]
    public void TimesAfterMidnightStoredOnTheFirstDayAreMovedToTheNextDay()
    {
        var (timetable, train) = TimetableWithForwardTrainStartingAt(Time.FromHourAndMinute(23, 50));
        var expected = TimesOf(train);
        Assert.IsTrue(expected.Any(t => t.Departure.Value >= TimeSpan.FromDays(1)), "The test needs a train running past midnight.");
        foreach (var call in train.Calls)
        {
            call.Arrival = OnTheFirstDay(call.Arrival);
            call.Departure = OnTheFirstDay(call.Departure);
        }

        timetable.PlaceTimesAfterMidnight();

        CollectionAssert.AreEqual(expected, TimesOf(train));
        Assert.AreSame(train.Calls[0], train.CallsInRunOrder[0], "The train starts where it starts again.");
    }

    [TestMethod]
    public void ATrainAlreadyOnTheRightDaysIsLeftAsItIs()
    {
        var (timetable, train) = TimetableWithForwardTrainStartingAt(Time.FromHourAndMinute(23, 50));
        var expected = TimesOf(train);

        timetable.PlaceTimesAfterMidnight();

        CollectionAssert.AreEqual(expected, TimesOf(train));
    }

    [TestMethod]
    public void ATrainWithinOneDayIsLeftAsItIs()
    {
        var (timetable, train) = TimetableWithForwardTrainStartingAt(Time.FromHourAndMinute(8, 10));
        var expected = TimesOf(train);

        timetable.PlaceTimesAfterMidnight();

        CollectionAssert.AreEqual(expected, TimesOf(train));
    }

    [TestMethod]
    public void NothingIsMovedWhenTheLayoutDoesNotRunOverMidnight()
    {
        var (timetable, train) = TimetableWithForwardTrainStartingAt(Time.FromHourAndMinute(23, 50), runsOverMidnight: false);
        foreach (var call in train.Calls)
        {
            call.Arrival = OnTheFirstDay(call.Arrival);
            call.Departure = OnTheFirstDay(call.Departure);
        }
        var expected = TimesOf(train);

        timetable.PlaceTimesAfterMidnight();

        CollectionAssert.AreEqual(expected, TimesOf(train));
    }
}
