using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Defines a service for exporting schedule data to external formats.
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports a schedule asynchronously to an external format.
    /// </summary>
    /// <param name="schedule">The <see cref="Schedule"/> to export.</param>
    /// <returns>An <see cref="ExportResult{T}"/> indicating success or failure with any error messages.</returns>
    Task<ExportResult<Schedule>> ExportScheduleAsync(Schedule schedule);
}
