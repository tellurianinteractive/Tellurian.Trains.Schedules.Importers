using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Tellurian.Trains.Schedules.Model.Schedules;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// A value is not written when it is the default of its type, so a property whose initial value is something else must
/// be written always (<c>[JsonIgnore(Condition = JsonIgnoreCondition.Never)]</c>), or setting it to the default would be
/// lost: the property would be left out and read back as its initial value.
/// </summary>
[TestClass]
public sealed class JsonDefaultValueTests
{
    private static readonly JsonSerializerOptions Options = PlanJson.CreateOptions();

    [TestMethod]
    public void EveryValueSetToTheDefaultOfItsTypeIsReadBack()
    {
        var lost = new List<string>();
        foreach (var typeInfo in TypesOfThePlan())
        {
            if (typeInfo.CreateObject is not { } create) continue;
            foreach (var property in typeInfo.Properties)
            {
                if (property.Get is null || property.Set is null || property.IsExtensionData) continue;
                var type = property.PropertyType;
                if (!type.IsValueType || Nullable.GetUnderlyingType(type) is not null) continue;
                var owner = create();
                var initial = property.Get(owner);
                var defaultValue = Activator.CreateInstance(type);
                if (Equals(initial, defaultValue)) continue;
                property.Set(owner, defaultValue);
                if (!Equals(property.Get(owner), defaultValue)) continue; // a setter that keeps its own value
                var read = JsonSerializer.Deserialize(JsonSerializer.Serialize(owner, typeInfo), typeInfo);
                if (!Equals(property.Get(read!), defaultValue)) lost.Add($"{typeInfo.Type.Name}.{property.Name} (initial {initial})");
            }
        }
        Assert.IsEmpty(lost, $"Set to the default of their type and read back as their initial value, so they need [JsonIgnore(Condition = JsonIgnoreCondition.Never)]:\n{string.Join("\n", lost)}");
    }

    /// <summary>Every object type written in a plan, found from <see cref="Plan"/> through properties, collections and derived types.</summary>
    private static IEnumerable<JsonTypeInfo> TypesOfThePlan()
    {
        var seen = new HashSet<Type>();
        var queue = new Queue<Type>([typeof(Plan)]);
        while (queue.TryDequeue(out var type))
        {
            if (!seen.Add(type)) continue;
            JsonTypeInfo typeInfo;
            try { typeInfo = Options.GetTypeInfo(type); }
            catch (NotSupportedException) { continue; }
            if (typeInfo.ElementType is { } element) queue.Enqueue(element);
            if (typeInfo.KeyType is { } key) queue.Enqueue(key);
            foreach (var derived in typeInfo.PolymorphismOptions?.DerivedTypes ?? []) queue.Enqueue(derived.DerivedType);
            if (typeInfo.Kind != JsonTypeInfoKind.Object) continue;
            foreach (var property in typeInfo.Properties) queue.Enqueue(Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            yield return typeInfo;
        }
    }
}
