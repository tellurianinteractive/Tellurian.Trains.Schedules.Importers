global using Tellurian.Trains.Schedules.Model.Geometry;
global using static Tellurian.Trains.Schedules.Model.Geometry.Tests.TestTolerance;

namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

/// <summary>Tolerances for asserting computed values in tests.</summary>
internal static class TestTolerance
{
    /// <summary>For values that are exact up to floating-point rounding.</summary>
    public const double Precision = 1e-9;

    /// <summary>Relative difference allowed between large computed values, e.g. areas in mm², that are exact up to rounding.</summary>
    public const double RelativePrecision = 1e-12;
}
