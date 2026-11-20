using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The migration of a plan from schema version 3 to 4 (<c>Documentation/ModelHarmonisation.md</c>, section 4, the
/// leftovers).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>A location's <c>isSignal</c> is removed: it overlapped the signal controlled location and nothing used it.
/// Without this step it would be kept for ever among the properties the model doesn't know.</item>
/// <item>From version 4 a value that is the default of its property is left out (<see cref="Schedules.PlanJson"/>).
/// A version 3 plan has them written, which reads the same, so nothing else is changed here. A default that is
/// changed later takes a migration of its own, writing the old default where the value is missing.</item>
/// </list>
/// </remarks>
public static class SchemaVersion4
{
    /// <summary>The step from schema version 3.</summary>
    public static PlanFileMigration Migration { get; } = new(3, Migrate);

    private static JsonNode Migrate(JsonNode content)
    {
        if (content["timetable"]?["layout"]?["operationLocations"] is not JsonArray locations) return content;
        foreach (var location in locations.OfType<JsonObject>()) location.Remove("isSignal");
        return content;
    }
}
