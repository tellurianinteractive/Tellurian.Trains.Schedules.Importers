using System.Text.Json;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.Files;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>The envelope and migration chain that every file of the suite shares.</summary>
[TestClass]
public sealed class FileEnvelopeTests
{
    // Like a plan file: one kind of file, read without an envelope as version 0.
    private static readonly FileKind Plan = new()
    {
        Name = "plan",
        Format = "tellurian-plan",
        ContentProperty = "plan",
        CurrentSchemaVersion = 2,
        ReadsWithoutEnvelope = true,
        Migrations =
        [
            new(0, content => { content["steps"] = content["steps"]!.GetValue<string>() + "1"; return content; }),
            new(1, content => { content["steps"] = content["steps"]!.GetValue<string>() + "2"; return content; }),
        ],
    };

    // Like a drawing file: one of several kinds of one format, versions from 1, never without an envelope.
    private static readonly FileKind Hall = new()
    {
        Name = "hall",
        Format = "tellurian-drawing",
        Kind = "Hall",
        ContentProperty = "content",
        CurrentSchemaVersion = 2,
        LowestSchemaVersion = 1,
        Migrations = [new(1, content => { content["upgraded"] = true; return content; })],
    };

    [TestMethod]
    public void TheEnvelopeIsWrittenFirstInAFixedOrder()
    {
        var json = FileEnvelope.Write(Hall, "1.2.3", writer => { writer.WriteStartObject(); writer.WriteString("name", "Hall A"); writer.WriteEndObject(); });

        var names = JsonDocument.Parse(json).RootElement.EnumerateObject().Select(p => p.Name).ToArray();
        CollectionAssert.AreEqual(new[] { "format", "kind", "schemaVersion", "appVersion", "content" }, names);
        Assert.AreEqual("Hall A", FileEnvelope.ReadContent(Hall, JsonNode.Parse(json))["name"]!.GetValue<string>());
    }

    [TestMethod]
    public void AKindWithoutAKindPropertyWritesNone()
    {
        var json = FileEnvelope.Write(Plan, "1.0", writer => { writer.WriteStartObject(); writer.WriteEndObject(); });

        Assert.IsFalse(JsonNode.Parse(json)!.AsObject().ContainsKey("kind"));
    }

    [TestMethod]
    public void AnOlderFileIsBroughtUpToDateStepByStep()
    {
        var content = FileEnvelope.ReadContent(Plan, JsonNode.Parse("""{ "format": "tellurian-plan", "schemaVersion": 0, "plan": { "steps": "" } }"""));

        Assert.AreEqual("12", content["steps"]!.GetValue<string>());
    }

    [TestMethod]
    public void AFileWithoutAnEnvelopeIsReadAsTheLowestVersionWhereTheKindAllowsIt()
    {
        var root = JsonNode.Parse("""{ "steps": "" }""");

        Assert.AreEqual(new FileHeader(null, 0, null, HasEnvelope: false), FileEnvelope.ReadHeader(Plan, root));
        Assert.AreEqual("12", FileEnvelope.ReadContent(Plan, root)["steps"]!.GetValue<string>());
        Assert.IsTrue(Error(() => FileEnvelope.ReadContent(Hall, JsonNode.Parse("""{ "name": "Hall A" }"""))) is NotThisFormat);
    }

    [TestMethod]
    public void TheHeaderIsReadWithoutTheContent()
    {
        var header = FileEnvelope.ReadHeader(Hall, JsonNode.Parse("""{ "format": "tellurian-drawing", "kind": "Hall", "schemaVersion": 1, "appVersion": "0.9.0", "content": {} }"""));

        Assert.AreEqual(new FileHeader("Hall", 1, "0.9.0", HasEnvelope: true), header);
    }

    [TestMethod]
    public void ANewerFileIsRefusedWithBothVersions()
    {
        var error = Error(() => FileEnvelope.ReadContent(Hall, JsonNode.Parse("""{ "format": "tellurian-drawing", "kind": "Hall", "schemaVersion": 3, "content": {} }""")));

        Assert.IsTrue(error is NewerVersion(3, 2), "the version found and the newest read");
    }

    [TestMethod]
    public void AnotherKindOfTheSameFormatIsRefusedWithBothKinds()
    {
        var error = Error(() => FileEnvelope.ReadContent(Hall, JsonNode.Parse("""{ "format": "tellurian-drawing", "kind": "Module", "schemaVersion": 2, "content": {} }""")));

        Assert.IsTrue(error is WrongKind("Module", "Hall"), "the kind found and the kind expected");
    }

    [TestMethod]
    public void AnotherFormatAnInvalidVersionAndNoContentAreRefused()
    {
        Assert.IsTrue(Error(() => FileEnvelope.ReadContent(Plan, JsonNode.Parse("""{ "format": "module", "schemaVersion": 2, "plan": {} }"""))) is NotThisFormat);
        Assert.IsTrue(Error(() => FileEnvelope.ReadContent(Hall, JsonNode.Parse("""{ "format": "tellurian-drawing", "kind": "Hall", "schemaVersion": 0, "content": {} }"""))) is NotThisFormat);
        Assert.IsTrue(Error(() => FileEnvelope.ReadContent(Plan, JsonNode.Parse("""{ "format": "tellurian-plan", "schemaVersion": 2 }"""))) is NotThisFormat);
    }

    [TestMethod]
    public void AMissingStepIsNamed()
    {
        var error = Error(() => FileEnvelope.Upgrade(Plan with { Migrations = [] }, new JsonObject(), 0, 2));

        Assert.IsTrue(error is MigrationMissing(0), "the version without a step");
    }

    [TestMethod]
    public void TextThatIsNoJsonIsRefused()
    {
        Assert.IsTrue(Error(() => FileEnvelope.Parse("not json")) is InvalidJson);
    }

    private static FileError Error<T>(Func<T> read) => Assert.ThrowsExactly<FileFormatException>(() => read()).Error;
}
