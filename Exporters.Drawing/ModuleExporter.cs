using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;

namespace Tellurian.Trains.Schedules.Exporters.Drawing;

/// <summary>
/// Writes Module Draw's module files (<c>.module.json</c>) at the current schema version: the module with its drawing,
/// the end profiles it uses and its measures (<see cref="ModuleFileContent.Create"/> makes the content). A write that is
/// a new revision (<see cref="ExportOptions.IsRevision"/>) counts up the module's <see cref="Module.Revision"/> and
/// records who revised it in <see cref="Module.RevisionLog"/>; the content given is not changed.
/// </summary>
public sealed class ModuleExporter : IExporter<ModuleFileContent>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawModule";

    /// <inheritdoc/>
    public string FileExtension => DrawingFileFormat.Extension(DrawingFileKind.Module);

    /// <inheritdoc/>
    public string MediaType => DrawingFileFormat.MediaType;

    /// <inheritdoc/>
    public Task<ExportResult<ModuleFileContent>> ExportAsync(ModuleFileContent item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default)
    {
        var written = Revise(item, options);
        return DrawingFileWriter.ToStreamAsync(written, DrawingFileWriter.Write(written, DrawingFileKind.Module, DrawingJsonContext.Default.ModuleFileContent, options), destination, cancellationToken);
    }

    /// <summary>The text of the module file for <paramref name="item"/>, as <see cref="Revise"/> makes it.</summary>
    public string Write(ModuleFileContent item, ExportOptions options) =>
        DrawingFileWriter.Write(Revise(item, options), DrawingFileKind.Module, DrawingJsonContext.Default.ModuleFileContent, options);

    /// <summary>
    /// What is written for <paramref name="item"/>: with <see cref="ExportOptions.IsRevision"/> the content with a copy of
    /// the module that has the next revision and the log entry; otherwise <paramref name="item"/> itself.
    /// </summary>
    public ModuleFileContent Revise(ModuleFileContent item, ExportOptions options)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(options);
        if (!options.IsRevision) return item;
        var module = item.Module;
        return item with
        {
            Module = module.With(m =>
            {
                m.Revision = module.Revision + 1;
                m.RevisionLog = [.. DrawingRevision.Record(module.RevisionLog, options.RevisedBy, DrawingFileWriter.RevisionDate(options))];
            }),
        };
    }
}
