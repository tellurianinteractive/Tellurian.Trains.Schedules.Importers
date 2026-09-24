namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// The rows of the Vehicle contributors report printed under one heading, which always start on a page of
/// their own.
/// </summary>
/// <param name="Kind">What the group is made up of, which decides its heading and its columns.</param>
/// <param name="Name">The name of the operation location or participant the group is for; <c>null</c> for a
/// kind of group that is headed by what it is rather than by whom.</param>
/// <param name="Lines">The rows, in the order they are printed.</param>
public sealed record VehicleContributorsGroup(
    VehicleContributorsGroupKind Kind,
    string? Name,
    IReadOnlyList<VehicleContributorLine> Lines);
