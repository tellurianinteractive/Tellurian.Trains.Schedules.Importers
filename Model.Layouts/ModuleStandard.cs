namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The standard a module is built to, e.g. "FREMO H0-RE". It is referred to by its short name; the Module Registry
/// defines the standards and has an id for each.
/// </summary>
public sealed class ModuleStandard : LayoutPart, IEquatable<ModuleStandard>
{
    /// <summary>The Module Registry's id of the standard; null when it isn't taken from there.</summary>
    public int? Id { get; set; }

    /// <summary>What the standard is called, e.g. "FREMO H0-RE". It is what identifies the standard to a reader.</summary>
    public required string ShortName { get; set; }

    /// <summary>The scale of the standard; null when not stated.</summary>
    public Scale? Scale { get; set; }

    /// <summary>Equal when they have the same short name, without regard to case: that is what identifies a standard.</summary>
    public bool Equals(ModuleStandard? other) => other is not null && string.Equals(ShortName, other.ShortName, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ModuleStandard other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(ShortName);

    /// <inheritdoc/>
    public override string ToString() => ShortName;
}
