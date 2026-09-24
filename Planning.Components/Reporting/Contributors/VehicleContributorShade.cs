namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// The background a row of the Vehicle contributors report is printed on, which tells at a glance whether the
/// unit is set up for the whole meeting, joins it later, or is a spare.
/// </summary>
public enum VehicleContributorShade
{
    /// <summary>White: an item in operation on every session, or one given no work at all.</summary>
    None,

    /// <summary>Light grey: a spare unit, brought but not set up.</summary>
    Spare,

    /// <summary>Light blue: first in operation on the first session or day, but not on all of them.</summary>
    FirstSession,

    /// <summary>Light green: first in operation on the second session or day.</summary>
    SecondSession,

    /// <summary>Light red: first in operation on the third session or day.</summary>
    ThirdSession,
}
