namespace Tellurian.Trains.Schedules.Model.Notes;

/// <summary>
/// The key for a location further along the run must be collected here before the train leaves.
/// </summary>
/// <param name="Unlocks">The location the key unlocks.</param>
/// <param name="KeyName">What the key is called, or empty when it is not named.</param>
public sealed record PickUpLockKeyNote(OperationLocation Unlocks, string KeyName) : GeneratedNote;

/// <summary>
/// The key for a location the train has now finished with is handed back here on arrival.
/// </summary>
/// <param name="Unlocks">The location the key unlocks.</param>
/// <param name="KeyName">What the key is called, or empty when it is not named.</param>
public sealed record LeaveLockKeyNote(OperationLocation Unlocks, string KeyName) : GeneratedNote;

/// <summary>
/// Derives the lock key notes for a train's calls at the stations that hold the keys.
/// </summary>
/// <remarks>
/// <para>
/// A key is a round trip, and both halves of it are the key-holding station's business: the driver
/// collects it there on the way out and hands it back there on the way home. Both notes are therefore
/// written at that station — a departure note going out and an arrival note coming back — and none at
/// the location the key unlocks, where the driver is already holding it.
/// </para>
/// <para>
/// Which visit to the holding station a key belongs to is decided by the stops in between: it is
/// collected at the <em>last</em> stop there before the work and handed back at the <em>first</em> stop
/// there after it. A train calling at the holding station twice on its way out is not told to collect
/// the same key twice.
/// </para>
/// </remarks>
public static class LockKeyNoteExtensions
{
    /// <summary>
    /// Where the notes sit among a call's other notes: after the ones saying whether there is work here
    /// at all, and before the crossings and cargo destinations. Collecting the key is what has to happen
    /// before the train can leave, so it is what the driver reads first.
    /// </summary>
    private const int LockKeyDisplayOrder = 200;

    extension(StationCall call)
    {
        /// <summary>
        /// The lock key notes for this call: one for each keyed location this train works either side of
        /// it whose key is held here.
        /// </summary>
        /// <remarks>
        /// Only a cargo train gets them. A key unlocks the switches to sidings that exist to be worked
        /// with wagons, so a train carrying no cargo has no reason to fetch one. The train must also
        /// <em>stop</em> at both ends — at the holding station, because a key cannot be collected in
        /// passing, and at the location it unlocks, because a train running through unlocks nothing.
        /// </remarks>
        public IEnumerable<ICallNote> LockKeyNotes
        {
            get
            {
                call = call.ValueOrException(nameof(call));
                if (call.Train is not { } train) return [];
                if (!train.IsCargo || !call.IsStop) return [];

                // By reference: a train can hold two calls that compare equal, and only identity says
                // which of them this one is.
                var calls = train.CallsInRunOrder.ToList();
                var index = calls.FindIndex(c => ReferenceEquals(c, call));
                if (index < 0) return [];

                var holder = call.OperationLocation;
                List<ICallNote> notes =
                [
                    .. Unlocked(calls, index + 1, 1, holder)
                        .Select(keyed => new PickUpLockKeyNote(keyed.Location, keyed.Name) { IsForDeparture = true, DisplayOrder = LockKeyDisplayOrder }),
                    .. Unlocked(calls, index - 1, -1, holder)
                        .Select(keyed => new LeaveLockKeyNote(keyed.Location, keyed.Name) { IsForArrival = true, DisplayOrder = LockKeyDisplayOrder }),
                ];
                // A train may work the same keyed location twice without calling here in between; that is
                // still one key, fetched once and handed back once.
                return [.. notes.Distinct()];
            }
        }
    }

    // The keyed locations the train stops at, walking away from the holding station in one direction and
    // giving up at the next stop back at that station: beyond it, the key is another visit's business.
    private static IEnumerable<(OperationLocation Location, string Name)> Unlocked(
        IReadOnlyList<StationCall> calls, int from, int step, OperationLocation holder)
    {
        for (var i = from; i >= 0 && i < calls.Count; i += step)
        {
            var other = calls[i];
            // A train that only passes neither collects a key nor works anything with one.
            if (!other.IsStop) continue;
            if (other.OperationLocation.Equals(holder)) yield break;
            // The key in force, not merely the one that is set: one left meaningless by a manning change
            // on either side is ignored until that is put right (see LockKeyFault).
            if (other.OperationLocation.EffectiveLockKey is { HeldAt: { } heldAt } key && heldAt.Equals(holder))
                yield return (other.OperationLocation, key.Name.OrElse(string.Empty));
        }
    }
}
