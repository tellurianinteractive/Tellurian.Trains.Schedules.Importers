using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.PlanFiles;

/// <summary>
/// Reads and writes a whole <see cref="Plan"/> as a plan file: an envelope that says what the file is and which
/// schema version its content has, around the plan itself. Every persistence path goes through here, browser
/// storage as well as files, so a plan of an older version is always brought up to date the same way
/// (<c>Documentation/ModelHarmonisation.md</c>, section 5).
/// </summary>
/// <remarks>
/// <code>
/// { "format": "tellurian-plan", "schemaVersion": 2, "appVersion": "3.12.0", "plan": { ... } }
/// </code>
/// A plan written before the envelope existed is the plan object alone, and is read as schema version 0.
/// A plan of a newer schema version than <see cref="CurrentSchemaVersion"/> is refused rather than read in part.
/// </remarks>
public static class PlanFile
{
    /// <summary>What the <c>format</c> of the envelope says.</summary>
    public const string Format = "tellurian-plan";

    /// <summary>The schema version this version writes, and the newest it reads.</summary>
    public const int CurrentSchemaVersion = 4;

    private const string FormatProperty = "format";
    private const string SchemaVersionProperty = "schemaVersion";
    private const string AppVersionProperty = "appVersion";
    private const string PlanProperty = "plan";

    // Schema version 0 was written with ReferenceHandler.Preserve and nests far; it is parsed before it is migrated.
    private const int MaxDepth = 256;

    // Source-generated metadata for the plan (PlanJson): no reflection, so it works trimmed and compiled ahead of time.
    private static readonly JsonTypeInfo<Plan> PlanType = (JsonTypeInfo<Plan>)PlanJson.CreateOptions().GetTypeInfo(typeof(Plan));
    private static readonly JsonTypeInfo<Plan> IndentedPlanType = (JsonTypeInfo<Plan>)PlanJson.CreateOptions(writeIndented: true).GetTypeInfo(typeof(Plan));

    /// <summary>The version of the model that writes, as recorded in the envelope.</summary>
    public static string AppVersion { get; } = VersionOf(typeof(Plan).Assembly);

    /// <summary><paramref name="plan"/> as a plan file of <see cref="CurrentSchemaVersion"/>.</summary>
    /// <param name="plan">The plan to write.</param>
    /// <param name="writeIndented">Whether to indent, for a file a person may read.</param>
    public static string Write(Plan plan, bool writeIndented = false)
    {
        ArgumentNullException.ThrowIfNull(plan);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = writeIndented, MaxDepth = MaxDepth }))
        {
            writer.WriteStartObject();
            writer.WriteString(FormatProperty, Format);
            writer.WriteNumber(SchemaVersionProperty, CurrentSchemaVersion);
            writer.WriteString(AppVersionProperty, AppVersion);
            writer.WritePropertyName(PlanProperty);
            JsonSerializer.Serialize(writer, plan, writeIndented ? IndentedPlanType : PlanType);
            writer.WriteEndObject();
        }
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    /// <summary>The plan in <paramref name="json"/>, a plan file of any schema version up to <see cref="CurrentSchemaVersion"/>.</summary>
    /// <exception cref="PlanFileException">The text is not a plan, or one this version can't read.</exception>
    public static Plan Read(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { MaxDepth = MaxDepth });
        }
        catch (JsonException ex)
        {
            throw new PlanFileException(PlanFileError.NotAPlan, $"The file is not valid JSON: {ex.Message}", ex);
        }
        return Read(root);
    }

    /// <summary>The plan in <paramref name="stream"/>; see <see cref="Read(string)"/>.</summary>
    /// <exception cref="PlanFileException">The text is not a plan, or one this version can't read.</exception>
    public static async Task<Plan> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return Read(await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <summary>The schema version of the plan file <paramref name="root"/>: what its envelope says, and 0 without one.</summary>
    /// <exception cref="PlanFileException">The envelope is of another format, or its version isn't a number.</exception>
    public static int SchemaVersionOf(JsonNode? root) => Open(root).Version;

    private static Plan Read(JsonNode? root)
    {
        var (version, content) = Open(root);
        if (version > CurrentSchemaVersion)
            throw new PlanFileException(PlanFileError.NewerVersion, $"The plan was saved by a newer version of the app (schema version {version}); this version reads up to schema version {CurrentSchemaVersion}.");
        content = PlanFileMigrations.Upgrade(content, version, CurrentSchemaVersion, PlanFileMigrations.All);
        try
        {
            return content.Deserialize(PlanType) ?? throw new PlanFileException(PlanFileError.NotAPlan, "The file holds no plan.");
        }
        catch (JsonException ex)
        {
            throw new PlanFileException(PlanFileError.NotAPlan, $"The plan can't be read: {ex.Message}", ex);
        }
    }

    private static (int Version, JsonNode Content) Open(JsonNode? root)
    {
        if (root is not JsonObject envelope) throw new PlanFileException(PlanFileError.NotAPlan, "The file holds no plan.");
        if (!envelope.TryGetPropertyValue(FormatProperty, out var format)) return (0, envelope);
        if (format?.GetValueKind() != JsonValueKind.String || format.GetValue<string>() != Format)
            throw new PlanFileException(PlanFileError.NotAPlan, $"The file is not a plan: its format is {format?.ToJsonString() ?? "null"}.");
        if (envelope[SchemaVersionProperty] is not JsonValue versionValue || !versionValue.TryGetValue<int>(out var version) || version < 0)
            throw new PlanFileException(PlanFileError.NotAPlan, "The plan file has no valid schema version.");
        if (envelope[PlanProperty] is not JsonObject content)
            throw new PlanFileException(PlanFileError.NotAPlan, "The plan file holds no plan.");
        return (version, content);
    }

    private static string VersionOf(Assembly assembly)
    {
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? string.Empty;
        var metadata = version.IndexOf('+', StringComparison.Ordinal);
        return metadata < 0 ? version : version[..metadata];
    }
}
