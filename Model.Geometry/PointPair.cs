namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>Two points, one from each set, that match within the tolerance.</summary>
public readonly record struct PointPair(int FirstIndex, int SecondIndex, double Distance);
