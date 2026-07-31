using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// The JSON settings for reading and writing a whole <see cref="Plan"/>, shared by every persistence
/// path — browser storage, file import and file export — so a plan written by one is always readable
/// by the others.
/// </summary>
/// <remarks>
/// The object graph contains reference cycles, so <see cref="ReferenceHandler.Preserve"/> and a deep
/// <see cref="JsonSerializerOptions.MaxDepth"/> are both required to round-trip it. Reading also
/// accepts property names used by earlier versions, see <see cref="AcceptLegacyNames"/>.
/// </remarks>
public static class PlanJson
{
    /// <summary>
    /// Creates the options for reading and writing a <see cref="Plan"/>.
    /// </summary>
    /// <param name="writeIndented">Whether to write indented JSON, for a plan saved to a file a person may read.</param>
    public static JsonSerializerOptions CreateOptions(bool writeIndented = false) => new()
    {
        WriteIndented = writeIndented,
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { AcceptLegacyNames } },
    };

    /// <summary>
    /// Lets a plan written by an earlier version still be read, by accepting the property names that
    /// version wrote. Each legacy name is read-only — it has no getter, so it is never written back,
    /// and a plan saved after being read once carries only the current names.
    /// </summary>
    /// <remarks>
    /// <see cref="Origin"/> and <see cref="Destination"/> named their location <c>Station</c> while only
    /// a station could be one. Both now take any cargo-exchanging <see cref="OperationLocation"/> and
    /// call it <c>Location</c>. The current name is also relaxed from required to optional, because a
    /// plan holding the legacy name has no <c>Location</c> property at all and would otherwise be
    /// rejected before this alias could fill it in.
    /// </remarks>
    private static void AcceptLegacyNames(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type != typeof(Origin) && typeInfo.Type != typeof(Destination)) return;
        if (typeInfo.Properties.FirstOrDefault(p => p.Name == nameof(Destination.Location)) is not { Set: { } setLocation } location) return;

        location.IsRequired = false;
        var legacy = typeInfo.CreateJsonPropertyInfo(typeof(OperationLocation), "Station");
        legacy.Set = setLocation;
        typeInfo.Properties.Add(legacy);
    }
}
