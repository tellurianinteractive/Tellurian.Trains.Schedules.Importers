namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>Note listing the destinations to which the train brings wagons from here.</summary>
/// <remarks>
/// One note for all the cargo flows that begin at the call, not one per flow: the reader wants the list of
/// places, and a sentence repeated per flow only buries it.
/// </remarks>
/// <param name="Parts">The cargo flows whose wagons are coupled on here.</param>
public sealed record CargoFlowDestinationNote(IReadOnlyList<CargoFlowTrainPart> Parts) : GeneratedNote
{
    /// <summary>A note for a single cargo flow.</summary>
    public CargoFlowDestinationNote(CargoFlowTrainPart part) : this([part]) { }

    /// <inheritdoc/>
    public bool Equals(CargoFlowDestinationNote? other) =>
        other is not null && base.Equals(other) && Parts.SequenceEqual(other.Parts);

    /// <inheritdoc/>
    public override int GetHashCode() => Parts.Aggregate(base.GetHashCode(), HashCode.Combine);
}
