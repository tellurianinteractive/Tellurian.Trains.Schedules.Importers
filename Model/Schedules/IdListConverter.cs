using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Writes a list of catalogue ids as a plain JSON array, instead of the object that
/// <see cref="ReferenceHandler.Preserve"/> wraps every collection in.
/// </summary>
/// <remarks>
/// Nothing ever refers to a list of ids, so the <c>$id</c> the wrapper gives it is never used and the
/// <c>$values</c> around it says nothing — pure weight wherever a station names a region. Reading
/// accepts the wrapped form as well, so a list written without this costs nothing to take back.
/// </remarks>
internal sealed class IdListConverter : JsonConverter<IList<int>>
{
    public override IList<int>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null) return null;
        if (reader.TokenType == JsonTokenType.StartObject && !AdvanceToValuesArray(ref reader)) return [];
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Cannot read a list of ids from {reader.TokenType}.");

        var ids = new List<int>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.Number) ids.Add(reader.GetInt32());
        }
        return ids;
    }

    // The wrapped form: { "$id": "23", "$values": [ … ] }. Leaves the reader on the array, or returns
    // false where the object holds none. TrySkip, not Skip, for the reason given in CountryByIdConverter.
    private static bool AdvanceToValuesArray(ref Utf8JsonReader reader)
    {
        var depth = reader.CurrentDepth;
        while (reader.Read() && !(reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == depth))
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            var isValues = reader.ValueTextEquals("$values");
            if (!reader.Read()) break;
            if (isValues) return true;
            reader.TrySkip();
        }
        return false;
    }

    public override void Write(Utf8JsonWriter writer, IList<int> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var id in value) writer.WriteNumberValue(id);
        writer.WriteEndArray();
    }
}
