using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Extensions;

/// <summary>
/// Extension methods for creating vehicles with the simple case of a single all-sessions schedule assignment.
/// </summary>
internal static class VehicleExtensions
{
    /// <summary>
    /// Creates a Vehicle with a single VehicleScheduleAssignment for all sessions,
    /// and adds it to the schedule. This is the common case for XPLN imports.
    /// </summary>
    /// <param name="schedule">The schedule to add the vehicle to.</param>
    /// <param name="id">The unique identifier for the vehicle.</param>
    /// <param name="vehicleType">The type of vehicle (Locomotive, Railcar, etc.).</param>
    /// <param name="number">The vehicle number.</param>
    /// <param name="vehicleClass">The vehicle class (e.g., "BR 218").</param>
    /// <param name="company">The operating company (optional).</param>
    /// <param name="externalId">External identifier (optional).</param>
    /// <param name="remark">Remark or description (optional).</param>
    /// <returns>The VehicleSchedule that TrainParts should be added to.</returns>
    public static VehicleSchedule CreateVehicleWithAllSessionsSchedule(
        this Schedule schedule,
        int id,
        VehicleType vehicleType,
        int number,
        string? vehicleClass = null,
        Company? company = null,
        string? externalId = null,
        string? remark = null)
    {
        // Create the vehicle
        var vehicle = new Vehicle(id, vehicleType, number)
        {
            Class = vehicleClass ?? string.Empty,
            Company = company,
            CompanyId = company?.Id,
            ExternalId = externalId,
            Remark = remark
        };

        // Create the VehicleSchedule
        var vehicleSchedule = new VehicleSchedule(id);

        // Create the assignment linking vehicle to schedule for all sessions
        var assignment = new VehicleScheduleAssignment(id, vehicle, vehicleSchedule);

        // Wire up to vehicle
        vehicle.ScheduleAssignments.Add(assignment);

        // Add to schedule using extension methods
        schedule.AddVehicle(vehicle);
        schedule.AddVehicleSchedule(vehicleSchedule);

        return vehicleSchedule;
    }
}
