using Tellurian.Trains.Schedules.Model.Files;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The migration of a plan from schema version 0 to 1 (<c>Documentation/ModelHarmonisation.md</c>, sections 3 and 5).
/// </summary>
/// <remarks>
/// <para>
/// Version 0 is the plan as <c>ReferenceHandler.Preserve</c> wrote it: every object and list with a <c>$id</c>, every
/// further mention of it a <c>$ref</c>, an object written where the writer first met it rather than where it belongs,
/// parents written on their children, PascalCase names and enum values as numbers. Version 1 is a tree: what a thing
/// owns is nested in it, everything else is referred to by id, no parent and nothing worked out is written, names are
/// camelCase and enum values are written by their name.
/// </para>
/// <para>
/// The step knows the shape of version 0 from the description below, not from the model, so that it still works when
/// the model has changed: the enum names are copied here as they were. It also does once what reading a version 0
/// plan used to do every time: the legacy names of origins and destinations, the legacy flags of a train category,
/// a country written as an object, and train parts without an id of their own.
/// </para>
/// </remarks>
public static class SchemaVersion1
{
    /// <summary>The step from schema version 0.</summary>
    public static FileMigration Migration { get; } = new(0, content => new Migrator(content).Run());

    private enum Kind { Value, Drop, Owned, OwnedList, Catalogue, Reference, ReferenceList, Enum, IdList, Countries, DutyParts }

    private sealed record Rule(Kind Kind, string? Type = null, string? Name = null, string? KeyName = null, EnumNames? Enum = null);

    private sealed record EnumNames(bool IsFlags, (long Value, string Name)[] Members);

    private static Rule Drop => new(Kind.Drop);
    private static Rule Owned(string type) => new(Kind.Owned, type);
    private static Rule OwnedList(string type) => new(Kind.OwnedList, type);
    private static Rule Catalogue(string type) => new(Kind.Catalogue, type);
    private static Rule Reference(string name, string? keyName = null) => new(Kind.Reference, Name: name, KeyName: keyName);
    private static Rule ReferenceList(string name) => new(Kind.ReferenceList, Name: name);
    private static Rule Enum(EnumNames names) => new(Kind.Enum, Enum: names);

    // The enums of version 0, as they were when version 1 was made.
    private static readonly EnumNames WagonSetKind = new(false, [(0, "Unspecified"), (1, "Passenger"), (2, "Cargo")]);
    private static readonly EnumNames ScheduledObjectType = new(false, [(0, "Unknown"), (1, "Locomotive"), (2, "Trainset"), (3, "Wagonset"), (4, "CargoFlow"), (5, "Cargo"), (6, "Shunter")]);
    private static readonly EnumNames TractionType = new(false, [(0, "Undefined"), (1, "None"), (2, "Steam"), (3, "Diesel"), (4, "Electric"), (5, "Dual"), (6, "Battery")]);
    private static readonly EnumNames DutyDifficulty = new(false, [(1, "Easy"), (2, "Medium"), (3, "Experienced")]);
    private static readonly EnumNames OtherVehicleType = new(false, [(0, "Locomotive"), (1, "Trainset"), (2, "Wagonset"), (3, "Other")]);
    private static readonly EnumNames TrainContent = new(true, [(0, "None"), (1, "Passenger"), (2, "Cargo")]);
    private static readonly EnumNames StartLabelPrinting = new(false, [(0, "None"), (1, "First"), (2, "All")]);
    private static readonly EnumNames TractionRole = new(false, [(0, "Front"), (1, "Back"), (2, "Reinforcement")]);
    private static readonly EnumNames RunaroundMode = new(false, [(0, "Automatic"), (1, "Never"), (2, "Always")]);
    private static readonly EnumNames TractionLayover = new(false, [(0, "None"), (1, "Stabling"), (2, "LiftedOff")]);
    private static readonly EnumNames Theme = new(false, [(0, "American"), (1, "European")]);
    private static readonly EnumNames TimeAxisOrientation = new(false, [(0, "Horizontal"), (1, "Vertical")]);
    private static readonly EnumNames StaffRoles = new(true, [(0, "None"), (1, "Dispatcher"), (2, "SwitchgearOperator"), (4, "YardManager"), (8, "Shuntman"), (16, "Trainee")]);
    private static readonly EnumNames DayOfWeek = new(false, [(0, "Sunday"), (1, "Monday"), (2, "Tuesday"), (3, "Wednesday"), (4, "Thursday"), (5, "Friday"), (6, "Saturday")]);

    private static readonly Dictionary<string, string[]> WorkedOut = new()
    {
        ["ScheduledObject"] = ["HasExternalId", "Designation"],
        ["Train"] = ["DriverStartTime", "DriverEndTime", "Layout", "AsTrainPart"],
        ["CargoFlowOptions"] = ["DestinationLocationNames", "OriginLocationNames", "DestinationsSummary"],
        ["TrainPart"] = ["Train", "Departure", "Arrival"],
        ["TrackStretch"] = ["Passings"],
        ["TimetableStretch"] = ["Stations"],
        ["DispatchStretch"] = ["IntermediateLocations"],
        ["StationCall"] = ["OperationLocation", "IsStop", "IsPassthrough", "SortTime"],
        ["CallNote"] = ["ToText", "ToHtml", "Text"],
        ["CargoOnlyOptions"] = ["Load", "Unload"],
    };

    // The shape of version 0. A property not named is a value, copied with its name in camelCase; an object or a list
    // not named is copied the same way all through.
    private static readonly Dictionary<string, Dictionary<string, Rule>> Shapes = new()
    {
        ["Plan"] = new()
        {
            ["TimetableId"] = Drop,
            ["Timetable"] = Owned("Timetable"),
            ["ScheduledObjects"] = OwnedList("ScheduledObject"),
            ["Schedules"] = OwnedList("Schedule"),
            ["DriverDuties"] = OwnedList("DriverDuty"),
            ["OtherVehicles"] = OwnedList("OtherVehicle"),
        },
        ["Timetable"] = new()
        {
            ["LayoutId"] = Drop,
            ["Layout"] = Owned("Layout"),
            ["TrainCategories"] = Catalogue("TrainCategory"),
            ["Trains"] = OwnedList("Train"),
            ["CargoFlowOptions"] = OwnedList("CargoFlowOptions"),
        },
        ["Layout"] = new()
        {
            ["Settings"] = Owned("PlanSettings"),
            ["Countries"] = new(Kind.Countries),
            ["Companies"] = Catalogue("Company"),
            ["Regions"] = Catalogue("Region"),
            ["OperationLocations"] = OwnedList("OperationLocation"),
            ["TrackStretches"] = OwnedList("TrackStretch"),
            ["TimetableStretches"] = OwnedList("TimetableStretch"),
            ["DispatchStretches"] = OwnedList("DispatchStretch"),
        },
        ["PlanSettings"] = new()
        {
            ["Identity"] = Owned("IdentitySettings"),
            ["General"] = Owned("GeneralSettings"),
            ["GraphicTimetable"] = Owned("GraphicTimetableSettings"),
        },
        ["IdentitySettings"] = new() { ["Theme"] = Enum(Theme) },
        ["GeneralSettings"] = new() { ["StartDay"] = Enum(DayOfWeek) },
        ["GraphicTimetableSettings"] = new() { ["Orientation"] = Enum(TimeAxisOrientation) },
        ["Company"] = new() { ["Layout"] = Drop, ["LayoutId"] = Drop },
        ["OperationLocation"] = new()
        {
            ["Layout"] = Drop,
            ["LayoutId"] = Drop,
            ["CargoServedFrom"] = Reference("cargoServedFromId"),
            ["ControlledBy"] = Reference("controlledById"),
            ["LockKey"] = Owned("LockKey"),
            ["Tracks"] = OwnedList("StationTrack"),
            ["StaffPosts"] = OwnedList("StaffPost"),
            ["StartLabels"] = Enum(StartLabelPrinting),
            ["Regions"] = Drop,
            ["RegionIds"] = new(Kind.IdList),
        },
        ["LockKey"] = new() { ["HeldAt"] = Reference("heldAtId") },
        ["StaffPost"] = new() { ["Roles"] = Enum(StaffRoles) },
        ["StationTrack"] = new() { ["Station"] = Drop, ["StationId"] = Drop, ["Calls"] = Drop },
        ["TrackStretch"] = new()
        {
            ["Layout"] = Drop,
            ["LayoutId"] = Drop,
            ["StartId"] = Drop,
            ["Start"] = Reference("startId", "StartId"),
            ["EndId"] = Drop,
            ["End"] = Reference("endId", "EndId"),
        },
        ["TimetableStretch"] = new() { ["Stretches"] = ReferenceList("stretchIds") },
        ["DispatchStretch"] = new()
        {
            ["From"] = Reference("fromId"),
            ["To"] = Reference("toId"),
            ["Stretches"] = ReferenceList("stretchIds"),
        },
        ["TrainCategory"] = new()
        {
            ["Content"] = Enum(TrainContent),
            ["StartLabels"] = Enum(StartLabelPrinting),
            ["StopLocationIds"] = new(Kind.IdList),
            ["CompanyId"] = Drop,
            ["Company"] = Reference("companyId", "CompanyId"),
            ["IsPassenger"] = Drop,
            ["IsFreight"] = Drop,
        },
        ["Train"] = new()
        {
            ["CompanyId"] = Drop,
            ["Company"] = Reference("companyId", "CompanyId"),
            ["CategoryId"] = Drop,
            ["Category"] = Reference("categoryId", "CategoryId"),
            ["TimetableId"] = Drop,
            ["Timetable"] = Drop,
            ["Calls"] = OwnedList("StationCall"),
            ["CargoFlows"] = OwnedList("CargoFlowTrainPart"),
        },
        ["StationCall"] = new()
        {
            ["TrackId"] = Drop,
            ["Track"] = Reference("trackId", "TrackId"),
            ["TrainId"] = Drop,
            ["Train"] = Drop,
            ["Runaround"] = Enum(RunaroundMode),
            ["Notes"] = OwnedList("CallNote"),
        },
        ["CallNote"] = new() { ["StationCall"] = Drop, ["StationCallId"] = Drop },
        ["TrainPart"] = new()
        {
            ["FromId"] = Drop,
            ["From"] = Reference("fromId", "FromId"),
            ["ToId"] = Drop,
            ["To"] = Reference("toId", "ToId"),
        },
        ["CargoFlowTrainPart"] = new()
        {
            ["CargoFlowOptionsId"] = Drop,
            ["CargoFlowOptions"] = Reference("cargoFlowOptionsId", "CargoFlowOptionsId"),
        },
        ["CargoFlowOptions"] = new()
        {
            ["Origins"] = OwnedList("Origin"),
            ["Destinations"] = OwnedList("Destination"),
        },
        ["Origin"] = new() { ["Location"] = Reference("locationId"), ["Station"] = Reference("locationId") },
        ["Destination"] = new() { ["Location"] = Reference("locationId"), ["Station"] = Reference("locationId") },
        ["ScheduledObject"] = new()
        {
            ["WagonSetKind"] = Enum(WagonSetKind),
            ["ObjectType"] = Enum(ScheduledObjectType),
            ["TractionType"] = Enum(TractionType),
            ["CompanyId"] = Drop,
            ["Company"] = Reference("companyId", "CompanyId"),
            ["PlanId"] = Drop,
            ["Plan"] = Drop,
            ["ScheduleAssignments"] = OwnedList("ScheduleAssignment"),
            ["Units"] = OwnedList("ScheduledUnit"),
        },
        ["ScheduledUnit"] = new() { ["TractionType"] = Enum(TractionType) },
        ["ScheduleAssignment"] = new()
        {
            ["Role"] = Enum(TractionRole),
            ["ScheduledObjectId"] = Drop,
            ["ScheduledObject"] = Drop,
            ["ScheduleId"] = Drop,
            ["Schedule"] = Reference("scheduleId", "ScheduleId"),
        },
        ["Schedule"] = new() { ["PlanId"] = Drop, ["Plan"] = Drop, ["Parts"] = OwnedList("ScheduledTrainPart") },
        ["ScheduledTrainPart"] = new()
        {
            ["ScheduleId"] = Drop,
            ["Schedule"] = Drop,
            ["TractionOptions"] = Owned("TractionOptions"),
            ["WagonSetOptions"] = Owned("WagonSetOptions"),
            ["CargoOnlyOptions"] = Owned("CargoOnlyOptions"),
            ["FromTrack"] = Reference("fromTrackId"),
            ["ToTrack"] = Reference("toTrackId"),
            ["Id"] = Drop,
        },
        ["TractionOptions"] = new()
        {
            ["FromLayover"] = Enum(TractionLayover),
            ["ToLayover"] = Enum(TractionLayover),
            ["Runaround"] = Enum(RunaroundMode),
        },
        ["WagonSetOptions"] = new() { ["FromLayover"] = Enum(TractionLayover), ["ToLayover"] = Enum(TractionLayover) },
        ["DriverDuty"] = new()
        {
            ["Difficulty"] = Enum(DutyDifficulty),
            ["Parts"] = new(Kind.DutyParts),
            ["Notes"] = OwnedList("DriverDutyNote"),
            ["CompanyId"] = Drop,
            ["Company"] = Reference("companyId", "CompanyId"),
            ["PlanId"] = Drop,
            ["Plan"] = Drop,
        },
        ["DriverDutyNote"] = new() { ["DriverDuty"] = Drop, ["DriverDutyId"] = Drop },
        ["OtherVehicle"] = new() { ["Type"] = Enum(OtherVehicleType) },
    };

    // The shapes that take the rules of another as well: a train part of either kind has those of TrainPart.
    private static readonly Dictionary<string, string> Base = new()
    {
        ["ScheduledTrainPart"] = "TrainPart",
        ["CargoFlowTrainPart"] = "TrainPart",
    };

    private sealed class Migrator
    {
        private const string IdProperty = "$id";
        private const string RefProperty = "$ref";
        private const string ValuesProperty = "$values";
        private const string TypeProperty = "$type";

        private readonly JsonNode _root;
        private readonly Dictionary<string, JsonNode> _byId = new(StringComparer.Ordinal);
        private readonly Dictionary<JsonNode, int> _partIds = new(ReferenceEqualityComparer.Instance);
        private readonly HashSet<JsonNode> _scheduledParts = new(ReferenceEqualityComparer.Instance);
        private readonly HashSet<JsonNode> _writtenParts = new(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<string, List<JsonObject>> _catalogues = new(StringComparer.Ordinal);
        private readonly Dictionary<JsonNode, int> _catalogueIds = new(ReferenceEqualityComparer.Instance);

        public Migrator(JsonNode root)
        {
            _root = root;
            Index(root);
        }

        public JsonNode Run()
        {
            NumberParts();
            CompleteCatalogues();
            return Transform(_root, "Plan") ?? new JsonObject();
        }

        private void Index(JsonNode? node)
        {
            switch (node)
            {
                case JsonObject value:
                    if (value[IdProperty] is JsonValue id && id.TryGetValue<string>(out var key))
                        _byId.TryAdd(key, value[ValuesProperty] is JsonArray values ? values : value);
                    foreach (var (_, child) in value) Index(child);
                    break;
                case JsonArray list:
                    foreach (var child in list) Index(child);
                    break;
            }
        }

        /// <summary>The object or list that <paramref name="node"/> stands for: a <c>$ref</c> resolved, a <c>$values</c> unwrapped.</summary>
        private JsonNode? Resolve(JsonNode? node)
        {
            if (node is not JsonObject value) return node;
            if (value[RefProperty] is JsonValue reference && reference.TryGetValue<string>(out var key)) return _byId.GetValueOrDefault(key);
            return value.ContainsKey(IdProperty) && value[ValuesProperty] is JsonArray values ? values : value;
        }

        private IEnumerable<JsonObject> Items(JsonNode? list) =>
            Resolve(list) is JsonArray items ? items.Select(Resolve).OfType<JsonObject>() : [];

        /// <summary>
        /// Puts every company, train category and region that something refers to into its catalogue, and gives each
        /// entry an id that no other in the catalogue has. An early plan wrote such an entry where it was used, with an
        /// empty catalogue and often an id of 0, and told two apart only by being two objects.
        /// </summary>
        private void CompleteCatalogues()
        {
            if (Resolve(_root) is not JsonObject plan) return;
            var timetable = Resolve(plan["Timetable"]) as JsonObject;
            var layout = Resolve(timetable?["Layout"]) as JsonObject;
            var trains = Items(timetable?["Trains"]).ToList();
            var categories = Items(timetable?["TrainCategories"]).ToList();
            Complete("Company", Items(layout?["Companies"]),
                trains.Select(t => t["Company"])
                    .Concat(categories.Select(c => c["Company"]))
                    .Concat(trains.Select(t => t["Category"]).Select(Resolve).OfType<JsonObject>().Select(c => c["Company"]))
                    .Concat(Items(plan["ScheduledObjects"]).Select(v => v["Company"]))
                    .Concat(Items(plan["DriverDuties"]).Select(d => d["Company"])));
            Complete("TrainCategory", categories, trains.Select(t => t["Category"]));
            Complete("Region", Items(layout?["Regions"]), Items(layout?["OperationLocations"]).SelectMany(l => Items(l["Regions"])));

            void Complete(string type, IEnumerable<JsonObject> catalogue, IEnumerable<JsonNode?> referred)
            {
                var entries = new List<JsonObject>();
                foreach (var entry in catalogue.Concat(referred.Select(Resolve).OfType<JsonObject>()))
                    if (!entries.Contains(entry, ReferenceEqualityComparer.Instance)) entries.Add(entry);
                var used = new HashSet<int>();
                var next = entries.Select(e => IdOf(e) ?? 0).DefaultIfEmpty(0).Max() + 1;
                foreach (var entry in entries)
                {
                    var id = IdOf(entry) is int own && own > 0 && used.Add(own) ? own : next++;
                    used.Add(id);
                    _catalogueIds[entry] = id;
                }
                _catalogues[type] = entries;
            }
        }

        /// <summary>The id that <paramref name="node"/> is referred to by in version 1: that of its catalogue entry or its own.</summary>
        private int? IdOfTarget(JsonNode? node) =>
            node is not null && _catalogueIds.TryGetValue(node, out var id) ? id : IdOf(node);

        /// <summary>Gives every train part of the vehicle schedules and the driver duties an id that no other has, those of the schedules first.</summary>
        private void NumberParts()
        {
            if (Resolve(_root) is not JsonObject plan) return;
            var parts = new List<JsonObject>();
            foreach (var schedule in Items(plan["Schedules"]))
                foreach (var part in Items(schedule["Parts"]))
                    if (_scheduledParts.Add(part)) parts.Add(part);
            foreach (var duty in Items(plan["DriverDuties"]))
                foreach (var part in Items(duty["Parts"]))
                    if (!parts.Contains(part, ReferenceEqualityComparer.Instance)) parts.Add(part);
            var used = new HashSet<int>();
            var next = parts.Select(p => IdOf(p) ?? 0).DefaultIfEmpty(0).Max() + 1;
            foreach (var part in parts)
                _partIds[part] = IdOf(part) is int id && id > 0 && used.Add(id) ? id : Used(next++);

            int Used(int id)
            {
                used.Add(id);
                return id;
            }
        }

        private JsonObject? Transform(JsonNode? node, string type)
        {
            if (Resolve(node) is not JsonObject source) return null;
            var result = new JsonObject();
            if (source[TypeProperty] is { } discriminator) result[TypeProperty] = discriminator.DeepClone();
            // A location written where the writer first met it, inside another location's CargoServedFrom, ControlledBy or
            // LockKey.HeldAt, has no type of its own: those properties take only a station, so it needed none.
            else if (type == "OperationLocation") result[TypeProperty] = "Station";
            if (type == "ScheduledTrainPart" && _partIds.TryGetValue(source, out var partId)) result["id"] = partId;
            if (_catalogueIds.TryGetValue(source, out var entryId)) result["id"] = entryId;
            foreach (var (name, value) in source)
            {
                if (name.StartsWith('$') || IsWorkedOut(type, name)) continue;
                var rule = RuleOf(type, name);
                switch (rule?.Kind ?? Kind.Value)
                {
                    case Kind.Drop:
                        break;
                    case Kind.Owned:
                        Set(result, Name(name), Transform(value, rule!.Type!));
                        break;
                    case Kind.OwnedList:
                        Set(result, Name(name), new JsonArray([.. Items(value).Select(item => Transform(item, rule!.Type!))]));
                        break;
                    case Kind.Catalogue:
                        var entries = _catalogues.TryGetValue(rule!.Type!, out var complete) ? complete : Items(value).ToList();
                        Set(result, Name(name), new JsonArray([.. entries.Select(item => Transform(item, rule.Type!))]));
                        break;
                    case Kind.Reference:
                        if (!result.ContainsKey(rule!.Name!)) Set(result, rule.Name!, IdOfTarget(Resolve(value)) ?? (rule.KeyName is { } key ? IdValue(source[key]) : null));
                        break;
                    case Kind.ReferenceList:
                        var ids = Items(value).Select(IdOf).OfType<int>().ToList();
                        if (ids.Count > 0) result[rule!.Name!] = new JsonArray([.. ids.Select(id => (JsonNode)id)]);
                        break;
                    case Kind.Enum:
                        Set(result, Name(name), EnumValue(value, rule!.Enum!));
                        break;
                    case Kind.IdList:
                        var list = Resolve(value) is JsonArray idList ? idList.Select(IdValue).OfType<int>().ToList() : [];
                        if (list.Count > 0) result[Name(name)] = new JsonArray([.. list.Select(id => (JsonNode)id)]);
                        break;
                    case Kind.Countries:
                        result[Name(name)] = new JsonArray([.. (Resolve(value) as JsonArray ?? []).Select(c => IdValue(c) ?? IdOf(Resolve(c))).OfType<int>().Select(id => (JsonNode)id)]);
                        break;
                    case Kind.DutyParts:
                        WriteDutyParts(result, value);
                        break;
                    default:
                        Set(result, Name(name), Copy(value));
                        break;
                }
            }
            foreach (var rule in ReferenceRules(type).Where(r => r.KeyName is not null && !result.ContainsKey(r.Name!)))
                Set(result, rule.Name!, IdValue(source[rule.KeyName!]));
            Complete(type, source, result);
            return result;
        }

        /// <summary>
        /// The references of <paramref name="type"/> that version 0 also kept as an id. A plan may have the id alone, the
        /// object being written only in its catalogue, so the id is taken when the object is missing.
        /// </summary>
        private static IEnumerable<Rule> ReferenceRules(string type) =>
            (Shapes.TryGetValue(type, out var shape) ? shape.Values.Where(r => r.Kind == Kind.Reference) : [])
            .Concat(Base.TryGetValue(type, out var baseType) ? ReferenceRules(baseType) : []);

        /// <summary>What version 0 said in more than one way, put into the one way of version 1.</summary>
        private void Complete(string type, JsonObject source, JsonObject result)
        {
            switch (type)
            {
                case "TrainCategory":
                    // A category of an early plan had two flags instead of its content.
                    var content = (source["Content"] is JsonValue value && value.TryGetValue<long>(out var number) ? number : 0)
                        | (source["IsPassenger"] is JsonValue passenger && passenger.TryGetValue<bool>(out var isPassenger) && isPassenger ? 1L : 0L)
                        | (source["IsFreight"] is JsonValue freight && freight.TryGetValue<bool>(out var isFreight) && isFreight ? 2L : 0L);
                    result["content"] = EnumValue(content, TrainContent);
                    break;
                case "OperationLocation":
                    // A station of an early plan had its regions themselves, not their ids; they are its catalogue entries.
                    if (Items(source["Regions"]).Select(IdOfTarget).OfType<int>().ToList() is { Count: > 0 } regions)
                        result["regionIds"] = new JsonArray([.. regions.Select(id => (JsonNode)id)]);
                    break;
            }
        }

        /// <summary>
        /// The parts of a driver duty: all of them by id, in order, and in full those that belong to no vehicle schedule
        /// and that no earlier duty has written.
        /// </summary>
        private void WriteDutyParts(JsonObject result, JsonNode? value)
        {
            var parts = Items(value).ToList();
            var ids = parts.Select(p => _partIds.TryGetValue(p, out var id) ? id : (int?)null).OfType<int>().ToList();
            var owned = parts.Where(p => !_scheduledParts.Contains(p) && _writtenParts.Add(p)).Select(p => (JsonNode?)Transform(p, "ScheduledTrainPart")).ToList();
            if (owned.Count > 0) result["parts"] = new JsonArray([.. owned]);
            if (ids.Count > 0) result["partIds"] = new JsonArray([.. ids.Select(id => (JsonNode)id)]);
        }

        private static Rule? RuleOf(string type, string name) =>
            Shapes.TryGetValue(type, out var shape) && shape.TryGetValue(name, out var rule) ? rule :
            Base.TryGetValue(type, out var baseType) ? RuleOf(baseType, name) : null;

        private static bool IsWorkedOut(string type, string name) =>
            (WorkedOut.TryGetValue(type, out var names) && names.Contains(name, StringComparer.Ordinal)) ||
            (Base.TryGetValue(type, out var baseType) && IsWorkedOut(baseType, name));

        /// <summary>A value, object or list as it is, with names in camelCase and the bookkeeping of version 0 left out.</summary>
        private JsonNode? Copy(JsonNode? node) => Resolve(node) switch
        {
            JsonObject value => new JsonObject(value.Where(p => !p.Key.StartsWith('$')).Select(p => KeyValuePair.Create(Name(p.Key), Copy(p.Value)))),
            JsonArray list => new JsonArray([.. list.Select(Copy)]),
            var other => other?.DeepClone(),
        };

        private static void Set(JsonObject result, string name, JsonNode? value)
        {
            if (value is not null) result[name] = value;
        }

        private static string Name(string name) => JsonNamingPolicy.CamelCase.ConvertName(name);

        private static int? IdOf(JsonNode? node) => node is JsonObject value ? IdValue(value["Id"]) : null;

        private static int? IdValue(JsonNode? node) => node is JsonValue value && value.TryGetValue<int>(out var id) ? id : null;

        private static JsonNode? EnumValue(JsonNode? node, EnumNames names) =>
            node is JsonValue value && value.TryGetValue<long>(out var number) ? EnumValue(number, names) : node?.DeepClone();

        private static JsonNode EnumValue(long number, EnumNames names)
        {
            if (names.Members.FirstOrDefault(m => m.Value == number) is { Name: { } exact }) return exact;
            if (!names.IsFlags || number < 0) return number;
            var flags = names.Members.Where(m => m.Value != 0 && (number & m.Value) == m.Value).ToList();
            return flags.Aggregate(0L, (all, m) => all | m.Value) == number ? string.Join(", ", flags.Select(m => m.Name)) : number;
        }
    }
}
