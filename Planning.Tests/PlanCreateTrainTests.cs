using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PlanCreateTrainTests
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

    [TestMethod]
    public void CreatesTrainWithOneCallPerLocationOnPath()
    {
        var plan = SimplePlan();

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        // M2 → Lund → Eslöv → Hässleholm
        CollectionAssert.AreEqual(
            new[] { "M2", "Lu", "E", "Hm" },
            train.Calls.Select(c => c.OperationLocation.Signature).ToArray());
    }

    [TestMethod]
    public void AddsTheTrainToTheTimetableWithNextIdAndNumber()
    {
        var plan = SimplePlan();

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.IsTrue(plan.Timetable.Trains.Contains(train));
        Assert.AreEqual(1, train.Id);
        Assert.AreEqual(1, train.Number);
        Assert.AreEqual(Passenger, train.Category);
        Assert.AreEqual(Sessions.All, train.Sessions);
    }

    [TestMethod]
    public void OriginIsDepartureOnlyAndTerminusIsArrivalOnly()
    {
        var plan = SimplePlan();

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, preparationMinutes: 10, finishingMinutes: 15);

        Assert.IsNotNull(train);
        var origin = train.Calls[0];
        var terminus = train.Calls[^1];

        Assert.IsTrue(origin.IsDeparture && !origin.IsArrival);
        Assert.AreEqual(Start, origin.Departure);
        Assert.AreEqual(Start.AddMinutes(-10), origin.Arrival); // driver prepares 10 minutes before departure

        Assert.IsTrue(terminus.IsArrival && !terminus.IsDeparture);
        Assert.AreEqual(terminus.Arrival.AddMinutes(15), terminus.Departure); // driver finishes 15 minutes after arrival
    }

    [TestMethod]
    public void IntermediateStationsAreStopsForAPassengerTrainThatCanExchangeThere()
    {
        var plan = SimplePlan();

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        foreach (var call in train.Calls.Skip(1).SkipLast(1))
        {
            Assert.IsTrue(call.IsStop, $"Expected a stop at {call.OperationLocation.Signature}.");
            Assert.IsTrue(call.Departure > call.Arrival, $"Expected a dwell at {call.OperationLocation.Signature}.");
        }
    }

    [TestMethod]
    public void TimesAreOrderedAlongTheTrain()
    {
        var plan = SimplePlan();

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        for (var i = 0; i < train.Calls.Count; i++)
        {
            Assert.IsTrue(train.Calls[i].Departure >= train.Calls[i].Arrival);
            if (i > 0) Assert.IsTrue(train.Calls[i].Arrival > train.Calls[i - 1].Departure);
        }
    }

    [TestMethod]
    public void FreightTrainPassesALocationWithoutCargoExchange()
    {
        var plan = SimplePlan();
        // Eslöv can exchange passengers but not cargo, so a freight train passes it without stopping.
        var eslöv = Location(plan, "E");
        eslöv.HasCargoExchange = false;

        var train = plan.Create(Freight, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        var eslövCall = train.Calls.Single(c => c.OperationLocation.Signature == "E");
        Assert.IsTrue(eslövCall.IsPassthrough);
        Assert.IsFalse(eslövCall.IsArrival || eslövCall.IsDeparture);
        Assert.AreEqual(eslövCall.Arrival, eslövCall.Departure);
        // A cargo-exchange station on the way (Lund) is still a stop.
        Assert.IsTrue(train.Calls.Single(c => c.OperationLocation.Signature == "Lu").IsStop);
    }

    [TestMethod]
    public void EveryTrainStopsAtAShadowStation()
    {
        var plan = SimplePlan();
        // Eslöv exchanges neither passengers nor cargo, but is a shadow station, so every train stops.
        var eslöv = (Station)Location(plan, "E");
        eslöv.HasCargoExchange = false;
        eslöv.HasPassengerExchange = false;
        eslöv.IsShadow = true;

        var train = plan.Create(Freight, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.IsTrue(train.Calls.Single(c => c.OperationLocation.Signature == "E").IsStop);
    }

    [TestMethod]
    public void StoresTheGivenMaxSpeedOnTheTrain()
    {
        var plan = SimplePlan();

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, maxSpeed: 60);

        Assert.IsNotNull(train);
        Assert.AreEqual(60, train.MaxSpeed);
    }

    [TestMethod]
    public void ASlowerTrainTakesLongerToReachTheTerminus()
    {
        var plan = SimplePlan();

        var slow = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, maxSpeed: 30);
        var fast = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, maxSpeed: 200);

        Assert.IsNotNull(slow);
        Assert.IsNotNull(fast);
        Assert.IsTrue(slow.Calls[^1].Arrival > fast.Calls[^1].Arrival,
            "A slower train should arrive at the terminus later than a faster one.");
    }

    [TestMethod]
    public void ReturnsNullWhenOriginEqualsDestination()
    {
        var plan = SimplePlan();
        var malmö = Location(plan, "M2");

        var train = plan.Create(Passenger, malmö, malmö, Start);

        Assert.IsNull(train);
        Assert.AreEqual(0, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void SignalControlledLocationIsAlwaysPassedThrough()
    {
        var plan = TestLayoutFactory.CreatePlan();
        // Lysekil → Malmö runs through Kristineberg (Kbg), a signal-controlled location.
        var train = plan.Create(Passenger, plan.Layout.OperationLocations.First(l => l.Signature == "Lys"),
            plan.Layout.OperationLocations.First(l => l.Signature == "M"), Start);

        Assert.IsNotNull(train);
        var kbg = train.Calls.Single(c => c.OperationLocation.Signature == "Kbg");
        Assert.IsTrue(kbg.IsPassthrough);
        Assert.IsFalse(kbg.IsArrival || kbg.IsDeparture);
    }

    [TestMethod]
    public void DwellAtADirectionChangeAllowsForTheLocoRunaround()
    {
        var plan = TestLayoutFactory.CreatePlan();
        // Malmö → Växjö reverses at Munkeröd (Mkd), the only station on the route that permits it.
        var train = plan.Create(Freight, plan.Layout.OperationLocations.First(l => l.Signature == "M"),
            plan.Layout.OperationLocations.First(l => l.Signature == "Vö"), Start);

        Assert.IsNotNull(train);
        var reversal = train.Calls.Single(c => c.OperationLocation.Signature == "Mkd");
        var settings = plan.Layout.Settings.TimeAndSpeed;
        var runaroundFastMinutes = (settings.StationTimings.LocoRunaroundRealMinutes ?? 5) * settings.FastClockSpeed;
        Assert.AreEqual(runaroundFastMinutes, (int)(reversal.Departure.Value - reversal.Arrival.Value).TotalMinutes);
    }
}
