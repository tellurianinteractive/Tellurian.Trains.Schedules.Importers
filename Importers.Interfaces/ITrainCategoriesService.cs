using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Defines a service for retrieving train category information.
/// </summary>
public interface ITrainCategoriesService
{
    /// <summary>
    /// Retrieves all available train categories asynchronously.
    /// </summary>
    /// <returns>A collection of <see cref="TrainCategory"/> objects representing all available categories.</returns>
    Task<IEnumerable<TrainCategory>> GetAllTrainCategoriesAsync();
}
