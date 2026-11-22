namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>How well two module ends fit (spec §3.2), from best to worst.</summary>
public enum JointFit
{
    /// <summary>Tracks, screw holes and ground line match. Green ✓, bolted.</summary>
    Fits,

    /// <summary>Tracks and screw holes match, the ground line doesn't. Yellow !, bolted.</summary>
    FitsWithMismatch,

    /// <summary>Tracks match, screw holes don't. Yellow + clamp symbol, clamped.</summary>
    FitsWithClamps,

    /// <summary>The track geometry differs. Red ✕.</summary>
    Incompatible,
}
