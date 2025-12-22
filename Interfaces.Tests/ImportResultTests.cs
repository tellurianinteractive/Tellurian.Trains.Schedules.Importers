using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using Tellurian.Trains.Schedules.Importers.Model;
using Tellurian.Trains.Schedules.Importers.Services;
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

namespace Tellurian.Trains.Schedules.Importers.Interfaces.Tests;

[TestClass]
public class ImportResultTests
{
    [TestMethod]
    public async Task SerializeAndDeserialize()
    {
        var target = await ImportResult(Path.Combine("Test data", "Montan2023H0e.ods"));
        var json = target.Json();
        Assert.IsNotNull(json);
        var result = JsonSerializer.Deserialize<ImportResult<Schedule>>(json, JsonSerializerOptions);
        Assert.AreEqual(4, result.Messages.Length);
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task DeserializesJson()
    {
        var json = $$"""
            {
                "name":"files",
                "messages":[
                    {"text":"Läser arbetsblad StationTrack...","severity":1},
                    {"text":"Läser arbetsblad Routes...","severity":1},
                    {"text":"Läser arbetsblad Trains:traindef,timetable,remarks...","severity":1},
                    {"text":"Läser arbetsblad Trains:locomotive,trainset,job,remarks...","severity":1}],
                "isSuccess":true
            }
            """;
        var result = JsonSerializer.Deserialize<ImportResult<Schedule>>(json, JsonSerializerOptions);
        Assert.AreEqual(4, result.Messages.Length);
        Assert.IsTrue(result.IsSuccess);
    }

    static JsonSerializerOptions JsonSerializerOptions => new() { PropertyNameCaseInsensitive = true };

    static Task<ImportResult<Schedule>> ImportResult(string testFilePath)
    {
        var file = new FileInfo(testFilePath);
        if (file.Exists)
        {
            var provider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
            var operatingCompainesService = new OperatingCompaniesFromJsonService();

            using var importer = new XplnDataImporter(file, provider, operatingCompainesService, NullLogger<XplnDataImporter>.Instance);
            return importer.ImportSchedule(Path.GetFileNameWithoutExtension(testFilePath));
        }
        return Task.FromResult(ImportResult<Schedule>.Failure(Message.System($"File {testFilePath} not found.")));
    }
}

