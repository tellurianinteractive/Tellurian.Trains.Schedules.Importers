using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public interface IImportService
{
    Task<ImportResult<Schedule>> ImportScheduleAsync(string name);
}
