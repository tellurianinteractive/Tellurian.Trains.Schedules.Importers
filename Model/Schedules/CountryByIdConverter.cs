using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Writes a <see cref="Country"/> as its <see cref="Country.Id"/> alone, and reads it back through
/// <c>Country.ById</c>.
/// </summary>
/// <remarks>
/// A country's name, languages and code belong to the catalogue in the code, not to a plan: a layout
/// only chooses which countries it uses. The id is a stable contract that never gets reused, so it is
/// all a plan needs. A country written as a whole object by an early version is made into its id by
/// the migration to schema version 1 (<see cref="PlanFiles.SchemaVersion1"/>).
/// </remarks>
internal sealed class CountryByIdConverter : JsonConverter<Country>
{
    public override Country? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.Number => Country.ById(reader.GetInt32()),
            _ => throw new JsonException($"Cannot read a {nameof(Country)} from {reader.TokenType}."),
        };

    public override void Write(Utf8JsonWriter writer, Country value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Id);
}
