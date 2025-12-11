using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public interface IImportService
{
    ImportResult<Schedule> ImportSchedule(string name);

}
