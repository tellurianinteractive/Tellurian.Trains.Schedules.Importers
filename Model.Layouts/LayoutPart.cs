using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// The base of the types of the contract that are written to files: it keeps the JSON properties that this version
/// doesn't know and writes them back, so that a file or an answer from a newer version isn't damaged by being read
/// and written. It can also be copied, for an app that keeps earlier states of what it edits (<see cref="Copy"/>).
/// </summary>
public abstract class LayoutPart
{
    /// <summary>The properties of the JSON object that this version doesn't know, as read. They are written last.</summary>
    [JsonExtensionData]
    [JsonPropertyOrder(int.MaxValue)]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    /// <summary>
    /// A copy that can be changed without changing this: what this owns is copied too, what it only refers to (a
    /// parent, another location) is not. A class that derives from a type of the contract is copied with its own
    /// properties; it overrides this only to copy mutable things of its own.
    /// </summary>
    public virtual LayoutPart Copy()
    {
        var copy = (LayoutPart)MemberwiseClone();
        copy.ExtensionData = ExtensionData is null ? null : new(ExtensionData);
        return copy;
    }
}

/// <summary>Copying with a change, as <c>with</c> does for a record.</summary>
public static class LayoutPartCopying
{
    /// <summary>
    /// A copy of <paramref name="part"/> (<see cref="LayoutPart.Copy"/>) with <paramref name="change"/> made to it;
    /// <paramref name="part"/> itself is not changed. E.g. <c>location.With(l => l.Name = "Tellby")</c>.
    /// </summary>
    /// <remarks>
    /// A classic extension method, not an extension block: generic extension members can fail to load under the
    /// Blazor WebAssembly interpreter.
    /// </remarks>
    public static T With<T>(this T part, Action<T> change) where T : LayoutPart
    {
        ArgumentNullException.ThrowIfNull(part);
        ArgumentNullException.ThrowIfNull(change);
        var copy = (T)part.Copy();
        change(copy);
        return copy;
    }
}
