namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Which of the two movements a shunting task's cargo flow describes. A shunting task is worked at one
/// station (see <see cref="Timetables.TrainCategory.IsShunting"/>), and the wagons it handles are either
/// on their way out of the station's sidings or on their way into them.
/// </summary>
/// <remarks>
/// Read from the flow rather than stored on it: where the wagons go is already stated by the
/// <see cref="CargoFlowOptions"/> the flow references, and a second field saying the same thing could
/// contradict it. See <c>CargoFlowTrainPartExtensions.ShuntingWork</c> for how it is derived.
/// </remarks>
public enum ShuntingWork
{
    /// <summary>
    /// The wagons have arrived at this station and are taken out to its cargo customers. The flow says
    /// where they came from — its <see cref="CargoFlowOptions.Origins"/>.
    /// </summary>
    ToCargoCustomers,

    /// <summary>
    /// The wagons are fetched from this station's cargo customers and made ready for a departing train.
    /// The flow says where they are going — its <see cref="CargoFlowOptions.Destinations"/>.
    /// </summary>
    FromCargoCustomers,
}
