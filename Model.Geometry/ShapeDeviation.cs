namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>The result of <see cref="ShapeComparison.MaximumDeviation"/>: the deviation and where it occurs.</summary>
public readonly record struct ShapeDeviation(double Maximum, Point At)
{
    /// <summary>True if the maximum deviation is at most <paramref name="tolerance"/> millimetres.</summary>
    public bool IsWithin(double tolerance) => Maximum <= tolerance;
}
