using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Represents a complete railway schedule containing timetables, vehicles, vehicle schedules, and driver duties.
/// </summary>
/// <remarks>
/// A schedule is the top-level container for all railway operation planning data.
/// It associates a timetable with the vehicles and personnel needed to operate the trains.
/// </remarks>
public class Plan : IEquatable<Plan>
{
    // Private parameterless constructor for EF Core and JSON deserialization
    [JsonConstructor]
    private Plan()
    {
        Name = string.Empty;
        Timetable = default!;
        ScheduledObjects = [];
        Schedules = [];
        DriverDuties = [];
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Plan"/> with the specified name and timetable.
    /// </summary>
    /// <param name="name">The name of the schedule.</param>
    /// <param name="timetable">The timetable associated with this schedule.</param>
    public Plan(string name, Timetable timetable)
    {
        Name = name;
        Timetable = timetable;
        TimetableId = timetable.Id;
        ScheduledObjects = [];
        Schedules = [];
        DriverDuties = [];
    }

    /// <summary>
    /// Creates a new schedule with the specified name and timetable.
    /// </summary>
    /// <param name="name">The name of the schedule.</param>
    /// <param name="timetable">The timetable associated with this schedule.</param>
    /// <returns>A new <see cref="Plan"/> instance.</returns>
    public static Plan Create(string name, Timetable timetable) =>
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
    public ICollection<ScheduledObject> ScheduledObjects { get; set; }

    /// <summary>
    /// Gets or sets the collection of vehicle schedules defining how vehicles are assigned to trains.
    /// </summary>
    public ICollection<Schedule> Schedules { get; set; }

    /// <summary>
    /// Gets or sets the collection of driver duties for this schedule.
    /// </summary>
    public ICollection<DriverDuty> DriverDuties { get; set; }

    /// <inheritdoc/>
    public bool Equals(Plan? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Plan other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>
/// Provides extension methods for <see cref="Plan"/>.
/// </summary>
public static class PlanExtensions
{
    extension(Plan plan)
    {
        /// <summary>
        /// Adds a vehicle to the schedule.
        /// </summary>
        /// <param name="vehicle">The vehicle to add.</param>
        /// <returns>The added vehicle.</returns>
        public ScheduledObject AddVehicle(ScheduledObject vehicle)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            if (!plan.ScheduledObjects.Contains(vehicle))
            {
                vehicle.Plan = plan;
                vehicle.PlanId = plan.Id;
                plan.ScheduledObjects.Add(vehicle);
            }
            return vehicle;
        }

        /// <summary>
        /// Adds a vehicle schedule to the schedule.
        /// </summary>
        /// <param name="vehicleSchedule">The vehicle schedule to add.</param>
        /// <returns>The added vehicle schedule.</returns>
        public Schedule AddVehicleSchedule(Schedule vehicleSchedule)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicleSchedule = vehicleSchedule.ValueOrException(nameof(vehicleSchedule));
            if (!plan.Schedules.Contains(vehicleSchedule))
            {
                vehicleSchedule.Plan = plan;
                vehicleSchedule.PlanId = plan.Id;
                plan.Schedules.Add(vehicleSchedule);
            }
            return vehicleSchedule;
        }

        /// <summary>
        /// Adds a driver duty to the schedule.
        /// </summary>
        /// <param name="driverDuty">The driver duty to add.</param>
        /// <returns>The added driver duty.</returns>
        public DriverDuty AddDriverDuty(DriverDuty driverDuty)
        {
            plan = plan.ValueOrException(nameof(plan));
            driverDuty = driverDuty.ValueOrException(nameof(driverDuty));
            if (!plan.DriverDuties.Contains(driverDuty))
            {
                driverDuty.Plan = plan;
                driverDuty.PlanId = plan.Id;
                plan.DriverDuties.Add(driverDuty);
            }
            return driverDuty;
        }

        /// <summary>
        /// Gets or creates call notes for train arriving at an operation location.
        /// </summary>
        /// <param name="call"></param>
        /// <returns></returns>
        public IEnumerable<CallNote> CallNotesForArrivalAt(StationCall call)
        {
            return call.Notes;
            // TODO: Add LINQ for creating the rest of the notes.
        }

        /// <summary>
        /// Gets all <see cref="ScheduledObject">schedule objects</see> associated with a <see cref="TrainPart"/>.
        /// </summary>
        /// <param name="trainPart"></param>
        /// <returns></returns>
        public IEnumerable<ScheduledObject> ScheduledObjectsFor(TrainPart trainPart)
        {
            return plan.ScheduledObjects
                .Where(so => so.ScheduleAssignments
                .Any(sa => sa.Schedule.Parts.Contains(trainPart)));
        }
    }





}
