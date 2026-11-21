namespace Tellurian.Trains.Schedules.Model.Layouts.Halls;

/// <summary>What a door in a hall is used for.</summary>
public enum DoorKind
{
    /// <summary>An ordinary door.</summary>
    Door,

    /// <summary>An emergency exit, which must be kept free.</summary>
    EmergencyExit,
}
