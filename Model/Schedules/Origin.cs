namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Origin of freight wagons when it is not the current train part's from-station.
/// </summary>
public class Origin
{
    /// <summary>
    /// The operation location that is the origin. Any location exchanging cargo qualifies
    /// (see <see cref="OperationLocation.HasCargoExchange"/>), not only a <see cref="Layouts.Station"/> —
    /// an <see cref="IndustrialArea"/> is an origin of freight wagons as much as a station is.
    /// </summary>
    public required OperationLocation Location { get; set; }
}
