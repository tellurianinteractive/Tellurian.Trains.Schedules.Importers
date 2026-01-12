using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents an assignment of a vehicle to a vehicle schedule for specific sessions.
/// </summary>
public class VehicleScheduleAssignment
{
    /// <summary>
    /// Gets or sets the unique identifier for this assignment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the sessions during which this assignment is active.
    /// </summary>
    public Sessions Sessions { get; set; } = Sessions.All;

    /// <summary>
    /// Gets or sets the foreign key to the vehicle. Required.
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle being assigned.
    /// </summary>
    public Vehicle Vehicle { get; set; } = default!;

    /// <summary>
    /// Gets or sets the foreign key to the vehicle schedule. Required.
    /// </summary>
    public int VehicleScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the vehicle schedule this vehicle is assigned to.
    /// </summary>
    public VehicleSchedule VehicleSchedule { get; set; } = default!;

    [JsonConstructor]
    private VehicleScheduleAssignment() { }

    /// <summary>
    /// Initializes a new instance of <see cref="VehicleScheduleAssignment"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the assignment.</param>
    /// <param name="vehicle">The vehicle being assigned.</param>
    /// <param name="vehicleSchedule">The vehicle schedule to assign to.</param>
    /// <param name="sessions">The sessions during which the assignment is active. Defaults to all sessions.</param>
    public VehicleScheduleAssignment(int id, Vehicle vehicle, VehicleSchedule vehicleSchedule, Sessions? sessions = null)
    {
        Id = id;
        Vehicle = vehicle;
        VehicleId = vehicle.Id;
        VehicleSchedule = vehicleSchedule;
        VehicleScheduleId = vehicleSchedule.Id;
        Sessions = sessions ?? Sessions.All;
    }
}
