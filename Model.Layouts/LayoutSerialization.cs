using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// How a <see cref="Layout"/> is written as JSON, in the layout block of a hall plan and inside a plan file, as members of
/// <see cref="Layout"/>: <c>Layout.JsonSerializerOptions</c>, <c>Layout.ModifyJsonTypeInfo</c> and
/// <c>layout.RestoreReferences()</c>. A layout is a tree: what a thing owns is nested in it, everything else is referred to
/// by its id, and no parent is written. Each location is written once, in <see cref="Layout.OperationLocations"/>; a
/// stretch refers to its ends by <c>startId</c> and <c>endId</c>, a location to the station that controls it by
/// <c>controlledById</c>, and a layout module to its location by <c>operationLocationId</c>. After reading,
/// <c>RestoreReferences</c> puts the objects back.
/// </summary>
public static class LayoutSerialization
{
    private static readonly ConditionalWeakTable<object, Dictionary<string, object?>> Pending = new();

    /// <summary>Properties that are not written: parents, the ids of parents, and objects written by their id instead.</summary>
    private static readonly (Type Owner, string[] Names)[] NotWritten =
    [
        (typeof(Company), [nameof(Company.Layout), nameof(Company.LayoutId)]),
        (typeof(OperationLocation), [nameof(OperationLocation.Layout), nameof(OperationLocation.LayoutId), nameof(OperationLocation.CargoServedFrom), nameof(OperationLocation.ControlledBy)]),
        (typeof(Station), [nameof(Station.Regions)]),
        (typeof(StationTrack), [nameof(StationTrack.Station), nameof(StationTrack.StationId)]),
        (typeof(LockKey), [nameof(LockKey.HeldAt)]),
        (typeof(TrackStretch), [nameof(TrackStretch.Layout), nameof(TrackStretch.LayoutId), nameof(TrackStretch.Start), nameof(TrackStretch.End)]),
        (typeof(TimetableStretch), [nameof(TimetableStretch.Stretches)]),
        (typeof(DispatchStretch), [nameof(DispatchStretch.From), nameof(DispatchStretch.To), nameof(DispatchStretch.Stretches)]),
        (typeof(Module), [nameof(Module.OperationLocation)]),
    ];

    /// <summary>Properties that exist only in JSON: the id of what the model refers to without keeping its id.</summary>
    private static readonly FileProperty[] FileProperties =
    [
        Single<OperationLocation>("cargoServedFromId", l => l.CargoServedFrom),
        Single<OperationLocation>("controlledById", l => l.ControlledBy),
        Single<LockKey>("heldAtId", k => k.HeldAt),
        Single<DispatchStretch>("fromId", s => s.From),
        Single<DispatchStretch>("toId", s => s.To),
        List<DispatchStretch>("stretchIds", s => s.Stretches.Select(t => t.Id)),
        List<TimetableStretch>("stretchIds", s => s.Stretches.Select(t => t.Id)),
        Single<Module>("operationLocationId", m => m.OperationLocation),
    ];

    private static readonly System.Text.Json.JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter() },
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { Modify } },
    };

    extension(Layout)
    {
        /// <summary>
        /// The options a layout is read and written with on its own, as in the layout block of a hall plan: camelCase
        /// names, enum values by their name in the code, nothing null written, and the kind of a location read wherever
        /// it stands. Read-only once used; for indented JSON, copy them with <c>WriteIndented</c> set.
        /// </summary>
        public static System.Text.Json.JsonSerializerOptions JsonSerializerOptions => Options;

        /// <summary>
        /// The modifier that gives the types of a layout their shape in JSON: what is not written is removed, and the ids
        /// written in place of objects are added. A plan file's options use it too, beside their own.
        /// </summary>
        public static void ModifyJsonTypeInfo(JsonTypeInfo typeInfo) => Modify(typeInfo);
    }

    extension(Layout layout)
    {
        /// <summary>
        /// Resolves the ids of a layout that has been read into the objects they stand for, and sets the parents of what
        /// it owns: a location's layout, a track's location, a stretch's ends, the station that controls a location, the
        /// stretches of a timetable or dispatch stretch, and the location of a layout module, whose tracks are then the
        /// location's own list.
        /// </summary>
        public void RestoreReferences() => Restore(layout);
    }

    private static void Modify(JsonTypeInfo typeInfo)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        if (typeInfo.Kind != JsonTypeInfoKind.Object) return;
        var notWritten = NotWritten.Where(n => n.Owner.IsAssignableFrom(typeInfo.Type)).SelectMany(n => n.Names).ToHashSet(StringComparer.Ordinal);
        for (var i = typeInfo.Properties.Count - 1; i >= 0; i--)
        {
            var property = typeInfo.Properties[i];
            if (property.AttributeProvider is System.Reflection.MemberInfo member && notWritten.Contains(member.Name)) typeInfo.Properties.RemoveAt(i);
        }
        if (typeof(Module).IsAssignableFrom(typeInfo.Type) && typeInfo.Properties.FirstOrDefault(p => p.AttributeProvider is System.Reflection.MemberInfo { Name: nameof(Module.Tracks) }) is { } tracks)
            // A module that is a location has the location's tracks; they are written with the location.
            tracks.ShouldSerialize = static (module, _) => ((Module)module).OperationLocation is null;
        foreach (var fileProperty in FileProperties.Where(p => p.Owner.IsAssignableFrom(typeInfo.Type)))
        {
            if (typeInfo.Properties.Any(p => p.Name == fileProperty.Name)) continue;
            var property = typeInfo.CreateJsonPropertyInfo(fileProperty.Type, fileProperty.Name);
            property.Get = fileProperty.Get;
            property.Set = (target, value) => Pending.GetOrCreateValue(target)[fileProperty.Name] = value;
            property.ShouldSerialize = static (_, value) => value is not null;
            typeInfo.Properties.Add(property);
        }
    }

    private static void Restore(Layout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        var locations = layout.OperationLocations.GroupBy(l => l.Id).ToDictionary(g => g.Key, g => g.First());
        var stretches = layout.TrackStretches.GroupBy(s => s.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var company in layout.Companies)
        {
            company.Layout = layout;
            company.LayoutId = layout.Id;
        }
        foreach (var location in layout.OperationLocations)
        {
            location.Layout = layout;
            location.LayoutId = layout.Id;
            foreach (var track in location.Tracks)
            {
                track.Station = location;
                track.StationId = location.Id;
            }
            if (Take(location, "cargoServedFromId") is int servedFrom) location.CargoServedFrom = locations.GetValueOrDefault(servedFrom) as Station;
            if (Take(location, "controlledById") is int controlledBy) location.ControlledBy = locations.GetValueOrDefault(controlledBy) as Station;
            if (location.LockKey is { } key && Take(key, "heldAtId") is int heldAt && locations.GetValueOrDefault(heldAt) is Station holder) key.HeldAt = holder;
        }
        foreach (var stretch in layout.TrackStretches)
        {
            stretch.Layout = layout;
            stretch.LayoutId = layout.Id;
            stretch.Start ??= locations.GetValueOrDefault(stretch.StartId)!;
            stretch.End ??= locations.GetValueOrDefault(stretch.EndId)!;
        }
        foreach (var stretch in layout.TimetableStretches)
            if (Take(stretch, "stretchIds") is List<int> ids) stretch.Stretches = [.. ids.Select(stretches.GetValueOrDefault).OfType<TrackStretch>()];
        foreach (var stretch in layout.DispatchStretches)
        {
            if (Take(stretch, "fromId") is int from) stretch.From = locations.GetValueOrDefault(from)!;
            if (Take(stretch, "toId") is int to) stretch.To = locations.GetValueOrDefault(to)!;
            if (Take(stretch, "stretchIds") is List<int> ids) stretch.Stretches = [.. ids.Select(stretches.GetValueOrDefault).OfType<TrackStretch>()];
        }
        foreach (var module in layout.Modules.Select(m => m.Module))
        {
            if (Take(module, "operationLocationId") is not int id || locations.GetValueOrDefault(id) is not { } location) continue;
            module.OperationLocation = location;
            module.Tracks = location.Tracks;
        }
    }

    private static object? Take(object target, string name)
    {
        if (!Pending.TryGetValue(target, out var values) || !values.Remove(name, out var value)) return null;
        if (values.Count == 0) Pending.Remove(target);
        return value;
    }

    private static FileProperty Single<T>(string name, Func<T, OperationLocation?> target) =>
        new(typeof(T), name, typeof(int?), o => target((T)o)?.Id);

    private static FileProperty List<T>(string name, Func<T, IEnumerable<int>> ids) =>
        new(typeof(T), name, typeof(List<int>), o => ids((T)o).ToList() is { Count: > 0 } list ? list : null);

    private sealed record FileProperty(Type Owner, string Name, Type Type, Func<object, object?> Get);
}
