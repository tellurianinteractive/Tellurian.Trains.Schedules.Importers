using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

/// <summary>
/// Covers what a category's stop pattern does to the route creator: it narrows where a new train is given
/// its stops, and constrains nothing while it is empty. See the model's <c>StopPatternRules</c>.
/// </summary>
[TestClass]
public class PlanCreateTrainStopPatternTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger, DefaultSpeed = 100 };

    private static Plan SimplePlan() => new("Test", new Timetable("Test", TestLayoutFactory.CreateSimpleLayout()));

    private static OperationLocation Location(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature);

    private static StationCall Call(Train train, string signature) =>
        train.Calls.Single(c => c.OperationLocation.Signature == signature);

    // M2 → Lund → Eslöv → Hässleholm, with the category's pattern naming the given locations.
    private static (Plan Plan, Train Train) Routed(params string[] stopSignatures)
    {
        var plan = SimplePlan();
        var category = Passenger;
        foreach (var signature in stopSignatures) category.StopLocationIds.Add(Location(plan, signature).Id);
        plan.Timetable.TrainCategories.Add(category);
        var train = plan.Create(category, Location(plan, "M2"), Location(plan, "Hm"), Start);
        Assert.IsNotNull(train);
        return (plan, train);
    }

    [TestMethod]
    public void ACategoryWithNoPatternStopsWhereverItCanExchange()
    {
        var (_, train) = Routed();

        Assert.IsTrue(Call(train, "Lu").IsStop);
        Assert.IsTrue(Call(train, "E").IsStop);
    }

    [TestMethod]
    public void OnlyTheLocationsThePatternNamesAreStops()
    {
        var (_, train) = Routed("Lu");

        Assert.IsTrue(Call(train, "Lu").IsStop, "Lund is in the pattern.");
        Assert.IsFalse(Call(train, "E").IsStop, "Eslöv is not, so the train runs through it.");
        Assert.IsFalse(Call(train, "E").IsArrival);
        Assert.IsFalse(Call(train, "E").IsDeparture);
    }

    [TestMethod]
    public void TheEndsAreStopsWhateverThePatternNames()
    {
        // The pattern names an intermediate location only: the train is still made ready where it starts
        // and put away where it ends.
        var (_, train) = Routed("Lu");

        Assert.IsTrue(Call(train, "M2").IsDeparture, "the train starts here");
        Assert.IsTrue(Call(train, "Hm").IsArrival, "and ends here");
    }

    [TestMethod]
    public void APatternNamingNowhereOnTheRouteLeavesTheTrainRunningThrough()
    {
        // Kävlinge is on the branch, so a train from Malmö to Hässleholm never passes it.
        var (_, train) = Routed("Kä");

        Assert.IsFalse(Call(train, "Lu").IsStop);
        Assert.IsFalse(Call(train, "E").IsStop);
    }

    [TestMethod]
    public void APatternDoesNotOpenAStopTheLocationDoesNotAllow()
    {
        var plan = SimplePlan();
        var eslöv = Location(plan, "E");
        eslöv.HasPassengerExchange = false;
        var category = Passenger;
        category.StopLocationIds.Add(Location(plan, "Lu").Id);
        category.StopLocationIds.Add(eslöv.Id);
        plan.Timetable.TrainCategories.Add(category);

        var train = plan.Create(category, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.IsTrue(Call(train, "Lu").IsStop);
        Assert.IsFalse(Call(train, "E").IsStop, "the pattern names it, but there are no passengers to exchange there");
    }

    [TestMethod]
    public void TheStopsThePatternTakesAwayShortenTheRun()
    {
        var stopping = Routed().Train;
        var running = Routed("Lu").Train;

        // Each stop costs a minute getting away and a minute braking, so dropping the Eslöv stop brings
        // the arrival at Hässleholm forward.
        Assert.IsTrue(Call(running, "Hm").Arrival < Call(stopping, "Hm").Arrival,
            "a train running through Eslöv arrives before one that stops there");
    }

    [TestMethod]
    public void TheReturnWorkingIsBuiltToTheSamePattern()
    {
        var (plan, train) = Routed("Lu");

        var back = plan.CreateReturn(train);

        Assert.IsNotNull(back);
        Assert.IsTrue(Call(back, "Lu").IsStop);
        Assert.IsFalse(Call(back, "E").IsStop);
    }
}
