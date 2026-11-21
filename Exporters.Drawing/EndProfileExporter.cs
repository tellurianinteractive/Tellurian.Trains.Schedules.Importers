using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Exporters.Drawing;

/// <summary>
/// Writes Module Draw's end profile files (<c>.endprofile.json</c>) at the current schema version. A write that is a
/// new revision (<see cref="ExportOptions.IsRevision"/>) counts up <see cref="EndProfile.Revision"/> and records who
/// revised it in <see cref="EndProfile.RevisionLog"/>; the end profile given is not changed.
/// </summary>
public sealed class EndProfileExporter : IExporter<EndProfile>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawEndProfile";

    /// <inheritdoc/>
    public string FileExtension => DrawingFileFormat.Extension(DrawingFileKind.EndProfile);

    /// <inheritdoc/>
    public string MediaType => DrawingFileFormat.MediaType;

    /// <inheritdoc/>
    public Task<ExportResult<EndProfile>> ExportAsync(EndProfile item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var written = Revise(item, options);
        return DrawingFileWriter.ToStreamAsync(written, DrawingFileWriter.Write(written, DrawingFileKind.EndProfile, DrawingJsonContext.Default.EndProfile, options), destination, cancellationToken);
    }

    /// <summary>The text of the end profile file for <paramref name="item"/>, as <see cref="Revise"/> makes it.</summary>
    public string Write(EndProfile item, ExportOptions options) =>
        DrawingFileWriter.Write(Revise(item, options), DrawingFileKind.EndProfile, DrawingJsonContext.Default.EndProfile, options);

    /// <summary>
    /// What is written for <paramref name="item"/>: with <see cref="ExportOptions.IsRevision"/> a copy with the next
    /// revision and the log entry; otherwise <paramref name="item"/> itself.
    /// </summary>
    public EndProfile Revise(EndProfile item, ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(options);
        if (!options.IsRevision) return item;
        return item.With(p =>
        {
            p.Revision = item.Revision + 1;
            p.RevisionLog = [.. DrawingRevision.Record(item.RevisionLog, options.RevisedBy, DrawingFileWriter.RevisionDate(options))];
        });
    }
}
