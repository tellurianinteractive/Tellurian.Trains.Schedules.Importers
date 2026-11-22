using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;


namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// Reads Module Draw's module files (<c>.module.json</c>, and <c>.tdmodule</c> of Module Draw 0.4 and earlier), of any schema
/// version up to the current one.
/// </summary>
public sealed class ModuleImporter : IImporter<ModuleFileContent>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawModule";

    /// <inheritdoc/>
    public IReadOnlyList<string> FileExtensions { get; } = [DrawingFileFormat.JsonExtension, DrawingFileFormat.LegacyExtension(DrawingFileKind.Module)];

    /// <inheritdoc/>
    public bool CanImport(string fileName, ReadOnlySpan<byte> content) => DrawingFileReader.IsOfKind(content, DrawingFileKind.Module);

    /// <inheritdoc/>
    public async Task<ImportResult<ModuleFileContent>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
        Import(await DrawingFileReader.ReadTextAsync(stream, cancellationToken).ConfigureAwait(false));

    /// <summary>The module file content in <paramref name="json"/>: the module with its drawing and the end profiles it uses; or a failure saying why not.</summary>
    public ImportResult<ModuleFileContent> Import(string json) =>
        DrawingFileReader.Read(json, DrawingFileKind.Module, DrawingJsonContext.Default.ModuleFileContent);
}
