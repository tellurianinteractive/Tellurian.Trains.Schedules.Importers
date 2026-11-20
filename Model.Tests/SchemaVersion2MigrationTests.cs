using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>The migration of a plan from schema version 1 to 2: the locations take the names of the shared model.</summary>
[TestClass]
public sealed class SchemaVersion2MigrationTests
{
    private const string Version1Plan = """
        { "format": "tellurian-plan", "schemaVersion": 1, "plan": { "name": "Plan", "timetable": { "layout": { "id": 1, "name": "Plan",
          "operationLocations": [
            { "$type": "Station", "id": 1, "name": "Tellby", "signature": "Tby", "countryId": 1, "isManned": true,
              "tracks": [ { "id": 11, "number": "1", "isMain": true }, { "id": 12, "number": "2a" } ] },
            { "$type": "Other", "id": 2, "name": "Hållplats", "signature": "Hp", "countryId": 103, "tracks": [ { "id": 21, "number": "1" } ] }
          ] }, "trains": [] } } }
        """;

    [TestMethod]
    public void TheKindIsTheDiscriminatorWrittenFirst()
    {
        var location = Migrated()["timetable"]!["layout"]!["operationLocations"]![0]!.AsObject();

        Assert.AreEqual("kind", location.First().Key);
        Assert.AreEqual("Station", location["kind"]!.GetValue<string>());
        Assert.IsFalse(location.ContainsKey("$type"));
    }

    [TestMethod]
    public void ATrackIsDesignatedAndACountryIsItsCode()
    {
        var locations = Migrated()["timetable"]!["layout"]!["operationLocations"]!.AsArray();

        Assert.AreEqual("2a", locations[0]!["tracks"]![1]!["designation"]!.GetValue<string>());
        Assert.IsNull(locations[0]!["tracks"]![1]!["number"]);
        Assert.AreEqual("SE", locations[0]!["countryCode"]!.GetValue<string>());
        Assert.AreEqual("BE", locations[1]!["countryCode"]!.GetValue<string>());
        Assert.IsNull(locations[0]!["countryId"]);
    }

    [TestMethod]
    public void APlanOfVersion1IsReadIntoTheSharedModel()
    {
        var plan = PlanFile.Read(Version1Plan);
        var locations = plan.Layout.OperationLocations.ToList();

        Assert.IsInstanceOfType<Station>(locations[0]);
        Assert.IsTrue(((Station)locations[0]).IsManned);
        Assert.AreEqual(1, locations[0].CountryId);
        Assert.AreEqual("2a", locations[0].Tracks.Last().Designation);
        Assert.IsInstanceOfType<OtherLocation>(locations[1]);
        Assert.AreEqual(103, locations[1].CountryId);
    }

    private static JsonNode Migrated() =>
        SchemaVersion2.Migration.Migrate(JsonNode.Parse(Version1Plan)!["plan"]!.DeepClone());
}
