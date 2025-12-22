using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public interface IOperatingCompaniesService
{
    Task<IEnumerable<OperatingCompany>> GetAllOperatingCompaies();
}
