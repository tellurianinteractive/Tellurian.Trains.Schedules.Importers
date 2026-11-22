namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The direction trains run on a <see cref="StationTrack"/>. Left and right are those of a viewer standing on the
/// viewing side of the module; they only mean something together with it. In a <see cref="Layout"/> the neighbouring
/// locations follow from the side each <see cref="TrackStretch"/> leaves the location from.
/// </summary>
public enum TrackDirection
{
    /// <summary>Trains run in both directions.</summary>
    Both = 0,

    /// <summary>Trains run from the left end to the right end.</summary>
    LeftToRight = 1,

    /// <summary>Trains run from the right end to the left end.</summary>
    RightToLeft = 2,
}
