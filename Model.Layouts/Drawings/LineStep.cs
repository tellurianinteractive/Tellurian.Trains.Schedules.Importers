using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Drawings;

/// <summary>A straight step to <see cref="End"/>.</summary>
public sealed record LineStep(Point End) : PathStep
{
    /// <inheritdoc/>
    public override Segment ToSegment(Point start) => new LineSegment(start, End);
}
