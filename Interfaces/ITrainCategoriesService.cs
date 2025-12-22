using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

public interface ITrainCategoriesService
{
    IEnumerable<TrainCategory> GetAllTrainCategories();
}
