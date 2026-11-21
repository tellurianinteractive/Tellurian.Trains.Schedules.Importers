namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// The result of <see cref="SegmentPath.Project"/>: <see cref="Distance"/> along the path to the closest
/// point, the <see cref="Offset"/> of the projected point to the left of the direction of travel
/// (negative: right), and the <see cref="Pose"/> on the path at the closest point.
/// </summary>
public readonly record struct PathProjection(double Distance, double Offset, Pose Pose);
