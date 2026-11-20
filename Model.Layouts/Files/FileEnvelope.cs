using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Files;

/// <summary>
/// The envelope every file of the suite has, and the chain of migrations that brings its content up to date: one
/// mechanism for the plan file and Module Draw's drawing files (<c>Documentation/LayoutsLayerSurvey.md</c>, section 4.2).
/// What a file kind is, is said by a <see cref="FileKind"/>; reading the content into objects is the caller's.
/// </summary>
/// <remarks>
/// The envelope is written first, in a fixed order (<c>format</c>, <c>kind</c> where the kind has one,
/// <c>schemaVersion</c>, <c>appVersion</c>), then the content. Properties of the envelope this version doesn't know are
/// not kept. A file of a newer schema version than the kind's current one is refused rather than read in part.
/// </remarks>
public static class FileEnvelope
{
    /// <summary>How deep a file may nest. Older plans were written with reference handling and nest far.</summary>
    public const int MaxDepth = 256;

    private const string FormatProperty = "format";
    private const string KindProperty = "kind";
    private const string SchemaVersionProperty = "schemaVersion";
    private const string AppVersionProperty = "appVersion";

    /// <summary>Parses <paramref name="json"/> as the JSON tree of a file.</summary>
    /// <exception cref="FileFormatException">The text is not valid JSON (<see cref="InvalidJson"/>).</exception>
    public static JsonNode? Parse(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            return JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { MaxDepth = MaxDepth });
        }
        catch (JsonException ex)
        {
            throw new FileFormatException(new InvalidJson(ex.Message), $"The file is not valid JSON: {ex.Message}", ex);
        }
    }

    /// <summary>What the envelope of <paramref name="root"/> says, as a file of <paramref name="kind"/>, without reading the content.</summary>
    /// <exception cref="FileFormatException">The file is not of <paramref name="kind"/>, or its envelope is incomplete.</exception>
    public static FileHeader ReadHeader(FileKind kind, JsonNode? root) => Open(kind, root).Header;

    /// <summary>
    /// The content of <paramref name="root"/>, a file of <paramref name="kind"/> of any schema version from the lowest to the
    /// current one, brought up to the current schema version by the kind's migrations.
    /// </summary>
    /// <exception cref="FileFormatException">The file is not of <paramref name="kind"/>, is newer, or can't be brought up to date.</exception>
    public static JsonNode ReadContent(FileKind kind, JsonNode? root)
    {
        var (header, content) = Open(kind, root);
        if (header.SchemaVersion > kind.CurrentSchemaVersion)
            throw new FileFormatException(new NewerVersion(header.SchemaVersion, kind.CurrentSchemaVersion),
                $"The {kind.Name} was saved by a newer version of the app (schema version {header.SchemaVersion}); this version reads up to schema version {kind.CurrentSchemaVersion}.");
        return Upgrade(kind, content, header.SchemaVersion, kind.CurrentSchemaVersion);
    }

    /// <summary>Brings <paramref name="content"/> from <paramref name="fromVersion"/> to <paramref name="toVersion"/>, one step at a time.</summary>
    /// <exception cref="FileFormatException">A step is missing (<see cref="MigrationMissing"/>).</exception>
    public static JsonNode Upgrade(FileKind kind, JsonNode content, int fromVersion, int toVersion)
    {
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentNullException.ThrowIfNull(content);
        for (var version = fromVersion; version < toVersion; version++)
        {
            var step = kind.Migrations.FirstOrDefault(m => m.FromVersion == version)
                ?? throw new FileFormatException(new MigrationMissing(version), $"There is no migration of a {kind.Name} from schema version {version}.");
            content = step.Migrate(content);
        }
        return content;
    }

    /// <summary>
    /// A file of <paramref name="kind"/> at its current schema version: the envelope, then the content written by
    /// <paramref name="writeContent"/> as one JSON value.
    /// </summary>
    /// <param name="kind">The kind of file.</param>
    /// <param name="appVersion">The version of the program that writes, recorded in the envelope.</param>
    /// <param name="writeContent">Writes the content, e.g. through source-generated metadata.</param>
    /// <param name="writeIndented">Whether to indent, for a file a person may read.</param>
    public static string Write(FileKind kind, string appVersion, Action<Utf8JsonWriter> writeContent, bool writeIndented = false)
    {
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentNullException.ThrowIfNull(writeContent);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = writeIndented, MaxDepth = MaxDepth }))
        {
            writer.WriteStartObject();
            writer.WriteString(FormatProperty, kind.Format);
            if (kind.Kind is not null) writer.WriteString(KindProperty, kind.Kind);
            writer.WriteNumber(SchemaVersionProperty, kind.CurrentSchemaVersion);
            writer.WriteString(AppVersionProperty, appVersion);
            writer.WritePropertyName(kind.ContentProperty);
            writeContent(writer);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static (FileHeader Header, JsonNode Content) Open(FileKind kind, JsonNode? root)
    {
        ArgumentNullException.ThrowIfNull(kind);
        if (root is not JsonObject envelope) throw NotThisFormat($"The file holds no {kind.Name}.");
        if (!envelope.TryGetPropertyValue(FormatProperty, out var format))
        {
            if (kind.ReadsWithoutEnvelope) return (new FileHeader(null, kind.LowestSchemaVersion, null, HasEnvelope: false), envelope);
            throw NotThisFormat($"The file is not a {kind.Name}: it has no envelope.");
        }
        if (format?.GetValueKind() != JsonValueKind.String || format.GetValue<string>() != kind.Format)
            throw NotThisFormat($"The file is not a {kind.Name}: its format is {format?.ToJsonString() ?? "null"}.");
        var fileKind = envelope[KindProperty] is JsonValue kindValue && kindValue.TryGetValue<string>(out var kindName) ? kindName : null;
        if (kind.Kind is not null && !string.Equals(fileKind, kind.Kind, StringComparison.OrdinalIgnoreCase))
            throw new FileFormatException(new WrongKind(fileKind, kind.Kind), $"The file is a {fileKind ?? "file of no kind"}, not a {kind.Name}.");
        if (envelope[SchemaVersionProperty] is not JsonValue versionValue || !versionValue.TryGetValue<int>(out var version) || version < kind.LowestSchemaVersion)
            throw NotThisFormat($"The {kind.Name} file has no valid schema version.");
        var appVersion = envelope[AppVersionProperty] is JsonValue appValue && appValue.TryGetValue<string>(out var app) ? app : null;
        if (envelope[kind.ContentProperty] is not JsonObject content)
            throw NotThisFormat($"The {kind.Name} file holds no {kind.Name}.");
        return (new FileHeader(fileKind, version, appVersion, HasEnvelope: true), content);
    }

    private static FileFormatException NotThisFormat(string reason) => new(new NotThisFormat(reason), reason);
}
