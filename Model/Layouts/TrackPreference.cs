namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// What a train wants of the track it is put on at a location, used to choose between the tracks that
/// suit its route equally well. See <c>OperationLocation.PreferredTrack</c>.
/// </summary>
public enum TrackPreference
{
    /// <summary>
    /// A track with a platform: a passenger train stopping to let its passengers on and off. Where the
    /// location has no platform, such a train falls back to the main track like any other.
    /// </summary>
    Platform,

    /// <summary>
    /// The main track: a train running through, and any train with no passengers to exchange here.
    /// </summary>
    MainTrack
}
