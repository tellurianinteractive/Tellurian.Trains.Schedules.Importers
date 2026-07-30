using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class DriverDutyTests
{
    private static readonly ValidationSettings Settings = new();

    // Forward 12:00 G->Yb->Snu (12:55), return 13:00 Snu->Yb->G (13:55).
    private static Plan CreatePlan()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        return Plan.Create("Test", timetable);
    }

    private static Train Forward(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 1);
    private static Train Return(Plan plan) => plan.Timetable.Trains.First(t => t.Number == 2);

    // Creates a schedule holding the given part and assigns a fresh locomotive to it; returns the part.
    private static ScheduledTrainPart AddLocoWorking(Plan plan, ScheduledTrainPart part, int locoNumber, Sessions? sessions = null)
    {
        var schedule = plan.CreateSchedule();
        var added = schedule.Add(part);
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", locoNumber, null);
        plan.AssignVehicle(schedule, loco, sessions);
        return added;
    }

    [TestMethod]
    public void CreateDriverDutyAddsAnEmptyDutyToThePlan()
    {
        var plan = CreatePlan();

        var duty = plan.CreateDriverDuty();

        Assert.HasCount(1, plan.DriverDuties);
        Assert.IsEmpty(duty.Parts);
        Assert.AreEqual(Sessions.All, duty.Sessions);
        Assert.AreEqual(duty, plan.DriverDuties.Single());
    }

    [TestMethod]
    public void CandidatePartsAreTractionPartsOnly()
    {
        var plan = CreatePlan();
        var locoPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        // A wagonset working of the return train: its part must not be offered (a driver drives traction).
        var wagonSchedule = plan.CreateSchedule();
        var wagonPart = wagonSchedule.Add(Return(plan).AsTrainPart);
        plan.AssignVehicle(wagonSchedule, plan.CreateVehicle(ScheduledObjectType.Wagonset, "W", 1, null));

        var candidates = plan.CandidatePartsFor(plan.CreateDriverDuty());

        Assert.Contains(locoPart, candidates);
        Assert.DoesNotContain(wagonPart, candidates);
    }

    [TestMethod]
    public void CandidatePartsForNonEmptyDutyAreLaterContinuations()
    {
        var plan = CreatePlan();
        var forwardPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);   // ends Snu 12:55
        var returnPart = AddLocoWorking(plan, Return(plan).AsTrainPart, locoNumber: 2);      // departs Snu 13:00
        var duty = plan.CreateDriverDuty();
        duty.Append(forwardPart);

        var candidates = plan.CandidatePartsFor(duty);

        Assert.Contains(returnPart, candidates, "The return departs at 13:00, after the duty arrives at 12:55.");
        Assert.DoesNotContain(forwardPart, candidates, "A part already in the duty is not offered again.");
    }

    [TestMethod]
    public void AppendRejectsOverlappingParts()
    {
        var plan = CreatePlan();
        var forwardPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);      // 12:00-12:55
        // Another forward-direction working of the same run overlaps it in time.
        var overlappingPart = AddLocoWorking(plan, Forward(plan).AsTrainPart(0, 1), locoNumber: 2); // 12:00-12:25
        var duty = plan.CreateDriverDuty();
        duty.Append(forwardPart);

        var result = duty.Append(overlappingPart);

        Assert.IsTrue(result.IsNone, "Overlapping parts cannot both be in one duty.");
        Assert.HasCount(1, duty.Parts);
    }

    [TestMethod]
    public void APartMayBeInSeveralDutiesOnDisjointSessionsButNotOverlappingOnes()
    {
        var plan = CreatePlan();
        var part = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);

        var oddDuty = plan.CreateDriverDuty();
        oddDuty.Sessions = Sessions.FromSessionNumbers(1, 3);
        var evenDuty = plan.CreateDriverDuty();
        evenDuty.Sessions = Sessions.FromSessionNumbers(2, 4);
        var clashingDuty = plan.CreateDriverDuty();
        clashingDuty.Sessions = Sessions.FromSessionNumbers(1, 2);

        Assert.IsTrue(oddDuty.Append(part).HasValue, "First duty takes the part on sessions 1,3.");
        Assert.IsTrue(evenDuty.Append(part).HasValue, "A second duty may take the same part on disjoint sessions 2,4.");
        Assert.IsTrue(clashingDuty.Append(part).IsNone, "A duty may not take a part another duty runs on a shared session.");
    }

    [TestMethod]
    public void CandidatePartsExcludePartsTakenByAnotherDutyOnAnOverlappingSession()
    {
        var plan = CreatePlan();
        var part = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var taken = plan.CreateDriverDuty();
        taken.Sessions = Sessions.FromSessionNumbers(1, 2);
        taken.Append(part);
        var building = plan.CreateDriverDuty();
        building.Sessions = Sessions.FromSessionNumbers(2, 3); // overlaps session 2

        Assert.DoesNotContain(part, plan.CandidatePartsFor(building));
    }

    [TestMethod]
    public void TractionExchangeNoteIsDerivedWhenTheSameTrainChangesTractionUnit()
    {
        var plan = CreatePlan();
        var firstLeg = AddLocoWorking(plan, Forward(plan).AsTrainPart(0, 1), locoNumber: 1);  // G->Yb, loco 1
        var secondLeg = AddLocoWorking(plan, Forward(plan).AsTrainPart(1, 2), locoNumber: 2); // Yb->Snu, loco 2
        var duty = plan.CreateDriverDuty();
        duty.Append(firstLeg);
        duty.Append(secondLeg);

        var notes = duty.TractionExchangeNotes.ToList();

        Assert.HasCount(1, notes);
        Assert.AreEqual("Yb", notes[0].At.Signature, ignoreCase: true);
    }

    [TestMethod]
    public void NoTractionExchangeNoteWhenTheSameTractionUnitWorksBothLegs()
    {
        var plan = CreatePlan();
        // One loco schedule holds both legs of the forward train, so the same unit works throughout.
        var schedule = plan.CreateSchedule();
        var firstLeg = schedule.Add(Forward(plan).AsTrainPart(0, 1));
        var secondLeg = schedule.Add(Forward(plan).AsTrainPart(1, 2));
        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", 1, null));
        var duty = plan.CreateDriverDuty();
        duty.Append(firstLeg);
        duty.Append(secondLeg);

        Assert.IsEmpty(duty.TractionExchangeNotes);
    }

    [TestMethod]
    public void DoubleAssignedPartOnOverlappingSessionsIsReported()
    {
        var plan = CreatePlan();
        var part = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var d1 = plan.CreateDriverDuty();
        d1.Sessions = Sessions.FromSessionNumbers(1, 2);
        d1.Add(part); // unguarded add so the conflict reaches validation
        var d2 = plan.CreateDriverDuty();
        d2.Sessions = Sessions.FromSessionNumbers(2, 3);
        d2.Add(part);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DutyPartDoubleAssigned)
            .ToList();

        Assert.HasCount(1, errors);
        Assert.AreEqual(ValidationScope.Duty, errors[0].Scope);
    }

    [TestMethod]
    public void DoubleAssignedPartOnDisjointSessionsIsAllowed()
    {
        var plan = CreatePlan();
        var part = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var odd = plan.CreateDriverDuty();
        odd.Sessions = Sessions.FromSessionNumbers(1, 3);
        odd.Add(part);
        var even = plan.CreateDriverDuty();
        even.Sessions = Sessions.FromSessionNumbers(2, 4);
        even.Add(part);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DutyPartDoubleAssigned);

        Assert.IsEmpty(errors);
    }

    // --- Driver duty coverage: a traction-assigned part must have a driver duty on every session it runs ---

    [TestMethod]
    public void TractionPartWithNoDriverDutyIsReported()
    {
        var plan = CreatePlan();
        AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1); // traction on all sessions, no duty

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.TrainPartMissingDriverDuty)
            .ToList();

        Assert.HasCount(1, errors);
        Assert.AreEqual(ValidationScope.Duty, errors[0].Scope);
        Assert.Contains(Forward(plan), errors[0].Trains);
    }

    [TestMethod]
    public void TractionPartCoveredOnAllItsSessionsIsNotReported()
    {
        var plan = CreatePlan();
        var part = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var duty = plan.CreateDriverDuty();
        duty.Append(part);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.TrainPartMissingDriverDuty);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void TractionPartCoveredOnOnlySomeSessionsReportsTheRemainder()
    {
        var plan = CreatePlan();
        var part = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1); // traction on all sessions
        var duty = plan.CreateDriverDuty();
        duty.Sessions = Sessions.FromSessionNumbers(1, 2); // driven on 1,2 only
        duty.Add(part); // unguarded add so the plan can hold a partial-coverage duty for validation

        var error = plan.GetValidationErrors(Settings)
            .Single(e => e.ErrorType == ValidationErrorType.TrainPartMissingDriverDuty);

        Assert.AreEqual(ValidationScope.Duty, error.Scope);
        StringAssert.Contains(error.Message.Text, "3");
        StringAssert.Contains(error.Message.Text, "14", "The default operating period runs to session 14, so the uncovered sessions include the top of the range.");
    }

    [TestMethod]
    public void WagonPartWithNoDriverDutyIsNotReported()
    {
        var plan = CreatePlan();
        var wagonSchedule = plan.CreateSchedule();
        wagonSchedule.Add(Forward(plan).AsTrainPart);
        plan.AssignVehicle(wagonSchedule, plan.CreateVehicle(ScheduledObjectType.Wagonset, "W", 1, null));

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.TrainPartMissingDriverDuty);

        Assert.IsEmpty(errors, "A driver drives traction, not wagons; a wagon-only part needs no duty.");
    }

    [TestMethod]
    public void DutyStartAndEndTimesDeriveFromFirstArrivalAndLastDeparture()
    {
        var plan = CreatePlan();
        var forwardPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var duty = plan.CreateDriverDuty();
        duty.Append(forwardPart);

        // The duty starts when the driver reports at the origin (first-call arrival) and ends when they
        // stand down at the destination (last-call departure) — not the movement departure/arrival.
        Assert.AreEqual(forwardPart.From.Arrival, duty.StartTime!.Value);
        Assert.AreEqual(forwardPart.To.Departure, duty.EndTime!.Value);
    }

    [TestMethod]
    public void OverriddenTimesTakePrecedenceOverTheDerivedOnes()
    {
        var plan = CreatePlan();
        var forwardPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var duty = plan.CreateDriverDuty();
        duty.Append(forwardPart);

        duty.OverriddenStartTime = Time.FromHourAndMinute(6, 30);
        duty.OverriddenEndTime = Time.FromHourAndMinute(22, 15);

        Assert.AreEqual(Time.FromHourAndMinute(6, 30), duty.StartTime!.Value);
        Assert.AreEqual(Time.FromHourAndMinute(22, 15), duty.EndTime!.Value);
    }

    [TestMethod]
    public void RenumberOrdersDutiesByStartThenEndTimeWithEmptyOnesLast()
    {
        var plan = CreatePlan();
        var forwardPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1); // starts ~12:00
        var returnPart = AddLocoWorking(plan, Return(plan).AsTrainPart, locoNumber: 2);   // starts ~13:00
        // Create them out of running order to prove the renumber sorts by time, not creation order.
        var empty = plan.CreateDriverDuty();
        var later = plan.CreateDriverDuty();
        later.Append(returnPart);
        var earlier = plan.CreateDriverDuty();
        earlier.Append(forwardPart);

        plan.RenumberDriverDuties();

        Assert.AreEqual("1", earlier.Identity);
        Assert.AreEqual("2", later.Identity);
        Assert.AreEqual("3", empty.Identity, "An empty duty has no start time and sorts last.");
    }

    [TestMethod]
    public void RenumberBreaksStartEndTiesByFirstSessionNumber()
    {
        var plan = CreatePlan();
        // Two variants of the same working (identical start and end times) split across alternating sessions.
        var oddPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1, sessions: Sessions.FromSessionNumbers(1, 3, 5));
        var evenPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 2, sessions: Sessions.FromSessionNumbers(2, 4, 6));
        var evenDuty = plan.CreateDriverDuty();
        evenDuty.Sessions = Sessions.FromSessionNumbers(2, 4, 6);
        evenDuty.Append(evenPart);
        var oddDuty = plan.CreateDriverDuty();
        oddDuty.Sessions = Sessions.FromSessionNumbers(1, 3, 5);
        oddDuty.Append(oddPart);

        plan.RenumberDriverDuties();

        Assert.AreEqual("1", oddDuty.Identity, "The 1,3,5 variant has the lower first session, so it sorts first.");
        Assert.AreEqual("2", evenDuty.Identity);
    }

    [TestMethod]
    public void RenumberLeavesAPinnedDutyAloneAndReservesItsNumber()
    {
        var plan = CreatePlan();
        var forwardPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1); // starts ~12:00
        var returnPart = AddLocoWorking(plan, Return(plan).AsTrainPart, locoNumber: 2);   // starts ~13:00
        var earlier = plan.CreateDriverDuty();
        earlier.Append(forwardPart);
        var pinned = plan.CreateDriverDuty();
        pinned.Append(returnPart);
        pinned.Identity = "1";
        pinned.IsExcludedFromRenumbering = true;

        plan.RenumberDriverDuties();

        Assert.AreEqual("1", pinned.Identity, "A pinned duty keeps its number.");
        Assert.AreEqual("2", earlier.Identity,
            "The ordinal walk steps over the reserved 1, so the first renumbered duty becomes 2 rather than colliding.");
    }

    [TestMethod]
    public void RenumberIgnoresANonNumericPinnedIdentity()
    {
        var plan = CreatePlan();
        var forwardPart = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var pinned = plan.CreateDriverDuty();
        pinned.Identity = "L1";
        pinned.IsExcludedFromRenumbering = true;
        var ordinary = plan.CreateDriverDuty();
        ordinary.Append(forwardPart);

        plan.RenumberDriverDuties();

        Assert.AreEqual("L1", pinned.Identity);
        Assert.AreEqual("1", ordinary.Identity, "A non-numeric pin reserves nothing, so numbering starts at 1.");
    }

    [TestMethod]
    public void PrintOrderIsNumericWithNonNumericIdentitiesLast()
    {
        var plan = CreatePlan();
        var ten = plan.CreateDriverDuty();
        ten.Identity = "10";
        var two = plan.CreateDriverDuty();
        two.Identity = "2";
        var post = plan.CreateDriverDuty();
        post.Identity = "Post";
        var loco = plan.CreateDriverDuty();
        loco.Identity = "L1";

        var order = string.Join(",", plan.DriverDutiesInPrintOrder.Select(d => d.Identity));

        // String ordering would put "10" before "2"; the pile is kept in numeric order.
        Assert.AreEqual("2,10,L1,Post", order);
    }

    [TestMethod]
    public void TwoPinnedDutiesSharingAnIdentityAreReported()
    {
        var plan = CreatePlan();
        var a = plan.CreateDriverDuty();
        a.Identity = "7";
        a.IsExcludedFromRenumbering = true;
        var b = plan.CreateDriverDuty();
        b.Identity = "7";
        b.IsExcludedFromRenumbering = true;

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DutyIdentityDuplicated)
            .ToList();

        Assert.HasCount(1, errors);
        Assert.AreEqual(ValidationScope.Duty, errors[0].Scope);
        Assert.IsNull(errors[0].FromTrack, "A numbering fault has no place on the layout to highlight.");
    }

    [TestMethod]
    public void TwoUnpinnedDutiesSharingAnIdentityAreNotReported()
    {
        var plan = CreatePlan();
        var a = plan.CreateDriverDuty();
        a.Identity = "7";
        var b = plan.CreateDriverDuty();
        b.Identity = "7";

        // Renumbering will resolve this on its own, so it is not worth reporting.
        Assert.IsEmpty(plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DutyIdentityDuplicated));
    }

    [TestMethod]
    public void APinnedDutyWithNoIdentityIsReported()
    {
        var plan = CreatePlan();
        var duty = plan.CreateDriverDuty();
        duty.Identity = "";
        duty.IsExcludedFromRenumbering = true;

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DutyIdentityMissing);

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void StaffCountDefaultsToOneAndIsReportedOutsideItsRange()
    {
        var plan = CreatePlan();
        var duty = plan.CreateDriverDuty();

        Assert.AreEqual(1, duty.StaffCount, "A new duty is worked by one loco driver.");
        Assert.IsEmpty(plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DutyStaffCountOutOfRange));

        duty.StaffCount = 4;

        Assert.HasCount(1, plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DutyStaffCountOutOfRange));
    }

    [TestMethod]
    public void StaffCountSurvivesAPlanSavedBeforeThePropertyExisted()
    {
        var plan = CreatePlan();
        plan.CreateDriverDuty();
        var options = new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve, MaxDepth = 256 };
        // Strip the property from the JSON to stand in for a plan saved before it existed.
        var json = JsonSerializer.Serialize(plan, options).Replace("\"StaffCount\":1,", "", StringComparison.Ordinal);

        var restored = JsonSerializer.Deserialize<Plan>(json, options)!;

        Assert.AreEqual(1, restored.DriverDuties.Single().StaffCount,
            "The property initialiser holds, so an older plan does not load as zero people.");
    }

    [TestMethod]
    public void DifficultyIsUnsetUntilGraded()
    {
        var plan = CreatePlan();
        var duty = plan.CreateDriverDuty();

        Assert.IsNull(duty.Difficulty, "An ungraded duty must not masquerade as the easiest grade.");

        duty.Difficulty = DutyDifficulty.Experienced;

        Assert.AreEqual(DutyDifficulty.Experienced, duty.Difficulty);
    }

    [TestMethod]
    public void DutyAndScheduleSharePartInstanceThroughJsonRoundTrip()
    {
        var plan = CreatePlan();
        var part = AddLocoWorking(plan, Forward(plan).AsTrainPart, locoNumber: 1);
        var duty = plan.CreateDriverDuty();
        duty.Append(part);
        duty.AddNote("Take the mail bag at Yb.");
        var options = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve, MaxDepth = 256 };

        var json = JsonSerializer.Serialize(plan, options);
        var restored = JsonSerializer.Deserialize<Plan>(json, options)!;

        var restoredDuty = restored.DriverDuties.Single();
        var restoredDutyPart = restoredDuty.Parts.Single();
        var restoredSchedulePart = restored.Schedules.Single().Parts.Single();
        Assert.AreSame(restoredSchedulePart, restoredDutyPart, "The duty must reference the same part instance the schedule owns.");
        Assert.AreEqual("Take the mail bag at Yb.", restoredDuty.Notes.Single().Text);
    }
}
