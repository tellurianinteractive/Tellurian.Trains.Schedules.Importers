using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Brings the JSON of operation locations written by earlier versions into the shape the closed hierarchy reads: the
/// type discriminator <c>"kind"</c> first, with its value as in the code. For readers of older files, before they
/// deserialize them.
/// </summary>
public static class OperationLocationJson
{
    /// <summary>The name of the type discriminator of an <see cref="OperationLocation"/>.</summary>
    public const string KindProperty = "kind";

    private static readonly string[] LocationProperties = ["operationLocation", "operationLocations"];

    /// <summary>
    /// Every object under a property named <c>operationLocation</c>, or in a list named <c>operationLocations</c>,
    /// anywhere in <paramref name="node"/>, gets its <c>"kind"</c> as the first property, with the name of the kind as
    /// in the code (<c>"station"</c> becomes <c>"Station"</c>), and <c>"Station"</c> where it has none: a version before
    /// the kinds had only stations. Changes <paramref name="node"/> in place.
    /// </summary>
    public static void Normalise(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject value:
                foreach (var (name, child) in value.ToList())
                {
                    if (LocationProperties.Contains(name, StringComparer.Ordinal))
                    {
                        if (child is JsonObject location) value[name] = Normalised(location);
                        else if (child is JsonArray locations)
                            for (var i = 0; i < locations.Count; i++)
                                if (locations[i] is JsonObject item) locations[i] = Normalised(item);
                    }
                    Normalise(value[name]);
                }
                break;
            case JsonArray list:
                foreach (var child in list) Normalise(child);
                break;
        }
    }

    private static JsonObject Normalised(JsonObject location)
    {
        var kind = location[KindProperty] is JsonValue value && value.GetValueKind() == JsonValueKind.String
            && Enum.TryParse<OperationLocationKind>(value.GetValue<string>(), ignoreCase: true, out var parsed)
                ? parsed
                : OperationLocationKind.Station;
        var result = new JsonObject { [KindProperty] = kind.ToString() };
        foreach (var (name, child) in location.ToList())
        {
            if (name == KindProperty) continue;
            location.Remove(name);
            result[name] = child;
        }
        return result;
    }
}
