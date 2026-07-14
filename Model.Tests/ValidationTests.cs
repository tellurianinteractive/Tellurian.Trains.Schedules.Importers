using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class ValidationTests
{
    private static readonly ValidationSettings Settings = new();

    private static Timetable NewTimetable()
    {
        TestDataFactory.Init();
        return new Timetable("Test", TestDataFactory.Layout());
    }

    private static TrainCategory Category => new() { Id = 1, Name = "P", Prefix = "P" };

    // --- T4: trains sharing company, category and number must run on non-overlapping sessions ---

    [TestMethod]
    public void DuplicateTrainNumberOnOverlappingSessionsIsReported()
    {
        var timetable = NewTimetable();
        var category = Category;
        var a = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        a.Sessions = Sessions.FromSessionNumbers(1, 2, 3);
        var b = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(14, 00));
        b.Sessions = Sessions.FromSessionNumbers(3, 4, 5); // overlaps a on session 3
        // Added directly to bypass Timetable.Add's identity de-duplication, as an import can.
        timetable.Trains.Add(a);
        timetable.Trains.Add(b);
        var plan = Plan.Create("Test", timetable);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber)
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void DuplicateTrainNumberOnDisjointSessionsIsAllowed()
    {
        var timetable = NewTimetable();
        var category = Category;
        var a = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        a.Sessions = Sessions.FromSessionNumbers(1, 2, 3);
        var b = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(14, 00));
        b.Sessions = Sessions.FromSessionNumbers(4, 5, 6); // no session in common with a
        timetable.Trains.Add(a);
        timetable.Trains.Add(b);
        var plan = Plan.Create("Test", timetable);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void DistinctTrainNumbersAreNotFlaggedAsDuplicates()
    {
        var timetable = NewTimetable();
        var category = Category;
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        var plan = Plan.Create("Test", timetable);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void TrainNumberValidationCanBeSwitchedOff()
    {
        var timetable = NewTimetable();
        var category = Category;
        var a = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        var b = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(14, 00));
        timetable.Trains.Add(a);
        timetable.Trains.Add(b);
        var plan = Plan.Create("Test", timetable);
        var settings = new ValidationSettings { ValidateTrainNumbers = false };

        var errors = plan.GetValidationErrors(settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber);

        Assert.IsEmpty(errors);
    }

    // --- S2: a schedule's parts must be geographically contiguous ---

    private static Plan PlanWithTwoForwardTrains()
    {
        var timetable = NewTimetable();
        var category = Category;
        // Two forward runs (G->Snu). Chaining one after the other leaves a gap: the vehicle would have
        // to jump from Snu back to G with no train.
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(14, 00)));
        return Plan.Create("Test", timetable);
    }

    private static Plan PlanWithForwardAndReturn()
    {
        var timetable = NewTimetable();
        var category = Category;
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));  // G->Snu
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00))); // Snu->G
        return Plan.Create("Test", timetable);
    }

    [TestMethod]
    public void NonContiguousScheduleIsReported()
    {
        var plan = PlanWithTwoForwardTrains();
        var first = plan.Timetable.Trains.First(t => t.Number == 1);
        var second = plan.Timetable.Trains.First(t => t.Number == 3);
        var schedule = plan.CreateSchedule();
        schedule.Add(first.AsTrainPart);  // ends at Snu
        schedule.Add(second.AsTrainPart); // starts at G, not Snu

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous)
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void ContiguousScheduleIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var @return = plan.Timetable.Trains.First(t => t.Number == 2);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);  // ends at Snu
        schedule.Add(@return.AsTrainPart);  // starts at Snu

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void ContiguityIsNotCheckedWhenPartsOverlap()
    {
        var timetable = NewTimetable();
        var category = Category;
        // Two forward runs (G->Snu) overlapping in time: not one vehicle's working. S1 reports the
        // overlap; S2 (contiguity) is skipped rather than cascading misleading gap warnings.
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00))); // 12:00-12:55
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(12, 30))); // 12:30-13:25, overlaps
        var plan = Plan.Create("Test", timetable);
        var schedule = plan.CreateSchedule();
        schedule.Add(plan.Timetable.Trains.First(t => t.Number == 1).AsTrainPart);
        schedule.Add(plan.Timetable.Trains.First(t => t.Number == 3).AsTrainPart);

        var errors = plan.GetValidationErrors(Settings).ToList();

        Assert.IsEmpty(errors.Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous), "S2 is skipped for an overlapping schedule.");
        Assert.IsNotEmpty(errors.Where(e => e.ErrorType == ValidationErrorType.VehicleScheduleOverlap), "S1 still reports the overlap.");
    }

    // --- S3: an all-session traction schedule must return the vehicle to its start station ---

    // Adds a locomotive worked to the schedule; closure (S3) only applies to traction schedules.
    private static void AddLoco(Plan plan, Schedule schedule)
    {
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", 1, null);
        plan.AssignVehicle(schedule, vehicle);
    }

    [TestMethod]
    public void AllSessionScheduleThatDoesNotReturnToStartIsReported()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // G->Snu, runs all sessions
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotClosed)
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void ClosedScheduleIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // G->Snu
        var @return = plan.Timetable.Trains.First(t => t.Number == 2); // Snu->G, closes the loop back to G
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        schedule.Add(@return.AsTrainPart);
        AddLoco(plan, schedule);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotClosed);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void OnDemandScheduleNeedNotReturnToStart()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        forward.Sessions = Sessions.FromBitPattern(CommonSessionPatterns.OnDemand);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotClosed);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void WagonScheduleNeedNotReturnToStart()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // G->Snu, does not return
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        var wagon = plan.CreateVehicle(ScheduledObjectType.Wagonset, "W", 1, null);
        plan.AssignVehicle(schedule, wagon);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotClosed);

        Assert.IsEmpty(errors, "Closure (S3) applies to traction units, not wagons.");
    }

    [TestMethod]
    public void SubsetSessionScheduleThatDoesNotReturnIsNotCheckedByS3()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        forward.Sessions = Sessions.FromSessionNumbers(1, 2, 3); // a subset of the 14-session period
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotClosed);

        Assert.IsEmpty(errors, "A subset-session schedule may be completed by a complementary schedule.");
    }

    // --- S5: each of a vehicle's session combinations must return it to its start ---

    private static (Plan plan, ScheduledObject vehicle) VehicleSplitAcrossAlternatingSessions()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // G->Snu
        var @return = plan.Timetable.Trains.First(t => t.Number == 2); // Snu->G
        forward.Sessions = Sessions.FromSessionNumbers(1, 3, 5, 7);
        @return.Sessions = Sessions.FromSessionNumbers(2, 4, 6);
        var odd = plan.CreateSchedule();
        odd.Add(forward.AsTrainPart);
        var even = plan.CreateSchedule();
        even.Add(@return.AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(odd, vehicle, Sessions.FromSessionNumbers(1, 3, 5, 7));
        plan.AssignVehicle(even, vehicle, Sessions.FromSessionNumbers(2, 4, 6));
        return (plan, vehicle);
    }

    [TestMethod]
    public void NonClosingSessionCombinationsAreReported()
    {
        var (plan, _) = VehicleSplitAcrossAlternatingSessions();

        var errors = plan.GetValidationErrors(Settings).ToList();

        // Two combinations (odd works G->Snu, even works Snu->G); neither returns to its own start.
        Assert.HasCount(2, errors.Where(e => e.ErrorType == ValidationErrorType.SessionCombinationNotClosed).ToList());
        // The schedules run subsets of sessions, so S3 does not also fire.
        Assert.IsEmpty(errors.Where(e => e.ErrorType == ValidationErrorType.ScheduleNotClosed));
    }

    [TestMethod]
    public void SessionCombinationValidationCanBeSwitchedOff()
    {
        var (plan, _) = VehicleSplitAcrossAlternatingSessions();
        var settings = new ValidationSettings { ValidateDriverDuties = false };

        var errors = plan.GetValidationErrors(settings)
            .Where(e => e.ErrorType == ValidationErrorType.SessionCombinationNotClosed);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void ScheduleValidationCanBeSwitchedOff()
    {
        var plan = PlanWithTwoForwardTrains();
        var first = plan.Timetable.Trains.First(t => t.Number == 1);
        var second = plan.Timetable.Trains.First(t => t.Number == 3);
        var schedule = plan.CreateSchedule();
        schedule.Add(first.AsTrainPart);
        schedule.Add(second.AsTrainPart);
        var settings = new ValidationSettings { ValidateSchedules = false };

        var errors = plan.GetValidationErrors(settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous);

        Assert.IsEmpty(errors);
    }

    // --- Vehicle double-booking: same day AND overlapping time, not merely overlapping sessions ---

    private static (Plan plan, ScheduledObject loco) VehicleOnTwoForwardSchedules(int secondTrainStartHour)
    {
        var timetable = NewTimetable();
        var category = Category;
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00))); // 12:00-12:55
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(secondTrainStartHour, 00)));
        var plan = Plan.Create("Test", timetable);
        var first = plan.CreateSchedule();
        first.Add(plan.Timetable.Trains.First(t => t.Number == 1).AsTrainPart);
        var second = plan.CreateSchedule();
        second.Add(plan.Timetable.Trains.First(t => t.Number == 3).AsTrainPart);
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", 1, null);
        plan.AssignVehicle(first, loco);   // all sessions
        plan.AssignVehicle(second, loco);  // all sessions
        return (plan, loco);
    }

    [TestMethod]
    public void VehicleOnTwoSchedulesAtDifferentTimesIsNotDoubleBooked()
    {
        // Both schedules run all sessions (so sessions overlap), but the second train runs 14:00-14:55,
        // after the first ends at 12:55 - the loco does a morning turn then an afternoon turn.
        var (plan, _) = VehicleOnTwoForwardSchedules(secondTrainStartHour: 14);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleDoubleBooked);

        Assert.IsEmpty(errors, "Two schedules on the same day at different times are a valid roster.");
    }

    [TestMethod]
    public void VehicleOnTwoSchedulesOverlappingInTimeIsDoubleBooked()
    {
        // Both schedules run a forward train at 12:00-12:55 on all sessions: the loco cannot work both.
        var (plan, _) = VehicleOnTwoForwardSchedules(secondTrainStartHour: 12);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleDoubleBooked)
            .ToList();

        Assert.HasCount(1, errors);
    }

    // --- ValidationError GUI predicates: used by components to highlight the offending object ---

    // A duplicate-train-number conflict is a compact source of a ValidationError that involves two
    // trains and carries a track/time span, which is all the predicates need.
    private static ValidationError DuplicateTrainNumberError(out Train a, out Train b)
    {
        var timetable = NewTimetable();
        var category = Category;
        a = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        a.Sessions = Sessions.FromSessionNumbers(1, 2, 3);
        b = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(14, 00));
        b.Sessions = Sessions.FromSessionNumbers(3, 4, 5);
        timetable.Trains.Add(a);
        timetable.Trains.Add(b);
        var plan = Plan.Create("Test", timetable);
        return plan.GetValidationErrors(Settings).Single(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber);
    }

    [TestMethod]
    public void InvolvesReturnsTrueForTrainsInConflict()
    {
        var error = DuplicateTrainNumberError(out var a, out var b);

        Assert.IsTrue(error.Involves(a));
        Assert.IsTrue(error.Involves(b));
    }

    [TestMethod]
    public void InvolvesReturnsFalseForUnrelatedTrain()
    {
        var error = DuplicateTrainNumberError(out _, out _);
        var unrelated = TestDataFactory.CreateTrainInOppositeDirection(Category, 99, Time.FromHourAndMinute(20, 00));

        Assert.IsFalse(error.Involves(unrelated));
    }

    [TestMethod]
    public void InvolvesTrackMatchesTheConflictTracks()
    {
        var error = DuplicateTrainNumberError(out _, out _);

        Assert.IsTrue(error.Involves(error.FromTrack));
        Assert.IsTrue(error.Involves(error.ToTrack));
    }

    [TestMethod]
    public void OverlapsTimeRangeIsTrueWithinSpanAndFalseOutside()
    {
        var error = DuplicateTrainNumberError(out _, out _);

        Assert.IsTrue(error.OverlapsTimeRange(error.FromTime, error.ToTime), "The error's own span overlaps itself.");
        Assert.IsFalse(error.OverlapsTimeRange(Time.FromHourAndMinute(1, 00), Time.FromHourAndMinute(2, 00)), "A range entirely before the span does not overlap.");
        Assert.IsFalse(error.OverlapsTimeRange(Time.FromHourAndMinute(23, 00), Time.FromHourAndMinute(23, 30)), "A range entirely after the span does not overlap.");
    }

    [TestMethod]
    public void SeverityIsDerivedFromErrorType()
    {
        var error = DuplicateTrainNumberError(out _, out _);

        // A genuine conflict is a warning; advisory findings (speed, loco coverage) are informational.
        Assert.AreEqual(Severity.Warning, error.Severity);
        Assert.AreEqual(Severity.Warning, ValidationError.SeverityOf(ValidationErrorType.StationTrackConflict));
        Assert.AreEqual(Severity.Information, ValidationError.SeverityOf(ValidationErrorType.TrainSpeedTooSlow));
        Assert.AreEqual(Severity.Information, ValidationError.SeverityOf(ValidationErrorType.LocomotiveCoverageGap));
    }
}
