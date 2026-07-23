using System.Text.Json;
using System.Text.Json.Serialization;
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
    public void CreateVehicleAddsToPoolWithComposedExternalId()
    {
        var plan = CreatePlan();

        var vehicle = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 12, null);

        Assert.HasCount(1, plan.ScheduledObjects);
        Assert.AreEqual(12, vehicle.Number);
        Assert.AreEqual("BR 218 12", vehicle.ExternalId);
    }

    [TestMethod]
    public void CreateVehicleFallsBackToItsIdWhenNumberIsZero()
    {
        var plan = CreatePlan();

        var vehicle = plan.CreateVehicle(ScheduledObjectType.Wagonset, null, 0, null);

        Assert.AreEqual(vehicle.Id, vehicle.Number, "A vehicle with no number falls back to its unique id.");
        Assert.IsFalse(string.IsNullOrWhiteSpace(vehicle.ExternalId));
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
}
