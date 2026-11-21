using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The standard a module is built to, e.g. "FREMO/H0-RE": the one standard type of the suite (survey step 7). The
/// catalogue holds the full entry; where a module or a layout names a standard, only its id, short name and scale are
/// written (<c>NameOnly</c>, <see cref="ModuleStandardExtensions"/>), so every property beyond those is optional and null means "not stated", with the
/// default given where it is used. Lengths in millimetres.
/// </summary>
public sealed class ModuleStandard : LayoutPart, IEquatable<ModuleStandard>
{
    /// <summary>The usual height of rail top above the floor, mm.</summary>
    public const double DefaultRailTopHeight = 1300;

    /// <summary>The usual depth of an operator space, mm.</summary>
    public const double DefaultOperatorSpaceDepth = 800;

    /// <summary>How close two widths are to be the same gauge, mm.</summary>
    public const double GaugeTolerance = 0.05;

    /// <summary>The colour tracks are drawn in when the standard gives none.</summary>
    public const string DefaultTrackColor = "#1f5fbf";

    /// <summary>
    /// The identity of the standard in the catalogue, the same in every file; null where a standard is only named. The
    /// Module Registry's id is an external reference (decision 12).
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>What the standard is called, e.g. "FREMO/H0-RE". It is what identifies the standard to a reader.</summary>
    public required string ShortName { get; set; }

    /// <summary>The scale of the standard; null when not stated.</summary>
    public Scale? Scale { get; set; }

    /// <summary>The track system, e.g. "Code 75"; null when not stated.</summary>
    public string? TrackSystem { get; set; }

    /// <summary>How the track is powered; null when not stated (two-rail).</summary>
    public RailSystem? RailSystem { get; set; }

    /// <summary>The gauges the standard allows, the normal gauge first; null when not stated.</summary>
    public List<TrackGauge>? Gauges { get; set; }

    /// <summary>The couplings used, e.g. "Kadee"; null when not stated.</summary>
    public string? Couplings { get; set; }

    /// <summary>The electrical rules, e.g. "DCC"; null when not stated.</summary>
    public string? Electricity { get; set; }

    /// <summary>The wheelset norm, e.g. "NEM 310"; null when not stated.</summary>
    public string? Wheelset { get; set; }

    /// <summary>The height of rail top above the floor; null for <see cref="DefaultRailTopHeight"/>.</summary>
    public double? RailTopHeight { get; set; }

    /// <summary>The smallest clearance beside the track; null when not stated.</summary>
    public double? MinimumClearance { get; set; }

    /// <summary>The depth of an operator space; null for <see cref="DefaultOperatorSpaceDepth"/>.</summary>
    public double? OperatorSpaceDepth { get; set; }

    /// <summary>The colour tracks are drawn in, e.g. "#1f5fbf"; null for <see cref="DefaultTrackColor"/>.</summary>
    public string? TrackColor { get; set; }

    /// <summary>The distance from a turnout's points to where a train on the other track can still pass; null when not stated.</summary>
    public double? FoulingDistance { get; set; }

    /// <summary>The smallest radius of a reverse curve; null when not stated.</summary>
    public double? MinimumReverseCurveRadius { get; set; }

    /// <summary>The radius used for a curve whose radius isn't given; null when not stated.</summary>
    public double? DefaultCurveRadius { get; set; }

    /// <summary>The end profile a single-track module of the standard usually has; null when not stated.</summary>
    public Guid? DefaultSingleTrackEndProfileId { get; set; }

    /// <summary>The end profile a double-track module of the standard usually has; null when not stated.</summary>
    public Guid? DefaultDoubleTrackEndProfileId { get; set; }

    /// <summary>The document the standard is defined in.</summary>
    public DocumentReference? Source { get; set; }

    /// <summary>What other systems keep about the standard, one entry per system; read and written back unchanged.</summary>
    public Dictionary<string, JsonElement>? ExternalReferences { get; set; }

    /// <summary>The countries the standard is used in, as ISO codes; null or empty when it is used everywhere.</summary>
    public List<string>? Countries { get; set; }

    /// <summary>The short name of the scale, e.g. "H0"; empty when not stated.</summary>
    [JsonIgnore]
    public string ScaleName => Scale?.ShortName ?? string.Empty;

    /// <summary>The height of rail top, or the default.</summary>
    [JsonIgnore]
    public double RailTopHeightOrDefault => RailTopHeight ?? DefaultRailTopHeight;

    /// <summary>The depth of an operator space, or the default.</summary>
    [JsonIgnore]
    public double OperatorSpaceDepthOrDefault => OperatorSpaceDepth ?? DefaultOperatorSpaceDepth;

    /// <summary>The track colour, or the default.</summary>
    [JsonIgnore]
    public string TrackColorOrDefault => string.IsNullOrWhiteSpace(TrackColor) ? DefaultTrackColor : TrackColor;

    /// <summary>The normal gauge: the first of <see cref="Gauges"/>; null when none is stated.</summary>
    [JsonIgnore]
    public double? NormalGauge => Gauges is { Count: > 0 } gauges ? gauges[0].Width : null;

    /// <summary>The gauge of <paramref name="width"/> among <see cref="Gauges"/>, if the standard names it.</summary>
    public TrackGauge? GaugeOf(double width) => Gauges?.FirstOrDefault(g => Math.Abs(g.Width - width) < GaugeTolerance);

    /// <summary>Whether the standard applies in <paramref name="country"/>: it has no countries, or it lists this one.</summary>
    public bool AppliesTo(string? country) =>
        Countries is not { Count: > 0 } || (country is not null && Countries.Any(c => string.Equals(c, country, StringComparison.OrdinalIgnoreCase)));

    /// <summary>A copy with copies of what the standard owns: its lists, scale and external references.</summary>
    public override ModuleStandard Copy()
    {
        var copy = (ModuleStandard)base.Copy();
        copy.Scale = (Scale?)Scale?.Copy();
        copy.Gauges = Gauges is null ? null : [.. Gauges];
        copy.Countries = Countries is null ? null : [.. Countries];
        copy.ExternalReferences = ExternalReferences is null ? null : new(ExternalReferences);
        return copy;
    }

    /// <summary>Equal when they have the same short name, without regard to case: that is what identifies a standard to a reader.</summary>
    public bool Equals(ModuleStandard? other) => other is not null && string.Equals(ShortName, other.ShortName, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ModuleStandard other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(ShortName);

    /// <inheritdoc/>
    public override string ToString() => ShortName;
}

/// <summary>The forms a <see cref="ModuleStandard"/> is written in where it is only named.</summary>
public static class ModuleStandardExtensions
{
    extension(ModuleStandard standard)
    {
        /// <summary>
        /// The standard as named in a module or a layout: a new standard with only its short name and the short name of
        /// its scale. The full entry is the catalogue's.
        /// </summary>
        public ModuleStandard NameOnly => new()
        {
            ShortName = standard.ShortName,
            Scale = standard.Scale is null ? null : new Scale { ShortName = standard.Scale.ShortName },
        };
    }
}
