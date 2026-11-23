using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// The migration of a plan from schema version 4 to 5: catalogue identity (<c>Documentation/LayoutsLayerSurvey.md</c>,
/// step 7).
/// </summary>
/// <remarks>
/// A standard's <c>id</c> is the catalogue's GUID (decision 12 of <c>CrossAppReview.md</c>); up to version 4 it was the
/// Module Registry's number. A number found there, in the layout's <c>preferredStandard</c> or a module's
/// <c>standard</c>, moves to the Registry's entry of the standard's <c>externalReferences</c> (<c>standardId</c>), where
/// every drawing file has it. A standard whose <c>id</c> is already a GUID is left as it is.
/// </remarks>
public static class SchemaVersion5
{
    /// <summary>The step from schema version 4.</summary>
    public static FileMigration Migration { get; } = new(4, Migrate);

    private const string Id = "id";

    private static JsonNode Migrate(JsonNode content)
    {
        if (content["timetable"]?["layout"] is not JsonObject layout) return content;
        RegistryIdToExternalReference(layout["preferredStandard"]);
        if (layout["modules"] is JsonArray modules)
            foreach (var placed in modules.OfType<JsonObject>()) RegistryIdToExternalReference(placed["module"]?["standard"]);
        return content;
    }

    private static void RegistryIdToExternalReference(JsonNode? node)
    {
        if (node is not JsonObject standard || standard[Id] is not JsonValue value || !value.TryGetValue<long>(out _)) return;
        standard.Remove(Id);
        if (standard[ExternalReferences.Section] is not JsonObject section) standard[ExternalReferences.Section] = section = [];
        if (section[ExternalReferences.ModuleRegistry] is not JsonObject entry) section[ExternalReferences.ModuleRegistry] = entry = [];
        entry[ExternalReferences.StandardId] ??= value;
    }
}
