using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// Editing one call time moves the times on one side of it, keeping the run and dwell times they already
/// have: a departure carries the rest of the run with it, an arrival the run leading up to it
/// (<c>Plan.SetDeparture</c> / <c>Plan.SetArrival</c>).
/// </summary>
[TestClass]
public class PlanCallTimeEditTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger, DefaultSpeed = 100 };

    private static Plan SimplePlan()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        var plan = new Plan("Test", new Timetable("Test", layout));
        // The pushed times must have room to land in; the window is not what is under test here.
        plan.Layout.Settings.General.AllowPlanTimeExtend = true;
        return plan;
    }

    private static OperationLocation Location(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature);

    // Malmö -> Hässleholm over Lund and Eslöv: four calls, the two in the middle to edit.
    private static Train MalmöToHässleholm(Plan plan) =>
        plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start)!;

    private static (Time Arrival, Time Departure)[] TimesOf(Train train) =>
        [.. train.CallsInRunOrder.Select(c => (c.Arrival, c.Departure))];

    [TestMethod]
    public void SettingADepartureInTheMiddlePushesEveryLaterTimeByTheSameMinutes()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[1];

        var result = plan.SetDeparture(edited, edited.Departure.AddMinutes(5));

        Assert.IsNotNull(result);
        var after = TimesOf(train);
        Assert.AreEqual(before[1].Departure.AddMinutes(5), after[1].Departure, "The edited departure is what was asked for.");
        for (var i = 2; i < after.Length; i++)
        {
            Assert.AreEqual(before[i].Arrival.AddMinutes(5), after[i].Arrival, $"Arrival of call {i} follows.");
            Assert.AreEqual(before[i].Departure.AddMinutes(5), after[i].Departure, $"Departure of call {i} follows.");
        }
    }

    [TestMethod]
    public void SettingADepartureKeepsTheCallsOwnArrivalAndTheTimesBeforeIt()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[1];

        plan.SetDeparture(edited, edited.Departure.AddMinutes(5));

        var after = TimesOf(train);
        Assert.AreEqual(before[1].Arrival, after[1].Arrival, "The train still arrives when it did; it stands five minutes longer.");
        Assert.AreEqual(before[0].Arrival, after[0].Arrival);
        Assert.AreEqual(before[0].Departure, after[0].Departure, "The edit works forwards; the origin is untouched.");
    }

    [TestMethod]
    public void SettingAnEarlierDeparturePullsEveryLaterTimeBack()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[1];

        plan.SetDeparture(edited, edited.Departure.AddMinutes(-3));

        var after = TimesOf(train);
        Assert.AreEqual(before[^1].Arrival.AddMinutes(-3), after[^1].Arrival);
        Assert.AreEqual(before[^1].Departure.AddMinutes(-3), after[^1].Departure);
    }

    [TestMethod]
    public void SettingAnArrivalInTheMiddlePushesEveryEarlierTimeByTheSameMinutes()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[1];

        var result = plan.SetArrival(edited, edited.Arrival.AddMinutes(4));

        Assert.IsNotNull(result);
        var after = TimesOf(train);
        Assert.AreEqual(before[1].Arrival.AddMinutes(4), after[1].Arrival, "The edited arrival is what was asked for.");
        Assert.AreEqual(before[0].Arrival.AddMinutes(4), after[0].Arrival, "The origin arrival follows.");
        Assert.AreEqual(before[0].Departure.AddMinutes(4), after[0].Departure, "The origin departure follows, keeping the run time into the call.");
    }

    [TestMethod]
    public void SettingAnArrivalKeepsTheCallsOwnDepartureAndTheTimesAfterIt()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[1];

        plan.SetArrival(edited, edited.Arrival.AddMinutes(4));

        var after = TimesOf(train);
        Assert.AreEqual(before[1].Departure, after[1].Departure, "The train still leaves when it did; it stands four minutes less.");
        for (var i = 2; i < after.Length; i++)
        {
            Assert.AreEqual(before[i].Arrival, after[i].Arrival, $"Arrival of call {i} is untouched.");
            Assert.AreEqual(before[i].Departure, after[i].Departure, $"The edit works backwards; call {i} is untouched.");
        }
    }

    [TestMethod]
    public void SettingAnEarlierArrivalPullsEveryEarlierTimeBack()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[^2];

        plan.SetArrival(edited, edited.Arrival.AddMinutes(-3));

        var after = TimesOf(train);
        Assert.AreEqual(before[0].Arrival.AddMinutes(-3), after[0].Arrival);
        Assert.AreEqual(before[0].Departure.AddMinutes(-3), after[0].Departure, "The train starts three minutes earlier to arrive earlier.");
    }

    [TestMethod]
    public void SettingTheOriginArrivalChangesThePreparationTimeAndNothingElse()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var origin = train.CallsInRunOrder[0];

        plan.SetArrival(origin, origin.Arrival.AddMinutes(-5));

        var after = TimesOf(train);
        Assert.AreEqual(before[0].Arrival.AddMinutes(-5), after[0].Arrival, "The driver now has five minutes more to prepare.");
        Assert.AreEqual(before[0].Departure, after[0].Departure, "The origin departure anchors the train.");
        for (var i = 1; i < after.Length; i++)
        {
            Assert.AreEqual(before[i].Arrival, after[i].Arrival);
            Assert.AreEqual(before[i].Departure, after[i].Departure);
        }
    }

    [TestMethod]
    public void SettingTheTerminusArrivalMovesTheWholeRunBeforeIt()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var terminus = train.CallsInRunOrder[^1];

        plan.SetArrival(terminus, terminus.Arrival.AddMinutes(7));

        var after = TimesOf(train);
        Assert.AreEqual(before[^1].Arrival.AddMinutes(7), after[^1].Arrival);
        Assert.AreEqual(before[^1].Departure, after[^1].Departure, "The driver still finishes when they did; the finishing time absorbs the change.");
        for (var i = 0; i < after.Length - 1; i++)
        {
            Assert.AreEqual(before[i].Arrival.AddMinutes(7), after[i].Arrival, $"Arrival of call {i} follows.");
            Assert.AreEqual(before[i].Departure.AddMinutes(7), after[i].Departure, $"Departure of call {i} follows.");
        }
    }

    [TestMethod]
    public void ChangeThatWouldLeaveTheOperatingWindowIsRefusedAndChangesNothing()
    {
        var plan = SimplePlan();
        // No extending here: the window is the constraint under test.
        plan.Layout.Settings.General.AllowPlanTimeExtend = false;
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[1];
        // Half an hour past the end of the operating window (20:00 by default).
        var pastTheWindow = (int)(plan.Layout.Settings.General.EndTime - train.CallsInRunOrder[^1].Departure.Value).TotalMinutes + 30;

        var result = plan.SetDeparture(edited, edited.Departure.AddMinutes(pastTheWindow));

        Assert.IsNull(result, "The train would run past the end of the operating window.");
        Assert.AreEqual(before.Length, TimesOf(train).Length);
        for (var i = 0; i < before.Length; i++)
        {
            Assert.AreEqual(before[i].Arrival, TimesOf(train)[i].Arrival, $"Arrival of call {i} is untouched.");
            Assert.AreEqual(before[i].Departure, TimesOf(train)[i].Departure, $"Departure of call {i} is untouched.");
        }
    }

    [TestMethod]
    public void ArrivalChangeThatWouldStartTheTrainBeforeTheWindowIsRefusedAndChangesNothing()
    {
        var plan = SimplePlan();
        // No extending here: the window is the constraint under test.
        plan.Layout.Settings.General.AllowPlanTimeExtend = false;
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[^1];
        // Half an hour before the start of the operating window: the edit pulls the whole run before it back.
        var beforeTheWindow = (int)(plan.Layout.Settings.General.StartTime - train.CallsInRunOrder[0].Arrival.Value).TotalMinutes - 30;

        var result = plan.SetArrival(edited, edited.Arrival.AddMinutes(beforeTheWindow));

        Assert.IsNull(result, "The train would start before the operating window opens.");
        var after = TimesOf(train);
        for (var i = 0; i < before.Length; i++)
        {
            Assert.AreEqual(before[i].Arrival, after[i].Arrival, $"Arrival of call {i} is untouched.");
            Assert.AreEqual(before[i].Departure, after[i].Departure, $"Departure of call {i} is untouched.");
        }
    }

    [TestMethod]
    public void SettingATimeToWhatItAlreadyIsChangesNothing()
    {
        var plan = SimplePlan();
        var train = MalmöToHässleholm(plan);
        var before = TimesOf(train);
        var edited = train.CallsInRunOrder[1];

        plan.SetDeparture(edited, edited.Departure);

        var after = TimesOf(train);
        for (var i = 0; i < before.Length; i++)
        {
            Assert.AreEqual(before[i].Arrival, after[i].Arrival);
            Assert.AreEqual(before[i].Departure, after[i].Departure);
        }
    }

    // A time is entered as a clock time; on a train running past midnight it belongs to the next day, so that
    // it still comes after the times before midnight.
    private static Plan OverMidnightPlan()
    {
        var plan = SimplePlan();
        plan.Layout.Settings.General.RunsOverMidnight = true;
        return plan;
    }

    private static Time ClockTime(Time time) => Time.FromTimeSpan(TimeSpan.FromTicks(time.Value.Ticks % TimeSpan.TicksPerDay));

    [TestMethod]
    public void AClockTimeAfterMidnightGoesOnTheNextDayAndKeepsTheRunOrder()
    {
        var plan = OverMidnightPlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Time.FromHourAndMinute(23, 40))!;
        var edited = train.CallsInRunOrder[1];
        var callsBefore = train.CallsInRunOrder.ToArray();

        var result = plan.SetDeparture(edited, Time.FromHourAndMinute(0, 5));

        Assert.IsNotNull(result);
        Assert.AreEqual(Time.FromDayHourMinute(1, 0, 5), edited.Departure, "00:05 after a call before midnight is 24:05.");
        CollectionAssert.AreEqual(callsBefore, train.CallsInRunOrder.ToArray(), "The calls stay in the order the train runs them.");
        for (var i = 2; i < callsBefore.Length; i++)
            Assert.IsTrue(callsBefore[i].Arrival >= edited.Departure, $"Call {i} still comes after the edited departure.");
    }

    [TestMethod]
    public void AClockTimeEnteredOverATimeAfterMidnightStaysOnTheNextDay()
    {
        var plan = OverMidnightPlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Time.FromHourAndMinute(23, 50))!;
        var edited = train.CallsInRunOrder[^1];
        Assert.IsTrue(edited.Arrival.Value >= TimeSpan.FromDays(1), "The test needs a train arriving after midnight.");
        var expected = edited.Arrival.AddMinutes(2);

        var result = plan.SetArrival(edited, ClockTime(expected));

        Assert.IsNotNull(result);
        Assert.AreEqual(expected, edited.Arrival);
        Assert.AreSame(edited, train.CallsInRunOrder[^1], "The destination is still the last call.");
    }

    [TestMethod]
    public void AClockTimeBeforeMidnightEnteredOverATimeAfterItMovesBackToTheFirstDay()
    {
        var plan = OverMidnightPlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Time.FromHourAndMinute(23, 50))!;
        var edited = train.CallsInRunOrder[^1];
        var origin = train.CallsInRunOrder[0];
        var clock = Time.FromHourAndMinute(23, 59);

        plan.SetArrival(edited, clock);

        Assert.AreEqual(clock, edited.Arrival, "23:59 is nearer a time just after midnight on the same night.");
        Assert.IsTrue(origin.Departure < edited.Arrival);
    }
}
