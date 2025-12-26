
using System.Text.Json;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

public class OperatingCompaniesFromJsonService(string? path = null) : IOperatingCompaniesService
{
    private readonly string _path = path ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JSON", "OperatingCompanies.json");
    public async Task<IEnumerable<OperatingCompany>> GetAllOperatingCompaies()
    {
        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<IEnumerable<OperatingCompany>>(json) ?? [];
    }
}
