using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Exporters.Interfaces;

/// <summary>
/// Exports a whole plan to a destination given when the service is made: a JSON file, a database. For writing to a
/// stream in a chosen format, see <see cref="IExporter{T}"/>.
/// </summary>
public interface IExportService
{
    /// <summary>Exports <paramref name="schedule"/>; the result says whether it succeeded, with messages if not.</summary>
    /// <param name="schedule">The <see cref="Plan"/> to export.</param>
    Task<ExportResult<Plan>> ExportScheduleAsync(Plan schedule);
}
