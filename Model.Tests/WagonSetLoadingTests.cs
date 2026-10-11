using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class WagonSetLoadingTests
{
    // Forward 12:00 G->Yb->Snu (12:55), return 13:00 Snu->Yb->G (13:55). A cargo wagonset works both.
    private static (ScheduledTrainPart Outward, ScheduledTrainPart Back) CreateWorking(bool load, bool unload)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var forward = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        var back = TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00));
        timetable.Add(forward);
        timetable.Add(back);
        var plan = Plan.Create("Test", timetable);

        var schedule = plan.CreateSchedule();
        var outward = schedule.Add(forward.AsTrainPart);
        var returning = schedule.Add(back.AsTrainPart);
        var wagons = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Timber", 1, null);
        wagons.WagonSetKind = WagonSetKind.Cargo;
        plan.AssignVehicle(schedule, wagons);
        outward.WagonSetOptions = new WagonSetOptions { HasCoupleNote = false, HasUncoupleNote = false, LoadWagons = load };
        returning.WagonSetOptions = new WagonSetOptions { HasCoupleNote = false, HasUncoupleNote = false, UnloadWagons = unload };
        return (outward, returning);
    }

    [TestMethod]
    public void LoadingWagonsGivesAnArrivalNote()
    {
        var (outward, _) = CreateWorking(load: true, unload: false);

        var note = outward.GeneratedArrivalNotes.OfType<LoadWagonsNote>().Single();

        Assert.IsTrue(note.IsForArrival);
    }

    [TestMethod]
    public void UnloadingWagonsReturnsThemToWhereTheyWereLoadedEvenWithAnotherTrain()
    {
        var (outward, returning) = CreateWorking(load: true, unload: true);

        var note = returning.GeneratedArrivalNotes.OfType<UnloadWagonsNote>().Single();

        Assert.AreEqual(outward.To.OperationLocation, note.ReturnTo);
        StringAssert.Contains(note.ToText, outward.To.OperationLocation.Name);
    }

    [TestMethod]
    public void UnloadingWagonsThatAreLoadedNowhereNamesNoReturnLocation()
    {
        var (_, returning) = CreateWorking(load: false, unload: true);

        var note = returning.GeneratedArrivalNotes.OfType<UnloadWagonsNote>().Single();

        Assert.IsNull(note.ReturnTo);
    }

    [TestMethod]
    public void NothingTickedGivesNoLoadingNotes()
    {
        var (outward, returning) = CreateWorking(load: false, unload: false);

        Assert.IsFalse(outward.GeneratedArrivalNotes.Any(n => n is LoadWagonsNote or UnloadWagonsNote));
        Assert.IsFalse(returning.GeneratedArrivalNotes.Any(n => n is LoadWagonsNote or UnloadWagonsNote));
    }

    [TestMethod]
    public void MatchingLoadAndUnloadGivesNoMismatch()
    {
        var (outward, returning) = CreateWorking(load: true, unload: true);

        Assert.IsEmpty(Mismatches(outward));
    }

    [TestMethod]
    public void UnloadWithoutEarlierLoadIsReported()
    {
        var (outward, _) = CreateWorking(load: false, unload: true);

        Assert.HasCount(1, Mismatches(outward));
    }

    [TestMethod]
    public void LoadNeverUnloadedIsReported()
    {
        var (outward, _) = CreateWorking(load: true, unload: false);

        Assert.HasCount(1, Mismatches(outward));
    }

    [TestMethod]
    public void UnloadBeforeLoadIsReportedTwice()
    {
        // Unload on the outward part and load on the way back: unloaded first, and then left loaded.
        var (outward, returning) = CreateWorking(load: false, unload: false);
        outward.WagonSetOptions!.UnloadWagons = true;
        returning.WagonSetOptions!.LoadWagons = true;

        Assert.HasCount(2, Mismatches(outward));
    }

    private static List<ValidationError> Mismatches(ScheduledTrainPart part) =>
        [.. part.Schedule!.Plan.GetValidationErrors(new ValidationSettings())
            .Where(e => e.ErrorType == ValidationErrorType.WagonSetLoadingMismatch)];
}
