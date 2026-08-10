using Tellurian.Trains.Schedules.Planning.Components.Reporting;
using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// Covers which operation locations earn a row in a timetable table. Only a location where at least one
/// train running the stretch stops does; everywhere else the column would hold nothing but pass-through
/// marks. The rule is read from the trains, so the list of locations grows as stops are planned.
/// </summary>
[TestClass]
public sealed class TimetableTableStopFilterTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true, DefaultSpeed = 100 };
    private static TrainCategory Freight => new() { Id = 2, Name = "Freight", Prefix = "G", IsFreight = true, DefaultSpeed = 80 };

    private readonly Plan _plan = TestLayoutFactory.CreatePlan();

    // Lysekil ── Göransberg ── Furudalen ── [Kristineberg] ── Munkeröd ── [Kyrkeby Östra] ── Devsjö
    //   ── Lekby ── Rotebro ── Malmö, where the two in brackets are signal controlled.
    private TimetableStretch MainLine => _plan.Layout.TimetableStretches.Single(s => s.Number == "1");

    private OperationLocation Location(string signature) =>
        _plan.Layout.OperationLocations.First(l => l.Signature == signature);

    private IReadOnlyList<string> RowNames(TrainGraphDirection direction = TrainGraphDirection.Upward) =>
        [.. TimetableTable.Create(MainLine, _plan.Timetable.Trains, direction).Rows
            .Where(r => r.Kind == TimetableRowKind.Normal)
            .Select(r => r.StationName)];

    // The number is given explicitly because trains are identified by it: the default number is only
    // free within the train's own category, so two trains of different categories can collide.
    private Train Run(TrainCategory category, string from, string to, int number)
    {
        var train = _plan.Create(category, Location(from), Location(to), Start, number: number);
        Assert.IsNotNull(train, $"The {category.Name} train {from}–{to} should have been created.");
        Assert.Contains(train, _plan.Timetable.Trains, $"Train {number} should be in the timetable.");
        return train;
    }

    [TestMethod]
    public void SignalControlledLocationsGetNoRow()
    {
        Run(Passenger, "Lys", "M", 2);

        // A train never stops at a signal controlled location, so Kristineberg and Kyrkeby Östra —
        // which the train does run through — are left out.
        CollectionAssert.AreEqual(
            new[] { "Lysekil", "Göransberg", "Furudalen", "Munkeröd", "Devsjö", "Lekby", "Rotebro", "Malmö" },
            RowNames().ToArray());
    }

    [TestMethod]
    public void AStationEveryTrainRunsPastGetsNoRow()
    {
        // Without passenger exchange a passenger train cannot stop at Göransberg, whatever the call says.
        Location("Gbg").HasPassengerExchange = false;
        Run(Passenger, "Lys", "M", 2);

        var rows = RowNames();
        Assert.DoesNotContain("Göransberg", rows, "Nobody stops there, so it is not worth a row.");
        Assert.Contains("Furudalen", rows, "The stations that are stopped at stay.");
    }

    [TestMethod]
    public void AStationEarnsItsRowFromTheOneTrainThatStopsThere()
    {
        Location("Gbg").HasPassengerExchange = false;
        Run(Passenger, "Lys", "M", 2);
        Assert.DoesNotContain("Göransberg", RowNames());

        // The freight train has cargo to exchange at Göransberg, so it stops — and one stop is enough.
        Run(Freight, "Lys", "M", 5001);
        Assert.Contains("Göransberg", RowNames());
    }

    [TestMethod]
    public void BothDirectionsOfAStretchListTheSameLocations()
    {
        Location("Gbg").HasPassengerExchange = false;
        Run(Passenger, "Lys", "M", 2);
        // The only train stopping at Göransberg runs the other way, but the stretch's two tables are
        // printed together, so they list the same locations in opposite order.
        Run(Freight, "M", "Lys", 5001);

        var upward = RowNames();
        var downward = RowNames(TrainGraphDirection.Downward);
        Assert.Contains("Göransberg", upward);
        CollectionAssert.AreEqual(upward.Reverse().ToArray(), downward.ToArray());
    }
}
