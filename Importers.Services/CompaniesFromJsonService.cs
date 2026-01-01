
using System.Text.Json;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

public class CompaniesFromJsonService(string? path = null) : ICompaniesService
{
    private readonly string _path = path ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON", "OperatingCompanies.json");
    public async Task<IEnumerable<Company>> GetAllCompaiesAsync()
    {
        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<IEnumerable<Company>>(json) ?? [];
    }
}
