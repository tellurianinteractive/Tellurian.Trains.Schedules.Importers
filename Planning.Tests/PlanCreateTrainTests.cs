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

    private static StationCall Call(Train train, string signature) =>
        train.Calls.Single(c => c.OperationLocation.Signature == signature);

    /// <summary>The run time in minutes from the departure at one location to the arrival at the next.</summary>
    private static int LegMinutes(Train train, string fromSignature, string toSignature) =>
        (int)(Call(train, toSignature).Arrival.Value - Call(train, fromSignature).Departure.Value).TotalMinutes;

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
        // M2 → Hm runs upward (Start → End of each stretch), so the passenger train takes the next free
        // even number from the category's start number of 1: 2.
        Assert.AreEqual(2, train.Number);
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
    public void EachEndOfALegWhereTheTrainStandsStillAddsAMinuteToItsRunTime()
    {
        var plan = SimplePlan();

        // The passenger train stands at its origin and terminus and stops at both stations in between, so
        // every leg gets a minute at each end: running times of 3, 3 and 4 minutes become 5, 5 and 6.
        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.AreEqual(5, LegMinutes(train, "M2", "Lu"));
        Assert.AreEqual(5, LegMinutes(train, "Lu", "E"));
        Assert.AreEqual(6, LegMinutes(train, "E", "Hm"));
    }

    [TestMethod]
    public void ALegIntoOrOutOfAPassThroughGetsNoAllowanceThere()
    {
        var plan = SimplePlan();
        // Without passenger exchange at Eslöv the passenger train cannot stop there, so it runs through at
        // speed: the leg in and the leg out each lose the minute the stop would have cost them.
        Location(plan, "E").HasPassengerExchange = false;

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.IsTrue(Call(train, "E").IsPassthrough, "Precondition: the train passes Eslöv.");
        Assert.AreEqual(5, LegMinutes(train, "M2", "Lu"), "Lund is still a stop, and the origin still stands.");
        Assert.AreEqual(4, LegMinutes(train, "Lu", "E"), "Only the departure from Lund adds a minute.");
        Assert.AreEqual(5, LegMinutes(train, "E", "Hm"), "Only the arrival at the terminus adds a minute.");
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

    [TestMethod]
    public void AnUpwardTrainGetsTheNextEvenNumberAndADownwardTrainTheNextOddNumber()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        var hm = Location(plan, "Hm");

        var upward = plan.Create(Passenger, m2, hm, Start);      // M2 → Hm runs upward → even
        var downward = plan.Create(Passenger, hm, m2, Start);    // Hm → M2 runs downward → odd

        Assert.IsNotNull(upward);
        Assert.IsNotNull(downward);
        Assert.AreEqual(2, upward.Number);
        Assert.AreEqual(1, downward.Number);
    }

    [TestMethod]
    public void ConsecutiveTrainsInTheSameDirectionAndCategoryTakeSuccessiveSameParityNumbers()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        var hm = Location(plan, "Hm");

        var first = plan.Create(Passenger, m2, hm, Start);
        var second = plan.Create(Passenger, m2, hm, Start);

        Assert.IsNotNull(first);
        Assert.IsNotNull(second);
        Assert.AreEqual(2, first.Number);
        Assert.AreEqual(4, second.Number);
    }

    [TestMethod]
    public void NumberingStartsFromTheCategoryStartNumber()
    {
        var plan = SimplePlan();
        var category = new TrainCategory { Id = 3, Name = "Express", Prefix = "X", IsPassenger = true, DefaultSpeed = 100, StartNumber = 100 };
        var m2 = Location(plan, "M2");
        var hm = Location(plan, "Hm");

        var upward = plan.Create(category, m2, hm, Start);       // even, from 100 → 100
        var downward = plan.Create(category, hm, m2, Start);     // odd, from 100 → 101

        Assert.IsNotNull(upward);
        Assert.IsNotNull(downward);
        Assert.AreEqual(100, upward.Number);
        Assert.AreEqual(101, downward.Number);
    }

    [TestMethod]
    public void EachCategoryIsNumberedIndependently()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        var hm = Location(plan, "Hm");

        var passenger = plan.Create(Passenger, m2, hm, Start);
        var freight = plan.Create(Freight, m2, hm, Start);

        Assert.IsNotNull(passenger);
        Assert.IsNotNull(freight);
        // Both are the first upward train of their own category, so both take number 2.
        Assert.AreEqual(2, passenger.Number);
        Assert.AreEqual(2, freight.Number);
    }

    [TestMethod]
    public void AnExplicitNumberOverridesTheDirectionDefault()
    {
        var plan = SimplePlan();

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start, number: 4321);

        Assert.IsNotNull(train);
        Assert.AreEqual(4321, train.Number);
    }

    [TestMethod]
    public void APassengerTrainIsPutOnTheTrackWithAPlatform()
    {
        var plan = SimplePlan();
        // Track 2 is the only one anywhere with a platform, so it is where the passengers are.
        foreach (var location in plan.Layout.OperationLocations) location["2"].PlatformLength = 4.5;

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.IsTrue(train.Calls.All(call => call.Track.Number == "2"));
    }

    [TestMethod]
    public void ATrainThatCarriesNoPassengersIgnoresThePlatforms()
    {
        var plan = SimplePlan();
        foreach (var location in plan.Layout.OperationLocations) location["2"].PlatformLength = 4.5;

        var train = plan.Create(Freight, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.IsTrue(train.Calls.All(call => call.Track.Number == "1"));
    }

    [TestMethod]
    public void APassengerTrainTakesTheMainTrackWhereTheLocationHasNoPlatform()
    {
        var plan = SimplePlan();
        // Only Eslöv has a platform; everywhere else the train stands where any other train would.
        Location(plan, "E")["2"].PlatformLength = 4.5;

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        CollectionAssert.AreEqual(
            new[] { "1", "1", "2", "1" },
            train.Calls.Select(call => call.Track.Number).ToArray());
    }

    [TestMethod]
    public void ANewTrainIsPutOnTheTrackNamedForTheWayItRunsThroughTheLocation()
    {
        var plan = SimplePlan();
        var lund = Location(plan, "Lu");
        lund["3"].PreviousLocationId = Location(plan, "M2").Id;
        lund["3"].NextLocationId = Location(plan, "E").Id;

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.AreEqual("3", TrackAt(train, "Lu"));
        // Nothing is said about the other locations, so the train stands where it always would.
        Assert.IsTrue(train.Calls.Where(call => call.OperationLocation.Signature != "Lu")
            .All(call => call.Track.Number == "1"));
    }

    [TestMethod]
    public void EachDirectionOfADoubleLineGetsItsOwnTrack()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        var hm = Location(plan, "Hm");
        var lund = Location(plan, "Lu");
        lund["1"].PreviousLocationId = m2.Id;
        lund["1"].NextLocationId = Location(plan, "E").Id;
        lund["2"].PreviousLocationId = Location(plan, "E").Id;
        lund["2"].NextLocationId = m2.Id;

        var upward = plan.Create(Passenger, m2, hm, Start);
        var downward = plan.Create(Passenger, hm, m2, Start);

        Assert.IsNotNull(upward);
        Assert.IsNotNull(downward);
        Assert.AreEqual("1", TrackAt(upward, "Lu"));
        Assert.AreEqual("2", TrackAt(downward, "Lu"));
    }

    [TestMethod]
    public void ATrackForBothDirectionsTakesTheTrainWhicheverWayItRuns()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        var hm = Location(plan, "Hm");
        var lund = Location(plan, "Lu");
        lund["3"].PreviousLocationId = m2.Id;
        lund["3"].NextLocationId = Location(plan, "E").Id;
        lund["3"].AppliesInBothDirections = true;

        var upward = plan.Create(Passenger, m2, hm, Start);
        var downward = plan.Create(Passenger, hm, m2, Start);

        Assert.IsNotNull(upward);
        Assert.IsNotNull(downward);
        Assert.AreEqual("3", TrackAt(upward, "Lu"));
        Assert.AreEqual("3", TrackAt(downward, "Lu"));
    }

    [TestMethod]
    public void ATrainRunningTheOtherWayIsNotPutOnATrackNamedForOneDirectionOnly()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        var lund = Location(plan, "Lu");
        lund["1"].PreviousLocationId = m2.Id;
        lund["1"].NextLocationId = Location(plan, "E").Id;

        var downward = plan.Create(Passenger, Location(plan, "Hm"), m2, Start);

        Assert.IsNotNull(downward);
        Assert.AreNotEqual("1", TrackAt(downward, "Lu"));
    }

    [TestMethod]
    public void ThePlatformDecidesBetweenTracksNamedForTheSameRoute()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        var hm = Location(plan, "Hm");
        var lund = Location(plan, "Lu");
        foreach (var track in new[] { lund["2"], lund["3"] })
        {
            track.PreviousLocationId = m2.Id;
            track.NextLocationId = Location(plan, "E").Id;
        }
        lund["3"].PlatformLength = 4.5;

        var passenger = plan.Create(Passenger, m2, hm, Start);
        var freight = plan.Create(Freight, m2, hm, Start);

        Assert.IsNotNull(passenger);
        Assert.IsNotNull(freight);
        // Both trains are named for track 2 and 3 alike; the passenger train stops to exchange
        // passengers and takes the platform, the freight train has none and takes the main track.
        Assert.AreEqual("3", TrackAt(passenger, "Lu"));
        Assert.AreEqual("2", TrackAt(freight, "Lu"));
    }

    [TestMethod]
    public void ATrainStartingAtALocationTakesTheTrackNamedForWhereItGoesOnTo()
    {
        var plan = SimplePlan();
        var m2 = Location(plan, "M2");
        // Track 4 is for trains between Kävlinge and Lund, track 3 for trains starting here for Lund.
        m2["4"].PreviousLocationId = Location(plan, "Kä").Id;
        m2["4"].NextLocationId = Location(plan, "Lu").Id;
        m2["3"].NextLocationId = Location(plan, "Lu").Id;

        var train = plan.Create(Passenger, m2, Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        Assert.AreEqual("3", TrackAt(train, "M2"));
    }

    private static string TrackAt(Train train, string signature) =>
        train.Calls.Single(call => call.OperationLocation.Signature == signature).Track.Number;

    [TestMethod]
    public void APassengerTrainMeetingWhereNoPassengersAreExchangedNeedsNoPlatform()
    {
        var plan = SimplePlan();
        foreach (var location in plan.Layout.OperationLocations) location["2"].PlatformLength = 4.5;
        // Eslöv exchanges no passengers, so its platform serves none and the train is simply put on the
        // main track — which is exactly where a meet there would put it.
        var eslöv = Location(plan, "E");
        eslöv.HasPassengerExchange = false;

        var train = plan.Create(Passenger, Location(plan, "M2"), Location(plan, "Hm"), Start);

        Assert.IsNotNull(train);
        CollectionAssert.AreEqual(
            new[] { "2", "2", "1", "2" },
            train.Calls.Select(call => call.Track.Number).ToArray());
    }
}
