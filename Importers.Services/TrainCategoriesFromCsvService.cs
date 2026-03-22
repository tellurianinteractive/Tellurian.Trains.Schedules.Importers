using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services;

/// <summary>
/// Service for retrieving train category data from a CSV file.
/// </summary>
/// <param name="path">Optional path to the CSV file containing train category data. If not specified, defaults to 'CSV/TrainCategories.csv' in the application base directory.</param>
public class TrainCategoriesFromCsvService(string? path = null) : ITrainCategoriesService
{
    private readonly string _path = path ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CSV", "TrainCategories.csv");

    /// <summary>
    /// Retrieves all train categories from the configured CSV file asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of train categories.</returns>
    public async Task<IEnumerable<TrainCategory>> GetAllTrainCategoriesAsync()
    {
        var categories = new List<TrainCategory>(50);
        bool isHeaderLine = true;
        await foreach (var line in File.ReadLinesAsync(_path))
        {
            if (isHeaderLine) { isHeaderLine = false; continue; }
            categories.Add(FromCsvLine(line));
        }
        return categories;
    }

    private static TrainCategory FromCsvLine(string line)
    {
        var fields = line.Split(',');
        var name = fields[0];
        var prefix = fields[1];
        var suffix = fields[2];
        bool isPassenger = bool.Parse(fields[3]);
        bool isFreight = bool.Parse(fields[4]);
        string color = fields[5];
        return new TrainCategory()
        {
            Id = NextId,
            Name = name,
            Prefix = prefix,
            Suffix = suffix,
            IsPassenger = isPassenger,
            IsFreight = isFreight,
            Color = color
        };
    }
    static int _nextId = -1000;
    private static int NextId => Interlocked.Decrement(ref _nextId);
}
