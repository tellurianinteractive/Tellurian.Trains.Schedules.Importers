using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Drawings.Halls;

namespace Tellurian.Trains.Schedules.Exporters.Drawing;

/// <summary>
/// Writes Module Draw's hall plan files (<c>.hallplan.json</c>) at the current schema version, with the layout as
/// <c>content.layout</c> in the layout's own form, as the planner reads it (<see cref="HallPlanFile.ReadLayout"/>). The
/// layout written has the hall plan's id and revision as its <see cref="Layout.SourceId"/> and
/// <see cref="Layout.SourceRevision"/>, and the hall plan's name when it has none of its own. A write that is a new
/// revision (<see cref="ExportOptions.IsRevision"/>) counts up <see cref="HallPlan.Revision"/> and records who revised it
/// in <see cref="HallPlan.RevisionLog"/>. The hall plan given is not changed.
/// </summary>
public sealed class HallPlanExporter : IExporter<HallPlan>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawHallPlan";

    /// <inheritdoc/>
    public string FileExtension => DrawingFileFormat.Extension(DrawingFileKind.HallPlan);

    /// <inheritdoc/>
    public string MediaType => DrawingFileFormat.MediaType;

    /// <inheritdoc/>
    public Task<ExportResult<HallPlan>> ExportAsync(HallPlan item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var written = Revise(item, options);
        return DrawingFileWriter.ToStreamAsync(written, DrawingFileWriter.Write(written, DrawingFileKind.HallPlan, DrawingJsonContext.Default.HallPlan, options), destination, cancellationToken);
    }

    /// <summary>The text of the hall plan file for <paramref name="item"/>, as <see cref="Revise"/> makes it.</summary>
    public string Write(HallPlan item, ExportOptions options) =>
        DrawingFileWriter.Write(Revise(item, options), DrawingFileKind.HallPlan, DrawingJsonContext.Default.HallPlan, options);

    /// <summary>
    /// What is written for <paramref name="item"/> (<see cref="HallPlan.Written"/>): with <see cref="ExportOptions.IsRevision"/>
    /// the next revision and the log entry, otherwise its own; in both, its layout has the hall plan's identity.
    /// </summary>
    public HallPlan Revise(HallPlan item, ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(options);
        return options.IsRevision
            ? item.Written(item.Revision + 1, DrawingRevision.Record(item.RevisionLog, options.RevisedBy, DrawingFileWriter.RevisionDate(options)))
            : item.Written(item.Revision, item.RevisionLog);
    }
}
