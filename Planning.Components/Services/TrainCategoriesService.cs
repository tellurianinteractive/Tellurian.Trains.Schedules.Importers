using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.Components.Services;

public sealed class TrainCategoriesService(HttpClient http) : ITrainCategoriesService
{
    public async Task<IEnumerable<TrainCategory>> GetAllTrainCategoriesAsync()
    {
        try
        {
            var csv = await http.GetStringAsync("_content/Tellurian.Trains.Schedules.Planning.Components/data/TrainCategories.csv");
            var categories = new List<TrainCategory>();
            bool isHeader = true;
            foreach (var line in csv.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                if (isHeader) { isHeader = false; continue; }
                var fields = line.Trim().Split(',');
                if (fields.Length < 6) continue;
                categories.Add(new TrainCategory
                {
                    Id = NextId,
                    Name = fields[0],
                    Prefix = fields[1],
                    Suffix = fields[2],
                    IsPassenger = bool.Parse(fields[3]),
                    IsFreight = bool.Parse(fields[4]),
                    Color = fields[5]
                });
            }
            return categories;
        }
        catch
        {
            return [];
        }
    }

    private static int _nextId = -2000;
    private static int NextId => Interlocked.Decrement(ref _nextId);
}
