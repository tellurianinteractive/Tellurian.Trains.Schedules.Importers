namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Why the <see cref="LockKey"/> a location carries is not in force.
/// </summary>
/// <remarks>
/// Manning is edited on both sides of a key long after it is set, and either change can leave the key
/// meaningless. The key is kept and ignored rather than deleted — the manning change may well be undone,
/// and throwing the key away would make that a retyping job — so the fault says which change did it,
/// and the plan's validation reports it (rule L4).
/// </remarks>
public enum LockKeyFault
{
    /// <summary>The key is in force, or there is no key.</summary>
    None,

    /// <summary>The location is a manned station: somebody is on duty to work its switches.</summary>
    LocationIsManned,

    /// <summary>The location exchanges no cargo, so nothing is worked there that needs unlocking.</summary>
    LocationExchangesNoCargo,

    /// <summary>The station holding the key is not manned, so nobody is there to hand it over.</summary>
    HolderIsNotManned,
}

/// <summary>
/// Which locations may require a lock key, which stations may hold one, and when a key that is set is
/// actually in force.
/// </summary>
public static class LockKeyExtensions
{
    extension(Layout layout)
    {
        /// <summary>
        /// The stations that may be offered as the holder of a lock key: every manned station. An
        /// unmanned one has nobody to hand the key over, which is the whole reason the key exists.
        /// </summary>
        public IEnumerable<Station> LockKeyHoldingStations =>
            layout.OperationLocations.OfType<Station>().Where(station => station.IsManned);
    }

    extension(StationCall call)
    {
        /// <summary>
        /// Whether the train is locked in at this call: it stands on a lockable track
        /// (<see cref="StationTrack.IsLockable"/>) and either the location is controlled from a manned
        /// station — locked in from arrival — or the location has a lock key in force and the train has
        /// stopped at the station holding it earlier in its run. Either is enough. A locked-in train
        /// leaves the line free, so other trains may pass or meet it.
        /// </summary>
        /// <remarks>
        /// The train must <em>stop</em> at the key-holding station, since a key cannot be collected in
        /// passing — the same rule the lock key notes follow (see <c>StationCall.LockKeyNotes</c>).
        /// </remarks>
        public bool IsLockedIn
        {
            get
            {
                if (!call.Track.IsLockable || !call.IsStop) return false;
                var location = call.OperationLocation;
                if (location.ControlledBy is { IsManned: true }) return true;
                if (location.EffectiveLockKey is not { HeldAt: { } holder }) return false;

                // By reference: calls that compare equal are still different calls.
                var calls = call.Train.CallsInRunOrder;
                for (var i = 0; i < calls.Count && !ReferenceEquals(calls[i], call); i++)
                    if (calls[i].IsStop && calls[i].OperationLocation.Equals(holder)) return true;
                return false;
            }
        }
    }

    extension(OperationLocation location)
    {
        /// <summary>
        /// Whether this location may require a lock key: it must exchange cargo — there is otherwise no
        /// reason for a train to work its switches — and it must not be a manned station, which has
        /// somebody on duty to work them.
        /// </summary>
        public bool CanRequireLockKey =>
            location.HasCargoExchange && location is not Station { IsManned: true };

        /// <summary>
        /// The key that is actually in force here: the one this location carries, while it is still
        /// needed and can still be fetched. <c>null</c> when there is no key, or when the manning on
        /// either side has made it meaningless — see <see cref="LockKeyFault"/>. Everything derived from
        /// a key reads it through here, so an ignored key produces no notes.
        /// </summary>
        public LockKey? EffectiveLockKey =>
            location.LockKeyFault is LockKeyFault.None ? location.LockKey : null;

        /// <summary>
        /// Why the key this location carries is not in force, or <see cref="LockKeyFault.None"/> when it
        /// is — or when there is no key at all.
        /// </summary>
        /// <remarks>
        /// A location that needs no key is the first answer given: where nobody has to unlock anything,
        /// who holds the key is beside the point.
        /// </remarks>
        public LockKeyFault LockKeyFault =>
            location.LockKey is not { } key ? LockKeyFault.None :
            location is Station { IsManned: true } ? LockKeyFault.LocationIsManned :
            !location.HasCargoExchange ? LockKeyFault.LocationExchangesNoCargo :
            key.HeldAt is not { IsManned: true } ? LockKeyFault.HolderIsNotManned :
            LockKeyFault.None;
    }
}
