namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Options tha applies to no-traction rolling stock.
/// </summary>
/// <remarks>
/// The individual wagons of a wagonset live on <see cref="ScheduledObject.Units"/> (the rake shared across
/// the whole schedule), not on the per-train-part options.
/// </remarks>
public sealed class WagonSetOptions : TrainPartOptions
{
    /// <summary>
    /// The wagon groups overall order in the train.
    /// </summary>
    public int OrderInTrain { get; set; }
}
