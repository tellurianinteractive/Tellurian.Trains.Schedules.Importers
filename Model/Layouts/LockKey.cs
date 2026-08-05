namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The key that unlocks the switches at an unmanned operation location, and the manned station where it
/// is kept between uses.
/// </summary>
/// <remarks>
/// Nobody is on duty at an unmanned location to work its switches, so they are padlocked and the key is
/// held at a manned station along the way. A train that has work there must collect the key before it
/// leaves that station and hand it back the next time it calls there — which is what the two lock key
/// notes tell the loco driver (see <c>StationCall.LockKeyNotes</c>).
/// </remarks>
public sealed class LockKey
{
    /// <summary>
    /// Gets or sets what the key is called, so the loco driver can ask for the right one where a station
    /// holds several. Optional; the notes read without it when it is not given.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the manned station where the key is kept.
    /// </summary>
    public Station HeldAt { get; set; } = default!;
}

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
