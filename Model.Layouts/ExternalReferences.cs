using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// What other systems keep about a thing (<c>externalReferences</c>), with one entry per system. Each system owns its
/// entry: it alone writes it and decides what is in it. The entry is kept as JSON, whole, so that what a system adds
/// survives every app that reads and writes the file; the names and the typed reading of the Module Registry's entry are
/// here so that every app agrees on them.
/// </summary>
public static class ExternalReferences
{
    /// <summary>The name of the section in a file.</summary>
    public const string Section = "externalReferences";

    /// <summary>The entry of the Module Registry.</summary>
    public const string ModuleRegistry = "moduleRegistry";

    /// <summary>In the Registry's entry of a module: the Registry's id of it.</summary>
    public const string ModuleId = "moduleId";

    /// <summary>In the Registry's entry of an operation location: the Registry's id of its station.</summary>
    public const string StationId = "stationId";

    /// <summary>In the Registry's entry of an end profile: the Registry's id of it.</summary>
    public const string EndProfileId = "endProfileId";

    /// <summary>In the Registry's entry of a standard: the Registry's id of it.</summary>
    public const string StandardId = "standardId";

    extension(IReadOnlyDictionary<string, JsonElement>? references)
    {
        /// <summary>
        /// The Module Registry's id named <paramref name="key"/> (<see cref="ModuleId"/>, <see cref="StationId"/> …), or
        /// null when the Registry's entry or the id is missing.
        /// </summary>
        public int? ModuleRegistryId(string key) =>
            references is not null && references.TryGetValue(ModuleRegistry, out var entry) && entry.ValueKind == JsonValueKind.Object &&
            entry.TryGetProperty(key, out var id) && id.ValueKind == JsonValueKind.Number && id.TryGetInt32(out var value) ? value : null;
    }

    /// <summary>
    /// <paramref name="references"/> with the Module Registry's id named <paramref name="key"/> set to <paramref name="id"/>,
    /// keeping everything else of the Registry's entry and the other systems' entries as they were.
    /// </summary>
    public static Dictionary<string, JsonElement> WithModuleRegistryId(IReadOnlyDictionary<string, JsonElement>? references, string key, int id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        var result = references is null ? [] : new Dictionary<string, JsonElement>(references);
        var entry = result.TryGetValue(ModuleRegistry, out var existing) && existing.ValueKind == JsonValueKind.Object
            ? JsonNode.Parse(existing.GetRawText())!.AsObject()
            : [];
        entry[key] = id;
        using var document = JsonDocument.Parse(entry.ToJsonString());
        result[ModuleRegistry] = document.RootElement.Clone();
        return result;
    }

    /// <summary>Whether two sections have the same entries with the same content; a missing section equals an empty one.</summary>
    public static bool Same(IReadOnlyDictionary<string, JsonElement>? a, IReadOnlyDictionary<string, JsonElement>? b)
    {
        if (ReferenceEquals(a, b)) return true;
        if ((a?.Count ?? 0) != (b?.Count ?? 0)) return false;
        if (a is null || b is null) return true;
        return a.All(entry => b.TryGetValue(entry.Key, out var other) && JsonElement.DeepEquals(entry.Value, other));
    }

    /// <summary>
    /// Whether two things are the same in another system: the same Module Registry id named <paramref name="registryKey"/>
    /// when both have one; else an entry of the same system with the same content.
    /// </summary>
    public static bool ReferToTheSame(IReadOnlyDictionary<string, JsonElement>? a, IReadOnlyDictionary<string, JsonElement>? b, string registryKey)
    {
        if (a is not { Count: > 0 } || b is not { Count: > 0 }) return false;
        if (a.ModuleRegistryId(registryKey) is int mine && b.ModuleRegistryId(registryKey) is int theirs) return mine == theirs;
        return a.Any(entry => b.TryGetValue(entry.Key, out var other) && JsonElement.DeepEquals(entry.Value, other));
    }
}
