using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Layouts;

/// <summary>
/// Reads the layout block of a hall plan file written by Module Draw. Only that block is a public contract of the file
/// (<c>Documentation/OperationLocations.md</c>, section 4.6): the locations, tracks and stretches worked out from the
/// hall plan, so that no reader needs its geometry. The rest of the file is Module Draw's own.
/// </summary>
public static class HallPlanFile
{
    /// <summary>What the <c>format</c> of a Module Draw file says.</summary>
    public const string Format = "tellurian-drawing";

    /// <summary>What the <c>kind</c> of a hall plan file says.</summary>
    public const string Kind = "HallPlan";

    /// <summary>The name of the layout block in the content of a hall plan file.</summary>
    public const string LayoutProperty = "layout";

    /// <summary>
    /// The layout of the hall plan file <paramref name="json"/>, with its references resolved
    /// (<c>layout.RestoreReferences()</c>). A layout block written alone, without the file around it, is read
    /// too.
    /// </summary>
    /// <exception cref="InvalidDataException">The text is not a hall plan file, or the file has no layout block.</exception>
    public static Layout ReadLayout(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        JsonNode? root;
        try
        {
            root = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { MaxDepth = 256 });
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"The file is not valid JSON: {ex.Message}", ex);
        }
        var block = LayoutBlock(root) ?? throw new InvalidDataException("The file has no layout block. Save the hall plan again with a version of Module Draw that writes it.");
        OperationLocationJson.Normalise(block);
        try
        {
            var layout = block.Deserialize<Layout>(Layout.JsonSerializerOptions) ?? throw new InvalidDataException("The layout block is empty.");
            layout.RestoreReferences();
            return layout;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"The layout block can't be read: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Whether <paramref name="utf8Json"/> looks like what <see cref="ReadLayout"/> reads: a Module Draw file of the kind
    /// hall plan, or a layout block on its own. Only the top level is looked at; a file that is no JSON gives
    /// <c>false</c>. A hall plan without a layout block is recognised, so that reading it says what is missing.
    /// </summary>
    public static bool IsHallPlan(ReadOnlySpan<byte> utf8Json)
    {
        string? format = null, kind = null;
        var hasLocations = false;
        try
        {
            var reader = new Utf8JsonReader(utf8Json, new JsonReaderOptions { MaxDepth = 256, CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) return false;
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString();
                if (!reader.Read()) return false;
                if (name == "format" && reader.TokenType == JsonTokenType.String) format = reader.GetString();
                else if (name == "kind" && reader.TokenType == JsonTokenType.String) kind = reader.GetString();
                else if (name is "operationLocations" or "modules" && reader.TokenType == JsonTokenType.StartArray) hasLocations = true;
                reader.Skip();
            }
        }
        catch (JsonException)
        {
            return false;
        }
        if (format is not null)
            return string.Equals(format, Format, StringComparison.Ordinal) && (kind is null || string.Equals(kind, Kind, StringComparison.OrdinalIgnoreCase));
        return hasLocations;
    }

    /// <summary>The layout block: <c>content.layout</c> of a hall plan file, or the root itself when it is a layout.</summary>
    private static JsonObject? LayoutBlock(JsonNode? root)
    {
        if (root is not JsonObject file) return null;
        if (file["format"] is JsonValue format && format.TryGetValue<string>(out var name))
        {
            if (!string.Equals(name, Format, StringComparison.Ordinal))
                throw new InvalidDataException($"The file is not a Module Draw file: its format is '{name}'.");
            if (file["kind"] is JsonValue kind && kind.TryGetValue<string>(out var kindName) && !string.Equals(kindName, Kind, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException($"The file is a {kindName} file, not a hall plan.");
            return file["content"]?[LayoutProperty] as JsonObject;
        }
        return file.ContainsKey("operationLocations") || file.ContainsKey("modules") ? file : null;
    }
}
