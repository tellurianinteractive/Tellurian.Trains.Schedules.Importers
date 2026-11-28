using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

/// <summary>The result of <see cref="JointFitEvaluator.Evaluate"/>.</summary>
public sealed record JointFitResult
{
    /// <summary>The final fit, after any override.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required JointFit Fit { get; init; }

    /// <summary>The fit the rules gave, before any override.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required JointFit RuleFit { get; init; }

    /// <summary>What differs between the two ends.</summary>
    public required FitDifferences Differences { get; init; }

    /// <summary>The catalogue override that replaced the rule result, if any.</summary>
    public EndProfileCompatibilityOverride? Override { get; init; }

    /// <summary>
    /// Screw holes without a partner, as indices into the screw holes of each end. There can be some
    /// even when the ends are bolted (enough other holes line up). Empty when the holes weren't compared geometrically.
    /// </summary>
    public IReadOnlyList<int> UnmatchedScrewHolesA { get; init; } = [];
    /// <summary>Screw holes of end B without a partner; see <see cref="UnmatchedScrewHolesA"/>.</summary>
    public IReadOnlyList<int> UnmatchedScrewHolesB { get; init; } = [];

    /// <summary>The largest ground-line deviation, in the coordinates of end B; null if not compared.</summary>
    public ShapeDeviation? GroundLineDeviation { get; init; }

    /// <summary>True when a catalogue override replaced the rule result.</summary>
    public bool IsOverridden => Override is not null;

    /// <summary>The join method that follows from the fit; null for incompatible ends.</summary>
    public JoinMethod? JoinMethod => JoinMethodFor(Fit);

    /// <summary>The join method that follows from <paramref name="fit"/>; null for <see cref="JointFit.Incompatible"/>.</summary>
    public static JoinMethod? JoinMethodFor(JointFit fit) => fit switch
    {
        JointFit.Fits or JointFit.FitsWithMismatch => EndProfiles.JoinMethod.Bolted,
        JointFit.FitsWithClamps => EndProfiles.JoinMethod.Clamped,
        _ => null,
    };
}
