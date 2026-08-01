using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.IO.MemoryMappedFiles;
using Tellurian.Trains.Schedules.Importers.Access.Extensions;
using Tellurian.Trains.Schedules.Importers.Interfaces;
using Tellurian.Trains.Schedules.Importers.Xpln.DataSetProviders;
using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Settings;
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
    private readonly ValidationSettings ValidationSettings = new()
    {
        MaxTrainSpeedMetersPerClockMinute = 8.0,
        MinTrainSpeedMetersPerClockMinute = 0.3,
        ValidateDriverDuties = true,
        ValidateSchedules = true,
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
        Assert.IsTrue(IsScheduleFileExisting("Montan2023H0e", out var file));
        using var m = MemoryMappedFile.CreateFromFile(file.FullName);
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
        await Import("Givskud-Modern-2025", 11, 125, 32, 8, 54, 73, 11, 43, 0);
    }

    [TestMethod]
    public async Task AssignsFallbackNumberToVehiclesWithoutParseableNumber()
    {
        // Barmstedt2022 has locomotives whose identifier has no number (e.g. "DB_GLok"). These must still
        // get a distinct, non-zero number, falling back to the vehicle's unique id.
        Assert.IsTrue(IsScheduleFileExisting("Barmstedt2022", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Barmstedt2022");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        Assert.IsFalse(result.Item.ScheduledObjects.Any(v => v.Number == 0), "No vehicle should have number 0.");
    }

    [TestMethod]
    public async Task ImportedDutyPartsReferenceTheSharedScheduleParts()
    {
        // A driver duty is a sequence of the train parts already defined in the vehicle schedules. The Job
        // import must link each duty segment to the existing traction schedule part(s), not mint a private
        // copy — otherwise a duty part has no schedule and its traction unit cannot be resolved.
        Assert.IsTrue(IsScheduleFileExisting("Rotebro2015", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Rotebro2015");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");
        var plan = result.Item;

        var scheduleParts = plan.Schedules.SelectMany(s => s.Parts).ToList();
        var schedulePartInstances = new HashSet<ScheduledTrainPart>(scheduleParts, ReferenceEqualityComparer.Instance);
        var tractionScheduleParts = scheduleParts
            .Where(sp => plan.ScheduledObjectsFor(sp).Any(so => so.IsTraction))
            .ToList();
        var dutyParts = plan.DriverDuties.SelectMany(d => d.Parts).ToList();

        Assert.IsNotEmpty(dutyParts, "The imported duties should have parts.");
        Assert.IsTrue(dutyParts.Any(dp => schedulePartInstances.Contains(dp)),
            "Imported duties must reference the schedule parts, not standalone copies.");
        foreach (var dutyPart in dutyParts)
        {
            var duplicate = tractionScheduleParts.FirstOrDefault(sp => !ReferenceEquals(sp, dutyPart) && sp.Equals(dutyPart));
            Assert.IsNull(duplicate, $"Duty part {dutyPart} duplicates a traction schedule part instead of referencing it.");
        }
    }

    [TestMethod]
    public async Task MergesLocomotiveAndTrainsetWithSameIdIntoRailcar()
    {
        // In Kolding202009 the Swedish self-propelled X2000 is listed under both the locomotive and the
        // trainset section with the same identifier. It must become a single railcar, not two vehicles.
        Assert.IsTrue(IsScheduleFileExisting("Kolding202009", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Kolding202009");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var x2000 = result.Item.ScheduledObjects.Where(v => string.Equals(v.ExternalId, "X2000", StringComparison.OrdinalIgnoreCase)).ToArray();
        Assert.HasCount(1, x2000, "X2000 vehicles");
        Assert.AreEqual(ScheduledObjectType.Trainset, x2000[0].ObjectType, "X2000 vehicle type");
    }

    [TestMethod]
    public async Task PopulatesTimetableCatalogueWithExactlyTheCategoriesUsedByTrains()
    {
        // The colour each train is drawn with rides on Train.Category, but the timetable's category
        // catalogue (shown on the Train Categories tab) must also list those same categories - no more,
        // no less - and each must have a distinct id so trains can reference it without collision.
        Assert.IsTrue(IsScheduleFileExisting("Barmstedt2022", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Barmstedt2022");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var timetable = result.Item.Timetable;
        var usedByTrains = timetable.Trains.Select(t => t.Category).OfType<TrainCategory>().Distinct().ToArray();
        Assert.IsNotEmpty(usedByTrains, "At least one train should have a category.");

        var catalogue = timetable.TrainCategories.ToArray();
        Assert.IsNotEmpty(catalogue, "The timetable's category catalogue should be populated.");
        CollectionAssert.AreEquivalent(usedByTrains, catalogue, "Catalogue should contain exactly the categories used by trains.");
        Assert.AreEqual(catalogue.Length, catalogue.Select(c => c.Id).Distinct().Count(), "Every catalogue category should have a distinct id.");
    }

    [TestMethod]
    public async Task AssignsDistinctIdsToImportedCompanies()
    {
        // Givskud-Modern-2025 references operating companies that are not in the catalogue (e.g. CFLDK,
        // DBSCH). Each discovered company must get a distinct id; otherwise trains and categories that
        // reference a company by id collide (selecting one resolves to the first with the shared id).
        Assert.IsTrue(IsScheduleFileExisting("Givskud-Modern-2025", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Givskud-Modern-2025");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var companies = result.Item.Layout.Companies.ToArray();
        Assert.IsNotEmpty(companies, "The layout should have operating companies.");
        Assert.AreEqual(companies.Length, companies.Select(c => c.Id).Distinct().Count(), "Every company should have a distinct id.");
    }

    [TestMethod]
    public async Task GroupsRoutesIntoTimetableStretchesWhenStartPositionIsZero()
    {
        // In XPLN the Routeid column is unique per row, so a timetable stretch (line) is delimited
        // by the start position resetting to zero. Barmstedt2022 has two lines: Vta..Ead and Ando..Ost.
        Assert.IsTrue(IsScheduleFileExisting("Barmstedt2022", out var file));

        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Barmstedt2022");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var stretches = result.Item.Timetable.Layout.TimetableStretches.ToArray();
        Assert.HasCount(2, stretches, "TimetableStretches");

        Assert.HasCount(7, stretches[0].Stretches, "First line track stretches");
        Assert.AreEqual("Vta", stretches[0].Starts.Signature, ignoreCase: true);
        Assert.AreEqual("EAD", stretches[0].Ends.Signature, ignoreCase: true);

        Assert.HasCount(7, stretches[1].Stretches, "Second line track stretches");
        Assert.AreEqual("ANDO", stretches[1].Starts.Signature, ignoreCase: true);
        Assert.AreEqual("OST", stretches[1].Ends.Signature, ignoreCase: true);
    }

    [TestMethod]
    public async Task GroupsRoutesByRouteIdWhenRouteIdIsShared()
    {
        // Värnamo2017 reuses the Routeid column to group segments into three lines, and its start
        // positions do not reset to zero. The Routeid grouping must take precedence over the position rule.
        Assert.IsTrue(IsScheduleFileExisting("Värnamo2017", out var file));

        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Värnamo2017");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var stretches = result.Item.Timetable.Layout.TimetableStretches.ToArray();
        Assert.HasCount(3, stretches, "TimetableStretches");

        Assert.AreEqual("GBG", stretches[0].Starts.Signature, ignoreCase: true);
        Assert.AreEqual("CSN", stretches[0].Ends.Signature, ignoreCase: true);
        Assert.AreEqual("VST", stretches[1].Starts.Signature, ignoreCase: true);
        Assert.AreEqual("GMJ", stretches[1].Ends.Signature, ignoreCase: true);
        Assert.AreEqual("RSE", stretches[2].Starts.Signature, ignoreCase: true);
        Assert.AreEqual("THG", stretches[2].Ends.Signature, ignoreCase: true);
    }

    [TestMethod]
    public async Task GroupsByStationContinuityWhenRouteIdDoesNotDistinguishLines()
    {
        // Montan2023H0e has Routeid = "1" on every row, which cannot distinguish lines, so grouping
        // falls back to station continuity: the chain breaks at SBG -> HIT, giving two lines.
        Assert.IsTrue(IsScheduleFileExisting("Montan2023H0e", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Montan2023H0e");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var stretches = result.Item.Timetable.Layout.TimetableStretches.ToArray();
        Assert.HasCount(2, stretches, "TimetableStretches");
        Assert.AreEqual("HIT-S31-HLS-HMU-SBG", string.Join("-", stretches[0].Stations.Select(s => s.Signature)), ignoreCase: true);
        Assert.AreEqual("HIT-WHM", string.Join("-", stretches[1].Stations.Select(s => s.Signature)), ignoreCase: true);
    }

    [TestMethod]
    public async Task SortsRouteIdGroupedLinksByStartPosition()
    {
        // Rotebro2015 groups by Routeid (10/20/30). Line 30's links are listed in reverse position
        // order (Bgs@30 then Brg@27), so they must be sorted by start position into Brg-Bgs-Ccw.
        Assert.IsTrue(IsScheduleFileExisting("Rotebro2015", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Rotebro2015");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var stretches = result.Item.Timetable.Layout.TimetableStretches.ToArray();
        Assert.HasCount(3, stretches, "TimetableStretches");
        var line30 = stretches.Single(s => s.Number == "30");
        Assert.AreEqual("Brg-Bgs-Ccw", string.Join("-", line30.Stations.Select(s => s.Signature)), ignoreCase: true);
    }

    [TestMethod]
    public async Task GroupsByStationContinuityAcrossNonZeroLineStarts()
    {
        // Givskud-Modern-2025 has unique per-row Routeid, so it uses station continuity. Line 2 starts
        // at Frw (non-zero position) and its positions then decrease, which the old position rules broke.
        Assert.IsTrue(IsScheduleFileExisting("Givskud-Modern-2025", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Givskud-Modern-2025");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var stretches = result.Item.Timetable.Layout.TimetableStretches.ToArray();
        Assert.HasCount(3, stretches, "TimetableStretches");
        Assert.AreEqual("Ing-Rot-Sa-Frw-Mkd", string.Join("-", stretches[0].Stations.Select(s => s.Signature)), ignoreCase: true);
        Assert.AreEqual("Frw-Pa-Muk-Grp-Vdp-Sti", string.Join("-", stretches[1].Stations.Select(s => s.Signature)), ignoreCase: true);
        Assert.AreEqual("Muk-Hov-Grb", string.Join("-", stretches[2].Stations.Select(s => s.Signature)), ignoreCase: true);
    }

    [TestMethod]
    public async Task DoesNotFragmentWhenRouteIdIsEssentiallyPerSegment()
    {
        // Givskud2021 has 25 links but 24 distinct Routeids (one stray duplicate "18"), i.e. Routeid is a
        // per-segment id, not a line id. That must NOT trigger Routeid grouping (which fragmented it into
        // 24 single-link stretches); station continuity groups the branching network into 7 real lines,
        // and no station may appear twice on a stretch.
        Assert.IsTrue(IsScheduleFileExisting("Givskud2021", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Givskud2021");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");

        var stretches = result.Item.Timetable.Layout.TimetableStretches.ToArray();
        Assert.HasCount(7, stretches, "TimetableStretches (must not fragment to one per link).");
        foreach (var s in stretches)
        {
            var signatures = s.Stations.Select(x => x.Signature).ToList();
            Assert.AreEqual(signatures.Count, signatures.Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                $"Stretch '{s.Number}' has a repeated station: {string.Join("-", signatures)}");
        }
    }

    [TestMethod]
    public async Task VehicleSchedulesResolveTheirVehicleAndCargoFlowsAreDistinguished()
    {
        // The Schedules turn chart labels each row with the vehicle that works the schedule and leaves
        // out cargo-flow schedules (freight consignments, not turning vehicles). Both rely on
        // Schedule.Vehicles resolving through the schedule assignments, so verify against real data.
        Assert.IsTrue(IsScheduleFileExisting("Givskud-Modern-2025", out var file));
        using var importer = new XplnDataImporter(file, DataSetProvider, OperatingCompaniesService, TrainCategoriesService, Logger);
        var result = await importer.ImportScheduleAsync("Givskud-Modern-2025");
        Assert.IsTrue(result.IsSuccess, "Import should succeed.");
        var plan = result.Item;

        // Every schedule resolves exactly the vehicle it was created for (XPLN is one vehicle per schedule).
        Assert.IsTrue(plan.Schedules.All(s => s.Vehicles.Any()), "Every schedule should resolve at least one vehicle.");

        // The chart's filter must drop exactly the cargo-flow schedules, and keep every other one.
        var shown = plan.Schedules.Where(s => !s.IsCargoFlow).ToList();
        var cargoFlowVehicleCount = plan.ScheduledObjects.Count(v => v.IsCargoFlow);
        Assert.IsGreaterThan(0, cargoFlowVehicleCount, "The file should contain cargo flows to exclude.");
        Assert.AreEqual(plan.Schedules.Count - cargoFlowVehicleCount, shown.Count, "Kept schedules");
        Assert.IsFalse(shown.Any(s => s.Vehicles.All(v => v.IsCargoFlow)), "No cargo-flow schedule should be shown.");
    }

    // The import language and country are taken from the culture in each test file's name
    // (for example "Barmstedt2022.de-DE.ods"); the importer falls back to the current culture
    // for a file without a culture segment (for example "DreamTrack2015.ods").
    //
    // The expected validation-warning counts for H0e-Schutterwald2013 (9 -> 11),
    // Kolding_Epoke_III_2022 (35 -> 49) and Langhurst 2019 (32 -> 43) rose when the speed check started
    // covering a train's last leg, which it used to skip. The added findings are all slow final legs in
    // the imported data. No file reports a route-continuity warning: every imported route runs over
    // stretches of its layout.
    [TestMethod()]
    [DataRow("Barmstedt2022", 14, 61, 18, 21, 14, 45, 10, 72)]
    [DataRow("DreamTrack2015", 12, 62, 24, 0, 0, 40, 11, 16)]
    [DataRow("FREMODERN-2023-Final-1-1", 14, 142, 58, 37, 0, 119, 14, 66)]
    [DataRow("FREMODERN-2023-Norge", 10, 41, 13, 0, 0, 20, 10, 4)]
    [DataRow("Givskud2021", 25, 143, 49, 74, 80, 109, 25, 63)]
    [DataRow("H0e-Schutterwald2013", 10, 26, 6, 0, 20, 25, 10, 12)]
    [DataRow("Hellerup2015", 18, 60, 24, 0, 87, 20, 18, 17)]
    [DataRow("Kolding_Epoke_III_2022", 19, 60, 16, 15, 18, 38, 19, 64)]
    [DataRow("Kolding202009", 5, 38, 13, 1, 4, 28, 5, 8)]
    [DataRow("Kolding2022", 14, 73, 26, 6, 10, 55, 14, 11)]
    [DataRow("KoldingNorge2019", 13, 56, 17, 0, 0, 56, 13, 12)]
    [DataRow("Langhurst 2019", 6, 15, 4, 7, 11, 4, 6, 48)]
    [DataRow("LTK2020", 0, 0, 0, 0, 0, 0, 0, 0, 18)]
    [DataRow("Magdeburg_v_DB33_DSB32_WTB11", 0, 0, 0, 0, 0, 0, 0, 0, 40)]
    [DataRow("Montan2023H0e", 5, 32, 3, 4, 24, 3, 5, 19)]
    [DataRow("Rotebro2015", 12, 39, 15, 0, 0, 31, 12, 63)]
    [DataRow("Rotebro2016", 16, 32, 12, 0, 0, 24, 16, 14)]
    [DataRow("Timmele2015", 12, 37, 13, 0, 0, 33, 12, 31)]
    [DataRow("Värnamo2016", 8, 40, 13, 0, 0, 27, 8, 23)]
    [DataRow("Värnamo2017", 9, 40, 12, 0, 0, 29, 9, 17)]

    public async Task Import(string scheduleName, int expectedTrackStretches, int expectedTrains, int expectedLocos, int expectedWagonsets, int expectedCargoFlows, int expectedDuties, int expectedDispatchStretches, int expectedValidationWarnings = 0, int expectedStoppingErrors = 0)
    {
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
                Assert.HasCount(expectedLocos, result.Item.ScheduledObjects.Where(v => v.ObjectType == ScheduledObjectType.Locomotive), "Locos");
                Assert.HasCount(expectedWagonsets, result.Item.ScheduledObjects.Where(v => v.ObjectType == ScheduledObjectType.Wagonset), "Wagonsets");
                Assert.HasCount(expectedCargoFlows, result.Item.ScheduledObjects.Where(v => v.ObjectType == ScheduledObjectType.CargoFlow), "Cargo flows");
                Assert.HasCount(expectedDuties, result.Item.DriverDuties, "Duties");

                var validationErrors = result.Item.GetValidationErrors(ValidationSettings);
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

    // Test files are named "<name>[.<culture>].ods" (the culture drives the import language/country),
    // so match by base name and accept an optional culture segment.
    private bool IsScheduleFileExisting(string name, [NotNullWhen(true)] out FileInfo? file)
    {
        file = null;
        if (string.IsNullOrWhiteSpace(name) || TestDocumentsDirectory is null) return false;
        file = TestDocumentsDirectory.GetFiles(name + "*" + FileSuffix).FirstOrDefault(f =>
        {
            var baseName = Path.GetFileNameWithoutExtension(f.Name);
            return baseName.Equals(name, StringComparison.Ordinal)
                || baseName.StartsWith(name + ".", StringComparison.Ordinal);
        });
        return file is not null;
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
