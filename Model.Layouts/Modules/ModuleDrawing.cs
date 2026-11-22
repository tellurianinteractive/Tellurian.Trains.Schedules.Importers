using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;
using Tellurian.Trains.Schedules.Model.Layouts.Tracks;

namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>
/// The drawing of a module, in the module's local coordinates: origin at the first exit's end-plate centre,
/// X pointing into the module from it (ModuleDrawing §2). A module has one drawing: a module that can be set up
/// in several ways is several modules, each with a file of its own. Only the Module Registry knows that they belong together.
/// </summary>
public sealed class ModuleDrawing : LayoutPart, IJsonOnSerializing, IJsonOnDeserialized
{
    /// <summary>The exits, in order; the first one defines the origin of the module coordinates.</summary>
    public List<ModuleExit> Exits { get; set; } = [];

    /// <summary>The centre line from the first exit to the second; null if there is none.</summary>
    public PathDrawing? MainPath { get; set; }

    /// <summary>The sides between end plates, in counter-clockwise order.</summary>
    public List<ModuleSide> Sides { get; set; } = [];

    /// <summary>The baseboard sections of the module.</summary>
    public List<Section> Sections { get; set; } = [];
    /// <summary>The fixed joints between the sections.</summary>
    public List<InternalJoint> InternalJoints { get; set; } = [];
    /// <summary>
    /// The track elements of the drawing: the geometry of the track plan (spec MD-6). Each refers by
    /// <see cref="TrackElement.StationTrackId"/> to a track of the module's list (the <c>Tracks</c> of the
    /// <see cref="Module"/>, spec MD-8), which has the properties that aren't geometry.
    /// </summary>
    public List<TrackElement> TrackElements { get; set; } = [];

    /// <summary>The areas next to the module where operators stand (MD-17).</summary>
    public List<OperatorSpace> OperatorSpaces { get; set; } = [];
    /// <summary>The sides, or parts of sides, meant to face the public (MD-18).</summary>
    public List<PublicSide> PublicSides { get; set; } = [];
    /// <summary>The parts of the module that people can pass under (MD-19).</summary>
    public List<DuckUnderArea> DuckUnderAreas { get; set; } = [];
    /// <summary>Text labels and markers on the drawing (MD-9).</summary>
    public List<ModuleLabel> Labels { get; set; } = [];

    /// <summary>Measures entered in or imported from the Registry, for comparison (MD-10).</summary>
    public RegistryMeasures? RegistryMeasures { get; set; }

    /// <summary>Set while the drawing is as auto-draw generated it (MD-20); null once edited by hand.</summary>
    public AutoDrawRecord? AutoDraw { get; set; }

    /// <summary>The kind of module, from the number of exits; not stored.</summary>
    [JsonIgnore]
    public ModuleKind Kind => Exits.Count switch
    {
        0 => ModuleKind.Invalid,
        1 => ModuleKind.EndOfLine,
        2 => ModuleKind.PassThrough,
        _ => ModuleKind.Junction,
    };

    /// <summary>The exit with <paramref name="label"/>; null when there is none.</summary>
    public ModuleExit? Exit(string label) => Exits.FirstOrDefault(e => e.Label == label);

    /// <summary>The exit at <paramref name="place"/>; null when there is none.</summary>
    public ModuleExit? Exit(ExitPlace place)
    {
        ArgumentNullException.ThrowIfNull(place);
        return place.In(this);
    }

    /// <summary>The lowest positive number that no track element of the drawing has as its id.</summary>
    public int FreeTrackElementId()
    {
        var used = TrackElements.Select(e => e.Id).ToHashSet();
        var id = 1;
        while (used.Contains(id)) id++;
        return id;
    }

    /// <summary>
    /// Gives every track element that has no id (0), or the id of an element before it, the lowest number that no
    /// element has, in the order of the list. Elements with ids of their own keep them.
    /// </summary>
    public void NumberTrackElements()
    {
        var seen = new HashSet<int>();
        foreach (var element in TrackElements.Where(e => e.Id < 1 || !seen.Add(e.Id)).ToList())
        {
            element.Id = FreeTrackElementId();
            seen.Add(element.Id);
        }
    }

    /// <summary>Track elements without an id get one before they are written, so a file never has an element without one.</summary>
    void IJsonOnSerializing.OnSerializing() => NumberTrackElements();

    /// <summary>Track elements read without an id, or with the id of an element before them, get one.</summary>
    void IJsonOnDeserialized.OnDeserialized() => NumberTrackElements();

    /// <summary>
    /// Makes the change <paramref name="changeExits"/> to the exits and keeps every side at the exits it was at: an
    /// exit whose direction changes, or that moves in the list, has another side and number (<see cref="ExitPlace"/>)
    /// afterwards. An exit is found again as itself or by its label; a side at an exit that is gone keeps its place.
    /// </summary>
    public void ChangeExits(Action<List<ModuleExit>> changeExits)
    {
        ArgumentNullException.ThrowIfNull(changeExits);
        var ends = Sides.Select(side => (side, From: side.FromExit.In(this), To: side.ToExit.In(this))).ToList();
        changeExits(Exits);
        foreach (var (side, from, to) in ends)
        {
            if (from is not null && ExitPlace.Of(this, from) is { } fromPlace) side.FromExit = fromPlace;
            if (to is not null && ExitPlace.Of(this, to) is { } toPlace) side.ToExit = toPlace;
        }
    }

    /// <summary>Whether nothing is drawn: no exits, sides, sections, track elements, areas, labels or measures.</summary>
    [JsonIgnore]
    public bool IsEmpty =>
        Exits.Count == 0 && MainPath is null && Sides.Count == 0 && Sections.Count == 0 && InternalJoints.Count == 0
        && TrackElements.Count == 0
        && OperatorSpaces.Count == 0 && PublicSides.Count == 0 && DuckUnderAreas.Count == 0 && Labels.Count == 0
        && RegistryMeasures is null && AutoDraw is null && ExtensionData is null;

    /// <summary>A copy with copies of what the drawing owns: its lists, exits, sides, sections, joints and track elements.</summary>
    public override ModuleDrawing Copy()
    {
        var copy = (ModuleDrawing)base.Copy();
        copy.Exits = [.. Exits.Select(e => e.Copy())];
        copy.Sides = [.. Sides.Select(s => s.Copy())];
        copy.Sections = [.. Sections.Select(s => s.Copy())];
        copy.InternalJoints = [.. InternalJoints.Select(j => j.Copy())];
        copy.TrackElements = [.. TrackElements.Select(t => t.Copy())];
        copy.OperatorSpaces = [.. OperatorSpaces];
        copy.PublicSides = [.. PublicSides];
        copy.DuckUnderAreas = [.. DuckUnderAreas];
        copy.Labels = [.. Labels];
        return copy;
    }
}
