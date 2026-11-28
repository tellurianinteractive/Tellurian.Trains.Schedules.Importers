namespace Tellurian.Trains.Schedules.Model.Drawings.Measures;

/// <summary>A measure that is entered in the Module Registry and derived from the drawing (MD-10).</summary>
public enum MeasureKind
{
    /// <summary>Total length of the main path, mm.</summary>
    Length,
    /// <summary>Length of the straight parts of the main path, mm.</summary>
    Straight,
    /// <summary>Radius of the main path's curves, mm.</summary>
    Radius,
    /// <summary>Total angle of the main path's curves, degrees.</summary>
    Angle,
    /// <summary>Width of the module, mm.</summary>
    Width,
    /// <summary>Number of sections.</summary>
    NumberOfSections,
}
