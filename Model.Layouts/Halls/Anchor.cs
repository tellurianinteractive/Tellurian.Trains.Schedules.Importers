using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>
/// Where a connected group of a hall plan's layout stands in the hall: the pose of one of its modules, from which the
/// poses of all the others are worked out along the lines (HP-5). One per main line.
/// </summary>
public sealed class Anchor : LayoutPart
{
    /// <summary>The <see cref="Line.Id"/> of the main line whose group this places.</summary>
    public int LineId { get; set; }

    /// <summary>The <see cref="LayoutModule.Id"/> of the module the group is placed by; by default the first one added.</summary>
    public int ModuleId { get; set; }

    /// <summary>The anchor module's local origin and X axis in hall coordinates, mm and degrees.</summary>
    public Pose Pose { get; set; } = Pose.Origin;

    /// <summary>A locked anchor can't be moved or rotated (HP-5).</summary>
    public bool IsLocked { get; set; }

    /// <summary>How the people setting up find the anchor's position (HP-21).</summary>
    public AnchorDimensions? Dimensions { get; set; }

    /// <inheritdoc/>
    public override Anchor Copy() => (Anchor)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => $"{LineId}:{ModuleId} {Pose}";
}
