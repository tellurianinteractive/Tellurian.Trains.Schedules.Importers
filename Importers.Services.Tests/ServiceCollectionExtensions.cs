using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

namespace Tellurian.Trains.Schedules.Importers.Services.Tests;

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

    public static IServiceCollection AddJsonExportService(this IServiceCollection services, FileInfo destination)
    {
        services.AddSingleton<IExportService>(new JsonExportService(destination));
        return services;
    }

    public static IServiceCollection AddJsonImportService(this IServiceCollection services, FileInfo source)
    {
        services.AddSingleton<IImportService>(new JsonImportService(source));
        return services;
    }

    public static IServiceProvider CreateTestServiceProvider(this IServiceCollection services) =>
        services.AddTestServices().BuildServiceProvider();
}
