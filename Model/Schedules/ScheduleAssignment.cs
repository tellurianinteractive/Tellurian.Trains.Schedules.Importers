using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Represents an assignment of a vehicle to a vehicle schedule for specific sessions.
/// </summary>
public class ScheduleAssignment
{
    /// <summary>
    /// Gets or sets the unique identifier for this assignment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// A number of the schedule unique withing <see cref="Company"/>, or globally if company is not specified.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the sessions during which this assignment is active.
    /// </summary>
    public Sessions Sessions { get; set; } = Sessions.All;

    /// <summary>
    /// Gets or sets the foreign key to the vehicle. Required.
    /// </summary>
    public int ScheduledObjectId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle being assigned.
    /// </summary>
    public ScheduledObject ScheduledObject { get; set; } = default!;

    /// <summary>
    /// Gets or sets the foreign key to the vehicle schedule. Required.
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle schedule this vehicle is assigned to.
    /// </summary>
    public Schedule Schedule { get; set; } = default!;

    [JsonConstructor]
    private ScheduleAssignment() { }

    /// <summary>
    /// Initializes a new instance of <see cref="ScheduleAssignment"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the assignment.</param>
    /// <param name="scheduledObject">The vehicle being assigned.</param>
    /// <param name="schedule">The vehicle schedule to assign to.</param>
    /// <param name="sessions">The sessions during which the assignment is active. Defaults to all sessions.</param>
    public ScheduleAssignment(int id, ScheduledObject scheduledObject, Schedule schedule, Sessions? sessions = null)
    {
        Id = id;
        ScheduledObject = scheduledObject;
        ScheduledObjectId = scheduledObject.Id;
        Schedule = schedule;
        ScheduleId = schedule.Id;
        Sessions = sessions ?? Sessions.All;
    }
}
