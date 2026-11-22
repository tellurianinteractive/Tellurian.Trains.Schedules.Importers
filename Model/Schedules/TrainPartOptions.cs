using System.Text.Json.Serialization;
namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Common properties for all train part options.
/// </summary>
public abstract class TrainPartOptions
{
    /// <summary>
    /// If true, should present a note telling staff that the unit should be copuled to the train.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool HasCoupleNote { get; set; } = true;

    /// <summary>
    /// If true, should present a note telling staff that the unit should be uncopuled from the train.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public bool HasUncoupleNote { get; set; } = true;

}
