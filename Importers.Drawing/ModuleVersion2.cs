using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// The step that brings a module from schema version 1 to 2, moved from Module Draw with the module (survey step 6):
/// <list type="bullet">
/// <item>The Module Registry's ids move to the Registry's entry in <c>externalReferences</c>.</item>
/// <item>Exit directions <c>west</c> and <c>east</c> become <c>Left</c> and <c>Right</c>.</item>
/// <item>A module has one drawing, in place of a list of variants.</item>
/// <item>A module has a list of tracks, which its track elements refer to in place of a designation of their own.</item>
/// <item>
/// A module is the module of the shared layout model: <c>number</c>, <c>prefix</c> and <c>moduleOwners</c> in place
/// of <c>fremoNumber</c> and <c>owners</c>, <c>standardId</c> and a <c>standard</c> by short name with its scale, and
/// the tracks in <c>tracks</c> with whole numbers as ids.
/// </item>
/// <item>The module's <c>theme</c> is removed.</item>
/// </list>
/// Every step is idempotent: it finds the earlier shape by its properties and leaves a module of today's shape as it is.
/// </summary>
internal static class ModuleVersion2
{
    private const string RegistryModuleId = "registryModuleId";
    private const string Id = "id";
    private const string Direction = "direction";
    private const string Exits = "exits";
    private const string Owners = "owners";

    private const string Drawing = "drawing";
    private const string Name = "name";
    private const string Prefix = "prefix";
    private const string Number = "number";
    private const string FremoNumber = "fremoNumber";
    private const string ModuleOwners = "moduleOwners";
    private const string StandardProperty = "standard";
    private const string StandardId = "standardId";
    private const string ShortName = "shortName";
    private const string Scale = "scale";
    private const string Tracks = "tracks";
    private const string TrackElements = "trackElements";
    private const string StationTracks = "stationTracks";
    private const string StationTrackId = "stationTrackId";
    private const string IsLengthEntered = "isLengthEntered";
    private const string IsStubEntered = "isStubEntered";
    private const string TracksWithEnteredLength = "tracksWithEnteredLength";
    private const string TracksWithEnteredStub = "tracksWithEnteredStub";
    private const string OwnerSeparator = ", ";

    private static readonly string Left = nameof(ExitSide.Left);
    private static readonly string Right = nameof(ExitSide.Right);
    private static readonly string Undefined = nameof(ExitSide.Undefined);

    private static readonly string[] VariantProperties = [Id, Name, "isDefault"];

    /// <summary>Brings a module, in its own file or embedded in a hall plan, from schema version 1 to 2.</summary>
    public static void Module(JsonNode? node)
    {
        if (node is not JsonObject module) return;
        // The module's theme is no longer kept (decided 2026-11-21): what it depicts isn't settled with the Registry.
        module.Remove("theme");
        module[Name] ??= string.Empty;
        Rename(module, FremoNumber, Number);
        OwnersAsText(module);
        SharedStandard(module);
        SingleDrawing(module);
        // Development builds of version 2 wrote the Registry's module ids as a list per variant.
        if (Property(Property(module, ExternalReferences.Section), ExternalReferences.ModuleRegistry) is JsonObject entry
            && entry.Remove("variants", out var perVariant) && perVariant is JsonArray { Count: > 0 } ids && ids[0]?[ExternalReferences.ModuleId] is JsonValue first)
            entry[ExternalReferences.ModuleId] ??= first.DeepClone();
        if (module[Drawing] is not JsonObject drawing) return;
        if (drawing.Remove(RegistryModuleId, out var moduleId) && moduleId is JsonValue)
            RegistryEntry(module)[ExternalReferences.ModuleId] ??= moduleId;
        Each(drawing, Exits, Exit);
        // The drawing's "tracks" were its track elements. A drawing has no "tracks" now: the module has, with another content.
        Rename(drawing, Tracks, TrackElements);
        TracksFromDesignations(module, drawing);
        TracksOfTheModule(module, drawing);
        if (Property(Property(drawing, "autoDraw"), "parameters") is JsonObject parameters)
        {
            parameters.Remove(RegistryModuleId);
            Rename(parameters, FremoNumber, Number);
            OwnersAsText(parameters);
            Each(parameters, Exits, Exit);
        }
    }

    /// <summary>Moves the value of <paramref name="from"/> to <paramref name="to"/>, unless <paramref name="to"/> has a value already.</summary>
    private static void Rename(JsonObject node, string from, string to)
    {
        if (node.Remove(from, out var value) && value is not null) node[to] ??= value;
    }

    /// <summary>
    /// The list of owners, each with a name and a prefix, becomes the prefix of the first owner and one text with the
    /// names of all owners. What else an owner had (share, the Registry's ids) is dropped.
    /// </summary>
    private static void OwnersAsText(JsonObject node)
    {
        if (!node.Remove(Owners, out var list) || list is not JsonArray owners) return;
        var all = owners.OfType<JsonObject>().ToList();
        if (all.Count > 0 && Text(all[0][Prefix]) is { } prefix && !string.IsNullOrWhiteSpace(prefix)) node[Prefix] ??= prefix.Trim();
        var names = all.Select(o => Text(o[Name])?.Trim()).Where(n => !string.IsNullOrEmpty(n)).ToList();
        if (names.Count > 0) node[ModuleOwners] ??= string.Join(OwnerSeparator, names);
    }

    /// <summary>
    /// The reference to the standard, with the catalogue's id and the name, and the scale beside it become the
    /// catalogue's id of the standard and the standard of the shared model: its short name and its scale by short name.
    /// The earlier shape is known by the standard's <c>name</c> and by a scale that is a text.
    /// </summary>
    private static void SharedStandard(JsonObject module)
    {
        var scale = module[Scale] is JsonValue ? Text(module[Scale]) : null;
        if (module[Scale] is JsonValue) module.Remove(Scale);
        if (module[StandardProperty] is not JsonObject standard) return;
        if (standard.ContainsKey(Name))
        {
            if (standard.Remove(Id, out var id) && id is not null) module[StandardId] ??= id;
            Rename(standard, Name, ShortName);
        }
        if (!string.IsNullOrWhiteSpace(scale)) standard[Scale] ??= new JsonObject { [ShortName] = scale };
    }

    /// <summary>
    /// Makes the module's default variant, or else its first, its one drawing. Other variants are left out: each is
    /// a module of its own, with its own file, and no app version could draw them. What told variants apart, the
    /// id, the name and the default mark, is removed.
    /// </summary>
    private static void SingleDrawing(JsonObject module)
    {
        if (!module.Remove("variants", out var node) || node is not JsonArray list) return;
        var variants = list.OfType<JsonObject>().ToList();
        var chosen = variants.FirstOrDefault(v => v["isDefault"] is JsonValue flag && flag.TryGetValue<bool>(out var isDefault) && isDefault) ?? variants.FirstOrDefault();
        if (chosen is null) return;
        list.Remove(chosen);
        foreach (var property in VariantProperties) chosen.Remove(property);
        module[Drawing] ??= chosen;
    }

    /// <summary>
    /// Makes the module's list of tracks from a version 1 drawing: one track per distinct designation of its track
    /// elements, in the order they occur, numbered from 1, with the elements pointing to it. A track is a main track
    /// when one of its elements was. Elements without a designation are plain line and get no track; their main-track
    /// mark is dropped. Lengths are left at 0 and not marked as entered, so the app works them out from the drawing.
    /// </summary>
    private static void TracksFromDesignations(JsonObject module, JsonObject drawing)
    {
        if (drawing[TrackElements] is not JsonArray elements) return;
        var tracks = new JsonArray();
        var byDesignation = new Dictionary<string, JsonObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var element in elements.OfType<JsonObject>())
        {
            element.Remove("designation", out var designationNode);
            element.Remove("isMainTrack", out var isMainNode);
            var designation = designationNode is JsonValue text && text.TryGetValue<string>(out var value) ? value.Trim() : string.Empty;
            if (designation.Length == 0) continue;
            if (!byDesignation.TryGetValue(designation, out var track))
            {
                track = new JsonObject
                {
                    [Id] = byDesignation.Count + 1,
                    ["designation"] = designation,
                    ["displayOrder"] = byDesignation.Count + 1,
                    ["isMain"] = false,
                    // Said explicitly: a track of the shared model is scheduled unless it says otherwise.
                    ["isScheduled"] = false,
                };
                byDesignation.Add(designation, track);
                tracks.Add(track);
            }
            if (isMainNode is JsonValue flag && flag.TryGetValue<bool>(out var isMain) && isMain) track["isMain"] = true;
            element[StationTrackId] = track[Id]!.DeepClone();
        }
        if (tracks.Count > 0) module[Tracks] ??= tracks;
    }

    /// <summary>
    /// Moves the list of tracks that development builds of version 2 wrote in the drawing (<c>stationTracks</c>) to
    /// the module. The ids, which were GUIDs, become the numbers 1, 2 and so on in the order of the list, also where
    /// the track elements refer to them. What said that the user has entered a track's length or stub mark becomes the
    /// drawing's two lists of track ids. An element that refers to a track that isn't in the list gets no track.
    /// </summary>
    private static void TracksOfTheModule(JsonObject module, JsonObject drawing)
    {
        var numbers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        if (drawing.Remove(StationTracks, out var list) && list is JsonArray tracks)
        {
            var withLength = new JsonArray();
            var withStub = new JsonArray();
            foreach (var track in tracks.OfType<JsonObject>())
            {
                var number = numbers.Count + 1;
                if (Text(track[Id]) is { } id) numbers.TryAdd(id, number);
                else numbers.Add(Guid.NewGuid().ToString(), number);
                track[Id] = number;
                if (track.Remove(IsLengthEntered, out var length) && IsTrue(length)) withLength.Add(number);
                if (track.Remove(IsStubEntered, out var stub) && IsTrue(stub)) withStub.Add(number);
            }
            module[Tracks] ??= tracks;
            if (withLength.Count > 0) drawing[TracksWithEnteredLength] ??= withLength;
            if (withStub.Count > 0) drawing[TracksWithEnteredStub] ??= withStub;
        }
        if (drawing[TrackElements] is not JsonArray elements) return;
        foreach (var element in elements.OfType<JsonObject>())
        {
            if (Text(element[StationTrackId]) is not { } id) continue;
            if (numbers.TryGetValue(id, out var number)) element[StationTrackId] = number;
            else element.Remove(StationTrackId);
        }
    }

    private static bool IsTrue(JsonNode? node) => node is JsonValue value && value.TryGetValue<bool>(out var flag) && flag;

    internal static string? Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    /// <summary>
    /// A one-word enum value as it is written now, with a capital first letter: <c>left</c> becomes <c>Left</c>.
    /// </summary>
    internal static string? EnumName(string? value) =>
        string.IsNullOrEmpty(value) ? value : string.Concat(char.ToUpperInvariant(value[0]).ToString(), value.AsSpan(1));

    private static void Exit(JsonNode node)
    {
        if (node is not JsonObject exit || exit[Direction] is not JsonValue value || !value.TryGetValue<string>(out var direction)) return;
        if (direction.Equals("west", StringComparison.OrdinalIgnoreCase)) exit[Direction] = Left;
        else if (direction.Equals("east", StringComparison.OrdinalIgnoreCase)) exit[Direction] = Right;
    }

    /// <summary>The Module Registry's entry in the external references of <paramref name="drawing"/>, created when missing.</summary>
    internal static JsonObject RegistryEntry(JsonObject drawing)
    {
        if (drawing[ExternalReferences.Section] is not JsonObject section) drawing[ExternalReferences.Section] = section = [];
        if (section[ExternalReferences.ModuleRegistry] is not JsonObject entry) section[ExternalReferences.ModuleRegistry] = entry = [];
        return entry;
    }

    internal static JsonNode? Property(JsonNode? node, string name) => (node as JsonObject)?[name];

    /// <summary>Applies <paramref name="migrate"/> to every object in the array <paramref name="name"/> of <paramref name="node"/>.</summary>
    internal static void Each(JsonNode? node, string name, Action<JsonNode> migrate)
    {
        if (Property(node, name) is not JsonArray items) return;
        foreach (var item in items.OfType<JsonObject>().ToList()) migrate(item);
    }
}
