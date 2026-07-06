using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PlanCreateRepeatingTrainsTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true, DefaultSpeed = 100 };

    private static Plan SimplePlan()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        return new Plan("Test", new Timetable("Test", layout));
    }

    private static OperationLocation Location(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature);

    [TestMethod]
    public void CreatesFirstTrainPlusOneClonePerIntervalUpToEndTime()
    {
        var plan = SimplePlan();

        // 08:00 first departure, one per hour, last allowed departure 11:00 → 08, 09, 10, 11 = 4 trains.
        var trains = plan.CreateRepeating(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(11, 0), intervalMinutes: 60);

        Assert.AreEqual(4, trains.Count);
        Assert.AreEqual(4, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void SpacesConsecutiveDeparturesByTheInterval()
    {
        var plan = SimplePlan();

        var trains = plan.CreateRepeating(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(10, 0), intervalMinutes: 30);

        // Departure = first departing call time of each train.
        var departures = trains
            .Select(t => t.Calls.First(c => c.IsDeparture).Departure.Value.TotalMinutes)
            .ToList();
        CollectionAssert.AreEqual(
            new[] { 8 * 60.0, 8 * 60 + 30, 9 * 60.0, 9 * 60 + 30, 10 * 60.0 },
            departures);
    }

    [TestMethod]
    public void StopsBeforeADepartureThatWouldFallAfterEndTime()
    {
        var plan = SimplePlan();

        // 08:00, then +45 = 08:45, then +90 = 09:30 which is after 09:00 → only two trains.
        var trains = plan.CreateRepeating(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(9, 0), intervalMinutes: 45);

        Assert.AreEqual(2, trains.Count);
    }

    [TestMethod]
    public void AssignsSequentialTrainIdsAndSameParityNumbers()
    {
        var plan = SimplePlan();

        var trains = plan.CreateRepeating(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(10, 0), intervalMinutes: 60);

        CollectionAssert.AreEqual(
            new[] { 1, 2, 3 }, trains.Select(t => t.Id).ToArray());
        // M2 → Hm runs upward, so every train in the series takes the next free even number.
        CollectionAssert.AreEqual(
            new[] { 2, 4, 6 }, trains.Select(t => t.Number).ToArray());
    }

    [TestMethod]
    public void CreatesOnlyTheFirstTrainWhenEndTimeIsBeforeTheSecondDeparture()
    {
        var plan = SimplePlan();

        var trains = plan.CreateRepeating(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(8, 30), intervalMinutes: 60);

        Assert.AreEqual(1, trains.Count);
    }

    [TestMethod]
    public void ReturnsEmptyWhenNoPathBetweenLocations()
    {
        var plan = SimplePlan();
        var same = Location(plan, "M2");

        var trains = plan.CreateRepeating(
            Passenger, same, same, Start, Time.FromHourAndMinute(10, 0), intervalMinutes: 60);

        Assert.AreEqual(0, trains.Count);
        Assert.AreEqual(0, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void ThrowsWhenIntervalIsNotPositive()
    {
        var plan = SimplePlan();

        Assert.Throws<ArgumentOutOfRangeException>(() => plan.CreateRepeating(
            Passenger, Location(plan, "M2"), Location(plan, "Hm"),
            Start, Time.FromHourAndMinute(10, 0), intervalMinutes: 0));
    }
}
