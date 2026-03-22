using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Service for the case that an external list of companies are noot needed.
/// </summary>
public class EmptyCompaniesService() : ICompaniesService
{
    /// <summary>
    /// Retrieves an empty collection of companies.
    /// </summary>
    /// <returns>Empty <see cref="IEnumerable{T}"/></returns>
    public Task<IEnumerable<Company>> GetAllCompaiesAsync()
    {
        var result = Enumerable.Empty<Company>();
        return Task.FromResult(result);
    }
}
