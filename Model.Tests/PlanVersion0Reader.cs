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
    /// Adds back what version 0 wrote as objects and the shared model doesn't write: the stations a location refers to,
    /// the station a lock key is held at, and a station's regions. For the readers and writers of version 0 in the tests.
    /// </summary>
    internal static void Version0References(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
        if (typeof(OperationLocation).IsAssignableFrom(typeInfo.Type))
        {
            Add<OperationLocation, Station?>(typeInfo, nameof(OperationLocation.CargoServedFrom), l => l.CargoServedFrom, (l, s) => l.CargoServedFrom = s);
            Add<OperationLocation, Station?>(typeInfo, nameof(OperationLocation.ControlledBy), l => l.ControlledBy, (l, s) => l.ControlledBy = s);
        }
        if (typeInfo.Type == typeof(Station))
            Add<Station, IList<Region>>(typeInfo, nameof(Station.Regions), s => s.Regions, (s, r) => s.Regions = r ?? []);
        if (typeInfo.Type == typeof(LockKey))
            Add<LockKey, Station>(typeInfo, nameof(LockKey.HeldAt), k => k.HeldAt, (k, s) => k.HeldAt = s);
    }

    private static void Add<TOwner, TValue>(JsonTypeInfo typeInfo, string name, Func<TOwner, TValue> get, Action<TOwner, TValue> set)
    {
        // A property the model marks [JsonIgnore] is still listed, ignored; it is replaced.
        if (typeInfo.Properties.FirstOrDefault(p => p.Name == name) is { } ignored) typeInfo.Properties.Remove(ignored);
        var property = typeInfo.CreateJsonPropertyInfo(typeof(TValue), name);
        property.Get = owner => get((TOwner)owner);
        property.Set = (owner, value) => set((TOwner)owner, (TValue)value!);
        typeInfo.Properties.Add(property);
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
