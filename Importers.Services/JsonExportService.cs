using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

public class JsonExportService(FileInfo destination) : IExportService
{
    private readonly FileInfo _destination = destination;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    /// <summary>
    /// Exports the specified schedule asynchronously as JSON to the configured file.
    /// </summary>
    /// <param name="schedule">The schedule to be exported. Cannot be null.</param>
    /// <returns>A task that represents the asynchronous export operation. The task result contains an ExportResult object with
    /// the exported schedule and status information.</returns>
    public async Task<ExportResult<Schedule>> ExportScheduleAsync(Schedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);

        try
        {
            var path = _destination.FullName;
            if (_destination.Directory is { Exists: false } directory)
            {
                directory.Create();
            }
            var json = JsonSerializer.Serialize(schedule, JsonOptions);
            File.WriteAllText(path, json);
            return ExportResult<Schedule>.Success(schedule);
        }
        catch (Exception ex)
        {
            return ExportResult<Schedule>.Failure($"{ex.GetType().Name}: {ex.Message}");
        }
    }
}
