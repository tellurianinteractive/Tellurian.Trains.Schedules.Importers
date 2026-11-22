using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;

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
/// A plan of a newer schema version than <see cref="CurrentSchemaVersion"/> is refused rather than read in part. The
/// envelope and the chain of migrations are the suite's, shared with every other kind of file (<see cref="FileEnvelope"/>).
/// </remarks>
public static class PlanFile
{
    /// <summary>What the <c>format</c> of the envelope says.</summary>
    public const string Format = "tellurian-plan";

    /// <summary>The schema version this version writes, and the newest it reads.</summary>
    public const int CurrentSchemaVersion = 4;

    /// <summary>What a plan file is: its envelope, its schema versions and its migrations (<see cref="FileEnvelope"/>).</summary>
    public static FileKind Kind { get; } = new()
    {
        Name = "plan",
        Format = Format,
        ContentProperty = "plan",
        CurrentSchemaVersion = CurrentSchemaVersion,
        ReadsWithoutEnvelope = true,
        Migrations = PlanFileMigrations.All,
    };

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
        var type = writeIndented ? IndentedPlanType : PlanType;
        return FileEnvelope.Write(Kind, AppVersion, writer => JsonSerializer.Serialize(writer, plan, type), writeIndented);
    }

    /// <summary>The plan in <paramref name="json"/>, a plan file of any schema version up to <see cref="CurrentSchemaVersion"/>.</summary>
    /// <exception cref="PlanFileException">The text is not a plan, or one this version can't read.</exception>
    public static Plan Read(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        JsonNode? root;
        try
        {
            root = FileEnvelope.Parse(json);
        }
        catch (FileFormatException ex)
        {
            throw ToPlanFileException(ex);
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
    public static int SchemaVersionOf(JsonNode? root)
    {
        try
        {
            return FileEnvelope.ReadHeader(Kind, root).SchemaVersion;
        }
        catch (FileFormatException ex)
        {
            throw ToPlanFileException(ex);
        }
    }

    /// <summary>The plan file's own exception for <paramref name="exception"/> of the shared file mechanism.</summary>
    internal static PlanFileException ToPlanFileException(FileFormatException exception) =>
        new(exception.Error switch
        {
            NewerVersion => PlanFileError.NewerVersion,
            MigrationMissing => PlanFileError.MigrationMissing,
            InvalidJson or NotThisFormat or WrongKind or InvalidContent => PlanFileError.NotAPlan,
        }, exception.Message, exception);

    private static Plan Read(JsonNode? root)
    {
        JsonNode content;
        try
        {
            content = FileEnvelope.ReadContent(Kind, root);
        }
        catch (FileFormatException ex)
        {
            throw ToPlanFileException(ex);
        }
        try
        {
            return content.Deserialize(PlanType) ?? throw new PlanFileException(PlanFileError.NotAPlan, "The file holds no plan.");
        }
        catch (JsonException ex)
        {
            throw new PlanFileException(PlanFileError.NotAPlan, $"The plan can't be read: {ex.Message}", ex);
        }
    }

    private static string VersionOf(Assembly assembly)
    {
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? assembly.GetName().Version?.ToString() ?? string.Empty;
        var metadata = version.IndexOf('+', StringComparison.Ordinal);
        return metadata < 0 ? version : version[..metadata];
    }
}
