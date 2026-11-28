using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>The migration of a plan from schema version 5 to 6: the plan keeps the result of the hall plan, not its drawings.</summary>
[TestClass]
public sealed class SchemaVersion6MigrationTests
{
    private const string Version5Plan = """
        { "format": "tellurian-plan", "schemaVersion": 5, "plan": { "name": "Plan", "timetable": { "layout": { "id": 1, "name": "Plan",
          "operationLocations": [ { "kind": "Station", "id": 4, "name": "Tellby", "signature": "Tby" } ],
          "sourceId": "30000000-0000-0000-0000-000000000003", "sourceRevision": 2,
          "preferredStandard": { "id": "5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4b01", "shortName": "FREMO/H0-RE", "scale": { "id": 3, "shortName": "H0" } },
          "modules": [
            { "id": 1, "module": { "id": "8c0a0d6e-0000-4000-8000-000000000001", "name": "Tellby", "prefix": "SF", "number": 7, "moduleOwners": "Stefan",
              "operationLocationId": 4, "drawing": { "exits": [] } },
              "externalReferences": { "moduleRegistry": { "moduleId": 12 } } },
            { "id": 2, "module": { "name": "Curve", "packageLabel": "Box 1", "tracks": [ { "id": 9, "designation": "1" } ] } }
          ],
          "lines": [ { "id": 1, "entries": [ { "moduleId": 1 }, { "moduleId": 2 } ] } ],
          "joints": [], "endProfiles": [ { "id": "7c0a0d6e-0000-4000-8000-000000000001" } ], "standards": [], "overrides": [],
          "fitTolerances": { "joint": 1 } }, "trains": [] } } }
        """;

    private static JsonObject Migrated() =>
        SchemaVersion6.Migration.Migrate(JsonNode.Parse(Version5Plan)!["plan"]!.DeepClone())["timetable"]!["layout"]!.AsObject();

    [TestMethod]
    public void TheDrawingsAreDropped()
    {
        var layout = Migrated();

        foreach (var name in new[] { "lines", "joints", "endProfiles", "standards", "overrides", "fitTolerances", "preferredStandard" })
            Assert.IsFalse(layout.ContainsKey(name), name);
        Assert.IsTrue(layout.ContainsKey("operationLocations"));
        Assert.AreEqual(2, layout["sourceRevision"]!.GetValue<int>());
    }

    [TestMethod]
    public void ThePreferredStandardBecomesScaleAndStandardName()
    {
        var layout = Migrated();

        Assert.AreEqual("FREMO/H0-RE", layout["standardName"]!.GetValue<string>());
        Assert.AreEqual(3, layout["scale"]!["id"]!.GetValue<int>());
    }

    [TestMethod]
    public void EachModuleKeepsWhatThePlannerShows()
    {
        var modules = Migrated()["modules"]!.AsArray();

        var tellby = modules[0]!.AsObject();
        Assert.IsFalse(tellby.ContainsKey("module"));
        Assert.AreEqual(1, tellby["id"]!.GetValue<int>());
        Assert.AreEqual("8c0a0d6e-0000-4000-8000-000000000001", tellby["sourceId"]!.GetValue<string>());
        Assert.AreEqual("SF 0007 Tellby", tellby["title"]!.GetValue<string>());
        Assert.AreEqual("Stefan", tellby["owners"]!.GetValue<string>());
        Assert.AreEqual(4, tellby["operationLocationId"]!.GetValue<int>());
        Assert.AreEqual(12, tellby["externalReferences"]!["moduleRegistry"]!["moduleId"]!.GetValue<int>());
        var curve = modules[1]!.AsObject();
        Assert.AreEqual("Curve", curve["title"]!.GetValue<string>());
        Assert.AreEqual("Box 1", curve["packageLabel"]!.GetValue<string>());
        Assert.IsFalse(curve.ContainsKey("operationLocationId"));
    }

    [TestMethod]
    public void AVersion5PlanCanBeRead()
    {
        var plan = PlanFile.Read(Version5Plan);

        Assert.AreEqual("FREMO/H0-RE", plan.Layout.StandardName);
        Assert.AreEqual("H0", plan.Layout.Scale!.ShortName);
        CollectionAssert.AreEqual(new[] { "SF 0007 Tellby", "Curve" }, plan.Layout.Modules.Select(m => m.Title).ToArray());
        Assert.AreEqual(plan.Layout.OperationLocations.Single().Id, plan.Layout.Modules.First().OperationLocationId);
    }
}
