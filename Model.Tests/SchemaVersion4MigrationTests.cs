using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// The migration of a plan from schema version 3 to 4, which removes a location's <c>isSignal</c>, and the default
/// values left out of a plan from version 4.
/// </summary>
[TestClass]
public sealed class SchemaVersion4MigrationTests
{
    private const string VarnamoFileName = "Plan.Varnamo-Moduldagar.json";

    private const string Version3Plan = """
        { "format": "tellurian-plan", "schemaVersion": 3, "plan": { "name": "Plan", "timetable": { "layout": { "id": 1, "name": "Plan",
          "operationLocations": [
            { "kind": "Station", "id": 1, "name": "Tellby", "signature": "Tby", "isSignal": true,
              "tracks": [ { "id": 11, "designation": "1", "isMain": true, "isScheduled": true, "platformLength": 1 } ] }
          ] }, "trains": [] } } }
        """;

    [TestMethod]
    public void IsSignalIsRemoved()
    {
        var content = SchemaVersion4.Migration.Migrate(JsonNode.Parse(Version3Plan)!["plan"]!.DeepClone());

        var location = content["timetable"]!["layout"]!["operationLocations"]![0]!.AsObject();
        Assert.IsFalse(location.ContainsKey("isSignal"));
        Assert.AreEqual("Tby", location["signature"]!.GetValue<string>());
    }

    [TestMethod]
    public void IsSignalIsNotKeptAmongTheUnknownProperties()
    {
        var plan = PlanFile.Read(Version3Plan);

        var location = plan.Layout.OperationLocations.Single();
        Assert.IsFalse(location.ExtensionData?.ContainsKey("isSignal") ?? false);
        Assert.DoesNotContain("isSignal", PlanFile.Write(plan));
    }

    [TestMethod]
    public void AValueIsLeftOutOnlyWhenItIsTheDefaultOfItsType()
    {
        var plan = PlanFile.Read(Version3Plan);
        var location = plan.Layout.OperationLocations.Single();
        location.Tracks.Add(new StationTrack(12, "2", isMain: false, isScheduled: false));
        location.HideMeets = true;

        var written = Location(PlanFile.Write(plan));
        var tracks = written["tracks"]!.AsArray().Select(t => t!.AsObject()).ToList();

        Assert.IsTrue(tracks[0]["isScheduled"]!.GetValue<bool>(), "a flag that starts as true is always written");
        Assert.IsFalse(tracks[1]["isScheduled"]!.GetValue<bool>(), "and written when false");
        Assert.IsFalse(tracks[1].ContainsKey("isMain"), "a flag that is false, the default of its type, is left out");
        Assert.IsTrue(written["hideMeets"]!.GetValue<bool>(), "a flag set away from its default is written");
        Assert.AreEqual(string.Empty, written["instructions"]!.GetValue<string>(), "an empty text is no null, so it is written");

        var read = PlanFile.Read(PlanFile.Write(plan)).Layout.OperationLocations.Single();
        Assert.IsTrue(read.Tracks[0].IsScheduled);
        Assert.IsFalse(read.Tracks[1].IsScheduled);
        Assert.IsTrue(read.HideMeets);
        Assert.AreEqual(string.Empty, read.Instructions);
    }

    [TestMethod]
    public void VarnamoReadsTheSameWithTheDefaultsLeftOut()
    {
        var plan = PlanFile.Read(VarnamoJson());
        var expected = PlanFingerprint.Of(plan);

        var json = PlanFile.Write(plan);
        PlanFingerprint.AssertSame(expected, PlanFingerprint.Of(PlanFile.Read(json)));
        PlanFingerprint.AssertSame(expected, PlanFingerprint.Of(PlanFile.Read(PlanFile.Write(PlanFile.Read(json)))));
    }

    private static JsonObject Location(string planFile) =>
        JsonNode.Parse(planFile)!["plan"]!["timetable"]!["layout"]!["operationLocations"]![0]!.AsObject();

    private static string VarnamoJson() => File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", VarnamoFileName));
}
