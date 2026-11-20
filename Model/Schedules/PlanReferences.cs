using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// How a <see cref="Plan"/> is shaped in a plan file of schema version 1 (<c>Documentation/ModelHarmonisation.md</c>,
/// section 3): what a thing owns is nested in it, everything else is referred to by its id, and neither a parent nor
/// anything worked out is written. After reading, the references are resolved and the parents set again
/// (<see cref="RestoreReferences(Layout)"/>, <see cref="RestoreReferences(Timetable)"/> and
/// <see cref="RestoreReferences(Plan)"/>); before writing, the ids are brought in line with the objects
/// (<see cref="SynchroniseIds"/>).
/// </summary>
/// <remarks>
/// Where the model keeps the id of what it refers to beside the object (<see cref="StationCall.TrackId"/>), the id is
/// written and the object is not. Where it doesn't (<see cref="OperationLocation.ControlledBy"/>), a property that
/// exists only in the file is added here, and what it reads is kept aside until the plan is complete.
/// </remarks>
internal static class PlanReferences
{
    private static readonly ConditionalWeakTable<object, Dictionary<string, object?>> Pending = new();

    /// <summary>
    /// Properties of the plan's own types that are not written: parents, the ids of parents, and objects whose id is
    /// written instead. Those of the layout's types are <c>Layout.ModifyJsonTypeInfo</c>'s.
    /// </summary>
    private static readonly (Type Owner, string[] Names)[] NotWritten =
    [
        (typeof(Plan), [nameof(Plan.TimetableId)]),
        (typeof(Timetable), [nameof(Timetable.LayoutId)]),
        (typeof(TrainCategory), [nameof(TrainCategory.Company)]),
        (typeof(Train), [nameof(Train.Timetable), nameof(Train.TimetableId), nameof(Train.Category), nameof(Train.Company)]),
        (typeof(StationCall), [nameof(StationCall.Train), nameof(StationCall.TrainId), nameof(StationCall.Track)]),
        (typeof(CallNote), [nameof(CallNote.StationCall), nameof(CallNote.StationCallId)]),
        (typeof(TrainPart), [nameof(TrainPart.From), nameof(TrainPart.To)]),
        (typeof(CargoFlowTrainPart), [nameof(CargoFlowTrainPart.CargoFlowOptions)]),
        (typeof(ScheduledTrainPart), [nameof(ScheduledTrainPart.Schedule), nameof(ScheduledTrainPart.ScheduleId), nameof(ScheduledTrainPart.FromTrack), nameof(ScheduledTrainPart.ToTrack)]),
        (typeof(Origin), [nameof(Origin.Location)]),
        (typeof(Destination), [nameof(Destination.Location)]),
        (typeof(ScheduledObject), [nameof(ScheduledObject.Plan), nameof(ScheduledObject.PlanId), nameof(ScheduledObject.Company)]),
        (typeof(ScheduleAssignment), [nameof(ScheduleAssignment.ScheduledObject), nameof(ScheduleAssignment.ScheduledObjectId), nameof(ScheduleAssignment.Schedule)]),
        (typeof(Schedule), [nameof(Schedule.Plan), nameof(Schedule.PlanId)]),
        (typeof(DriverDuty), [nameof(DriverDuty.Plan), nameof(DriverDuty.PlanId), nameof(DriverDuty.Company), nameof(DriverDuty.Parts)]),
        (typeof(DriverDutyNote), [nameof(DriverDutyNote.DriverDuty), nameof(DriverDutyNote.DriverDutyId)]),
    ];

    /// <summary>Properties that exist only in the file: the id of an object the model refers to without keeping its id.</summary>
    private static readonly FileProperty[] FileProperties =
    [
        Single<Origin>("locationId", o => o.Location),
        Single<Destination>("locationId", d => d.Location),
        Single<ScheduledTrainPart>("fromTrackId", p => p.FromTrack),
        Single<ScheduledTrainPart>("toTrackId", p => p.ToTrack),
        List<DriverDuty>("partIds", d => d.Parts.Select(p => p.Id)),
        new(typeof(DriverDuty), "parts", typeof(List<ScheduledTrainPart>), o => OwnedParts((DriverDuty)o)),
    ];

    /// <summary>The modifier of <see cref="PlanJson"/> that gives every type of a plan its shape in the file.</summary>
    public static void Shape(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
        var notWritten = NotWritten.Where(n => n.Owner.IsAssignableFrom(typeInfo.Type)).SelectMany(n => n.Names).ToHashSet(StringComparer.Ordinal);
        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
        {
            var property = typeInfo.Properties[i];
            // Worked out (no setter), a name only read from older plans (no getter), or not written at all.
            if (property.Get is null || property.Set is null || IsNotWritten(property, notWritten)) typeInfo.Properties.RemoveAt(i);
        }
        foreach (var fileProperty in FileProperties.Where(p => p.Owner.IsAssignableFrom(typeInfo.Type)))
        {
            var property = typeInfo.CreateJsonPropertyInfo(fileProperty.Type, fileProperty.Name);
            property.Get = fileProperty.Get;
            property.Set = (target, value) => Keep(target, fileProperty.Name, value);
            property.ShouldSerialize = static (_, value) => value is not null;
            typeInfo.Properties.Add(property);
        }
    }

    private static bool IsNotWritten(JsonPropertyInfo property, HashSet<string> notWritten) =>
        property.AttributeProvider is System.Reflection.MemberInfo member && notWritten.Contains(member.Name);

    /// <summary>
    /// Brings every id the file refers by in line with the object it stands for, and gives every train part an id
    /// of its own. Called before a plan is written.
    /// </summary>
    public static void SynchroniseIds(Plan plan)
    {
        GivePartsIds(plan);
        if (plan.Timetable is { } timetable)
        {
            plan.TimetableId = timetable.Id;
            foreach (var stretch in timetable.Layout?.TrackStretches ?? [])
            {
                if (stretch.Start is not null) stretch.StartId = stretch.Start.Id;
                if (stretch.End is not null) stretch.EndId = stretch.End.Id;
            }
            foreach (var category in timetable.TrainCategories) category.CompanyId = category.Company?.Id ?? category.CompanyId;
            foreach (var train in timetable.Trains)
            {
                train.CategoryId = train.Category?.Id ?? train.CategoryId;
                train.CompanyId = train.Company?.Id ?? train.CompanyId;
                foreach (var call in train.Calls) if (call.Track is not null) call.TrackId = call.Track.Id;
                foreach (var part in train.CargoFlows)
                {
                    SynchroniseCalls(part);
                    if (part.CargoFlowOptions is not null) part.CargoFlowOptionsId = part.CargoFlowOptions.Id;
                }
            }
        }
        foreach (var vehicle in plan.ScheduledObjects)
        {
            vehicle.CompanyId = vehicle.Company?.Id ?? vehicle.CompanyId;
            foreach (var assignment in vehicle.ScheduleAssignments) if (assignment.Schedule is not null) assignment.ScheduleId = assignment.Schedule.Id;
        }
        foreach (var part in AllScheduledParts(plan)) SynchroniseCalls(part);
        foreach (var duty in plan.DriverDuties) duty.CompanyId = duty.Company?.Id ?? duty.CompanyId;
    }

    /// <summary>Resolves the references of a layout that has been read, and sets the parents of what it owns (<c>layout.RestoreReferences()</c>).</summary>
    public static void RestoreReferences(Layout layout) => layout.RestoreReferences();

    /// <summary>Resolves the references of a timetable that has been read, its layout's first, and sets the parents of what it owns.</summary>
    public static void RestoreReferences(Timetable timetable)
    {
        if (timetable.Layout is not { } layout) return;
        RestoreReferences(layout);
        timetable.LayoutId = layout.Id;
        var tracks = layout.OperationLocations.SelectMany(l => l.Tracks).GroupBy(t => t.Id).ToDictionary(g => g.Key, g => g.First());
        var locations = layout.OperationLocations.ToDictionary(l => l.Id);
        var options = timetable.CargoFlowOptions.GroupBy(o => o.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var option in timetable.CargoFlowOptions)
        {
            foreach (var origin in option.Origins) if (Take(origin, "locationId") is int id && locations.GetValueOrDefault(id) is { } location) origin.Location = location;
            foreach (var destination in option.Destinations) if (Take(destination, "locationId") is int id && locations.GetValueOrDefault(id) is { } location) destination.Location = location;
        }
        foreach (var train in timetable.Trains)
        {
            train.Timetable = timetable;
            train.TimetableId = timetable.Id;
            foreach (var call in train.Calls)
            {
                call.Train = train;
                call.TrainId = train.Id;
                call.Track ??= tracks.GetValueOrDefault(call.TrackId)!;
                foreach (var note in call.Notes)
                {
                    note.StationCall = call;
                    note.StationCallId = call.Id;
                }
            }
        }
        var calls = Calls(timetable);
        foreach (var part in timetable.Trains.SelectMany(t => t.CargoFlows))
        {
            RestoreCalls(part, calls);
            part.CargoFlowOptions ??= options.GetValueOrDefault(part.CargoFlowOptionsId)!;
        }
    }

    /// <summary>Resolves the references of a plan that has been read, its timetable's being resolved already, and sets the parents of what it owns.</summary>
    public static void RestoreReferences(Plan plan)
    {
        if (plan.Timetable is { } timetable) plan.TimetableId = timetable.Id;
        var calls = plan.Timetable is null ? [] : Calls(plan.Timetable);
        var tracks = plan.Timetable?.Layout?.OperationLocations.SelectMany(l => l.Tracks).GroupBy(t => t.Id).ToDictionary(g => g.Key, g => g.First()) ?? [];
        var schedules = plan.Schedules.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var vehicle in plan.ScheduledObjects)
        {
            vehicle.Plan = plan;
            vehicle.PlanId = plan.Id;
            foreach (var assignment in vehicle.ScheduleAssignments)
            {
                assignment.ScheduledObject = vehicle;
                assignment.ScheduledObjectId = vehicle.Id;
                assignment.Schedule ??= schedules.GetValueOrDefault(assignment.ScheduleId)!;
            }
        }
        var parts = new Dictionary<int, ScheduledTrainPart>();
        foreach (var schedule in plan.Schedules)
        {
            schedule.Plan = plan;
            schedule.PlanId = plan.Id;
            foreach (var part in schedule.Parts)
            {
                part.Schedule = schedule;
                part.ScheduleId = schedule.Id;
                RestorePart(part);
            }
        }
        foreach (var duty in plan.DriverDuties)
        {
            duty.Plan = plan;
            duty.PlanId = plan.Id;
            foreach (var note in duty.Notes)
            {
                note.DriverDuty = duty;
                note.DriverDutyId = duty.Id;
            }
            if (Take(duty, "parts") is List<ScheduledTrainPart> owned) foreach (var part in owned) RestorePart(part);
        }
        foreach (var duty in plan.DriverDuties)
        {
            if (Take(duty, "partIds") is List<int> ids) duty.Parts = [.. ids.Select(parts.GetValueOrDefault).OfType<ScheduledTrainPart>()];
        }

        void RestorePart(ScheduledTrainPart part)
        {
            RestoreCalls(part, calls);
            if (Take(part, "fromTrackId") is int from) part.FromTrack = tracks.GetValueOrDefault(from);
            if (Take(part, "toTrackId") is int to) part.ToTrack = tracks.GetValueOrDefault(to);
            parts.TryAdd(part.Id, part);
        }
    }

    /// <summary>
    /// The parts of <paramref name="duty"/> that the file writes inside it: those that belong to no vehicle schedule
    /// and that no earlier duty of the plan already holds. Every other part is written by its schedule, or by the
    /// first duty that holds it, and referred to by id.
    /// </summary>
    private static List<ScheduledTrainPart>? OwnedParts(DriverDuty duty)
    {
        var earlier = duty.Plan?.DriverDuties.TakeWhile(d => !ReferenceEquals(d, duty)).SelectMany(d => d.Parts).ToHashSet(ReferenceEqualityComparer.Instance) ?? [];
        var owned = duty.Parts.Where(p => p.Schedule is null && !earlier.Contains(p)).ToList();
        return owned.Count > 0 ? owned : null;
    }

    /// <summary>
    /// Gives every track, station call and train part an id that no other of its kind in the plan has, since the file
    /// refers to them by id alone. An id that is already unique is kept; the references are written from the objects,
    /// after this.
    /// </summary>
    /// <remarks>
    /// The train parts of the vehicle schedules and driver duties and the cargo flows of the trains are one kind: they
    /// share a table in a database, which keys them by their id. The scheduled parts come first, as they are referred
    /// to by id, so a cargo flow is the one given a new id.
    /// </remarks>
    private static void GivePartsIds(Plan plan)
    {
        if (plan.Timetable is { } timetable)
        {
            GiveUniqueIds(timetable.Layout?.OperationLocations.SelectMany(l => l.Tracks) ?? [], t => t.Id, (t, id) => t.Id = id);
            GiveUniqueIds(timetable.Trains.SelectMany(t => t.Calls), c => c.Id, (c, id) => c.Id = id);
        }
        var cargoFlows = plan.Timetable?.Trains.SelectMany(t => t.CargoFlows) ?? [];
        GiveUniqueIds(AllScheduledParts(plan).Cast<TrainPart>().Concat(cargoFlows), p => p.Id, (p, id) => p.Id = id);
    }

    private static void GiveUniqueIds<T>(IEnumerable<T> items, Func<T, int> getId, Action<T, int> setId)
    {
        var list = items.ToList();
        var used = new HashSet<int>();
        var next = list.Select(getId).DefaultIfEmpty(0).Max() + 1;
        foreach (var item in list)
        {
            if (getId(item) > 0 && used.Add(getId(item))) continue;
            setId(item, next);
            used.Add(next++);
        }
    }

    private static IEnumerable<ScheduledTrainPart> AllScheduledParts(Plan plan) =>
        plan.Schedules.SelectMany(s => s.Parts).Concat(plan.DriverDuties.SelectMany(d => d.Parts)).Distinct(ReferenceEqualityComparer.Instance).Cast<ScheduledTrainPart>();

    private static Dictionary<int, StationCall> Calls(Timetable timetable) =>
        timetable.Trains.SelectMany(t => t.Calls).GroupBy(c => c.Id).ToDictionary(g => g.Key, g => g.First());

    private static void SynchroniseCalls(TrainPart part)
    {
        if (part.From is not null) part.FromId = part.From.Id;
        if (part.To is not null) part.ToId = part.To.Id;
    }

    private static void RestoreCalls(TrainPart part, Dictionary<int, StationCall> calls)
    {
        part.From ??= calls.GetValueOrDefault(part.FromId)!;
        part.To ??= calls.GetValueOrDefault(part.ToId)!;
    }

    private static void Keep(object target, string name, object? value) => Pending.GetOrCreateValue(target)[name] = value;

    private static object? Take(object target, string name)
    {
        if (!Pending.TryGetValue(target, out var values) || !values.Remove(name, out var value)) return null;
        if (values.Count == 0) Pending.Remove(target);
        return value;
    }

    private static FileProperty Single<T>(string name, Func<T, object?> target) =>
        new(typeof(T), name, typeof(int?), o => target((T)o) switch
        {
            OperationLocation location => location.Id,
            StationTrack track => track.Id,
            _ => null,
        });

    private static FileProperty List<T>(string name, Func<T, IEnumerable<int>> ids) =>
        new(typeof(T), name, typeof(List<int>), o => ids((T)o).ToList() is { Count: > 0 } list ? list : null);

    private sealed record FileProperty(Type Owner, string Name, Type Type, Func<object, object?> Get);
}
