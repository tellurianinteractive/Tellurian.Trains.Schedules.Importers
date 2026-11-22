using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts.Halls;

namespace Tellurian.Trains.Schedules.Exporters.Drawing.Tests;

/// <summary>Module Draw's hall files read in every earlier shape and written in today's (decision 10: ids as whole numbers).</summary>
[TestClass]
public sealed class HallTests
{
    private static readonly HallExporter Exporter = new();

    private static string TestFile(string name) => File.ReadAllText(Path.Combine("TestData", name)).ReplaceLineEndings("\n");

    private static Hall Read(string json)
    {
        var result = new HallImporter().Import(json);
        Assert.IsTrue(result.IsSuccess, string.Join(", ", result.Messages.Select(m => m.Text)));
        return result.Item;
    }

    [TestMethod]
    [DataRow("club-hall.v1.hall.json")]
    [DataRow("club-hall.draft.hall.json")]
    public void ObstaclesDoorsAndZonesGetWholeNumbersAsIds(string name)
    {
        Assert.Contains("-", (string)JsonNode.Parse(TestFile(name))!["content"]!["obstacles"]![0]!["id"]!, "The test needs GUIDs as ids.");

        var hall = Read(TestFile(name));

        Assert.AreEqual("Club hall", hall.Name);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, hall.Obstacles.Select(o => o.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2 }, hall.Doors.Select(d => d.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 1 }, hall.NoGoZones.Select(z => z.Id).ToArray());
        Assert.AreEqual(ObstacleKind.Furniture, hall.Obstacles[2].Kind);
        Assert.AreEqual(DoorKind.EmergencyExit, hall.Doors[1].Kind);
        Assert.IsTrue(hall.Obstacles.All(o => o.ExtensionData is null));
    }

    [TestMethod]
    public void AWrittenHallHasTodaysShapeAndReadsBackTheSame()
    {
        var written = Exporter.Write(Read(TestFile("club-hall.v1.hall.json")), ExportOptions.Default);

        var file = JsonNode.Parse(written)!;
        Assert.AreEqual("Hall", (string)file["kind"]!);
        Assert.AreEqual(DrawingFileFormat.CurrentSchemaVersion(DrawingFileKind.Hall), (int)file["schemaVersion"]!);
        Assert.AreEqual(1, (int)file["content"]!["obstacles"]![0]!["id"]!);
        Assert.AreEqual("EmergencyExit", (string)file["content"]!["doors"]![1]!["kind"]!);
        Assert.AreEqual(written, Exporter.Write(Read(written), ExportOptions.Default), "Today's shape reads and writes unchanged.");
    }

    [TestMethod]
    public void ObstacleIdsAreMappedForTheHallPlansThatReferToThem()
    {
        var hall = JsonNode.Parse(TestFile("club-hall.v1.hall.json"))!["content"]!;
        var second = (string)hall["obstacles"]![1]!["id"]!;

        var ids = DrawingMigrations.HallToVersion2(hall);

        Assert.AreEqual(2, ids[second]);
        Assert.IsEmpty(DrawingMigrations.HallToVersion2(hall), "The step leaves a migrated hall as it is.");
    }

    [TestMethod]
    public void WholeNumberIdsAreKeptAndTheOthersNumberedOn()
    {
        var hall = new JsonObject
        {
            ["doors"] = new JsonArray(new JsonObject { ["id"] = 4 }, new JsonObject { ["id"] = "x" }, new JsonObject { ["id"] = 4 }, new JsonObject()),
        };

        DrawingMigrations.HallToVersion2(hall);

        CollectionAssert.AreEqual(new[] { 4, 5, 6, 7 }, hall["doors"]!.AsArray().Select(d => (int)d!["id"]!).ToArray());
    }

    [TestMethod]
    public void ARevisionCountsUpAndRecordsWhoRevisedIt()
    {
        var hall = Read(TestFile("club-hall.v1.hall.json"));
        var options = new ExportOptions(IsRevision: true, RevisedBy: "Stefan", Date: new DateOnly(2026, 11, 21));

        var revision = hall.Revision;

        var revised = Exporter.Revise(hall, options);

        Assert.AreEqual(revision + 1, revised.Revision);
        Assert.AreEqual("Stefan", revised.RevisionLog[0].Name);
        Assert.AreEqual(revision, hall.Revision, "The hall given is not changed.");
        Assert.AreSame(hall, Exporter.Revise(hall, ExportOptions.Default), "A write that isn't a revision writes the hall as it is.");
    }

    [TestMethod]
    public void OnlyHallFilesCanBeImported()
    {
        var importer = new HallImporter();
        Assert.IsTrue(importer.CanImport("x.json", System.Text.Encoding.UTF8.GetBytes(TestFile("club-hall.draft.hall.json"))));
        Assert.IsFalse(importer.CanImport("x.json", """{ "format": "tellurian-drawing", "kind": "HallPlan" }"""u8));
        Assert.AreEqual(FileErrorCode.WrongKind, importer.Import("""{ "format": "tellurian-drawing", "kind": "EndProfile", "schemaVersion": 2, "content": {} }""").Messages.Single().Code);
    }
}
