using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Importers.Services.Tests;

[TestClass]
public class JsonExportServiceIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    private IServiceProvider _serviceProvider = null!;
    private IDataSetProvider DataSetProvider => _serviceProvider.GetRequiredService<IDataSetProvider>();
    private ICompaniesService CompaniesService => _serviceProvider.GetRequiredService<ICompaniesService>();
    private ITrainCategoriesService TrainCategoriesService => _serviceProvider.GetRequiredService<ITrainCategoriesService>();
    private ILogger<XplnDataImporter> Logger => _serviceProvider.GetRequiredService<ILogger<XplnDataImporter>>();

    [TestInitialize]
    public void TestInitialize()
    {
        _serviceProvider = new ServiceCollection().CreateTestServiceProvider();
        CultureInfo.CurrentCulture = new CultureInfo("de-DE");
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    [TestMethod]
    public async Task ImportsXplnAndExportsToJson()
    {
        // Arrange
        var scheduleName = "Barmstedt2022";
        var odsFile = new FileInfo(Path.Combine("Test data", $"{scheduleName}.ods"));
        Assert.IsTrue(odsFile.Exists, $"Test file {odsFile.FullName} not found");

        using var importer = new XplnDataImporter(odsFile, DataSetProvider, CompaniesService, TrainCategoriesService, Logger);
        var importResult = await importer.ImportScheduleAsync(scheduleName);
        Assert.IsTrue(importResult.IsSuccess, $"Import failed: {string.Join(", ", importResult.Messages.Select(m => m.ToString()))}");

        var schedule = importResult.Item;

        var jsonPath = $"{scheduleName}_{Guid.NewGuid():N}.json";
        var jsonFile = new FileInfo(jsonPath);

        // Act - Export to JSON using DI
        using var exportServiceProvider = new ServiceCollection()
            .AddJsonExportService(jsonFile)
            .BuildServiceProvider();
        var exportService = exportServiceProvider.GetRequiredService<IExportService>();
        var exportResult = await exportService.ExportScheduleAsync(schedule);

        // Assert - Verify export succeeded
        Assert.IsTrue(exportResult.IsSuccess, $"Export failed: {string.Join(", ", exportResult.Messages)}");
        Assert.IsTrue(File.Exists(jsonFile.FullName), $"JSON file was not created at {jsonFile.FullName}");
        Assert.IsTrue(new FileInfo(jsonFile.FullName).Length > 0, "JSON file is empty");
        Console.WriteLine($"JSON file created at: {jsonFile.FullName}");

        // Assert - Verify JSON structure
        var jsonContent = await File.ReadAllTextAsync(jsonFile.FullName);
        var jsonOptions = new JsonDocumentOptions { MaxDepth = 256 };
        using var document = JsonDocument.Parse(jsonContent, jsonOptions);
        var root = document.RootElement;

        // Verify schedule name is present
        Assert.IsTrue(root.TryGetProperty("Name", out var nameElement), "JSON missing 'Name' property");
        Assert.AreEqual(scheduleName, nameElement.GetString());

        // Verify timetable is present
        Assert.IsTrue(root.TryGetProperty("Timetable", out _), "JSON missing 'Timetable' property");

        // Verify vehicles is present
        Assert.IsTrue(root.TryGetProperty("ScheduledObjects", out _), "JSON missing 'ScheduledObjects' property");

        // Verify vehicle schedules is present
        Assert.IsTrue(root.TryGetProperty("Schedules", out _), "JSON missing 'Schedules' property");

        // Verify driver duties is present
        Assert.IsTrue(root.TryGetProperty("DriverDuties", out _), "JSON missing 'DriverDuties' property");

        Console.WriteLine($"JSON file validated successfully");
    }

    [TestMethod]
    public async Task ExportsToJsonAndImportsBack()
    {
        // Arrange - Import from XPLN
        var scheduleName = "Barmstedt2022";
        var odsFile = new FileInfo(Path.Combine("Test data", $"{scheduleName}.ods"));
        Assert.IsTrue(odsFile.Exists, $"Test file {odsFile.FullName} not found");

        using var importer = new XplnDataImporter(odsFile, DataSetProvider, CompaniesService, TrainCategoriesService, Logger);
        var xplnImportResult = await importer.ImportScheduleAsync(scheduleName);
        Assert.IsTrue(xplnImportResult.IsSuccess, $"XPLN import failed: {string.Join(", ", xplnImportResult.Messages.Select(m => m.ToString()))}");

        var originalSchedule = xplnImportResult.Item;

        var jsonPath = $"{scheduleName}_{Guid.NewGuid():N}.json";
        var jsonFile = new FileInfo(jsonPath);

        try
        {
            // Act - Export to JSON
            using var exportServiceProvider = new ServiceCollection()
                .AddJsonExportService(jsonFile)
                .BuildServiceProvider();
            var exportService = exportServiceProvider.GetRequiredService<IExportService>();
            var exportResult = await exportService.ExportScheduleAsync(originalSchedule);
            Assert.IsTrue(exportResult.IsSuccess, $"Export failed: {string.Join(", ", exportResult.Messages)}");

            // Act - Import from JSON
            jsonFile.Refresh(); // Refresh to get updated file info
            using var importServiceProvider = new ServiceCollection()
                .AddJsonImportService(jsonFile)
                .BuildServiceProvider();
            var jsonImportService = importServiceProvider.GetRequiredService<IImportService>();
            var jsonImportResult = await jsonImportService.ImportScheduleAsync(scheduleName);

            // Assert - Verify JSON import succeeded
            Assert.IsTrue(jsonImportResult.IsSuccess, $"JSON import failed: {string.Join(", ", jsonImportResult.Messages.Select(m => m.ToString()))}");

            var importedSchedule = jsonImportResult.Item;

            // Assert - Verify key properties match
            Assert.AreEqual(originalSchedule.Name, importedSchedule.Name, "Schedule name mismatch");
            Assert.AreEqual(originalSchedule.Timetable.Trains.Count, importedSchedule.Timetable.Trains.Count, "Train count mismatch");
            Assert.AreEqual(originalSchedule.Timetable.Layout.OperationLocations.Count, importedSchedule.Timetable.Layout.OperationLocations.Count, "Station count mismatch");
            Assert.AreEqual(originalSchedule.ScheduledObjects.Count, importedSchedule.ScheduledObjects.Count, "ScheduledObjects count mismatch");
            Assert.AreEqual(originalSchedule.Schedules.Count, importedSchedule.Schedules.Count, "Schedules count mismatch");
            Assert.AreEqual(originalSchedule.DriverDuties.Count, importedSchedule.DriverDuties.Count, "DriverDuties count mismatch");

            Console.WriteLine($"Round-trip JSON export/import completed successfully");
            Console.WriteLine($"  Trains: {importedSchedule.Timetable.Trains.Count}");
            Console.WriteLine($"  Stations: {importedSchedule.Timetable.Layout.OperationLocations.Count}");
            Console.WriteLine($"  ScheduledObjects: {importedSchedule.ScheduledObjects.Count}");
            Console.WriteLine($"  Schedules: {importedSchedule.Schedules.Count}");
            Console.WriteLine($"  DriverDuties: {importedSchedule.DriverDuties.Count}");
        }
        finally
        {
            // Cleanup
            if (File.Exists(jsonPath))
            {
                File.Delete(jsonPath);
            }
        }
    }
}
