namespace Tellurian.Trains.Schedules.Model.Drawings.Measures;

/// <summary>
/// A derived measure next to the entered Registry value (MD-10). <see cref="Derived"/> is null when the
/// drawing doesn't give the measure, e.g. without a main path; such a measure is always flagged.
/// </summary>
public sealed record MeasureDifference(MeasureKind Kind, double? Derived, double Entered, bool IsFlagged)
{
    /// <summary>Derived minus entered; null when there is no derived value.</summary>
    public double? Difference => Derived - Entered;
}
