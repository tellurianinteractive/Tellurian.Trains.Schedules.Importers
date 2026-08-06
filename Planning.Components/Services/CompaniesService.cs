using System.Net.Http.Json;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

public sealed class CompaniesService(HttpClient http) : ICompaniesService
{
    public async Task<IEnumerable<Company>> GetAllCompaiesAsync()
    {
        try
        {
            var records = await http.GetFromJsonAsync<IEnumerable<CompanyRecord>>("_content/Tellurian.Trains.Schedules.Planning.Components/data/OperatingCompanies.json") ?? [];
            // The seed file stores an ISO country code; the model references a country by its stable
            // Country.Id, so map the code through the catalogue (unknown codes become null).
            return [.. records.Select(r => new Company(r.Id, r.Name, r.Signature, Country.ByCountryCode(r.CountryCode ?? "")?.Id))];
        }
        catch
        {
            return [];
        }
    }

    // Mirrors the seed JSON shape, which predates the move from country code to country id.
    private sealed record CompanyRecord(int Id, string Name, string Signature, string? CountryCode);
}
