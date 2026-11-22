namespace Tellurian.Trains.Schedules.Model.Layouts;

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

    /// <summary>An exit's end profile isn't in <see cref="Layout.EndProfiles"/>, so its tracks can't be counted.</summary>
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
}
