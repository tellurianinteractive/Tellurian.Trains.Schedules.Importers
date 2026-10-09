namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// The background a row of the Vehicle contributors report is printed on. What a colour means is up to the
/// arrangement the row is printed in (see <see cref="VehicleContributorShading"/>), apart from grey, which is
/// a spare unit everywhere.
/// </summary>
public enum VehicleContributorShade
{
    /// <summary>White: nothing to point out.</summary>
    None,

    /// <summary>Light grey: a spare unit, brought but not set up.</summary>
    Grey,

    /// <summary>Light yellow.</summary>
    Yellow,

    /// <summary>Light blue.</summary>
    Blue,

    /// <summary>Light green.</summary>
    Green,

    /// <summary>Light red.</summary>
    Red,
}
