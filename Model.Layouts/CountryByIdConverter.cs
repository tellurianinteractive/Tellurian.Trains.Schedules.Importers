using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model;

/// <summary>
/// Writes a <see cref="Country"/> as its <see cref="Country.Id"/> alone, and reads it back through
/// <c>Country.ById</c>.
/// </summary>
/// <remarks>
/// A country's name, languages and code belong to the catalogue in the code, not to a file: a layout
/// only chooses which countries it uses. The id is a stable contract that never gets reused, so it is
/// all a file needs, in a plan and in a layout block alike. A country written as a whole object by an
/// early version of the planner is made into its id by the plan file's migration to schema version 1.
/// </remarks>
public sealed class CountryByIdConverter : JsonConverter<Country>
{
    /// <inheritdoc/>
    public override Country? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.Number => Country.ById(reader.GetInt32()),
            _ => throw new JsonException($"Cannot read a {nameof(Country)} from {reader.TokenType}."),
        };

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Country value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Id);
}
