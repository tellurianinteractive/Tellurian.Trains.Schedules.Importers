using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Drawings.Measures;

/// <summary>
/// Measures derived from a module drawing's drawing (MD-10), comparable with the Registry values.
/// Lengths in mm, angles in degrees. Measures that depend on the main path are null without one.
/// </summary>
public sealed record ModuleMeasures : ExtensibleRecord
{
    /// <summary>The length of the main path: arc length + straight length (Registry <c>TotalLength</c>).</summary>
    public double? Length { get; init; }

    /// <summary>The total length of the straight parts of the main path.</summary>
    public double? Straight { get; init; }

    /// <summary>The radius of the main path's curves; null if it has none, or curves of different radii.</summary>
    public double? Radius { get; init; }

    /// <summary>The total angle the main path's curves turn, regardless of direction.</summary>
    public double? Angle { get; init; }

    /// <summary>The widest part of the module: the outline's maximum extent perpendicular to the main path.</summary>
    public double? Width { get; init; }

    /// <summary>The sum of the lengths of all track routes.</summary>
    public double TrackLength { get; init; }

    /// <summary>The number of sections in the drawing.</summary>
    public int NumberOfSections { get; init; }

    /// <summary>For ramp modules: the highest minus the lowest exit rail-top offset; null otherwise (MD-7a).</summary>
    public double? RampHeightDifference { get; init; }

    /// <summary>
    /// For ramp modules: the gradient along the main path from the first exit to the second, in per mille
    /// (positive rising); null without a main path or when the module isn't a ramp.
    /// </summary>
    public double? RampGradient { get; init; }

    /// <summary>True if the exits have different rail-top offsets.</summary>
    [JsonIgnore]
    public bool IsRamp => RampHeightDifference is not null;
}
