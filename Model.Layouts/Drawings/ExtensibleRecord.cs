using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts.Drawings;

/// <summary>
/// The base of every record that is written to a drawing file. It keeps the JSON properties this version of
/// the library doesn't know, and writes them back, so that a file from a newer version of the app, or with an
/// addition from another system, isn't damaged by a save (spec §6).
/// </summary>
public abstract record ExtensibleRecord
{
    /// <summary>
    /// The properties of the JSON object that this library doesn't know, as read. They are written last.
    /// They don't take part in equality: two records are equal when the properties the library knows are.
    /// </summary>
    [JsonExtensionData]
    [JsonPropertyOrder(int.MaxValue)]
    public Dictionary<string, JsonElement>? ExtensionData { get; init; }

    /// <inheritdoc/>
    public virtual bool Equals(ExtensibleRecord? other) => other is not null && EqualityContract == other.EqualityContract;

    /// <inheritdoc/>
    public override int GetHashCode() => EqualityContract.GetHashCode();
}
