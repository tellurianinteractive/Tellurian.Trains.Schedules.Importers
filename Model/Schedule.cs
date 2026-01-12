using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a complete railway schedule containing timetables, vehicles, vehicle schedules, and driver duties.
/// </summary>
/// <remarks>
/// A schedule is the top-level container for all railway operation planning data.
/// It associates a timetable with the vehicles and personnel needed to operate the trains.
/// </remarks>
public class Schedule : IEquatable<Schedule>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Schedule()
    {
        Name = string.Empty;
        Timetable = default!;
        Vehicles = [];
        VehicleSchedules = [];
        DriverDuties = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Schedule"/> with the specified name and timetable.
    /// </summary>
    /// <param name="name">The name of the schedule.</param>
    /// <param name="timetable">The timetable associated with this schedule.</param>
    public Schedule(string name, Timetable timetable)
    {
        Name = name;
        Timetable = timetable;
        TimetableId = timetable.Id;
        Vehicles = [];
        VehicleSchedules = [];
        DriverDuties = [];
    }

    /// <summary>
    /// Creates a new schedule with the specified name and timetable.
    /// </summary>
    /// <param name="name">The name of the schedule.</param>
    /// <param name="timetable">The timetable associated with this schedule.</param>
    /// <returns>A new <see cref="Schedule"/> instance.</returns>
    public static Schedule Create(string name, Timetable timetable) =>
        new(name, timetable);

    /// <summary>
    /// Gets or sets the unique identifier for this schedule.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this schedule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the foreign key to the associated timetable.
    /// </summary>
    public int TimetableId { get; set; }

    /// <summary>
    /// Gets or sets the timetable associated with this schedule.
    /// </summary>
    public Timetable Timetable { get; set; }

    /// <summary>
    /// Gets or sets the collection of vehicles available in this schedule.
    /// </summary>
    public ICollection<Vehicle> Vehicles { get; set; }

    /// <summary>
    /// Gets or sets the collection of vehicle schedules defining how vehicles are assigned to trains.
    /// </summary>
    public ICollection<VehicleSchedule> VehicleSchedules { get; set; }

    /// <summary>
    /// Gets or sets the collection of driver duties for this schedule.
    /// </summary>
    public ICollection<DriverDuty> DriverDuties { get; set; }

    /// <inheritdoc/>
    public bool Equals(Schedule? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Schedule other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides extension methods for <see cref="Schedule"/>.
/// </summary>
public static class ScheduleExtensions
{
    /// <summary>
    /// Adds a vehicle to the schedule.
    /// </summary>
    /// <param name="me">The schedule to add the vehicle to.</param>
    /// <param name="vehicle">The vehicle to add.</param>
    /// <returns>The added vehicle.</returns>
    public static Vehicle AddVehicle(this Schedule me, Vehicle vehicle)
    {
        me = me.ValueOrException(nameof(me));
        vehicle = vehicle.ValueOrException(nameof(vehicle));
        if (!me.Vehicles.Contains(vehicle))
        {
            vehicle.Schedule = me;
            vehicle.ScheduleId = me.Id;
            me.Vehicles.Add(vehicle);
        }
        return vehicle;
    }

    /// <summary>
    /// Adds a vehicle schedule to the schedule.
    /// </summary>
    /// <param name="me">The schedule to add the vehicle schedule to.</param>
    /// <param name="vehicleSchedule">The vehicle schedule to add.</param>
    /// <returns>The added vehicle schedule.</returns>
    public static VehicleSchedule AddVehicleSchedule(this Schedule me, VehicleSchedule vehicleSchedule)
    {
        me = me.ValueOrException(nameof(me));
        vehicleSchedule = vehicleSchedule.ValueOrException(nameof(vehicleSchedule));
        if (!me.VehicleSchedules.Contains(vehicleSchedule))
        {
            vehicleSchedule.Schedule = me;
            vehicleSchedule.ScheduleId = me.Id;
            me.VehicleSchedules.Add(vehicleSchedule);
        }
        return vehicleSchedule;
    }

    /// <summary>
    /// Adds a driver duty to the schedule.
    /// </summary>
    /// <param name="schedule">The schedule to add the driver duty to.</param>
    /// <param name="driverDuty">The driver duty to add.</param>
    /// <returns>The added driver duty.</returns>
    public static DriverDuty AddDriverDuty(this Schedule schedule, DriverDuty driverDuty)
    {
        schedule = schedule.ValueOrException(nameof(schedule));
        driverDuty = driverDuty.ValueOrException(nameof(driverDuty));
        if (!schedule.DriverDuties.Contains(driverDuty))
        {
            driverDuty.Schedule = schedule;
            driverDuty.ScheduleId = schedule.Id;
            schedule.DriverDuties.Add(driverDuty);
        }
        return driverDuty;
    }
}
