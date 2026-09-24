namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// One row of the Vehicle contributors report: a rolling stock item, where it is to be set up, and — when
/// anybody brings it — the unit one participant brings.
/// </summary>
/// <param name="Vehicle">The rolling stock item.</param>
/// <param name="Start">Where the item is to be set up, or <c>null</c> when it is not in operation.</param>
/// <param name="Contributor">The unit the row is about: every arrangement prints a row per unit brought, the
/// primary one and each spare. <c>null</c> when nobody brings the item.</param>
/// <param name="OwnerName">The name of the participant bringing <paramref name="Contributor"/>.</param>
/// <param name="IsPrimary">Whether <paramref name="Contributor"/> is the unit set up on the layout rather than a spare.</param>
/// <param name="Notes">The notes worth printing, each on a line of its own: the contributor's own, then the one
/// about the item as a whole.</param>
public sealed record VehicleContributorLine(
    ScheduledObject Vehicle,
    VehicleStart? Start,
    VehicleContributor? Contributor,
    string? OwnerName,
    bool IsPrimary,
    IReadOnlyList<string> Notes)
{
    /// <summary>
    /// Whether the row names an owner of a traction unit without a usable DCC address: none, or 0 for one the
    /// owner is still to provide. A unit nobody brings has no owner to ask, and one that is not driven needs none.
    /// </summary>
    public bool IsDccAddressMissing =>
        Vehicle.NeedsDccAddress && Contributor is { DccAddress: null or DccAddresses.ToBeProvided };

    /// <summary>
    /// The background the row is printed on. A spare is always shaded as a spare; the item itself — the primary
    /// unit, or an item nobody brings yet — by the session or day it is first in operation, unless it is in
    /// operation on all of them. An item first in operation from the fourth session or day on is not shaded.
    /// </summary>
    public VehicleContributorShade Shade => this switch
    {
        { Contributor: not null, IsPrimary: false } => VehicleContributorShade.Spare,
        { Start: null or { IsEverySession: true } } => VehicleContributorShade.None,
        { Start.FirstPosition: 1 } => VehicleContributorShade.FirstSession,
        { Start.FirstPosition: 2 } => VehicleContributorShade.SecondSession,
        { Start.FirstPosition: 3 } => VehicleContributorShade.ThirdSession,
        _ => VehicleContributorShade.None,
    };
}
