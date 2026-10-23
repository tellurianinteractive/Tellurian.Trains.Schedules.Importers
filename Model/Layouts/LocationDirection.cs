namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// One way out of a location: the locations directly beside it on that side, and everything that can be
/// reached through them without passing the location again.
/// </summary>
/// <param name="Adjacent">The locations directly connected to the location on this side, in name order.</param>
/// <param name="Reach">Every location on this side, the adjacent ones included.</param>
public sealed record LocationDirection(IReadOnlyList<OperationLocation> Adjacent, IReadOnlySet<OperationLocation> Reach)
{
    /// <summary>
    /// The locations of spurs folded into this direction (see <c>DispatchDirectionsAt</c>): dead ends with
    /// nobody else to ring, which belong here because their track runs the same way.
    /// </summary>
    public IReadOnlyList<OperationLocation> Spurs { get; init; } = [];
}
