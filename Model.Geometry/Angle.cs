namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>
/// An angle in degrees, counter-clockwise from the positive X axis. The value is not normalised;
/// use <see cref="Normalized"/> or <see cref="Signed"/> when a canonical form is needed.
/// </summary>
public readonly record struct Angle(double Degrees)
{
    /// <summary>The angle 0 degrees, along the positive X axis.</summary>
    public static Angle Zero => new(0);
    /// <summary>The angle 90 degrees, a quarter turn counter-clockwise.</summary>
    public static Angle Quarter => new(90);
    /// <summary>The angle 180 degrees, a half turn.</summary>
    public static Angle Half => new(180);

    /// <summary>The angle in radians.</summary>
    public double Radians => Degrees * Math.PI / 180.0;

    /// <summary>The angle in the range [0, 360).</summary>
    public Angle Normalized
    {
        get
        {
            var d = Degrees % 360.0;
            if (d < 0) d += 360.0;
            if (d >= 360.0 - Tolerance.Angle) d = 0;
            return new(d);
        }
    }

    /// <summary>The angle in the range (-180, 180].</summary>
    public Angle Signed
    {
        get
        {
            var d = Normalized.Degrees;
            return new(d > 180.0 ? d - 360.0 : d);
        }
    }

    /// <summary>An angle from a value in radians.</summary>
    public static Angle FromRadians(double radians) => new(radians * 180.0 / Math.PI);

    /// <summary>True if the two angles point in the same direction, within <paramref name="tolerance"/> degrees.</summary>
    public bool IsCloseTo(Angle other, double tolerance = Tolerance.Angle) =>
        Math.Abs((this - other).Signed.Degrees) <= tolerance;

    /// <summary>The sum of two angles.</summary>
    public static Angle operator +(Angle a, Angle b) => new(a.Degrees + b.Degrees);
    /// <summary>The difference between two angles.</summary>
    public static Angle operator -(Angle a, Angle b) => new(a.Degrees - b.Degrees);
    /// <summary>The angle in the opposite rotational direction.</summary>
    public static Angle operator -(Angle a) => new(-a.Degrees);
    /// <summary>The angle multiplied by <paramref name="factor"/>.</summary>
    public static Angle operator *(Angle a, double factor) => new(a.Degrees * factor);

    /// <inheritdoc/>
    public override string ToString() => $"{Degrees:0.###}°";
}
