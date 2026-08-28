namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// What a train exchanges where it stops, which is what decides where it is able to stop at all: a
/// passenger train needs somewhere to exchange passengers, a cargo train somewhere to exchange cargo,
/// and a train that carries both needs either.
/// </summary>
/// <remarks>
/// The flags say what the train hands over, not what it is for. A train that exchanges nothing —
/// <see cref="None"/> — is a service train: a construction train working at a site, or a locomotive or
/// trainset being moved out of service. What such a train is <em>for</em> is said by its category's
/// <see cref="TrainCategory.Name"/>, not by this. A construction train that leaves material wagons
/// behind exchanges cargo like any other freight train, and belongs in a <see cref="Cargo"/> category.
/// </remarks>
[Flags]
public enum TrainContent
{
    /// <summary>
    /// The train exchanges nothing: a service train, which therefore may stop anywhere its route allows
    /// without the location having to exchange anything.
    /// </summary>
    None = 0,

    /// <summary>
    /// The train exchanges passengers, so it can stop where
    /// <see cref="Layouts.OperationLocation.HasPassengerExchange"/>.
    /// </summary>
    Passenger = 1,

    /// <summary>
    /// The train exchanges cargo wagons, so it can stop where
    /// <see cref="Layouts.OperationLocation.HasCargoExchange"/>.
    /// </summary>
    Cargo = 2,
}

/// <summary>
/// Provides extension members for <see cref="TrainContent"/>.
/// </summary>
public static class TrainContentExtensions
{
    extension(TrainContent)
    {
        /// <summary>
        /// The content stated as the pair of booleans that data written before <see cref="TrainContent"/>
        /// existed uses: the category catalogue CSV, and a plan written by an earlier version. Neither
        /// one set makes it <see cref="TrainContent.None"/>, a service train.
        /// </summary>
        /// <param name="exchangesPassengers">Whether the train exchanges passengers.</param>
        /// <param name="exchangesCargo">Whether the train exchanges cargo wagons.</param>
        public static TrainContent From(bool exchangesPassengers, bool exchangesCargo) =>
            (exchangesPassengers ? TrainContent.Passenger : TrainContent.None) |
            (exchangesCargo ? TrainContent.Cargo : TrainContent.None);
    }
}
