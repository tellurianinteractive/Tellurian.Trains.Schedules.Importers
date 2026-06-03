using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Planning.App.Services;

public sealed class ScheduleImportService(
    ICompaniesService companiesService,
    ITrainCategoriesService categoriesService,
    ILoggerFactory loggerFactory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    public async Task<ImportResult<Schedule>> ImportFromJsonAsync(Stream stream, string name)
    {
        try
        {
            var schedule = await JsonSerializer.DeserializeAsync<Schedule>(stream, JsonOptions);
            if (schedule is null)
                return ImportResult<Schedule>.Failure(Message.System("Failed to deserialise JSON: result was null."));
            return ImportResult<Schedule>.Success(schedule);
        }
        catch (JsonException ex)
        {
            return ImportResult<Schedule>.Failure(Message.System($"JSON deserialisation error: {ex.Message}"));
        }
    }

    public async Task<ImportResult<Schedule>> ImportFromXplnAsync(Stream stream, string fileName)
    {
        try
        {
            IDataSetProvider provider = fileName.EndsWith(".ods", StringComparison.OrdinalIgnoreCase)
                ? new OdsDataSetProvider(loggerFactory.CreateLogger<OdsDataSetProvider>())
                : new XlsxDataSetProvider(loggerFactory.CreateLogger<XlsxDataSetProvider>());

            var logger = loggerFactory.CreateLogger<XplnDataImporter>();
            using var importer = new XplnDataImporter(stream, provider, companiesService, categoriesService, logger);
            var name = Path.GetFileNameWithoutExtension(fileName);
            return await importer.ImportScheduleAsync(name);
        }
        catch (Exception ex)
        {
            return ImportResult<Schedule>.Failure(Message.System($"XPLN import error: {ex.Message}"));
        }
    }
}
