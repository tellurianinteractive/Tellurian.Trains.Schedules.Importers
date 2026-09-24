namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// What a group of the Vehicle contributors report is made up of, which decides its heading and its columns.
/// </summary>
public enum VehicleContributorsGroupKind
{
    /// <summary>The vehicles starting at one operation location.</summary>
    OperationLocation,

    /// <summary>The vehicles given no work yet, which therefore have nowhere to start.</summary>
    NotInOperation,

    /// <summary>The units one participant brings.</summary>
    Contributor,

    /// <summary>The vehicles nobody has booked to bring yet.</summary>
    NotYetBooked,

    /// <summary>Every traction unit, in DCC address order.</summary>
    AllUnits,
}
