using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using Tellurian.Trains.Schedules.Importers.Access.Extensions;
using Tellurian.Trains.Schedules.Importers.Model;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Tests;

[TestClass]
public class XplnImporterTests
{
    const string FileSuffix = ".ods";
    private DirectoryInfo? TestDocumentsDirectory;
    private readonly ValidationOptions ValidationOptions = new()
    {
        MaxTrainSpeedMetersPerClockMinute = 8.0,
        MinTrainSpeedMetersPerClockMinute = 0.3,
        ValidateDriverDuties = true,
        ValidateLocoSchedules = true,
        ValidateStationCalls = true,
        ValidateStationTracks = true,
        ValidateStretches = true,
        ValidateTrainsetSchedules = true,
        ValidateTrainSpeed = true,
        ValidateTrainNumbers = true,

    };


    [TestInitialize]
    public void TestInitialize()
    {
        TestDocumentsDirectory = new DirectoryInfo("Test data");
    }

    [TestMethod]
    public void ImportsMemoryMappedFile()
    {
        using var m = MemoryMappedFile.CreateFromFile(Path.Combine(TestDocumentsDirectory!.FullName, "Montan2023H0e.ods"));
        var inputStream = m.CreateViewStream();
        var dataSetProvider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
        using var importer = new XplnDataImporter(inputStream, dataSetProvider, NullLogger<XplnDataImporter>.Instance);
        var result = importer.ImportSchedule("Montan2023H0e");
        if (result.IsFailure)
        {
            Assert.Fail();
        }

    }

    [TestMethod]
    public void Imports()
    {
        Import("Magdeburg_v_DB33_DSB32_WTB11", "de-DE", 142, 58, 77, 119, 0, 40);
    }

    [TestMethod]
    public void ImportsGivskudModern2025()
    {
        Import("Givskud-Modern-2025", "da-DK", 125, 32, 50, 73, 1, 0);
    }

    [TestMethod()]
    [DataRow("Montan2023H0e", "de-DE", 32, 3, 28, 3, 0)]
    [DataRow("Barmstedt2022", "de-DE", 61, 18, 36, 45, 2)]
    [DataRow("Givskud2021", "da-DK", 32, 3, 28, 3, 0, 7)]
    [DataRow("Kolding_Epoke_III_2022", "da-DK", 60, 16, 32, 38, 6)]
    [DataRow("Langhurst 2019", "de-DE", 15, 4, 22, 4, 25)]
    [DataRow("Kolding2022", "da-DK", 73, 26, 55, 55, 0)]
    [DataRow("Kolding202009", "da-DK", 38, 14, 36, 28, 0)]
    [DataRow("KoldingNorge2019", "no-NO", 56, 16, 0, 56, 1)]
    [DataRow("Värnamo2017", "sv-SE", 40, 12, 0, 29, 0)]
    [DataRow("Värnamo2016", "sv-SE", 40, 13, 0, 27, 0)]
    [DataRow("Rotebro2016", "sv-SE", 32, 12, 0, 24, 0)]
    [DataRow("Rotebro2015", "sv-SE", 39, 15, 0, 31, 1)]
    [DataRow("Timmele2015", "sv-SE", 37, 13, 0, 33, 4)]
    [DataRow("Hellerup2015", "da-DK", 60, 24, 57, 20, 0)]
    [DataRow("DreamTrack2015", null, 62, 24, 0, 40, 0)]
    [DataRow("H0e-Schutterwald2013", "de-DE", 26, 6, 20, 25, 6)]
    [DataRow("LTK2020", "de-DE", 15, 4, 22, 4, 25, 18)]
    [DataRow("FREMODERN-2023-Final-1-1", "da-DK", 142, 58, 77, 119, 0, 0)]

    public void Import(string scheduleName, string? culture, int expectedTrains, int expectedLocos, int expectedTrainsets, int expectedDuties, int expectedValidationWarnings = 0, int expectedStoppingErrors = 0)
    {
        culture ??= "sv-SE";
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        if (IsScheduleFileExisting(scheduleName , out var file))
        {
            var dataSetProvider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
            using var importer = new XplnDataImporter(file, dataSetProvider, NullLogger<XplnDataImporter>.Instance);
            var result = importer.ImportSchedule(scheduleName);
            if (result.IsFailure)
            {
                WriteLines(result.Messages.ToStrings(), file);
                Assert.AreEqual(expectedStoppingErrors, result.Messages.Count(m => m.Severity == Severity.Error), "Stopping errors");
            }
            else
            {
                var timetable = result.Item.Timetable;
                Assert.HasCount(expectedTrains, timetable.Trains, "Trains");
                Assert.HasCount(expectedLocos, result.Item.LocoSchedules, "Locos");
                Assert.HasCount(expectedTrainsets, result.Item.TrainsetSchedules, "Trainsets");
                Assert.HasCount(expectedDuties, result.Item.DriverDuties, "Duties");

                var validationErrors = result.Item.GetValidationErrors(ValidationOptions);
                WriteLines(result.Messages.ToStrings().Concat(validationErrors.ToStrings()), file);
                Assert.AreEqual(expectedValidationWarnings, validationErrors.Count(), "Validation errors");
            }

        }
        else
        {
            Assert.Fail($"File {scheduleName}.ODS is not found. Forget setting to copy to output?");
        }
    }

    private static void WriteLines(IEnumerable<string> messages, FileInfo file)
    {

        using var writer = new StreamWriter(file.FullName.Replace(FileSuffix, "Log.txt"));
        writer.WriteLine($"Validation at {DateTime.Now}");
        foreach (var message in messages) writer.WriteLine(message);
        writer.WriteLine("Validation completed.");
    }

    private bool IsScheduleFileExisting(string name, [NotNullWhen(true)] out FileInfo? file)
    {
        if (string.IsNullOrWhiteSpace(name)) { file = null; return false; }
        var filePathName = Path.Combine(TestDocumentsDirectory?.FullName ?? "", name + FileSuffix);
        if (!File.Exists(filePathName)) { file = null; return false; }
        file = new FileInfo(filePathName);
        return true;
    }

    [TestMethod, Ignore("Only used for special cases.")]
    public void ImportsGivskudModern2025ToDatabase() => ImportToDatabase("Givskud-Modern-2025", "C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2025\\2025-04 Givskud\\Timetable.accdb");

    public void ImportToDatabase(string scheduleName, string databaseFilePath)
    {
        if (IsScheduleFileExisting(scheduleName, out var file))
        {
            var dataSetProvider = new OdsDataSetProvider(NullLogger<OdsDataSetProvider>.Instance);
            using var importer = new XplnDataImporter(file, dataSetProvider, NullLogger<XplnDataImporter>.Instance);
            var result = importer.ImportSchedule(scheduleName);
            if (result.IsSuccess) {
                result.Item.SaveToDatabase(databaseFilePath.ConnectionString());
            }
        }
    }
}
