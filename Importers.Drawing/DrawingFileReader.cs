using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// Reads Module Draw's drawing files: the envelope, the migrations up to the current schema version, and the content
/// through the source-generated <see cref="DrawingJsonContext"/>. Used by the importers of each kind; never throws
/// because of what a file contains.
/// </summary>
public static class DrawingFileReader
{
    private const string FormatProperty = "format";
    private const string KindProperty = "kind";
    private const string SchemaVersionProperty = "schemaVersion";
    private const string AppVersionProperty = "appVersion";

    /// <summary>
    /// What the envelope of <paramref name="json"/> says, without reading its content; null when it isn't a drawing
    /// file, with <paramref name="message"/> saying why.
    /// </summary>
    public static DrawingFileHeader? ReadHeader(string json, out Message? message)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            message = null;
            return Header(FileEnvelope.Parse(json));
        }
        catch (FileFormatException ex)
        {
            message = MessageOf(ex);
            return null;
        }
    }

    /// <summary>What the envelope of <paramref name="root"/> says.</summary>
    /// <exception cref="FileFormatException">The file isn't a drawing file (<see cref="NotThisFormat"/>).</exception>
    public static DrawingFileHeader Header(JsonNode? root)
    {
        if (root is not JsonObject envelope || Text(envelope, FormatProperty) != DrawingFileFormat.Format)
            throw NotADrawingFile($"The file isn't a Tellurian drawing file (\"{FormatProperty}\" must be \"{DrawingFileFormat.Format}\").");
        if (!Enum.TryParse<DrawingFileKind>(Text(envelope, KindProperty), ignoreCase: true, out var kind) || !Enum.IsDefined(kind))
            throw NotADrawingFile($"The drawing file's kind {envelope[KindProperty]?.ToJsonString() ?? "null"} is unknown.");
        if (envelope[SchemaVersionProperty] is not JsonValue value || !value.TryGetValue<int>(out var version) || version < 1)
            throw NotADrawingFile("The drawing file has no valid schema version.");
        return new(kind, version, Text(envelope, AppVersionProperty));
    }

    /// <summary>
    /// The content of <paramref name="json"/> as a <paramref name="kind"/> file, brought up to the current schema version
    /// and read with <paramref name="typeInfo"/>; or a failure whose message has a <see cref="FileErrorCode"/>.
    /// </summary>
    public static ImportResult<T> Read<T>(string json, DrawingFileKind kind, JsonTypeInfo<T> typeInfo)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(typeInfo);
        try
        {
            var fileKind = DrawingMigrations.FileKind(kind);
            var root = FileEnvelope.Parse(json);
            var header = Header(root);
            var content = FileEnvelope.ReadContent(fileKind, root);
            // Operation locations of earlier files: the kind first and named as in the code, as the shared model reads it.
            OperationLocationJson.Normalise(content);
            var item = content.Deserialize(typeInfo);
            return item is null
                ? Failure<T>(FileErrorCode.InvalidContent, $"The {DrawingFileFormat.Name(kind)} file is empty.")
                : ImportResult<T>.Success(item);
        }
        catch (FileFormatException ex)
        {
            return ImportResult<T>.Failure(MessageOf(ex));
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or FormatException or ArgumentException)
        {
            return Failure<T>(FileErrorCode.InvalidContent, $"The {DrawingFileFormat.Name(kind)} file content is invalid: {ex.Message}");
        }
    }

    /// <summary>The text of <paramref name="stream"/>, read as UTF-8.</summary>
    public static async Task<string> ReadTextAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Whether <paramref name="content"/> is a drawing file of <paramref name="kind"/>, by a quick look at the top level
    /// of its envelope; never throws.
    /// </summary>
    public static bool IsOfKind(ReadOnlySpan<byte> content, DrawingFileKind kind)
    {
        if (content.StartsWith(Encoding.UTF8.Preamble)) content = content[Encoding.UTF8.Preamble.Length..];
        try
        {
            var reader = new Utf8JsonReader(content, new JsonReaderOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject) return false;
            string? format = null, fileKind = null;
            while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
            {
                var name = reader.GetString();
                if (!reader.Read()) return false;
                if (reader.TokenType == JsonTokenType.String && name == FormatProperty) format = reader.GetString();
                else if (reader.TokenType == JsonTokenType.String && name == KindProperty) fileKind = reader.GetString();
                else reader.Skip();
                if (format is not null && fileKind is not null) break;
            }
            return format == DrawingFileFormat.Format && string.Equals(fileKind, kind.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static Message MessageOf(FileFormatException exception) =>
        Message.Error(exception.Message) with { Code = FileErrorCode.Of(exception.Error) };

    private static ImportResult<T> Failure<T>(string code, string text) => ImportResult<T>.Failure(Message.Error(text) with { Code = code });

    private static FileFormatException NotADrawingFile(string reason) => new(new NotThisFormat(reason), reason);

    private static string? Text(JsonObject envelope, string name) =>
        envelope[name] is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;
}
