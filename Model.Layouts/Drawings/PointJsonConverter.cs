using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Layouts.Drawings;

/// <summary>Writes a <see cref="Point"/> as <c>{ "x": 0, "y": 0 }</c>, leaving out its computed properties.</summary>
public sealed class PointJsonConverter : JsonConverter<Point>
{
    /// <inheritdoc/>
    public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("A point must be an object.");
        double? x = null, y = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected a property name in a point.");
            var name = reader.GetString();
            reader.Read();
            switch (name)
            {
                case PoseJsonConverter.X: x = reader.GetDouble(); break;
                case PoseJsonConverter.Y: y = reader.GetDouble(); break;
                default: reader.Skip(); break;
            }
        }
        if (x is null || y is null) throw new JsonException($"A point needs {PoseJsonConverter.X} and {PoseJsonConverter.Y}.");
        return new(x.Value, y.Value);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(PoseJsonConverter.X, value.X);
        writer.WriteNumber(PoseJsonConverter.Y, value.Y);
        writer.WriteEndObject();
    }
}
