namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// DetailedActuall info about specific wagons in a train.
/// </summary>
/// <param name="OrderInTrain">The position in train in forward direction. Order should be reversed in backwards direction.</param>
/// <param name="Class">Type of wagon according standard clas definitions.</param>
public record Wagon(int OrderInTrain, string Class)
{
    /// <summary>
    /// Optional indivdual number of wagon.
    /// </summary>
    public string? Number { get; set; }
    /// <summary>
    /// If true this is a passenger wagon.
    /// </summary>
    /// <remarks>A wagon can be both for passenger and cargo</remarks>
    public bool IsPassenger { get; set; }
    /// <summary>
    /// If true this is a freight wagon.
    /// </summary>
    /// <remarks>A wagon can be both for passenger and cargo</remarks>
    public bool IsCargo { get; set; }
}
