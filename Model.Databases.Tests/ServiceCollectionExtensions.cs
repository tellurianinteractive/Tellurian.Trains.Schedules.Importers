using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Services;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

namespace Tellurian.Trains.Schedules.Model.Databases.Tests;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestServices(this IServiceCollection services)
    {
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Debug);
            config.AddFilter("System", LogLevel.Information);
            config.AddFilter("Microsoft", LogLevel.Warning);
        });
        services.AddSingleton<IDataSetProvider, OdsDataSetProvider>();
        services.AddSingleton<ICompaniesService, CompaniesFromJsonService>();
        services.AddSingleton<ITrainCategoriesService, TrainCategoriesFromCsvService>();
        return services;
    }

    public static IServiceCollection AddDatabaseExportService(this IServiceCollection services, string databasePath)
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite($"DataSource={databasePath}")
            .Options;
        services.AddSingleton(options);
        services.AddSingleton<IExportService, DatabaseExportService>();
        return services;
    }

    public static IServiceProvider CreateTestServiceProvider(this IServiceCollection services) =>
        services.AddTestServices().BuildServiceProvider();
}
