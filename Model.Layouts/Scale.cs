namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A model scale, e.g. "H0" at 1:87. A scale is a ratio only: the gauges belong to the standard
/// (<c>ModuleStandard.Gauges</c>), so H0 and H0m are one scale (decision 11 of <c>CrossAppReview.md</c>).
/// </summary>
public sealed class Scale : LayoutPart, IEquatable<Scale>
{
    /// <summary>The id of the scale in the suite's list of scales, e.g. 3 for H0; null when not taken from there.</summary>
    public int? Id { get; set; }

    /// <summary>What the scale is called, e.g. "H0". It is what identifies the scale to a reader.</summary>
    public required string ShortName { get; set; }

    /// <summary>What one unit of the model is of the real thing, e.g. 87 for 1:87 or 43.5 for 1:43.5; null when not stated.</summary>
    public decimal? Denominator { get; set; }

    /// <summary>What tells two scales of the same name apart, e.g. "NMRA"; null when nothing is needed.</summary>
    public string? Remark { get; set; }

    /// <summary>
    /// Equal when they have the same short name, without regard to case, and the same ratio: two scales can share a name,
    /// e.g. 0 at 1:45 and 1:48.
    /// </summary>
    public bool Equals(Scale? other) => other is not null && string.Equals(ShortName, other.ShortName, StringComparison.OrdinalIgnoreCase) && Denominator == other.Denominator;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Scale other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(ShortName), Denominator);

    /// <inheritdoc/>
    public override string ToString() => ShortName;
}
