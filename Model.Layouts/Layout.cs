using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A layout: its operation locations and the stretches between them, and what is decided for it as a whole. When it is
/// made in a hall plan, it also has the modules of the hall plan, also the plain-line ones, the lines that order them,
/// the planner's joints, and the end profiles, standards and overrides that the modules need, so that it is complete
/// without the hall. Ids are numbers that are unique within the layout for their kind of thing.
/// </summary>
public sealed class Layout : LayoutPart, IEquatable<Layout>
{
    /// <summary>Creates an empty layout.</summary>
    public Layout()
    {
        Countries = [];
        Companies = [];
        Regions = [];
        OperationLocations = [];
        TrackStretches = [];
        TimetableStretches = [];
        DispatchStretches = [];
        TopologyPositions = [];
        StretchLines = [];
        Modules = [];
        Lines = [];
        Joints = [];
        EndProfiles = [];
        Standards = [];
        Overrides = [];
    }

    /// <summary>The identity of the layout.</summary>
    public int Id { get; set; }

    /// <summary>The name of the layout.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>What is set for the layout as a whole.</summary>
    public LayoutSettings Settings { get; set; } = new();

    /// <summary>The countries the layout uses, from the catalogue in the code.</summary>
    public ICollection<Country> Countries { get; set; }

    /// <summary>The companies that operate trains and vehicles on the layout; each is stored here and referred to by id.</summary>
    public ICollection<Company> Companies { get; set; }

    /// <summary>The regions stations can represent; each is stored here and referred to by id.</summary>
    public ICollection<Region> Regions { get; set; }

    /// <summary>The operation locations of the layout.</summary>
    public ICollection<OperationLocation> OperationLocations { get; set; }

    /// <summary>The stretches between neighbouring locations.</summary>
    public ICollection<TrackStretch> TrackStretches { get; set; }

    /// <summary>The routes shown as one graphical timetable each.</summary>
    public ICollection<TimetableStretch> TimetableStretches { get; set; }

    /// <summary>The lines between the dispatch endpoints.</summary>
    public ICollection<DispatchStretch> DispatchStretches { get; set; }

    /// <summary>Where each location is drawn in the topology schematic.</summary>
    public ICollection<TopologyPosition> TopologyPositions { get; set; }

    /// <summary>
    /// The order of the locations along each line of the hall plan the layout was made in, worked out when it was read;
    /// empty for a layout entered from scratch.
    /// </summary>
    public ICollection<StretchLine> StretchLines { get; set; }

    /// <summary>The id of the hall plan the layout was made in; null for a layout entered from scratch.</summary>
    public Guid? SourceId { get; set; }

    /// <summary>The revision of the hall plan the layout was made in, so a reader can tell what it has loaded.</summary>
    public int SourceRevision { get; set; }

    /// <summary>
    /// The module standard that the hall plan is made for, which also tells its scale. Modules of another standard can
    /// be part of the layout. Null when not stated.
    /// </summary>
    public ModuleStandard? PreferredStandard { get; set; }

    /// <summary>
    /// Every module of the hall plan the layout was made in, also the plain-line ones; empty for a layout entered
    /// from scratch.
    /// </summary>
    public ICollection<LayoutModule> Modules { get; set; }

    /// <summary>
    /// The lines that order the <see cref="Modules"/>, end to end (HP-3). A line without <see cref="Line.StartAt"/> is a
    /// main line; it and the branches from it are one connected group. Empty for a layout entered from scratch.
    /// </summary>
    public ICollection<Line> Lines { get; set; }

    /// <summary>The joints the planner has decided something for (HP-11); the other joints follow the rules.</summary>
    public ICollection<Joint> Joints { get; set; }

    /// <summary>Every end profile that the <see cref="Modules"/> use, each once.</summary>
    public ICollection<EndProfile> EndProfiles { get; set; }

    /// <summary>
    /// The standards of the layout and of its modules, each once, with all their properties: rail-top height, fouling
    /// distance, clearance. A module's own <see cref="Module.Standard"/> may name its standard only.
    /// </summary>
    public ICollection<ModuleStandard> Standards { get; set; }

    /// <summary>The compatibility overrides that apply to pairs of the <see cref="EndProfiles"/>.</summary>
    public ICollection<EndProfileCompatibilityOverride> Overrides { get; set; }

    /// <summary>The tolerances that decide whether two module ends fit; null for <see cref="FitTolerances.Default"/>.</summary>
    public FitTolerances? FitTolerances { get; set; }

    /// <inheritdoc/>
    public bool Equals(Layout? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Layout other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;
}

/// <summary>What follows from the properties of a <see cref="Layout"/>.</summary>
public static class LayoutFacts
{
    extension(Layout layout)
    {
        /// <summary>The scale of the layout: that of its preferred standard. Null when not stated.</summary>
        public Scale? Scale => layout.PreferredStandard?.Scale;
    }
}
