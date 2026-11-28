namespace Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

/// <summary>
/// Which side of a plate's upper edge a fillet's centre is on, seen as the edge runs from left to right.
/// </summary>
public enum FilletSide
{
    /// <summary>The centre is above the edge: the fillet is hollow, like the inside of a corner.</summary>
    Above,

    /// <summary>The centre is below the edge: the fillet is rounded, like an outside corner.</summary>
    Below,
}
