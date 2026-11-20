using Tellurian.Trains.Schedules.Model.Files;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The migration of a plan from schema version 1 to 2 (<c>Documentation/ModelHarmonisation.md</c>, section 10, step 4):
/// the planner's locations became the shared model's (<c>Model.Layouts</c> 0.3), so they take its names.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The kind of a location is its type discriminator <c>"kind"</c> instead of <c>"$type"</c>, written first, with the same values.</item>
/// <item>A track's name is <c>"designation"</c> instead of <c>"number"</c>.</item>
/// <item>A location's country is its ISO code, <c>"countryCode"</c>, instead of the planner's catalogue id, <c>"countryId"</c>.
/// The codes are copied here as the catalogue had them, so the step doesn't depend on it.</item>
/// </list>
/// </remarks>
public static class SchemaVersion2
{
    /// <summary>The step from schema version 1.</summary>
    public static FileMigration Migration { get; } = new(1, Migrate);

    // The planner's country catalogue as it was when version 2 was made: id to ISO 3166-1 alpha-2 code.
    private static readonly Dictionary<int, string> CountryCodes = new()
    {
        [1] = "SE", [2] = "NO", [3] = "DK", [4] = "DE", [5] = "CH", [6] = "GB", [7] = "NL", [8] = "IT", [9] = "PL",
        [10] = "FR", [11] = "HU", [12] = "CZ", [13] = "SK", [14] = "AT",
        [101] = "US", [102] = "CA", [103] = "BE", [104] = "FI",
    };

    private static JsonNode Migrate(JsonNode content)
    {
        if (content["timetable"]?["layout"]?["operationLocations"] is not JsonArray locations) return content;
        for (var i = 0; i < locations.Count; i++)
        {
            if (locations[i] is not JsonObject location) continue;
            var migrated = new JsonObject();
            if (location["$type"] is { } kind) migrated["kind"] = kind.DeepClone();
            foreach (var (name, value) in location.ToList())
            {
                switch (name)
                {
                    case "$type":
                        break;
                    case "countryId":
                        if (value is JsonValue id && id.TryGetValue<int>(out var countryId) && CountryCodes.TryGetValue(countryId, out var code)) migrated["countryCode"] = code;
                        break;
                    case "tracks" when value is JsonArray tracks:
                        migrated[name] = new JsonArray([.. tracks.Select(Track)]);
                        break;
                    default:
                        location.Remove(name);
                        migrated[name] = value;
                        break;
                }
            }
            locations[i] = migrated;
        }
        return content;
    }

    private static JsonNode? Track(JsonNode? track)
    {
        if (track is not JsonObject value) return track?.DeepClone();
        var migrated = new JsonObject();
        foreach (var (name, property) in value) migrated[name == "number" ? "designation" : name] = property?.DeepClone();
        return migrated;
    }
}
