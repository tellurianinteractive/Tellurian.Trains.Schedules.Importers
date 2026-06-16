namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Provides common day patterns for railway operations.
/// </summary>
public static class CommonDayPatterns
{
    /// <summary>Daily operation.</summary>
    public const ushort Daily = 0b_____10_1111111_1111111;
    /// <summary>Monday only.</summary>
    public const ushort Monday = 0b____10_0000001_0000001;
    /// <summary>Tuesday only.</summary>
    public const ushort Thuesday = 0b__10_0000010_0000010;
    /// <summary>Wednesday only.</summary>
    public const ushort Wednesday = 0b_10_0000100_0000100;
    /// <summary>Thursday only.</summary>
    public const ushort Thursday = 0b__10_0001000_0001000;
    /// <summary>Friday only.</summary>
    public const ushort Friday = 0b____10_0010000_0010000;
    /// <summary>Saturday only.</summary>
    public const ushort Saturday = 0b__10_0100000_0100000;
    /// <summary>Sunday only.</summary>
    public const ushort Sunday = 0b____10_1000000_1000000;

}
