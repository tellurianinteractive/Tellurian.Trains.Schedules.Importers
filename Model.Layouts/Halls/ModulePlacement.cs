using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>Where a module of a hall plan stands in the hall, worked out from its group's anchor along the lines (HP-5).</summary>
/// <param name="ModuleId">The <see cref="PlacedModule.Id"/>.</param>
/// <param name="Placement">From the module's coordinates to the hall's: rotated and moved, never mirrored.</param>
public sealed record ModulePlacement(int ModuleId, Transform Placement)
{
    /// <summary>The module's local origin and X axis in hall coordinates.</summary>
    public Pose Pose => Placement.Apply(Pose.Origin);

    /// <summary><paramref name="point"/> of the module, in hall coordinates.</summary>
    public Point ToHall(Point point) => Placement.Apply(point);

    /// <summary><paramref name="pose"/> of the module, such as an exit, in hall coordinates.</summary>
    public Pose ToHall(Pose pose) => Placement.Apply(pose);
}
