using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

/// <summary>Parameters for <see cref="EndProfileTemplate.Create"/>. Heights are relative to rail top (Y = 0).</summary>
public sealed record EndProfileTemplateParameters
{
    /// <summary>The end profile's id; null for a new id.</summary>
    public Guid? Id { get; init; }
    /// <summary>The designation, e.g. "FREMO-H0-B96".</summary>
    public required string Designation { get; init; }
    /// <summary>The short name of the scale, e.g. "H0".</summary>
    public required string Scale { get; init; }
    /// <summary>The module standards the end profile belongs to.</summary>
    public IReadOnlyList<CatalogueReference> Standards { get; init; } = [];
    /// <summary>The width of the plate, centred on the centre line, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double PlateWidth { get; init; }

    /// <summary>The height from the track bed to rail top of every track; null for <see cref="TrackCrossSection.DefaultHeight"/>.</summary>
    public double? TrackHeight { get; init; }

    /// <summary>Y of the top edge of the plate; normally negative (below rail top), at minus the track height where the tracks are.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double PlateTop { get; init; }

    /// <summary>Y of the bottom edge of the plate.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double PlateBottom { get; init; }

    /// <summary>The tracks' lateral offsets from the centre line, positive to the right; one centred track by default.</summary>
    public IReadOnlyList<double> TrackOffsets { get; init; } = [0];
    /// <summary>The gauge of every track, mm.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public required double Gauge { get; init; }
    /// <summary>The rail system of every track.</summary>
    public RailSystem RailSystem { get; init; }
    /// <summary>The centres of the screw holes; holes too close to the plate's edges are left out.</summary>
    public IReadOnlyList<Point> ScrewHoles { get; init; } = [];
    /// <summary>The diameter of every screw hole, mm.</summary>
    public double ScrewHoleDiameter { get; init; } = 8;
    /// <summary>Identifies the screw-hole pattern, used for fit when holes aren't drawn.</summary>
    public string? ScrewHolePattern { get; init; }
    /// <summary>The document the drawing was made from, such as the standard's PDF.</summary>
    public DocumentReference? Source { get; init; }
}
