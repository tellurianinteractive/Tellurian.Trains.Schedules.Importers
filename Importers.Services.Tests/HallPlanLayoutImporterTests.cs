using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Importers.Interfaces;

namespace Tellurian.Trains.Schedules.Importers.Services.Tests;

/// <summary>The Module Draw hall plan importer, and choosing an importer among those registered.</summary>
[TestClass]
public class HallPlanLayoutImporterTests
{
    public TestContext TestContext { get; set; } = default!;

    private static readonly HallPlanLayoutImporter Importer = new();

    private static byte[] HallPlanFileBytes(bool withByteOrderMark = false)
    {
        var layout = new Layout { Name = "Club meeting", SourceId = Guid.Parse("40000000-0000-0000-0000-000000000004"), SourceRevision = 2 };
        var tellby = new Station(1, "Tellby", "Tby") { Layout = layout };
        tellby.Tracks.Add(new StationTrack(11, "1") { Station = tellby });
        var ytterby = new Station(2, "Ytterby", "Yb") { Layout = layout };
        ytterby.Tracks.Add(new StationTrack(21, "1") { Station = ytterby });
        layout.OperationLocations.Add(tellby);
        layout.OperationLocations.Add(ytterby);
        layout.TrackStretches.Add(new TrackStretch(1, tellby, ytterby, 10) { Layout = layout });
        var file = new JsonObject
        {
            ["format"] = HallPlanFile.Format,
            ["kind"] = HallPlanFile.Kind, ["schemaVersion"] = 2,
            ["content"] = new JsonObject { ["layout"] = JsonNode.Parse(JsonSerializer.Serialize(layout, Layout.JsonSerializerOptions)) },
        };
        var bytes = Encoding.UTF8.GetBytes(file.ToJsonString());
        return withByteOrderMark ? [.. Encoding.UTF8.Preamble, .. bytes] : bytes;
    }

    [TestMethod]
    public void AHallPlanIsRecognisedWithOrWithoutByteOrderMark()
    {
        Assert.IsTrue(Importer.CanImport("meeting.json", HallPlanFileBytes()));
        Assert.IsTrue(Importer.CanImport("meeting.json", HallPlanFileBytes(withByteOrderMark: true)));
        Assert.IsFalse(Importer.CanImport("plan.json", Encoding.UTF8.GetBytes("""{ "format": "tellurian-plan", "plan": { } }""")));
    }

    [TestMethod]
    public async Task TheLayoutIsReadWithItsReferences()
    {
        using var stream = new MemoryStream(HallPlanFileBytes(withByteOrderMark: true));

        var result = await Importer.ImportAsync(stream, "meeting.json", TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccess);
        var layout = result.Item;
        Assert.AreEqual("Club meeting", layout.Name);
        Assert.AreEqual(2, layout.SourceRevision);
        Assert.AreSame(layout.OperationLocations.First(), layout.TrackStretches.Single().Start);
    }

    [TestMethod]
    public async Task AHallPlanWithoutLayoutFailsWithAMessage()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""{ "format": "tellurian-drawing", "kind": "HallPlan", "schemaVersion": 1, "content": { } }"""));

        var result = await Importer.ImportAsync(stream, "old.json", TestContext.CancellationToken);

        Assert.IsTrue(result.IsFailure);
        Assert.Contains("layout block", result.Messages.Single().Text);
    }

    [TestMethod]
    public void TheFirstImporterForTheExtensionThatRecognisesTheFileReadsIt()
    {
        var other = new FakeImporter(".json", recognises: false);
        var csv = new FakeImporter(".csv", recognises: true);
        ILayoutImporter[] importers = [other, csv, Importer];

        Assert.AreSame(Importer, importers.For("Meeting.JSON", HallPlanFileBytes()));
        Assert.AreSame(csv, importers.For("layout.csv", "a;b"u8));
        Assert.IsNull(importers.For("plan.json", """{ "format": "tellurian-plan" }"""u8));
        Assert.IsNull(importers.For("layout.ods", HallPlanFileBytes()));
        CollectionAssert.AreEqual(new[] { ".json", ".csv" }, importers.FileExtensions.ToArray());
    }

    private sealed class FakeImporter(string extension, bool recognises) : ILayoutImporter
    {
        public string ResourceKey => "Fake";
        public IReadOnlyList<string> FileExtensions => [extension];
        public bool CanImport(string fileName, ReadOnlySpan<byte> content) => recognises;
        public Task<ImportResult<Layout>> ImportAsync(Stream stream, string fileName, CancellationToken cancellationToken = default) =>
            Task.FromResult(ImportResult<Layout>.Success(new Layout()));
    }
}
