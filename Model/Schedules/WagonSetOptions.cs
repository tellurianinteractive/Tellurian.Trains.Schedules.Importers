namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Options tha applies to no-traction rolling stock.
/// </summary>
public sealed class WagonSetOptions : TrainPartOptions
{

    /// <summary>
    /// Optional specification of wagons in train part.
    /// </summary>
    public ICollection<Wagon> WagonGroup { get; set; } = [];
    /// <summary>
    /// The wagon groups overall order in the train.
    /// </summary>
    public int OrderInTrain { get; set; }
}

/// <summary>
///
/// </summary>
public static class NonTractionOptionsExtensions
{
    extension(WagonSetOptions options)
    {
        /// <summary>
        /// Adds a passenger wagon to the wagon group
        /// </summary>
        /// <param name="orderInTrain"></param>
        /// <param name="class"></param>
        /// <param name="number"></param>
        public void AddPassengerWagon(int orderInTrain, string @class, string? number = null) =>
            options.WagonGroup.Add(new Wagon(orderInTrain, @class) { Number = number, IsPassenger = true });

        /// <summary>
        /// Adds a freight wagon to the wagon group
        /// </summary>
        /// <param name="orderInTrain"></param>
        /// <param name="class"></param>
        /// <param name="number"></param>
        public void AddFreightWagon(int orderInTrain, string @class, string? number = null) =>
            options.WagonGroup.Add(new Wagon(orderInTrain, @class) { Number = number, IsCargo = true });
    }
}
