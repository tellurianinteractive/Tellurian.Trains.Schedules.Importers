using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Timetables;

/// <summary>
/// Represents a group of wagons within a train, including its position and the stations where it runs.
/// </summary>
/// <remarks>
/// A WagonGroup defines a logical grouping of wagons in a train, identified by its position and related
/// station calls. This class is typically used in train composition scenarios to track wagon groups and their movement
/// between stations. The navigation properties to Train and StationCall entities are intended for use with Entity
/// Framework Core and are ignored during JSON serialization.
/// </remarks>
[method: JsonConstructor]
public class WagonGroup()
{
    /// <summary>
    /// Gets or sets the unique identifier for this wagon group.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets an optional remark about this wagon group.
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// Gets or sets the position of this wagon group within the train (1 = front).
    /// </summary>
    public int PositionInTrain { get; set; }

    /// <summary>
    /// Gets or sets the foreign key to the train.
    /// </summary>
    public int TrainId { get; set; }

    /// <summary>
    /// Gets or sets the train this wagon group belongs to.
    /// </summary>
    [JsonIgnore]
    public Train Train { get; set; } = default!;

    /// <summary>
    /// Gets or sets the foreign key to the origin station call.
    /// </summary>
    public int FromStationCallId { get; set; }

    /// <summary>
    /// Gets or sets the station call where this wagon group is attached.
    /// </summary>
    [JsonIgnore]
    public StationCall FromStationCall { get; set; } = default!;

    /// <summary>
    /// Gets or sets the foreign key to the destination station call.
    /// </summary>
    public int ToStationCallId { get; set; }

    /// <summary>
    /// Gets or sets the station call where this wagon group is detached.
    /// </summary>
    [JsonIgnore]
    public StationCall ToStationCall { get; set; } = default!;
}
