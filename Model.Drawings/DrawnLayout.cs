using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Drawings;

/// <summary>
/// A layout as it is drawn in a hall plan (<c>ModelSurfaceAnalysis.md</c> 5): the operating <see cref="Layout"/>, and
/// what it is made of: the modules with their drawings, the lines that order them, the joints, and the end profiles,
/// standards and overrides the modules need, so that it is complete without the hall. The planner takes only the
/// result (<see cref="LineStretches.ToLayout"/>): locations, tracks and stretches with what was measured.
/// </summary>
/// <remarks>
/// In a hall plan file it is written as one object, <c>content.layout</c>: the operating layout's properties with the
/// drawn ones beside them (<see cref="ToBlock"/>, <see cref="FromBlock"/>).
/// </remarks>
public sealed class DrawnLayout
{
    /// <summary>The operating layout: locations, tracks, stretches and what is decided for them.</summary>
    public Layout Layout { get; set; } = new();

    /// <summary>
    /// The module standard that the hall plan is made for, which also tells its scale. Modules of another standard can
    /// be part of the layout. Null when not stated.
    /// </summary>
    public ModuleStandard? PreferredStandard { get; set; }

    /// <summary>Every module of the hall plan, also the plain-line ones.</summary>
    public ICollection<PlacedModule> Modules { get; set; } = [];

    /// <summary>
    /// The lines that order the <see cref="Modules"/>, end to end (HP-3). A line without <see cref="Line.StartAt"/> is a
    /// main line; it and the branches from it are one connected group.
    /// </summary>
    public ICollection<Line> Lines { get; set; } = [];

    /// <summary>The joints the planner has decided something for (HP-11); the other joints follow the rules.</summary>
    public ICollection<Joint> Joints { get; set; } = [];

    /// <summary>Every end profile that the <see cref="Modules"/> use, each once.</summary>
    public ICollection<EndProfile> EndProfiles { get; set; } = [];

    /// <summary>
    /// The standards of the layout and of its modules, each once, with all their properties: rail-top height, fouling
    /// distance, clearance. A module's own <see cref="Module.Standard"/> may name its standard only.
    /// </summary>
    public ICollection<ModuleStandard> Standards { get; set; } = [];

    /// <summary>The compatibility overrides that apply to pairs of the <see cref="EndProfiles"/>.</summary>
    public ICollection<EndProfileCompatibilityOverride> Overrides { get; set; } = [];

    /// <summary>The tolerances that decide whether two module ends fit; null for <see cref="FitTolerances.Default"/>.</summary>
    public FitTolerances? FitTolerances { get; set; }

    /// <summary>The scale of the layout: that of its preferred standard. Null when not stated.</summary>
    public Scale? Scale => PreferredStandard?.Scale;

    /// <summary>Sets the references the JSON form leaves out: the layout's own, and each module's location.</summary>
    public void RestoreReferences()
    {
        Layout.RestoreReferences();
        var locations = Layout.OperationLocations.GroupBy(l => l.Id).ToDictionary(g => g.Key, g => g.First());
        foreach (var module in Modules.Select(m => m.Module))
        {
            if (module.OperationLocation is not null || module.OperationLocationId is not int id || locations.GetValueOrDefault(id) is not { } location) continue;
            module.OperationLocation = location;
        }
    }

    /// <summary>A copy through the JSON form, with its references restored.</summary>
    public DrawnLayout Copy() => FromBlock(ToBlock());

    /// <summary>The layout block of a hall plan file: the operating layout's properties with the drawn ones beside them.</summary>
    public JsonObject ToBlock()
    {
        var block = JsonSerializer.SerializeToNode(Layout, LayoutJsonContext.Default.Layout)!.AsObject();
        block.Remove(ModulesProperty); // the module summaries are the planner's; the block has the modules themselves
        Add(block, "preferredStandard", PreferredStandard);
        Add(block, ModulesProperty, Modules);
        Add(block, "lines", Lines);
        Add(block, "joints", Joints);
        Add(block, "endProfiles", EndProfiles);
        Add(block, "standards", Standards);
        Add(block, "overrides", Overrides);
        Add(block, "fitTolerances", FitTolerances);
        return block;
    }

    /// <summary>
    /// The drawn layout of a layout block (<see cref="ToBlock"/>), with its references restored. The block is not
    /// changed.
    /// </summary>
    /// <exception cref="JsonException">The block can't be read.</exception>
    public static DrawnLayout FromBlock(JsonObject block)
    {
        ArgumentNullException.ThrowIfNull(block);
        var rest = (JsonObject)block.DeepClone();
        var drawn = new DrawnLayout
        {
            PreferredStandard = Take<ModuleStandard>(rest, "preferredStandard"),
            Modules = Take<ICollection<PlacedModule>>(rest, ModulesProperty) ?? [],
            Lines = Take<ICollection<Line>>(rest, "lines") ?? [],
            Joints = Take<ICollection<Joint>>(rest, "joints") ?? [],
            EndProfiles = Take<ICollection<EndProfile>>(rest, "endProfiles") ?? [],
            Standards = Take<ICollection<ModuleStandard>>(rest, "standards") ?? [],
            Overrides = Take<ICollection<EndProfileCompatibilityOverride>>(rest, "overrides") ?? [],
            FitTolerances = Take<FitTolerances>(rest, "fitTolerances"),
        };
        drawn.Layout = rest.Deserialize(LayoutJsonContext.Default.Layout) ?? new();
        drawn.RestoreReferences();
        return drawn;
    }

    private const string ModulesProperty = "modules";

    private static void Add<T>(JsonObject block, string name, T? value)
    {
        if (value is null) return;
        block[name] = JsonSerializer.SerializeToNode(value, TypeInfo<T>());
    }

    private static T? Take<T>(JsonObject block, string name)
    {
        if (!block.TryGetPropertyValue(name, out var node)) return default;
        block.Remove(name);
        return node is null ? default : node.Deserialize(TypeInfo<T>());
    }

    private static JsonTypeInfo<T> TypeInfo<T>() =>
        (JsonTypeInfo<T>)(DrawnLayoutJsonContext.Default.GetTypeInfo(typeof(T)) ?? throw new InvalidOperationException($"{typeof(T)} is not in the drawn layout's JSON context."));

    /// <inheritdoc/>
    public override string ToString() => Layout.ToString();
}

/// <summary>
/// Writes and reads a <see cref="DrawnLayout"/> as the layout block of a hall plan file (<see cref="DrawnLayout.ToBlock"/>),
/// with the layout's own options and metadata whatever options the hall plan around it has. Public, so that another
/// app's source-generated serialiser that writes a hall plan uses it.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class DrawnLayoutJsonConverter : JsonConverter<DrawnLayout>
{
    /// <inheritdoc/>
    public override DrawnLayout? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonNode.Parse(ref reader) is JsonObject block ? DrawnLayout.FromBlock(block) : throw new JsonException("The layout block is not an object.");

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, DrawnLayout value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        value.ToBlock().WriteTo(writer);
    }
}
