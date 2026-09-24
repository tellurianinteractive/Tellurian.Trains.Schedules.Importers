namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// How the Vehicle contributors report arranges the rolling stock of a plan — each arrangement answering the
/// question of one kind of reader.
/// </summary>
public enum VehicleContributorsGrouping
{
    /// <summary>
    /// One group per operation location, for the owner of a station: which vehicles are to be set up there,
    /// and before which session or day.
    /// </summary>
    OperationLocation,

    /// <summary>
    /// One group per participant bringing rolling stock: what they bring to the meeting, and where each item is
    /// to be set up.
    /// </summary>
    Contributor,

    /// <summary>
    /// Every locomotive and trainset unit in one list in DCC address order, so that addresses can be checked and
    /// handed out. Wagonsets, which are not driven, are left out.
    /// </summary>
    DccAddress,
}
