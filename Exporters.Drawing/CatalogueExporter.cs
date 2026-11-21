using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

namespace Tellurian.Trains.Schedules.Exporters.Drawing;

/// <summary>
/// Writes Module Draw's catalogue files (<c>.catalogue.json</c>) at the current schema version. A catalogue has no
/// revision log, so <see cref="ExportOptions.IsRevision"/> changes nothing; its entries keep their own.
/// </summary>
public sealed class CatalogueExporter : IExporter<DrawingCatalogue>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawCatalogue";

    /// <inheritdoc/>
    public string FileExtension => DrawingFileFormat.Extension(DrawingFileKind.Catalogue);

    /// <inheritdoc/>
    public string MediaType => DrawingFileFormat.MediaType;

    /// <inheritdoc/>
    public Task<ExportResult<DrawingCatalogue>> ExportAsync(DrawingCatalogue item, Stream destination, ExportOptions options, CancellationToken cancellationToken = default) =>
        DrawingFileWriter.ToStreamAsync(item, Write(item, options), destination, cancellationToken);

    /// <summary>The text of the catalogue file for <paramref name="item"/>.</summary>
    public string Write(DrawingCatalogue item, ExportOptions options) =>
        DrawingFileWriter.Write(item, DrawingFileKind.Catalogue, DrawingJsonContext.Default.DrawingCatalogue, options);
}
