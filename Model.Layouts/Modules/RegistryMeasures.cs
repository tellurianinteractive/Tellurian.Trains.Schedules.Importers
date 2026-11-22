namespace Tellurian.Trains.Schedules.Model.Layouts.Modules;

/// <summary>
/// Measures as entered in, or imported from, the Module Registry. They are compared with the
/// measures derived from the drawing (MD-10). All are optional; lengths in mm, angle in degrees.
/// </summary>
public sealed record RegistryMeasures : ExtensibleRecord
{
    /// <summary>Total length of the straight parts, mm.</summary>
    public double? Straight { get; init; }
    /// <summary>Radius of the curves, mm; 0 or null for a straight module.</summary>
    public double? Radius { get; init; }
    /// <summary>Total angle of the curves, degrees.</summary>
    public double? Angle { get; init; }
    /// <summary>Total length, curves and straights, mm (Registry <c>TotalLength</c>).</summary>
    public double? Length { get; init; }
    /// <summary>Width of the module, mm.</summary>
    public double? Width { get; init; }
    /// <summary>Number of sections.</summary>
    public int? NumberOfSections { get; init; }
}
