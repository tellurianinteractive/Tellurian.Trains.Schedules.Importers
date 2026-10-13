namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// How the wagons uncoupled from an arriving cargo flow are dealt with, which depends on where they arrive.
/// </summary>
public enum ArrivedWagonHandling
{
    /// <summary>Nothing to say: the wagons are for the station itself and no flow asks for them to be shunted.</summary>
    None,

    /// <summary>At a shadow station: sorted as the station's own instructions say.</summary>
    SortAccordingToStationInstructions,

    /// <summary>Some wagons are bound for other destinations than the station: shunted on to them.</summary>
    ShuntToDestinations,

    /// <summary>All wagons are for the station itself and a flow asks for them to be shunted: shunted to the customers.</summary>
    ShuntToCustomers,
}

/// <summary>
/// Note telling the station's dispatcher what to do with the cargo flow's freight wagons that arrive here.
/// </summary>
/// <remarks>
/// <para>
/// The arrival counterpart of <see cref="CargoFlowDestinationNote"/>, and derived the same way: from
/// <see cref="CargoFlowTrainPart.HasUncoupleNote"/> at the call the flow's wagons are disconnected at.
/// What is said depends on the kind of location and on where the wagons are bound
/// (<see cref="ArrivedWagonHandling"/>); where there is nothing to say, no note is made.
/// </para>
/// <para>
/// For the dispatcher only. The loco driver's booklet states the flow in its cargo block, with the load
/// limits and wagon classes that belong to planning it; repeating it as a note would say the same thing
/// twice on the page it is already on.
/// </para>
/// <para>
/// One note for all the cargo flows that end at the call, like its counterpart.
/// </para>
/// </remarks>
/// <param name="Parts">The cargo flows whose wagons come off here.</param>
public sealed record CargoFlowUncoupleNote(IReadOnlyList<CargoFlowTrainPart> Parts) : GeneratedNote
{
    /// <summary>A note for a single cargo flow.</summary>
    public CargoFlowUncoupleNote(CargoFlowTrainPart part) : this([part]) { }

    /// <summary>What is done with the wagons after they are uncoupled.</summary>
    public ArrivedWagonHandling Handling { get; init; }

    /// <inheritdoc/>
    public bool Equals(CargoFlowUncoupleNote? other) =>
        other is not null && base.Equals(other) && Handling == other.Handling && Parts.SequenceEqual(other.Parts);

    /// <inheritdoc/>
    public override int GetHashCode() => Parts.Aggregate(HashCode.Combine(base.GetHashCode(), Handling), HashCode.Combine);
}
