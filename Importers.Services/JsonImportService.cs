using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Service for importing schedule data from JSON files.
/// </summary>
/// <param name="source">The source file containing the JSON data to import.</param>
public class JsonImportService(FileInfo source) : IImportService
{
    private readonly FileInfo _source = source;

    /// <summary>
    /// Imports a schedule from a JSON file asynchronously.
    /// </summary>
    /// <param name="name">The expected name of the schedule. If provided and non-empty,
    /// validates that the imported schedule has a matching name.</param>
    /// <returns>A task that represents the asynchronous import operation. The task result contains an ImportResult object with
    /// the imported schedule and status information.</returns>
    public async Task<ImportResult<Plan>> ImportScheduleAsync(string name)
    {
        try
        {
            if (!_source.Exists)
            {
                return ImportResult<Plan>.Failure(Message.System($"File not found: {_source.FullName}"));
            }

            await using var stream = _source.OpenRead();
            var schedule = await PlanFile.ReadAsync(stream);

            if (!string.IsNullOrEmpty(name) && schedule.Name != name)
            {
                return ImportResult<Plan>.Failure(
                    Message.System($"Schedule name mismatch: expected '{name}', got '{schedule.Name}'"));
            }

            return ImportResult<Plan>.Success(schedule);
        }
        catch (PlanFileException ex)
        {
            return ImportResult<Plan>.Failure(Message.System(ex.Message));
        }
        catch (Exception ex)
        {
            return ImportResult<Plan>.Failure(Message.System($"{ex.GetType().Name}: {ex.Message}"));
        }
    }
}
