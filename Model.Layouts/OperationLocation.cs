using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A place where trains can stop, meet or be routed: a station, a signal controlled location, an industrial area or
/// another location such as a halt. The hierarchy is closed: these four are all there are, and a switch over them is
/// checked to be complete. In JSON the kind is the type discriminator, <c>"kind"</c>, with the values of
/// <see cref="OperationLocationKind"/>.
/// </summary>
/// <remarks>
/// <para>
/// A location holds both what it is, wherever it is set up (name, signature, its tracks, what can be exchanged), and
/// what is decided for it in one layout (instructions, who controls it, timings), as properties of the same object
/// (<c>Documentation/ModelHarmonisation.md</c>, decisions 11 and 12). A module file leaves the decisions out.
/// </para>
/// <para>
/// The kind governs whether a train may stop at all, on top of what each call says: at a <see cref="Station"/> and an
/// <see cref="OtherLocation"/> a train stops when the call says so, at a <see cref="SignalControlledLocation"/> never.
/// A passenger train also needs <see cref="HasPassengerExchange"/>, a cargo train <see cref="HasCargoExchange"/>.
/// </para>
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(Station), nameof(OperationLocationKind.Station))]
[JsonDerivedType(typeof(SignalControlledLocation), nameof(OperationLocationKind.SignalControlled))]
[JsonDerivedType(typeof(IndustrialArea), nameof(OperationLocationKind.IndustrialArea))]
[JsonDerivedType(typeof(OtherLocation), nameof(OperationLocationKind.Other))]
public closed class OperationLocation : LayoutPart, IEquatable<OperationLocation>
{
    /// <summary>For JSON and EF Core.</summary>
    [JsonConstructor]
    protected OperationLocation()
    {
        Name = string.Empty;
        Signature = string.Empty;
        Tracks = [];
    }

    /// <summary>Creates a location with its identity, name and signature.</summary>
    /// <param name="id">The identity of the location within the layout.</param>
    /// <param name="name">The full name; an underscore is read as a space.</param>
    /// <param name="signature">The short name used in timetables.</param>
    /// <exception cref="ArgumentException">The name or the signature is empty.</exception>
    protected OperationLocation(int id, string name, string signature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(signature);
        Id = id;
        Name = name.Replace("_", " ", StringComparison.OrdinalIgnoreCase);
        Signature = signature;
        Tracks = [];
    }

    /// <summary>The foreign key of the layout the location belongs to. Not written: the layout owns the location.</summary>
    [JsonIgnore]
    public int LayoutId { get; set; }

    /// <summary>The layout the location belongs to. Not written: the layout owns the location.</summary>
    [JsonIgnore]
    public Layout Layout { get; set; } = default!;

    /// <summary>The identity of the location within the layout, which everything else refers to. Given once and never changed.</summary>
    public int Id { get; set; }

    /// <summary>The full name, e.g. "Tellby".</summary>
    public string Name { get; set; }

    /// <summary>The short name used in timetables, e.g. "Tby". Unique within the country of operation, without regard to case.</summary>
    public string Signature { get; set; }

    /// <summary>The country the location is operated as part of: an ISO 3166-1 alpha-2 code, e.g. "SE". Null when not stated.</summary>
    public string? CountryCode { get; set; }

    /// <summary>The owner of the module, as a name. Optional.</summary>
    public string? Owner { get; set; }

    /// <summary>The phone number of whoever operates the location. Optional.</summary>
    public int? PhoneNumber { get; set; }

    /// <summary>
    /// Instructions, as markdown, for how the location is worked in this layout: which tracks are used for what, how
    /// the shunting is arranged. Not the owner's general instructions for the module.
    /// </summary>
    public string Instructions { get; set; } = string.Empty;

    /// <summary>Suppresses notes about trains crossing or overtaking at this location.</summary>
    public bool HideMeets { get; set; }

    /// <summary>Suppresses trains that don't stop at this location, in some reports.</summary>
    public virtual bool HidePassings { get; set; }

    /// <summary>A train can change its direction of travel here, which typically requires a loco runaround.</summary>
    public virtual bool IsChangingTrainDirectionPossible { get; set; }

    /// <summary>Passengers board and alight here: tickets can be issued from and to this location.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public virtual bool HasPassengerExchange { get; set; } = true;

    /// <summary>Cargo wagons are exchanged here.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public virtual bool HasCargoExchange { get; set; } = true;

    /// <summary>The line ends here.</summary>
    public bool IsTerminus { get; set; }

    /// <summary>The location is a harbour.</summary>
    public bool IsHarbour { get; set; }

    /// <summary>A key is needed to work the turnouts; the key itself, and where it is held, is <see cref="LockKey"/>.</summary>
    public bool IsKeyRequired { get; set; }

    /// <summary>
    /// Where the centre of the location is in its module: metres along the main track from the left end. Distances
    /// between locations are measured from centre to centre. Null means the middle of the main track.
    /// </summary>
    public double? Centre { get; set; }

    /// <summary>
    /// The station whose shunting yard works this location's local freight, or <c>null</c> when nothing serves it.
    /// Only where cargo is exchanged. A shunting yard is derived from this relation, not declared. Written by its id,
    /// <see cref="CargoServedFromId"/>.
    /// </summary>
    [JsonIgnore]
    public Station? CargoServedFrom
    {
        get;
        set { field = value; _cargoServedFromId = null; }
    }

    /// <summary>
    /// The id of <see cref="CargoServedFrom"/>, the form it is written in: the station's id while there is one, else the
    /// id read, until <c>layout.RestoreReferences()</c> has found the station.
    /// </summary>
    [JsonInclude]
    internal int? CargoServedFromId
    {
        get => CargoServedFrom?.Id ?? _cargoServedFromId;
        set => _cargoServedFromId = value;
    }
    private int? _cargoServedFromId;

    /// <summary>
    /// The manned station whose dispatcher works this location's signals and switches from afar, or <c>null</c>.
    /// Only a signal controlled location, an unmanned station or an industrial area can be controlled. Written by its
    /// id, <see cref="ControlledById"/>.
    /// </summary>
    [JsonIgnore]
    public Station? ControlledBy
    {
        get;
        set { field = value; _controlledById = null; }
    }

    /// <summary>
    /// The id of <see cref="ControlledBy"/>, the form it is written in: the station's id while there is one, else the id
    /// read, until <c>layout.RestoreReferences()</c> has found the station.
    /// </summary>
    [JsonInclude]
    internal int? ControlledById
    {
        get => ControlledBy?.Id ?? _controlledById;
        set => _controlledById = value;
    }
    private int? _controlledById;

    /// <summary>The key a train needs to unlock the switches here, or <c>null</c> when none is needed.</summary>
    public LockKey? LockKey { get; set; }

    /// <summary>The tracks of the location.</summary>
    public IList<StationTrack> Tracks { get; set; }

    /// <summary>The operational times of this location; a value that is null takes the layout's default.</summary>
    public StationTimings Timings { get; set; } = new();

    /// <summary>
    /// What other systems keep about the location, with one entry per system, copied unchanged from its source, e.g.
    /// the Module Registry's station id. An import uses it to find the same location again.
    /// </summary>
    public Dictionary<string, JsonElement>? ExternalReferences { get; set; }

    /// <summary>
    /// A location of <paramref name="kind"/> that has no name or signature yet, as when a user starts entering one; the
    /// constructors of the kinds refuse that, and <see cref="OperationLocationRules.Validate(OperationLocation)"/> reports it.
    /// </summary>
    public static OperationLocation Create(OperationLocationKind kind) => kind switch
    {
        OperationLocationKind.Station => new Station(),
        OperationLocationKind.SignalControlled => new SignalControlledLocation(),
        OperationLocationKind.IndustrialArea => new IndustrialArea(),
        OperationLocationKind.Other => new OtherLocation(),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    /// <summary>
    /// A copy with copies of what the location owns: its tracks, which belong to the copy, its timings, its lock key and,
    /// for a station, its staff posts and the list of its regions. The locations it refers to, its layout and its
    /// regions themselves are not copied.
    /// </summary>
    public override OperationLocation Copy()
    {
        var copy = (OperationLocation)base.Copy();
        copy.Tracks = [.. Tracks.Select(track =>
        {
            var copied = track.Copy();
            copied.Station = copy;
            return copied;
        })];
        copy.Timings = Timings.Copy();
        copy.LockKey = LockKey is null ? null : new() { Name = LockKey.Name, HeldAt = LockKey.HeldAt };
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        if (copy is Station station)
        {
            station.StaffPosts = [.. station.StaffPosts.Select(post => new StaffPost { Roles = post.Roles })];
            station.Regions = [.. station.Regions];
            station.UnresolvedRegionIds = [.. station.UnresolvedRegionIds];
        }
        return copy;
    }

    /// <summary>The track designated <paramref name="designation"/>.</summary>
    /// <exception cref="InvalidOperationException">The location has no such track.</exception>
    public StationTrack this[string designation] =>
        Tracks.SingleOrDefault(t => t.Designation == designation) ?? throw new InvalidOperationException($"Station {Name} has no track '{designation}'");

    /// <inheritdoc/>
    public bool Equals(OperationLocation? other) => Signature.Equals(other?.Signature, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is OperationLocation other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Signature.GetHashCode(StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override string ToString() => Name;
}
