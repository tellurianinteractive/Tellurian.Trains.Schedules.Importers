using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>The migration of a plan from schema version 4 to 5: a standard's Registry number becomes an external reference.</summary>
[TestClass]
public sealed class SchemaVersion5MigrationTests
{
    private const string StandardGuid = "5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4b01";

    private const string Version4Plan = """
        { "format": "tellurian-plan", "schemaVersion": 4, "plan": { "name": "Plan", "timetable": { "layout": { "id": 1, "name": "Plan",
          "preferredStandard": { "id": 7, "shortName": "FREMO/H0-RE", "scale": { "id": 3, "shortName": "H0" } },
          "modules": [
            { "id": 1, "module": { "name": "Tellby", "standard": { "id": 7, "shortName": "FREMO/H0-RE" } } },
            { "id": 2, "module": { "name": "Curve", "standard": { "id": "5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4b01", "shortName": "FREMO/H0-RE" } } }
          ] }, "trains": [] } } }
        """;

    private static JsonNode Migrated() => SchemaVersion5.Migration.Migrate(JsonNode.Parse(Version4Plan)!["plan"]!.DeepClone());

    [TestMethod]
    public void TheRegistryNumberOfTheStandardBecomesAnExternalReference()
    {
        var standard = Migrated()["timetable"]!["layout"]!["preferredStandard"]!.AsObject();

        Assert.IsFalse(standard.ContainsKey("id"));
        Assert.AreEqual(7, standard["externalReferences"]!["moduleRegistry"]!["standardId"]!.GetValue<int>());
        Assert.AreEqual(3, standard["scale"]!["id"]!.GetValue<int>(), "A scale keeps its id: the suite's list of scales.");
    }

    [TestMethod]
    public void AModulesStandardIsMovedAndAGuidIsKept()
    {
        var modules = Migrated()["timetable"]!["layout"]!["modules"]!.AsArray();

        Assert.AreEqual(7, modules[0]!["module"]!["standard"]!["externalReferences"]!["moduleRegistry"]!["standardId"]!.GetValue<int>());
        Assert.AreEqual(StandardGuid, modules[1]!["module"]!["standard"]!["id"]!.GetValue<string>());
    }

    [TestMethod]
    public void AVersion4PlanWithARegistryNumberCanBeRead()
    {
        var plan = PlanFile.Read(Version4Plan);

        Assert.AreEqual("FREMO/H0-RE", plan.Layout.StandardName, "since version 6 the plan keeps the standard by name");
        Assert.AreEqual(3, plan.Layout.Scale!.Id);
        Assert.AreEqual("Curve", plan.Layout.Modules.ElementAt(1).Title);
    }
}
