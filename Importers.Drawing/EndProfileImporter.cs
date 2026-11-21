using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// Reads Module Draw's end profile files (<c>.endprofile.json</c>, and <c>.tdendprofile</c> of Module Draw 0.4 and
/// earlier), of any schema version up to the current one.
/// </summary>
public sealed class EndProfileImporter : IImporter<EndProfile>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawEndProfile";

    /// <inheritdoc/>
    public IReadOnlyList<string> FileExtensions { get; } = [DrawingFileFormat.JsonExtension, DrawingFileFormat.LegacyExtension(DrawingFileKind.EndProfile)];

    /// <inheritdoc/>
    public bool CanImport(string fileName, ReadOnlySpan<byte> content) => DrawingFileReader.IsOfKind(content, DrawingFileKind.EndProfile);

    /// <inheritdoc/>
    public async Task<ImportResult<EndProfile>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
        Import(await DrawingFileReader.ReadTextAsync(stream, cancellationToken).ConfigureAwait(false));

    /// <summary>The end profile in <paramref name="json"/>, the text of an end profile file; or a failure saying why not.</summary>
    public ImportResult<EndProfile> Import(string json) =>
        DrawingFileReader.Read(json, DrawingFileKind.EndProfile, DrawingJsonContext.Default.EndProfile);
}
