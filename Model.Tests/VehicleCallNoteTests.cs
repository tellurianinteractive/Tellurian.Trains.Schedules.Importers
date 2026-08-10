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

    // A plan holding one train, worked end to end by one locomotive's schedule whose traction options
    // the test sets.
    private static (Plan Plan, StationCall Arrival, StationCall Departure) Arrange(Action<TractionOptions> options)
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
        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 42, null), Sessions.All);

        return (plan, part.To, part.From);
    }

    [TestMethod]
    public void CirculatingReachesBothTheDriverAndTheDispatcher()
    {
        // The whole point of the note: the driver makes the move and the dispatcher has to give it the
        // road, so it is no use to either of them alone.
        var (plan, arrival, _) = Arrange(o => o.ReverseLoco = true);

        Assert.ContainsSingle(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<CirculateNote>());
        Assert.ContainsSingle(arrival.StationNotes(Sessions.All, Settings, plan).OfType<CirculateNote>());
    }

    [TestMethod]
    public void CirculatingIsAnArrivalNote()
    {
        var (plan, arrival, departure) = Arrange(o => o.ReverseLoco = true);

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
        var (plan, arrival, _) = Arrange(o => { o.TurnLoco = true; o.ReverseLoco = true; });

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
    public void TheRestOfTheVehicleFamilyReachesTheReadersToo()
    {
        // The notes were generated on the train part all along; nothing read them. These two are the
        // ends of the same journey: fetched from parking before leaving, driven back after arriving.
        var (plan, arrival, departure) = Arrange(o => { o.ToParking = true; o.FromParking = true; });

        Assert.ContainsSingle(arrival.StationNotes(Sessions.All, Settings, plan).OfType<ToParkingNote>());
        Assert.ContainsSingle(departure.DriverNotes(Sessions.All, Settings, plan).OfType<FromParkingNote>());
    }

    [TestMethod]
    public void WithoutAPlanThereAreNoVehicleNotes()
    {
        // A timetable is readable before a single vehicle has been scheduled; there is then simply
        // nothing to say about the vehicles.
        var (_, arrival, _) = Arrange(o => o.ReverseLoco = true);

        Assert.IsEmpty(arrival.DriverNotes(Sessions.All, Settings).OfType<CirculateNote>());
        Assert.IsEmpty(arrival.VehicleNotes(null));
    }

    [TestMethod]
    public void ThePartsPersistedNotesAreNotCountedTwice()
    {
        // A call assembles its persisted notes itself, so the part must contribute only what it
        // generates — otherwise every hand-written note at a scheduled call prints twice.
        var (plan, arrival, _) = Arrange(o => o.ReverseLoco = true);
        arrival.Notes.Add(new TextCallNote("Ring the dispatcher.", "en") { IsForArrival = true });

        Assert.ContainsSingle(arrival.DriverNotes(Sessions.All, Settings, plan).OfType<TextCallNote>());
    }
}
