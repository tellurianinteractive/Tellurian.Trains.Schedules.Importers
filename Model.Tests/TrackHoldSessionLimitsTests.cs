using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Vehicles standing on a track before their first train and after their last stand there from the start of the
/// session to its end, not from midnight to midnight: nothing outside the session limits is operated, so no
/// conflict refers to a time outside them.
/// </summary>
[TestClass]
public class TrackHoldSessionLimitsTests
{
    private static Plan Arrange(Action<Settings.GeneralSettings> configure)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        var plan = Plan.Create("Test", timetable);
        configure(plan.Layout.Settings.General);
        var train = plan.Timetable.Trains.Single();
        train.Sessions = Sessions.All;
        var schedule = plan.CreateSchedule();
        schedule.Append(train.AsTrainPart);
        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 11, null), Sessions.All);
        return plan;
    }

    private static Time First(Plan plan) => plan.Timetable.Trains.Single().CallsInRunOrder[0].Arrival;
    private static Time Last(Plan plan) => plan.Timetable.Trains.Single().CallsInRunOrder[^1].Departure;

    [TestMethod]
    public void HoldsLieWithinTheOperatingWindow()
    {
        var plan = Arrange(general => { general.StartTime = TimeSpan.FromHours(7); general.EndTime = TimeSpan.FromHours(20); });

        var holds = plan.Schedules.TrackHolds.ToList();

        Assert.IsTrue(holds.Any(h => h.From == Time.FromHourAndMinute(7, 0) && h.To == First(plan)), "Before the first train: from the session start.");
        Assert.IsTrue(holds.Any(h => h.From == Last(plan) && h.To == Time.FromHourAndMinute(20, 0)), "After the last train: to the session end.");
        Assert.IsFalse(holds.Any(h => h.From.Value < TimeSpan.FromHours(7) || h.To.Value > TimeSpan.FromHours(20)));
    }

    [TestMethod]
    public void RoundTheClockHoldsFollowTheFirstSessionStart()
    {
        var plan = Arrange(general => { general.RunsOverMidnight = true; general.FirstSessionStartTime = TimeSpan.FromHours(7); });

        var holds = plan.Schedules.TrackHolds.ToList();

        Assert.IsTrue(holds.Any(h => h.From == Time.FromHourAndMinute(7, 0) && h.To == First(plan)), "Before the first train: from the first session start.");
        Assert.IsTrue(holds.Any(h => h.From == Last(plan) && h.To == Time.FromDayHourMinute(1, 7, 0)), "After the last train: until the next session starts.");
    }

    [TestMethod]
    public void RoundTheClockFromMidnightHoldsAreTheWholeDay()
    {
        var plan = Arrange(general => general.RunsOverMidnight = true);

        var holds = plan.Schedules.TrackHolds.ToList();

        Assert.IsTrue(holds.Any(h => h.From == Time.FromHourAndMinute(0, 0) && h.To == First(plan)));
        Assert.IsTrue(holds.Any(h => h.From == Last(plan) && h.To == Time.FromDayHourMinute(1, 0, 0)));
    }

    [TestMethod]
    public void NoHoldBeforeASessionThatStartsAfterTheFirstTrain()
    {
        var plan = Arrange(general => { general.RunsOverMidnight = true; general.FirstSessionStartTime = TimeSpan.FromHours(13); });

        var holds = plan.Schedules.TrackHolds.ToList();

        Assert.IsFalse(holds.Any(h => h.To == First(plan)),
            "The first train leaves before the session starts, so nothing stands there before it within the session.");
    }
}

/// <summary>
/// Vehicles that alternate between two schedules day by day stand overnight with one schedule only on the
/// evenings followed by a day of that schedule. On the other evenings they go on with the other schedule the next
/// day, and are no conflict with themselves.
/// </summary>
[TestClass]
public class TrackHoldAlternatingSchedulesTests
{
    private static readonly Sessions MondayWednesdayFridaySunday = Sessions.FromSessionNumbers(1, 3, 5, 7);
    private static readonly Sessions TuesdayThursdaySaturday = Sessions.FromSessionNumbers(2, 4, 6);

    [TestMethod]
    public void AWagonsetAlternatingBetweenTwoSchedulesOnOneTrackIsNoConflict()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(14, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 4, Time.FromHourAndMinute(15, 00)));
        var plan = Plan.Create("Test", timetable);
        plan.Layout.Settings.General.UseDays = true;
        plan.Layout.Settings.General.MaxSessions = 7;
        Train TrainNumber(int number) => plan.Timetable.Trains.Single(t => t.Number == number);
        TrainNumber(1).Sessions = TrainNumber(2).Sessions = MondayWednesdayFridaySunday;
        TrainNumber(3).Sessions = TrainNumber(4).Sessions = TuesdayThursdaySaturday;
        var home = TrainNumber(1).CallsInRunOrder[0].Track;

        // Both schedules leave from and come back to the same track at home, each with trains of its own.
        Schedule Round(Train outbound, Train inbound)
        {
            var schedule = plan.CreateSchedule();
            var first = schedule.Append(outbound.AsTrainPart).Value;
            var last = schedule.Append(inbound.AsTrainPart).Value;
            first.WagonSetOptions = new WagonSetOptions();
            last.WagonSetOptions = new WagonSetOptions();
            first.FromTrack = home;
            last.ToTrack = home;
            return schedule;
        }
        var wagonset = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 11, null);
        plan.AssignVehicle(Round(TrainNumber(1), TrainNumber(2)), wagonset, MondayWednesdayFridaySunday);
        plan.AssignVehicle(Round(TrainNumber(3), TrainNumber(4)), wagonset, TuesdayThursdaySaturday);

        var errors = home.GetValidationErrors(plan.Schedules, extendOccupancyByVehicleStay: true).ToList();

        Assert.IsEmpty(errors, string.Join(Environment.NewLine, errors.Select(e => e.Message.Text)));
    }
}
