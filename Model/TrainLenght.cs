
namespace Tellurian.Trains.Schedules.Model;

public readonly struct TrainLenght
{
    public int? Axles { get; init; }
    public int? Meters { get; init; }
    public override string ToString() =>
        $"{this.MaxAxles} {this.MaxMeters}";
}

public static class TrainLengthExtensions
{
    extension(TrainLenght lenght)
    {
        public static TrainLenght AxlesOnly(int axles) =>
            new() { Axles = axles };

        public static TrainLenght MetersOnly(int meters) =>
            new() { Meters = meters };

        public static TrainLenght AxlesAndMeters(int axles, int meters) =>
            new() { Axles = axles, Meters = meters };

        internal string MaxAxles => lenght.Axles.HasValue ? $"{lenght.Axles.Value}ʘ" : string.Empty;
        internal string MaxMeters => lenght.Meters.HasValue ? $"{lenght.Meters.Value}m" : string.Empty;
    }
}
