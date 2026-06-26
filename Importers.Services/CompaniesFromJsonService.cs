
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
        var json = await File.ReadAllTextAsync(_path);
        var records = JsonSerializer.Deserialize<IEnumerable<CompanyRecord>>(json) ?? [];
        // The seed file stores a human-friendly ISO country code; the model references a country by
        // its stable Country.Id, so map the code through the catalogue (unknown codes become null).
        return [.. records.Select(r => new Company(r.Id, r.Name, r.Signature, Country.ByCountryCode(r.CountryCode ?? "")?.Id))];
    }

    // Mirrors the seed JSON shape, which predates the move from country code to country id.
    private sealed record CompanyRecord(int Id, string Name, string Signature, string? CountryCode);
}
