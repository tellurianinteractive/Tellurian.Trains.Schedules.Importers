using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Drawings.Halls;

namespace Tellurian.Trains.Schedules.Exporters.Drawing;

/// <summary>
/// Writes Module Draw's hall files (<c>.hall.json</c>) at the current schema version. A write that is a new revision
/// (<see cref="ExportOptions.IsRevision"/>) counts up <see cref="Hall.Revision"/> and records who revised it in
/// <see cref="Hall.RevisionLog"/>; the hall given is not changed.
/// </summary>
public sealed class HallExporter : IExporter<Hall>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawHall";

    /// <inheritdoc/>
    public string FileExtension => DrawingFileFormat.Extension(DrawingFileKind.Hall);

    /// <inheritdoc/>
    public string MediaType => DrawingFileFormat.MediaType;

    /// <inheritdoc/>
    public Task<ExportResult<Hall>> ExportAsync(Hall item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var written = Revise(item, options);
        return DrawingFileWriter.ToStreamAsync(written, DrawingFileWriter.Write(written, DrawingFileKind.Hall, DrawingJsonContext.Default.Hall, options), destination, cancellationToken);
    }

    /// <summary>The text of the hall file for <paramref name="item"/>, as <see cref="Revise"/> makes it.</summary>
    public string Write(Hall item, ExportOptions options) =>
        DrawingFileWriter.Write(Revise(item, options), DrawingFileKind.Hall, DrawingJsonContext.Default.Hall, options);

    /// <summary>
    /// What is written for <paramref name="item"/>: with <see cref="ExportOptions.IsRevision"/> a copy with the next
    /// revision and the log entry; otherwise <paramref name="item"/> itself.
    /// </summary>
    public Hall Revise(Hall item, ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(options);
        if (!options.IsRevision) return item;
        return item.With(h =>
        {
            h.Revision = item.Revision + 1;
            h.RevisionLog = [.. DrawingRevision.Record(item.RevisionLog, options.RevisedBy, DrawingFileWriter.RevisionDate(options))];
        });
    }
}
