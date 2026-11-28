namespace Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

/// <summary>How the track is powered. Tracks of different rail systems can't be joined (spec §3.2 rule 1).</summary>
public enum RailSystem
{
    /// <summary>Two-rail track: the two running rails carry the current.</summary>
    TwoRail,
    /// <summary>Three-rail track: a centre conductor and the running rails carry the current.</summary>
    ThreeRail,
}
