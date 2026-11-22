using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Reads a plan of schema version 0 through the model, the way the app did before version 1: with
/// <see cref="ReferenceHandler.Preserve"/>. It is the yardstick for the migrations: a plan migrated and read must hold
/// the same as the plan read the old way. The names that the shared model changed are read under their version 0
/// names: a location's <c>$type</c>, a track's <c>Number</c> and a location's <c>CountryId</c>. It is removed when the
/// model no longer has what version 0 holds.
/// </summary>
internal static class PlanVersion0Reader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new CountryById() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Version0Names } },
    };

    /// <summary>The plan in <paramref name="json"/>, read as version 0 was, platforms given on reading included.</summary>
    public static Plan Read(string json)
    {
        var plan = JsonSerializer.Deserialize<Plan>(json, Options) ?? throw new InvalidOperationException("No plan.");
        plan.Layout.EnsurePlatforms();
        return plan;
    }

    /// <summary>
    /// Makes a type's metadata read and write what version 0 had: the file-only id properties of later versions, which are
    /// internal, are taken out, so that their names don't clash with what version 0 wrote; and every public property that
    /// the model now marks <c>[JsonIgnore]</c> and that can be set is put back, since version 0 wrote them all: parents,
    /// and the objects that later versions write by their id. For the readers and writers of version 0 in the tests.
    /// </summary>
    internal static void Version0References(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
        // Version 0 refers by $ref, which can't be given to a constructor's parameters: a type that later versions read
        // through a constructor taking an id is created with its public parameterless one instead.
        if (typeInfo.Type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Any(c => c.GetParameters().Length > 0 && c.GetCustomAttribute<JsonConstructorAttribute>() is not null) &&
            typeInfo.Type.GetConstructor(Type.EmptyTypes) is { } parameterless)
            typeInfo.CreateObject = () => parameterless.Invoke(null);
        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
            if (typeInfo.Properties[i].AttributeProvider is PropertyInfo { GetMethod.IsPublic: false }) typeInfo.Properties.RemoveAt(i);
        foreach (var member in typeInfo.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (member.GetIndexParameters().Length > 0 || member.SetMethod is not { IsPublic: true } || member.GetMethod is not { IsPublic: true }) continue;
            if (member.GetCustomAttribute<JsonIgnoreAttribute>(inherit: true) is not { Condition: JsonIgnoreCondition.Always }) continue;
            // A property the model marks [JsonIgnore] is still listed, ignored; it is replaced.
            if (typeInfo.Properties.FirstOrDefault(p => p.Name == member.Name) is { } ignored) typeInfo.Properties.Remove(ignored);
            var property = typeInfo.CreateJsonPropertyInfo(member.PropertyType, member.Name);
            property.Get = member.GetValue;
            property.Set = member.SetValue;
            typeInfo.Properties.Add(property);
        }
    }

    private static void Version0Names(JsonTypeInfo typeInfo)
    {
        Version0References(typeInfo);
        if (typeInfo.Type == typeof(OperationLocation) && typeInfo.PolymorphismOptions is { } polymorphism)
            polymorphism.TypeDiscriminatorPropertyName = "$type";
        if (typeInfo.Type == typeof(StationTrack) && typeInfo.Properties.FirstOrDefault(p => p.Name == nameof(StationTrack.Designation)) is { } designation)
            designation.Name = "Number";
        if (typeof(OperationLocation).IsAssignableFrom(typeInfo.Type) && typeInfo.Kind == JsonTypeInfoKind.Object)
        {
            var countryId = typeInfo.CreateJsonPropertyInfo(typeof(int?), "CountryId");
            countryId.Set = (location, id) => ((OperationLocation)location).CountryId = (int?)id;
            typeInfo.Properties.Add(countryId);
        }
    }

    private sealed class CountryById : JsonConverter<Country>
    {
        public override Country? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType == JsonTokenType.Number ? Country.ById(reader.GetInt32()) : throw new JsonException("A country is written as its id.");

        public override void Write(Utf8JsonWriter writer, Country value, JsonSerializerOptions options) => writer.WriteNumberValue(value.Id);
    }
}
