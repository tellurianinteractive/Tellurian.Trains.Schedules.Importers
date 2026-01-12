using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a railway vehicle (locomotive or trainset) that can be assigned to trains.
/// </summary>
public class Vehicle
{
    /// <summary>
    /// Gets or sets the unique identifier for this vehicle.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the vehicle number.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets an optional external identifier for this vehicle.
    /// </summary>
    public string? ExternalId { get; set; }

    /// <summary>
    /// Gets or sets the number of units that make up this vehicle (for multiple-unit trainsets).
    /// </summary>
    public int NumberOfUnits { get; set; }

    /// <summary>
    /// Gets or sets an optional remark about this vehicle.
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the type of this vehicle.
    /// </summary>
    public VehicleType VehicleType { get; set; }

    /// <summary>
    /// Gets or sets the class designation of this vehicle.
    /// </summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this vehicle can operate in both directions.
    /// </summary>
    public bool IsDoubleDirected { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the owning company. Optional.
    /// </summary>
    public int? CompanyId { get; set; }

    /// <summary>
    /// Gets or sets the company that owns this vehicle.
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the owning schedule. Required.
    /// </summary>
    public int ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the schedule this vehicle belongs to.
    /// </summary>
    public Schedule Schedule { get; set; } = default!;

    /// <summary>
    /// Gets or sets the collection of schedule assignments for this vehicle.
    /// </summary>
    public ICollection<VehicleScheduleAssignment> ScheduleAssignments { get; set; }

    [JsonConstructor]
    private Vehicle()
    {
        ScheduleAssignments = new HashSet<VehicleScheduleAssignment>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Vehicle"/> with the specified values.
    /// </summary>
    /// <param name="id">The unique identifier for the vehicle.</param>
    /// <param name="vehicleType">The type of vehicle.</param>
    /// <param name="number">The vehicle number.</param>
    public Vehicle(int id, VehicleType vehicleType, int number)
    {
        Id = id;
        VehicleType = vehicleType;
        Number = number;
        ScheduleAssignments = new HashSet<VehicleScheduleAssignment>();
    }
}

/// <summary>
/// Specifies the type of railway vehicle.
/// </summary>
public enum VehicleType
{
    /// <summary>
    /// Vehicle type is not specified.
    /// </summary>
    Unknown,

    /// <summary>
    /// A locomotive that pulls or pushes other vehicles.
    /// </summary>
    Locomotive,

    /// <summary>
    /// A self-propelled trainset (multiple unit).
    /// </summary>
    Trainset,
}
