namespace Tellurian.Trains.Schedules.Model.Layouts.AutoDraw;

/// <summary>
/// The data a module is auto-drawn from (ModuleDrawing §8.1). The properties map one-to-one to
/// Module Registry data, except the ones marked as new. Lengths in mm, angles in degrees.
/// </summary>
public sealed record AutoDrawParameters : ExtensibleRecord
{
    /// <summary>The module id to use; a new one is created if null.</summary>
    public Guid? ModuleId { get; init; }

    /// <summary>Registry <c>FullName</c>; may be empty for a module identified by its FREMO designation.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The owner's number of the module (Registry <c>FremoNumber</c>).</summary>
    public int? Number { get; init; }

    /// <summary>The letters the owner marks modules with, e.g. <c>SFjä</c>; with the number it forms the designation.</summary>
    public string? Prefix { get; init; }

    /// <summary>The names of all owners in one text, separated by commas.</summary>
    public string? ModuleOwners { get; init; }

    /// <summary>Tells apart the ways one module can be set up, e.g. "Short" (Registry <c>ConfigurationLabel</c>).</summary>
    public string? ConfigurationLabel { get; init; }

    /// <summary>The standard's short name, e.g. "FREMO/H0-RE" or "MMM-2R".</summary>
    public required string Standard { get; init; }

    /// <summary>Total length of the straight parts.</summary>
    public double? Straight { get; init; }
    /// <summary>Radius of the curve.</summary>
    public double? Radius { get; init; }
    /// <summary>Angle of the curve.</summary>
    public double? Angle { get; init; }

    /// <summary>Registry <c>Length</c>; only kept for comparison (MD-10).</summary>
    public double? Length { get; init; }

    /// <summary>The widest part of the module.</summary>
    public double? Width { get; init; }

    /// <summary>The number of through tracks; the end profiles decide when it differs.</summary>
    public int? NumberOfThroughTracks { get; init; }
    /// <summary>The number of baseboard sections.</summary>
    public int? NumberOfSections { get; init; }
    /// <summary>The registered exits; default exits are assumed when empty.</summary>
    public IReadOnlyList<AutoDrawExit> Exits { get; init; } = [];

    /// <summary>The module is flagged as end of line (Registry flag).</summary>
    public bool IsEndOfLine { get; init; }
    /// <summary>People can pass under the module (Registry flag); the whole outline becomes a duck-under area.</summary>
    public bool IsDuckunder { get; init; }
    /// <summary>The module is a turntable (Registry flag).</summary>
    public bool IsTurntable { get; init; }

    /// <summary>
    /// The module turns an asymmetric end profile over (<i>Endewender</i>, Registry flag), so an asymmetric profile
    /// isn't mirrored at the far exits (ModuleDrawing §4.3).
    /// </summary>
    public bool IsEndProfileSwitcher { get; init; }

    /// <summary>The module is the operation place of a station (Registry <c>Station.PrimaryModule</c>).</summary>
    public bool IsOperationPlace { get; init; }

    /// <summary>New, not in the Registry.</summary>
    public CurveDirection CurveDirection { get; init; }

    /// <summary>New, not in the Registry.</summary>
    public StraightPosition StraightPosition { get; init; }

    /// <summary>New, not in the Registry.</summary>
    public WideningSide WideningSide { get; init; }

    /// <summary>New, not in the Registry. The angle at which a widened body runs out from the end plate.</summary>
    public double TaperAngle { get; init; } = 45;
}
