namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Provides common session patterns for railway operations.
/// </summary>
public static class CommonSessionPatterns
{
    /// <summary>All sessions active.</summary>
    public const ushort All = 0b______00_1111111_1111111;
    /// <summary>Odd sessions only (1, 3, 5, ...).</summary>
    public const ushort Odd = 0b______00_0101010_1010101;
    /// <summary>Even sessions only (2, 4, 6, ...).</summary>
    public const ushort Even = 0b_____00_1010101_0101010;
    /// <summary>Every third session, pattern 1 (1, 4, 7, ...).</summary>
    public const ushort Third1 = 0b___00_0100100_1001001;
    /// <summary>Every third session, pattern 2 (2, 5, 8, ...).</summary>
    public const ushort Third2 = 0b___00_1001001_0010010;
    /// <summary>Every third session, pattern 3 (3, 6, 9, ...).</summary>
    public const ushort Third3 = 0b___00_0010010_0100100;
    /// <summary>On-demand train flag.</summary>
    public const ushort OnDemand = 0b_01_0000000_0000000;
}
