using System.Globalization;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The migration of a plan from schema version 5 to 6: the plan keeps the result of the hall plan, not its drawings
/// (<c>Documentation/ModelSurfaceAnalysis.md</c>, step 2).
/// </summary>
/// <remarks>
/// The layout's <c>lines</c>, <c>joints</c>, <c>endProfiles</c>, <c>standards</c>, <c>overrides</c> and
/// <c>fitTolerances</c> go. Its <c>preferredStandard</c> becomes the layout's <c>scale</c> and <c>standardName</c>. Each
/// of its <c>modules</c> loses the module with its drawing and keeps what the planner shows: its id, the module's own id
/// as <c>sourceId</c>, its title, owners, package label and location, and its external references. What was measured on
/// a stretch, and the order of the locations along the lines, are not in a version 5 plan; they come with the next
/// update from the hall plan.
/// </remarks>
public static class SchemaVersion6
{
    /// <summary>The step from schema version 5.</summary>
    public static FileMigration Migration { get; } = new(5, Migrate);

    private static readonly string[] Dropped = ["lines", "joints", "endProfiles", "standards", "overrides", "fitTolerances"];

    private static JsonNode Migrate(JsonNode content)
    {
        if (content["timetable"]?["layout"] is not JsonObject layout) return content;
        foreach (var name in Dropped) layout.Remove(name);
        if (layout["preferredStandard"] is JsonObject standard)
        {
            if (standard["scale"] is JsonObject scale) layout["scale"] = scale.DeepClone();
            if (Text(standard["shortName"]) is string shortName) layout["standardName"] = shortName;
        }
        layout.Remove("preferredStandard");
        if (layout["modules"] is JsonArray modules)
        {
            var locationNames = LocationNames(layout);
            layout["modules"] = new JsonArray([.. modules.OfType<JsonObject>().Select(m => Summary(m, locationNames))]);
        }
        return content;
    }

    private static JsonObject Summary(JsonObject placed, Dictionary<long, string> locationNames)
    {
        var summary = new JsonObject();
        if (placed["id"] is JsonNode id) summary["id"] = id.DeepClone();
        if (placed["module"] is JsonObject module)
        {
            if (module["id"] is JsonNode sourceId) summary["sourceId"] = sourceId.DeepClone();
            var locationId = LocationId(module);
            var title = Title(module, locationId is long l ? locationNames.GetValueOrDefault(l) : null);
            if (title.Length > 0) summary["title"] = title;
            if (Text(module["moduleOwners"]) is string owners) summary["owners"] = owners;
            if (Text(module["packageLabel"]) is string package) summary["packageLabel"] = package;
            if (locationId is not null) summary["operationLocationId"] = (module["operationLocationId"] ?? module["operationLocation"]!["id"])!.DeepClone();
        }
        if (placed["externalReferences"] is JsonObject references) summary["externalReferences"] = references.DeepClone();
        return summary;
    }

    /// <summary>The id of the module's location: by id when placed, else the nested location's.</summary>
    private static long? LocationId(JsonObject module) =>
        Number(module["operationLocationId"]) ?? Number(module["operationLocation"]?["id"]);

    private static Dictionary<long, string> LocationNames(JsonObject layout)
    {
        var names = new Dictionary<long, string>();
        if (layout["operationLocations"] is not JsonArray locations) return names;
        foreach (var location in locations.OfType<JsonObject>())
            if (Number(location["id"]) is long id && Text(location["name"]) is string name) names.TryAdd(id, name);
        return names;
    }

    /// <summary>
    /// The module's title as version 5 worked it out: with a prefix and a number, the designation followed by the name of
    /// its location; with a number only, its name followed by the number; else its name followed by its configuration label
    /// in lower case.
    /// </summary>
    private static string Title(JsonObject module, string? locationName)
    {
        var name = Text(module["name"]);
        var prefix = Text(module["prefix"]);
        var number = Number(module["number"]);
        if (number is long n && prefix is not null) return Join(string.Create(CultureInfo.InvariantCulture, $"{prefix} {n:0000}"), locationName);
        if (number is long m) return Join(name, m.ToString(CultureInfo.InvariantCulture));
        return Join(name, Text(module["configurationLabel"])?.ToLowerInvariant());
    }

    private static string Join(params string?[] parts) => string.Join(' ', parts.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p!.Trim()));

    private static string? Text(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<string>(out var text) && !string.IsNullOrWhiteSpace(text) ? text.Trim() : null;

    private static long? Number(JsonNode? node) =>
        node is JsonValue value && value.TryGetValue<long>(out var number) ? number : null;
}
