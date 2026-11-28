global using static Tellurian.Trains.Schedules.Model.Drawings.Tests.TestTolerance;

namespace Tellurian.Trains.Schedules.Model.Drawings.Tests;

/// <summary>Tolerances for asserting computed values in tests.</summary>
internal static class TestTolerance
{
    /// <summary>For values that are exact up to floating-point rounding.</summary>
    public const double Precision = 1e-9;

    /// <summary>Relative difference allowed between areas computed from separately tessellated outlines.</summary>
    public const double RelativeArea = 1e-4;
}
