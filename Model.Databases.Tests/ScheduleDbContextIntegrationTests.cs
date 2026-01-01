using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Model;

namespace Tellurian.Trains.Schedules.Model.Databases.Tests;

[TestClass]
public class ScheduleDbContextIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;
    private CancellationToken CancellationToken => TestContext.CancellationToken;

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
    public async Task CanCreateDatabaseSchema()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        // Act
        await using var context = new ScheduleDbContext(options);
        await context.Database.OpenConnectionAsync(CancellationToken);
        await context.Database.EnsureCreatedAsync(CancellationToken);

        // Assert - Verify all DbSets are accessible
        Assert.IsNotNull(context.Layouts);
        Assert.IsNotNull(context.Companies);
        Assert.IsNotNull(context.OperationLocations);
        Assert.IsNotNull(context.StationTracks);
        Assert.IsNotNull(context.TrackStretches);
        Assert.IsNotNull(context.TimetableStretches);
        Assert.IsNotNull(context.Timetables);
        Assert.IsNotNull(context.TrainCategories);
        Assert.IsNotNull(context.Trains);
        Assert.IsNotNull(context.StationCalls);
        Assert.IsNotNull(context.Schedules);
        Assert.IsNotNull(context.Vehicles);
        Assert.IsNotNull(context.VehicleScheduleAssignments);
        Assert.IsNotNull(context.VehicleSchedules);
        Assert.IsNotNull(context.DriverDuties);
        Assert.IsNotNull(context.TrainParts);
        Assert.IsNotNull(context.CallNotes);
        Assert.IsNotNull(context.TextCallNotes);
        Assert.IsNotNull(context.DriverDutyNotes);
    }

    [TestMethod]
    public async Task CanSaveAndQueryBasicEntities()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ScheduleDbContext(options);
        await context.Database.OpenConnectionAsync(CancellationToken);
        await context.Database.EnsureCreatedAsync(CancellationToken);

        // Act - Create and save a simple layout with a company and station
        var layout = new Layout { Id = 1, Name = "Test Layout" };
        var company = new Company(1, "Test Company", "TST", "DE");
        layout.Add(company);

        var station = new OperationLocation(1, "Test Station", "TS");
        layout.Add(station);

        var track = new StationTrack(1, "1");
        station.Add(track);

        context.Layouts.Add(layout);
        await context.SaveChangesAsync(CancellationToken);

        // Assert - Query back
        var savedLayout = await context.Layouts
            .Include(l => l.Companies)
            .Include(l => l.Stations)
            .ThenInclude(s => s.Tracks)
            .FirstAsync(CancellationToken);

        Assert.AreEqual("Test Layout", savedLayout.Name);
        Assert.AreEqual(1, savedLayout.Companies.Count);
        Assert.AreEqual("Test Company", savedLayout.Companies.First().Name);
        Assert.AreEqual(1, savedLayout.Stations.Count);
        Assert.AreEqual("Test Station", savedLayout.Stations.First().Name);
        Assert.AreEqual(1, savedLayout.Stations.First().Tracks.Count);
    }

    [TestMethod]
    public async Task CanImportFromXpln()
    {
        // Arrange - verify we can import from ODS file
        var scheduleName = "Barmstedt2022";
        var odsFile = new FileInfo(Path.Combine("Test data", $"{scheduleName}.ods"));
        Assert.IsTrue(odsFile.Exists, $"Test file {odsFile.FullName} not found");

        // Act - Import from ODS
        using var importer = new XplnDataImporter(odsFile, DataSetProvider, CompaniesService, TrainCategoriesService, Logger);
        var importResult = await importer.ImportScheduleAsync(scheduleName);

        // Assert - Verify import succeeded and has expected data
        Assert.IsTrue(importResult.IsSuccess, $"Import failed: {string.Join(", ", importResult.Messages.Select(m => m.ToString()))}");

        var schedule = importResult.Item;
        Assert.IsNotNull(schedule);
        Assert.IsNotNull(schedule.Timetable);
        Assert.IsNotNull(schedule.Timetable.Layout);

        // Verify expected counts from Barmstedt2022
        Assert.AreEqual(61, schedule.Timetable.Trains.Count, "Expected 61 trains");
        Assert.AreEqual(18, schedule.Vehicles.Count(v => v.VehicleType == VehicleType.Locomotive), "Expected 18 loco schedules");
        Assert.AreEqual(21, schedule.Vehicles.Count(v => v.VehicleType == VehicleType.Trainset), "Expected 21 trainset schedules");
        Assert.AreEqual(45, schedule.DriverDuties.Count, "Expected 45 driver duties");
        Assert.AreEqual(14, schedule.Timetable.Trains.Sum(t => t.WagonGroups.Count), "Expected 14 wagon groups");
    }

    [TestMethod]
    public async Task ImportsXplnAndSavesToSqlite()
    {
        // Arrange
        var scheduleName = "Barmstedt2022";
        var odsFile = new FileInfo(Path.Combine("Test data", $"{scheduleName}.ods"));

        using var importer = new XplnDataImporter(odsFile, DataSetProvider, CompaniesService, TrainCategoriesService, Logger);
        var importResult = await importer.ImportScheduleAsync(scheduleName);
        Assert.IsTrue(importResult.IsSuccess);

        var schedule = importResult.Item;

        var dbPath = Path.Combine(TestContext.TestRunResultsDirectory ?? ".", $"{scheduleName}.db");
        if (File.Exists(dbPath)) File.Delete(dbPath);

        // Create service provider with export service configured for this database
        using var exportServiceProvider = new ServiceCollection()
            .AddDatabaseExportService(dbPath)
            .BuildServiceProvider();

        // Act
        var exportService = exportServiceProvider.GetRequiredService<IExportService>();
        var exportResult = await exportService.ExportScheduleAsync(schedule);

        // Assert
        Assert.IsTrue(exportResult.IsSuccess, $"Export failed: {string.Join(", ", exportResult.Messages)}");
        Console.WriteLine($"SQLite database created at: {dbPath}");

        var options = exportServiceProvider.GetRequiredService<DbContextOptions<ScheduleDbContext>>();
        await using var context = new ScheduleDbContext(options);
        var savedSchedule = await context.Schedules.FirstAsync(CancellationToken);
        Assert.IsNotNull(savedSchedule);
        Assert.AreEqual(scheduleName, savedSchedule.Name);
    }
}
