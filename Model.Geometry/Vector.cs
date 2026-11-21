namespace Tellurian.Trains.Schedules.Model.Geometry;

/// <summary>A 2D vector in millimetres.</summary>
public readonly record struct Vector(double X, double Y)
{
    /// <summary>The zero vector.</summary>
    public static Vector Zero => new(0, 0);

    /// <summary>The length of the vector in millimetres.</summary>
    public double Length => Math.Sqrt(X * X + Y * Y);
    /// <summary>The squared length; cheaper than <see cref="Length"/> for comparisons.</summary>
    public double LengthSquared => X * X + Y * Y;

    /// <summary>The direction of the vector. Undefined (zero) for the zero vector.</summary>
    public Angle Direction => Angle.FromRadians(Math.Atan2(Y, X));

    /// <summary>The vector rotated 90° counter-clockwise (to the left).</summary>
    public Vector LeftNormal => new(-Y, X);

    /// <summary>The unit vector in the same direction, or <see cref="Zero"/> if the vector is shorter than <see cref="Tolerance.Length"/>.</summary>
    public Vector Normalized
    {
        get
        {
            var length = Length;
            return length < Tolerance.Length ? Zero : new(X / length, Y / length);
        }
    }

    /// <summary>A vector of <paramref name="length"/> millimetres in the direction <paramref name="angle"/>.</summary>
    public static Vector FromAngle(Angle angle, double length = 1.0) =>
        new(Math.Cos(angle.Radians) * length, Math.Sin(angle.Radians) * length);

    /// <summary>The dot product with <paramref name="other"/>.</summary>
    public double Dot(Vector other) => X * other.X + Y * other.Y;

    /// <summary>The z component of the 3D cross product; positive when <paramref name="other"/> is to the left.</summary>
    public double Cross(Vector other) => X * other.Y - Y * other.X;

    /// <summary>The vector rotated <paramref name="angle"/> counter-clockwise.</summary>
    public Vector Rotated(Angle angle)
    {
        var (sin, cos) = Math.SinCos(angle.Radians);
        return new(X * cos - Y * sin, X * sin + Y * cos);
    }

    /// <summary>True if the two vectors differ by at most <paramref name="tolerance"/> millimetres.</summary>
    public bool IsCloseTo(Vector other, double tolerance = Tolerance.Length) => (this - other).Length <= tolerance;

    /// <summary>The sum of two vectors.</summary>
    public static Vector operator +(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);
    /// <summary>The difference between two vectors.</summary>
    public static Vector operator -(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y);
    /// <summary>The vector pointing the opposite way.</summary>
    public static Vector operator -(Vector a) => new(-a.X, -a.Y);
    /// <summary>The vector scaled by <paramref name="factor"/>.</summary>
    public static Vector operator *(Vector a, double factor) => new(a.X * factor, a.Y * factor);
    /// <summary>The vector scaled by <paramref name="factor"/>.</summary>
    public static Vector operator *(double factor, Vector a) => new(a.X * factor, a.Y * factor);
    /// <summary>The vector divided by <paramref name="divisor"/>.</summary>
    public static Vector operator /(Vector a, double divisor) => new(a.X / divisor, a.Y / divisor);

    /// <inheritdoc/>
    public override string ToString() => $"({X:0.###}, {Y:0.###})";
}
