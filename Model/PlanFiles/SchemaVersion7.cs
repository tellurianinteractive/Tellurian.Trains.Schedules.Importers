using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The migration of a plan from schema version 6 to 7: the planner's own data leaves the shared layout for the
/// timetable (<c>Documentation/ModelSurfaceAnalysis.md</c>, step 3).
/// </summary>
/// <remarks>
/// The layout's <c>settings</c>, <c>companies</c>, <c>timetableStretches</c>, <c>dispatchStretches</c> and
/// <c>topologyPositions</c> move to the timetable. What the planner decided for each location (its timings, phone
/// number, instructions, hide flags, staffing, start labels, split dispatch list and passenger interchange) moves to the
/// timetable's <c>locationPlanning</c>, by the location's id; which way through its location each track is for
/// (<c>previousLocationId</c>, <c>nextLocationId</c>, <c>appliesInBothDirections</c>) moves to the timetable's
/// <c>stationTrackRoutes</c>, by the location's and the track's ids.
/// </remarks>
public static class SchemaVersion7
{
    /// <summary>The step from schema version 6.</summary>
    public static FileMigration Migration { get; } = new(6, Migrate);

    private static readonly string[] LayoutLists = ["settings", "companies", "timetableStretches", "dispatchStretches", "topologyPositions"];

    private static readonly string[] LocationProperties =
        ["timings", "phoneNumber", "instructions", "hideMeets", "hidePassings", "staffPosts", "shiftCount", "startLabels", "splitDispatchByDirection", "isPassengerInterchange"];

    private static readonly string[] TrackProperties = ["previousLocationId", "nextLocationId", "appliesInBothDirections"];

    private static JsonNode Migrate(JsonNode content)
    {
        if (content["timetable"] is not JsonObject timetable || timetable["layout"] is not JsonObject layout) return content;
        foreach (var name in LayoutLists)
        {
            if (!layout.TryGetPropertyValue(name, out var value)) continue;
            layout.Remove(name);
            if (!timetable.ContainsKey(name) && value is not null) timetable[name] = value;
        }
        var planning = new JsonArray();
        var routes = new JsonArray();
        if (layout["operationLocations"] is JsonArray locations)
        {
            foreach (var location in locations.OfType<JsonObject>())
            {
                if (Moved(location, LocationProperties, "locationId") is { } decided) planning.Add(decided);
                if (location["tracks"] is not JsonArray tracks) continue;
                foreach (var track in tracks.OfType<JsonObject>())
                {
                    // A track that names no location has no route, whatever else was written for it.
                    if (Moved(track, TrackProperties, "trackId") is not { } route || (route["previousLocationId"] is null && route["nextLocationId"] is null)) continue;
                    route["locationId"] = location["id"]?.DeepClone();
                    routes.Add(route);
                }
            }
        }
        if (planning.Count > 0) timetable["locationPlanning"] = planning;
        if (routes.Count > 0) timetable["stationTrackRoutes"] = routes;
        return content;
    }

    /// <summary>
    /// The <paramref name="properties"/> of <paramref name="owner"/>, taken out of it, in an object of their own keyed
    /// by the owner's id as <paramref name="key"/>; null when it has none of them.
    /// </summary>
    private static JsonObject? Moved(JsonObject owner, string[] properties, string key)
    {
        JsonObject? moved = null;
        foreach (var name in properties)
        {
            if (!owner.TryGetPropertyValue(name, out var value)) continue;
            owner.Remove(name);
            if (value is null) continue;
            moved ??= new JsonObject { [key] = owner["id"]?.DeepClone() };
            moved[name] = value;
        }
        return moved;
    }
}
