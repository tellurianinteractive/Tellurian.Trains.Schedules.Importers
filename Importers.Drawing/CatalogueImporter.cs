using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;

namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// Reads Module Draw's catalogue files (<c>.catalogue.json</c>, and <c>.tdcatalogue</c> of Module Draw 0.4 and earlier):
/// scales, standards, add-ons, end profiles and compatibility overrides. A catalogue has no identity of its own; its
/// entries are merged into another by their ids (<see cref="Catalogue.Merge"/>).
/// </summary>
public sealed class CatalogueImporter : IImporter<Catalogue>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawCatalogue";

    /// <inheritdoc/>
    public IReadOnlyList<string> FileExtensions { get; } = [DrawingFileFormat.JsonExtension, DrawingFileFormat.LegacyExtension(DrawingFileKind.Catalogue)];

    /// <inheritdoc/>
    public bool CanImport(string fileName, ReadOnlySpan<byte> content) => DrawingFileReader.IsOfKind(content, DrawingFileKind.Catalogue);

    /// <inheritdoc/>
    public async Task<ImportResult<Catalogue>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
        Import(await DrawingFileReader.ReadTextAsync(stream, cancellationToken).ConfigureAwait(false));

    /// <summary>The catalogue in <paramref name="json"/>, the text of a catalogue file; or a failure saying why not.</summary>
    public ImportResult<Catalogue> Import(string json) =>
        DrawingFileReader.Read(json, DrawingFileKind.Catalogue, DrawingJsonContext.Default.Catalogue);
}
