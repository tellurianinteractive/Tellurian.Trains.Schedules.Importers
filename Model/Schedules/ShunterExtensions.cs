namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// The shunters of a <see cref="Plan"/>: locomotives stationed at one operation location for any shunting
/// there (see <see cref="ScheduledObjectType.Shunter"/>). A shunter works no schedule, so it is never assigned
/// to a train; it is rolling stock all the same, brought to the meeting by a participant like any other
/// locomotive. Deleting one lives in <c>DeletionRules</c>, as for any vehicle.
/// </summary>
/// <remarks>
/// None of these persist: a caller that changes the plan through them saves it afterwards (in the app,
/// <c>ScheduleState.SaveAndNotify()</c>).
/// </remarks>
public static class ShunterExtensions
{
    extension(OperationLocation location)
    {
        /// <summary>
        /// Whether a shunter may be stationed here: anywhere but a signal controlled location, which trains
        /// only ever run past and which has no tracks to shunt.
        /// </summary>
        public bool CanHaveShunters => location is not SignalControlledLocation;
    }

    extension(Plan plan)
    {
        /// <summary>
        /// The shunters stationed at <paramref name="location"/>, in designation order.
        /// </summary>
        /// <param name="location">The operation location.</param>
        public IReadOnlyList<ScheduledObject> ShuntersAt(OperationLocation location)
        {
            plan = plan.ValueOrException(nameof(plan));
            location = location.ValueOrException(nameof(location));
            return [.. plan.ScheduledObjects
                .Where(vehicle => vehicle.IsShunter && vehicle.StationedAtId == location.Id)
                .OrderBy(vehicle => vehicle.Designation, StringComparer.CurrentCulture)
                .ThenBy(vehicle => vehicle.Id)];
        }

        /// <summary>
        /// The operation location <paramref name="vehicle"/> is stationed at, or <c>null</c> for a vehicle that
        /// is not a shunter, and for a shunter whose location is no longer in the layout.
        /// </summary>
        /// <param name="vehicle">The vehicle.</param>
        public OperationLocation? StationOf(ScheduledObject vehicle)
        {
            plan = plan.ValueOrException(nameof(plan));
            vehicle = vehicle.ValueOrException(nameof(vehicle));
            return vehicle.IsShunter && vehicle.StationedAtId is { } id
                ? plan.Layout.OperationLocations.FirstOrDefault(location => location.Id == id)
                : null;
        }

        /// <summary>
        /// Stations a new shunter at <paramref name="location"/>. Refused, returning <c>null</c>, where no
        /// shunter may be stationed (see <c>CanHaveShunters</c>).
        /// </summary>
        /// <remarks>
        /// As with <c>CreateVehicle</c>, the shunter is given no external id, and it is its operator and number
        /// that identify it. A shunter works no schedule, so it claims its identity on every session; use
        /// <c>VehicleClaiming</c> with <c>Sessions.All</c> first where the caller must not create one whose
        /// identity is already taken.
        /// </remarks>
        /// <param name="location">The location the shunter is stationed at.</param>
        /// <param name="class">The locomotive class; may be empty.</param>
        /// <param name="number">The number, or 0 to fall back to the vehicle's id.</param>
        /// <param name="company">The operating company, or <c>null</c>.</param>
        /// <param name="tractionType">How the shunter is powered.</param>
        public ScheduledObject? AddShunter(OperationLocation location, string? @class, int number, Company? company, TractionType tractionType = TractionType.Undefined)
        {
            plan = plan.ValueOrException(nameof(plan));
            location = location.ValueOrException(nameof(location));
            if (!location.CanHaveShunters) return null;
            var shunter = plan.CreateVehicle(ScheduledObjectType.Shunter, @class, number, company);
            shunter.TractionType = tractionType == TractionType.None ? TractionType.Undefined : tractionType;
            shunter.StationedAtId = location.Id;
            return shunter;
        }
    }
}
