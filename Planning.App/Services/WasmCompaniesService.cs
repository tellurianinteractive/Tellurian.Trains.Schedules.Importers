using System.Net.Http.Json;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

public sealed class WasmCompaniesService(HttpClient http) : ICompaniesService
{
    public async Task<IEnumerable<Company>> GetAllCompaiesAsync()
    {
        try
        {
            return await http.GetFromJsonAsync<IEnumerable<Company>>("data/OperatingCompanies.json") ?? [];
        }
        catch
        {
            return [];
        }
    }
}
