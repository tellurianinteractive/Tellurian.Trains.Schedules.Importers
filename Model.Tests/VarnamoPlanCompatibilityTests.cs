using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// A whole plan made in the app for a real meeting (Värnamo Moduldagar, saved 2026-10-29), kept as the
/// document every change of the plan format is verified against: what it holds must still be there after
/// it has been read, and after it has been written and read again. See
/// <c>Documentation/ModelHarmonisation.md</c>, section 5.
/// </summary>
/// <remarks>
/// The document is of schema version 0, without a version number, written with <c>ReferenceHandler.Preserve</c>,
/// and is read through the migration to the current version (<see cref="PlanFile"/>).
/// It has what a small test plan lacks: locations written inside another location's
/// <c>CargoServedFrom</c> and <c>LockKey.HeldAt</c>, schedules written inside the vehicles that run them,
/// and train parts written under vehicles, driver duties and schedules alike.
/// </remarks>
[TestClass]
public sealed class VarnamoPlanCompatibilityTests
{
    private const string FileName = "Plan.Varnamo-Moduldagar.json";

    [TestMethod]
    public void EverythingInThePlanIsRead()
    {
        var plan = ReadPlan();

        var layout = plan.Layout;
        Assert.AreEqual(4, layout.Countries.Count, "countries");
        Assert.AreEqual(29, plan.Timetable.Companies.Count, "companies");
        Assert.AreEqual(10, layout.Regions.Count, "regions");
        Assert.AreEqual(31, layout.OperationLocations.Count, "operation locations");
        Assert.AreEqual(128, layout.OperationLocations.Sum(location => location.Tracks.Count), "station tracks");
        Assert.AreEqual(30, layout.TrackStretches.Count, "track stretches");
        Assert.AreEqual(9, plan.Timetable.TimetableStretches.Count, "timetable stretches");
        Assert.AreEqual(17, plan.Timetable.DispatchStretches.Count, "dispatch stretches");
        Assert.AreEqual(9, plan.Timetable.TopologyPositions.Count, "topology positions");

        var timetable = plan.Timetable;
        Assert.AreEqual(35, timetable.TrainCategories.Count, "train categories");
        Assert.AreEqual(275, timetable.Trains.Count, "trains");
        Assert.AreEqual(1579, timetable.Trains.Sum(train => train.Calls.Count), "station calls");
        Assert.AreEqual(108, timetable.Trains.Sum(train => train.CargoFlows.Count), "cargo flows");
        Assert.AreEqual(28, timetable.CargoFlowOptions.Count, "cargo flow options");

        Assert.AreEqual(81, plan.ScheduledObjects.Count, "vehicles");
        Assert.AreEqual(72, plan.Schedules.Count, "schedules");
        Assert.AreEqual(179, plan.DriverDuties.Count, "driver duties");
        Assert.AreEqual(279, plan.DriverDuties.Sum(duty => duty.Parts.Count), "train parts in driver duties");
        Assert.AreEqual(23, plan.Participants.Count, "participants");
        Assert.AreEqual(81, plan.VehicleContributions.Count, "vehicle contributions");
        Assert.AreEqual(13, plan.OtherVehicles.Count, "other vehicles");
    }

    [TestMethod]
    public void EveryReferenceIsResolved()
    {
        var plan = ReadPlan();

        var locations = plan.Layout.OperationLocations.ToHashSet(ReferenceEqualityComparer.Instance);
        var tracks = plan.Layout.OperationLocations.SelectMany(location => location.Tracks).ToHashSet(ReferenceEqualityComparer.Instance);
        var calls = plan.Timetable.Trains.SelectMany(train => train.Calls).ToHashSet(ReferenceEqualityComparer.Instance);

        // A location written inside another location's property must be the very object in the list.
        foreach (var location in plan.Layout.OperationLocations)
        {
            if (location.CargoServedFrom is { } servedFrom) Assert.IsTrue(locations.Contains(servedFrom), $"{location}: cargo served from");
            if (location.ControlledBy is { } controlledBy) Assert.IsTrue(locations.Contains(controlledBy), $"{location}: controlled by");
            if (location.LockKey is { } key) Assert.IsTrue(locations.Contains(key.HeldAt), $"{location}: lock key held at");
        }
        foreach (var stretch in plan.Layout.TrackStretches)
        {
            Assert.IsTrue(locations.Contains(stretch.Start), $"{stretch}: start");
            Assert.IsTrue(locations.Contains(stretch.End), $"{stretch}: end");
        }
        foreach (var call in plan.Timetable.Trains.SelectMany(train => train.Calls))
        {
            Assert.IsTrue(tracks.Contains(call.Track), $"{call}: track");
        }
        foreach (var part in plan.DriverDuties.SelectMany(duty => duty.Parts).Concat(plan.Schedules.SelectMany(schedule => schedule.Parts)))
        {
            Assert.IsTrue(calls.Contains(part.From), "a train part starts at a call of a train");
            Assert.IsTrue(calls.Contains(part.To), "a train part ends at a call of a train");
        }
        // The per-track index is rebuilt on read and must hold every call the trains run.
        Assert.AreEqual(calls.Count, tracks.Cast<StationTrack>().Sum(track => track.Calls.Count), "per-track call index");
    }

    [TestMethod]
    public void NothingIsLostWhenThePlanIsWrittenAndReadAgain()
    {
        var plan = ReadPlan();
        var expected = Contents.Of(plan);

        var restored = PlanFile.Read(PlanFile.Write(plan));

        Assert.AreEqual(expected, Contents.Of(restored));
    }

    private static Plan ReadPlan()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", FileName));
        return PlanFile.Read(json);
    }

    /// <summary>What a plan holds, as numbers and names that can be compared between two readings of it.</summary>
    private sealed record Contents(
        string Locations,
        string Tracks,
        string Stretches,
        int DispatchStretches,
        int TimetableStretches,
        int Companies,
        int Regions,
        int TrainCategories,
        string Trains,
        int Calls,
        int CallNotes,
        int CargoFlows,
        int Vehicles,
        int ScheduleAssignments,
        int Schedules,
        int PartsInSchedules,
        int DriverDuties,
        int PartsInDriverDuties,
        int DistinctParts,
        int Participants,
        int VehicleContributions,
        int OtherVehicles)
    {
        public static Contents Of(Plan plan)
        {
            var layout = plan.Layout;
            var trains = plan.Timetable.Trains;
            var scheduleParts = plan.Schedules.SelectMany(schedule => schedule.Parts).ToList();
            var dutyParts = plan.DriverDuties.SelectMany(duty => duty.Parts).ToList();
            return new(
                Locations: string.Join('|', layout.OperationLocations.OrderBy(location => location.Id)
                    .Select(location => $"{location.Id}:{location.GetType().Name}:{location.Signature}:{location.CargoServedFrom?.Id}:{location.ControlledBy?.Id}:{location.LockKey?.HeldAt.Id}")),
                Tracks: string.Join('|', layout.OperationLocations.SelectMany(location => location.Tracks).OrderBy(track => track.Id)
                    .Select(track => $"{track.Id}:{track.StationId}:{track.Number}:{track.Calls.Count}")),
                Stretches: string.Join('|', layout.TrackStretches.OrderBy(stretch => stretch.Id)
                    .Select(stretch => $"{stretch.Id}:{stretch.Start.Id}-{stretch.End.Id}:{stretch.Distance}")),
                DispatchStretches: plan.Timetable.DispatchStretches.Sum(stretch => 1 + stretch.Stretches.Count),
                TimetableStretches: plan.Timetable.TimetableStretches.Sum(stretch => 1 + stretch.Stretches.Count),
                Companies: plan.Timetable.Companies.Count,
                Regions: layout.Regions.Count,
                TrainCategories: plan.Timetable.TrainCategories.Count,
                Trains: string.Join('|', trains.OrderBy(train => train.Id)
                    .Select(train => $"{train.Id}:{train.Number}:{train.Category?.Id}:{train.Company?.Id}:{train.Calls.Count}")),
                Calls: trains.Sum(train => train.Calls.Count),
                CallNotes: trains.SelectMany(train => train.Calls).Sum(call => call.Notes.Count),
                CargoFlows: trains.Sum(train => train.CargoFlows.Count),
                Vehicles: plan.ScheduledObjects.Count,
                ScheduleAssignments: plan.ScheduledObjects.Sum(vehicle => vehicle.ScheduleAssignments.Count),
                Schedules: plan.Schedules.Count,
                PartsInSchedules: scheduleParts.Count,
                DriverDuties: plan.DriverDuties.Count,
                PartsInDriverDuties: dutyParts.Count,
                DistinctParts: scheduleParts.Concat(dutyParts).Distinct(ReferenceEqualityComparer.Instance).Count(),
                Participants: plan.Participants.Count,
                VehicleContributions: plan.VehicleContributions.Count,
                OtherVehicles: plan.OtherVehicles.Count);
        }
    }
}
