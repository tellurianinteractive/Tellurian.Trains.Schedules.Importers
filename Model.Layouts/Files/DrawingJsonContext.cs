using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Model.Files;

/// <summary>
/// Source-generated JSON metadata for the content of Module Draw's drawing files; trim and AOT safe. The envelope is
/// written by <see cref="FileEnvelope"/>. Enum values are written as in the code, e.g. <c>TwoRail</c>; values in
/// another case, as earlier versions wrote them, are read. Grows kind by kind as the drawing types move here
/// (<c>LayoutsLayerSurvey.md</c> §7 step 5).
/// </summary>
/// <remarks>
/// The options are those Module Draw's files have always been written with, so that a file read and written again is
/// unchanged. An app with drawing types of its own that embed these (Module Draw while its module and hall types
/// remain there) uses the same options and the public converters in <see cref="Layouts.Drawings"/>.
/// </remarks>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true,
    UseStringEnumConverter = true,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true,
    Converters =
    [
        typeof(PointJsonConverter),
        typeof(AngleJsonConverter),
        typeof(PoseJsonConverter),
    ])]
[JsonSerializable(typeof(EndProfile))]
[JsonSerializable(typeof(Tellurian.Trains.Schedules.Model.Layouts.Catalogues.Catalogue))]
[JsonSerializable(typeof(Tellurian.Trains.Schedules.Model.Layouts.Halls.Hall))]
[JsonSerializable(typeof(Tellurian.Trains.Schedules.Model.Layouts.Halls.HallPlan))]
[JsonSerializable(typeof(ModuleFileContent))]
[JsonSerializable(typeof(IReadOnlyList<DrawingRevision>))]
public sealed partial class DrawingJsonContext : JsonSerializerContext;
