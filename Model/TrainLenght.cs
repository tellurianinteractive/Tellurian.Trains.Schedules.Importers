
namespace Tellurian.Trains.Schedules.Model;

public readonly struct TrainLenght
{
    public int? Axles { get; init; }
    public int? Meters { get; init; }
    public override string ToString() =>
        Axles.HasValue && Meters.HasValue ? $"{Axles.Value}ʘ {Meters.Value}m" :
        Axles.HasValue ? $"{Axles.Value}ʘ" :
        Meters.HasValue ? $"{Meters.Value}m" :
        string.Empty;

    public static TrainLenght AxlesOnly(int axles) => new() { Axles = axles };
}
