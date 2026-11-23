namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>
/// The measurements that tell the people setting up where to put the anchor module (HP-21): the distances from a
/// reference point on the anchor to two walls or obstacles, and the angle to a wall. The distances and the angle are
/// worked out from the anchor's pose; typing them moves the anchor instead.
/// </summary>
public sealed record AnchorDimensions : ExtensibleRecord
{
    /// <summary>The point on the anchor module that is measured from.</summary>
    public required AnchorReferencePoint ReferencePoint { get; init; }

    /// <summary>The first wall or obstacle measured to.</summary>
    public required HallElementReference First { get; init; }

    /// <summary>The second wall or obstacle measured to.</summary>
    public required HallElementReference Second { get; init; }

    /// <summary>The index of the wall that the anchor's angle is given to.</summary>
    public int? AngleWall { get; init; }
}
