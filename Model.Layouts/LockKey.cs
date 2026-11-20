using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The key a train needs to unlock the switches of a location that nobody works, and the manned station where it is
/// held. A train collects it on the way, so the location must come after the holder in the train's run.
/// </summary>
public sealed class LockKey
{
    /// <summary>What the key is called, e.g. "Key A".</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The manned station where the key is held. Not written by the contract: it is referred to by id where a layout is written.</summary>
    [JsonIgnore]
    public Station HeldAt { get; set; } = default!;

    /// <summary>The properties of the JSON object that this version doesn't know, as read. They are written last.</summary>
    [JsonExtensionData]
    [JsonPropertyOrder(int.MaxValue)]
    public Dictionary<string, System.Text.Json.JsonElement>? ExtensionData { get; set; }

}
