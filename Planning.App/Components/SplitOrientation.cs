namespace Tellurian.Trains.Schedules.Planning.App.Components;

/// <summary>
/// The orientation of a <see cref="SplitPane"/> / dock split.
/// <see cref="Horizontal"/> lays the two panes side by side (a row);
/// <see cref="Vertical"/> stacks them (a column).
/// </summary>
public enum SplitOrientation
{
    /// <summary>Panes side by side (left / right).</summary>
    Horizontal,

    /// <summary>Panes stacked (top / bottom).</summary>
    Vertical
}
