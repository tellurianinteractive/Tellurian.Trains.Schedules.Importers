namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>How a segment of a plate's upper edge reaches its end.</summary>
public enum PlateSegmentKind
{
    /// <summary>A straight line to the end point.</summary>
    Line,

    /// <summary>An arc around a centre to the end point, the shorter way round.</summary>
    Arc,

    /// <summary>
    /// An arc of a given radius that continues smoothly from the previous segment and meets the next one
    /// tangentially; where it ends is worked out, as a drawing gives only the radius.
    /// </summary>
    Fillet,
}
