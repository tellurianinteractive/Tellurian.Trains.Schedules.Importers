namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A model scale, e.g. "H0". It is referred to by its short name; the Module Registry defines the scales and has an id
/// for each.
/// </summary>
public sealed class Scale : LayoutPart, IEquatable<Scale>
{
    /// <summary>The Module Registry's id of the scale; null when it isn't taken from there.</summary>
    public int? Id { get; set; }

    /// <summary>What the scale is called, e.g. "H0". It is what identifies the scale to a reader.</summary>
    public required string ShortName { get; set; }

    /// <summary>What one unit of the model is of the real thing, e.g. 87 for 1:87; null when not stated.</summary>
    public int? Denominator { get; set; }

    /// <summary>The normal gauge in millimetres, e.g. 16.5; null when not stated.</summary>
    public double? Gauge { get; set; }

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
