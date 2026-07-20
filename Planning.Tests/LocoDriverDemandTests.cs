using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class LocoDriverDemandTests
{
    private static readonly TimeSpan WindowStart = TimeSpan.FromHours(10);
    private static readonly TimeSpan WindowEnd = TimeSpan.FromHours(12);

    private static Station Station(int id, string signature)
    {
        var station = new Station(id, signature, signature);
        station.Add(new StationTrack(id * 10 + 1, "1"));
        return station;
    }

    private static readonly TrainCategory Category = new() { Id = 1, Name = "G", Prefix = "G" };

    private static (Timetable Timetable, Station A, Station B) EmptyTimetable()
    {
        var a = Station(1, "A");
        var b = Station(2, "B");
        var layout = new Layout { Name = "Staffing" };
        layout.Add(a);
        layout.Add(b);
        layout.Add(new TrackStretch(1, a, b, 10));
        return (new Timetable("Test", layout), a, b);
    }

    // A train occupying a driver from `from` to `to`, running on the given sessions.
    private static Train AddTrain(Timetable timetable, Station a, Station b, int id, TimeSpan from, TimeSpan to, Sessions? sessions = null)
    {
        var train = new Train(id, Category, id) { Category = Category };
        if (sessions is { } s) train.Sessions = s;
        _ = train.Add(new StationCall(id * 10 + 1, a["1"], Time.FromTimeSpan(from), Time.FromTimeSpan(from)));
        _ = train.Add(new StationCall(id * 10 + 2, b["1"], Time.FromTimeSpan(to), Time.FromTimeSpan(to)));
        timetable.Add(train);
        return train;
    }

    private static int[] Demand(Timetable timetable, int maxSessions = 14, bool useDays = false) =>
        timetable.RequiredLocoDriversPerMinute(WindowStart, WindowEnd, useDays, maxSessions);

    // Minute index within the window for a whole hour and minute.
    private static int At(int hour, int minute = 0) =>
        (int)(new TimeSpan(hour, minute, 0) - WindowStart).TotalMinutes;

    [TestMethod]
    public void NoTrainsNeedsNoDrivers()
    {
        var (timetable, _, _) = EmptyTimetable();
        var demand = Demand(timetable);

        Assert.HasCount(120, demand);
        Assert.IsTrue(demand.All(d => d == 0));
    }

    [TestMethod]
    public void OneTrainNeedsOneDriverForItsWholeServiceWindow()
    {
        var (timetable, a, b) = EmptyTimetable();
        AddTrain(timetable, a, b, 1, new TimeSpan(10, 30, 0), new TimeSpan(11, 0, 0));

        var demand = Demand(timetable);

        Assert.AreEqual(0, demand[At(10, 29)]);
        Assert.AreEqual(1, demand[At(10, 30)]);
        Assert.AreEqual(1, demand[At(10, 59)]);
        // The window ends when the driver is released, so the last minute is no longer occupied.
        Assert.AreEqual(0, demand[At(11, 0)]);
    }

    [TestMethod]
    public void OverlappingTrainsOnTheSameSessionNeedOneDriverEach()
    {
        var (timetable, a, b) = EmptyTimetable();
        AddTrain(timetable, a, b, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));
        AddTrain(timetable, a, b, 2, new TimeSpan(10, 30, 0), new TimeSpan(11, 30, 0));

        var demand = Demand(timetable);

        Assert.AreEqual(1, demand[At(10, 15)]);
        Assert.AreEqual(2, demand[At(10, 45)]);
        Assert.AreEqual(1, demand[At(11, 15)]);
    }

    [TestMethod]
    public void TrainsOnDifferentSessionsDoNotAddUp()
    {
        var (timetable, a, b) = EmptyTimetable();
        // Both run 10:00–11:00, but never on the same session, so one driver covers both.
        AddTrain(timetable, a, b, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), Sessions.FromSessionNumbers(1));
        AddTrain(timetable, a, b, 2, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), Sessions.FromSessionNumbers(2));

        Assert.AreEqual(1, Demand(timetable)[At(10, 30)]);
    }

    [TestMethod]
    public void DemandIsTheWorstCaseOverAllSessions()
    {
        var (timetable, a, b) = EmptyTimetable();
        // Session 1 is the busiest with two concurrent trains; session 2 has only one.
        AddTrain(timetable, a, b, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), Sessions.FromSessionNumbers(1));
        AddTrain(timetable, a, b, 2, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), Sessions.FromSessionNumbers(1));
        AddTrain(timetable, a, b, 3, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), Sessions.FromSessionNumbers(2));

        Assert.AreEqual(2, Demand(timetable)[At(10, 30)]);
    }

    [TestMethod]
    public void SessionsOutsideTheOperatingPeriodAreIgnored()
    {
        var (timetable, a, b) = EmptyTimetable();
        // The layout has two sessions; a train running only on session 3 has no in-period session and so
        // counts on every session rather than disappearing.
        AddTrain(timetable, a, b, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), Sessions.FromSessionNumbers(1));
        AddTrain(timetable, a, b, 2, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0), Sessions.FromSessionNumbers(3));

        Assert.AreEqual(2, Demand(timetable, maxSessions: 2)[At(10, 30)]);
    }

    [TestMethod]
    public void ServiceRunningPastMidnightWrapsToTheStartOfTheDay()
    {
        var (timetable, a, b) = EmptyTimetable();
        // 23:30 to 24:20, i.e. twenty minutes into the next day.
        AddTrain(timetable, a, b, 1, new TimeSpan(23, 30, 0), new TimeSpan(24, 20, 0));

        var demand = timetable.RequiredLocoDriversPerMinute(TimeSpan.Zero, TimeSpan.FromHours(24), useDays: false, maxSessions: 14);

        Assert.AreEqual(1, demand[0]);
        Assert.AreEqual(1, demand[19]);
        Assert.AreEqual(0, demand[20]);
        Assert.AreEqual(1, demand[23 * 60 + 30]);
    }

    [TestMethod]
    public void ServiceReachingOutsideTheWindowIsClippedToIt()
    {
        var (timetable, a, b) = EmptyTimetable();
        AddTrain(timetable, a, b, 1, new TimeSpan(9, 0, 0), new TimeSpan(13, 0, 0));

        var demand = Demand(timetable);

        Assert.HasCount(120, demand);
        Assert.IsTrue(demand.All(d => d == 1));
    }

    [TestMethod]
    public void EmptyWindowNeedsNoDrivers()
    {
        var (timetable, a, b) = EmptyTimetable();
        AddTrain(timetable, a, b, 1, new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0));

        Assert.IsEmpty(timetable.RequiredLocoDriversPerMinute(WindowStart, WindowStart, useDays: false, maxSessions: 14));
    }
}
