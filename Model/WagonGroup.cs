using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

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
