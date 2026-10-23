using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that what a vehicle schedule says is done with its vehicles reaches the two people who have
/// to do it — the loco driver and the station's dispatcher — through the notes of the call it happens
/// at, and that the turning and circulating instructions say the right thing.
/// </summary>
[TestClass]
public class VehicleCallNoteTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // A plan holding one train, worked end to end by one traction unit's schedule whose traction options
    // the test sets. The unit is an ordinary locomotive unless the test says otherwise — one that has to
    // be run round to leave the way it came.
    private static (Plan Plan, StationCall Arrival, StationCall Departure) Arrange(
        Action<TractionOptions> options,
        ScheduledObjectType vehicleType = ScheduledObjectType.Locomotive,
        bool isReversibleTrain = false)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        var plan = Plan.Create("Test", timetable);

        var schedule = plan.CreateSchedule();
        schedule.Append(plan.Timetable.Trains.Single().AsTrainPart);
        var part = schedule.Parts.Single();
        part.TractionOptions = new TractionOptions();
        options(part.TractionOptions);

        // The notes that name a vehicle are written one per traction unit, so the schedule needs one.
        var vehicle = plan.CreateVehicle(vehicleType, "T44", 42, null);
        vehicle.IsReversibleTrain = isReversibleTrain;
        plan.AssignVehicle(schedule, vehicle, Sessions.All);

        return (plan, part.To, part.From);
    }

    [TestMethod]
    public void CirculatingReachesBothTheDriverAndTheDispatcher()
    {
        // The whole point of the note: the driver makes the move and the dispatcher has to give it the
        // road, so it is no use to either of them alone.
        var (plan, arrival, _) = Arrange(o => o.Runaround = RunaroundMode.Always);

        Assert.ContainsSingle(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<CirculateNote>());
        Assert.ContainsSingle(arrival.StationNotes(Sessions.All, Settings, plan).OfType<CirculateNote>());
    }

    [TestMethod]
    public void CirculatingIsAnArrivalNote()
    {
        var (plan, arrival, departure) = Arrange(o => o.Runaround = RunaroundMode.Always);

        var note = arrival.DriverNotes(Sessions.All, Settings, plan).OfType<CirculateNote>().Single();
        Assert.IsTrue(note.IsForArrival);
        Assert.IsFalse(note.IsForDeparture);
        Assert.IsEmpty(departure.DriverNotes(Sessions.All, Settings, plan).OfType<CirculateNote>(),
            "The move is made where the train arrives, not where it started.");
    }

    [TestMethod]
    public void TurningAndCirculatingTogetherGiveOneNote()
    {
        // One errand, not two: the loco leaves the train, goes to the turntable and comes back on the
        // other end. Two notes would read as two independent movements.
        var (plan, arrival, _) = Arrange(o => { o.TurnLoco = true; o.Runaround = RunaroundMode.Always; });

        var notes = arrival.DriverNotes(Sessions.All, Settings, plan);
        Assert.ContainsSingle(notes.OfType<TurnAndCirculateNote>());
        Assert.IsEmpty(notes.OfType<CirculateNote>());
        Assert.IsEmpty(notes.OfType<TurnNote>());
    }

    [TestMethod]
    public void TurningAloneGivesTheTurnNote()
    {
        var (plan, arrival, _) = Arrange(o => o.TurnLoco = true);

        Assert.ContainsSingle(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<TurnNote>());
    }

    [TestMethod]
    public void NeitherFlagGivesNoTurningNote()
    {
        var (plan, arrival, _) = Arrange(_ => { });

        var notes = arrival.DriverNotes(Sessions.All, Settings, plan);
        Assert.IsEmpty(notes.OfType<CirculateNote>());
        Assert.IsEmpty(notes.OfType<TurnNote>());
        Assert.IsEmpty(notes.OfType<TurnAndCirculateNote>());
    }

    [TestMethod]
    public void ATrainsetIsNotAskedToRunRound()
    {
        // A trainset is driven from either end as it stands, so there is nothing to run round: the flag
        // is kept as the planner set it, but it says nothing to anybody here.
        var (plan, arrival, _) = Arrange(o => o.Runaround = RunaroundMode.Always, ScheduledObjectType.Trainset);

        Assert.IsEmpty(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ALocomotiveOnAReversibleTrainIsNotAskedToRunRound()
    {
        // The far end has a cab, in a driving trailer or a second locomotive, so the driver walks to it
        // instead of the locomotive being moved.
        var (plan, arrival, _) = Arrange(o => o.Runaround = RunaroundMode.Always, isReversibleTrain: true);

        Assert.IsEmpty(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<CirculateNote>());
    }

    [TestMethod]
    public void ATrainsetAskedToTurnAndRunRoundGetsNoNote()
    {
        // A trainset has no loco to turn or run round, so nothing is printed.
        var (plan, arrival, _) = Arrange(
            o => { o.TurnLoco = true; o.Runaround = RunaroundMode.Always; }, ScheduledObjectType.Trainset);

        var notes = arrival.DriverNotes(Sessions.All, Settings, plan);
        Assert.IsEmpty(notes.OfType<TurnNote>());
        Assert.IsEmpty(notes.OfType<CirculateNote>());
        Assert.IsEmpty(notes.OfType<TurnAndCirculateNote>());
    }

    [TestMethod]
    public void ACallAsksForTurningAndCirculatingWithoutAnyPartDoingSo()
    {
        var (plan, arrival, departure) = Arrange(_ => { });
        departure.TurnLoco = true;
        departure.Runaround = RunaroundMode.Always;

        foreach (var notes in new[] { departure.DriverNotes(Sessions.All, Settings, plan), departure.StationNotes(Sessions.All, Settings, plan) })
        {
            var note = notes.OfType<TurnAndCirculateNote>().Single();
            Assert.IsTrue(note.IsForArrival);
            Assert.IsEmpty(notes.OfType<TurnNote>());
            Assert.IsEmpty(notes.OfType<CirculateNote>());
        }
        Assert.IsEmpty(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<TurnAndCirculateNote>());
    }

    [TestMethod]
    public void PushingWagonsGivesADepartureNoteForDriverAndDispatcher()
    {
        var (plan, _, departure) = Arrange(_ => { });
        departure.PushWagons = true;

        foreach (var notes in new[] { departure.DriverNotes(Sessions.All, Settings, plan), departure.StationNotes(Sessions.All, Settings, plan) })
        {
            var note = notes.OfType<PushWagonsNote>().Single();
            Assert.IsTrue(note.IsForDeparture);
            Assert.IsFalse(note.IsForArrival);
            Assert.AreEqual("Push wagons.", note.ToText);
        }
    }

    [TestMethod]
    public void PushingWagonsNamesTheDestinationsOfTheCargoFlowsAhead()
    {
        var (plan, arrival, departure) = Arrange(_ => { });
        var description = plan.Timetable.Add(new CargoFlowOptions { Destinations = { new Destination { Location = arrival.OperationLocation } } });
        plan.Timetable.Trains.Single().CreateCargoFlow(1, departure, arrival, description);
        departure.PushWagons = true;

        var note = departure.StationNotes(Sessions.All, Settings, plan).OfType<PushWagonsNote>().Single();
        Assert.StartsWith("Push wagons to ", note.ToText);
        Assert.Contains(arrival.OperationLocation.Name, note.ToText);
    }

    [TestMethod]
    public void PushingWagonsNamesWhereAWagonsetEndsItsPartWhenThereIsNoCargoFlow()
    {
        var (plan, arrival, departure) = Arrange(_ => { }, ScheduledObjectType.Wagonset);
        departure.PushWagons = true;

        var note = departure.DriverNotes(Sessions.All, Settings, plan).OfType<PushWagonsNote>().Single();
        Assert.AreEqual($"Push wagons to {arrival.OperationLocation.Name}", note.ToText);
    }

    [TestMethod]
    public void PushingWagonsNeedsNoPlan()
    {
        var (_, _, departure) = Arrange(_ => { });
        departure.PushWagons = true;

        Assert.ContainsSingle(departure.DriverNotes(Sessions.All, Settings).OfType<PushWagonsNote>());
    }

    [TestMethod]
    public void NothingIsPushedFromWhereTheTrainEnds()
    {
        var (plan, arrival, _) = Arrange(_ => { });
        arrival.PushWagons = true;

        Assert.IsEmpty(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<PushWagonsNote>());
        Assert.IsEmpty(arrival.StationNotes(Sessions.All, Settings, plan).OfType<PushWagonsNote>());
    }

    [TestMethod]
    public void ACallThatDoesNotPushGivesNoPushNote()
    {
        var (plan, _, departure) = Arrange(_ => { });

        Assert.IsEmpty(departure.DriverNotes(Sessions.All, Settings, plan).OfType<PushWagonsNote>());
    }

    [TestMethod]
    public void ACallFlagJoinsTheFlagOfAPartEndingThereInOneNote()
    {
        var (plan, arrival, _) = Arrange(o => o.TurnLoco = true);
        arrival.Runaround = RunaroundMode.Always;

        var notes = arrival.DriverNotes(Sessions.All, Settings, plan);
        Assert.ContainsSingle(notes.OfType<TurnAndCirculateNote>());
        Assert.IsEmpty(notes.OfType<TurnNote>());
    }

    [TestMethod]
    [DataRow(ScheduledObjectType.Trainset, false)]
    [DataRow(ScheduledObjectType.Locomotive, true)]
    public void ACallFlagIsNotPrintedForATrainsetOrAPushPullTrain(ScheduledObjectType type, bool reversible)
    {
        var (plan, arrival, _) = Arrange(_ => { }, type, reversible);
        arrival.TurnLoco = true;
        arrival.Runaround = RunaroundMode.Always;

        Assert.IsEmpty(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<GeneratedNote>()
            .Where(n => n is TurnNote or CirculateNote or TurnAndCirculateNote));
    }

    [TestMethod]
    public void TheRestOfTheVehicleFamilyReachesTheReadersToo()
    {
        // The notes were generated on the train part all along; nothing read them. These two are the
        // ends of the same journey: fetched from parking before leaving, driven back after arriving.
        var (plan, arrival, departure) = Arrange(o => { o.ToLayover = TractionLayover.Stabling; o.FromLayover = TractionLayover.Stabling; });

        Assert.ContainsSingle(arrival.StationNotes(Sessions.All, Settings, plan).OfType<ToParkingNote>());
        Assert.ContainsSingle(departure.DriverNotes(Sessions.All, Settings, plan).OfType<FromParkingNote>());
    }

    [TestMethod]
    public void WithoutAPlanThereAreNoVehicleNotes()
    {
        // A timetable is readable before a single vehicle has been scheduled; there is then simply
        // nothing to say about the vehicles.
        var (_, arrival, _) = Arrange(o => o.Runaround = RunaroundMode.Always);

        Assert.IsEmpty(arrival.DriverNotes(Sessions.All, Settings).OfType<CirculateNote>());
        Assert.IsEmpty(arrival.VehicleNotes(null));
    }

    [TestMethod]
    public void ThePartsPersistedNotesAreNotCountedTwice()
    {
        // A call assembles its persisted notes itself, so the part must contribute only what it
        // generates — otherwise every hand-written note at a scheduled call prints twice.
        var (plan, arrival, _) = Arrange(o => o.Runaround = RunaroundMode.Always);
        arrival.Notes.Add(new TextCallNote("Ring the dispatcher.", "en") { IsForArrival = true });

        Assert.ContainsSingle(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<TextCallNote>());
    }
}
