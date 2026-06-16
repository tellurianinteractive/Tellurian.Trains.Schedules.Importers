namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Origin of freight wagons when it is not the current train part's from-station.
/// </summary>
public class Origin
{
    /// <summary>
    /// Station that is origin.
    /// </summary>
    public required Station Station { get; set; }
}
