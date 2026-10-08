using System.Globalization;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a traction unit can spend a layover off the station's tracks — driven to stabling or
/// lifted off the layout — that this reaches the loco driver as a note at both ends, that setting one end
/// of a layover sets the other to match, and that ends which do not match are reported (rule S6).
/// </summary>
[TestClass]
public class TractionLayoverTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);
    private static readonly ValidationSettings Validation = new();

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // Forward 12:00 G → Snu 12:55, back 13:00 Snu → G 13:55, and forward again 15:00 G → Snu: a locomotive
    // working the trains named, in that order, with no traction options on any part to begin with.
    private static (Plan Plan, Schedule Schedule, IReadOnlyList<ScheduledTrainPart> Parts) Arrange(params int[] trainNumbers)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(15, 00)));
        var plan = Plan.Create("Test", timetable);
        var schedule = plan.CreateSchedule();
        foreach (var number in trainNumbers) schedule.Add(plan.Timetable.Trains.First(t => t.Number == number).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 42, null);
        plan.AssignVehicle(schedule, vehicle, Sessions.All);
        return (plan, schedule, schedule.OrderedParts);
    }

    private static StationTrack TrackAt(Plan plan, string signature, string number) =>
        plan.Layout.OperationLocations.Single(l => l.Signature == signature)[number];

    private static List<ValidationError> Mismatches(Plan plan) =>
        [.. plan.GetValidationErrors(Validation).Where(e => e.ErrorType == ValidationErrorType.ScheduleLayoverMismatch)];

    [TestMethod]
    public void ALiftedOffUnitIsLiftedOffAfterArrivalAndOnAgainBeforeDeparture()
    {
        var (plan, schedule, parts) = Arrange(1, 2);
        schedule.SetArrivalLayover(parts[0], TractionLayover.LiftedOff);

        var arrival = parts[0].To.DriverNotes(Sessions.All, Settings, plan).ToList();
        var departure = parts[1].From.DriverNotes(Sessions.All, Settings, plan).ToList();

        StringAssert.StartsWith(arrival.OfType<LiftOffNote>().Single().ToText, "After arrival, lift");
        StringAssert.StartsWith(departure.OfType<LiftOnNote>().Single().ToText, "Before departure, lift");
        Assert.IsEmpty(arrival.OfType<ToParkingNote>());
        Assert.IsEmpty(departure.OfType<FromParkingNote>());
        Assert.IsEmpty(departure.OfType<UseNote>(), "Pairing a layover must not add a use note to the next part.");
    }

    [TestMethod]
    public void SettingWhereTheTractionGoesAfterArrivalSetsWhereItComesFromNext()
    {
        var (_, schedule, parts) = Arrange(1, 2, 3);

        schedule.SetArrivalLayover(parts[0], TractionLayover.Stabling);

        Assert.AreEqual(TractionLayover.Stabling, parts[0].TractionOptions!.ToLayover);
        Assert.AreEqual(TractionLayover.Stabling, parts[1].TractionOptions!.FromLayover);
        Assert.AreEqual(TractionLayover.None, parts[1].TractionOptions!.ToLayover, "Only the paired end changes.");
        Assert.IsNull(parts[2].TractionOptions, "The pairing reaches one part, not the whole working.");
    }

    [TestMethod]
    public void SettingWhereTheTractionComesFromBeforeDepartureSetsWhereItWentBefore()
    {
        var (_, schedule, parts) = Arrange(1, 2);

        schedule.SetDepartureLayover(parts[1], TractionLayover.LiftedOff);

        Assert.AreEqual(TractionLayover.LiftedOff, parts[0].TractionOptions!.ToLayover);
        Assert.AreEqual(TractionLayover.LiftedOff, parts[1].TractionOptions!.FromLayover);
    }

    [TestMethod]
    public void ClearingOneEndClearsTheOther()
    {
        var (_, schedule, parts) = Arrange(1, 2);
        schedule.SetArrivalLayover(parts[0], TractionLayover.Stabling);

        schedule.SetDepartureLayover(parts[1], TractionLayover.None);

        Assert.AreEqual(TractionLayover.None, parts[0].TractionOptions!.ToLayover);
        Assert.AreEqual(TractionLayover.None, parts[1].TractionOptions!.FromLayover);
    }

    [TestMethod]
    public void StablingForgetsATrackNamedAtEitherEndOfTheLayover()
    {
        var (plan, schedule, parts) = Arrange(1, 2);
        parts[0].ToTrack = TrackAt(plan, "Snu", "2");
        parts[1].FromTrack = TrackAt(plan, "Snu", "2");

        schedule.SetArrivalLayover(parts[0], TractionLayover.Stabling);

        Assert.IsNull(parts[0].ToTrack);
        Assert.IsNull(parts[1].FromTrack);
    }

    [TestMethod]
    public void AcrossABrokenJointNothingIsPaired()
    {
        // Train 1 ends at Snu and train 3 leaves from G: the vehicle has no layover between them to pair.
        var (_, schedule, parts) = Arrange(1, 3);

        schedule.SetArrivalLayover(parts[0], TractionLayover.Stabling);

        Assert.IsNull(parts[1].TractionOptions);
    }

    [TestMethod]
    public void MatchingEndsAreNotReported()
    {
        var (plan, schedule, parts) = Arrange(1, 2, 3);
        schedule.SetArrivalLayover(parts[0], TractionLayover.LiftedOff);
        schedule.SetArrivalLayover(parts[1], TractionLayover.Stabling);

        Assert.IsEmpty(Mismatches(plan));
    }

    [TestMethod]
    public void EndsThatDoNotMatchAreReported()
    {
        var (plan, _, parts) = Arrange(1, 2, 3);
        parts[0].TractionOptions = new TractionOptions { ToLayover = TractionLayover.Stabling };
        parts[1].TractionOptions = new TractionOptions { FromLayover = TractionLayover.LiftedOff };

        var error = Mismatches(plan).Single();

        Assert.AreEqual(ValidationScope.Schedule, error.Scope);
        Assert.AreEqual(Severity.Warning, error.Message.Severity);
        StringAssert.Contains(error.Message.Text, "‘to stabling’");
        StringAssert.Contains(error.Message.Text, "‘lift on’");
    }

    [TestMethod]
    public void AnEndLeftOnTheTrackMustBeMetOnTheTrack()
    {
        // Lifted off after arrival, but the next part expects the unit still on the track.
        var (plan, _, parts) = Arrange(1, 2);
        parts[0].TractionOptions = new TractionOptions { ToLayover = TractionLayover.LiftedOff };

        Assert.HasCount(1, Mismatches(plan));
    }

    [TestMethod]
    public void ABrokenJointIsNotReportedAsAMismatch()
    {
        var (plan, _, parts) = Arrange(1, 3);
        parts[0].TractionOptions = new TractionOptions { ToLayover = TractionLayover.Stabling };

        Assert.IsEmpty(Mismatches(plan), "A broken joint is S2's to report.");
    }

    [TestMethod]
    public void FetchedFromStablingBeforeTheFirstTrainMustBeStabledAfterTheLast()
    {
        // G → Snu → G every session: the unit ends each session where it starts the next.
        var (plan, _, parts) = Arrange(1, 2);
        parts[0].TractionOptions = new TractionOptions { FromLayover = TractionLayover.Stabling };

        var error = Mismatches(plan).Single();

        Assert.AreEqual(Severity.Warning, error.Message.Severity);
        Assert.IsNotNull(error.Vehicle);
        StringAssert.Contains(error.Message.Text, "‘on track’");
        StringAssert.Contains(error.Message.Text, "‘from stabling’");
    }

    [TestMethod]
    public void StabledAfterTheLastTrainWhereItWasFetchedFromStablingIsNotReported()
    {
        var (plan, _, parts) = Arrange(1, 2);
        parts[0].TractionOptions = new TractionOptions { FromLayover = TractionLayover.Stabling };
        parts[1].TractionOptions = new TractionOptions { ToLayover = TractionLayover.Stabling };

        Assert.IsEmpty(Mismatches(plan));
    }

    [TestMethod]
    public void AWorkingEndingElsewhereIsNotPairedOverTheWrap()
    {
        // G → Snu → G → Snu: the unit ends the session at Snu but starts it at G — the closure rule's concern.
        var (plan, _, parts) = Arrange(1, 2, 3);
        parts[0].TractionOptions = new TractionOptions { FromLayover = TractionLayover.Stabling };

        Assert.IsEmpty(Mismatches(plan));
    }

    // Forward G → Snu on the odd sessions and back on the even, each in its own schedule, by one locomotive:
    // a circulation over two sessions, returning to G only at the end of the second.
    private static (Plan Plan, ScheduledTrainPart Forward, ScheduledTrainPart Return) ArrangeTwoSessionCirculation()
    {
        var (plan, _, _) = Arrange();
        plan.Layout.Settings.General.MaxSessions = 2;
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var @return = plan.Timetable.Trains.First(t => t.Number == 2);
        forward.Sessions = Sessions.FromSessionNumbers(1);
        @return.Sessions = Sessions.FromSessionNumbers(2);
        var odd = plan.CreateSchedule();
        odd.Add(forward.AsTrainPart);
        var even = plan.CreateSchedule();
        even.Add(@return.AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T43", 7, null);
        plan.AssignVehicle(odd, vehicle, Sessions.FromSessionNumbers(1));
        plan.AssignVehicle(even, vehicle, Sessions.FromSessionNumbers(2));
        return (plan, odd.Parts.Single(), even.Parts.Single());
    }

    [TestMethod]
    public void OverTwoSessionsTheStablingIsMatchedWhenTheUnitReturns()
    {
        var (plan, forward, @return) = ArrangeTwoSessionCirculation();
        forward.TractionOptions = new TractionOptions { FromLayover = TractionLayover.Stabling };
        @return.TractionOptions = new TractionOptions { ToLayover = TractionLayover.Stabling };

        Assert.IsEmpty(Mismatches(plan), "Left on the track at Snu overnight, stabled again at G after the return.");
    }

    [TestMethod]
    public void OverTwoSessionsAMissingStablingOnReturnIsReported()
    {
        var (plan, forward, _) = ArrangeTwoSessionCirculation();
        forward.TractionOptions = new TractionOptions { FromLayover = TractionLayover.Stabling };

        var error = Mismatches(plan).Single();

        Assert.HasCount(2, error.Schedules, "The two parts belong to different schedules.");
    }

    [TestMethod]
    public void OverTwoSessionsTheOvernightLayoverInBetweenIsPairedToo()
    {
        var (plan, forward, @return) = ArrangeTwoSessionCirculation();
        forward.TractionOptions = new TractionOptions { ToLayover = TractionLayover.LiftedOff };

        Assert.HasCount(1, Mismatches(plan), "Lifted off at Snu after session 1, but not lifted on before session 2.");

        @return.TractionOptions = new TractionOptions { FromLayover = TractionLayover.LiftedOff };

        Assert.IsEmpty(Mismatches(plan));
    }
}
