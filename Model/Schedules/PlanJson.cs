using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// The JSON settings of the content of a plan file of the current schema version. Read and write a
/// whole plan through <see cref="PlanFiles.PlanFile"/>, which adds the envelope and brings older plans up to date.
/// </summary>
/// <remarks>
/// Property names are camelCase and enum values are written by their name in the code
/// (<c>Documentation/OperationLocations.md</c>, decision 31). A plan is a tree: what a thing owns is nested in it,
/// everything else is referred to by id, and no parent is written (<see cref="PlanReferences"/>). Nothing is written
/// that is null or the default of its type; a property whose initial value is something else carries
/// <c>[JsonIgnore(Condition = JsonIgnoreCondition.Never)]</c> and is always written. The metadata is source-generated:
/// the layout's types from <c>Model.Layouts</c>, the plan's own from <see cref="PlanJsonContext"/>, so reading and
/// writing a plan needs no reflection.
/// </remarks>
public static class PlanJson
{
    /// <summary>
    /// Creates the options for reading and writing the content of a plan file.
    /// </summary>
    /// <param name="writeIndented">Whether to write indented JSON, for a plan saved to a file a person may read.</param>
    public static JsonSerializerOptions CreateOptions(bool writeIndented = false) =>
        new(PlanJsonContext.Default.Options)
        {
            WriteIndented = writeIndented,
            // The layout's types first: they are generated where their internal members are visible.
            TypeInfoResolver = JsonTypeInfoResolver.Combine(LayoutSerialization.Context, PlanJsonContext.Default),
        };
}

/// <summary>The source-generated JSON metadata of the plan's own types, with the same settings as the layout's.</summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    UseStringEnumConverter = true,
    AllowOutOfOrderMetadataProperties = true,
    IgnoreReadOnlyProperties = true,
    Converters =
    [
        typeof(Tellurian.Trains.Schedules.Model.Layouts.Drawings.PointJsonConverter),
        typeof(Tellurian.Trains.Schedules.Model.Layouts.Drawings.AngleJsonConverter),
        typeof(Tellurian.Trains.Schedules.Model.Layouts.Drawings.PoseJsonConverter),
    ])]
[JsonSerializable(typeof(Plan))]
internal sealed partial class PlanJsonContext : JsonSerializerContext;
