namespace Tellurian.Trains.Schedules.Model.Settings;

/// <summary>
/// The orientation of the time axis in the graphical timetable.
/// </summary>
public enum TimeAxisOrientation
{
    /// <summary>Time runs horizontally (left to right); stations are stacked vertically.</summary>
    Horizontal,

    /// <summary>Time runs vertically (top to bottom); stations are spread horizontally.</summary>
    Vertical
}
