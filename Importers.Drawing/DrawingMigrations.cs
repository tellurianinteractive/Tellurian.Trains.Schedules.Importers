using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// The migrations of Module Draw's drawing files: the steps that bring the content of an older schema version up to
/// the current one, on the JSON tree. Written once and never changed, since files of each version exist. Only reading
/// needs them, so they are here and not in the model.
/// </summary>
/// <remarks>
/// Every step is idempotent: it finds the earlier shape by its properties and leaves content of today's shape as it is.
/// Until schema version 2 is released, the last step is also run on files of the current version, which development
/// builds wrote in earlier shapes (<see cref="IsDraftUpgraded"/>).
/// </remarks>
public static class DrawingMigrations
{
    private const string RegistryId = "registryId";

    /// <summary>Whether the step to the current schema version also runs on files of that version: true until version 2 is released.</summary>
    public const bool IsDraftUpgraded = true;

    /// <summary>The file kind of <paramref name="kind"/> with its migrations, for <see cref="FileEnvelope"/>.</summary>
    public static FileKind FileKind(DrawingFileKind kind) => DrawingFileFormat.FileKind(kind) with { Migrations = StepsOf(kind) };

    /// <summary>
    /// Brings an end profile, in its own file or embedded in another, from schema version 1 to 2: the Module Registry's
    /// id moves to the Registry's entry of <c>externalReferences</c> (<c>endProfileId</c>).
    /// </summary>
    public static void EndProfileToVersion2(JsonNode? endProfile) => MoveRegistryId(endProfile, ExternalReferences.EndProfileId);

    private static IReadOnlyList<FileMigration> StepsOf(DrawingFileKind kind) => kind switch
    {
        DrawingFileKind.EndProfile => [new(1, content => { EndProfileToVersion2(content); return content; })],
        _ => throw new NotSupportedException($"The {DrawingFileFormat.Name(kind)} file is still read by Module Draw."),
    };

    private static void MoveRegistryId(JsonNode? node, string name)
    {
        if (node is JsonObject drawing && drawing.Remove(RegistryId, out var id) && id is JsonValue)
            RegistryEntry(drawing)[name] ??= id;
    }

    /// <summary>The Module Registry's entry in the external references of <paramref name="drawing"/>, created when missing.</summary>
    private static JsonObject RegistryEntry(JsonObject drawing)
    {
        if (drawing[ExternalReferences.Section] is not JsonObject section) drawing[ExternalReferences.Section] = section = [];
        if (section[ExternalReferences.ModuleRegistry] is not JsonObject entry) section[ExternalReferences.ModuleRegistry] = entry = [];
        return entry;
    }
}
