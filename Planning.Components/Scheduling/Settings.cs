using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning.Components.Scheduling;

/// <summary>
/// Combined settings for timetable planning, including both timetable parameters and graph display settings.
/// </summary>
public record Settings
{
    public TimetableSettings Timetable { get; set; } = TimetableSettings.Default;
    public GraphSettings Graph { get; set; } = GraphSettings.Default;
}
