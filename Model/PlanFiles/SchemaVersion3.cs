using Tellurian.Trains.Schedules.Model.Files;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The migration of a plan from schema version 2 to 3 (<c>Documentation/ModelHarmonisation.md</c>, section 4, the
/// leftovers): the platforms the planner used to give on every read are given once, here, to the plans written before
/// there were platform lengths.
/// </summary>
/// <remarks>
/// A location that exchanges passengers, and where not one track has a platform, has every track given a platform of
/// 1 m: before platform lengths, every track of a passenger location served passengers, so the plan works as it did,
/// and the planner may shorten or clear the tracks that in truth have none. A location where a platform is recorded is
/// left alone, as is one that exchanges no passengers. From version 3 the plan says which tracks have a platform, as a
/// hall plan does, and nothing is added when it is read.
/// </remarks>
public static class SchemaVersion3
{
    /// <summary>The step from schema version 2.</summary>
    public static FileMigration Migration { get; } = new(2, Migrate);

    /// <summary>The platform length given, in metres: the planner's <c>StationTrack.DefaultPlatformLength</c> when version 3 was made.</summary>
    private const double PlatformLength = 1;

    private static JsonNode Migrate(JsonNode content)
    {
        if (content["timetable"]?["layout"]?["operationLocations"] is not JsonArray locations) return content;
        foreach (var location in locations.OfType<JsonObject>())
        {
            if (!ExchangesPassengers(location) || location["tracks"] is not JsonArray tracks || tracks.Count == 0) continue;
            if (tracks.OfType<JsonObject>().Any(t => Number(t["platformLength"]) > 0)) continue;
            foreach (var track in tracks.OfType<JsonObject>()) track["platformLength"] = PlatformLength;
        }
        return content;
    }

    // As the kinds were in version 2: a signal controlled location and an industrial area never exchange passengers, a
    // shadow station always does, and otherwise the flag says, true when it is missing.
    private static bool ExchangesPassengers(JsonObject location)
    {
        var kind = location["kind"] is JsonValue k && k.TryGetValue<string>(out var name) ? name : null;
        if (kind is "SignalControlled" or "IndustrialArea") return false;
        if (kind is "Station" && Flag(location["isShadow"]) == true) return true;
        return Flag(location["hasPassengerExchange"]) ?? true;
    }

    private static bool? Flag(JsonNode? node) => node is JsonValue value && value.TryGetValue<bool>(out var flag) ? flag : null;

    private static double Number(JsonNode? node) => node is JsonValue value && value.TryGetValue<double>(out var number) ? number : 0;
}
