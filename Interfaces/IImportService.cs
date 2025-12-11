using Tellurian.Trains.Timetables.Importers.Model;

namespace Tellurian.Trains.Timetables.Importers.Interfaces;

public interface IImportService
{
    ImportResult<Schedule> ImportSchedule(string name);

}
