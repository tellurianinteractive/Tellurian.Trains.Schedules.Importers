using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;

namespace Tellurian.Trains.Schedules.Model.Layouts.Measures;

/// <summary>Compares derived measures with the entered Registry values and flags differences (MD-10).</summary>
public static class MeasureComparison
{
    /// <summary>One entry per entered value, in the order of <see cref="MeasureKind"/>.</summary>
    public static IReadOnlyList<MeasureDifference> Compare(ModuleMeasures derived, RegistryMeasures? entered, MeasureTolerances? tolerances = null)
    {
        ArgumentNullException.ThrowIfNull(derived);
        if (entered is null) return [];
        tolerances ??= MeasureTolerances.Default;

        // A straight module has no curve: the Registry has 0 or nothing as its radius.
        var radius = derived.Length is null ? null : derived.Radius ?? (double?)0;
        var differences = new List<MeasureDifference>();
        Add(MeasureKind.Length, derived.Length, entered.Length, tolerances.Length);
        Add(MeasureKind.Straight, derived.Straight, entered.Straight, tolerances.Length);
        Add(MeasureKind.Radius, radius, entered.Radius, tolerances.Length);
        Add(MeasureKind.Angle, derived.Angle, entered.Angle, tolerances.Angle);
        Add(MeasureKind.Width, derived.Width, entered.Width, tolerances.Length);
        Add(MeasureKind.NumberOfSections, derived.NumberOfSections, entered.NumberOfSections, 0);
        return differences;

        void Add(MeasureKind kind, double? value, double? enteredValue, double tolerance)
        {
            if (enteredValue is not { } e) return;
            var isFlagged = value is not { } d || Math.Abs(d - e) > tolerance + Tolerance.Length;
            differences.Add(new(kind, value, e, isFlagged));
        }
    }
}
