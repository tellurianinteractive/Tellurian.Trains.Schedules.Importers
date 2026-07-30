namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

/// <summary>
/// The rows of a vehicle block on a train part page — traction units or wagonsets.
/// </summary>
/// <remarks>
/// Both blocks have the same six columns and the same shape, differing only in which scheduled objects
/// they list, so one data type and one view serve both.
/// </remarks>
public sealed class TrainPartVehicleData
{
    /// <summary>The vehicles listed, ordered by the sessions they work.</summary>
    public IReadOnlyList<TrainPartVehicle> Vehicles { get; init; } = [];

    /// <summary>How the sessions column is rendered.</summary>
    public required SessionsSettings SessionsSettings { get; init; }

    /// <summary>Whether the block has anything to show. An empty block is omitted entirely.</summary>
    public bool HasData => Vehicles.Count > 0;
}

/// <summary>
/// One vehicle working one movement: which sessions, which vehicle, and from where to where.
/// </summary>
public sealed class TrainPartVehicle
{
    /// <summary>The locomotive, trainset or wagonset.</summary>
    public required ScheduledObject Vehicle { get; init; }

    /// <summary>The movement it works, giving the from/to stations and times.</summary>
    public required ScheduledTrainPart TrainPart { get; init; }

    /// <summary>The sessions it works this movement on.</summary>
    public Sessions Sessions { get; init; }

    /// <summary>
    /// The identity printed for the vehicle.
    /// </summary>
    /// <remarks>
    /// This is the column that gets the driver to the physical locomotive: it has to match the identity
    /// written on the loco card, and through that the turnus card and the throttle. <c>Designation</c> is
    /// the external id when there is one, which is how the vehicle is named everywhere else in the
    /// application — <c>ToString()</c> would return the composed identity and silently bypass it, so a
    /// vehicle carrying an external id would print under a name the physical cards do not use.
    /// Wagonsets follow the same rule, because they carry cards too and the driver has to pick the right
    /// rake out of several standing in the yard.
    /// </remarks>
    public string Designation => Vehicle.Designation;
}
