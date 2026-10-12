using System.Globalization;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a wagonset can be moved to stabling or lifted off the layout between two trains, as a
/// traction unit can: the choice is kept in the wagonset options, paired at both ends, said in a note,
/// reported when the ends disagree, and it frees the track the wagonset would otherwise hold.
/// </summary>
[TestClass]
public class WagonSetLayoverTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    // Forward 12:00 G → Snu 12:55, back 13:00 Snu → G 13:55, forward again 15:00: a wagonset working them.
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
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gb", 7, null);
        plan.AssignVehicle(schedule, vehicle, Sessions.All);
        return (plan, schedule, schedule.OrderedParts);
    }

    [TestMethod]
    public void TheLayoverIsKeptInTheWagonSetOptionsAndPairedAtTheOtherEnd()
    {
        var (_, schedule, parts) = Arrange(1, 2);

        schedule.SetArrivalLayover(parts[0], TractionLayover.LiftedOff);

        Assert.AreEqual(TractionLayover.LiftedOff, parts[0].WagonSetOptions!.ToLayover);
        Assert.AreEqual(TractionLayover.LiftedOff, parts[1].WagonSetOptions!.FromLayover);
        Assert.IsNull(parts[0].TractionOptions, "A wagonset has no traction options to put it in.");
        Assert.AreEqual(TractionLayover.LiftedOff, parts[0].ArrivalLayover);
        Assert.AreEqual(TractionLayover.LiftedOff, parts[1].DepartureLayover);
    }

    [TestMethod]
    public void AWagonSetLiftedOffIsLiftedOffAfterArrivalAndOnAgainBeforeDeparture()
    {
        var (plan, schedule, parts) = Arrange(1, 2);
        schedule.SetArrivalLayover(parts[0], TractionLayover.LiftedOff);

        var arrival = parts[0].To.DriverNotes(Sessions.All, Settings, plan).ToList();
        var departure = parts[1].From.DriverNotes(Sessions.All, Settings, plan).ToList();

        StringAssert.StartsWith(arrival.OfType<LiftOffNote>().Single().ToText, "After arrival, lift");
        StringAssert.StartsWith(departure.OfType<LiftOnNote>().Single().ToText, "Before departure, lift");
        Assert.IsEmpty(arrival.OfType<UncoupleNote>(), "Lifting off says all the uncouple note would.");
        Assert.IsEmpty(departure.OfType<CoupleNote>());
    }

    [TestMethod]
    public void AStabledWagonSetIsMovedNotDriven()
    {
        var (plan, schedule, parts) = Arrange(1, 2);
        schedule.SetArrivalLayover(parts[0], TractionLayover.Stabling);

        var arrival = parts[0].To.DriverNotes(Sessions.All, Settings, plan).OfType<ToParkingNote>().Single();
        var departure = parts[1].From.DriverNotes(Sessions.All, Settings, plan).OfType<FromParkingNote>().Single();

        StringAssert.StartsWith(arrival.ToText, "After arrival, move");
        StringAssert.StartsWith(departure.ToText, "Before departure, move");
    }

    [TestMethod]
    public void EndsThatDoNotMatchAreReported()
    {
        var (plan, _, parts) = Arrange(1, 2);
        parts[0].WagonSetOptions = new WagonSetOptions { ToLayover = TractionLayover.Stabling };

        var errors = plan.GetValidationErrors(new ValidationSettings())
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleLayoverMismatch).ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void AWagonSetOffTheTrackHoldsNoTrackBetweenItsTrains()
    {
        var (_, schedule, parts) = Arrange(1, 2);
        var arrivalTrack = parts[0].To.Track;
        Assert.IsTrue(schedule.TrackHolds.Any(h => h.Track.Equals(arrivalTrack) && h.From == parts[0].To.Departure),
            "Left on the track, the wagonset holds it until the next train.");

        schedule.SetArrivalLayover(parts[0], TractionLayover.LiftedOff);

        Assert.IsFalse(schedule.TrackHolds.Any(h => h.Track.Equals(arrivalTrack) && h.From == parts[0].To.Departure),
            "Lifted off, it holds no track in between.");
    }
}
