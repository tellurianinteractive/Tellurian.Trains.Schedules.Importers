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

    // --- S4: a schedule must have a traction unit on every session it operates ---

    // Adds a locomotive to the schedule for the given sessions (all sessions by default).
    private static ScheduledObject AddLoco(Plan plan, Schedule schedule, Sessions? sessions = null)
    {
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", 1, null);
        plan.AssignVehicle(schedule, vehicle, sessions);
        return vehicle;
    }

    [TestMethod]
    public void ScheduleWithNoVehicleAssignedIsReported()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart); // no vehicle assigned at all

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleHasNoVehicle)
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void TrainWithoutTractionOnEverySessionIsReported()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        forward.Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4); // runs on four sessions
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule, Sessions.FromSessionNumbers(1, 2)); // traction on 1,2 only — 3,4 uncovered

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.TrainMissingTraction && e.Involves(forward))
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void TrainHauledOnlyByWagonsIsReportedAsMissingTraction()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        var wagon = plan.CreateVehicle(ScheduledObjectType.Wagonset, "W", 1, null);
        plan.AssignVehicle(schedule, wagon);

        var errors = plan.GetValidationErrors(Settings).ToList();

        Assert.IsNotEmpty(errors.Where(e => e.ErrorType == ValidationErrorType.TrainMissingTraction), "A wagon cannot haul the train; it needs a traction unit.");
        Assert.IsNotEmpty(errors.Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed), "The wagon runs G->Snu without returning, so its circulation does not close.");
    }

    // A wagonset works its own turnus with no traction of its own; the train is hauled by the locomotive's
    // separate turnus, so per-train traction coverage is satisfied and the wagonset turnus is not flagged.
    [TestMethod]
    public void WagonsetTurnusAlongsideALocoTurnusIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var @return = plan.Timetable.Trains.First(t => t.Number == 2);
        var locoSchedule = plan.CreateSchedule();
        locoSchedule.Add(forward.AsTrainPart);
        locoSchedule.Add(@return.AsTrainPart);
        AddLoco(plan, locoSchedule);
        var wagonSchedule = plan.CreateSchedule();
        wagonSchedule.Add(forward.AsTrainPart);
        wagonSchedule.Add(@return.AsTrainPart);
        var wagon = plan.CreateVehicle(ScheduledObjectType.Wagonset, "W", 1, null);
        plan.AssignVehicle(wagonSchedule, wagon);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType is ValidationErrorType.TrainMissingTraction or ValidationErrorType.ScheduleHasNoVehicle);

        Assert.IsEmpty(errors, "The train is hauled by the loco turnus, so the parallel wagonset turnus needs no traction of its own.");
    }

    [TestMethod]
    public void TrainWithTractionOnAllSessionsIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var @return = plan.Timetable.Trains.First(t => t.Number == 2);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        schedule.Add(@return.AsTrainPart);
        AddLoco(plan, schedule); // all sessions

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType is ValidationErrorType.ScheduleHasNoVehicle or ValidationErrorType.TrainMissingTraction);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void OnDemandTrainNeedsNoTraction()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        forward.Sessions = Sessions.FromBitPattern(CommonSessionPatterns.OnDemand);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart); // no vehicle, but on-demand only

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType is ValidationErrorType.ScheduleHasNoVehicle or ValidationErrorType.TrainMissingTraction)
            .Where(e => e.Involves(forward));

        Assert.IsEmpty(errors, "An on-demand train needs no reserved traction.");
    }

    // --- S3 + S5: a traction unit's circulation must close (flow-balance) over the operating period ---

    [TestMethod]
    public void TractionUnitThatDoesNotReturnIsReported()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // G->Snu, runs all sessions
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed)
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void ClosedCirculationIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // G->Snu
        var @return = plan.Timetable.Trains.First(t => t.Number == 2); // Snu->G, closes the loop back to G
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        schedule.Add(@return.AsTrainPart);
        AddLoco(plan, schedule);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void OnDemandTractionNeedNotReturn()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        forward.Sessions = Sessions.FromBitPattern(CommonSessionPatterns.OnDemand);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed);

        Assert.IsEmpty(errors);
    }

    // A loco that works the forward leg on some sessions and the return leg on equally many other sessions
    // balances over the period (its position carries over between sessions): its circulation closes.
    [TestMethod]
    public void TractionUnitBalancedAcrossAlternatingSessionsIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // G->Snu
        var @return = plan.Timetable.Trains.First(t => t.Number == 2); // Snu->G
        forward.Sessions = Sessions.FromSessionNumbers(1, 3, 5);
        @return.Sessions = Sessions.FromSessionNumbers(2, 4, 6);
        var odd = plan.CreateSchedule();
        odd.Add(forward.AsTrainPart);
        var even = plan.CreateSchedule();
        even.Add(@return.AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(odd, vehicle, Sessions.FromSessionNumbers(1, 3, 5));
        plan.AssignVehicle(even, vehicle, Sessions.FromSessionNumbers(2, 4, 6));

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed);

        Assert.IsEmpty(errors, "Three forward legs and three return legs balance over the period.");
    }

    [TestMethod]
    public void TractionUnitWithMoreForwardThanReturnLegsIsReported()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var @return = plan.Timetable.Trains.First(t => t.Number == 2);
        forward.Sessions = Sessions.FromSessionNumbers(1, 3, 5, 7); // four forward legs
        @return.Sessions = Sessions.FromSessionNumbers(2, 4, 6);    // only three return legs
        var odd = plan.CreateSchedule();
        odd.Add(forward.AsTrainPart);
        var even = plan.CreateSchedule();
        even.Add(@return.AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(odd, vehicle, Sessions.FromSessionNumbers(1, 3, 5, 7));
        plan.AssignVehicle(even, vehicle, Sessions.FromSessionNumbers(2, 4, 6));

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed)
            .ToList();

        Assert.HasCount(1, errors, "One more forward than return leg leaves the unit stranded once.");
    }

    // The reported case: two schedules (forward A>B and return B>A) with two locos, each loco assigned to
    // BOTH schedules on complementary sessions. Every loco works both directions over the period, so each
    // closes — no conflict, even though neither schedule on its own returns to its start.
    [TestMethod]
    public void TwoLocosEachWorkingBothDirectionsAreAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1); // A>B
        var @return = plan.Timetable.Trains.First(t => t.Number == 2); // B>A
        forward.Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5, 6);
        @return.Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5, 6);
        var fwd = plan.CreateSchedule();
        fwd.Add(forward.AsTrainPart);
        var rtn = plan.CreateSchedule();
        rtn.Add(@return.AsTrainPart);
        var loco1 = plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", 1, null);
        var loco2 = plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", 2, null);
        // loco1: forward on 1,3,5 and return on 2,4,6
        plan.AssignVehicle(fwd, loco1, Sessions.FromSessionNumbers(1, 3, 5));
        plan.AssignVehicle(rtn, loco1, Sessions.FromSessionNumbers(2, 4, 6));
        // loco2: forward on 2,4,6 and return on 1,3,5
        plan.AssignVehicle(fwd, loco2, Sessions.FromSessionNumbers(2, 4, 6));
        plan.AssignVehicle(rtn, loco2, Sessions.FromSessionNumbers(1, 3, 5));

        var errors = plan.GetValidationErrors(Settings).ToList();

        Assert.IsEmpty(errors.Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed), "Each loco works both directions over the period, so its circulation closes.");
        Assert.IsEmpty(errors.Where(e => e.ErrorType is ValidationErrorType.ScheduleHasNoVehicle or ValidationErrorType.TrainMissingTraction), "Both directions have traction on every session.");
    }

    // Closure also applies to wagonsets and cargo-only units: they each turn on their own working and must
    // return to a repeatable circulation. Only cargo flows (waybill-directed freight) are exempt.

    [TestMethod]
    public void WagonsetThatDoesNotReturnIsReported()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);                              // G->Snu only
        var wagon = plan.CreateVehicle(ScheduledObjectType.Wagonset, "W", 1, null);
        plan.AssignVehicle(schedule, wagon);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed && e.Involves(wagon))
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void ClosedWagonsetCirculationIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var @return = plan.Timetable.Trains.First(t => t.Number == 2);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        schedule.Add(@return.AsTrainPart);                             // returns to G, closes
        var wagon = plan.CreateVehicle(ScheduledObjectType.Wagonset, "W", 1, null);
        plan.AssignVehicle(schedule, wagon);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed && e.Involves(wagon));

        Assert.IsEmpty(errors, "A wagonset that works both directions closes.");
    }

    [TestMethod]
    public void CargoOnlyUnitThatDoesNotReturnIsReported()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        var cargo = plan.CreateVehicle(ScheduledObjectType.Cargo, "C", 1, null);
        plan.AssignVehicle(schedule, cargo);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed && e.Involves(cargo))
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void CargoFlowIsExemptFromClosure()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        var cargoFlow = plan.CreateVehicle(ScheduledObjectType.CargoFlow, "CF", 1, null);
        plan.AssignVehicle(schedule, cargoFlow);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed && e.Involves(cargoFlow));

        Assert.IsEmpty(errors, "A cargo flow is directed by waybills, not a turning vehicle, so closure does not apply.");
    }

    [TestMethod]
    public void ClosureValidationCanBeSwitchedOff()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule);
        var settings = new ValidationSettings { ValidateSchedules = false };

        var errors = plan.GetValidationErrors(settings)
            .Where(e => e.ErrorType == ValidationErrorType.VehicleNotClosed);

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

    // --- Stretch conflicts: two trains meet on a single-track stretch only on a common session ---

    // Two trains occupy the same single-track stretch (G<->Yb) at the same clock time: the forward run
    // 12:00-12:25 and an opposing run that departs Yb 12:00 and arrives G 12:25.
    private static (Plan plan, Train forward, Train opposite) TrainsMeetingOnSingleTrackStretch()
    {
        var timetable = NewTimetable();
        var category = Category;
        var forward = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));  // G->Yb 12:00-12:25
        var opposite = TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(11, 30)); // Yb->G 12:00-12:25
        timetable.Add(forward);
        timetable.Add(opposite);
        return (Plan.Create("Test", timetable), forward, opposite);
    }

    [TestMethod]
    public void TrainsMeetingOnSingleTrackOnCommonSessionsIsReported()
    {
        var (plan, forward, opposite) = TrainsMeetingOnSingleTrackStretch();
        forward.Sessions = Sessions.FromSessionNumbers(1, 2, 3);
        opposite.Sessions = Sessions.FromSessionNumbers(3, 4, 5); // overlaps on session 3

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.TrackStretchConflict);

        Assert.IsNotEmpty(errors, "Two trains sharing a single-track stretch at the same time on a common session conflict.");
    }

    [TestMethod]
    public void TrainsMeetingOnSingleTrackOnDisjointSessionsIsAllowed()
    {
        var (plan, forward, opposite) = TrainsMeetingOnSingleTrackStretch();
        forward.Sessions = Sessions.FromSessionNumbers(1, 3, 5);
        opposite.Sessions = Sessions.FromSessionNumbers(2, 4, 6); // never runs on the same session as forward

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.TrackStretchConflict);

        Assert.IsEmpty(errors, "Trains that never share a session cannot meet, even though their clock times overlap.");
    }

    // A layout with a single double-track stretch G->Yb, and one G->Yb train per spec (departure, running
    // minutes, sessions). Two trains may share the double track at once; only a third overlapping train on a
    // common session is a conflict.
    private static Plan DoubleTrackPlan(params (int number, Time departure, int minutes, Sessions sessions)[] specs)
    {
        TestDataFactory.Init();
        var stations = TestDataFactory.Stations.ToArray();
        var g = stations[0];
        var yb = stations[1];
        var layout = new Layout { Name = "Test" };
        layout.Add(g);
        layout.Add(yb);
        var stretch = new TrackStretch(1, g, yb, 10, tracksCount: 2);
        layout.Add(stretch);
        var line = new TimetableStretch(1, "1");
        line.AddLast(stretch);
        layout.Add(line);
        var timetable = new Timetable("Test", layout);
        var gTrack = 1;
        var ybTrack = 1;
        foreach (var (number, departure, minutes, sessions) in specs)
        {
            var train = new Train(number, Category, number) { Category = Category, Sessions = sessions };
            // Distinct tracks so overlapping trains do not also raise station-track conflicts.
            _ = train.Add(new StationCall(2 * number - 1, g[gTrack.ToString()], departure, departure));
            _ = train.Add(new StationCall(2 * number, yb[ybTrack.ToString()], departure.AddMinutes(minutes), departure.AddMinutes(minutes)));
            timetable.Add(train);
            gTrack = gTrack % 4 + 1;
            ybTrack = ybTrack % 2 + 1;
        }
        return Plan.Create("Test", timetable);
    }

    private static IEnumerable<ValidationError> StretchConflicts(Plan plan) =>
        plan.GetValidationErrors(Settings).Where(e => e.ErrorType == ValidationErrorType.TrackStretchConflict);

    [TestMethod]
    public void TwoTrainsSharingADoubleTrackIsAllowed()
    {
        // Two trains overlap 12:05-12:25 on the double track — within its two-track capacity.
        var plan = DoubleTrackPlan(
            (1, Time.FromHourAndMinute(12, 00), 25, Sessions.All),
            (2, Time.FromHourAndMinute(12, 05), 25, Sessions.All));

        Assert.IsEmpty(StretchConflicts(plan), "Two trains fit on a double-track stretch.");
    }

    [TestMethod]
    public void ThreeTrainsSharingADoubleTrackOnACommonSessionIsReported()
    {
        // All three overlap around 12:10 on the double track and all run session 3: one too many.
        var plan = DoubleTrackPlan(
            (1, Time.FromHourAndMinute(12, 00), 25, Sessions.FromSessionNumbers(1, 3, 5)),
            (2, Time.FromHourAndMinute(12, 05), 25, Sessions.FromSessionNumbers(2, 3, 4)),
            (3, Time.FromHourAndMinute(12, 10), 25, Sessions.FromSessionNumbers(3, 6, 9)));

        Assert.IsNotEmpty(StretchConflicts(plan), "Three trains on a double track at once on a common session exceed capacity.");
    }

    [TestMethod]
    public void ThreeTrainsSharingADoubleTrackButNoCommonSessionIsAllowed()
    {
        // All three overlap around 12:10, but only trains 1 and 3 ever run together (odd sessions); train 2
        // runs even sessions. On any one session at most two trains share the stretch, so it stays in
        // capacity — the case the endpoint-only check used to flag.
        var plan = DoubleTrackPlan(
            (1, Time.FromHourAndMinute(12, 00), 25, Sessions.FromSessionNumbers(1, 3, 5)),
            (2, Time.FromHourAndMinute(12, 05), 25, Sessions.FromSessionNumbers(2, 4, 6)),
            (3, Time.FromHourAndMinute(12, 10), 25, Sessions.FromSessionNumbers(1, 3, 5)));

        Assert.IsEmpty(StretchConflicts(plan), "No session ever has three of these trains on the stretch at once.");
    }

    [TestMethod]
    public void ShortMiddleTrainDoesNotCauseAFalseDoubleTrackConflict()
    {
        // Train 2 has cleared the stretch (12:05-12:07) before train 3 departs at 12:10, so only trains 1 and
        // 3 share it then — two trains, within capacity. The endpoint-only check compared train 1 with train 3
        // and wrongly flagged it; the sweep does not.
        var plan = DoubleTrackPlan(
            (1, Time.FromHourAndMinute(12, 00), 60, Sessions.All),  // 12:00-13:00, long
            (2, Time.FromHourAndMinute(12, 05), 2, Sessions.All),   // 12:05-12:07, gone before 12:10
            (3, Time.FromHourAndMinute(12, 10), 25, Sessions.All)); // 12:10-12:35

        Assert.IsEmpty(StretchConflicts(plan), "Only two trains are ever on the stretch at once.");
    }

    [TestMethod]
    public void FourTrainsSharingADoubleTrackOnACommonSessionIsReported()
    {
        // Four trains overlap around 12:15 on a double track, all on session 2: well over capacity.
        var plan = DoubleTrackPlan(
            (1, Time.FromHourAndMinute(12, 00), 25, Sessions.FromSessionNumbers(2)),
            (2, Time.FromHourAndMinute(12, 05), 25, Sessions.FromSessionNumbers(2)),
            (3, Time.FromHourAndMinute(12, 10), 25, Sessions.FromSessionNumbers(2)),
            (4, Time.FromHourAndMinute(12, 15), 25, Sessions.FromSessionNumbers(2)));

        Assert.IsNotEmpty(StretchConflicts(plan), "Four trains cannot share a double-track stretch at once.");
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

    // --- Scope: each rule is targeted to the planning surface that resolves it -------------------

    [TestMethod]
    public void ScopeOfClassifiesEachErrorTypeBySurface()
    {
        Assert.AreEqual(ValidationScope.Train, ValidationError.ScopeOf(ValidationErrorType.StationTrackConflict));
        Assert.AreEqual(ValidationScope.Train, ValidationError.ScopeOf(ValidationErrorType.DuplicateTrainNumber));
        Assert.AreEqual(ValidationScope.Schedule, ValidationError.ScopeOf(ValidationErrorType.ScheduleNotContiguous));
        Assert.AreEqual(ValidationScope.Schedule, ValidationError.ScopeOf(ValidationErrorType.TrainMissingTraction));
        Assert.AreEqual(ValidationScope.Schedule, ValidationError.ScopeOf(ValidationErrorType.LocomotiveCoverageGap));
        Assert.AreEqual(ValidationScope.Vehicle, ValidationError.ScopeOf(ValidationErrorType.VehicleDoubleBooked));
        Assert.AreEqual(ValidationScope.Vehicle, ValidationError.ScopeOf(ValidationErrorType.VehicleNotClosed));
    }

    // A vehicle-scope error marks its own vehicle chip only — never the parts column or another vehicle.
    [TestMethod]
    public void VehicleScopeErrorMarksItsVehicleNotSchedule()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        var loco = AddLoco(plan, schedule);                             // G->Snu only, does not return
        var other = plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", 2, null);

        var error = plan.GetValidationErrors(Settings)
            .Single(e => e.ErrorType == ValidationErrorType.VehicleNotClosed);

        Assert.AreEqual(ValidationScope.Vehicle, error.Scope);
        Assert.IsTrue(error.Involves(loco), "The non-closing vehicle is marked.");
        Assert.IsFalse(error.Involves(other), "An unrelated vehicle is not marked.");
        Assert.IsFalse(error.Involves(schedule), "A vehicle-scope error must not mark the parts column.");
    }

    // A schedule-keyed error marks exactly its own schedule's parts column — not another schedule.
    [TestMethod]
    public void ScheduleScopeErrorMarksItsScheduleOnly()
    {
        var plan = PlanWithTwoForwardTrains();
        var first = plan.Timetable.Trains.First(t => t.Number == 1);
        var second = plan.Timetable.Trains.First(t => t.Number == 3);
        var schedule = plan.CreateSchedule();
        schedule.Add(first.AsTrainPart);
        schedule.Add(second.AsTrainPart);                               // non-contiguous
        var other = plan.CreateSchedule();

        var error = plan.GetValidationErrors(Settings)
            .Single(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous);

        Assert.AreEqual(ValidationScope.Schedule, error.Scope);
        Assert.IsTrue(error.Involves(schedule), "The non-contiguous schedule is marked.");
        Assert.IsFalse(error.Involves(other), "Another schedule is not marked.");
    }

    // A train-keyed schedule-scope error (missing traction) marks every schedule that works the train.
    [TestMethod]
    public void TrainKeyedScheduleScopeErrorMarksSchedulesWorkingTheTrain()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        forward.Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);
        AddLoco(plan, schedule, Sessions.FromSessionNumbers(1, 2));      // 3,4 uncovered

        var error = plan.GetValidationErrors(Settings)
            .Single(e => e.ErrorType == ValidationErrorType.TrainMissingTraction && e.Involves(forward));

        Assert.AreEqual(ValidationScope.Schedule, error.Scope);
        Assert.IsTrue(error.Involves(schedule), "A schedule working the under-covered train is marked.");
    }
}
