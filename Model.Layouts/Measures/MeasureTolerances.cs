namespace Tellurian.Trains.Schedules.Model.Layouts.Measures;

/// <summary>How much a derived measure may differ from the entered Registry value before it is flagged (MD-10).</summary>
public sealed record MeasureTolerances
{
    /// <summary>The Registry keeps lengths in whole mm and rounds the arc length.</summary>
    public const double DefaultLength = 1;

    /// <summary>The default angle tolerance, degrees.</summary>
    public const double DefaultAngle = 0.1;

    /// <summary>The default tolerances.</summary>
    public static MeasureTolerances Default { get; } = new();

    /// <summary>For lengths, radius and width, mm.</summary>
    public double Length { get; init; } = DefaultLength;

    /// <summary>Degrees.</summary>
    public double Angle { get; init; } = DefaultAngle;
}
