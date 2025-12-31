using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

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

    public Schedule(string name, Timetable timetable)
    {
        Name = name;
        Timetable = timetable;
        TimetableId = timetable.Id;
        Vehicles = [];
        VehicleSchedules = [];
        DriverDuties = [];
    }

    public static Schedule Create(string name, Timetable timetable) =>
        new(name, timetable);

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // FK property for EF Core
    public int TimetableId { get; set; }
    public Timetable Timetable { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; }
    public ICollection<VehicleSchedule> VehicleSchedules { get; set; }
    public ICollection<DriverDuty> DriverDuties { get; set; }

    public bool Equals(Schedule? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => obj is Schedule other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public override string ToString() => Name;
}

public static class ScheduleExtensions
{
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
