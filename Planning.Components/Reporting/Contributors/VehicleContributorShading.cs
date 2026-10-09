namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// The rules deciding the background of a row of the Vehicle contributors report. Each arrangement has its own,
/// because each reader looks for something different: a spare is grey wherever it is printed, but what the item
/// itself is shaded by depends on the question the page answers.
/// </summary>
public static class VehicleContributorShading
{
    /// <summary>The rule for an arrangement.</summary>
    public static Func<VehicleContributorLine, VehicleContributorShade> For(VehicleContributorsGrouping grouping) => grouping switch
    {
        VehicleContributorsGrouping.OperationLocation => ByStartSessionParity,
        VehicleContributorsGrouping.Contributor => ByLateStart,
        _ => ByFirstSession,
    };

    /// <summary>
    /// For a station owner: an item not in operation on every session or day is shaded by the one it starts on,
    /// light yellow for an odd and light blue for an even one, so that what is to be set up before each session
    /// can be told from what is set up before the one after. An item in operation throughout, or not at all, is
    /// not shaded.
    /// </summary>
    public static VehicleContributorShade ByStartSessionParity(VehicleContributorLine line) => line switch
    {
        { IsSpare: true } => VehicleContributorShade.Grey,
        { Start: null or { IsEverySession: true } } => VehicleContributorShade.None,
        { Start.FirstPosition: var position } when position % 2 == 1 => VehicleContributorShade.Yellow,
        _ => VehicleContributorShade.Blue,
    };

    /// <summary>
    /// For an owner: the item shaded light yellow only when it is not needed on the first session or day, so
    /// what an owner does not have to bring for the start of the meeting stands out. An item not in operation at
    /// all is not shaded.
    /// </summary>
    public static VehicleContributorShade ByLateStart(VehicleContributorLine line) => line switch
    {
        { IsSpare: true } => VehicleContributorShade.Grey,
        { Start.FirstPosition: > 1 } => VehicleContributorShade.Yellow,
        _ => VehicleContributorShade.None,
    };

    /// <summary>
    /// The item shaded light blue, green or red by the first, second or third session or day it is first in
    /// operation, unless it is in operation on all of them. An item first in operation from the fourth session or
    /// day on, or not at all, is not shaded.
    /// </summary>
    public static VehicleContributorShade ByFirstSession(VehicleContributorLine line) => line switch
    {
        { IsSpare: true } => VehicleContributorShade.Grey,
        { Start: null or { IsEverySession: true } } => VehicleContributorShade.None,
        { Start.FirstPosition: 1 } => VehicleContributorShade.Blue,
        { Start.FirstPosition: 2 } => VehicleContributorShade.Green,
        { Start.FirstPosition: 3 } => VehicleContributorShade.Red,
        _ => VehicleContributorShade.None,
    };
}
