using System.Globalization;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that the other track vehicles are put on after one part arrives and fetched from before the
/// next departs are kept in step: setting one end sets the other, and a named track not met at the other
/// end is reported (rule S7).
/// </summary>
[TestClass]
public class StandingTrackPairTests
{
    private static readonly ValidationSettings Validation = new();

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // Forward 12:00 G → Snu 12:55 arriving on track 1, back 13:00 Snu → G departing from track 2, and
    // forward again 15:00 from G. Snu is given a track 3 as well, so a track can be named that neither
    // train uses. A locomotive works the trains named, in that order.
    private static (Plan Plan, Schedule Schedule, IReadOnlyList<ScheduledTrainPart> Parts) Arrange(params int[] trainNumbers)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(15, 00)));
        var plan = Plan.Create("Test", timetable);
        ((Station)Snu(plan)).Add(new StationTrack(99, "3"));
        var schedule = plan.CreateSchedule();
        foreach (var number in trainNumbers) schedule.Add(plan.Timetable.Trains.First(t => t.Number == number).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "T44", 42, null);
        plan.AssignVehicle(schedule, vehicle, Sessions.All);
        return (plan, schedule, schedule.OrderedParts);
    }

    private static OperationLocation Snu(Plan plan) => plan.Layout.OperationLocations.Single(l => l.Signature == "Snu");
    private static StationTrack SnuTrack(Plan plan, string number) => Snu(plan)[number];

    private static List<ValidationError> Mismatches(Plan plan) =>
        [.. plan.GetValidationErrors(Validation).Where(e => e.ErrorType == ValidationErrorType.ScheduleStandingTrackMismatch)];

    [TestMethod]
    public void PuttingTheVehiclesOnATrackHasTheNextPartFetchThemFromIt()
    {
        var (plan, schedule, parts) = Arrange(1, 2);

        schedule.SetArrivalTrack(parts[0], SnuTrack(plan, "3"));

        Assert.AreEqual(SnuTrack(plan, "3"), parts[0].OtherToTrack);
        Assert.AreEqual(SnuTrack(plan, "3"), parts[1].OtherFromTrack);
        Assert.IsEmpty(Mismatches(plan));
    }

    [TestMethod]
    public void FetchingTheVehiclesFromATrackHasThePreviousPartPutThemOnIt()
    {
        var (plan, schedule, parts) = Arrange(1, 2);

        schedule.SetDepartureTrack(parts[1], SnuTrack(plan, "3"));

        Assert.AreEqual(SnuTrack(plan, "3"), parts[0].OtherToTrack);
        Assert.IsEmpty(Mismatches(plan));
    }

    [TestMethod]
    public void ATrackTheOtherTrainUsesNeedsNoNameAtThatEnd()
    {
        // The next train departs from track 2, so vehicles put there are where it needs them.
        var (plan, schedule, parts) = Arrange(1, 2);

        schedule.SetArrivalTrack(parts[0], SnuTrack(plan, "2"));

        Assert.IsNull(parts[1].FromTrack);
        Assert.IsEmpty(Mismatches(plan));
    }

    [TestMethod]
    public void GoingBackToTheTrainsOwnTrackForgetsThePairedTrack()
    {
        var (plan, schedule, parts) = Arrange(1, 2);
        schedule.SetArrivalTrack(parts[0], SnuTrack(plan, "3"));

        schedule.SetArrivalTrack(parts[0], null);

        Assert.IsNull(parts[0].OtherToTrack);
        Assert.IsNull(parts[1].OtherFromTrack);
    }

    [TestMethod]
    public void ANamedTrackClearsStablingAtBothEnds()
    {
        var (plan, schedule, parts) = Arrange(1, 2);
        schedule.SetArrivalLayover(parts[0], TractionLayover.Stabling);

        schedule.SetArrivalTrack(parts[0], SnuTrack(plan, "3"));

        Assert.AreEqual(TractionLayover.None, parts[0].TractionOptions!.ToLayover);
        Assert.AreEqual(TractionLayover.None, parts[1].TractionOptions!.FromLayover);
    }

    [TestMethod]
    public void ATrackPutOnButNotFetchedFromIsReported()
    {
        var (plan, _, parts) = Arrange(1, 2, 3);
        parts[0].ToTrack = SnuTrack(plan, "3");

        var error = Mismatches(plan).Single();

        Assert.AreEqual(ValidationScope.Schedule, error.Scope);
        Assert.AreEqual(Severity.Warning, error.Message.Severity);
        StringAssert.Contains(error.Message.Text, "on track 3");
        StringAssert.Contains(error.Message.Text, "from track 2");
    }

    [TestMethod]
    public void ATrackFetchedFromButNotPutOnIsReported()
    {
        var (plan, _, parts) = Arrange(1, 2);
        parts[1].FromTrack = SnuTrack(plan, "3");

        Assert.HasCount(1, Mismatches(plan));
    }

    [TestMethod]
    public void WithoutANamedTrackALocomotiveMayMoveFromArrivalToDepartureTrack()
    {
        // Arriving on track 1 and leaving from track 2 is an everyday loco move the use note covers.
        var (plan, _, _) = Arrange(1, 2);

        Assert.IsEmpty(Mismatches(plan));
    }

    [TestMethod]
    public void AcrossABrokenJointNothingIsPairedOrReported()
    {
        var (plan, schedule, parts) = Arrange(1, 3);

        schedule.SetArrivalTrack(parts[0], SnuTrack(plan, "3"));

        Assert.IsNull(parts[1].FromTrack);
        Assert.IsEmpty(Mismatches(plan));
    }
}
