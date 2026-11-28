using System.Text;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Drawings.EndProfiles;

namespace Tellurian.Trains.Schedules.Importers.Drawing.Tests;

[TestClass]
public sealed class EndProfileImporterTests
{
    private static readonly EndProfileImporter Importer = new();

    private static string TestFile(string name) => File.ReadAllText(Path.Combine("TestData", name));

    [TestMethod]
    public void ACurrentFileIsRead()
    {
        var result = Importer.Import(TestFile("fremo-h0-b96.endprofile.json"));

        Assert.IsTrue(result.IsSuccess, string.Join(", ", result.Messages.Select(m => m.Text)));
        var profile = result.Item;
        Assert.AreEqual(("FREMO-H0-B96", "B96", "H0"), (profile.Designation, profile.ShortName, profile.Scale));
        Assert.AreEqual(Guid.Parse("5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4d01"), profile.Id);
        Assert.AreEqual("FREMO/H0-RE", profile.Standards.Single().Name);
        Assert.IsNotNull(profile.Outline);
        Assert.IsNotEmpty(profile.Tracks);
        Assert.IsTrue(profile.HasScrewHoles);
    }

    [TestMethod]
    public void AVersion1FileIsReadWithItsKindAndEnumValuesInCamelCase()
    {
        var json = TestFile("fremo-h0-b96.v1.endprofile.json");
        Assert.Contains("\"kind\": \"endProfile\"", json, "The test needs the version 1 spelling.");

        var result = Importer.Import(json);

        Assert.IsTrue(result.IsSuccess, string.Join(", ", result.Messages.Select(m => m.Text)));
        Assert.AreEqual(RailSystem.TwoRail, result.Item.Tracks[0].RailSystem);
    }

    [TestMethod]
    public void TheRegistryIdOfAVersion1FileMovesToTheExternalReferences()
    {
        var file = JsonNode.Parse(TestFile("fremo-h0-b96.v1.endprofile.json"))!.AsObject();
        file["content"]!["registryId"] = 42;

        var profile = Importer.Import(file.ToJsonString()).Item;

        Assert.AreEqual(42, profile.ExternalReferences!["moduleRegistry"].GetProperty("endProfileId").GetInt32());
        Assert.IsNull(profile.ExtensionData);
    }

    [TestMethod]
    [DataRow("{ not json", FileErrorCode.InvalidJson, DisplayName = "Invalid JSON")]
    [DataRow("""{ "format": "tellurian-plan", "schemaVersion": 4 }""", FileErrorCode.NotThisFormat, DisplayName = "Another format")]
    [DataRow("""{ "format": "tellurian-drawing", "kind": "Hall", "schemaVersion": 2, "content": {} }""", FileErrorCode.WrongKind, DisplayName = "Another kind")]
    [DataRow("""{ "format": "tellurian-drawing", "kind": "EndProfile", "schemaVersion": 99, "content": {} }""", FileErrorCode.NewerVersion, DisplayName = "A newer version")]
    [DataRow("""{ "format": "tellurian-drawing", "kind": "EndProfile", "schemaVersion": 2, "content": { "designation": "X" } }""", FileErrorCode.InvalidContent, DisplayName = "Required properties missing")]
    public void AFileThatCantBeReadGivesAFailureWithACode(string json, string code)
    {
        var result = Importer.Import(json);

        Assert.IsTrue(result.IsFailure);
        Assert.AreEqual(code, result.Messages.Single().Code);
    }

    [TestMethod]
    public void AnInvalidPlateIsReportedAsInvalidContent()
    {
        var file = JsonNode.Parse(TestFile("fremo-h0-b96.endprofile.json"))!.AsObject();
        file["content"]!["plate"] = JsonNode.Parse("""{ "start": { "x": 0, "y": 0 }, "segments": [], "bottom": -10 }""");

        var result = Importer.Import(file.ToJsonString());

        Assert.AreEqual(FileErrorCode.InvalidContent, result.Messages.Single().Code);
    }

    [TestMethod]
    public void OnlyEndProfileFilesCanBeImported()
    {
        Assert.IsTrue(Importer.CanImport("x.json", Encoding.UTF8.GetBytes(TestFile("fremo-h0-b96.endprofile.json"))));
        Assert.IsTrue(Importer.CanImport("x.json", [.. Encoding.UTF8.Preamble, .. Encoding.UTF8.GetBytes(TestFile("fremo-h0-b96.v1.endprofile.json"))]));
        Assert.IsFalse(Importer.CanImport("x.json", """{ "format": "tellurian-drawing", "kind": "Module" }"""u8));
        Assert.IsFalse(Importer.CanImport("x.json", "not json"u8));
        CollectionAssert.AreEqual(new[] { ".json", ".tdendprofile" }, Importer.FileExtensions.ToArray());
    }

    [TestMethod]
    public async Task AStreamIsRead()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TestFile("mmm480.endprofile.json")));

        var result = await Importer.ImportAsync(stream, "mmm480.endprofile.json", TestContext.CancellationToken);

        Assert.AreEqual("MMM480", result.Item.Designation);
    }

    [TestMethod]
    public void TheHeaderIsReadWithoutTheContent()
    {
        var header = DrawingFileReader.ReadHeader(TestFile("fremo-h0-b96.v1.endprofile.json"), out var message);

        Assert.IsNull(message);
        Assert.AreEqual(new DrawingFileHeader(DrawingFileKind.EndProfile, 1, header!.AppVersion), header);
        Assert.IsTrue(header.IsSupported);
        Assert.IsNull(DrawingFileReader.ReadHeader("{}", out message));
        Assert.AreEqual(FileErrorCode.NotThisFormat, message!.Code);
    }

    public TestContext TestContext { get; set; } = null!;
}
