namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A layout: its operation locations and the stretches between them, and what is decided for it as a whole. When it is
/// made from a hall plan, it also has what the hall plan's drawings gave: the modules without their drawings, the order
/// of the locations along each line, and on each stretch what was measured. The drawings themselves stay with the hall
/// plan (<see cref="DrawnLayout"/>). Ids are numbers that are unique within the layout for their kind of thing.
/// </summary>
public sealed class Layout : LayoutPart, IEquatable<Layout>
{
    /// <summary>Creates an empty layout.</summary>
    public Layout()
    {
        Countries = [];
        Regions = [];
        OperationLocations = [];
        TrackStretches = [];
        StretchLines = [];
        Modules = [];
    }

    /// <summary>The identity of the layout.</summary>
    public int Id { get; set; }

    /// <summary>The name of the layout.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The countries the layout uses, from the catalogue in the code.</summary>
    public ICollection<Country> Countries { get; set; }

    /// <summary>The regions stations can represent; each is stored here and referred to by id.</summary>
    public ICollection<Region> Regions { get; set; }

    /// <summary>The operation locations of the layout.</summary>
    public ICollection<OperationLocation> OperationLocations { get; set; }

    /// <summary>The stretches between neighbouring locations.</summary>
    public ICollection<TrackStretch> TrackStretches { get; set; }

    /// <summary>
    /// The order of the locations along each line of the hall plan the layout was made in, worked out when it was read;
    /// empty for a layout entered from scratch.
    /// </summary>
    public ICollection<StretchLine> StretchLines { get; set; }

    /// <summary>The id of the hall plan the layout was made in; null for a layout entered from scratch.</summary>
    public Guid? SourceId { get; set; }

    /// <summary>The revision of the hall plan the layout was made in, so a reader can tell what it has loaded.</summary>
    public int SourceRevision { get; set; }

    /// <summary>The scale of the layout: that of the hall plan's preferred standard, when it was made in one. Null when not stated.</summary>
    public Scale? Scale { get; set; }

    /// <summary>The short name of the module standard the hall plan is made for; null when not stated.</summary>
    public string? StandardName { get; set; }

    /// <summary>
    /// Every module of the hall plan the layout was made in, also the plain-line ones, without their drawings; empty for
    /// a layout entered from scratch.
    /// </summary>
    public ICollection<LayoutModule> Modules { get; set; }

    /// <inheritdoc/>
    public bool Equals(Layout? other) => other is not null && Id == other.Id;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Layout other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => Name;
}
