using System.ComponentModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// A module: what it is called and who owns it, which is the same wherever it is set up. A module can be an
/// operation location; a module that is not is plain line.
/// </summary>
public class Module : LayoutPart
{
    /// <summary>
    /// The identity of the module drawing, the same in every file it is written to; null for a module made without a
    /// drawing, as by the planner. Written first.
    /// </summary>
    [JsonPropertyOrder(FirstInFile)]
    public Guid? Id { get; set; }

    /// <summary>The name the owner has given the module, e.g. "Tellby" or "Curve 30°".</summary>
    public required string Name { get; set; }

    /// <summary>The letters the owner marks modules with, e.g. "SF"; null when there are none.</summary>
    public string? Prefix { get; set; }

    /// <summary>The owner's number of the module, e.g. 12; null when it has none.</summary>
    public int? Number { get; set; }

    /// <summary>
    /// The <see cref="Prefix"/> and <see cref="Number"/> are a FREMO module name: an owning person is a FREMO member
    /// with a FREMO prefix. Only then may the designation be presented as a FREMO name.
    /// </summary>
    public bool HasFremoDesignation { get; set; }

    /// <summary>Tells apart the ways one module can be set up, e.g. "Short"; null when there is only one.</summary>
    public string? ConfigurationLabel { get; set; }

    /// <summary>The box the module is transported in; null when it comes on its own.</summary>
    public string? PackageLabel { get; set; }

    /// <summary>
    /// The names of all owners, separated by commas, e.g. "Stefan Fjällemark, Module Club". For reading only: who
    /// owns a module is kept by the Module Registry, which sets or overwrites this when a drawing is uploaded.
    /// </summary>
    public string? ModuleOwners { get; set; }

    /// <summary>The standard the module is built to, which also tells its scale; null when not stated.</summary>
    public ModuleStandard? Standard { get; set; }

    /// <summary>
    /// The operation location that the module is; null for a module of plain line. How it is written depends on where
    /// the module is: on its own, as in a module file, the location is nested in the module
    /// (<see cref="NestedOperationLocation"/>); placed in a layout, where every location is written once in
    /// <see cref="Layout.OperationLocations"/>, the module refers to it by id (<see cref="OperationLocationId"/>).
    /// </summary>
    /// <remarks>
    /// The module's <see cref="Tracks"/> stay one list with the location's: a location without tracks given to a module
    /// with tracks takes the module's; a location with tracks brings its own; a module whose location is removed keeps
    /// the location's tracks as its own.
    /// </remarks>
    [JsonIgnore]
    public OperationLocation? OperationLocation
    {
        get;
        set
        {
            var tracks = Tracks;
            field = value;
            _operationLocationId = null;
            if (value is null) _ownTracks = tracks;
            else
            {
                if (value.Tracks.Count == 0 && tracks.Count > 0) value.Tracks = tracks;
                foreach (var track in value.Tracks) track.Station = value;
                _ownTracks = [];
            }
        }
    }

    /// <summary>Whether the module is placed in a layout, as a <see cref="PlacedModule"/>'s module; decides how it is written.</summary>
    internal bool IsPlaced { get; set; }

    /// <summary>
    /// The location as written in a module on its own: nested, with its tracks. Null, and so not written, in a layout.
    /// Read, the location's tracks become the module's (<see cref="Tracks"/>).
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("operationLocation")]
    public OperationLocation? NestedOperationLocation
    {
        get => IsPlaced ? null : OperationLocation;
        set => OperationLocation = value; // a file that wrote the tracks on the module and none in the location: they become the location's
    }

    /// <summary>
    /// The id of <see cref="OperationLocation"/>, as written in a layout; the id read until <c>layout.RestoreReferences()</c>
    /// has found the location. Null, and so not written, for a module on its own.
    /// </summary>
    public int? OperationLocationId
    {
        get => IsPlaced ? OperationLocation?.Id ?? _operationLocationId : null;
        set => _operationLocationId = value;
    }
    private int? _operationLocationId;

    /// <summary>
    /// The tracks of the module: the tracks of plain line, or those of its operation location, the very same list as
    /// <see cref="OperationLocation.Tracks"/>. They are written where they are owned: with the location when the module
    /// is one, else on the module (<see cref="OwnTracks"/>). Setting them on a module that is an operation location sets
    /// the location's.
    /// </summary>
    [JsonIgnore]
    public IList<StationTrack> Tracks
    {
        get => OperationLocation?.Tracks ?? _ownTracks;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (OperationLocation is { } location)
            {
                location.Tracks = value;
                foreach (var track in value) track.Station = location;
            }
            else _ownTracks = value;
        }
    }
    private IList<StationTrack> _ownTracks = [];

    /// <summary>
    /// The tracks as written on the module: its own, for a module of plain line; null, and so not written, for a module
    /// that is an operation location, whose tracks are written with the location.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("tracks")]
    public IList<StationTrack>? OwnTracks
    {
        get => OperationLocation is null && _operationLocationId is null ? Tracks : null;
        set
        {
            if (value is null) return;
            // A file that wrote the tracks on the module and none in the location: they are the location's.
            if (OperationLocation is { Tracks.Count: > 0 }) return;
            Tracks = value;
        }
    }

    /// <summary>The add-ons of the standard that the module meets.</summary>
    public List<CatalogueReference> AddOns { get; set; } = [];

    /// <summary>The first year of the period the module depicts.</summary>
    public int? RepresentsFromYear { get; set; }

    /// <summary>The last year of the period the module depicts.</summary>
    public int? RepresentsUptoYear { get; set; }

    /// <summary>The number of through tracks as entered; compared with the drawing.</summary>
    public int? NumberOfThroughTracks { get; set; }

    /// <summary>
    /// The highest speed allowed on the module, km/h of the prototype, as set by the owner and never worked out; null
    /// when the module sets no limit. The same unit as <see cref="TrackStretch.Speed"/> and <see cref="StationTrack.SpeedLimit"/>.
    /// </summary>
    public int? SpeedLimit { get; set; }

    /// <summary>The module has normal-gauge track. Always written, since it is true unless said otherwise.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool HasNormalGauge { get; set; } = true;

    /// <summary>The module has narrow-gauge track.</summary>
    public bool HasNarrowGauge { get; set; }

    /// <summary>The operator passes under the module to reach the inside of the layout.</summary>
    public bool IsDuckunder { get; set; }

    /// <summary>The module is a turntable.</summary>
    public bool IsTurntable { get; set; }

    /// <summary>The line ends on the module.</summary>
    public bool IsEndOfLine { get; set; }

    /// <summary>
    /// The module turns an asymmetric end profile over (German <i>Endewender</i>, Registry flag): an asymmetric
    /// feature, such as the high side of FREMO-H0-B96, is on one side of the track at one end and on the other side at
    /// the other end.
    /// </summary>
    public bool IsEndProfileSwitcher { get; set; }

    /// <summary>The module has a signal.</summary>
    public bool IsSignalModule { get; set; }

    /// <summary>The module can be shown on its own.</summary>
    public bool IsStandAlone { get; set; }

    /// <summary>The Module Registry's code for how far the module works.</summary>
    public int? FunctionalState { get; set; }

    /// <summary>The Module Registry's code for how far the landscape is finished.</summary>
    public int? LandscapeState { get; set; }

    /// <summary>A note for the reader.</summary>
    public string? Note { get; set; }

    /// <summary>Counted up each time the module is saved as a new revision.</summary>
    public int Revision { get; set; }

    /// <summary>Who saved the drawing, and the last date each of them did, newest first.</summary>
    public List<DrawingRevision> RevisionLog { get; set; } = [];

    /// <summary>What other systems keep about the module, with one entry per system; read and written back unchanged.</summary>
    public Dictionary<string, System.Text.Json.JsonElement>? ExternalReferences { get; set; }

    /// <summary>
    /// The geometry of the module, with its track elements; empty for a module without a drawing, as made by the
    /// planner. Never null, so code that draws needn't ask; it is written as <c>drawing</c> only when it isn't empty
    /// (<see cref="WrittenDrawing"/>).
    /// </summary>
    [JsonIgnore]
    public ModuleDrawing Drawing
    {
        get => _drawing ??= new();
        set => _drawing = value ?? throw new ArgumentNullException(nameof(value));
    }
    private ModuleDrawing? _drawing;

    /// <summary>Whether the module has a drawing: one that isn't empty.</summary>
    [JsonIgnore]
    public bool HasDrawing => _drawing is { IsEmpty: false };

    /// <summary>The drawing as written: null, and so not written, when it is empty. Written last, being the largest part.</summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [JsonPropertyName("drawing")]
    [JsonPropertyOrder(LastInFile)]
    public ModuleDrawing? WrittenDrawing
    {
        get => HasDrawing ? _drawing : null;
        set => _drawing = value;
    }

    /// <summary>
    /// A copy with copies of the module's operation location, tracks, drawing and lists; the tracks of a location
    /// module are those of the copied location. The standard is referred to, not copied.
    /// </summary>
    public override Module Copy()
    {
        var copy = (Module)base.Copy();
        copy.IsPlaced = false; // placed again when a layout module takes it
        copy._ownTracks = [.. _ownTracks.Select(track => track.Copy())];
        copy.OperationLocation = OperationLocation?.Copy();
        copy.AddOns = [.. AddOns];
        copy.RevisionLog = [.. RevisionLog];
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        copy._drawing = _drawing?.Copy();
        return copy;
    }

    private const int FirstInFile = -1;
    private const int LastInFile = 1;

    /// <inheritdoc/>
    public override string ToString() => this.DisplayName;
}

/// <summary>What a <see cref="Module"/> is called, worked out from what is given: prefix, number and name.</summary>
public static class ModuleExtensions
{
    extension(Module module)
    {
        /// <summary>
        /// The prefix and the number, as far as they are given: the prefix, a space and the number with four digits,
        /// "SF 0007"; without a prefix the number alone, as it is, "12". Null when the module has no number.
        /// </summary>
        public string? Designation =>
            module.Number is not int number ? null :
            HasText(module.Prefix) ? string.Create(CultureInfo.InvariantCulture, $"{module.Prefix!.Trim()} {number:0000}") :
            number.ToString(CultureInfo.InvariantCulture);

        /// <summary>The scale of the module: that of its standard. Null when not stated.</summary>
        public Scale? Scale => module.Standard?.Scale;

        /// <summary>
        /// What the module is called, without its standard and scale: what is written on it and where its standard
        /// goes without saying. With a prefix and a number: the designation, followed by the name of its operation
        /// location when it is one, "SF 0007 Tellby". With a number and no prefix: its name followed by the number,
        /// "Curve 12". Without a number: its name, followed by the configuration label in lower case when it has one,
        /// "Tellby short". Parts that are not given are left out; empty when nothing is.
        /// </summary>
        public string Title =>
            module.Designation is string designation && HasText(module.Prefix) ? Join(designation, module.OperationLocation?.Name) :
            module.Designation is string number ? Join(module.Name, number) :
            Join(module.Name, module.ConfigurationLabel?.Trim().ToLowerInvariant());

        /// <summary>
        /// What the module is called in lists: its standard and scale, when stated, followed by its
        /// <c>Title</c>, without a word said twice (<see cref="Wording.JoinWithoutRepeatedWords"/>).
        /// Standard "FREMO/H0-RE" with scale "H0" gives "FREMO/H0-RE SF 0007 Tellby".
        /// </summary>
        public string DisplayName => Wording.JoinWithoutRepeatedWords(module.Standard?.ShortName, module.Scale?.ShortName, module.Title);
    }

    private static string Join(params IEnumerable<string?> parts) => string.Join(' ', parts.Where(HasText).Select(p => p!.Trim()));

    private static bool HasText(string? value) => !string.IsNullOrWhiteSpace(value);
}
