using System.Text;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Reads the layout of a Module Draw hall plan file (<see cref="HallPlanFile"/>), or of a layout block written on its
/// own, which is the open format for a layout made by any other tool.
/// </summary>
public sealed class HallPlanLayoutImporter : ILayoutImporter
{
    /// <inheritdoc/>
    public string ResourceKey => "ModuleDrawHallPlan";

    /// <inheritdoc/>
    public IReadOnlyList<string> FileExtensions { get; } = [".json"];

    /// <inheritdoc/>
    public bool CanImport(string fileName, ReadOnlySpan<byte> content) => HallPlanFile.IsHallPlan(WithoutByteOrderMark(content));

    /// <inheritdoc/>
    public async Task<ImportResult<Layout>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var json = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            return ImportResult<Layout>.Success(HallPlanFile.ReadLayout(json));
        }
        catch (InvalidDataException ex)
        {
            return ImportResult<Layout>.Failure(Message.Error(ex.Message));
        }
    }

    private static ReadOnlySpan<byte> WithoutByteOrderMark(ReadOnlySpan<byte> content) =>
        content.StartsWith(Encoding.UTF8.Preamble) ? content[Encoding.UTF8.Preamble.Length..] : content;
}
