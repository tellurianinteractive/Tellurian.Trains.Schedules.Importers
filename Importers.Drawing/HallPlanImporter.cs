using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Drawings.Halls;

namespace Tellurian.Trains.Schedules.Importers.Drawing;

/// <summary>
/// Reads Module Draw's hall plan files (<c>.hallplan.json</c>) of schema version 2: the hall plan with its hall and its
/// layout, whose references are restored. Earlier hall plan files are refused; no released version of Module Draw made
/// them. The planner reads only the layout of a hall plan file, with <see cref="Model.Drawings.HallPlanFile.ReadLayout"/>.
/// </summary>
public sealed class HallPlanImporter : IImporter<HallPlan>
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawHallPlan";

    /// <inheritdoc/>
    public IReadOnlyList<string> FileExtensions { get; } = [DrawingFileFormat.JsonExtension, DrawingFileFormat.LegacyExtension(DrawingFileKind.HallPlan)];

    /// <inheritdoc/>
    public bool CanImport(string fileName, ReadOnlySpan<byte> content) => DrawingFileReader.IsOfKind(content, DrawingFileKind.HallPlan);

    /// <inheritdoc/>
    public async Task<ImportResult<HallPlan>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
        Import(await DrawingFileReader.ReadTextAsync(stream, cancellationToken).ConfigureAwait(false));

    /// <summary>The hall plan in <paramref name="json"/>, the text of a hall plan file; or a failure saying why not.</summary>
    public ImportResult<HallPlan> Import(string json) =>
        DrawingFileReader.Read(json, DrawingFileKind.HallPlan, DrawingJsonContext.Default.HallPlan);
}
