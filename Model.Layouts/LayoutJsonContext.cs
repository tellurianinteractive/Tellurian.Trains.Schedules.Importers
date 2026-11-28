using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The source-generated JSON metadata of the layout's types, so that reading and writing needs no reflection and works
/// when trimmed or compiled ahead of time. How each type is written is said by attributes on the type itself.
/// </summary>
/// <remarks>
/// Property names are camelCase and enum values are written by their name in the code. A value is not written when it
/// is the default of its type; a property whose initial value is something else carries
/// <c>[JsonIgnore(Condition = JsonIgnoreCondition.Never)]</c>, so that it is always written. A location's
/// <c>kind</c> is read wherever it stands in the object. Properties without a setter are worked out and not written.
/// The plan file uses these options too (<c>PlanJson</c>), combined with the plan's own context.
/// </remarks>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    UseStringEnumConverter = true,
    AllowOutOfOrderMetadataProperties = true,
    IgnoreReadOnlyProperties = true)]
[JsonSerializable(typeof(Layout))]
[JsonSerializable(typeof(OperationLocation))]
[JsonSerializable(typeof(LockKey))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
internal sealed partial class LayoutJsonContext : JsonSerializerContext;
