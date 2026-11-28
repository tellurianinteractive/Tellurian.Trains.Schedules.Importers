using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Modules;

/// <summary>A text label or simple marker in a module drawing. The pose's heading is the text direction.</summary>
public sealed record ModuleLabel : ExtensibleRecord
{
    /// <summary>What the label shows.</summary>
    public LabelKind Kind { get; init; }
    /// <summary>The text shown, if any.</summary>
    public string? Text { get; init; }
    /// <summary>Position in module coordinates; the heading is the text direction.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Pose { get; init; }

    /// <summary>For markers with an extent, such as a platform edge, mm.</summary>
    public double? Length { get; init; }
}
