using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Drawings.Halls;

/// <summary>
/// A layout and where it stands in a hall (<c>LayoutsLayerSurvey.md</c> 2.8). The <see cref="Layout"/> is the drawn
/// layout: the operating layout with the modules, the lines that order them, the joints, and the end profiles, standards
/// and overrides they need; the planner takes the result of it (<c>hallPlan.ToLayout()</c>). The hall plan adds the
/// hall, where each connected group stands in it (<see cref="Anchors"/>), and what only matters for setting up in that
/// hall. Module poses are not stored: they follow from the anchors and the lines.
/// </summary>
/// <remarks>
/// A document of its own, with a <see cref="Guid"/>, written in hall plan files. The layout is written there as
/// <c>content.layout</c> with the layout's own JSON options (<see cref="DrawnLayout.ToBlock"/>), so that what the planner
/// reads from a hall plan file (<see cref="HallPlanFile.ReadDrawnLayout"/>) is what was written. When it is written, the
/// layout's <see cref="Layout.SourceId"/> and <see cref="Layout.SourceRevision"/> are this hall plan's id and revision.
/// </remarks>
public sealed class HallPlan : LayoutPart
{
    /// <summary>The identity of the hall plan, the same in every file it is written to.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Guid Id { get; set; }

    /// <summary>What the hall plan is called, e.g. the name of the meeting.</summary>
    public required string Name { get; set; }

    /// <summary>A note for the reader.</summary>
    public string? Note { get; set; }

    /// <summary>An embedded copy of the hall (HA-5).</summary>
    public required Hall Hall { get; set; }

    /// <summary>
    /// The layout set up in the hall, as drawn. Its <see cref="DrawnLayout.PreferredStandard"/> is the hall plan's
    /// primary standard, which also tells the scale.
    /// </summary>
    [JsonConverter(typeof(DrawnLayoutJsonConverter))]
    public required DrawnLayout Layout { get; set; }

    /// <summary>Where each connected group of the layout stands in the hall: one per main line (HP-5).</summary>
    public List<Anchor> Anchors { get; set; } = [];

    /// <summary>Further standards accepted in the hall plan, besides the layout's preferred one (HP-1).</summary>
    public List<CatalogueReference> AcceptedStandards { get; set; } = [];

    /// <summary>Add-ons the hall plan prefers; modules that don't meet them are warned about (VA-14).</summary>
    public List<CatalogueReference> PreferredAddOns { get; set; } = [];

    /// <summary>Tolerances and defaults.</summary>
    public HallPlanSettings Settings { get; set; } = new();

    /// <summary>Areas where visitors have access (HP-17).</summary>
    public List<PublicArea> PublicAreas { get; set; } = [];

    /// <summary>Paths for train drivers to walk along (HP-18).</summary>
    public List<WalkingPath> WalkingPaths { get; set; } = [];

    /// <summary>Warnings the planner has accepted (HP-15).</summary>
    public List<Acknowledgement> Acknowledgements { get; set; } = [];

    /// <summary>Counted up each time the hall plan is saved as a new revision.</summary>
    public int Revision { get; set; }

    /// <summary>Who saved the hall plan, and the last date each of them did, newest first.</summary>
    public List<DrawingRevision> RevisionLog { get; set; } = [];

    /// <summary>What other systems keep about the hall plan, with one entry per system; read and written back unchanged.</summary>
    public Dictionary<string, JsonElement>? ExternalReferences { get; set; }

    /// <summary>The primary standard: the layout's preferred one. Null when not stated.</summary>
    [JsonIgnore]
    public ModuleStandard? PrimaryStandard => Layout.PreferredStandard;

    /// <summary>The primary standard, when it has an id, and the further accepted standards, each once.</summary>
    public IEnumerable<CatalogueReference> AllAcceptedStandards()
    {
        var all = PrimaryStandard is { Id: Guid id } primary ? AcceptedStandards.Prepend(new(id, primary.ShortName)) : AcceptedStandards;
        return all.DistinctBy(s => s.Id);
    }

    /// <summary>Whether modules of the standard <paramref name="standardId"/> are accepted.</summary>
    public bool IsAccepted(Guid standardId) => AllAcceptedStandards().Any(s => s.Id == standardId);

    /// <summary>The anchor of the main line <paramref name="lineId"/>, if any.</summary>
    public Anchor? FindAnchor(int lineId) => Anchors.FirstOrDefault(a => a.LineId == lineId);

    /// <summary>Adds <paramref name="area"/>, giving it the next id unless it has one not in use; returns it.</summary>
    public PublicArea Add(PublicArea area) => Add(PublicAreas, area, a => a.Id, (a, id) => a.Id = id);

    /// <summary>Adds <paramref name="path"/>, giving it the next id unless it has one not in use; returns it.</summary>
    public WalkingPath Add(WalkingPath path) => Add(WalkingPaths, path, p => p.Id, (p, id) => p.Id = id);

    /// <summary>Adds <paramref name="acknowledgement"/>, giving it the next id unless it has one not in use; returns it.</summary>
    public Acknowledgement Add(Acknowledgement acknowledgement) => Add(Acknowledgements, acknowledgement, a => a.Id, (a, id) => a.Id = id);

    /// <summary>
    /// A copy with copies of what the hall plan owns: the hall, the layout (copied through its JSON form, with its
    /// references restored), the anchors, areas, paths, acknowledgements, lists and external references.
    /// </summary>
    public override HallPlan Copy()
    {
        var copy = (HallPlan)base.Copy();
        copy.Hall = Hall.Copy();
        copy.Layout = Layout.Copy();
        copy.Anchors = [.. Anchors.Select(a => a.Copy())];
        copy.AcceptedStandards = [.. AcceptedStandards];
        copy.PreferredAddOns = [.. PreferredAddOns];
        copy.PublicAreas = [.. PublicAreas.Select(a => a.Copy())];
        copy.WalkingPaths = [.. WalkingPaths.Select(p => p.Copy())];
        copy.Acknowledgements = [.. Acknowledgements.Select(a => a.Copy())];
        copy.RevisionLog = [.. RevisionLog];
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        return copy;
    }

    /// <summary>
    /// What a hall plan file holds for this hall plan saved as <paramref name="revision"/>: a copy that shares what the
    /// hall plan owns, with <paramref name="revisionLog"/>, and a layout object of its own that has the hall plan's id and
    /// <paramref name="revision"/> as <see cref="Layout.SourceId"/> and <see cref="Layout.SourceRevision"/>, and the hall
    /// plan's name when it has none. This hall plan is not changed.
    /// </summary>
    public HallPlan Written(int revision, IEnumerable<DrawingRevision> revisionLog)
    {
        ArgumentNullException.ThrowIfNull(revisionLog);
        var written = (HallPlan)MemberwiseClone();
        written.Revision = revision;
        written.RevisionLog = [.. revisionLog];
        var drawn = Layout.Copy();
        drawn.Layout.SourceId = Id;
        drawn.Layout.SourceRevision = revision;
        if (string.IsNullOrWhiteSpace(drawn.Layout.Name)) drawn.Layout.Name = Name;
        written.Layout = drawn;
        return written;
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    private static T Add<T>(List<T> items, T item, Func<T, int> idOf, Action<T, int> setId)
    {
        ArgumentNullException.ThrowIfNull(item);
        var id = idOf(item);
        if (id <= 0 || items.Any(i => idOf(i) == id)) setId(item, items.Count == 0 ? 1 : Math.Max(0, items.Max(idOf)) + 1);
        items.Add(item);
        return item;
    }
}
