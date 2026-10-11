namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// What a <see cref="ScheduledObjectType.Wagonset"/> is used for. Stated on the wagonset itself, as the
/// individual wagons of a set that lists them may be of both kinds and a set need not list any.
/// </summary>
/// <remarks>Stored by number: new kinds are added last.</remarks>
public enum WagonSetKind
{
    /// <summary>Not stated, as for every wagonset made before the kind existed.</summary>
    Unspecified,

    /// <summary>Carries passengers.</summary>
    Passenger,

    /// <summary>Carries freight, which can be loaded and unloaded on the way.</summary>
    Cargo,
}
