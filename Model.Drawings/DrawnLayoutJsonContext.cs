using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>
/// The source-generated JSON metadata of what a drawn layout adds to the operating layout in a layout block
/// (<see cref="DrawnLayout.ToBlock"/>): the modules with their drawings, the lines, joints, end profiles, standards,
/// overrides and fit tolerances. The options are the layout's own (<c>LayoutJsonContext</c>), with the converters of the
/// geometry the drawings are made of.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    UseStringEnumConverter = true,
    AllowOutOfOrderMetadataProperties = true,
    IgnoreReadOnlyProperties = true,
    Converters =
    [
        typeof(PointJsonConverter),
        typeof(AngleJsonConverter),
        typeof(PoseJsonConverter),
    ])]
[JsonSerializable(typeof(ICollection<PlacedModule>))]
[JsonSerializable(typeof(ICollection<Line>))]
[JsonSerializable(typeof(ICollection<Joint>))]
[JsonSerializable(typeof(ICollection<EndProfile>))]
[JsonSerializable(typeof(ICollection<ModuleStandard>))]
[JsonSerializable(typeof(ICollection<EndProfileCompatibilityOverride>))]
[JsonSerializable(typeof(FitTolerances))]
[JsonSerializable(typeof(ModuleStandard))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
internal sealed partial class DrawnLayoutJsonContext : JsonSerializerContext;
