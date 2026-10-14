namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// Note saying to circulate the loco — run it round to the other end of the train — so the train can
/// leave in the opposite direction to the one it arrived from.
/// </summary>
/// <remarks>
/// <para>
/// Derived from <see cref="TractionOptions.RunaroundLoco"/>, and only where the traction actually needs
/// the move. Both the loco driver and the station's dispatcher get it: the driver does the move, and the
/// dispatcher has to give it the road.
/// </para>
/// <para>
/// It names no vehicle. Circulating is a manoeuvre every loco driver knows, and the loco is the one
/// standing in front of them — so the note says what to do and nothing more. That also makes it one note
/// per train part rather than one per traction unit, which is what double-headed traction wants: the
/// whole consist circulates once.
/// </para>
/// </remarks>
public sealed record CirculateNote : GeneratedNote;

/// <summary>
/// Note saying to turn the loco, usually on a turntable, so it faces the other way. Derived from
/// <see cref="TractionOptions.TurnLoco"/>.
/// </summary>
public sealed record TurnNote : GeneratedNote;

/// <summary>
/// Note saying to both turn the loco and circulate it to the other end of the train.
/// </summary>
/// <remarks>
/// One note rather than a <see cref="TurnNote"/> beside a <see cref="CirculateNote"/>, because it is one
/// errand: the loco leaves the train, goes to the turntable and comes back on the other end. Two notes
/// would read as two independent moves.
/// </remarks>
public sealed record TurnAndCirculateNote : GeneratedNote;

/// <summary>
/// Note saying the wagons are pushed from this call on — the loco is at the other end of the train from
/// the direction of travel. A departure note, derived from <see cref="StationCall.PushWagons"/>, for the
/// loco driver and the dispatcher alike. Like the circulating note it names no vehicle.
/// </summary>
/// <remarks>
/// Where the wagons are bound it says so — the destinations of the cargo flows still ahead on the train,
/// otherwise the places the wagonsets' parts end — and is otherwise a bare instruction.
/// </remarks>
/// <param name="Flows">The cargo flows still ahead of the call, whose destinations the note names.</param>
/// <param name="Places">The places the wagonsets end their part at, named when there are no flows.</param>
public sealed record PushWagonsNote(IReadOnlyList<CargoFlowTrainPart> Flows, IReadOnlyList<string> Places) : GeneratedNote
{
    /// <summary>A note that names no destination.</summary>
    public PushWagonsNote() : this([], []) { }

    /// <inheritdoc/>
    public bool Equals(PushWagonsNote? other) =>
        other is not null && base.Equals(other) && Flows.SequenceEqual(other.Flows) && Places.SequenceEqual(other.Places);

    /// <inheritdoc/>
    public override int GetHashCode() => Places.Aggregate(Flows.Aggregate(base.GetHashCode(), HashCode.Combine), HashCode.Combine);
}
