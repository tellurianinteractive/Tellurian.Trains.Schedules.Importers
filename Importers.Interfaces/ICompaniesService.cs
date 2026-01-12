using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Interfaces;

/// <summary>
/// Defines a service for retrieving railway company information.
/// </summary>
public interface ICompaniesService
{
    /// <summary>
    /// Retrieves all available railway companies asynchronously.
    /// </summary>
    /// <returns>A collection of <see cref="Company"/> objects representing all available companies.</returns>
    Task<IEnumerable<Company>> GetAllCompaiesAsync();
}
