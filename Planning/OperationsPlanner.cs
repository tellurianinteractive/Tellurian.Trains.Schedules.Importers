using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Timetables;

namespace Tellurian.Trains.Schedules.Planning;

/// <summary>
/// Represents a planner that provides functionality for creating and modify timetables and schedules.
/// </summary>
/// <remarks>
/// The planner shoul also provide a mechanism to notify a user interface about unresolved
/// planning conflicts.
/// </remarks>
public class OperationsPlanner(Schedule schedule, TimetableSettings timetableSettings)
{
    internal Schedule Schedule { get; } = schedule;
    internal Timetable Timetable => Schedule.Timetable;
    internal Layout Layout => Timetable.Layout;
    internal TimetableSettings TimetableSettings { get; } = timetableSettings;
}
