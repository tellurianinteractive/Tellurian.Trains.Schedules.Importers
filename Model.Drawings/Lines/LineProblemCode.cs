namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>
/// Why the lines of a <see cref="Layout"/> don't give a stretch, or give one in doubt. User interfaces show a text per
/// code (<c>LayoutsLayerSurvey.md</c> 2.10).
/// </summary>
public enum LineProblemCode
{
    /// <summary>A line or a join names a module that isn't in <see cref="Layout.Modules"/>.</summary>
    ModuleMissing = 0,

    /// <summary>A line or a join names an exit that the module's drawing doesn't have.</summary>
    ExitMissing = 1,

    /// <summary>An exit is joined to more than one other exit.</summary>
    ExitJoinedTwice = 2,

    /// <summary>An exit's end profile isn't in <see cref="DrawnLayout.EndProfiles"/>, so its tracks can't be counted.</summary>
    EndProfileMissing = 3,

    /// <summary>Plain-line modules lead from a location to a free exit or a dead end, with no location there.</summary>
    LineEndsWithoutLocation = 4,

    /// <summary>A plain-line module joins more than two others, so the stretch through it is not one line.</summary>
    JunctionWithoutLocation = 5,

    /// <summary>Plain-line modules form a loop with no location on it.</summary>
    LoopWithoutLocation = 6,

    /// <summary>
    /// Two locations are joined by more than one way of plain-line modules. Only the first is a stretch, since a
    /// stretch is known by its two locations.
    /// </summary>
    StretchesInParallel = 7,

    /// <summary>A track of an exit's end profile has no track element ending where it meets the joint.</summary>
    TrackMissingAtExit = 8,

    /// <summary>A track of one exit has no track at the matching offset of the exit it is joined to.</summary>
    TracksDontMeetAtJoint = 9,

    /// <summary>No drawn track crosses the location's centre, so no route along the tracks can start or end there.</summary>
    NoTrackAtCentre = 10,

    /// <summary>The tracks are drawn, but there is no way along them from one location's centre to the next.</summary>
    NoTrackPath = 11,

    /// <summary>A module in a hall plan's lines can't be placed: its group has no anchor, or nothing joins it to the anchor.</summary>
    ModuleNotPlaced = 12,

    /// <summary>A joint names an exit that the module's drawing doesn't draw, so the next module can't be placed against it.</summary>
    ExitNotDrawn = 13,

    /// <summary>A loop or rejoining line doesn't close within the hall plan's joint gap and angle (VA-5).</summary>
    ClosureGapTooLarge = 14,

    /// <summary>The rail tops at a joint differ by more than the hall plan's height tolerance (VA-10).</summary>
    HeightStepTooLarge = 15,

    /// <summary>The two ends of a joint are incompatible: their track geometry differs (HP-11).</summary>
    JointDoesNotFit = 16,
}
