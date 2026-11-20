namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A rule that an operation location or its tracks break; user interfaces show a text per code.</summary>
public enum LocationProblemCode
{
    /// <summary>The location has no name.</summary>
    NameMissing,

    /// <summary>The location has no signature.</summary>
    SignatureMissing,

    /// <summary>The country code isn't two letters.</summary>
    CountryCodeInvalid,

    /// <summary>The centre is negative.</summary>
    CentreNegative,

    /// <summary>Only a station can be a shadow station.</summary>
    ShadowNotAllowed,

    /// <summary>Only a station can have a turntable.</summary>
    TurntableNotAllowed,

    /// <summary>Passengers can't be exchanged at this kind of location.</summary>
    PassengerExchangeNotAllowed,

    /// <summary>Cargo can't be exchanged at this kind of location.</summary>
    CargoExchangeNotAllowed,

    /// <summary>Cargo is always exchanged at an industrial area and at a shadow station.</summary>
    CargoExchangeRequired,

    /// <summary>Passengers are always exchanged at a shadow station.</summary>
    PassengerExchangeRequired,

    /// <summary>Trains can't change direction at this kind of location.</summary>
    DirectionChangeNotAllowed,

    /// <summary>Trains can't cross at this kind of location.</summary>
    TrainsCrossingNotAllowed,

    /// <summary>A track has no designation.</summary>
    TrackDesignationMissing,

    /// <summary>Two tracks have the same designation.</summary>
    TrackDesignationNotUnique,

    /// <summary>A track has a negative length or platform length.</summary>
    TrackLengthNegative,

    /// <summary>Only a station can be manned.</summary>
    MannedNotAllowed,

    /// <summary>A manned station and a halt can't be controlled from another location.</summary>
    ControlledByNotAllowed,

    /// <summary>The location that controls this one is not a manned station of the layout.</summary>
    ControlledByNotManned,

    /// <summary>This kind of location has no instructions.</summary>
    InstructionsNotAllowed,

    /// <summary>Something is decided for an operation location in a layout, but the module is not an operation location.</summary>
    OperationLocationMissing,

    /// <summary>Something is decided in a layout for a track that the module doesn't have.</summary>
    TrackUnknown,

    /// <summary>A track has the name of a key but can't be locked.</summary>
    KeyNameWithoutLock,
}
