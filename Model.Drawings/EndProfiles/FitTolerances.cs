using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

/// <summary>Tolerances and limits for comparing end profiles (spec §3.2).</summary>
public sealed record FitTolerances : ExtensibleRecord
{
    /// <summary>Three bolts hold two ends together with enough stability.</summary>
    public const int DefaultMinimumBoltedScrewHoles = 3;

    /// <summary>The default tolerances.</summary>
    public static FitTolerances Default { get; } = new();

    /// <summary>Track positions, gauges and screw holes, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public double Joint { get; init; } = 0.5;

    /// <summary>Ground line, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public double Landscape { get; init; } = 2.0;

    /// <summary>
    /// Ends whose screw holes don't all match can still be bolted when at least this many holes line up,
    /// e.g. a low and a high profile of the same family.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int MinimumBoltedScrewHoles { get; init; } = DefaultMinimumBoltedScrewHoles;
}
