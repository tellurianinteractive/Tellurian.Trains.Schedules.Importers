using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Two wagonsets that take turns on one schedule (one works the odd sessions, the other the even) do not
/// stand on the track at the same time by design: the one that is not working stands where it was left,
/// all day, so both being put on the same track is a conflict.
/// </summary>
[TestClass]
public class TrackHoldSessionsTests
{
    private static readonly Sessions Odd = Sessions.FromSessionNumbers(1, 3, 5, 7, 9, 11, 13);
    private static readonly Sessions Even = Sessions.FromSessionNumbers(2, 4, 6, 8, 10, 12, 14);

    private static (Plan Plan, StationTrack Track) Arrange(StationTrack? secondTrack = null)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        var plan = Plan.Create("Test", timetable);
        var train = plan.Timetable.Trains.First(t => t.Number == 1);
        train.Sessions = Sessions.All;

        var track = plan.Layout.OperationLocations.Single(l => l.Signature == "Snu")["2"];
        var schedule = plan.CreateSchedule();
        var part = schedule.Append(train.AsTrainPart).Value;
        part.WagonSetOptions = new WagonSetOptions();
        part.ToTrack = track;
        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 11, null), Odd);
        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 12, null), Even);
        return (plan, track);
    }

    [TestMethod]
    public void WagonsetsWorkingDifferentSessionsHaveHoldsOfTheirOwn()
    {
        var (plan, track) = Arrange();

        var holds = plan.Schedules.TrackHolds.Where(h => h.Track.Equals(track) && !h.IsIdle).ToList();

        Assert.HasCount(2, holds);
        Assert.IsTrue(holds.Any(h => h.Sessions.Overlaps(Odd) && !h.Sessions.Overlaps(Even)));
        Assert.IsTrue(holds.Any(h => h.Sessions.Overlaps(Even) && !h.Sessions.Overlaps(Odd)));
    }

    [TestMethod]
    public void AWagonsetNotWorkingASessionStandsIdleOnItsTrackAllDay()
    {
        var (plan, track) = Arrange();

        var idle = plan.Schedules.TrackHolds.Where(h => h.Track.Equals(track) && h.IsIdle).ToList();

        Assert.HasCount(2, idle);
        Assert.IsTrue(idle.All(h => h.From == Time.FromDayHourMinute(0, 0, 0) && h.To == Time.FromDayHourMinute(1, 0, 0)));
        Assert.IsTrue(idle.Any(h => h.Sessions.Overlaps(Even) && !h.Sessions.Overlaps(Odd)), "The odd-session wagonset stands idle on the even sessions.");
    }

    [TestMethod]
    public void TwoWagonsetsPutOnTheSameTrackConflict()
    {
        var (plan, track) = Arrange();

        var errors = track.GetValidationErrors(plan.Schedules, extendOccupancyByVehicleStay: true).ToList();

        Assert.IsNotEmpty(errors, "One wagonset is idle on the track while the other is put on it.");
    }

    [TestMethod]
    public void WagonsetsAssignedToAllSessionsHaveNoIdleHolds()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        var plan = Plan.Create("Test", timetable);
        var train = plan.Timetable.Trains.First();
        train.Sessions = Sessions.All;
        var schedule = plan.CreateSchedule();
        schedule.Append(train.AsTrainPart);
        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 11, null), Sessions.All);

        Assert.IsFalse(plan.Schedules.TrackHolds.Any(h => h.IsIdle));
    }
}
