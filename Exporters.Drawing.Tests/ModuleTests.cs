using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;
using Tellurian.Trains.Schedules.Model.Layouts.Tracks;

namespace Tellurian.Trains.Schedules.Exporters.Drawing.Tests;

/// <summary>Module Draw's module files read in every earlier shape and written in today's (survey step 6a).</summary>
[TestClass]
public sealed class ModuleTests
{
    /// <summary>A module as a development build of version 2 wrote it, in a shape between versions 1 and 2, marked as version 1.</summary>
    private const string Draft = "station-3-sections.module.json";
    private const string Version1 = "station-3-sections.v1.module.json";
    private static readonly ModuleExporter Exporter = new();

    private static string TestFile(string name) => File.ReadAllText(Path.Combine("TestData", name)).ReplaceLineEndings("\n");

    private static ModuleFileContent Read(string json)
    {
        var result = new ModuleImporter().Import(json);
        Assert.IsTrue(result.IsSuccess, string.Join(", ", result.Messages.Select(m => m.Text)));
        return result.Item;
    }

    [TestMethod]
    public void AModuleFileIsReadWithItsDrawingLocationAndEndProfiles()
    {
        var content = Read(TestFile(Draft));
        var module = content.Module;

        Assert.IsNotNull(module.Id);
        Assert.IsNotNull(module.Standard?.Id, "The standard carries the catalogue's id.");
        Assert.IsTrue(module.HasDrawing);
        Assert.HasCount(2, module.Drawing.Exits);
        CollectionAssert.AreEqual(new[] { ExitSide.Left, ExitSide.Right }, module.Drawing.Exits.Select(e => e.Direction).ToArray());
        Assert.IsNotNull(module.OperationLocation);
        Assert.IsTrue(module.Drawing.TrackElements.All(e => e.StationTrackId is null || module.Tracks.Any(t => t.Id == e.StationTrackId)));
        Assert.IsNotEmpty(content.EndProfiles);
    }

    [TestMethod]
    public void AVersion1FileGetsTodaysShape()
    {
        var written = JsonNode.Parse(Exporter.Write(Read(TestFile(Version1)), ExportOptions.Default))!;

        Assert.AreEqual(DrawingFileFormat.CurrentSchemaVersion(DrawingFileKind.Module), (int)written["schemaVersion"]!);
        var module = written["content"]!["module"]!.AsObject();
        foreach (var earlier in new[] { "variants", "owners", "fremoNumber", "scale", "theme" }) Assert.IsFalse(module.ContainsKey(earlier), earlier);
        Assert.IsTrue(module.ContainsKey("drawing"));
        Assert.AreEqual("Left", (string)module["drawing"]!["exits"]![0]!["direction"]!);
    }

    [TestMethod]
    public void TodaysShapeReadsAndWritesUnchanged()
    {
        var once = Exporter.Write(Read(TestFile(Version1)), ExportOptions.Default);

        Assert.AreEqual(once, Exporter.Write(Read(once), ExportOptions.Default));
    }

    [TestMethod]
    public void TheThemeIsDropped()
    {
        var file = JsonNode.Parse(TestFile(Version1))!.AsObject();
        file["content"]!["module"]!["theme"] = "Sweden 1970s";

        var module = Read(file.ToJsonString()).Module;

        Assert.IsNull(module.ExtensionData);
    }

    [TestMethod]
    public void TrackElementsAreReadAsTheirKinds()
    {
        var elements = Read(TestFile(Draft)).Module.Drawing.TrackElements;

        Assert.IsNotEmpty(elements);
        Assert.IsTrue(elements.All(e => e is StraightTrack or CurvedTrack or FlexTrack or Turnout or Crossing or DoubleSlip or BufferStop));
    }

    [TestMethod]
    public void ARevisionCountsUpTheModulesRevision()
    {
        var content = Read(TestFile(Draft));
        var revision = content.Module.Revision;

        var revised = Exporter.Revise(content, new ExportOptions(IsRevision: true, RevisedBy: "Stefan", Date: new DateOnly(2026, 11, 21)));

        Assert.AreEqual(revision + 1, revised.Module.Revision);
        Assert.AreEqual("Stefan", revised.Module.RevisionLog[0].Name);
        Assert.AreEqual(revision, content.Module.Revision, "The content given is not changed.");
    }

    [TestMethod]
    public void AModuleWithoutADrawingWritesNone()
    {
        var module = new Module { Name = "Planned" };

        Assert.IsFalse(module.HasDrawing);
        Assert.IsTrue(module.Drawing.IsEmpty, "The drawing is there to draw, but empty.");
        Assert.IsNull(module.WrittenDrawing);
    }

    [TestMethod]
    public void ACopyOwnsItsDrawing()
    {
        var module = Read(TestFile(Draft)).Module;

        var copy = module.With(m => m.Drawing.Exits[0].Label = "Changed");

        Assert.AreNotEqual("Changed", module.Drawing.Exits[0].Label);
        Assert.AreEqual("Changed", copy.Drawing.Exits[0].Label);
    }

    [TestMethod]
    public void SidesReferToTheirExitsBySideAndNumber()
    {
        var drawing = Read(TestFile(Draft)).Module.Drawing;

        Assert.AreEqual((ExitPlace.Left, ExitPlace.Right), (drawing.Sides[0].FromExit, drawing.Sides[0].ToExit));
        Assert.AreEqual((ExitPlace.Right, ExitPlace.Left), (drawing.Sides[1].FromExit, drawing.Sides[1].ToExit));
        var written = JsonNode.Parse(Exporter.Write(Read(TestFile(Version1)), ExportOptions.Default))!;
        var side = written["content"]!["module"]!["drawing"]!["sides"]![0]!;
        Assert.AreEqual(("Left", 1), ((string)side["fromExit"]!["side"]!, (int)side["fromExit"]!["number"]!));
    }

    [TestMethod]
    public void ASideAtAnExitThatIsNotThereGetsAPlaceNoExitHas()
    {
        var file = JsonNode.Parse(TestFile(Draft))!.AsObject();
        file["content"]!["module"]!["drawing"]!["sides"]![0]!["toExit"] = "No such exit";

        var side = Read(file.ToJsonString()).Module.Drawing.Sides[0];

        Assert.AreEqual(new ExitPlace(ExitSide.Undefined), side.ToExit);
    }

    [TestMethod]
    public void TrackElementsAreNumberedFromOne()
    {
        var elements = Read(TestFile(Draft)).Module.Drawing.TrackElements;

        CollectionAssert.AreEqual(Enumerable.Range(1, elements.Count).ToArray(), elements.Select(e => e.Id).ToArray());
    }

    [TestMethod]
    public void EnteredLengthsAndStubsAreFlagsOnTheTracks()
    {
        var file = JsonNode.Parse(TestFile(Draft))!.AsObject();
        var module = file["content"]!["module"]!;
        const int first = 1, second = 2;
        module["operationLocation"]!["tracks"] = new JsonArray(
            new JsonObject { ["id"] = first, ["designation"] = "1" },
            new JsonObject { ["id"] = second, ["designation"] = "2" });
        module["drawing"]!["tracksWithEnteredLength"] = new JsonArray(first, second);
        module["drawing"]!["tracksWithEnteredStub"] = new JsonArray(second);

        var read = Read(file.ToJsonString()).Module;

        Assert.IsTrue(read.Tracks.Single(t => t.Id == first) is { IsLengthEntered: true, IsStubEntered: false });
        Assert.IsTrue(read.Tracks.Single(t => t.Id == second) is { IsLengthEntered: true, IsStubEntered: true });
        Assert.IsNull(read.Drawing.ExtensionData, "The lists are gone.");
    }

    [TestMethod]
    public void TheStandardIdBesideTheStandardBecomesItsId()
    {
        var file = JsonNode.Parse(TestFile(Draft))!.AsObject();
        var id = (string)file["content"]!["module"]!["standardId"]!;

        var module = Read(file.ToJsonString()).Module;

        Assert.AreEqual(Guid.Parse(id), module.Standard!.Id);
        Assert.IsNull(module.ExtensionData);
        var written = JsonNode.Parse(Exporter.Write(Read(file.ToJsonString()), ExportOptions.Default))!["content"]!["module"]!.AsObject();
        Assert.IsFalse(written.ContainsKey("standardId"));
        Assert.AreEqual(id, (string)written["standard"]!["id"]!);
    }

    [TestMethod]
    public void TheSpeedLimitIsReadAndWritten()
    {
        var content = Read(TestFile(Draft));
        content.Module.SpeedLimit = 40;

        var read = Read(Exporter.Write(content, ExportOptions.Default));

        Assert.AreEqual(40, read.Module.SpeedLimit);
        Assert.IsNull(Read(TestFile(Draft)).Module.SpeedLimit, "No limit unless the owner sets one.");
    }

    [TestMethod]
    public void OnlyModuleFilesCanBeImported()
    {
        var importer = new ModuleImporter();
        Assert.IsTrue(importer.CanImport("x.json", System.Text.Encoding.UTF8.GetBytes(TestFile(Draft))));
        Assert.IsFalse(importer.CanImport("x.json", """{ "format": "tellurian-drawing", "kind": "Hall" }"""u8));
    }
}
