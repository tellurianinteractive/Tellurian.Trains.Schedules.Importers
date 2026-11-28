namespace Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

/// <summary>How two module ends are held together.</summary>
public enum JoinMethod
{
    /// <summary>Bolts through matching screw holes.</summary>
    Bolted,

    /// <summary>Clamps (Swedish <i>tving</i>), when the screw holes don't line up.</summary>
    Clamped,
}
