using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Files;

namespace Tellurian.Trains.Schedules.Exporters.Drawing;

/// <summary>
/// Writes Module Draw's drawing files: the envelope through <see cref="FileEnvelope"/>, at the current schema version,
/// then the content through the source-generated <see cref="DrawingJsonContext"/>. Used by the exporters of each kind.
/// </summary>
public static class DrawingFileWriter
{
    /// <summary>The <see cref="Message.Code"/> of a failure to write to the destination.</summary>
    public const string WriteFailed = nameof(WriteFailed);

    /// <summary><paramref name="item"/> as the text of a <paramref name="kind"/> file.</summary>
    public static string Write<T>(T item, DrawingFileKind kind, JsonTypeInfo<T> typeInfo, ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(typeInfo);
        ArgumentNullException.ThrowIfNull(options);
        return FileEnvelope.Write(DrawingFileFormat.FileKind(kind), options.AppVersion ?? DrawingFileFormat.LibraryVersion,
            writer => JsonSerializer.Serialize(writer, item, typeInfo), options.WriteIndented);
    }

    /// <summary>Writes <paramref name="json"/> to <paramref name="destination"/> as UTF-8; the result has <paramref name="written"/> or a failure.</summary>
    public static async Task<ExportResult<T>> ToStreamAsync<T>(T written, string json, Stream destination, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);
        try
        {
            await destination.WriteAsync(Encoding.UTF8.GetBytes(json), cancellationToken).ConfigureAwait(false);
            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            return ExportResult<T>.Success(written);
        }
        catch (Exception ex) when (ex is IOException or NotSupportedException or ObjectDisposedException)
        {
            return ExportResult<T>.Failure(Message.Error($"{ex.GetType().Name}: {ex.Message}") with { Code = WriteFailed });
        }
    }

    /// <summary>The date of a revision: the one in <paramref name="options"/>, or today.</summary>
    public static DateOnly RevisionDate(ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.Date ?? DateOnly.FromDateTime(DateTime.Today);
    }
}
