using System.Text.Json.Nodes;
using Tellurian.Trains.Schedules.Model.PlanFiles;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>The migration of a plan from schema version 2 to 3: platforms given once to the plans written without them.</summary>
[TestClass]
public sealed class SchemaVersion3MigrationTests
{
    private const string Version2Plan = """
        { "format": "tellurian-plan", "schemaVersion": 2, "plan": { "name": "Plan", "timetable": { "layout": { "id": 1, "name": "Plan",
          "operationLocations": [
            { "kind": "Station", "id": 1, "name": "Tellby", "signature": "Tby", "hasPassengerExchange": true,
              "tracks": [ { "id": 11, "designation": "1" }, { "id": 12, "designation": "2", "platformLength": 0 } ] },
            { "kind": "Station", "id": 2, "name": "Ytterby", "signature": "Yb", "hasPassengerExchange": true,
              "tracks": [ { "id": 21, "designation": "1", "platformLength": 120 }, { "id": 22, "designation": "2" } ] },
            { "kind": "Station", "id": 3, "name": "Godsby", "signature": "Gb", "hasPassengerExchange": false,
              "tracks": [ { "id": 31, "designation": "1" } ] },
            { "kind": "Station", "id": 4, "name": "Skuggby", "signature": "Sk", "hasPassengerExchange": false, "isShadow": true,
              "tracks": [ { "id": 41, "designation": "1" } ] },
            { "kind": "SignalControlled", "id": 5, "name": "Blocket", "signature": "Bl", "tracks": [ { "id": 51, "designation": "1" } ] },
            { "kind": "Other", "id": 6, "name": "Hållplats", "signature": "Hp", "tracks": [ { "id": 61, "designation": "1" } ] }
          ] }, "trains": [] } } }
        """;

    [TestMethod]
    public void EveryTrackOfAPassengerLocationWithoutPlatformsGetsOne()
    {
        Assert.AreEqual("1,1", Platforms("Tby"));
        Assert.AreEqual("1", Platforms("Sk"), "a shadow station always exchanges passengers");
        Assert.AreEqual("1", Platforms("Hp"), "passenger exchange is true when not written");
    }

    [TestMethod]
    public void ALocationWithAPlatformOrWithoutPassengersIsLeftAlone()
    {
        Assert.AreEqual("120,0", Platforms("Yb"));
        Assert.AreEqual("0", Platforms("Gb"));
        Assert.AreEqual("0", Platforms("Bl"));
    }

    [TestMethod]
    public void AVersion2PlanReadsWithItsPlatforms()
    {
        var plan = PlanFile.Read(Version2Plan);

        var tellby = plan.Layout.OperationLocations.Single(l => l.Signature == "Tby");
        Assert.IsTrue(tellby.Tracks.All(t => t.PlatformLength == StationTrack.DefaultPlatformLength));
    }

    private static string Platforms(string signature)
    {
        var locations = SchemaVersion3.Migration.Migrate(JsonNode.Parse(Version2Plan)!["plan"]!.DeepClone())["timetable"]!["layout"]!["operationLocations"]!.AsArray();
        var location = locations.Single(l => l!["signature"]!.GetValue<string>() == signature)!;
        return string.Join(",", location["tracks"]!.AsArray().Select(t => t!["platformLength"]?.GetValue<double>() ?? 0));
    }
}
