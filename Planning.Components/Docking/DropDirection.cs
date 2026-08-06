namespace Tellurian.Trains.Schedules.Planning.Components.Docking;

/// <summary>
/// Where a dragged view is dropped relative to a target pane.
/// </summary>
public enum DropDirection
{
    /// <summary>Dock to the left of the target (new pane becomes the left column).</summary>
    Left,

    /// <summary>Dock to the right of the target.</summary>
    Right,

    /// <summary>Dock above the target (new pane becomes the top row).</summary>
    Top,

    /// <summary>Dock below the target.</summary>
    Bottom
}
