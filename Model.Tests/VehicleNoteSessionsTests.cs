using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies what a vehicle schedule says at the two calls that bound one of its parts — which traction
/// unit to use, what to couple and uncouple, what to do with the traction so the train can leave the
/// other way — and that each of those notes is qualified by the sessions the vehicle actually works the
/// train on, rather than being read as holding every time the train comes.
/// </summary>
/// <remarks>
/// The notes reach the reader through <c>DriverNotes</c> and <c>StationNotes</c>, which is how they are
/// asked for here: a note the model derives but no reader is given is not a note. The tracks the vehicles
/// are fetched from and put on have their own suite in <c>OtherTrackTests</c>.
/// </remarks>
[TestClass]
public class VehicleNoteSessionsTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // Forward 12:00 G → Yb → Snu 12:55, worked end to end by one vehicle with the part's default options:
    // couple it where the part starts and uncouple it where it ends. The train runs on every session and
    // the vehicle is assigned for all of them, unless the test says otherwise.
    private static (Plan Plan, ScheduledTrainPart Part, ScheduledObject Vehicle) Arrange(
        ScheduledObjectType vehicleType = ScheduledObjectType.Locomotive,
        Sessions? trainRuns = null,
        Sessions? vehicleAssigned = null)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        var plan = Plan.Create("Test", timetable);
        Forward(plan).Sessions = trainRuns ?? Sessions.All;

        var schedule = plan.CreateSchedule();
        var part = schedule.Append(Forward(plan).AsTrainPart).Value;
        if (vehicleType == ScheduledObjectType.Wagonset) part.WagonSetOptions = new WagonSetOptions();
        else part.TractionOptions = new TractionOptions();

        var vehicle = plan.CreateVehicle(vehicleType, "T44", 42, null);
        plan.AssignVehicle(schedule, vehicle, vehicleAssigned ?? Sessions.All);
        return (plan, part, vehicle);
    }

    private static Train Forward(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 1);

    private static StationTrack TrackAt(Plan plan, string signature, string number) =>
        plan.Layout.OperationLocations.Single(l => l.Signature == signature)[number];

    private static IEnumerable<ICallNote> DepartureNotes(Plan plan, ScheduledTrainPart part) =>
        part.From.DriverNotes(Sessions.All, Settings, plan);

    private static IEnumerable<ICallNote> ArrivalNotes(Plan plan, ScheduledTrainPart part) =>
        part.To.DriverNotes(Sessions.All, Settings, plan);

    [TestMethod]
    public void AWagonsetIsUncoupledWhereItsPartEnds()
    {
        // The mirror of the couple note at the part's start, and until now the one half that was derived
        // from the options but never said: HasUncoupleNote was read for traction only.
        var (plan, part, wagonset) = Arrange(ScheduledObjectType.Wagonset);

        var note = ArrivalNotes(plan, part).OfType<UncoupleNote>().Single();

        Assert.AreEqual(wagonset, note.ScheduledObject);
        Assert.IsTrue(note.IsForArrival);
        Assert.AreEqual($"Uncouple {wagonset} from train.", note.ToText);
    }

    [TestMethod]
    public void AWagonsetThatStaysOnTheTrainIsNotUncoupled()
    {
        var (plan, part, _) = Arrange(ScheduledObjectType.Wagonset);
        part.WagonSetOptions!.HasUncoupleNote = false;

        Assert.IsEmpty(ArrivalNotes(plan, part).OfType<UncoupleNote>());
    }

    [TestMethod]
    public void AWagonsetPutOnAnotherTrackIsNotAlsoUncoupled()
    {
        // Putting it on another track takes it off the train, so the ToTrackNote says all the uncouple
        // note would — as it already does for traction.
        var (plan, part, _) = Arrange(ScheduledObjectType.Wagonset);
        part.ToTrack = TrackAt(plan, "Snu", "2");

        Assert.IsEmpty(ArrivalNotes(plan, part).OfType<UncoupleNote>());
        // For a wagonset the track is the dispatcher's to read; the driver is told to shunt instead.
        Assert.ContainsSingle(part.To.StationNotes(Forward(plan).Sessions, Settings, plan).OfType<ToTrackNote>());
    }

    [TestMethod]
    public void ACoupleNoteLeadsWithTheSessionsTheVehicleWorks()
    {
        var (plan, part, vehicle) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5));

        var note = DepartureNotes(plan, part).OfType<CoupleNote>().Single();

        Assert.AreEqual("1,3,5", note.Sessions?.ToText(Settings));
        Assert.AreEqual($"1,3,5: Couple {vehicle} to train.", note.ToText);
    }

    [TestMethod]
    public void AnUncoupleNoteLeadsWithTheSessionsTheVehicleWorks()
    {
        var (plan, part, vehicle) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5));

        var note = ArrivalNotes(plan, part).OfType<UncoupleNote>().Single();

        Assert.AreEqual($"1,3,5: Uncouple {vehicle} from train.", note.ToText);
    }

    [TestMethod]
    public void TheParkingNotesLeadWithTheSessionsTheVehicleWorks()
    {
        var (plan, part, vehicle) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(2, 4));
        part.TractionOptions!.FromLayover = TractionLayover.Stabling;
        part.TractionOptions.ToLayover = TractionLayover.Stabling;

        var departure = DepartureNotes(plan, part).OfType<FromParkingNote>().Single();
        var arrival = ArrivalNotes(plan, part).OfType<ToParkingNote>().Single();

        StringAssert.StartsWith(departure.ToText, "2,4: Before departure, drive");
        StringAssert.StartsWith(arrival.ToText, "2,4: After arrival, drive");
        Assert.AreEqual(vehicle, departure.ScheduledObject);
    }

    [TestMethod]
    public void AVehicleWorkingEverySessionTheTrainRunsNamesNoSessions()
    {
        // Assigned for more than the train runs: on every session there is, it is there — saying so would
        // only repeat the sessions column.
        var (plan, part, vehicle) = Arrange(
            trainRuns: Sessions.FromSessionNumbers(1, 3, 5),
            vehicleAssigned: Sessions.FromSessionNumbers(1, 2, 3, 4, 5));

        var note = DepartureNotes(plan, part).OfType<CoupleNote>().Single();

        Assert.IsNull(note.Sessions);
        Assert.AreEqual($"Couple {vehicle} to train.", note.ToText);
    }

    [TestMethod]
    public void OnlyTheSessionsTheTrainAlsoRunsAreNamed()
    {
        var (plan, part, _) = Arrange(
            trainRuns: Sessions.FromSessionNumbers(1, 3, 5, 7),
            vehicleAssigned: Sessions.FromSessionNumbers(1, 2, 3, 4));

        var note = DepartureNotes(plan, part).OfType<CoupleNote>().Single();

        Assert.AreEqual("1,3", note.Sessions?.ToText(Settings));
    }

    [TestMethod]
    public void AVehicleNotWorkingAnySessionTheTrainRunsGetsNoNote()
    {
        var (plan, part, _) = Arrange(
            trainRuns: Sessions.FromSessionNumbers(1, 3, 5),
            vehicleAssigned: Sessions.FromSessionNumbers(2, 4, 6));

        Assert.IsEmpty(DepartureNotes(plan, part).OfType<CoupleNote>());
        Assert.IsEmpty(ArrivalNotes(plan, part).OfType<UncoupleNote>());
    }

    [TestMethod]
    public void LocomotivesTakingTurnsEachNameTheirOwnSessions()
    {
        var (plan, part, first) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5));
        var second = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 43, null);
        plan.AssignVehicle(part.Schedule!, second, Sessions.FromSessionNumbers(2, 4, 6));

        var notes = DepartureNotes(plan, part).OfType<CoupleNote>().ToList();

        Assert.HasCount(2, notes);
        Assert.AreEqual("1,3,5", notes.Single(n => n.ScheduledObject == first).Sessions?.ToText(Settings));
        Assert.AreEqual("2,4,6", notes.Single(n => n.ScheduledObject == second).Sessions?.ToText(Settings));
    }

    [TestMethod]
    public void TurningIsQualifiedByTheSessionsOfAllTheTractionTogether()
    {
        // The note names no traction unit — the whole consist turns once — so it holds on every session
        // any of them is there, not on one unit's.
        var (plan, part, _) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(1, 3));
        var second = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 43, null);
        plan.AssignVehicle(part.Schedule!, second, Sessions.FromSessionNumbers(5, 7));
        part.TractionOptions!.TurnLoco = true;

        var note = ArrivalNotes(plan, part).OfType<TurnNote>().Single();

        Assert.AreEqual("1,3,5,7", note.Sessions?.ToText(Settings));
    }

    [TestMethod]
    public void TurningOnEverySessionTheTrainRunsNamesNoSessions()
    {
        var (plan, part, _) = Arrange();
        part.TractionOptions!.TurnLoco = true;

        var note = ArrivalNotes(plan, part).OfType<TurnNote>().Single();

        Assert.IsNull(note.Sessions);
        Assert.AreEqual("Turn locomotive.", note.ToText);
    }

    [TestMethod]
    public void TurningStandsUnqualifiedWhereNoVehicleWorksThePartYet()
    {
        // What the planner asked for holds until the vehicles say otherwise, and there is then nothing
        // known to qualify it by — an empty session qualifier would read as "on no session at all".
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        var plan = Plan.Create("Test", timetable);
        var part = plan.CreateSchedule().Append(Forward(plan).AsTrainPart).Value;
        part.TractionOptions = new TractionOptions { TurnLoco = true };

        var note = ArrivalNotes(plan, part).OfType<TurnNote>().Single();

        Assert.IsNull(note.Sessions);
    }

    [TestMethod]
    public void TheQualifiedNotesReachTheStationsDispatchList()
    {
        // The dispatch list is the reason these are qualified at all: the person on duty is there every
        // session, and has to see which of them an instruction is for.
        var (plan, part, vehicle) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5));
        var train = Forward(plan);

        var departure = part.From.StationNotes(train.Sessions, Settings, plan);
        var arrival = part.To.StationNotes(train.Sessions, Settings, plan);

        Assert.AreEqual($"1,3,5: Couple {vehicle} to train.", departure.OfType<CoupleNote>().Single().ToText);
        Assert.AreEqual($"1,3,5: Uncouple {vehicle} from train.", arrival.OfType<UncoupleNote>().Single().ToText);
    }

    [TestMethod]
    public void OneVehicleWorkedByTwoSchedulesIsStillSaidOnce()
    {
        // A locomotive and the wagonset it hauls give two parts over the same two calls, and a part is
        // equal to any part over those calls — so both resolve to both vehicles. Each vehicle's note is
        // built twice and must collapse, sessions and formatting included.
        var (plan, part, _) = Arrange(ScheduledObjectType.Locomotive, vehicleAssigned: Sessions.FromSessionNumbers(1, 3));
        var wagons = plan.CreateSchedule();
        var wagonPart = wagons.Append(Forward(plan).AsTrainPart).Value;
        wagonPart.WagonSetOptions = new WagonSetOptions();
        plan.AssignVehicle(wagons, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Bo", 7, null), Sessions.All);

        var notes = DepartureNotes(plan, part).OfType<CoupleNote>().ToList();

        Assert.HasCount(2, notes, "One note for the locomotive and one for the wagonset, neither repeated.");
    }
}
