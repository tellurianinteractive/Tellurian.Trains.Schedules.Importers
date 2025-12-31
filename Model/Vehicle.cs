using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

public class Vehicle
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string? ExternalId { get; set; }
    public int NumberOfUnits { get; set; }
    public string? Remark { get; set; }
    public VehicleType VehicleType { get; set; }
    public string Class { get; set; } = string.Empty;
    public bool IsDoubleDirected { get; set; }

    // FK property for EF Core
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    // FK property for EF Core - owning Schedule (required)
    public int ScheduleId { get; set; }
    public Schedule Schedule { get; set; } = default!;

    public ICollection<VehicleScheduleAssignment> ScheduleAssignments { get; set; }

    [JsonConstructor]
    private Vehicle()
    {
        ScheduleAssignments = new HashSet<VehicleScheduleAssignment>();
    }

    public Vehicle(int id, VehicleType vehicleType, int number)
    {
        Id = id;
        VehicleType = vehicleType;
        Number = number;
        ScheduleAssignments = new HashSet<VehicleScheduleAssignment>();
    }
}

public enum VehicleType
{
    Unknown,
    Locomotive,
    Trainset,
}
