using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Drawings;

/// <summary>
/// A circular step around <see cref="Center"/>, turning <see cref="Sweep"/> degrees
/// (positive counter-clockwise). The radius is the distance from the start to the centre.
/// </summary>
public sealed record ArcStep(Point Center, double Sweep) : PathStep
{
    /// <inheritdoc/>
    public override Segment ToSegment(Point start) =>
        new ArcSegment(Center, start.DistanceTo(Center), (start - Center).Direction, Sweep);
}
