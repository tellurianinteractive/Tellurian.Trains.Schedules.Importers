using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// Writes a <see cref="Country"/> as its <see cref="Country.Id"/> alone, and reads it back through
/// <c>Country.ById</c>.
/// </summary>
/// <remarks>
/// A country's name, languages and code belong to the catalogue in the code, not to a plan: a layout
/// only chooses which countries it uses. Storing the whole country copied that catalogue into every
/// plan, where it could not be corrected — a country whose languages are put right in a later version
/// would keep the old ones in every plan already saved. The id is a stable contract that never gets
/// reused, so it is all a plan needs.
/// <para>
/// Reading also accepts the object an earlier version wrote, taking its <c>Id</c> and looking the
/// country up. A country the catalogue no longer offers is read from the stored values themselves, so
/// no layout silently loses one; ids are only ever appended, so this is not expected to happen.
/// </para>
/// </remarks>
internal sealed class CountryByIdConverter : JsonConverter<Country>
{
    public override Country? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.Number => Country.ById(reader.GetInt32()),
            JsonTokenType.StartObject => ReadObject(ref reader),
            _ => throw new JsonException($"Cannot read a {nameof(Country)} from {reader.TokenType}."),
        };

    // The shape written before the id alone was enough: the whole country, with the $id that
    // ReferenceHandler.Preserve gave it. Nothing ever referred to a country, so there is no $ref case.
    private static Country? ReadObject(ref Utf8JsonReader reader)
    {
        int? id = null;
        string resourceKey = "", languages = "", countryCode = "";
        var depth = reader.CurrentDepth;
        while (reader.Read() && !(reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == depth))
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            var name = reader.GetString();
            if (!reader.Read()) break;
            switch (name)
            {
                case nameof(Country.Id): id = reader.GetInt32(); break;
                case nameof(Country.ResourceKey): resourceKey = reader.GetString() ?? ""; break;
                case nameof(Country.Languages): languages = reader.GetString() ?? ""; break;
                case nameof(Country.CountryCode): countryCode = reader.GetString() ?? ""; break;
                // TrySkip, not Skip: a plan read from a stream arrives in blocks, and Skip refuses to
                // work on a reader that has not yet seen the end of the document. The whole object is
                // in the buffer before a value converter is called, so this always succeeds.
                default: reader.TrySkip(); break;
            }
        }
        if (id is not int countryId) return null;
        return Country.ById(countryId) ?? new Country(countryId, resourceKey, languages, countryCode);
    }

    public override void Write(Utf8JsonWriter writer, Country value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Id);
}
