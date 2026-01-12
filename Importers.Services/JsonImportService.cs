using System.Text.Json;
using System.Text.Json.Serialization;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Service for importing schedule data from JSON files.
/// </summary>
/// <param name="source">The source file containing the JSON data to import.</param>
public class JsonImportService(FileInfo source) : IImportService
{
    private readonly FileInfo _source = source;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    /// <summary>
    /// Imports a schedule from a JSON file asynchronously.
    /// </summary>
    /// <param name="name">The expected name of the schedule. If provided and non-empty,
    /// validates that the imported schedule has a matching name.</param>
    /// <returns>A task that represents the asynchronous import operation. The task result contains an ImportResult object with
    /// the imported schedule and status information.</returns>
    public async Task<ImportResult<Schedule>> ImportScheduleAsync(string name)
    {
        try
        {
            if (!_source.Exists)
            {
                return ImportResult<Schedule>.Failure(Message.System($"File not found: {_source.FullName}"));
            }

            await using var stream = _source.OpenRead();
            var schedule = await JsonSerializer.DeserializeAsync<Schedule>(stream, JsonOptions);

            if (schedule is null)
            {
                return ImportResult<Schedule>.Failure(Message.System("Failed to deserialize JSON: result was null"));
            }

            if (!string.IsNullOrEmpty(name) && schedule.Name != name)
            {
                return ImportResult<Schedule>.Failure(
                    Message.System($"Schedule name mismatch: expected '{name}', got '{schedule.Name}'"));
            }

            return ImportResult<Schedule>.Success(schedule);
        }
        catch (JsonException ex)
        {
            return ImportResult<Schedule>.Failure(Message.System($"JSON deserialization error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return ImportResult<Schedule>.Failure(Message.System($"{ex.GetType().Name}: {ex.Message}"));
        }
    }
}
