using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Files;

/// <summary>
/// One kind of file of the suite, as its envelope says it: a plan, or one of Module Draw's drawing files. Every file is
/// read and written through <see cref="FileEnvelope"/> with its kind, so that each is brought up to date the same way.
/// </summary>
/// <remarks>
/// <code>
/// { "format": "tellurian-plan", "schemaVersion": 4, "appVersion": "3.12.0", "plan": { … } }
/// { "format": "tellurian-drawing", "kind": "HallPlan", "schemaVersion": 2, "appVersion": "0.9.0", "content": { … } }
/// </code>
/// </remarks>
public sealed record FileKind
{
    /// <summary>A name for messages, e.g. "plan" or "hall plan".</summary>
    public required string Name { get; init; }

    /// <summary>What the envelope's <c>format</c> says, e.g. <c>tellurian-plan</c>.</summary>
    public required string Format { get; init; }

    /// <summary>What the envelope's <c>kind</c> says, e.g. <c>HallPlan</c>; null for a format with one kind of file and no <c>kind</c>.</summary>
    public string? Kind { get; init; }

    /// <summary>The name of the property that holds the content: <c>plan</c> in a plan file, <c>content</c> in a drawing file.</summary>
    public required string ContentProperty { get; init; }

    /// <summary>The schema version written, and the newest read.</summary>
    public required int CurrentSchemaVersion { get; init; }

    /// <summary>The oldest schema version read; an envelope saying an older one is refused.</summary>
    public int LowestSchemaVersion { get; init; }

    /// <summary>
    /// Whether a file without an envelope, the content alone, is read as the lowest schema version, as a plan written
    /// before the envelope existed is. Otherwise such a file is refused.
    /// </summary>
    public bool ReadsWithoutEnvelope { get; init; }

    /// <summary>The steps that bring the content from one schema version to the next, one per version from the lowest.</summary>
    public IReadOnlyList<FileMigration> Migrations { get; init; } = [];
}

/// <summary>
/// One step of a file kind's migrations: from <paramref name="FromVersion"/> to the next, on the content as a JSON tree.
/// Written once and never changed, since files of that version exist.
/// </summary>
/// <param name="FromVersion">The schema version the step starts from.</param>
/// <param name="Migrate">Changes the content of that version into the next; may change the node in place and return it.</param>
public sealed record FileMigration(int FromVersion, Func<JsonNode, JsonNode> Migrate);

/// <summary>What the envelope of a file says, read without its content.</summary>
/// <param name="Kind">The envelope's <c>kind</c>, or null when it has none.</param>
/// <param name="SchemaVersion">The schema version of the content.</param>
/// <param name="AppVersion">The version of the program that wrote it, or null when not said (or no envelope).</param>
/// <param name="HasEnvelope">False for a file that is the content alone.</param>
public sealed record FileHeader(string? Kind, int SchemaVersion, string? AppVersion, bool HasEnvelope);
