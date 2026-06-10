using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Service for exporting schedule data to JSON files.
/// </summary>
/// <param name="destination">The destination file where the JSON output will be written.</param>
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
    public async Task<ExportResult<Plan>> ExportScheduleAsync(Plan schedule)
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
            return ExportResult<Plan>.Success(schedule);
        }
        catch (Exception ex)
        {
            return ExportResult<Plan>.Failure($"{ex.GetType().Name}: {ex.Message}");
        }
    }
}
