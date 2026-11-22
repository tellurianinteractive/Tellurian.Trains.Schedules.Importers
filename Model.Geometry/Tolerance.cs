namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// Numeric tolerances. All lengths are in millimetres and all angles in degrees.
/// </summary>
public static class Tolerance
{
    /// <summary>Two lengths or coordinates closer than this are equal.</summary>
    public const double Length = 1e-6;

    /// <summary>Two angles (degrees) closer than this are equal.</summary>
    public const double Angle = 1e-9;

    /// <summary>Two unit vectors whose cross product (the sine of the angle between them) is smaller than this are parallel.</summary>
    public const double Collinear = 1e-12;

    /// <summary>Default maximum distance between an arc and its tessellated chords.</summary>
    public const double Chord = 0.05;

    /// <summary>
    /// Maximum distance between an arc and its chords when measuring areas, e.g. of round pillars: coarse enough to keep
    /// the measure fast, while the area lost is about two thirds of a square millimetre per millimetre of arc.
    /// </summary>
    public const double AreaChord = 1;
}
