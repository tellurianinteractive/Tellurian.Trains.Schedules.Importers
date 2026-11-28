using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings.Halls;

/// <summary>The point on the anchor module that its dimensions are measured from (HP-21).</summary>
public sealed record AnchorReferencePoint : ExtensibleRecord
{
    /// <summary>What the point is.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required ReferencePointKind Kind { get; init; }

    /// <summary>The exit of the anchor module, by side and number, for <see cref="ReferencePointKind.ExitCentre"/> and <see cref="ReferencePointKind.PlateCorner"/>.</summary>
    public ExitPlace? Exit { get; init; }

    /// <summary>The corner, for <see cref="ReferencePointKind.PlateCorner"/>.</summary>
    public PlateSide? Corner { get; init; }

    /// <summary>The point in module coordinates, mm, for <see cref="ReferencePointKind.MarkedPoint"/>.</summary>
    public Point? Point { get; init; }

    /// <summary>The centre of the end plate of <paramref name="exit"/>.</summary>
    public static AnchorReferencePoint ExitCentre(ExitPlace exit) =>
        new() { Kind = ReferencePointKind.ExitCentre, Exit = exit };

    /// <summary>The <paramref name="corner"/> of the end plate of <paramref name="exit"/>.</summary>
    public static AnchorReferencePoint PlateCorner(ExitPlace exit, PlateSide corner) =>
        new() { Kind = ReferencePointKind.PlateCorner, Exit = exit, Corner = corner };

    /// <summary>A point the planner has marked, in module coordinates.</summary>
    public static AnchorReferencePoint Marked(Point point) =>
        new() { Kind = ReferencePointKind.MarkedPoint, Point = point };
}
