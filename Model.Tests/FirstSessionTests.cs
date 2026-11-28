using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Dispatching;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class FirstSessionTests
{
    // Forward 12:00 G->Yb->Snu (12:55), return 13:00 Snu->Yb->G (13:55).
    private static Plan CreatePlan(TimeSpan? firstSessionStart = null, int maxSessions = 6)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = TestDataFactory.NewTimetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        var general = timetable.Settings.General;
        general.MaxSessions = maxSessions;
        if (firstSessionStart is { } start)
        {
            general.RunsOverMidnight = true;
            general.FirstSessionStartTime = start;
        }
        return Plan.Create("Test", timetable);
    }

    private static Train Forward(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 1);
    private static Train Return(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 2);

    // A driver duty driving the forward train and then the return train.
    private static DriverDuty AddDutyWithBothTrains(Plan plan, Sessions? sessions = null)
    {
        var duty = plan.CreateDriverDuty();
        if (sessions is { } s) duty.Sessions = s;
        foreach (var train in new[] { Forward(plan), Return(plan) })
        {
            var schedule = plan.CreateSchedule();
            var part = schedule.Add(train.AsTrainPart);
            plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", train.Number, null));
            duty.Append(part);
        }
        return duty;
    }

    [TestMethod]
    public void PeriodNumbersAreTheSessionsOfTheOperatingPeriod()
    {
        var settings = new SessionsSettings { MaxNumberOfSessions = 6 };

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, settings.PeriodNumbers.ToArray());
    }

    [TestMethod]
    public void PeriodNumbersAreAtMostSevenDays()
    {
        var settings = new SessionsSettings { MaxNumberOfSessions = 14, UseDaysInsteadOfSessionNumbers = true };

        Assert.HasCount(7, settings.PeriodNumbers);
    }

    [TestMethod]
    public void NumbersWithinIgnoreSessionsBeyondThePeriod()
    {
        var settings = new SessionsSettings { MaxNumberOfSessions = 6 };

        var numbers = Sessions.FromSessionNumbers(4, 5, 6, 11, 12, 13).NumbersWithin(settings);

        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, numbers.ToArray());
    }

    [TestMethod]
    public void NoTrainStartsBeforeFirstSessionWhenMeetingBeginsWithTheOperatingWindow()
    {
        var plan = CreatePlan();

        Assert.IsEmpty(plan.TrainsStartingBeforeFirstSession);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, plan.SessionNumbersOf(Forward(plan)).ToArray());
    }

    [TestMethod]
    public void TrainStartingBeforeFirstSessionDoesNotRunInFirstSession()
    {
        var plan = CreatePlan(firstSessionStart: TimeSpan.FromHours(12.5));

        Assert.IsTrue(plan.StartsBeforeFirstSession(Forward(plan)));
        Assert.IsFalse(plan.StartsBeforeFirstSession(Return(plan)));
        CollectionAssert.AreEqual(new[] { 2, 3, 4, 5, 6 }, plan.SessionNumbersOf(Forward(plan)).ToArray());
    }

    [TestMethod]
    public void TrainsOfDriverDutyStartingBeforeFirstSessionDoNotRunInFirstSession()
    {
        var plan = CreatePlan(firstSessionStart: TimeSpan.FromHours(12.5));
        AddDutyWithBothTrains(plan);

        Assert.IsTrue(plan.StartsBeforeFirstSession(Return(plan)), "The return train starts 13:00, but its duty starts with the forward train.");
        Assert.DoesNotContain(1, plan.SessionNumbersOf(Return(plan)).ToArray());
    }

    [TestMethod]
    public void ClearancesOfTrainInDriverDutyStartingBeforeFirstSessionAreOutsideFirstSession()
    {
        var plan = CreatePlan(firstSessionStart: TimeSpan.FromHours(12.5));
        AddDutyWithBothTrains(plan);
        var settings = plan.Timetable.Settings.General.SessionSettings();
        var origin = Return(plan).CallsInRunOrder[0].OperationLocation;

        var withDuties = Return(plan).ClearancesAt(origin, settings, plan).Single();
        var withoutDuties = Return(plan).ClearancesAt(origin, settings).Single();

        Assert.AreEqual(ClearanceKind.Departure, withDuties.Kind);
        Assert.IsTrue(withDuties.IsOutsideFirstSession);
        Assert.IsFalse(withoutDuties.IsOutsideFirstSession, "Without the plan only the train's own start counts.");
    }

    [TestMethod]
    public void DriverDutyNotWorkedInFirstSessionDoesNotExcludeItsTrains()
    {
        var plan = CreatePlan(firstSessionStart: TimeSpan.FromHours(12.5));
        AddDutyWithBothTrains(plan, Sessions.FromSessionNumbers(2, 3, 4, 5, 6));

        Assert.IsFalse(plan.StartsBeforeFirstSession(Return(plan)));
    }
}
