
namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents train length restrictions specified in axles and/or meters.
/// </summary>
public readonly struct TrainLenght
{
    /// <summary>
    /// Gets or initializes the maximum number of axles.
    /// </summary>
    public int? Axles { get; init; }

    /// <summary>
    /// Gets or initializes the maximum length in meters.
    /// </summary>
    public int? Meters { get; init; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{this.MaxAxles} {this.MaxMeters}";
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
        internal string MaxAxles => lenght.Axles.HasValue ? $"{lenght.Axles.Value}ʘ" : string.Empty;

        /// <summary>
        /// Gets the formatted meter string (e.g., "150m").
        /// </summary>
        internal string MaxMeters => lenght.Meters.HasValue ? $"{lenght.Meters.Value}m" : string.Empty;
    }
}
