using System.Text;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Layouts.EndProfiles;

namespace Tellurian.Trains.Schedules.Exporters.Drawing.Tests;

[TestClass]
public sealed class EndProfileExporterTests
{
    private static readonly EndProfileExporter Exporter = new();

    private static string TestFile(string name) => File.ReadAllText(Path.Combine("TestData", name)).ReplaceLineEndings("\n");

    private static EndProfile Read(string name) => new EndProfileImporter().Import(TestFile(name)).Item;

    private static string AppVersionOf(string json) => (string)JsonNode.Parse(json)!["appVersion"]!;

    [TestMethod]
    [DataRow("fremo-h0-b96.endprofile.json")]
    [DataRow("mmm480.endprofile.json")]
    public void AFileReadAndWrittenIsUnchanged(string name)
    {
        var original = TestFile(name);

        var written = Exporter.Write(Read(name), new ExportOptions(AppVersion: AppVersionOf(original)));

        Assert.AreEqual(original.TrimEnd(), written.ReplaceLineEndings("\n"));
    }

    [TestMethod]
    public void AVersion1FileIsWrittenAtTheCurrentVersion()
    {
        var written = JsonNode.Parse(Exporter.Write(Read("fremo-h0-b96.v1.endprofile.json"), ExportOptions.Default))!;

        Assert.AreEqual("EndProfile", (string)written["kind"]!);
        Assert.AreEqual(DrawingFileFormat.CurrentSchemaVersion(DrawingFileKind.EndProfile), (int)written["schemaVersion"]!);
        Assert.AreEqual(DrawingFileFormat.LibraryVersion, (string)written["appVersion"]!);
        Assert.AreEqual("TwoRail", (string)written["content"]!["tracks"]![0]!["railSystem"]!);
    }

    [TestMethod]
    public void ARevisionCountsUpAndRecordsWhoRevisedIt()
    {
        var profile = Read("fremo-h0-b96.endprofile.json");
        var revision = profile.Revision;
        var options = new ExportOptions(IsRevision: true, RevisedBy: " Stefan ", Date: new DateOnly(2026, 11, 21));

        var once = Exporter.Revise(profile, options);
        var twice = Exporter.Revise(once, options with { RevisedBy = "Anna", Date = new DateOnly(2026, 11, 22) });
        var again = Exporter.Revise(twice, options with { RevisedBy = "stefan", Date = new DateOnly(2026, 11, 23) });

        Assert.AreEqual(revision, profile.Revision, "The end profile given is not changed.");
        Assert.AreEqual(revision + 3, again.Revision);
        CollectionAssert.AreEqual(new[] { ("stefan", new DateOnly(2026, 11, 23)), ("Anna", new DateOnly(2026, 11, 22)) },
            again.RevisionLog.Select(r => (r.Name, r.Date)).ToArray());
    }

    [TestMethod]
    public void AWriteThatIsNoRevisionWritesTheEndProfileAsItIs()
    {
        var profile = Read("fremo-h0-b96.endprofile.json");
        Assert.AreSame(profile, Exporter.Revise(profile, ExportOptions.Default));
    }

    [TestMethod]
    public async Task AnExportToAStreamHoldsWhatWasWritten()
    {
        var profile = Read("mmm480.endprofile.json");
        using var stream = new MemoryStream();

        var result = await Exporter.ExportAsync(profile, stream, new ExportOptions(IsRevision: true, RevisedBy: "Stefan"), TestContext.CancellationToken);

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(profile.Revision + 1, result.Item!.Revision);
        var reread = new EndProfileImporter().Import(Encoding.UTF8.GetString(stream.ToArray())).Item;
        Assert.AreEqual(result.Item.Revision, reread.Revision);
        Assert.AreEqual("Stefan", reread.RevisionLog[0].Name);
    }

    [TestMethod]
    public async Task AStreamThatCantBeWrittenGivesAFailureWithACode()
    {
        using var readOnly = new MemoryStream([], writable: false);

        var result = await Exporter.ExportAsync(Read("mmm480.endprofile.json"), readOnly, ExportOptions.Default, TestContext.CancellationToken);

        Assert.AreEqual(DrawingFileWriter.WriteFailed, result.Messages.Single().Code);
    }

    [TestMethod]
    public void ACopyOwnsItsTracks()
    {
        var profile = Read("fremo-h0-b96.endprofile.json");
        var copy = profile.Copy();
        copy.Tracks[0].Offset += 10;
        copy.Tracks.Clear();

        Assert.IsNotEmpty(profile.Tracks);
        Assert.AreNotEqual(profile.Id, profile.Copy("Mine").Id);
        Assert.IsNull(profile.Copy("Mine").ExternalReferences);
    }

    public TestContext TestContext { get; set; } = null!;
}
