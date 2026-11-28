namespace Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

/// <summary>What differs between two end profiles at a joint, or between a profile and its mirror image.</summary>
[Flags]
public enum FitDifferences
{
    /// <summary>Nothing differs.</summary>
    None = 0,
    /// <summary>The number of tracks differs.</summary>
    TrackCount = 1 << 0,
    /// <summary>Track offsets differ.</summary>
    TrackPositions = 1 << 1,
    /// <summary>Facing tracks have no gauge in common.</summary>
    Gauge = 1 << 2,
    /// <summary>Facing tracks have different rail systems.</summary>
    RailSystem = 1 << 3,
    /// <summary>The screw holes don't line up, or the screw-hole patterns differ.</summary>
    ScrewHoles = 1 << 4,

    /// <summary>Neither drawn screw holes nor pattern identifiers are available on both ends; assumed bolted.</summary>
    ScrewHolesUnknown = 1 << 5,

    /// <summary>The ground lines deviate more than the landscape tolerance.</summary>
    GroundLine = 1 << 6,

    /// <summary>At least one end has no ground line, so it wasn't compared.</summary>
    GroundLineUnknown = 1 << 7,

    /// <summary>The end-plate outlines differ. Informational: plate size isn't a fit criterion.</summary>
    Outline = 1 << 8,

    /// <summary>The dowels don't line up.</summary>
    Dowels = 1 << 9,

    /// <summary>Any difference in the track geometry (spec §3.2 rule 1).</summary>
    Tracks = TrackCount | TrackPositions | Gauge | RailSystem,
}
