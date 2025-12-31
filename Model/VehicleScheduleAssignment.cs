using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

public class VehicleScheduleAssignment
{
    public int Id { get; set; }
    public Sessions Sessions { get; set; } = Sessions.All;

    // FK property for EF Core - owning Vehicle (required)
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    // FK property for EF Core - the schedule this vehicle runs
    public int VehicleScheduleId { get; set; }
    public VehicleSchedule VehicleSchedule { get; set; } = default!;

    [JsonConstructor]
    private VehicleScheduleAssignment() { }

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


