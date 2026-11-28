using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>A circle, e.g. a screw hole or a dowel in an end profile.</summary>
public sealed record Circle(Point Center, double Diameter) : ExtensibleRecord
{
    /// <summary>The same circle mirrored about the centre line; only for comparing end profiles.</summary>
    public Circle MirroredX() => this with { Center = Center.MirroredX() };
}
