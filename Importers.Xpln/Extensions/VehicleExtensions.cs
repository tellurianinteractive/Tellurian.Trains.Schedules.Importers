using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

/// <summary>
/// Extension methods for creating vehicles with the simple case of a single all-sessions schedule assignment.
/// </summary>
internal static class VehicleExtensions
{
    /// <summary>
    /// Creates a Vehicle with a single ScheduleAssignment for all sessions,
    /// and adds it to the schedule. This is the common case for XPLN imports.
    /// </summary>
    /// <param name="schedule">The schedule to add the vehicle to.</param>
    /// <param name="id">The unique identifier for the vehicle.</param>
    /// <param name="vehicleType">The type of vehicle (Locomotive, Trainset, etc.).</param>
    /// <param name="number">The vehicle number.</param>
    /// <param name="vehicleClass">The vehicle class (e.g., "BR 218").</param>
    /// <param name="company">The operating company (optional).</param>
    /// <param name="externalId">External identifier (optional).</param>
    /// <param name="remark">Remark or description (optional).</param>
    /// <returns>The VehicleSchedule that TrainParts should be added to.</returns>
    public static Schedule CreateVehicleWithAllSessionsSchedule(
        this Plan schedule,
        int id,
        ScheduledObjectType vehicleType,
        int number,
        string? vehicleClass = null,
        Company? company = null,
        string? externalId = null,
        string? remark = null)
    {
        // Create the vehicle. When the identifier carries no parseable number, fall back to the unique
        // id (the source row number) so every vehicle has a distinct, non-zero number for display.
        var vehicle = new ScheduledObject(id, vehicleType, number == 0 ? id : number)
        {
            Class = vehicleClass ?? string.Empty,
            Company = company,
            CompanyId = company?.Id,
            ExternalId = externalId,
            Remark = remark
        };

        // Create the VehicleSchedule
        var vehicleSchedule = new Schedule(id);

        // Create the assignment linking vehicle to schedule for all sessions
        var assignment = new ScheduleAssignment(id, vehicle, vehicleSchedule);

        // Wire up to vehicle
        vehicle.ScheduleAssignments.Add(assignment);

        // Add to schedule using extension methods
        schedule.AddVehicle(vehicle);
        schedule.AddVehicleSchedule(vehicleSchedule);

        return vehicleSchedule;
    }
}
