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
    private const string Tracks = "tracks";
    private const string Gauge = "gauge";
    private const string Gauges = "gauges";
    private const string Scale = "scale";
    private const string ShortName = "shortName";
    private const string NormalGauge = "normalGauge";
    private const string NarrowGauge = "narrowGauge";
    private const string Width = "width";
    private const string Standards = "standards";
    private const string EndProfiles = "endProfiles";

    /// <summary>Whether the step to the current schema version also runs on files of that version: true until version 2 is released.</summary>
    public const bool IsDraftUpgraded = true;

    /// <summary>The file kind of <paramref name="kind"/> with its migrations, for <see cref="FileEnvelope"/>.</summary>
    public static FileKind FileKind(DrawingFileKind kind) => DrawingFileFormat.FileKind(kind) with { Migrations = StepsOf(kind) };

    /// <summary>
    /// Brings an end profile, in its own file or embedded in another, from schema version 1 to 2: the Module Registry's
    /// id moves to the Registry's entry of <c>externalReferences</c> (<c>endProfileId</c>), and each track's
    /// <c>gauge</c> becomes a list, <c>gauges</c> (decision 13).
    /// </summary>
    public static void EndProfileToVersion2(JsonNode? endProfile)
    {
        MoveRegistryId(endProfile, ExternalReferences.EndProfileId);
        if ((endProfile as JsonObject)?[Tracks] is not JsonArray tracks) return;
        foreach (var track in tracks.OfType<JsonObject>())
            if (!track.ContainsKey(Gauges) && track.Remove(Gauge, out var gauge) && gauge is JsonValue) track[Gauges] = new JsonArray(gauge);
    }

    /// <summary>
    /// Brings a standard, in a catalogue or embedded in a hall plan, from schema version 1 to 2: the Module Registry's id
    /// moves to the Registry's entry of <c>externalReferences</c> (<c>standardId</c>); the scale, written as its short
    /// name, becomes the shared model's scale; <c>normalGauge</c> and <c>narrowGauge</c> become the list <c>gauges</c>,
    /// the normal gauge first (decisions 11 to 13).
    /// </summary>
    public static void StandardToVersion2(JsonNode? standard)
    {
        MoveRegistryId(standard, ExternalReferences.StandardId);
        if (standard is not JsonObject entry) return;
        if (entry[Scale] is JsonValue scale && scale.TryGetValue<string>(out var scaleName)) entry[Scale] = new JsonObject { [ShortName] = scaleName };
        var gauges = new JsonArray();
        foreach (var name in new[] { NormalGauge, NarrowGauge })
            if (entry.Remove(name, out var width) && width is JsonValue) gauges.Add((JsonNode)new JsonObject { [Width] = width });
        if (gauges.Count > 0 && !entry.ContainsKey(Gauges)) entry[Gauges] = gauges;
    }

    /// <summary>Brings a catalogue from schema version 1 to 2: its standards and end profiles.</summary>
    public static void CatalogueToVersion2(JsonNode? catalogue)
    {
        Each(catalogue, Standards, StandardToVersion2);
        Each(catalogue, EndProfiles, EndProfileToVersion2);
    }

    private static IReadOnlyList<FileMigration> StepsOf(DrawingFileKind kind) => kind switch
    {
        DrawingFileKind.EndProfile => [new(1, content => { EndProfileToVersion2(content); return content; })],
        DrawingFileKind.Catalogue => [new(1, content => { CatalogueToVersion2(content); return content; })],
        _ => throw new NotSupportedException($"The {DrawingFileFormat.Name(kind)} file is still read by Module Draw."),
    };

    private static void MoveRegistryId(JsonNode? node, string name)
    {
        if (node is JsonObject drawing && drawing.Remove(RegistryId, out var id) && id is JsonValue)
            RegistryEntry(drawing)[name] ??= id;
    }

    /// <summary>Applies <paramref name="migrate"/> to every object in the array <paramref name="name"/> of <paramref name="node"/>.</summary>
    private static void Each(JsonNode? node, string name, Action<JsonNode> migrate)
    {
        if ((node as JsonObject)?[name] is not JsonArray items) return;
        foreach (var item in items.OfType<JsonObject>().ToList()) migrate(item);
    }

    /// <summary>The Module Registry's entry in the external references of <paramref name="drawing"/>, created when missing.</summary>
    private static JsonObject RegistryEntry(JsonObject drawing)
    {
        if (drawing[ExternalReferences.Section] is not JsonObject section) drawing[ExternalReferences.Section] = section = [];
        if (section[ExternalReferences.ModuleRegistry] is not JsonObject entry) section[ExternalReferences.ModuleRegistry] = entry = [];
        return entry;
    }
}
