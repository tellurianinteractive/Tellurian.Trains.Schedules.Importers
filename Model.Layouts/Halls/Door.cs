using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>A door or emergency exit in a wall, with a zone in front of it that must be kept free.</summary>
public sealed class Door : LayoutPart
{
    /// <summary>How far into the hall the zone in front of a door is kept free unless said otherwise, mm.</summary>
    public const double DefaultKeepFreeDepth = 1000;

    /// <summary>The identity of the door, unique among the hall's doors; given by <see cref="Hall.Add(Door)"/>.</summary>
    public int Id { get; set; }

    /// <summary>What the door is used for.</summary>
    public DoorKind Kind { get; set; }

    /// <summary>The centre of the opening on the wall, heading into the hall.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Pose Position { get; set; }

    /// <summary>The width of the opening, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Width { get; set; }

    /// <summary>How far into the hall the zone in front of the door must be kept free, mm.</summary>
    public double KeepFreeDepth { get; set; } = DefaultKeepFreeDepth;

    /// <summary>A label shown in the drawing.</summary>
    public string? Label { get; set; }

    /// <summary>True if modules can be carried in and out of the hall through this door.</summary>
    public bool IsModuleEntrance { get; set; }

    /// <summary>The keep-free zone: the width of the opening, <see cref="KeepFreeDepth"/> into the hall.</summary>
    public PathDrawing KeepFreeZone()
    {
        // The rectangle's X axis runs along the wall, to the right of the heading, and its Y axis into the hall.
        var corner = new Pose(Position.Position + Position.Left * (Width / 2), Position.Heading - Angle.Quarter);
        return PathDrawing.Rectangle(Width, KeepFreeDepth, Transform.FromPose(corner));
    }

    /// <inheritdoc/>
    public override Door Copy() => (Door)base.Copy();

    /// <inheritdoc/>
    public override string ToString() => Label ?? Kind.ToString();
}
