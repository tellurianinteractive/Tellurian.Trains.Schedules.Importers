using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public interface ITrainCategoriesService
{
    Task<IEnumerable<TrainCategory>> GetAllTrainCategoriesAsync();
}
