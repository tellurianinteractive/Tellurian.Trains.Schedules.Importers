namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The side of a location that a stretch leaves from, as seen by a viewer standing on the viewing side of the
/// module. Up is the side away from the viewer and down the side towards the viewer; they are for a line that
/// leaves across the left to right direction, such as a branch at a right angle.
/// </summary>
public enum ExitSide
{
    /// <summary>Not known.</summary>
    Undefined = 0,

    /// <summary>On the viewer's left.</summary>
    Left = 1,

    /// <summary>On the viewer's right.</summary>
    Right = 2,

    /// <summary>On the side away from the viewer.</summary>
    Up = 3,

    /// <summary>On the side towards the viewer.</summary>
    Down = 4,
}
