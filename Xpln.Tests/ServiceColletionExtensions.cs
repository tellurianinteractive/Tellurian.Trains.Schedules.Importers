using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Services;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Tests;

internal static class ServiceColletionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddTestServices()
        {
            services.AddLogging(config =>
            {
                config.AddConsole();
                config.SetMinimumLevel(LogLevel.Debug); // Global minimum log level
                config.AddFilter("System", LogLevel.Information); // Filter for specific namespace
                config.AddFilter("Microsoft", LogLevel.Warning); // Filter for specific namespace
            });
            services.AddSingleton<IDataSetProvider, OdsDataSetProvider>();
            services.AddSingleton<IOperatingCompaniesService, OperatingCompaniesFromJsonService>();
            services.AddSingleton<ITrainCategoriesService, TrainCategoriesFromCsvService>();
            return services;
        }

        public static IServiceProvider CreateTestsServiceProvider() => new ServiceCollection().AddTestServices().BuildServiceProvider();
    }

}
