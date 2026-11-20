using System.Text.Json;
using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>The envelope of a plan file and its chain of migrations (<c>Documentation/ModelHarmonisation.md</c>, section 5).</summary>
[TestClass]
public sealed class PlanFileTests
{
    private const string VarnamoFileName = "Plan.Varnamo-Moduldagar.json";

    [TestMethod]
    public void APlanIsWrittenInAnEnvelopeWithTheCurrentSchemaVersion()
    {
        var root = JsonNode.Parse(PlanFile.Write(ReadVarnamo()))!.AsObject();

        Assert.AreEqual(PlanFile.Format, root["format"]!.GetValue<string>());
        Assert.AreEqual(PlanFile.CurrentSchemaVersion, root["schemaVersion"]!.GetValue<int>());
        Assert.IsFalse(string.IsNullOrWhiteSpace(root["appVersion"]!.GetValue<string>()));
        Assert.IsInstanceOfType<JsonObject>(root["plan"]);
    }

    [TestMethod]
    public void APlanWithoutAnEnvelopeIsReadAsVersionZero()
    {
        var json = VarnamoJson();

        Assert.AreEqual(0, PlanFile.SchemaVersionOf(JsonNode.Parse(json, documentOptions: new JsonDocumentOptions { MaxDepth = 256 })));
        var plan = PlanFile.Read(json);
        Assert.AreEqual(31, plan.Layout.OperationLocations.Count);
        Assert.AreEqual(275, plan.Timetable.Trains.Count);
    }

    [TestMethod]
    public void APlanReadsTheSameAfterBeingWrittenInTheEnvelope()
    {
        var plan = ReadVarnamo();

        var restored = PlanFile.Read(PlanFile.Write(plan, writeIndented: true));

        Assert.AreEqual(plan.Layout.OperationLocations.Count, restored.Layout.OperationLocations.Count);
        Assert.AreEqual(plan.Timetable.Trains.Sum(t => t.Calls.Count), restored.Timetable.Trains.Sum(t => t.Calls.Count));
        Assert.AreEqual(plan.Schedules.Count, restored.Schedules.Count);
        Assert.AreEqual(plan.DriverDuties.Count, restored.DriverDuties.Count);
    }

    [TestMethod]
    public void APlanOfANewerSchemaVersionIsRefused()
    {
        var json = $$"""{ "format": "{{PlanFile.Format}}", "schemaVersion": {{PlanFile.CurrentSchemaVersion + 1}}, "plan": {} }""";

        var exception = Assert.ThrowsExactly<PlanFileException>(() => PlanFile.Read(json));
        Assert.AreEqual(PlanFileError.NewerVersion, exception.Error);
    }

    [TestMethod]
    public void AFileOfAnotherFormatIsRefused()
    {
        var exception = Assert.ThrowsExactly<PlanFileException>(() => PlanFile.Read("""{ "format": "module", "schemaVersion": 2 }"""));
        Assert.AreEqual(PlanFileError.NotAPlan, exception.Error);
    }

    [TestMethod]
    public void TextThatIsNotJsonIsRefused()
    {
        var exception = Assert.ThrowsExactly<PlanFileException>(() => PlanFile.Read("not json"));
        Assert.AreEqual(PlanFileError.NotAPlan, exception.Error);
    }

    [TestMethod]
    public void TheStepsRunInOrderFromTheVersionOfThePlan()
    {
        IReadOnlyList<PlanFileMigration> steps =
        [
            new(1, content => Append(content, "1")),
            new(0, content => Append(content, "0")),
            new(2, content => Append(content, "2")),
        ];

        var result = PlanFileMigrations.Upgrade(new JsonObject { ["steps"] = "" }, 0, 3, steps);

        Assert.AreEqual("012", result["steps"]!.GetValue<string>());
        Assert.AreEqual("12", PlanFileMigrations.Upgrade(new JsonObject { ["steps"] = "" }, 1, 3, steps)["steps"]!.GetValue<string>());

        static JsonNode Append(JsonNode content, string step)
        {
            content["steps"] = $"{content["steps"]!.GetValue<string>()}{step}";
            return content;
        }
    }

    [TestMethod]
    public void AMissingStepIsReported()
    {
        var exception = Assert.ThrowsExactly<PlanFileException>(() => PlanFileMigrations.Upgrade(new JsonObject(), 0, 2, [new(0, content => content)]));
        Assert.AreEqual(PlanFileError.MigrationMissing, exception.Error);
    }

    [TestMethod]
    public void TheChainReachesTheCurrentSchemaVersion()
    {
        for (var version = 0; version < PlanFile.CurrentSchemaVersion; version++)
            Assert.IsTrue(PlanFileMigrations.All.Any(m => m.FromVersion == version), $"No step from schema version {version}.");
    }

    private static string VarnamoJson() => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", VarnamoFileName));

    private static Plan ReadVarnamo() => PlanFile.Read(VarnamoJson());
}
