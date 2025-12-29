using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public interface ICompaniesService
{
    Task<IEnumerable<Company>> GetAllCompaiesAsync();
}
