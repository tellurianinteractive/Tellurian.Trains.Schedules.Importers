using System.Globalization;
using System.Text.Json;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a train part can say its vehicles stand on another track than the train uses where the
/// part starts or ends, that this reaches the loco driver and the dispatcher as a note to fetch them from
/// it or put them on it, and that the track stays sound when the part, the plan or the track changes.
/// </summary>
[TestClass]
public class OtherTrackTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // Forward 12:00 G (track 3) → Yb → Snu (track 1) 12:55, worked end to end by one locomotive with the
    // part's default traction options: couple it at the start and uncouple it at the end.
    // The train runs on every session and the vehicle is assigned for all of them, unless the test says
    // otherwise.
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
    private static Train Return(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 2);

    private static StationTrack TrackAt(Plan plan, string signature, string number) =>
        plan.Layout.OperationLocations.Single(l => l.Signature == signature)[number];

    private static StationCall CallAt(Train train, string signature) =>
        train.Calls.First(c => c.OperationLocation.Signature == signature);

    private IEnumerable<ICallNote> DepartureNotes(Plan plan, ScheduledTrainPart part) =>
        part.From.DriverNotes(Sessions.All, Settings, plan);

    private IEnumerable<ICallNote> ArrivalNotes(Plan plan, ScheduledTrainPart part) =>
        part.To.DriverNotes(Sessions.All, Settings, plan);

    [TestMethod]
    public void AVehicleOnAnotherTrackIsFetchedFromItBeforeDeparture()
    {
        var (plan, part, vehicle) = Arrange();
        part.FromTrack = TrackAt(plan, "G", "1");

        var note = DepartureNotes(plan, part).OfType<FromTrackNote>().Single();

        Assert.IsTrue(note.IsForDeparture);
        Assert.AreEqual($"Before departure, fetch locomotive {vehicle} from track 1.", note.ToText);
        Assert.ContainsSingle(part.From.StationNotes(Sessions.All, Settings, plan).OfType<FromTrackNote>(),
            "The dispatcher has to set the road for the move, so the note is theirs as well.");
    }

    [TestMethod]
    public void AVehicleIsPutOnAnotherTrackAfterArrival()
    {
        var (plan, part, vehicle) = Arrange();
        part.ToTrack = TrackAt(plan, "Snu", "2");

        var note = ArrivalNotes(plan, part).OfType<ToTrackNote>().Single();

        Assert.IsTrue(note.IsForArrival);
        Assert.AreEqual($"After arrival, shunt locomotive {vehicle} to track 2.", note.ToText);
        Assert.ContainsSingle(part.To.StationNotes(Sessions.All, Settings, plan).OfType<ToTrackNote>());
    }

    [TestMethod]
    public void FetchingFromAnotherTrackSaysAllTheCoupleNoteWould()
    {
        // Fetching the vehicle is what brings it to the train; being told to couple it as well is being
        // told the same thing twice.
        var (plan, part, _) = Arrange();
        part.FromTrack = TrackAt(plan, "G", "1");

        var notes = DepartureNotes(plan, part).ToList();

        Assert.ContainsSingle(notes.OfType<FromTrackNote>());
        Assert.IsEmpty(notes.OfType<CoupleNote>());
        Assert.IsEmpty(notes.OfType<UseNote>());
    }

    [TestMethod]
    public void ANamedTrackSaysMoreThanParking()
    {
        // Parking says only that the vehicle is not on the train's track; the named track says which one.
        var (plan, part, _) = Arrange();
        part.TractionOptions!.FromParking = true;
        part.TractionOptions.ToParking = true;
        part.FromTrack = TrackAt(plan, "G", "1");
        part.ToTrack = TrackAt(plan, "Snu", "2");

        Assert.IsEmpty(DepartureNotes(plan, part).OfType<FromParkingNote>());
        Assert.ContainsSingle(DepartureNotes(plan, part).OfType<FromTrackNote>());
        Assert.IsEmpty(ArrivalNotes(plan, part).OfType<ToParkingNote>());
        Assert.ContainsSingle(ArrivalNotes(plan, part).OfType<ToTrackNote>());
    }

    [TestMethod]
    public void PuttingOnAnotherTrackSaysAllTheUncoupleNoteWould()
    {
        var (plan, part, _) = Arrange();
        part.ToTrack = TrackAt(plan, "Snu", "2");

        var notes = ArrivalNotes(plan, part).ToList();

        Assert.ContainsSingle(notes.OfType<ToTrackNote>());
        Assert.IsEmpty(notes.OfType<UncoupleNote>());
    }

    [TestMethod]
    public void TurningTheLocoIsStillAskedForBeforeItIsPutAway()
    {
        var (plan, part, _) = Arrange();
        part.TractionOptions!.TurnLoco = true;
        part.ToTrack = TrackAt(plan, "Snu", "2");

        var notes = ArrivalNotes(plan, part).ToList();

        Assert.ContainsSingle(notes.OfType<TurnNote>());
        Assert.ContainsSingle(notes.OfType<ToTrackNote>());
    }

    [TestMethod]
    public void AWagonsetIsFetchedFromAnotherTrackToo()
    {
        var (plan, part, _) = Arrange(ScheduledObjectType.Wagonset);
        part.FromTrack = TrackAt(plan, "G", "2");
        part.ToTrack = TrackAt(plan, "Snu", "2");

        Assert.ContainsSingle(DepartureNotes(plan, part).OfType<ShuntWagonsToDepartureTrackNote>());
        Assert.IsEmpty(DepartureNotes(plan, part).OfType<CoupleNote>());
        Assert.ContainsSingle(ArrivalNotes(plan, part).OfType<ShuntWagonsToArrivalTrackNote>(),
            "A wagonset has no arrival notes of its own, but being put on another track is one.");
    }

    [TestMethod]
    public void TheLocoDriverIsToldOnlyToShuntTheWagons()
    {
        // The wagonset block of the duty booklet names the wagonset and its tracks, so the timetable
        // repeats neither.
        var (plan, part, _) = Arrange(ScheduledObjectType.Wagonset);
        part.FromTrack = TrackAt(plan, "G", "2");
        part.ToTrack = TrackAt(plan, "Snu", "2");

        var departure = DepartureNotes(plan, part).ToList();
        var arrival = ArrivalNotes(plan, part).ToList();

        Assert.IsEmpty(departure.OfType<FromTrackNote>());
        Assert.AreEqual("Shunt wagons to departure track before departure.", departure.OfType<ShuntWagonsToDepartureTrackNote>().Single().ToText);
        Assert.IsEmpty(arrival.OfType<ToTrackNote>());
        Assert.AreEqual("Shunt wagons to their arrival track after arrival.", arrival.OfType<ShuntWagonsToArrivalTrackNote>().Single().ToText);
    }

    [TestMethod]
    public void TheDispatcherStillReadsWhichWagonsetGoesToWhichTrack()
    {
        // The dispatcher has no wagonset block to look the track up in.
        var (plan, part, wagonset) = Arrange(ScheduledObjectType.Wagonset);
        part.FromTrack = TrackAt(plan, "G", "2");
        part.ToTrack = TrackAt(plan, "Snu", "2");

        var departure = part.From.StationNotes(Sessions.All, Settings, plan);
        var arrival = part.To.StationNotes(Sessions.All, Settings, plan);

        Assert.AreEqual($"Before departure, fetch wagonset {wagonset} from track 2.", departure.OfType<FromTrackNote>().Single().ToText);
        Assert.IsEmpty(departure.OfType<ShuntWagonsNote>());
        Assert.AreEqual($"After arrival, shunt wagonset {wagonset} to track 2.", arrival.OfType<ToTrackNote>().Single().ToText);
        Assert.IsEmpty(arrival.OfType<ShuntWagonsNote>());
    }

    [TestMethod]
    public void TheNoteNamesTheDaysTheKindOfVehicleAndWhenTheMoveIsMade()
    {
        // The wording read at the station, in Swedish: the days tight enough to take no more room than
        // they must, the kind of vehicle to look for, and whether the move belongs before the departure
        // or after the arrival.
        var (plan, part, wagonset) = Arrange(
            ScheduledObjectType.Wagonset,
            trainRuns: Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday),
            vehicleAssigned: Sessions.FromDays(Days.Monday | Days.Wednesday | Days.Friday));
        plan.Layout.Settings.General.UseDays = true;
        plan.Layout.Settings.General.MaxSessions = 7;
        part.FromTrack = TrackAt(plan, "G", "1");
        part.ToTrack = TrackAt(plan, "Snu", "2");

        WithCulture("sv-SE", () =>
        {
            var departure = part.From.StationNotes(Sessions.All, Settings, plan).OfType<FromTrackNote>().Single();
            var arrival = part.To.StationNotes(Sessions.All, Settings, plan).OfType<ToTrackNote>().Single();

            Assert.AreEqual($"M,O,F: Innan avgång hämta vagnsätt {wagonset} från spår 1.", departure.ToText);
            Assert.AreEqual($"M,O,F: Efter ankomst växla in vagnsätt {wagonset} till spår 2.", arrival.ToText);
        });
    }

    // Runs a test in a given language and puts the culture back, so a test asserting translated text
    // does not leave it set for whatever runs next on this thread.
    private static void WithCulture(string culture, Action test)
    {
        var (uiCulture, currentCulture) = (CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture);
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            test();
        }
        finally
        {
            (CultureInfo.CurrentUICulture, CultureInfo.CurrentCulture) = (uiCulture, currentCulture);
        }
    }

    [TestMethod]
    public void WagonsShuntedOnSomeOfTheTrainsSessionsNameThoseSessions()
    {
        var (plan, part, _) = Arrange(ScheduledObjectType.Wagonset, vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5));
        part.FromTrack = TrackAt(plan, "G", "2");
        var settings = plan.Layout.Settings.General.SessionSettings();
        var sessions = Sessions.FromSessionNumbers(1, 3, 5);

        var note = DepartureNotes(plan, part).OfType<ShuntWagonsToDepartureTrackNote>().Single();

        Assert.AreEqual($"{sessions.ToText(settings)}: Shunt wagons to departure track before departure.", note.ToText);
    }

    [TestMethod]
    public void WagonsetsOfSeveralSchedulesAreShuntedWithOneNote()
    {
        // Two wagonsets take turns on the train, each with its own schedule fetching it from a siding.
        // Between them there are wagons to shunt on every session the train runs.
        var (plan, part, _) = Arrange(ScheduledObjectType.Wagonset, vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5, 7, 9, 11, 13));
        part.FromTrack = TrackAt(plan, "G", "2");
        var other = plan.CreateSchedule();
        var otherPart = other.Append(Forward(plan).AsTrainPart).Value;
        otherPart.WagonSetOptions = new WagonSetOptions();
        otherPart.FromTrack = TrackAt(plan, "G", "4");
        plan.AssignVehicle(other, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 12, null), Sessions.FromSessionNumbers(2, 4, 6, 8, 10, 12, 14));

        var note = DepartureNotes(plan, part).OfType<ShuntWagonsToDepartureTrackNote>().Single();

        Assert.IsNull(note.Sessions);
        Assert.AreEqual("Shunt wagons to departure track before departure.", note.ToText);
    }

    [TestMethod]
    public void WagonsetsOfSeveralSchedulesNameTheSessionsTheyAreShuntedOnTogether()
    {
        var (plan, part, _) = Arrange(ScheduledObjectType.Wagonset, vehicleAssigned: Sessions.FromSessionNumbers(1));
        part.ToTrack = TrackAt(plan, "Snu", "2");
        var other = plan.CreateSchedule();
        var otherPart = other.Append(Forward(plan).AsTrainPart).Value;
        otherPart.WagonSetOptions = new WagonSetOptions();
        otherPart.ToTrack = TrackAt(plan, "Snu", "2");
        plan.AssignVehicle(other, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 12, null), Sessions.FromSessionNumbers(3));
        var settings = plan.Layout.Settings.General.SessionSettings();

        var note = ArrivalNotes(plan, part).OfType<ShuntWagonsToArrivalTrackNote>().Single();

        Assert.AreEqual(Sessions.FromSessionNumbers(1, 3).ToText(settings), note.Sessions?.ToText(settings));
    }

    [TestMethod]
    public void TheTrainsOwnTrackIsNoOtherTrack()
    {
        var (plan, part, _) = Arrange();
        part.FromTrack = part.From.Track;

        Assert.IsNull(part.OtherFromTrack);
        Assert.IsEmpty(DepartureNotes(plan, part).OfType<FromTrackNote>());
        Assert.ContainsSingle(DepartureNotes(plan, part).OfType<CoupleNote>(), "The vehicle is where the train is, so it is coupled as usual.");
    }

    [TestMethod]
    public void ATrackAtAnotherStationSaysNothingAboutThePart()
    {
        // What is left behind when the part's call is moved to another location: it would send the
        // vehicle to a track at a station the part does not start from.
        var (plan, part, _) = Arrange();
        part.FromTrack = TrackAt(plan, "Yb", "1");

        Assert.IsNull(part.OtherFromTrack);
        Assert.IsEmpty(DepartureNotes(plan, part).OfType<FromTrackNote>());
    }

    [TestMethod]
    public void OnlyTheVehiclesOfThePartsOwnScheduleAreSentToTheTrack()
    {
        // A wagonset's schedule works the same train. The locomotive's schedule names the other track,
        // so the locomotive is fetched from it — the wagonset is not.
        var (plan, part, loco) = Arrange();
        part.FromTrack = TrackAt(plan, "G", "1");
        var wagons = plan.CreateSchedule();
        wagons.Append(Forward(plan).AsTrainPart);
        plan.AssignVehicle(wagons, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Bo", 7, null), Sessions.All);

        var note = DepartureNotes(plan, part).OfType<FromTrackNote>().Single();

        Assert.AreEqual(loco, note.ScheduledObject);
    }

    [TestMethod]
    public void AVehicleWorkingEverySessionTheTrainRunsNamesNoSessions()
    {
        // The train runs 1, 3 and 5 and the vehicle is assigned for more than that: on every session the
        // train runs it is there, so naming sessions would only repeat the train's own.
        var (plan, part, vehicle) = Arrange(
            trainRuns: Sessions.FromSessionNumbers(1, 3, 5),
            vehicleAssigned: Sessions.FromSessionNumbers(1, 2, 3, 4, 5));
        part.FromTrack = TrackAt(plan, "G", "1");

        var note = DepartureNotes(plan, part).OfType<FromTrackNote>().Single();

        Assert.IsNull(note.Sessions);
        Assert.AreEqual($"Before departure, fetch locomotive {vehicle} from track 1.", note.ToText);
    }

    [TestMethod]
    public void AVehicleWorkingSomeOfTheTrainsSessionsNamesThoseSessions()
    {
        var (plan, part, vehicle) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5));
        part.ToTrack = TrackAt(plan, "Snu", "2");
        var settings = plan.Layout.Settings.General.SessionSettings();
        var sessions = Sessions.FromSessionNumbers(1, 3, 5);

        var note = ArrivalNotes(plan, part).OfType<ToTrackNote>().Single();

        Assert.AreEqual(sessions.ToText(settings), note.Sessions?.ToText(settings));
        Assert.AreEqual($"{sessions.ToText(settings)}: After arrival, shunt locomotive {vehicle} to track 2.", note.ToText,
            "The sessions lead, so the reader sees first whether the note is for today.");
        StringAssert.Contains(note.ToHtml.Value, sessions.ToHtml(settings).Value,
            "Written as the session circles the session columns use.");
    }

    [TestMethod]
    public void OnlyTheSessionsTheTrainAlsoRunsAreNamed()
    {
        // Assigned 1–4, but the train runs only the odd sessions: the vehicle does it on 1 and 3.
        var (plan, part, _) = Arrange(
            trainRuns: Sessions.FromSessionNumbers(1, 3, 5, 7),
            vehicleAssigned: Sessions.FromSessionNumbers(1, 2, 3, 4));
        part.FromTrack = TrackAt(plan, "G", "1");
        var settings = plan.Layout.Settings.General.SessionSettings();

        var note = DepartureNotes(plan, part).OfType<FromTrackNote>().Single();

        Assert.AreEqual(Sessions.FromSessionNumbers(1, 3).ToText(settings), note.Sessions?.ToText(settings));
    }

    [TestMethod]
    public void AVehicleNotWorkingAnySessionTheTrainRunsGetsNoNote()
    {
        var (plan, part, _) = Arrange(
            trainRuns: Sessions.FromSessionNumbers(1, 3, 5),
            vehicleAssigned: Sessions.FromSessionNumbers(2, 4, 6));
        part.FromTrack = TrackAt(plan, "G", "1");

        Assert.IsEmpty(DepartureNotes(plan, part).OfType<FromTrackNote>());
    }

    [TestMethod]
    public void VehiclesTakingTurnsEachNameTheirOwnSessions()
    {
        // Two locomotives share the schedule on alternate sessions of a train that runs every session.
        var (plan, part, first) = Arrange(vehicleAssigned: Sessions.FromSessionNumbers(1, 3, 5));
        var second = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 43, null);
        plan.AssignVehicle(part.Schedule!, second, Sessions.FromSessionNumbers(2, 4, 6));
        part.FromTrack = TrackAt(plan, "G", "1");
        var settings = plan.Layout.Settings.General.SessionSettings();

        var notes = DepartureNotes(plan, part).OfType<FromTrackNote>().ToList();

        Assert.HasCount(2, notes);
        Assert.AreEqual("1,3,5", notes.Single(n => n.ScheduledObject == first).Sessions?.ToText(settings));
        Assert.AreEqual("2,4,6", notes.Single(n => n.ScheduledObject == second).Sessions?.ToText(settings));
    }

    [TestMethod]
    public void ALayoutCountingInDaysNamesTheDays()
    {
        var (plan, part, _) = Arrange(
            trainRuns: Sessions.FromDays(Days.Monday | Days.Tuesday | Days.Wednesday | Days.Thursday | Days.Friday),
            vehicleAssigned: Sessions.FromDays(Days.Monday | Days.Wednesday));
        plan.Layout.Settings.General.UseDays = true;
        plan.Layout.Settings.General.MaxSessions = 7;
        part.FromTrack = TrackAt(plan, "G", "1");
        var settings = plan.Layout.Settings.General.SessionSettings();

        var note = DepartureNotes(plan, part).OfType<FromTrackNote>().Single();

        Assert.AreEqual(Sessions.FromDays(Days.Monday | Days.Wednesday).ToText(settings), note.Sessions?.ToText(settings));
        StringAssert.StartsWith(note.ToText, $"{Sessions.FromDays(Days.Monday | Days.Wednesday).ToText(settings)}: Before departure, fetch");
    }

    [TestMethod]
    public void TheOtherTracksAreRestoredAsTheLayoutsOwnTracks()
    {
        var (plan, part, _) = Arrange();
        part.FromTrack = TrackAt(plan, "G", "1");
        part.ToTrack = TrackAt(plan, "Snu", "2");

        var options = PlanJson.CreateOptions();
        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, options), options)!;
        var restoredPart = restored.Schedules.Single().Parts.Single();

        // The very tracks of the restored layout, not copies of them: a copy is a track no station has.
        Assert.AreSame(TrackAt(restored, "G", "1"), restoredPart.FromTrack);
        Assert.AreSame(TrackAt(restored, "Snu", "2"), restoredPart.ToTrack);
        Assert.IsNotNull(restoredPart.OtherFromTrack);
        Assert.IsNotNull(restoredPart.OtherToTrack);
    }

    [TestMethod]
    public void MovingAPartEndToAnotherStationForgetsTheTrackChosenThere()
    {
        var (plan, outward, _) = Arrange();
        var back = outward.Schedule!.Append(Return(plan).AsTrainPart).Value;
        outward.FromTrack = TrackAt(plan, "G", "1");
        outward.ToTrack = TrackAt(plan, "Snu", "2");
        back.FromTrack = TrackAt(plan, "Snu", "1");

        Assert.IsNotNull(outward.Schedule);

        var edit = outward.Schedule.EditPart(outward, outward.From, CallAt(Forward(plan), "Yb"));

        Assert.IsTrue(edit.HasValue, edit.Message);
        Assert.IsNull(outward.ToTrack, "A track at Snu says nothing about a part that now ends at Yb.");
        Assert.AreSame(TrackAt(plan, "G", "1"), outward.FromTrack, "The end that did not move keeps its track.");
        Assert.AreEqual("Yb", back.From.OperationLocation.Signature, "The next part follows the edit…");
        Assert.IsNull(back.FromTrack, "…and forgets its track at the station it no longer starts from.");
    }

    [TestMethod]
    public void ATrackVehiclesAreFetchedFromCannotBeDeleted()
    {
        var (plan, part, _) = Arrange();
        var track = TrackAt(plan, "G", "1");
        part.FromTrack = track;

        var deletion = plan.TryDelete(track);

        Assert.IsTrue(deletion.IsDenied, "No train calls at the track, but a vehicle stands on it.");
        Assert.Contains(track, track.Station.Tracks);
    }

    [TestMethod]
    public void ATrackTrainsCallAtCannotBeDeleted()
    {
        var (plan, part, _) = Arrange();

        Assert.IsTrue(plan.MayDelete(part.From.Track).IsDenied);
    }

    [TestMethod]
    public void DeletingATrackForgetsItOnAPartThatNoLongerCountsIt()
    {
        // No train calls at G track 4, and the part ends at Snu, so a track at G says nothing about its end.
        var (plan, part, _) = Arrange();
        var elsewhere = TrackAt(plan, "G", "4");
        part.ToTrack = elsewhere;

        var deletion = plan.TryDelete(elsewhere);

        Assert.IsFalse(deletion.IsDenied, deletion.ToString());
        Assert.DoesNotContain(elsewhere, elsewhere.Station.Tracks);
        Assert.IsNull(part.ToTrack, "The part is not left holding a track that is no longer anywhere.");
    }
}
