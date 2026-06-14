namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Common properties for all train part options.
/// </summary>
public abstract class TrainPartOptions
{
    /// <summary>
    /// If true, should present a note telling staff that the unit should be copuled to the train.
    /// </summary>
    public bool HasCoupleNote { get; set; }

    /// <summary>
    /// If true, should present a note telling staff that the unit should be uncopuled from the train.
    /// </summary>
    public bool HasUncoupleNote { get; set; }

}
