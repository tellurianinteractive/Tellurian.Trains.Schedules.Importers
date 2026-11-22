namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A stretch between two neighbouring locations as the lines of a <see cref="Layout"/> give it (<c>LayoutsLayerSurvey.md</c>
/// 3.3, step 1): which modules, the side each location is left from, and the number of tracks. It runs in the
/// direction of the line it is part of.
/// </summary>
public sealed record LineStretch
{
    /// <summary>The location the stretch starts at.</summary>
    public required OperationLocation Start { get; init; }

    /// <summary>The exit of a module of <see cref="Start"/> that the stretch leaves from.</summary>
    public required ExitReference StartExit { get; init; }

    /// <summary>The location the stretch ends at.</summary>
    public required OperationLocation End { get; init; }

    /// <summary>The exit of a module of <see cref="End"/> that the stretch arrives at.</summary>
    public required ExitReference EndExit { get; init; }

    /// <summary>The <see cref="LayoutModule.Id"/> of the plain-line modules between, in order from the start.</summary>
    public required IReadOnlyList<int> ModuleIds { get; init; }

    /// <summary>The fewest tracks at any joint of the stretch, by the end profiles; null when no end profile is known.</summary>
    public int? TracksCount { get; init; }

    /// <summary>
    /// The distance in metres from centre to centre along the main paths of the modules (survey 3.2, the fallback);
    /// null when a module has no main path, a location leaves across its main path (up or down) or spans more than one
    /// module.
    /// </summary>
    public double? MainPathDistance { get; init; }

    /// <summary>
    /// The shortest route along the drawn tracks from centre to centre (survey 3.3 step 2, 3.4); null when a module of
    /// the stretch has no track elements, or the tracks give no route, which is then a problem.
    /// </summary>
    public TrackRoute? Route { get; init; }

    /// <summary>
    /// The measured distance, metres: along the <see cref="Route"/>, else along the main paths; null when neither can be
    /// told.
    /// </summary>
    public double? Distance => Route?.Distance ?? MainPathDistance;

    /// <summary>
    /// The speed, km/h: the modules' speed limits, each weighted by the module's length along the stretch, the location
    /// modules from their centre to the exit; along the <see cref="Route"/> when there is one, else along the main paths.
    /// Null when no module has a limit.
    /// </summary>
    public int? Speed { get; init; }

    /// <summary>The <see cref="Line.Id"/> of the line the stretch is part of.</summary>
    public required int LineId { get; init; }

    /// <summary>Where in its line the stretch starts: the number of the joint it leaves by, for ordering.</summary>
    internal int Position { get; init; }

    /// <summary>The side of <see cref="Start"/> the stretch leaves from.</summary>
    public ExitSide StartSide => StartExit.Side;

    /// <summary>The side of <see cref="End"/> the stretch arrives at.</summary>
    public ExitSide EndSide => EndExit.Side;

    /// <inheritdoc/>
    public override string ToString() => $"{Start}–{End}";
}
