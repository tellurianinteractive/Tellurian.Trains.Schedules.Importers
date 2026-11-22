using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Files;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>
/// The shape of a module end, drawn in end view as seen from outside the module: X lateral (positive right), Y up,
/// origin on the centre line at rail top (RÖK); millimetres. A document of its own, with a <see cref="Guid"/>, written
/// in end profile files and embedded in module files, catalogues and hall plans.
/// </summary>
/// <remarks>
/// A mutable class like the rest of the shared model (decision 20 of <c>CrossAppReview.md</c>): an app that keeps
/// earlier states copies it with <see cref="Copy()"/> or <c>With(change)</c>. Its parts are values (paths, circles,
/// plate shape) and tracks, which it owns.
/// </remarks>
public sealed class EndProfile : LayoutPart
{
    /// <summary>The identity of the end profile, the same in every file it is written to.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required Guid Id { get; set; }

    /// <summary>The designation, e.g. "FREMO-H0-B96" or "MMM480" (Registry <c>ModuleEndProfile.Designation</c>).</summary>
    public required string Designation { get; set; }

    /// <summary>
    /// A short name, e.g. "B96", printed along the end plate in module drawings; null or empty uses
    /// <see cref="Designation"/>.
    /// </summary>
    public string? ShortName { get; set; }

    /// <summary>The short name, or the designation when there is none.</summary>
    [JsonIgnore]
    public string ShortNameOrDesignation => string.IsNullOrWhiteSpace(ShortName) ? Designation : ShortName.Trim();

    /// <summary>The short name of the scale, e.g. "H0".</summary>
    public required string Scale { get; set; }

    /// <summary>The standards the profile belongs to, by id and short name.</summary>
    public List<CatalogueReference> Standards { get; set; } = [];

    /// <summary>The end-plate outline: a closed path. Shown for information; it isn't a fit criterion.</summary>
    public PathDrawing? Outline { get; set; }

    /// <summary>
    /// The end plate as entered: its upper edge's segments (lines, arcs, fillets) and its bottom. When present,
    /// <see cref="Outline"/> and <see cref="GroundLine"/> are generated from it; they are still stored for readers
    /// that don't know the plate.
    /// </summary>
    public PlateShape? Plate { get; set; }

    /// <summary>The landscape surface across the end: an open path.</summary>
    public PathDrawing? GroundLine { get; set; }

    /// <summary>The tracks crossing the end, from left to right.</summary>
    public List<EndProfileTrack> Tracks { get; set; } = [];

    /// <summary>The screw holes that join two module ends.</summary>
    public List<Circle> ScrewHoles { get; set; } = [];

    /// <summary>Alignment pins, where the standard has them.</summary>
    public List<Circle> Dowels { get; set; } = [];

    /// <summary>Identifies the screw-hole pattern, used for fit when holes aren't drawn.</summary>
    public string? ScrewHolePattern { get; set; }

    /// <summary>The document the drawing was made from, such as the standard's PDF.</summary>
    public DocumentReference? Source { get; set; }

    /// <summary>What other systems keep about the end profile, with one entry per system; read and written back unchanged.</summary>
    public Dictionary<string, JsonElement>? ExternalReferences { get; set; }

    /// <summary>Counted up each time the end profile is saved as a new revision.</summary>
    public int Revision { get; set; }

    /// <summary>Who saved the drawing, and the last date each of them did, newest first.</summary>
    public List<DrawingRevision> RevisionLog { get; set; } = [];

    /// <summary>Whether screw holes are drawn.</summary>
    [JsonIgnore]
    public bool HasScrewHoles => ScrewHoles.Count > 0;

    /// <summary>A copy with copies of what the profile owns: its lists, tracks and external references.</summary>
    public override EndProfile Copy()
    {
        var copy = (EndProfile)base.Copy();
        copy.Standards = [.. Standards];
        copy.Tracks = [.. Tracks.Select(t => t.Copy())];
        copy.ScrewHoles = [.. ScrewHoles];
        copy.Dowels = [.. Dowels];
        copy.RevisionLog = [.. RevisionLog];
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        return copy;
    }

    /// <summary>
    /// A copy to edit as a profile of one's own: a new id, the designation <paramref name="designation"/>, no external
    /// references and revision 0, so the copy doesn't point to what the original is registered as. The source is kept,
    /// so the copy still says where it came from.
    /// </summary>
    public EndProfile Copy(string designation) => this.With(p =>
    {
        p.Id = Guid.NewGuid();
        p.Designation = designation;
        p.ExternalReferences = null;
        p.Revision = 0;
        p.RevisionLog = [];
    });

    /// <summary>The drawing mirrored about its centre line (X → −X); only for comparing end profiles.</summary>
    public EndProfile MirroredX() => this.With(p =>
    {
        p.Plate = null;
        p.Outline = Outline?.MirroredX();
        p.GroundLine = GroundLine?.MirroredX();
        p.Tracks = [.. Tracks.Select(t => t.MirroredX())];
        p.ScrewHoles = [.. ScrewHoles.Select(h => h.MirroredX())];
        p.Dowels = [.. Dowels.Select(d => d.MirroredX())];
    });

    /// <summary>The drawing as it is built at an exit with <paramref name="orientation"/>.</summary>
    public EndProfile Oriented(ProfileOrientation orientation) =>
        orientation == ProfileOrientation.Mirrored ? MirroredX() : this;

    /// <inheritdoc/>
    public override string ToString() => Designation;
}
