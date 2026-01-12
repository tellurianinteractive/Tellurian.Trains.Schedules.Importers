using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using Tellurian.Trains.Schedules.Importers.Access.Extensions;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Validations;
using Tellurian.Utilities;

namespace Tellurian.Trains.Schedules.Importers.Xpln.Tests;

[TestClass]
public class XplnDataImporterTests
{
    const string FileSuffix = ".ods";

    private readonly IServiceProvider _serviceProvider = IServiceCollection.CreateTestsServiceProvider();
    private IDataSetProvider DataSetProvider => _serviceProvider.GetRequiredService<IDataSetProvider>();
    private ICompaniesService OperatingCompaniesService => _serviceProvider.GetRequiredService<ICompaniesService>();
    private ITrainCategoriesService TrainCategoriesService => _serviceProvider.GetRequiredService<ITrainCategoriesService>();
    private ILogger<XplnDataImporter> Logger => _serviceProvider.GetRequiredService<ILogger<XplnDataImporter>>();

    private DirectoryInfo? TestDocumentsDirectory;
    private readonly ValidationOptions ValidationOptions = new()
    {
        MaxTrainSpeedMetersPerClockMinute = 8.0,
        MinTrainSpeedMetersPerClockMinute = 0.3,
        ValidateDriverDuties = true,
        ValidateVehicleSchedules = true,
        ValidateStationCalls = true,
        ValidateStationTracks = true,
        ValidateStretches = true,
        ValidateTrainSpeed = true,
        ValidateTrainNumbers = true,
    };


    [TestInitialize]
    public void TestInitialize()
    {
        TestDocumentsDirectory = new DirectoryInfo("Test data");
    }

    [TestMethod]
    public async Task ImportsMemoryMappedFile()
    {
        using var m = MemoryMappedFile.CreateFromFile(Path.Combine(TestDocumentsDirectory!.FullName, "Montan2023H0e.ods"));
        var inputStream = m.CreateViewStream();

        using var importer = new XplnDataImporter(inputStream, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Montan2023H0e");
        if (result.IsFailure)
        {
            Assert.Fail();
        }

    }


    [TestMethod]
    public async Task ImportsGivskudModern2025()
    {
        await Import("Givskud-Modern-2025", "da-DK", 11, 125, 32, 8, 54, 73, 11, 1, 0);
    }

    [TestMethod()]
    [DataRow("Barmstedt2022", "de-DE", 14, 61, 18, 21, 14, 45, 10, 2)]
    [DataRow("DreamTrack2015", null, 12, 62, 24, 0, 0, 40, 11, 0)]
    [DataRow("FREMODERN-2023-Final-1-1", "da-DK", 14, 142, 58, 37, 0, 119, 14, 5)]
    [DataRow("FREMODERN-2023-Norge", "nb-NO", 10, 41, 13, 0, 0, 20, 10, 0)]
    [DataRow("Givskud2021", "da-DK", 25, 143, 49, 74, 80, 109, 25, 0)]
    [DataRow("H0e-Schutterwald2013", "de-DE", 10, 26, 6, 0, 20, 25, 10, 6)]
    [DataRow("Hellerup2015", "da-DK", 18, 60, 24, 0, 87, 20, 18, 2)]
    [DataRow("Kolding_Epoke_III_2022", "da-DK", 19, 60, 16, 15, 18, 38, 19, 10)]
    [DataRow("Kolding202009", "da-DK", 5, 38, 14, 2, 4, 28, 5, 0)]
    [DataRow("Kolding2022", "da-DK", 14, 73, 26, 6, 10, 55, 14, 0)]
    [DataRow("KoldingNorge2019", "nb-NO", 13, 56, 16, 0, 0, 56, 13, 1)]
    [DataRow("Langhurst 2019", "de-DE", 6, 15, 4, 7, 11, 4, 6, 25)]
    [DataRow("LTK2020", "de-DE", 0, 0, 0, 0, 0, 0, 0, 0, 18)]
    [DataRow("Magdeburg_v_DB33_DSB32_WTB11", "de-DE", 0, 0, 0, 0, 0, 0, 0, 0, 40)]
    [DataRow("Montan2023H0e", "de-DE", 5, 32, 3, 4, 24, 3, 5, 0)]
    [DataRow("Rotebro2015", "sv-SE", 12, 39, 15, 0, 0, 31, 12, 1)]
    [DataRow("Rotebro2016", "sv-SE", 16, 32, 12, 0, 0, 24, 16, 0)]
    [DataRow("Timmele2015", "sv-SE", 12, 37, 13, 0, 0, 33, 12, 6)]
    [DataRow("Värnamo2016", "sv-SE", 8, 40, 13, 0, 0, 27, 8, 0)]
    [DataRow("Värnamo2017", "sv-SE", 9, 40, 12, 0, 0, 29, 9, 0)]

    public async Task Import(string scheduleName, string? culture, int expectedTrackStretches, int expectedTrains, int expectedLocos, int expectedTrainsets, int expectedWagonGroups, int expectedDuties, int expectedDispatchStretches, int expectedValidationWarnings = 0, int expectedStoppingErrors = 0)
    {
        culture ??= "sv-SE";
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        if (IsScheduleFileExisting(scheduleName, out var file))
        {
            using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);

            var result = await importer.ImportScheduleAsync(scheduleName);
            if (result.IsFailure)
            {
                WriteLines(result.Messages.ToStrings(), file);
                Assert.AreEqual(expectedStoppingErrors, result.Messages.Count(m => m.Severity == Severity.Error), "Stopping errors");
                if (result.Name.IsAnyOf("LTK2020", "Magdeburg_v_DB33_DSB32_WTB11")) return; // these have severe errors.
                Assert.Fail("Stopping errors.");
            }
            else
            {
                var timetable = result.Item.Timetable;
                Assert.HasCount(expectedTrackStretches, result.Item.Timetable.Layout.TrackStretches, "TrackStreches");
                Assert.HasCount(expectedDispatchStretches, result.Item.Timetable.Layout.DispatchStretches, "DispatchStreches");
                Assert.HasCount(expectedTrains, timetable.Trains, "Trains");
                Assert.HasCount(expectedLocos, result.Item.Vehicles.Where(v => v.VehicleType == VehicleType.Locomotive), "Locos");
                Assert.HasCount(expectedTrainsets, result.Item.Vehicles.Where(v => v.VehicleType == VehicleType.Trainset), "Trainsets");
                Assert.AreEqual(expectedWagonGroups, timetable.Trains.Sum(t => t.WagonGroups.Count));
                Assert.HasCount(expectedDuties, result.Item.DriverDuties, "Duties");

                var validationErrors = result.Item.GetValidationErrors(ValidationOptions);
                WriteLines(result.Messages.ToStrings().Concat(validationErrors.ToStrings()), file);
                Assert.AreEqual(expectedValidationWarnings, validationErrors.Count(), "Validation warnings");
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
    public async Task ImportsGivskudModern2025ToDatabase() =>
        await ImportToDatabase("Givskud-Modern-2025", "C:\\Users\\Stefan\\OneDrive\\Modelljärnväg\\Träffar\\2025\\2025-04 Givskud\\Timetable.accdb");

    public async Task ImportToDatabase(string scheduleName, string databaseFilePath)
    {
        if (IsScheduleFileExisting(scheduleName, out var file))
        {
            using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
            var result = await importer.ImportScheduleAsync(scheduleName);
            if (result.IsSuccess)
            {
                result.Item.SaveToDatabase(databaseFilePath.ConnectionString());
            }
        }
    }
}
