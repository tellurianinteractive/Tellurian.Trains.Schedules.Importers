using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Drawings.Halls;

namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// Reads Module Draw's hall files (<c>.hall.json</c>, and <c>.tdhall</c> of Module Draw 0.4 and earlier), of any schema
/// version up to the current one.
/// </summary>
public sealed class HallImporter : IImporter<Hall>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawHall";

    /// <inheritdoc/>
    public IReadOnlyList<string> FileExtensions { get; } = [DrawingFileFormat.JsonExtension, DrawingFileFormat.LegacyExtension(DrawingFileKind.Hall)];

    /// <inheritdoc/>
    public bool CanImport(string fileName, ReadOnlySpan<byte> content) => DrawingFileReader.IsOfKind(content, DrawingFileKind.Hall);

    /// <inheritdoc/>
    public async Task<ImportResult<Hall>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
        Import(await DrawingFileReader.ReadTextAsync(stream, cancellationToken).ConfigureAwait(false));

    /// <summary>The hall in <paramref name="json"/>, the text of a hall file; or a failure saying why not.</summary>
    public ImportResult<Hall> Import(string json) =>
        DrawingFileReader.Read(json, DrawingFileKind.Hall, DrawingJsonContext.Default.Hall);
}
