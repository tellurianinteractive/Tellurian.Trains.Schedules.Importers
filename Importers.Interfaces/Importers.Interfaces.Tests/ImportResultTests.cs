using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using TimetablePlanning.Importers.Model;
using TimetablePlanning.Importers.Xpln;
using TimetablePlanning.Importers.Xpln.DataSetProviders;

namespace TimetablePlanning.Importers.Interfaces.Tests;

[TestClass]
public class ImportResultTests
{
    [TestMethod]
    public void SerializeAndDeserialize()
    {
        var target = ImportResult(@"..\..\..\..\..\Importers.Xpln\Importers.Tests\Test data\Montan2023H0e.ods");
        var json = target.Json();
        Assert.IsNotNull(json);
        var result = JsonSerializer.Deserialize<ImportResult<Schedule>>(json, JsonSerializerOptions);
        Assert.AreEqual(4, result.Messages.Length);
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void DeserializesJson()
    {
        var json = $$"""
            {
                "name":"files",
                "messages":[
                    {"text":"L�ser arbetsblad StationTrack...","severity":1},
                    {"text":"L�ser arbetsblad Routes...","severity":1},
                    {"text":"L�ser arbetsblad Trains:traindef,timetable,remarks...","severity":1},
                    {"text":"L�ser arbetsblad Trains:locomotive,trainset,job,remarks...","severity":1}],
                "isSuccess":true
            }
            """;
        var result = JsonSerializer.Deserialize<ImportResult<Schedule>>(json, JsonSerializerOptions);
        Assert.AreEqual(4, result.Messages.Length);
        Assert.IsTrue(result.IsSuccess);
    }

    static JsonSerializerOptions JsonSerializerOptions => new() { PropertyNameCaseInsensitive=true };

    static ImportResult<Schedule> ImportResult(string testFilePath)
    {
        var file = new FileInfo(testFilePath);
        if (file.Exists)
        {
            var provider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
            using var importer = new XplnDataImporter(file, provider, NullLogger<XplnDataImporter>.Instance);
            return importer.ImportSchedule(Path.GetFileNameWithoutExtension(testFilePath));
        }
        return ImportResult<Schedule>.Failure(Message.System($"File {testFilePath} not found."));
    }
}

