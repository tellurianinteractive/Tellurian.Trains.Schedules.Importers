using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Represents a group of wagons within a train, including its position and the station where it runs.
/// </summary>
/// <remarks>A WagonGroup defines a logical grouping of wagons in a train, identified by its position and related
/// station calls. This class is typically used in train composition scenarios to track wagon groups and their movement
/// between stations. The navigation properties to Train and StationCall entities are intended for use with Entity
/// Framework Core and are ignored during JSON serialization.</remarks>
[method: JsonConstructor]
public class WagonGroup()
{
    public int Id { get; set; }
    public string? Remark { get; set; }
    public int PositionInTrain { get; set; }

    // FK property for EF Core
    public int TrainId { get; set; }
    [JsonIgnore]
    public Train Train { get; set; } = default!;

    public int FromStationCallId { get; set; }
    [JsonIgnore]
    public StationCall FromStationCall { get; set; } = default!;
    public int ToStationCallId { get; set; }
    [JsonIgnore]
    public StationCall ToStationCall { get; set; } = default!;
}
