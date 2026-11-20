using System.Text.Json.Serialization;
namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Origin of freight wagons when it is not the current train part's from-station.
/// </summary>
public class Origin
{
    /// <summary>Creates an empty origin; <see cref="Location"/> must be set.</summary>
    public Origin() { }

    /// <summary>For JSON: the location is read as its id and found when the plan's references are restored.</summary>
    [JsonConstructor]
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    internal Origin(int? locationId)
    {
        Location = default!;
        LocationId = locationId;
    }

    /// <summary>
    /// The operation location that is the origin. Any location exchanging cargo qualifies
    /// (see <see cref="OperationLocation.HasCargoExchange"/>), not only a <see cref="Layouts.Station"/> —
    /// an <see cref="IndustrialArea"/> is an origin of freight wagons as much as a station is.
    /// </summary>
    [JsonIgnore]
    public required OperationLocation Location
    {
        get;
        set { field = value; _locationId = null; }
    }

    /// <summary>The id of <see cref="Location"/>, the form it is written in; the id read until the location is found.</summary>
    [JsonInclude]
    internal int? LocationId
    {
        get => Location?.Id ?? _locationId;
        set => _locationId = value;
    }
    private int? _locationId;
}
