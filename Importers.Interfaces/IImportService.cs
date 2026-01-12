using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Defines a service for importing schedule data from external sources.
/// </summary>
public interface IImportService
{
    /// <summary>
    /// Imports a schedule asynchronously from the specified source.
    /// </summary>
    /// <param name="name">The name or path identifying the schedule data source.</param>
    /// <returns>An <see cref="ImportResult{T}"/> containing the imported <see cref="Schedule"/> and any validation messages.</returns>
    Task<ImportResult<Schedule>> ImportScheduleAsync(string name);
}
