using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the two rules that decide a call's stop flags: the location says whether the train can stop
/// there at all, and the train parts say where it must. See <c>StopRules</c>.
/// </summary>
[TestClass]
public class StopRulesTests
{
    private static readonly TrainCategory Passenger = new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true };
    private static readonly TrainCategory Freight = new() { Id = 2, Name = "Freight", Prefix = "G", IsFreight = true };
    private static readonly TrainCategory Mixed = new() { Id = 3, Name = "Mixed", Prefix = "PG", IsPassenger = true, IsFreight = true };
    private static readonly TrainCategory Uncategorised = new() { Id = 4, Name = "Empty stock", Prefix = "T" };

    private static Layout Layout => new() { Id = 1, Name = "Test" };

    // A train over three locations, calling at each of them and stopping everywhere.
    private static Train CreateTrain(TrainCategory? category, params OperationLocation[] locations)
    {
        var train = new Train(1, 1000) { Category = category, CategoryId = category?.Id };
        var time = Time.FromHourAndMinute(8, 0);
        for (var i = 0; i < locations.Length; i++)
        {
            var track = new StationTrack(i + 1, "1");
            locations[i].Add(track);
            // The flags are set after the call joins the train: Train.Add makes the first call added a
            // departure only, and this train is meant to stop everywhere.
            var call = train.Add(new StationCall(i + 1, track, time, time.AddMinutes(2)));
            call.IsArrival = true;
            call.IsDeparture = true;
            time = time.AddMinutes(20);
        }
        return train;
    }

    private static Plan CreatePlan(Layout layout, Train train)
    {
        var timetable = new Timetable("Test", layout);
        timetable.Add(train);
        return new Plan("Test", timetable);
    }

    // ---- What the location allows ---------------------------------------------------------------

    [TestMethod]
    public void APassengerTrainCannotStopWhereThereIsNoPassengerExchange()
    {
        var station = new Station(1, "Ytterby", "Yb") { HasPassengerExchange = false };
        var train = CreateTrain(Passenger, station);

        Assert.IsFalse(train.CanStopAt(station));
        Assert.IsFalse(train.Calls[0].IsStop, "The flags stand, but the call is a pass-through all the same.");
        Assert.IsTrue(train.Calls[0].IsArrival, "The flags themselves are never cleared.");
    }

    [TestMethod]
    public void AFreightTrainCannotStopWhereThereIsNoCargoExchange()
    {
        var station = new Station(1, "Ytterby", "Yb") { HasCargoExchange = false };
        var train = CreateTrain(Freight, station);

        Assert.IsFalse(train.CanStopAt(station));
        Assert.IsFalse(train.Calls[0].IsStop);
    }

    [TestMethod]
    public void AFreightTrainStopsWhereOnlyCargoIsExchanged()
    {
        var station = new Station(1, "Ytterby", "Yb") { HasPassengerExchange = false };
        var train = CreateTrain(Freight, station);

        Assert.IsTrue(train.CanStopAt(station));
        Assert.IsTrue(train.Calls[0].IsStop);
    }

    [TestMethod]
    public void AMixedTrainStopsWhereEitherIsExchanged()
    {
        var passengersOnly = new Station(1, "Ytterby", "Yb") { HasCargoExchange = false };
        var cargoOnly = new Station(2, "Kode", "Kd") { HasPassengerExchange = false };
        var train = CreateTrain(Mixed, passengersOnly, cargoOnly);

        Assert.IsTrue(train.CanStopAt(passengersOnly));
        Assert.IsTrue(train.CanStopAt(cargoOnly));
    }

    [TestMethod]
    public void ATrainCarryingNeitherIsRestrictedByTheLocationTypeAlone()
    {
        // Empty stock and light engines exchange nothing, so no exchange is demanded of the location.
        var station = new Station(1, "Ytterby", "Yb") { HasPassengerExchange = false, HasCargoExchange = false };
        var train = CreateTrain(Uncategorised, station);

        Assert.IsTrue(train.CanStopAt(station));
        Assert.IsTrue(train.Calls[0].IsStop);
    }

    [TestMethod]
    public void NoTrainCanStopAtASignalControlledLocation()
    {
        var block = new SignalControlledLocation(1, "Björkås", "Bkå");
        var train = CreateTrain(Uncategorised, block);

        Assert.IsFalse(train.CanStopAt(block));
        Assert.IsFalse(train.Calls[0].IsStop);
    }

    [TestMethod]
    public void APassengerTrainCannotStopAtAnIndustrialArea()
    {
        var industry = new IndustrialArea(1, "Sockerbruket", "Sbr");
        var train = CreateTrain(Passenger, industry);

        Assert.IsFalse(train.CanStopAt(industry));
    }

    [TestMethod]
    public void AShadowYardExchangesBothWhateverIsConfigured()
    {
        var shadow = new Station(1, "Mohult", "Moh") { IsShadow = true, HasPassengerExchange = false, HasCargoExchange = false };

        Assert.IsTrue(shadow.HasPassengerExchange, "A shadow yard stands for everything beyond the layout.");
        Assert.IsTrue(shadow.HasCargoExchange);
        Assert.IsTrue(CreateTrain(Passenger, shadow).CanStopAt(shadow));
        Assert.IsTrue(CreateTrain(Freight, new Station(2, "Kode", "Kd")).CanStopAt(shadow));
    }

    [TestMethod]
    public void RestoringTheExchangeBringsThePlannedStopBack()
    {
        var station = new Station(1, "Ytterby", "Yb");
        var train = CreateTrain(Passenger, station);
        Assert.IsTrue(train.Calls[0].IsStop);

        station.HasPassengerExchange = false;
        Assert.IsFalse(train.Calls[0].IsStop);

        station.HasPassengerExchange = true;
        Assert.IsTrue(train.Calls[0].IsStop, "Nothing was cleared, so the planned stop is still there.");
    }

    // ---- What the train parts require -----------------------------------------------------------

    [TestMethod]
    public void TheTrainsOwnEndsRequireTheirFlags()
    {
        var layout = Layout;
        var origin = AddStation(layout, 1, "Ytterby", "Yb");
        var middle = AddStation(layout, 2, "Kode", "Kd");
        var destination = AddStation(layout, 3, "Stora Höga", "Sth");
        var train = CreateTrain(Passenger, origin, middle, destination);
        var plan = CreatePlan(layout, train);

        Assert.IsTrue(plan.IsDepartureRequired(train.Calls[0]));
        Assert.IsFalse(plan.IsArrivalRequired(train.Calls[0]), "Nothing arrives at the origin.");
        Assert.IsTrue(plan.IsArrivalRequired(train.Calls[2]));
        Assert.IsFalse(plan.IsDepartureRequired(train.Calls[2]), "Nothing departs the destination.");
        Assert.IsFalse(plan.IsDepartureRequired(train.Calls[1]), "An intermediate stop is the planner's to make or drop.");
        Assert.IsFalse(plan.IsArrivalRequired(train.Calls[1]));
    }

    [TestMethod]
    public void ACallADriverDutyStartsFromRequiresItsDeparture()
    {
        var layout = Layout;
        var origin = AddStation(layout, 1, "Ytterby", "Yb");
        var middle = AddStation(layout, 2, "Kode", "Kd");
        var destination = AddStation(layout, 3, "Stora Höga", "Sth");
        var train = CreateTrain(Passenger, origin, middle, destination);
        var plan = CreatePlan(layout, train);
        var duty = new DriverDuty(1, "1") { Plan = plan };
        duty.Parts.Add(new ScheduledTrainPart(train.Calls[1], train.Calls[2]) { Id = 1 });
        plan.DriverDuties.Add(duty);

        Assert.IsTrue(plan.IsDepartureRequired(train.Calls[1]), "The duty takes the driver from here.");
        Assert.IsFalse(plan.IsArrivalRequired(train.Calls[1]), "Nothing ends here.");
    }

    [TestMethod]
    public void ApplyStopRulesSetsWhatThePartsNeedAndClearsNothing()
    {
        var layout = Layout;
        var origin = AddStation(layout, 1, "Ytterby", "Yb");
        var middle = AddStation(layout, 2, "Kode", "Kd");
        var destination = AddStation(layout, 3, "Stora Höga", "Sth");
        middle.HasPassengerExchange = false;
        var train = CreateTrain(Passenger, origin, middle, destination);
        var plan = CreatePlan(layout, train);
        // A plan from an earlier version, with the flags its parts depend on missing.
        foreach (var call in train.Calls) { call.IsArrival = false; call.IsDeparture = false; }
        train.Calls[1].IsArrival = true;

        var changed = plan.ApplyStopRules();

        Assert.AreEqual(2, changed, "Only the two end calls needed putting right.");
        Assert.IsTrue(train.Calls[0].IsDeparture);
        Assert.IsTrue(train.Calls[2].IsArrival);
        Assert.IsTrue(train.Calls[1].IsArrival, "A flag is never cleared, even where the train cannot stop.");
        Assert.IsFalse(train.Calls[1].IsStop, "It is a pass-through all the same.");
        Assert.AreEqual(0, plan.ApplyStopRules(), "Idempotent.");
    }

    [TestMethod]
    public void ApplyStopRulesReadsTheEndsInRunOrderNotInsertionOrder()
    {
        var layout = Layout;
        var first = AddStation(layout, 1, "Ytterby", "Yb");
        var last = AddStation(layout, 2, "Kode", "Kd");
        var train = CreateTrain(Passenger, first, last);
        // A call added last but timed first: the run order, not the insertion order, decides the ends.
        var early = AddStation(layout, 3, "Stora Höga", "Sth");
        var track = new StationTrack(9, "1");
        early.Add(track);
        train.Add(new StationCall(9, track, Time.FromHourAndMinute(7, 0), Time.FromHourAndMinute(7, 2)));
        var plan = CreatePlan(layout, train);

        plan.ApplyStopRules();

        Assert.IsTrue(train.Calls[2].IsDeparture, "The 07:00 call is where the train starts.");
        Assert.IsFalse(plan.IsDepartureRequired(train.Calls[0]), "The 08:00 call is no longer the origin.");
    }

    private static Station AddStation(Layout layout, int id, string name, string signature)
    {
        var station = new Station(id, name, signature);
        layout.OperationLocations.Add(station);
        return station;
    }
}
