using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PlanUpdateTimingsTests
{
    private static readonly Time Start = Time.FromHourAndMinute(8, 0);

    private static TrainCategory Passenger => new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true, DefaultSpeed = 100 };
    private static TrainCategory Freight => new() { Id = 2, Name = "Freight", Prefix = "G", IsFreight = true, DefaultSpeed = 100 };

    private static Plan SimplePlan()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        return new Plan("Test", new Timetable("Test", layout));
    }

    private static OperationLocation Location(Plan plan, string signature) =>
        plan.Layout.OperationLocations.First(l => l.Signature == signature);

    private static StationCall Call(Train train, string signature) =>
        train.Calls.Single(c => c.OperationLocation.Signature == signature);

    /// <summary>The run time in minutes from the departure at one location to the arrival at the next.</summary>
    private static int LegMinutes(Train train, string fromSignature, string toSignature) =>
        (int)(Call(train, toSignature).Arrival.Value - Call(train, fromSignature).Departure.Value).TotalMinutes;

    [TestMethod]
    public void KeepsTheOriginDepartureFixed()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start)!;
        var originArrival = train.Calls[0].Arrival;

        plan.UpdateTimings(train);

        Assert.AreEqual(Start, train.Calls[0].Departure);
        Assert.AreEqual(originArrival, train.Calls[0].Arrival);
    }

    [TestMethod]
    public void RemovingAnIntermediateStopClearsItsDwellAndBringsTheTerminusEarlier()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start)!;
        var eslöv = Call(train, "E");
        var removedDwell = eslöv.Departure.Value - eslöv.Arrival.Value;
        var terminusArrivalBefore = train.Calls[^1].Arrival;

        // Change the stop pattern: Eslöv is now a pass-through.
        eslöv.IsArrival = false;
        eslöv.IsDeparture = false;

        var result = plan.UpdateTimings(train);

        Assert.IsNotNull(result);
        Assert.AreEqual(eslöv.Arrival, eslöv.Departure, "A pass-through has equal arrival and departure times.");
        // Eslöv no longer costs its dwell, nor the minute the stop added to the leg in and the leg out, so
        // the terminus arrives earlier by all three together.
        Assert.AreEqual(
            terminusArrivalBefore.AddMinutes(-(int)removedDwell.TotalMinutes - 2 * TrainExtensions.StopAllowanceMinutes),
            train.Calls[^1].Arrival);
    }

    [TestMethod]
    public void MakingACallAStopAddsAMinuteToTheLegOnEachSideOfIt()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start)!;
        var eslöv = Call(train, "E");

        // Start from a pass-through, so neither leg around Eslöv carries an allowance for it.
        eslöv.IsArrival = false;
        eslöv.IsDeparture = false;
        plan.UpdateTimings(train);
        var legInBefore = LegMinutes(train, "Lu", "E");
        var legOutBefore = LegMinutes(train, "E", "Hm");

        eslöv.IsArrival = true;
        eslöv.IsDeparture = true;
        var result = plan.UpdateTimings(train);

        Assert.IsNotNull(result);
        Assert.AreEqual(legInBefore + TrainExtensions.StopAllowanceMinutes, LegMinutes(train, "Lu", "E"),
            "Braking into the new stop lengthens the leg leading up to it.");
        Assert.AreEqual(legOutBefore + TrainExtensions.StopAllowanceMinutes, LegMinutes(train, "E", "Hm"),
            "Getting away from the new stop lengthens the leg following it.");
    }

    [TestMethod]
    public void AddingAnIntermediateStopGivesItADwellAndPushesTheTerminusLater()
    {
        var plan = SimplePlan();
        var train = plan.Create(Freight, Location(plan, "M2"), Location(plan, "Hm"), Start)!;
        var eslöv = Call(train, "E");

        // Start from a pass-through: the freight is planned through Eslöv without stopping. Eslöv keeps
        // its cargo exchange, because a train that cannot stop at a location can never be made to
        // (see the model's StopRules) — the stop pattern is the only thing being changed here.
        eslöv.IsArrival = false;
        eslöv.IsDeparture = false;
        plan.UpdateTimings(train);
        Assert.IsTrue(eslöv.IsPassthrough, "Precondition: the freight passes Eslöv.");
        var terminusArrivalBefore = train.Calls[^1].Arrival;

        // Change the stop pattern: the train now stops at Eslöv.
        eslöv.IsArrival = true;
        eslöv.IsDeparture = true;

        var result = plan.UpdateTimings(train);

        Assert.IsNotNull(result);
        Assert.IsTrue(eslöv.Departure > eslöv.Arrival, "A new stop is given a dwell.");
        Assert.IsTrue(train.Calls[^1].Arrival > terminusArrivalBefore, "The added dwell pushes the terminus later.");
    }

    [TestMethod]
    public void RecomputesConsistentRunAndDwellTimesFromCorruptedTimes()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start)!;

        // Corrupt every non-origin time; UpdateTimings must restore an ordered, consistent schedule.
        foreach (var call in train.Calls.Skip(1))
        {
            call.Arrival = Start;
            call.Departure = Start;
        }

        var result = plan.UpdateTimings(train);

        Assert.IsNotNull(result);
        for (var i = 0; i < train.Calls.Count; i++)
        {
            Assert.IsTrue(train.Calls[i].Departure >= train.Calls[i].Arrival);
            if (i > 0) Assert.IsTrue(train.Calls[i].Arrival > train.Calls[i - 1].Departure);
        }
    }

    [TestMethod]
    public void PreservesAnIntentionalDwellOnAStopThatStaysAStop()
    {
        var plan = SimplePlan();
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start)!;
        var lund = Call(train, "Lu");
        // Give Lund a deliberately long dwell.
        lund.Departure = lund.Arrival.AddMinutes(20);

        plan.UpdateTimings(train);

        Assert.AreEqual(20, (int)(lund.Departure.Value - lund.Arrival.Value).TotalMinutes,
            "An existing dwell on a stop that stays a stop is kept.");
    }

    [TestMethod]
    public void ReturnsTheTrainUnchangedWhenItHasFewerThanTwoCalls()
    {
        var plan = SimplePlan();
        var train = new Train(1, 1) { Sessions = Sessions.All };
        plan.Timetable.Add(train);

        Assert.AreSame(train, plan.UpdateTimings(train));
    }
}
