using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class ScheduleEditingTests
{
    // Forward 12:00 G→Yb→Snu (12:55), return 13:00 Snu→Yb→G (13:55): the return continues the forward run.
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

    [TestMethod]
    public void CreateScheduleAddsAnEmptyScheduleToThePlan()
    {
        var plan = CreatePlan();

        var schedule = plan.CreateSchedule();

        Assert.HasCount(1, plan.Schedules);
        Assert.IsEmpty(schedule.Parts);
        Assert.AreEqual(0, schedule.Number, "Number stays 0 until the first part is appended.");
        Assert.AreEqual(schedule, plan.Schedules.Single());
    }

    [TestMethod]
    public void CreateScheduleAllocatesDistinctIds()
    {
        var plan = CreatePlan();

        var first = plan.CreateSchedule();
        var second = plan.CreateSchedule();

        Assert.AreNotEqual(first.Id, second.Id);
    }

    [TestMethod]
    public void CandidateTrainsForEmptyScheduleAreAllSchedulableTrains()
    {
        var plan = CreatePlan();

        var candidates = plan.CandidateTrainsFor(plan.CreateSchedule());

        Assert.HasCount(2, candidates);
    }

    [TestMethod]
    public void CandidateTrainsForWholeForwardScheduleIsOnlyTheReturn()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart); // G→Snu, arrives 12:55

        var candidates = plan.CandidateTrainsFor(schedule);

        Assert.HasCount(1, candidates);
        Assert.AreEqual(2, candidates[0].Number, "Only the return train joins at Snu at/after 12:55; the forward train's only Snu call is its last.");
    }

    [TestMethod]
    public void CandidateTrainsForPartialScheduleAllowsJoiningATrainMidRun()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart(0, 1)); // G→Yb, arrives 12:25

        var candidates = plan.CandidateTrainsFor(schedule);

        // The forward train can be rejoined at its middle call (Yb 12:30), continuing to Snu — the split case.
        Assert.Contains(Forward(plan), candidates);
        Assert.AreEqual(1, schedule.JoinCallIndexFor(Forward(plan)), "The forward train joins at its Yb call (index 1).");
    }

    [TestMethod]
    public void AppendingThePartialContinuationRebuildsTheWholeRun()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart(0, 1)); // G→Yb

        var result = schedule.Append(Forward(plan).AsTrainPart(1, 2)); // Yb→Snu

        Assert.IsTrue(result.HasValue, result.Message);
        Assert.HasCount(2, schedule.Parts);
        Assert.AreEqual("Snu", schedule.EndLocation!.Signature, ignoreCase: true);
    }

    [TestMethod]
    public void JoinCallIndexIsNullWhenTheTrainCannotContinueTheSchedule()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart); // ends at Snu 12:55

        Assert.IsNull(schedule.JoinCallIndexFor(Forward(plan)), "Its only Snu call is the last, so it cannot continue.");
    }

    [TestMethod]
    public void CreateVehicleAddsToPoolWithNoExternalId()
    {
        var plan = CreatePlan();

        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 12, null);

        Assert.HasCount(1, plan.ScheduledObjects);
        Assert.AreEqual(12, vehicle.Number);
        Assert.IsFalse(vehicle.HasExternalId, "An external id is the identifier a vehicle was imported under.");
        Assert.AreEqual("12 BR 218", vehicle.Designation, "Its designation is composed instead.");
        Assert.AreEqual(VehicleIdentity.Of(null, 12), vehicle.Identity, "So operator and number identify it.");
    }

    [TestMethod]
    public void CreateVehicleFallsBackToItsIdWhenNumberIsZero()
    {
        var plan = CreatePlan();

        var vehicle = plan.CreateVehicle(ScheduledObjectType.Wagonset, null, 0, null);

        Assert.AreEqual(vehicle.Id, vehicle.Number, "A vehicle with no number falls back to its unique id.");
    }

    // --- The editor guard behind rule P5: an identity may name only one vehicle per session ---

    [TestMethod]
    public void VehicleClaimingFindsAVehicleWithTheSameOperatorAndNumber()
    {
        var plan = CreatePlan();
        var db = new Company(1, "Deutsche Bahn", "DB");
        var existing = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 5, db);

        Assert.AreEqual(existing, plan.VehicleClaiming(VehicleIdentity.Of(db.Id, 5), Sessions.All));
    }

    [TestMethod]
    public void VehicleClaimingFindsAVehicleOfAnotherType()
    {
        var plan = CreatePlan();
        var wagonset = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 5, null);

        Assert.AreEqual(wagonset, plan.VehicleClaiming(VehicleIdentity.Of(null, 5), Sessions.All),
            "A locomotive may not take the operator and number a wagonset already has.");
    }

    [TestMethod]
    public void VehicleClaimingIgnoresTheVehicleBeingEdited()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 5, null);

        Assert.IsNull(plan.VehicleClaiming(VehicleIdentity.Of(null, 5), Sessions.All, excluding: vehicle),
            "A vehicle may of course keep its own identity.");
    }

    [TestMethod]
    public void VehicleClaimingIgnoresAVehicleWorkingOtherSessions()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        var existing = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 5, null);
        plan.AssignVehicle(schedule, existing, Sessions.FromSessionNumbers(1, 2));

        Assert.IsNull(plan.VehicleClaiming(VehicleIdentity.Of(null, 5), Sessions.FromSessionNumbers(3, 4)),
            "The identity is free on the sessions the other vehicle does not work.");
        Assert.AreEqual(existing, plan.VehicleClaiming(VehicleIdentity.Of(null, 5), Sessions.FromSessionNumbers(2, 3)),
            "It is taken as soon as one session is shared.");
    }

    [TestMethod]
    public void VehicleClaimingIgnoresADifferentOperator()
    {
        var plan = CreatePlan();
        plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 5, new Company(1, "Deutsche Bahn", "DB"));

        Assert.IsNull(plan.VehicleClaiming(VehicleIdentity.Of(operatorId: 2, 5), Sessions.All));
        Assert.IsNull(plan.VehicleClaiming(VehicleIdentity.Of(operatorId: null, 5), Sessions.All),
            "A vehicle with no operator is a different identity from one with an operator.");
    }

    [TestMethod]
    public void VehicleClaimingIgnoresTheOperatorAndNumberOfAnImportedVehicle()
    {
        var plan = CreatePlan();
        var imported = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 5, null);
        imported.ExternalId = "DBSCH EG 01"; // as the import sets it

        Assert.IsNull(plan.VehicleClaiming(VehicleIdentity.Of(null, 5), Sessions.All),
            "A vehicle carrying an external id is identified by that, not by its operator and number.");
        Assert.AreEqual(imported, plan.VehicleClaiming(VehicleIdentity.Of("dbsch eg 01", null, 0), Sessions.All),
            "An external id identifies case-insensitively.");
    }

    [TestMethod]
    public void UpdateVehicleKeepsAGivenExternalId()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 12, null);

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Locomotive, "DBSCH EG 01", "EG", 1, null);

        Assert.AreEqual("DBSCH EG 01", vehicle.ExternalId);
        Assert.AreEqual("DBSCH EG 01", vehicle.Designation, "The external id is shown when present.");
    }

    [TestMethod]
    public void UpdateVehicleSetsTractionTypeAndNumberOfUnitsForATractionUnit()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Rc", 6, null);

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Locomotive, "Rc 6", "Rc", 6, null, TractionType.Electric, 2);

        Assert.AreEqual(TractionType.Electric, vehicle.TractionType);
        Assert.AreEqual(2, vehicle.NumberOfUnits);
    }

    [TestMethod]
    public void UpdateVehicleClampsNumberOfUnitsToAtLeastOne()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 1, null);

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Wagonset, "Gbs", "Gbs", 1, null, numberOfUnits: 0);

        Assert.AreEqual(1, vehicle.NumberOfUnits);
    }

    [TestMethod]
    public void UpdateVehicleLeavesTractionTypeNoneForAWagonset()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 1, null);

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Wagonset, "Gbs", "Gbs", 1, null, TractionType.Electric, 3);

        Assert.AreEqual(TractionType.None, vehicle.TractionType, "A wagonset is never a traction unit.");
    }

    [TestMethod]
    public void UpdateVehicleSetsReversibleTrainForALocomotive()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Rc", 6, null);

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Locomotive, "Rc 6", "Rc", 6, null, isReversibleTrain: true);

        Assert.IsTrue(vehicle.IsReversibleTrain);
        Assert.IsTrue(vehicle.ReversesWithoutRunaround);
    }

    [TestMethod]
    public void UpdateVehicleLeavesReversibleTrainClearForATrainset()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Trainset, "X2", 1, null);

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Trainset, "X2", "X2", 1, null, isReversibleTrain: true);

        Assert.IsFalse(vehicle.IsReversibleTrain, "Only a locomotive can be spared the runaround by one.");
        Assert.IsTrue(vehicle.ReversesWithoutRunaround, "A trainset turns round as it stands regardless.");
    }

    [TestMethod]
    public void UpdateVehicleDropsReversibleTrainWhenTheTypeIsNoLongerALocomotive()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Rc", 6, null);
        plan.UpdateVehicle(vehicle, ScheduledObjectType.Locomotive, "Rc 6", "Rc", 6, null, isReversibleTrain: true);

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Wagonset, "Gbs", "Gbs", 6, null, isReversibleTrain: true);

        Assert.IsFalse(vehicle.IsReversibleTrain);
        Assert.IsFalse(vehicle.ReversesWithoutRunaround, "A wagonset is not traction at all.");
    }

    [TestMethod]
    public void UpdateVehicleClearsWagonsWhenTypeIsNotWagonset()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 1, null);
        vehicle.AddWagon("Gbs");
        vehicle.AddWagon("Habis");

        plan.UpdateVehicle(vehicle, ScheduledObjectType.Locomotive, "Rc 6", "Rc", 6, null);

        Assert.IsEmpty(vehicle.Units, "The wagon rake belongs only to a wagonset.");
    }

    [TestMethod]
    public void AddAndRemoveWagonKeepOrderInTrainContiguous()
    {
        var plan = CreatePlan();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 1, null);

        var first = vehicle.AddWagon("A");
        var second = vehicle.AddWagon("B");
        var third = vehicle.AddWagon("C");
        Assert.AreEqual(1, first.Position);
        Assert.AreEqual(2, second.Position);
        Assert.AreEqual(3, third.Position);

        vehicle.RemoveUnit(second);

        Assert.HasCount(2, vehicle.Units);
        CollectionAssert.AreEqual(new[] { 1, 2 }, vehicle.Units.OrderBy(w => w.Position).Select(w => w.Position).ToArray());
        Assert.AreEqual("C", vehicle.Units.OrderBy(w => w.Position).Last().Class);
    }

    [TestMethod]
    public void WagonsOnAWagonsetSurviveJsonRoundTrip()
    {
        var plan = CreatePlan();
        var wagonset = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 1, null);
        wagonset.AddWagon("Gbs", "1234", isCargo: true);
        wagonset.AddWagon("Habis", isCargo: true);

        // The same options the app's JsonExportService/JsonImportService use.
        var options = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve, MaxDepth = 256 };
        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, options), options)!;

        var restoredWagonset = restored.ScheduledObjects.Single(v => v.IsWagonSet);
        var wagons = restoredWagonset.Units.OrderBy(w => w.Position).ToList();
        Assert.HasCount(2, wagons);
        Assert.AreEqual("Gbs", wagons[0].Class);
        Assert.AreEqual("1234", wagons[0].Number);
        Assert.AreEqual(2, wagons[1].Position);
    }

    [TestMethod]
    public void VehiclesWithBlankExternalIdStayDistinctById()
    {
        var plan = CreatePlan();
        var one = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        var two = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 2, null);

        plan.UpdateVehicle(one, ScheduledObjectType.Locomotive, null, "BR 218", 1, null);
        plan.UpdateVehicle(two, ScheduledObjectType.Locomotive, null, "BR 218", 2, null);

        Assert.AreNotEqual(one, two, "Two vehicles with blank external ids must not merge.");
        Assert.AreNotEqual(one.GetHashCode(), two.GetHashCode());
        Assert.HasCount(2, plan.ScheduledObjects);
    }

    [TestMethod]
    public void AssignVehicleCreatesAnAssignmentAndAppearsInScheduleVehicles()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);

        var result = plan.AssignVehicle(schedule, vehicle);

        Assert.IsTrue(result.HasValue);
        Assert.HasCount(1, vehicle.ScheduleAssignments);
        Assert.Contains(vehicle, schedule.Vehicles.ToList());
    }

    [TestMethod]
    public void AssignVehicleIsIdempotentForTheSameVehicleAndSchedule()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);

        plan.AssignVehicle(schedule, vehicle);
        plan.AssignVehicle(schedule, vehicle);

        Assert.HasCount(1, vehicle.ScheduleAssignments, "The same vehicle is not assigned twice to one schedule.");
    }

    [TestMethod]
    public void RemovePartRemovesOnlyThatPartAndKeepsTheOthers()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        var first = schedule.Append(Forward(plan).AsTrainPart(0, 1)).Value; // G→Yb
        schedule.Append(Forward(plan).AsTrainPart(1, 2)); // Yb→Snu

        schedule.RemovePart(first);

        Assert.HasCount(1, schedule.Parts, "Only the removed part is gone; the rest stay.");
        Assert.AreEqual("Yb", schedule.StartLocation!.Signature, ignoreCase: true);
        Assert.IsNull(first.Schedule, "The removed part is detached.");
        Assert.IsNull(first.ScheduleId);
    }

    [TestMethod]
    public void TruncateFromRemovesThePartAndEverythingAfterIt()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart(0, 1)); // G→Yb
        var second = schedule.Append(Forward(plan).AsTrainPart(1, 2)).Value; // Yb→Snu

        schedule.TruncateFrom(second);

        Assert.HasCount(1, schedule.Parts);
        Assert.AreEqual("Yb", schedule.EndLocation!.Signature, ignoreCase: true);
    }

    [TestMethod]
    public void TryDeleteScheduleRemovesItAndUnassignsItsVehicles()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, vehicle);

        var result = plan.TryDelete(schedule);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsEmpty(plan.Schedules);
        Assert.IsEmpty(vehicle.ScheduleAssignments, "The vehicle is unassigned but stays in the pool.");
        Assert.HasCount(1, plan.ScheduledObjects);
    }

    [TestMethod]
    public void AllSessionsCoverTheWholePeriodAndLeaveNothingFree()
    {
        Assert.IsTrue(Sessions.All.CoversAllWithin(useDays: false, maxSessions: 14));
        Assert.IsEmpty(Sessions.All.ComplementWithin(useDays: false, maxSessions: 14).Numbers);
    }

    [TestMethod]
    public void OddSessionsAreFreeOnTheEvenSessions()
    {
        var odd = Sessions.FromBitPattern(CommonSessionPatterns.Odd);

        Assert.IsFalse(odd.CoversAllWithin(useDays: false, maxSessions: 14));
        CollectionAssert.AreEqual(new byte[] { 2, 4, 6, 8, 10, 12, 14 },
            odd.ComplementWithin(useDays: false, maxSessions: 14).Numbers);
    }

    [TestMethod]
    public void AssignedSessionsIsTheUnionAcrossAVehiclesAssignments()
    {
        var plan = CreatePlan();
        var odd = plan.CreateSchedule();
        odd.Append(Forward(plan).AsTrainPart);
        var even = plan.CreateSchedule();
        even.Append(Return(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);

        plan.AssignVehicle(odd, vehicle, Sessions.FromBitPattern(CommonSessionPatterns.Odd));
        Assert.IsFalse(vehicle.AssignedSessions.CoversAllWithin(false, 14), "Only odd so far — even is still free.");

        plan.AssignVehicle(even, vehicle, Sessions.FromBitPattern(CommonSessionPatterns.Even));
        Assert.IsTrue(vehicle.AssignedSessions.CoversAllWithin(false, 14), "Odd plus even covers the whole period.");
    }

    [TestMethod]
    public void EffectiveSessionsOfAnEmptyScheduleIsAllSessions()
    {
        var plan = CreatePlan();

        var schedule = plan.CreateSchedule();

        Assert.IsTrue(schedule.EffectiveSessions.CoversAllWithin(useDays: false, maxSessions: 14));
    }

    [TestMethod]
    public void EffectiveSessionsIsTheIntersectionOfItsPartsTrains()
    {
        var plan = CreatePlan();
        Forward(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3);
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, schedule.EffectiveSessions.Numbers);
    }

    [TestMethod]
    public void AppendRejectsATrainWithNoSessionInCommonWithTheSchedule()
    {
        var plan = CreatePlan();
        Forward(plan).Sessions = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        Return(plan).Sessions = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart); // ends at Snu 12:55, operates odd sessions

        var result = schedule.Append(Return(plan).AsTrainPart); // continues at Snu, but operates even sessions

        Assert.IsTrue(result.IsNone, "A train that never runs on a session the schedule operates cannot join.");
        Assert.HasCount(1, schedule.Parts);
    }

    [TestMethod]
    public void CandidateTrainsExcludeTrainsWithNoSessionInCommon()
    {
        var plan = CreatePlan();
        Forward(plan).Sessions = Sessions.FromBitPattern(CommonSessionPatterns.Odd);
        Return(plan).Sessions = Sessions.FromBitPattern(CommonSessionPatterns.Even);
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);

        var candidates = plan.CandidateTrainsFor(schedule);

        Assert.DoesNotContain(Return(plan), candidates, "The even-only return shares no session with the odd-only schedule.");
    }

    // Builds a schedule that works the forward train and assigns it a vehicle of the given type on the given
    // sessions (all sessions by default), then returns a fresh empty schedule with that same kind of vehicle
    // assigned — the "being built" schedule whose candidate list is under test.
    private static (Plan plan, Schedule building) SetUpWorkedForwardAnd(ScheduledObjectType workedBy, ScheduledObjectType buildingWith, Sessions? workedSessions = null)
    {
        var plan = CreatePlan();
        var worked = plan.CreateSchedule();
        worked.Append(Forward(plan).AsTrainPart);
        plan.AssignVehicle(worked, plan.CreateVehicle(workedBy, "A", 1, null), workedSessions);
        var building = plan.CreateSchedule();
        plan.AssignVehicle(building, plan.CreateVehicle(buildingWith, "B", 2, null));
        return (plan, building);
    }

    [TestMethod]
    public void CandidateTrainsExcludeATrainAlreadyFullyWorkedByTheSameKindOfVehicle()
    {
        var (plan, building) = SetUpWorkedForwardAnd(ScheduledObjectType.Locomotive, ScheduledObjectType.Locomotive);

        var candidates = plan.CandidateTrainsFor(building);

        Assert.DoesNotContain(Forward(plan), candidates,
            "The forward train is already worked to its end, on every session, by a locomotive, so it is fully allocated for traction.");
        Assert.Contains(Return(plan), candidates, "The return train is worked by no locomotive, so it is still available.");
    }

    [TestMethod]
    public void CandidateTrainsKeepATrainFullyWorkedByADifferentKindOfVehicle()
    {
        // A locomotive already works the forward train; the schedule being built is for a wagonset.
        var (plan, building) = SetUpWorkedForwardAnd(ScheduledObjectType.Locomotive, ScheduledObjectType.Wagonset);

        var candidates = plan.CandidateTrainsFor(building);

        Assert.Contains(Forward(plan), candidates,
            "Traction allocation does not consume the wagonset role, so the train is still available for a wagonset schedule.");
    }

    [TestMethod]
    public void CandidateTrainsKeepATrainStillUnworkedOnSomeSessions()
    {
        // The forward train runs every session, but the locomotive works it on the odd sessions only.
        var (plan, building) = SetUpWorkedForwardAnd(ScheduledObjectType.Locomotive, ScheduledObjectType.Locomotive,
            Sessions.FromBitPattern(CommonSessionPatterns.Odd));

        var candidates = plan.CandidateTrainsFor(building);

        Assert.Contains(Forward(plan), candidates,
            "The even sessions are still unworked by a locomotive, so the train stays available for a complementary schedule.");
    }

    [TestMethod]
    public void CandidateTrainsIncludeEveryTrainWhenNoVehicleIsAssigned()
    {
        var plan = CreatePlan();
        var worked = plan.CreateSchedule();
        worked.Append(Forward(plan).AsTrainPart);
        plan.AssignVehicle(worked, plan.CreateVehicle(ScheduledObjectType.Locomotive, "A", 1, null));

        // The schedule being built has no vehicle assigned, so its role is unknown and nothing is filtered.
        var candidates = plan.CandidateTrainsFor(plan.CreateSchedule());

        Assert.Contains(Forward(plan), candidates,
            "Without an assigned vehicle the list holds every train, so a wagonset schedule can still use a traction-allocated train.");
        Assert.Contains(Return(plan), candidates);
    }

    [TestMethod]
    public void CandidateTrainsExcludeATrainTheAssignedVehicleSharesNoSessionWith()
    {
        var plan = CreatePlan();
        Forward(plan).Sessions = Sessions.FromBitPattern(CommonSessionPatterns.Odd); // forward runs odd sessions
        var building = plan.CreateSchedule();
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "B", 2, null);
        plan.AssignVehicle(building, loco, Sessions.FromBitPattern(CommonSessionPatterns.Even)); // vehicle works even only

        var candidates = plan.CandidateTrainsFor(building);

        Assert.DoesNotContain(Forward(plan), candidates,
            "The vehicle works only even sessions and the forward train runs only odd, so the vehicle can never work it.");
    }

    [TestMethod]
    public void TryDeleteAssignmentUnassignsTheVehicleButKeepsTheSchedule()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        var assignment = plan.AssignVehicle(schedule, vehicle).Value;

        plan.TryDelete(assignment);

        Assert.IsEmpty(vehicle.ScheduleAssignments);
        Assert.HasCount(1, plan.Schedules);
        Assert.IsEmpty(schedule.Vehicles.ToList());
    }

    [TestMethod]
    public void ComplementaryScheduleContainsOnlyTrainsRunningBeyondTheEffectiveSessions()
    {
        var plan = CreatePlan();
        plan.Layout.Settings.General.MaxSessions = 7;
        Forward(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5); // limits the origin to 1-5
        Return(plan).Sessions = Sessions.All;                                 // also runs on 6-7
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        origin.Add(Return(plan).AsTrainPart);

        var result = plan.CreateComplementarySchedule(origin);

        Assert.IsTrue(result.HasValue, result.Message);
        var complement = result.Value;
        Assert.HasCount(1, complement.Parts);
        Assert.AreEqual(Return(plan), complement.Parts.Single().Train, "Only the all-sessions train also runs on sessions 6-7.");
        Assert.Contains(complement, plan.Schedules);
        Assert.AreNotEqual(origin, complement);
    }

    [TestMethod]
    public void ComplementaryScheduleCopiesReferencesToTheOriginalTrains()
    {
        var plan = CreatePlan();
        plan.Layout.Settings.General.MaxSessions = 7;
        Forward(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5);
        Return(plan).Sessions = Sessions.All;
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        origin.Add(Return(plan).AsTrainPart);

        var complement = plan.CreateComplementarySchedule(origin).Value;

        Assert.IsTrue(ReferenceEquals(Return(plan), complement.Parts.Single().Train),
            "The complement references the same train, it does not clone it.");
    }

    [TestMethod]
    public void ComplementaryScheduleReturnsNoneWhenTheOriginCoversTheWholePeriod()
    {
        var plan = CreatePlan();
        // Both trains default to Sessions.All and MaxSessions is 14, so the origin already covers everything.
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        origin.Add(Return(plan).AsTrainPart);

        var result = plan.CreateComplementarySchedule(origin);

        Assert.IsTrue(result.IsNone, "There is no session left for a complement.");
        Assert.HasCount(1, plan.Schedules, "No complementary schedule is added.");
    }

    [TestMethod]
    public void ComplementaryScheduleReturnsNoneForAnEmptySchedule()
    {
        var plan = CreatePlan();
        var origin = plan.CreateSchedule();

        var result = plan.CreateComplementarySchedule(origin);

        Assert.IsTrue(result.IsNone);
        Assert.HasCount(1, plan.Schedules, "The empty origin stays; no complement is added.");
    }

    [TestMethod]
    public void ComplementaryScheduleKeepsAbsoluteSessionNumbers()
    {
        var plan = CreatePlan();
        plan.Layout.Settings.General.MaxSessions = 7;
        Forward(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5);
        Return(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5, 6); // runs into session 6 only
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        origin.Add(Return(plan).AsTrainPart);

        var complement = plan.CreateComplementarySchedule(origin).Value;

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6 }, complement.EffectiveSessions.Numbers,
            "Sessions keep their absolute numbers — the surviving train still reports session 6.");
    }

    [TestMethod]
    public void CloneScheduleCopiesEveryPartReferencingTheSameTrains()
    {
        var plan = CreatePlan();
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        origin.Add(Return(plan).AsTrainPart);

        var clone = plan.CloneSchedule(origin);

        Assert.AreNotEqual(origin, clone);
        Assert.Contains(clone, plan.Schedules);
        Assert.HasCount(2, clone.Parts);
        CollectionAssert.AreEqual(
            origin.OrderedParts.Select(p => p.Train).ToList(),
            clone.OrderedParts.Select(p => p.Train).ToList(),
            "The clone works the same trains in the same order.");
        Assert.IsTrue(clone.OrderedParts.Zip(origin.OrderedParts).All(x => ReferenceEquals(x.First.Train, x.Second.Train)),
            "The clone references the same trains, it does not clone them.");
    }

    [TestMethod]
    public void CloneScheduleIsIndependentOfTheOrigin()
    {
        var plan = CreatePlan();
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        var clone = plan.CloneSchedule(origin);

        clone.TruncateFrom(clone.OrderedParts[0]); // edit the clone

        Assert.IsEmpty(clone.Parts);
        Assert.HasCount(1, origin.Parts, "Editing the clone does not change the origin.");
    }

    [TestMethod]
    public void CloneOfAnEmptyScheduleIsAnEmptySchedule()
    {
        var plan = CreatePlan();
        var origin = plan.CreateSchedule();

        var clone = plan.CloneSchedule(origin);

        Assert.AreNotEqual(origin, clone);
        Assert.IsEmpty(clone.Parts);
        Assert.HasCount(2, plan.Schedules);
    }

    [TestMethod]
    public void ComplementaryScheduleSharesTheOriginNumberAndVehicleOnTheLeftoverSessions()
    {
        var plan = CreatePlan();
        plan.Layout.Settings.General.MaxSessions = 7;
        Forward(plan).Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5);
        Return(plan).Sessions = Sessions.All;
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        origin.Add(Return(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(origin, vehicle, Sessions.FromSessionNumbers(1, 2, 3, 4, 5));

        var complement = plan.CreateComplementarySchedule(origin).Value;

        Assert.AreEqual(origin.Number, complement.Number, "The complement is listed just below the origin, sharing its number.");
        Assert.Contains(vehicle, complement.Vehicles.ToList(), "The same vehicle also works the complement.");
        var assignment = vehicle.ScheduleAssignments.First(a => complement.Equals(a.Schedule));
        CollectionAssert.AreEqual(new byte[] { 6, 7 }, assignment.Sessions.Numbers,
            "The vehicle works the complement on the sessions the origin leaves out.");
    }

    [TestMethod]
    public void ClonedScheduleSharesTheOriginNumberAndVehicleWithItsSessions()
    {
        var plan = CreatePlan();
        var origin = plan.CreateSchedule();
        origin.Add(Forward(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(origin, vehicle, Sessions.FromSessionNumbers(1, 2, 3));

        var clone = plan.CloneSchedule(origin);

        Assert.AreEqual(origin.Number, clone.Number, "The clone is listed just below the origin, sharing its number.");
        Assert.Contains(vehicle, clone.Vehicles.ToList(), "The same vehicle also works the clone.");
        var assignment = vehicle.ScheduleAssignments.First(a => clone.Equals(a.Schedule));
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, assignment.Sessions.Numbers,
            "The clone keeps the origin assignment's sessions.");
    }

    // Editing a part's span. The stations are G – Yb – Snu; the forward train runs G 12:00 → Yb 12:25/12:30
    // → Snu 12:55 and the return train Snu 13:00 → Yb 13:25/13:30 → G 13:55.
    private static string Span(ScheduledTrainPart part) =>
        $"{part.From.OperationLocation.Signature}–{part.To.OperationLocation.Signature}";

    private static StationCall CallAt(Train train, string signature) =>
        train.Calls.First(c => c.OperationLocation.Signature.Equals(signature, StringComparison.OrdinalIgnoreCase));

    // The whole forward run followed by the whole return run: G–Snu, Snu–G.
    private static (Schedule Schedule, ScheduledTrainPart Out, ScheduledTrainPart Back) CreateOutAndBack(Plan plan)
    {
        var schedule = plan.CreateSchedule();
        var outward = schedule.Add(Forward(plan).AsTrainPart);
        var back = schedule.Add(Return(plan).AsTrainPart);
        return (schedule, outward, back);
    }

    // A train whose calls were added in another order than it runs them: the origin G 12:00 first, then
    // the terminus Snu 12:55, and only then the Yb stop in between — so Calls and CallsInRunOrder differ.
    private static Train CreateTrainWithCallsAddedOutOfRunOrder(Plan plan, TrainCategory category)
    {
        var stations = TestDataFactory.Stations.ToArray();
        var train = new Train(9, category, 9) { Category = category };
        train.Add(new StationCall(1, stations[0]["3"], Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 00)));
        train.Add(new StationCall(2, stations[2]["1"], Time.FromHourAndMinute(12, 55), Time.FromHourAndMinute(12, 55)));
        train.Add(new StationCall(3, stations[1]["2"], Time.FromHourAndMinute(12, 25), Time.FromHourAndMinute(12, 30)));
        plan.Timetable.Add(train);
        return train;
    }

    [TestMethod]
    public void ATrainPartIsTakenFromTheCallsInRunOrderNotInsertionOrder()
    {
        var plan = CreatePlan();
        var train = CreateTrainWithCallsAddedOutOfRunOrder(plan, Forward(plan).Category!);

        var part = train.AsTrainPart(0, 1);

        Assert.AreEqual("G–Yb", Span(part), "Index 1 is the stop the train runs second, not the call added second.");
        Assert.AreEqual("G–Snu", Span(train.AsTrainPart), "The whole train still runs from its first stop to its last.");
    }

    [TestMethod]
    public void TheJoinCallIndexIsAPositionInTheRunOrderedCalls()
    {
        var plan = CreatePlan();
        var category = Forward(plan).Category!;
        var earlier = TestDataFactory.CreateTrainInForwardDirection(category, 4, Time.FromHourAndMinute(11, 00));
        plan.Timetable.Add(earlier);
        var train = CreateTrainWithCallsAddedOutOfRunOrder(plan, category);
        var schedule = plan.CreateSchedule();
        schedule.Append(earlier.AsTrainPart(0, 1)); // G–Yb, arriving 11:25

        var index = schedule.JoinCallIndexFor(train);

        Assert.AreEqual(1, index, "Yb is the train's second stop in run order, though its call was added last.");
        Assert.AreEqual("Yb–Snu", Span(train.AsTrainPart(index!.Value, 2)),
            "The picker builds the part from that same index, so it gets the run the planner sees.");
    }

    [TestMethod]
    public void ChangingAPartStartMovesThePreviousPartEndToMeetIt()
    {
        var plan = CreatePlan();
        var (schedule, outward, back) = CreateOutAndBack(plan);

        var edit = schedule.EditPart(back, CallAt(Return(plan), "Yb"), back.To);

        Assert.IsTrue(edit.HasValue, edit.Message);
        Assert.AreEqual("Yb–G", Span(back), "The edited part starts where it was asked to.");
        Assert.AreEqual("G–Yb", Span(outward), "The previous part is shortened to end where the next one now starts.");
        Assert.IsTrue(edit.Value.AdaptsPrevious);
        Assert.IsTrue(edit.Value.IsConsistent, "The working is still a single contiguous, non-overlapping run.");
    }

    [TestMethod]
    public void ChangingAPartEndMovesTheNextPartStartToMeetIt()
    {
        var plan = CreatePlan();
        var (schedule, outward, back) = CreateOutAndBack(plan);

        var edit = schedule.EditPart(outward, outward.From, CallAt(Forward(plan), "Yb"));

        Assert.IsTrue(edit.HasValue, edit.Message);
        Assert.AreEqual("G–Yb", Span(outward));
        Assert.AreEqual("Yb–G", Span(back), "The next part is adapted to start where the previous one now ends.");
        Assert.IsTrue(edit.Value.AdaptsNext);
        Assert.IsTrue(edit.Value.IsConsistent);
    }

    [TestMethod]
    public void ANeighbourIsExtendedWhenTheJointMovesFurtherAlongItsRun()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        var outward = schedule.Add(Forward(plan).AsTrainPart(0, 1)); // G–Yb
        var back = schedule.Add(Return(plan).AsTrainPart(1, 2));     // Yb–G

        var edit = schedule.EditPart(back, CallAt(Return(plan), "Snu"), back.To);

        Assert.IsTrue(edit.HasValue, edit.Message);
        Assert.AreEqual("Snu–G", Span(back));
        Assert.AreEqual("G–Snu", Span(outward), "A neighbour is extended as readily as it is shortened.");
        Assert.IsTrue(edit.Value.IsConsistent);
    }

    [TestMethod]
    public void AdaptingANeighbourLeavesTheRestOfTheWorkingAlone()
    {
        var plan = CreatePlan();
        var category = Forward(plan).Category!;
        var second = TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(14, 00));
        plan.Timetable.Add(second);
        var schedule = plan.CreateSchedule();
        var outward = schedule.Add(Forward(plan).AsTrainPart); // G–Snu
        var back = schedule.Add(Return(plan).AsTrainPart);     // Snu–G
        var last = schedule.Add(second.AsTrainPart);           // G–Snu, from 14:00

        schedule.EditPart(back, CallAt(Return(plan), "Yb"), back.To);

        Assert.AreEqual("G–Yb", Span(outward), "Only the neighbour's joint end moves...");
        Assert.AreEqual("G", outward.From.OperationLocation.Signature, "...its own far end stays where it was.");
        Assert.AreEqual("G–Snu", Span(last), "The part after the edited one is untouched when only the start changed.");
    }

    [TestMethod]
    public void AnEditTheNeighbourCannotFollowIsAppliedAndLeavesAGap()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        var outward = schedule.Add(Forward(plan).AsTrainPart(1, 2)); // Yb–Snu
        var back = schedule.Add(Return(plan).AsTrainPart);           // Snu–G

        // The previous part's train calls at Yb only where that part itself starts, so it cannot end there.
        var edit = schedule.EditPart(back, CallAt(Return(plan), "Yb"), back.To);

        Assert.IsTrue(edit.HasValue, edit.Message);
        Assert.AreEqual("Yb–G", Span(back), "The edit the planner asked for is applied...");
        Assert.AreEqual("Yb–Snu", Span(outward), "...and the neighbour that cannot follow is left as it was.");
        Assert.IsFalse(edit.Value.AdaptsPrevious);
        Assert.IsTrue(edit.Value.LeavesGapBefore);
        var errors = plan.GetValidationErrors(new ValidationSettings());
        Assert.Contains(ValidationErrorType.ScheduleNotContiguous, errors.Select(e => e.ErrorType).ToList(),
            "The gap is reported as a conflict for the planner to resolve.");
    }

    [TestMethod]
    public void AGapThatWasAlreadyThereIsNotClosedAsASideEffect()
    {
        var plan = CreatePlan();
        var category = Forward(plan).Category!;
        var earlier = TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(11, 00));
        plan.Timetable.Add(earlier);
        var schedule = plan.CreateSchedule();
        var first = schedule.Add(earlier.AsTrainPart);        // G–Snu, 11:00–11:55
        var second = schedule.Add(Forward(plan).AsTrainPart); // G–Snu, 12:00–12:55: the working is broken here

        var edit = schedule.EditPart(second, CallAt(Forward(plan), "Yb"), second.To);

        Assert.IsTrue(edit.HasValue, edit.Message);
        Assert.AreEqual("Yb–Snu", Span(second));
        Assert.AreEqual("G–Snu", Span(first), "A working already broken at the joint keeps its gap rather than being rewritten.");
        Assert.IsFalse(edit.Value.AdaptsPrevious);
    }

    [TestMethod]
    public void AnEditedPartKeepsItsIdentitySoADriverDutyFollowsIt()
    {
        var plan = CreatePlan();
        var (schedule, _, back) = CreateOutAndBack(plan);
        var duty = plan.CreateDriverDuty();
        duty.Add(back);

        schedule.EditPart(back, CallAt(Return(plan), "Yb"), back.To);

        Assert.AreEqual("Yb–G", Span(duty.Parts.Single()), "The duty works the part the vehicle works.");
    }

    [TestMethod]
    public void PlanningAnEditChangesNothing()
    {
        var plan = CreatePlan();
        var (schedule, outward, back) = CreateOutAndBack(plan);

        var planned = schedule.PlanPartEdit(back, CallAt(Return(plan), "Yb"), back.To);

        Assert.IsTrue(planned.HasValue, planned.Message);
        Assert.IsTrue(planned.Value.AdaptsPrevious, "The preview tells that the previous part would be adapted...");
        Assert.AreEqual("Snu–G", Span(back), "...but nothing is changed until the edit is applied.");
        Assert.AreEqual("G–Snu", Span(outward));
    }

    [TestMethod]
    public void EditIsRejectedWhenTheCallsAreNotOnThePartsOwnTrain()
    {
        var plan = CreatePlan();
        var (schedule, _, back) = CreateOutAndBack(plan);

        var edit = schedule.EditPart(back, CallAt(Forward(plan), "G"), back.To);

        Assert.IsTrue(edit.IsNone, "A part keeps its train; only its span can be changed.");
        Assert.AreEqual("Snu–G", Span(back));
    }

    [TestMethod]
    public void EditIsRejectedWhenTheArrivalIsNotAfterTheDeparture()
    {
        var plan = CreatePlan();
        var (schedule, _, back) = CreateOutAndBack(plan);

        var edit = schedule.EditPart(back, back.To, back.From);

        Assert.IsTrue(edit.IsNone, "A part must cover at least one leg.");
        Assert.AreEqual("Snu–G", Span(back));
    }

    [TestMethod]
    public void EditIsRejectedForAPartThatIsNotInTheSchedule()
    {
        var plan = CreatePlan();
        var (schedule, _, _) = CreateOutAndBack(plan);
        var other = Forward(plan).AsTrainPart(0, 1);

        var edit = schedule.EditPart(other, other.From, other.To);

        Assert.IsTrue(edit.IsNone);
    }

    // Working a train into the middle of a schedule. The working here is the forward run G 12:00–Snu 12:55
    // followed by a late return Snu 15:00–G 15:55, so the vehicle stands at Snu from 12:55 to 15:00 — long
    // enough for the 13:00 return to G and the 14:00 forward run back to Snu.
    private static (Plan Plan, Schedule Schedule) CreatePlanWithALayover()
    {
        var plan = CreatePlan();
        var category = Forward(plan).Category!;
        var lateReturn = TestDataFactory.CreateTrainInOppositeDirection(category, 4, Time.FromHourAndMinute(15, 00));
        plan.Timetable.Add(lateReturn);
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart);
        schedule.Append(lateReturn.AsTrainPart);
        return (plan, schedule);
    }

    private static Train BackAgain(Plan plan)
    {
        var category = Forward(plan).Category!;
        var train = plan.Timetable.Trains.FirstOrDefault(t => t.Number == 3);
        if (train is null)
        {
            train = TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(14, 00));
            plan.Timetable.Add(train);
        }
        return train;
    }

    [TestMethod]
    public void AWorkingHasOneJointMoreThanItHasParts()
    {
        var plan = CreatePlan();
        var (schedule, _, _) = CreateOutAndBack(plan);

        var joints = schedule.Joints;

        Assert.HasCount(3, joints, "One before the first part, one between the two, one after the last.");
        Assert.IsTrue(joints[0].IsStart);
        Assert.IsTrue(joints[^1].IsEnd);
        Assert.AreEqual("Snu", joints[1].From!.Signature, ignoreCase: true, message: "The vehicle stands at Snu between the two runs.");
    }

    [TestMethod]
    public void AnEmptyScheduleHasNoJoints()
    {
        var plan = CreatePlan();

        Assert.IsEmpty(plan.CreateSchedule().Joints, "There is no working yet to join anything to.");
    }

    [TestMethod]
    public void AJointReportsHowLongTheVehicleStandsThere()
    {
        var (_, schedule) = CreatePlanWithALayover();

        var joint = schedule.Joints[1];

        Assert.AreEqual("02:05", joint.Layover!.Value.HHMM(), "Snu 12:55 until Snu 15:00.");
        Assert.IsFalse(joint.IsBroken);
        Assert.IsTrue(joint.HasRoom);
    }

    [TestMethod]
    public void AJointWhereThePartsMeetExactlyHasNoRoom()
    {
        var plan = CreatePlan();
        var category = Forward(plan).Category!;
        var straightOn = TestDataFactory.CreateTrainInOppositeDirection(category, 6, Time.FromHourAndMinute(12, 55));
        plan.Timetable.Add(straightOn);
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart); // arrives Snu 12:55
        schedule.Append(straightOn.AsTrainPart);    // leaves Snu 12:55

        Assert.IsNull(schedule.Joints[1].Layover, "The vehicle changes train without standing at all.");
        Assert.IsFalse(schedule.Joints[1].HasRoom, "Nothing can be worked into a joint the parts meet at.");
    }

    [TestMethod]
    public void AHandoverAtAnIntermediateStopLeavesTheTimeTheTrainStandsThere()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Forward(plan).AsTrainPart(0, 1)); // G–Yb, arrives 12:25
        schedule.Append(Forward(plan).AsTrainPart(1, 2)); // Yb–Snu, departs 12:30

        // The two parts are of the same train, which itself stands five minutes at Yb: that is the vehicle's
        // own time there, so the joint offers it rather than pretending the parts meet exactly.
        Assert.AreEqual("00:05", schedule.Joints[1].Layover!.Value.HHMM());
    }

    [TestMethod]
    public void OnlyTrainsThatFitTheLayoverAreOfferedForAJoint()
    {
        var (plan, schedule) = CreatePlanWithALayover();
        BackAgain(plan); // the 14:00 forward run, which leaves from G, not from where the vehicle stands

        var candidates = plan.CandidateTrainsInJoint(schedule, schedule.Joints[1]);

        Assert.HasCount(1, candidates);
        Assert.AreEqual(2, candidates[0].Number, "Only the 13:00 return leaves Snu within the time the vehicle stands there.");
    }

    [TestMethod]
    public void ATrainIsOfferedForTheStartOfAWorkingWhenItBringsTheVehicleThere()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Return(plan).AsTrainPart); // Snu 13:00 – G 13:55

        var candidates = plan.CandidateTrainsInJoint(schedule, schedule.Joints[0]);

        Assert.HasCount(1, candidates);
        Assert.AreEqual(1, candidates[0].Number, "The forward run arrives at Snu at 12:55, before the working starts.");
    }

    [TestMethod]
    public void AnOutAndBackTripIsWorkedIntoALayoverALegAtATime()
    {
        var (plan, schedule) = CreatePlanWithALayover();
        var backTrain = BackAgain(plan); // the 14:00 forward run, in the timetable from the start
        var settings = new ValidationSettings();

        // The leg out: the 13:00 return to G. It leaves the vehicle at G while the working goes on from Snu.
        var joint = schedule.Joints[1];
        var run = joint.FittingCallsFor(Return(plan))!.Value;
        var outward = schedule.Insert(Return(plan).AsTrainPart(run.From, run.To));

        Assert.IsTrue(outward.HasValue, outward.Message);
        Assert.AreEqual("Snu–G", Span(outward.Value), "The whole return run fits the layover.");
        Assert.Contains(ValidationErrorType.ScheduleNotContiguous,
            plan.GetValidationErrors(settings).Select(e => e.ErrorType).ToList(),
            "The working is broken until the leg back is worked in — the temporary inconsistency the planner expects.");

        // The leg back: the 14:00 forward run, now offered at the broken joint because it bridges it.
        var broken = schedule.Joints[2];
        Assert.IsTrue(broken.IsBroken);
        var candidates = plan.CandidateTrainsInJoint(schedule, broken);
        Assert.Contains(backTrain, candidates);
        var bridge = broken.FittingCallsFor(backTrain)!.Value;
        var back = schedule.Insert(backTrain.AsTrainPart(bridge.From, bridge.To));

        Assert.IsTrue(back.HasValue, back.Message);
        Assert.AreEqual("G–Snu", Span(back.Value));
        Assert.IsEmpty(plan.GetValidationErrors(settings).Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous),
            "With the leg back worked in, the working hangs together again.");
    }

    [TestMethod]
    public void InsertRejectsAPartTheVehicleCannotBeThereFor()
    {
        var (plan, schedule) = CreatePlanWithALayover();
        var overlapping = BackAgain(plan).AsTrainPart(0, 2); // G 14:00 – Snu 14:55

        // Nothing else runs at that hour, so this one is only rejected once it clashes with a part added first.
        schedule.Insert(Return(plan).AsTrainPart);        // Snu 13:00 – G 13:55
        var first = schedule.Insert(overlapping);
        var again = schedule.Insert(BackAgain(plan).AsTrainPart(0, 1)); // G 14:00 – Yb 14:25, inside the part above

        Assert.IsTrue(first.HasValue, first.Message);
        Assert.IsTrue(again.IsNone, "A vehicle cannot be in two places at once.");
        Assert.HasCount(4, schedule.Parts);
    }

    [TestMethod]
    public void APartIsWorkedInBeforeTheFirstPartOfAWorking()
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Append(Return(plan).AsTrainPart); // Snu 13:00 – G 13:55

        var inserted = schedule.Insert(Forward(plan).AsTrainPart); // G 12:00 – Snu 12:55

        Assert.IsTrue(inserted.HasValue, inserted.Message);
        Assert.AreEqual("G", schedule.StartLocation!.Signature, ignoreCase: true, message: "The working now starts where the added run does.");
        Assert.AreEqual(inserted.Value, schedule.FirstPart);
    }

    [TestMethod]
    public void AFittingRunThatBridgesTheJointOutrightIsPreferred()
    {
        var (plan, schedule) = CreatePlanWithALayover();
        var category = Forward(plan).Category!;
        // A shuttle Snu 13:00 – Yb 13:25/13:30 – Snu 13:55 would both leave from and return to where the
        // vehicle stands, so it is the run the editor offers first for that train.
        var shuttle = new Train(5, category, 5) { Category = category };
        var stations = TestDataFactory.Stations.ToArray();
        shuttle.Add(new StationCall(1, stations[2]["1"], Time.FromHourAndMinute(13, 00), Time.FromHourAndMinute(13, 00)));
        shuttle.Add(new StationCall(2, stations[1]["2"], Time.FromHourAndMinute(13, 25), Time.FromHourAndMinute(13, 30)));
        shuttle.Add(new StationCall(3, stations[2]["1"], Time.FromHourAndMinute(13, 55), Time.FromHourAndMinute(13, 55)));
        plan.Timetable.Add(shuttle);

        var run = schedule.Joints[1].FittingCallsFor(shuttle);

        Assert.IsNotNull(run);
        Assert.AreEqual((0, 2), run!.Value, "The whole shuttle run, which leaves the working contiguous.");
    }
}
