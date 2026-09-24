namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// The values a DCC address of a traction unit may take (see <see cref="VehicleContributor.DccAddress"/>).
/// </summary>
public static class DccAddresses
{
    /// <summary>
    /// The address entered for a traction unit whose owner is still to provide the real one.
    /// </summary>
    public const int ToBeProvided = 0;

    /// <summary>
    /// The highest address accepted: the four digits every common command station takes.
    /// </summary>
    public const int Highest = 9999;

    /// <summary>
    /// Whether <paramref name="address"/> may be stored as a DCC address: an address from 1 to
    /// <see cref="Highest"/>, or <see cref="ToBeProvided"/>.
    /// </summary>
    /// <param name="address">The address to check.</param>
    public static bool IsValid(int address) => address is >= ToBeProvided and <= Highest;
}
