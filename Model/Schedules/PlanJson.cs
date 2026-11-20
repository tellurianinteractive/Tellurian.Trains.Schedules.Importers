using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Schedules;

/// <summary>
/// The JSON settings of the content of a plan file of the current schema version. Read and write a
/// whole plan through <see cref="PlanFiles.PlanFile"/>, which adds the envelope and brings older plans up to date.
/// </summary>
/// <remarks>
/// Property names are camelCase and enum values are written by their name in the code
/// (<c>Documentation/OperationLocations.md</c>, decision 31). A plan is a tree: what a thing owns is nested in it,
/// everything else is referred to by id, and no parent is written (<see cref="PlanReferences"/>). Nothing is written
/// that is null, nor a list of ids that is empty, nor a value that is the default of its property
/// (<see cref="LeaveDefaultValuesOut"/>).
/// </remarks>
public static class PlanJson
{
    /// <summary>
    /// Creates the options for reading and writing the content of a plan file.
    /// </summary>
    /// <param name="writeIndented">Whether to write indented JSON, for a plan saved to a file a person may read.</param>
    public static JsonSerializerOptions CreateOptions(bool writeIndented = false) => new()
    {
        WriteIndented = writeIndented,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // The kind of a location is its type discriminator; read it wherever it stands in the object.
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter(), new CountryByIdConverter() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                // The layout's types as in a hall plan's layout block, then the plan's own.
                Layout.ModifyJsonTypeInfo,
                PlanReferences.Shape,
                WriteIdListsOnlyWhereTheyHaveSome,
                // Last, so it adds to what the others decided.
                LeaveDefaultValuesOut,
            },
        },
    };

    /// <summary>
    /// Leaves an empty list of ids out altogether: a station's <see cref="Layouts.Station.RegionIds"/>
    /// where it stands for nowhere off the layout, which is every station but the few shadow yards, and a
    /// train category's <see cref="Timetables.TrainCategory.StopLocationIds"/> where it has no stop
    /// pattern. An empty list says exactly what its absence does.
    /// </summary>
    private static void WriteIdListsOnlyWhereTheyHaveSome(JsonTypeInfo typeInfo)
    {
        if (!IdLists.TryGetValue(typeInfo.Type, out var name)) return;
        if (typeInfo.Properties.FirstOrDefault(p => p.AttributeProvider is System.Reflection.MemberInfo m && m.Name == name) is not { } property) return;
        property.ShouldSerialize = static (_, value) => value is IList<int> { Count: > 0 };
    }

    /// <summary>
    /// Leaves out a value that is what a new object of the same type has (schema version 4): a flag that is off, a text
    /// that is empty, a number that is its initial value, and a list that is empty where a new object has an empty one.
    /// Reading creates the object the same way, so the value comes back as it was.
    /// </summary>
    /// <remarks>
    /// The default is taken from an object created as reading creates it, not from <c>default(T)</c>, so a flag that is
    /// true from the start is left out when true and written when false. A type read through a constructor with
    /// parameters is left as it is, since a missing parameter would be given <c>default(T)</c>. Changing the initial
    /// value of a property changes what a plan without it means, and so takes a migration of the plan file.
    /// </remarks>
    private static void LeaveDefaultValuesOut(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object || typeInfo.CreateObject is not { } create) return;
        object prototype;
        try { prototype = create(); }
        catch { return; }
        foreach (var property in typeInfo.Properties)
        {
            if (property.IsExtensionData || property.Get is not { } get || property.AttributeProvider is not System.Reflection.MemberInfo) continue;
            object? initial;
            try { initial = get(prototype); }
            catch { continue; }
            Func<object?, bool>? isDefault = initial switch
            {
                System.Collections.ICollection { Count: 0 } => static value => value is System.Collections.ICollection { Count: 0 },
                _ when IsSimple(property.PropertyType) => value => Equals(value, initial),
                _ => null,
            };
            if (isDefault is null) continue;
            var shouldSerialize = property.ShouldSerialize;
            property.ShouldSerialize = (owner, value) => !isDefault(value) && (shouldSerialize is null || shouldSerialize(owner, value));
        }
    }

    // A value compared by Equals: what a JSON value is read into, and not an object of the model.
    private static bool IsSimple(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) || type == typeof(TimeSpan) ||
            type == typeof(TimeOnly) || type == typeof(DateOnly) || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid);
    }

    // The lists of ids left out where they are empty, one per type holding one.
    private static readonly Dictionary<Type, string> IdLists = new()
    {
        [typeof(Layouts.Station)] = nameof(Layouts.Station.RegionIds),
        [typeof(Timetables.TrainCategory)] = nameof(Timetables.TrainCategory.StopLocationIds),
    };
}
