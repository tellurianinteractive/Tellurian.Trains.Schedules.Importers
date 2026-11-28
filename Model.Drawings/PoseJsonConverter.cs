using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Model.Geometry;

namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>Writes a <see cref="Pose"/> flat, as <c>{ "x": 0, "y": 0, "heading": 180 }</c> with the heading in degrees.</summary>
public sealed class PoseJsonConverter : JsonConverter<Pose>
{
    /// <summary>The JSON property name of the X coordinate.</summary>
    public const string X = "x";
    /// <summary>The JSON property name of the Y coordinate.</summary>
    public const string Y = "y";
    /// <summary>The JSON property name of the heading in degrees.</summary>
    public const string Heading = "heading";

    /// <inheritdoc/>
    public override Pose Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("A pose must be an object.");
        double? x = null, y = null, heading = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Expected a property name in a pose.");
            var name = reader.GetString();
            reader.Read();
            switch (name)
            {
                case X: x = reader.GetDouble(); break;
                case Y: y = reader.GetDouble(); break;
                case Heading: heading = reader.GetDouble(); break;
                default: reader.Skip(); break;
            }
        }
        if (x is null || y is null || heading is null) throw new JsonException($"A pose needs {X}, {Y} and {Heading}.");
        return new(new(x.Value, y.Value), new(heading.Value));
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Pose value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber(X, value.Position.X);
        writer.WriteNumber(Y, value.Position.Y);
        writer.WriteNumber(Heading, value.Heading.Degrees);
        writer.WriteEndObject();
    }
}
