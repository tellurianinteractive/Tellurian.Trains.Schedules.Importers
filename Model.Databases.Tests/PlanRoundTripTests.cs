using Microsoft.EntityFrameworkCore;
using Tellurian.Trains.Schedules.Model.PlanFiles;
using Tellurian.Trains.Schedules.Model.Tests;

namespace Tellurian.Trains.Schedules.Model.Databases.Tests;

/// <summary>
/// A whole plan exported to the database and read back holds what it held: the yardstick of the mapping.
/// </summary>
[TestClass]
public sealed class PlanRoundTripTests
{
    public TestContext TestContext { get; set; } = null!;

    private static Plan ReadVarnamo() => PlanFile.Read(File.ReadAllText(Path.Combine("Test data", "Plan.Varnamo-Moduldagar.json")));

    [TestMethod]
    public async Task VarnamoKeepsItsFingerprint()
    {
        var expected = PlanFingerprint.Of(ReadVarnamo());

        var loaded = await ExportAndImportAsync(ReadVarnamo(), "VarnamoFingerprint.db");

        PlanFingerprint.AssertSame(expected, PlanFingerprint.Of(loaded));
    }

    [TestMethod]
    public async Task VarnamoIsWrittenAsBeforeAfterTheDatabase()
    {
        var plan = ReadVarnamo();
        var expected = PlanFile.Write(plan, writeIndented: true);

        var loaded = await ExportAndImportAsync(plan, "VarnamoFile.db");

        AssertSameText(expected, PlanFile.Write(loaded, writeIndented: true));
    }

    [TestMethod]
    public async Task APlanMadeFromAHallPlanKeepsWhatTheHallPlanGaveIt()
    {
        var plan = PlanFactory.CreatePlan(HallPlanLayout(), "en");
        var staffed = plan.Layout.OperationLocations.Single(l => l.Signature == "Yb");
        plan.Timetable.EnsurePlanningOf(staffed).StaffPosts.Add(new StaffPost { Roles = StaffRoles.Dispatcher | StaffRoles.Shuntman, ExtensionData = new() { ["shift"] = JsonElementOf("\"night\"") } });
        plan.Timetable.Settings.TimeAndSpeed.StationTimings.ExtensionData = new() { ["futureTiming"] = JsonElementOf("5") };
        var expected = PlanFile.Write(plan, writeIndented: true);

        var loaded = await ExportAndImportAsync(plan, "HallPlan.db");

        AssertSameText(expected, PlanFile.Write(loaded, writeIndented: true));
        var layout = loaded.Layout;
        var tellby = layout.OperationLocations.Single(l => l.Signature == "Tby");
        var ytterby = (Station)layout.OperationLocations.Single(l => l.Signature == "Yb");
        Assert.AreEqual(tellby.Id, layout.Modules.First().OperationLocationId, "a module refers to the layout's own location");
        Assert.AreEqual("Curve", layout.Modules.Last().Title);
        Assert.AreEqual("FREMO H0", layout.StandardName);
        Assert.AreEqual("H0", layout.Scale?.ShortName);
        Assert.AreEqual(9.5, layout.TrackStretches.First().MeasuredDistance);
        Assert.AreEqual(70, layout.TrackStretches.First().MeasuredSpeed);
        Assert.IsTrue(layout.StretchLines.Single().Legs.Single().Joins(tellby.Id, ytterby.Id));
        Assert.AreSame(ytterby, tellby.LockKey?.HeldAt, "a lock key is held at the layout's own station");
        Assert.AreEqual("4711", tellby.ExternalReferences?["registry"].GetString());
        Assert.AreEqual("kept", tellby.ExtensionData?["fromAnotherSystem"].GetString());
        CollectionAssert.AreEqual(new[] { 1, 2 }, layout.TrackStretches.First().ModuleIds.ToArray());
        var staffing = loaded.Timetable.PlanningOf(ytterby).StaffPosts.Single();
        Assert.AreEqual(StaffRoles.Dispatcher | StaffRoles.Shuntman, staffing.Roles);
        Assert.AreEqual("night", staffing.ExtensionData?["shift"].GetString(), "unknown properties inside a JSON column");
        Assert.AreEqual(5, loaded.Timetable.Settings.TimeAndSpeed.StationTimings.ExtensionData?["futureTiming"].GetInt32(), "unknown properties inside the settings");
    }

    private static Layout HallPlanLayout()
    {
        var layout = new Layout
        {
            Name = "Club meeting",
            SourceId = Guid.Parse("8f0c6a8e-3b7e-4c1f-9d3a-2a9b8c7d6e5f"),
            SourceRevision = 3,
            StandardName = "FREMO H0",
            Scale = new Scale { Id = 4, ShortName = "H0" },
        };
        var tellby = new Station(1, "Tellby", "Tby")
        {
            CountryCode = "NO",
            HasCargoExchange = true,
            Layout = layout,
            ExternalReferences = new() { ["registry"] = JsonElementOf("\"4711\"") },
            ExtensionData = new() { ["fromAnotherSystem"] = JsonElementOf("\"kept\"") },
        };
        tellby.Tracks.Add(new StationTrack(11, "1") { Station = tellby, IsMain = true });
        tellby.Tracks.Add(new StationTrack(12, "2") { Station = tellby });
        var ytterby = new Station(2, "Ytterby", "Yb") { CountryCode = "NO", IsManned = true, Layout = layout };
        ytterby.Tracks.Add(new StationTrack(21, "1") { Station = ytterby, IsMain = true });
        tellby.LockKey = new LockKey { Name = "A1", HeldAt = ytterby };
        layout.OperationLocations.Add(tellby);
        layout.OperationLocations.Add(ytterby);
        layout.TrackStretches.Add(new TrackStretch(1, tellby, ytterby, 10) { Speed = 80, Layout = layout, ModuleIds = [1, 2], MeasuredDistance = 9.5, MeasuredSpeed = 70 });
        layout.Modules.Add(new LayoutModule { Id = 1, Title = "Tellby", OperationLocationId = tellby.Id });
        layout.Modules.Add(new LayoutModule { Id = 2, Title = "Curve", PackageLabel = "Box 1" });
        layout.StretchLines.Add(new StretchLine { Id = 1, Name = "Main", Legs = [new(tellby.Id, ytterby.Id)] });
        return layout;
    }

    private static System.Text.Json.JsonElement JsonElementOf(string json) => System.Text.Json.JsonDocument.Parse(json).RootElement.Clone();

    private async Task<Plan> ExportAndImportAsync(Plan plan, string fileName)
    {
        var options = new DbContextOptionsBuilder<ScheduleDbContext>()
            .UseSqlite($"DataSource={Path.Combine(TestContext.TestRunResultsDirectory ?? ".", fileName)};Pooling=False")
            .Options;
        var exported = await new DatabaseExportService(options).ExportScheduleAsync(plan);
        Assert.IsTrue(exported.IsSuccess, string.Join(", ", exported.Messages));
        var imported = await new DatabaseImportService(options).ImportScheduleAsync(plan.Name);
        Assert.IsTrue(imported.IsSuccess, string.Join(", ", imported.Messages));
        return imported.Item;
    }

    private static void AssertSameText(string expected, string actual)
    {
        if (expected == actual) return;
        var lines = expected.Split('\n');
        var otherLines = actual.Split('\n');
        var index = Enumerable.Range(0, Math.Max(lines.Length, otherLines.Length))
            .First(i => i >= lines.Length || i >= otherLines.Length || lines[i] != otherLines[i]);
        var from = Math.Max(0, index - 8);
        Assert.Fail($"The plan files differ at line {index + 1}:\nexpected:\n{string.Join('\n', lines.Skip(from).Take(10))}\nactual:\n{string.Join('\n', otherLines.Skip(from).Take(10))}");
    }
}
