using Tellurian.Trains.Schedules.Model.Resources;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents train length restrictions specified in axles and/or meters.
/// </summary>
public readonly struct TrainLenght
{
    /// <summary>
    /// Specification of train length. There may be no limitation or any compination of limitations.
    /// </summary>
    /// <param name="axles">Max number of axles.</param>
    /// <param name="wagons">Max number of wagons.</param>
    /// <param name="meters">Max length in meters.</param>
    public TrainLenght(int? axles, int? wagons, double? meters)
    {
        Axles = axles;
        Wagons = wagons;
        Meters = meters;
    }
    /// <summary>
    /// Gets or initializes the maximum number of axles.
    /// </summary>
    public int? Axles { get; init; }

    /// <summary>
    ///  Gets or initializes the maximum number of wagons.
    /// </summary>
    public int? Wagons { get; init; }

    /// <summary>
    /// Gets or initializes the maximum length in meters.
    /// </summary>
    public double? Meters { get; init; }

    /// <inheritdoc/>
    public override string ToString() =>
     this.HasValue || this.HasWagons || this.HasMeters ?
        $"{this.MaxAxles} {this.MaxWagons} {this.MaxMeters}" : Strings.Undefined;

}

/// <summary>
/// Provides extension methods for <see cref="TrainLenght"/>.
/// </summary>
public static class TrainLengthExtensions
{
    extension(TrainLenght lenght)
    {
        /// <summary>
        /// Gets an unspecified train length.
        /// </summary>
        public static TrainLenght Unspecified => new();

        /// <summary>
        /// Creates a train length with only axle restriction.
        /// </summary>
        /// <param name="axles">The maximum number of axles.</param>
        /// <returns>A train length with axle restriction.</returns>
        public static TrainLenght AxlesOnly(int axles) =>
            new() { Axles = axles };

        /// <summary>
        /// Creates a train length with only wagons restriction.
        /// </summary>
        /// <param name="wagons"></param>
        /// <returns></returns>
        public static TrainLenght WagonsOnly(int wagons) =>
            new() { Wagons = wagons };

        /// <summary>
        /// Creates a train length with only meter restriction.
        /// </summary>
        /// <param name="meters">The maximum length in meters.</param>
        /// <returns>A train length with meter restriction.</returns>
        public static TrainLenght MetersOnly(int meters) =>
            new() { Meters = meters };

        /// <summary>
        /// Creates a train length with both axle and meter restrictions.
        /// </summary>
        /// <param name="axles">The maximum number of axles.</param>
        /// <param name="meters">The maximum length in meters.</param>
        /// <returns>A train length with both restrictions.</returns>
        public static TrainLenght AxlesAndMeters(int axles, int meters) =>
            new() { Axles = axles, Meters = meters };

        /// <summary>
        /// Gets the formatted axle string (e.g., "24ʘ").
        /// </summary>
        internal string MaxAxles => lenght.HasAxles ? $"{lenght.Axles!.Value}ʘ" : string.Empty;

        /// <summary>
        /// Gets the formatted axle string (e.g., "12■").
        /// </summary>
        internal string MaxWagons => lenght.HasWagons ? $"{lenght.Wagons!.Value}■" : string.Empty;

        /// <summary>
        /// Gets the formatted meter string (e.g., "2,5m").
        /// </summary>
        internal string MaxMeters => lenght.HasMeters ? $"{lenght.Meters!.Value:F1}m" : string.Empty;

        internal bool HasAxles => lenght.Axles.HasValue;
        internal bool HasWagons => lenght.Wagons.HasValue;
        internal bool HasMeters => lenght.Meters.HasValue;
    }
}
