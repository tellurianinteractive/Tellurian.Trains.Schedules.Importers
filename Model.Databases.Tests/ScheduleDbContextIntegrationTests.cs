using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;

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
        Assert.IsNotNull(context.ScheduledObjects);
        Assert.IsNotNull(context.ScheduleAssignments);
        Assert.IsNotNull(context.Schedules);
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
        var company = new Company(1, "Test Company", "TST", countryId: 4); // 4 = Germany
        layout.Add(company);

        var station = new Station(1, "Test Station", "TS");
        layout.Add(station);

        var track = new StationTrack(1, "1");
        station.Add(track);

        context.Layouts.Add(layout);
        await context.SaveChangesAsync(CancellationToken);

        // Assert - Query back
        var savedLayout = await context.Layouts
            .Include(l => l.Companies)
            .Include(l => l.OperationLocations)
            .ThenInclude(s => s.Tracks)
            .FirstAsync(CancellationToken);

        Assert.AreEqual("Test Layout", savedLayout.Name);
        Assert.AreEqual(1, savedLayout.Companies.Count);
        Assert.AreEqual("Test Company", savedLayout.Companies.First().Name);
        Assert.AreEqual(4, savedLayout.Companies.First().CountryId);
        Assert.AreEqual(1, savedLayout.OperationLocations.Count);
        Assert.AreEqual("Test Station", savedLayout.OperationLocations.First().Name);
        Assert.AreEqual(1, savedLayout.OperationLocations.First().Tracks.Count);
    }

    [TestMethod]
    public async Task RoundTripsLayoutAndStationRegions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ScheduleDbContext(options);
        await context.Database.OpenConnectionAsync(CancellationToken);
        await context.Database.EnsureCreatedAsync(CancellationToken);

        // The layout owns a region catalogue; a station is assigned a subset of it.
        var layout = new Layout { Id = 1, Name = "L" };
        var south = layout.Add(new Region { Id = 1, Name = "Söder", CountryId = 1, BackgroundColor = "#ffff00" });
        layout.Add(new Region { Id = 2, Name = "Norr", CountryId = 1, BackgroundColor = "#0066FF" });

        var station = new Station(1, "Shadow", "SH") { IsShadow = true };
        station.Add(south);
        layout.Add(station);

        // Act
        context.Layouts.Add(layout);
        await context.SaveChangesAsync(CancellationToken);
        context.ChangeTracker.Clear();

        // Assert - the catalogue (Layout.Regions) and the assignment (Station.Regions) both persist.
        var loadedLayout = await context.Layouts.Include(l => l.Regions).FirstAsync(CancellationToken);
        Assert.AreEqual(2, loadedLayout.Regions.Count, "Layout region catalogue");

        var loadedStation = await context.Stations.Include(s => s.Regions).FirstAsync(CancellationToken);
        Assert.AreEqual(1, loadedStation.Regions.Count, "Station region assignment");
        Assert.AreEqual("Söder", loadedStation.Regions.First().Name);
        Assert.AreEqual(1, loadedStation.Regions.First().CountryId);
        Assert.AreEqual("#ffff00", loadedStation.Regions.First().BackgroundColor);
    }

    [TestMethod]
    public async Task RoundTripsTrainPartOptions()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ScheduleDbContext(options);
        await context.Database.OpenConnectionAsync(CancellationToken);
        await context.Database.EnsureCreatedAsync(CancellationToken);

        var layout = new Layout { Id = 1, Name = "L" };
        var station = new Station(1, "Station", "S");
        var track = new StationTrack(1, "1");
        station.Add(track);
        layout.Add(station);

        var timetable = new Timetable("T", layout);
        var train = new Train(1, 100) { Timetable = timetable, TimetableId = timetable.Id };
        train.Add(new StationCall(1, track, Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 0)));
        train.Add(new StationCall(2, track, Time.FromHourAndMinute(8, 30), Time.FromHourAndMinute(8, 30)));

        var part = train.AsTrainPart(0, 1);
        part.Id = 1;
        part.TractionOptions = new TractionOptions { HasCoupleNote = true, NumberOfUnits = 2, TurnLoco = true };
        part.WagonSetOptions = new WagonSetOptions { OrderInTrain = 3 };
        part.CargoOnlyOptions = new CargoOnlyOptions { CargoName = "Coal", HasCoupleNote = true };

        // Act
        context.Layouts.Add(layout);
        context.Timetables.Add(timetable);
        context.Trains.Add(train);
        context.TrainParts.Add(part);
        await context.SaveChangesAsync(CancellationToken);
        context.ChangeTracker.Clear();

        var loaded = await context.TrainParts
            .OfType<ScheduledTrainPart>()
            .Include(p => p.WagonSetOptions!)
            .FirstAsync(CancellationToken);

        // Assert - each option kind persisted and read back from SQLite
        Assert.IsNotNull(loaded.TractionOptions, "TractionOptions");
        Assert.AreEqual(2, loaded.TractionOptions!.NumberOfUnits);
        Assert.IsTrue(loaded.TractionOptions.TurnLoco);
        Assert.IsTrue(loaded.TractionOptions.HasCoupleNote);

        Assert.IsNotNull(loaded.WagonSetOptions, "NonTractionOptions");
        Assert.AreEqual(3, loaded.WagonSetOptions!.OrderInTrain);

        Assert.IsNotNull(loaded.CargoOnlyOptions, "CargoOnlyOptions");
        Assert.AreEqual("Coal", loaded.CargoOnlyOptions!.CargoName);
        Assert.IsTrue(loaded.CargoOnlyOptions.Load, "Load is computed from HasCoupleNote");
    }

    [TestMethod]
    public async Task RoundTripsWagonsetWagons()
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new ScheduleDbContext(options);
        await context.Database.OpenConnectionAsync(CancellationToken);
        await context.Database.EnsureCreatedAsync(CancellationToken);

        var layout = new Layout { Id = 1, Name = "L" };
        var timetable = new Timetable("T", layout);
        var plan = new Plan("P", timetable) { Id = 1 };
        var wagonset = new ScheduledObject(1, ScheduledObjectType.Wagonset, 1) { PlanId = plan.Id, Plan = plan };
        wagonset.AddWagon("Gbs", "1234", isCargo: true);
        wagonset.AddWagon("Habis", isCargo: true);
        plan.ScheduledObjects.Add(wagonset);

        // Act
        context.Plans.Add(plan);
        await context.SaveChangesAsync(CancellationToken);
        context.ChangeTracker.Clear();

        var loaded = await context.ScheduledObjects
            .Include(e => e.Units)
            .FirstAsync(e => e.ObjectType == ScheduledObjectType.Wagonset, CancellationToken);

        // Assert - the ordered rake persisted and read back from its own table
        var wagons = loaded.Units.OrderBy(w => w.Position).ToList();
        Assert.HasCount(2, wagons);
        Assert.AreEqual("Gbs", wagons[0].Class);
        Assert.AreEqual("1234", wagons[0].Number);
        Assert.AreEqual(2, wagons[1].Position);
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
        Assert.AreEqual(18, schedule.ScheduledObjects.Count(v => v.ObjectType == ScheduledObjectType.Locomotive), "Expected 18 loco schedules");
        Assert.AreEqual(21, schedule.ScheduledObjects.Count(v => v.ObjectType == ScheduledObjectType.Wagonset), "Expected 21 wagonset schedules");
        Assert.AreEqual(45, schedule.DriverDuties.Count, "Expected 45 driver duties");
        Assert.AreEqual(14, schedule.ScheduledObjects.Count(v => v.ObjectType == ScheduledObjectType.CargoFlow), "Expected 14 cargo flows");
    }

    [TestMethod]
    public async Task GraphicTimetableSettingsSurviveBrowserStorageRoundTripOfRealPlan()
    {
        // Mirrors Planning.App ScheduleStateService: serialise the whole imported Plan with the same
        // options the browser store uses, then restore it, and confirm an edited graphical setting
        // survives (and that the full real graph serialises without throwing).
        var options = new System.Text.Json.JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            MaxDepth = 256
        };
        var odsFile = new FileInfo(Path.Combine("Test data", "Barmstedt2022.ods"));
        using var importer = new XplnDataImporter(odsFile, DataSetProvider, CompaniesService, TrainCategoriesService, Logger);
        var schedule = (await importer.ImportScheduleAsync("Barmstedt2022")).Item;
        Assert.IsNotNull(schedule);

        schedule.Timetable.Layout.Settings.GraphicTimetable.KilometerSpacing = 99;

        var json = System.Text.Json.JsonSerializer.Serialize(schedule, options);
        var restored = System.Text.Json.JsonSerializer.Deserialize<Plan>(json, options);

        Assert.IsNotNull(restored);
        Assert.AreEqual(99, restored.Timetable.Layout.Settings.GraphicTimetable.KilometerSpacing);
    }

    [TestMethod]
    public async Task TextCallNotesKeepTheirTextThroughWholePlanRoundTrip()
    {
        // Decides whether empty-Texts notes come from the round-trip itself (a live bug) or only from
        // legacy data: import a real plan with notes, round-trip the whole graph, and confirm a note's
        // text survives. If this passes, the round-trip is sound and empty Texts is pre-existing data.
        var options = new System.Text.Json.JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
            MaxDepth = 256
        };
        var odsFile = new FileInfo(Path.Combine("Test data", "Barmstedt2022.ods"));
        using var importer = new XplnDataImporter(odsFile, DataSetProvider, CompaniesService, TrainCategoriesService, Logger);
        var schedule = (await importer.ImportScheduleAsync("Barmstedt2022")).Item;
        Assert.IsNotNull(schedule);

        var noteBefore = schedule.Timetable.Trains
            .SelectMany(t => t.Calls).SelectMany(c => c.Notes)
            .FirstOrDefault(n => !string.IsNullOrEmpty(n.Text));
        Assert.IsNotNull(noteBefore, "Expected at least one non-empty note in the imported plan.");
        var expected = noteBefore.Text;

        var restored = System.Text.Json.JsonSerializer.Deserialize<Plan>(
            System.Text.Json.JsonSerializer.Serialize(schedule, options), options);

        var matching = restored!.Timetable.Trains
            .SelectMany(t => t.Calls).SelectMany(c => c.Notes)
            .Count(n => n.Text == expected);
        Assert.IsTrue(matching > 0, $"Note text '{expected}' was lost during the whole-plan round-trip.");
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
        var savedSchedule = await context.Plans.FirstAsync(CancellationToken);
        Assert.IsNotNull(savedSchedule);
        Assert.AreEqual(scheduleName, savedSchedule.Name);
    }
}
