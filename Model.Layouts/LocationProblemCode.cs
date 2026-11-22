namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>A rule that an operation location or its tracks break; user interfaces show a text per code.</summary>
public enum LocationProblemCode
{
    /// <summary>The location has no name.</summary>
    NameMissing = 0,

    /// <summary>The location has no signature.</summary>
    SignatureMissing = 1,

    /// <summary>The country code isn't two letters.</summary>
    CountryCodeInvalid = 2,

    /// <summary>The centre is negative.</summary>
    CentreNegative = 3,

    /// <summary>Only a station can be a shadow station.</summary>
    ShadowNotAllowed = 4,

    /// <summary>Only a station can have a turntable.</summary>
    TurntableNotAllowed = 5,

    /// <summary>Passengers can't be exchanged at this kind of location.</summary>
    PassengerExchangeNotAllowed = 6,

    /// <summary>Cargo can't be exchanged at this kind of location.</summary>
    CargoExchangeNotAllowed = 7,

    /// <summary>Cargo is always exchanged at an industrial area and at a shadow station.</summary>
    CargoExchangeRequired = 8,

    /// <summary>Passengers are always exchanged at a shadow station.</summary>
    PassengerExchangeRequired = 9,

    /// <summary>Trains can't change direction at this kind of location.</summary>
    DirectionChangeNotAllowed = 10,

    /// <summary>Trains can't cross at this kind of location.</summary>
    TrainsCrossingNotAllowed = 11,

    /// <summary>A track has no designation.</summary>
    TrackDesignationMissing = 12,

    /// <summary>Two tracks have the same designation.</summary>
    TrackDesignationNotUnique = 13,

    /// <summary>A track has a negative length or platform length.</summary>
    TrackLengthNegative = 14,

    /// <summary>Only a station can be manned.</summary>
    MannedNotAllowed = 15,

    /// <summary>A manned station and a halt can't be controlled from another location.</summary>
    ControlledByNotAllowed = 16,

    /// <summary>The location that controls this one is not a manned station of the layout.</summary>
    ControlledByNotManned = 17,

    /// <summary>This kind of location has no instructions.</summary>
    InstructionsNotAllowed = 18,

    /// <summary>Something is decided for an operation location in a layout, but the module is not an operation location.</summary>
    OperationLocationMissing = 19,

    /// <summary>Something is decided in a layout for a track that the module doesn't have.</summary>
    TrackUnknown = 20,

    /// <summary>A track has the name of a key but can't be locked.</summary>
    KeyNameWithoutLock = 21,
}
