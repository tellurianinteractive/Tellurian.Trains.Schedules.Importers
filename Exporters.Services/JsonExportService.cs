using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Exporters.Services;

/// <summary>
/// Exports a whole plan as a plan file to <paramref name="destination"/>, creating its folder if needed. Written by
/// <see cref="PlanJsonExporter"/>.
/// </summary>
/// <param name="destination">The file to write; replaced if it exists.</param>
public class JsonExportService(FileInfo destination) : IExportService
{
    private readonly PlanJsonExporter _exporter = new();

    /// <inheritdoc/>
    public async Task<ExportResult<Plan>> ExportScheduleAsync(Plan schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        try
        {
            if (destination.Directory is { Exists: false } directory) directory.Create();
            await using var stream = destination.Create();
            return await _exporter.ExportAsync(schedule, stream, ExportOptions.Default).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return ExportResult<Plan>.Failure(Message.Error($"{ex.GetType().Name}: {ex.Message}") with { Code = PlanJsonExporter.WriteFailed });
        }
    }
}
