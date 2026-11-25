namespace Tellurian.Trains.Schedules.Model.Dispatching;

/// <summary>Who has to agree before a train may run a <see cref="DispatchLeg"/>.</summary>
public enum DispatchLegKind
{
    /// <summary>
    /// The dispatch stretch has different dispatchers at its ends, who agree between them: one asks, the other
    /// accepts. This holds also when the train starts, ends, joins or leaves the stretch at a place along it
    /// without a dispatcher, such as an unmanned station: the place is part of the line between them.
    /// </summary>
    BetweenDispatchers,

    /// <summary>
    /// The same dispatcher works both ends, such as a station and the junction it controls, and clears the train
    /// alone.
    /// </summary>
    OneDispatcher,

    /// <summary>
    /// Only one end has a dispatcher; at the other the driver works the points and signals, such as a branch to an
    /// industrial area nobody controls. The driver calls the dispatcher, who gives the go to a train coming from the
    /// driver's end, and clears a train going there.
    /// </summary>
    DriverCalls,
}

/// <summary>
/// A part of a train's run that is cleared as one: between two consecutive places where the train is cleared.
/// </summary>
/// <remarks>
/// The places a train is cleared at are its first and last call, its calls at dispatch endpoints (see
/// <c>IsDispatchEndpoint</c>), and where it joins or leaves a dispatch stretch at a place along it. See
/// <see cref="DispatchLegExtensions"/>.
/// </remarks>
/// <param name="From">The call the leg starts at.</param>
/// <param name="To">The call the leg ends at.</param>
/// <param name="Kind">Who has to agree.</param>
public sealed record DispatchLeg(StationCall From, StationCall To, DispatchLegKind Kind)
{
    /// <summary>The dispatch stretch the leg runs on, or <c>null</c> where it runs on none, such as a branch.</summary>
    public DispatchStretch? Stretch { get; init; }

    /// <summary>
    /// The dispatcher at the end the train comes from: of the dispatch stretch in the direction of travel, also when
    /// the train starts at a place along it; where the leg runs on no stretch, of the place the leg starts at.
    /// <c>null</c> where that end has none.
    /// </summary>
    public Station? FromDispatcher { get; init; }

    /// <summary>
    /// The dispatcher at the end the train goes to: of the dispatch stretch in the direction of travel, also when
    /// the train ends at a place along it; where the leg runs on no stretch, of the place the leg ends at.
    /// <c>null</c> where that end has none.
    /// </summary>
    public Station? ToDispatcher { get; init; }
}
