using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public interface IExportService
{
    Task<ExportResult<Schedule>> ExportScheduleAsync(Schedule schedule);
}
