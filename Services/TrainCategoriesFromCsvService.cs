using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

public class TrainCategoriesFromCsvService(string? path = null) : ITrainCategoriesService
{
    private readonly string _path = path ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSV", "TrainCategories.csv");

    public Task<IEnumerable<TrainCategory>> GetAllTrainCategoriesAsync()
    {
        // Returns empty enumerable for now - categories will be created on-the-fly during import
        // Future: load predefined categories from CSV file if it exists
        return Task.FromResult<IEnumerable<TrainCategory>>([]);
    }
}
