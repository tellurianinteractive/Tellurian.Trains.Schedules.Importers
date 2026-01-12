
using System.Text.Json;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Service for retrieving company data from a JSON file.
/// </summary>
/// <param name="path">Optional path to the JSON file containing company data. If not specified, defaults to 'JSON/OperatingCompanies.json' in the application base directory.</param>
public class CompaniesFromJsonService(string? path = null) : ICompaniesService
{
    private readonly string _path = path ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON", "OperatingCompanies.json");

    /// <summary>
    /// Retrieves all companies from the configured JSON file asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of companies.</returns>
    public async Task<IEnumerable<Company>> GetAllCompaiesAsync()
    {
        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<IEnumerable<Company>>(json) ?? [];
    }
}
