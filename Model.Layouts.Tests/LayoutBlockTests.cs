using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// A layout as JSON, as in the layout block of a hall plan: each location written once, everything else referred to by
/// id, and the objects put back after reading (<c>Layout.JsonSerializerOptions</c>, <see cref="HallPlanFile"/>).
/// </summary>
[TestClass]
public class LayoutBlockTests
{
    private static readonly JsonSerializerOptions Options = Layout.JsonSerializerOptions;

    /// <summary>Tellby – Ytterby with a curve between them, Ytterby controlled from Tellby.</summary>
    private static Layout HallPlanLayout()
    {
        var layout = new Layout { SourceId = Guid.Parse("20000000-0000-0000-0000-000000000002"), SourceRevision = 4 };
        var tellby = new Station(1, "Tellby", "Tby") { IsManned = true, Layout = layout };
        tellby.Tracks.Add(new StationTrack(11, "1") { Station = tellby });
        tellby.Tracks.Add(new StationTrack(12, "2") { Station = tellby });
        var ytterby = new Station(2, "Ytterby", "Yb") { ControlledBy = tellby, Layout = layout };
        ytterby.Tracks.Add(new StationTrack(21, "1") { Station = ytterby });
        layout.OperationLocations.Add(tellby);
        layout.OperationLocations.Add(ytterby);
        layout.TrackStretches.Add(new TrackStretch(1, tellby, ytterby, 12.5) { StartSide = ExitSide.Right, EndSide = ExitSide.Left, ModuleIds = [2] });
        layout.Modules.Add(new LayoutModule { Id = 1, Module = new Module { Name = "Tellby", Prefix = "SF", Number = 7, OperationLocation = tellby, Tracks = tellby.Tracks } });
        layout.Modules.Add(new LayoutModule { Id = 2, Module = new Module { Name = "Curve", Tracks = { new StationTrack(31, "1") } } });
        layout.Modules.Add(new LayoutModule { Id = 3, Module = new Module { Name = "Ytterby", OperationLocation = ytterby, Tracks = ytterby.Tracks } });
        return layout;
    }

    [TestMethod]
    public void EachLocationIsWrittenOnceAndReferredToById()
    {
        var json = JsonNode.Parse(JsonSerializer.Serialize(HallPlanLayout(), Options))!;

        var modules = json["modules"]!.AsArray();
        Assert.AreEqual(1, modules[0]!["module"]!["operationLocationId"]!.GetValue<int>());
        Assert.IsNull(modules[0]!["module"]!["operationLocation"], "the location is written in operationLocations only");
        Assert.IsNull(modules[0]!["module"]!["tracks"], "a location module's tracks are the location's");
        Assert.IsNotNull(modules[1]!["module"]!["tracks"], "a plain-line module's tracks are its own");
        Assert.AreEqual(1, json["operationLocations"]![1]!["controlledById"]!.GetValue<int>());
        Assert.AreEqual(1, json["trackStretches"]![0]!["startId"]!.GetValue<int>());
        Assert.AreEqual("Station", json["operationLocations"]![0]!["kind"]!.GetValue<string>());
    }

    [TestMethod]
    public void TheObjectsArePutBackAfterReading()
    {
        var layout = JsonSerializer.Deserialize<Layout>(JsonSerializer.Serialize(HallPlanLayout(), Options), Options)!;
        layout.RestoreReferences();

        var tellby = layout.OperationLocations.First();
        var ytterby = layout.OperationLocations.Last();
        Assert.AreSame(tellby, ytterby.ControlledBy);
        Assert.AreSame(tellby, layout.TrackStretches.Single().Start);
        Assert.AreSame(ytterby, layout.TrackStretches.Single().End);
        Assert.AreSame(tellby, tellby.Tracks.First().Station);
        var module = layout.Modules.First().Module;
        Assert.AreSame(tellby, module.OperationLocation);
        Assert.AreSame(tellby.Tracks, module.Tracks, "one list of tracks for the location and its module");
        Assert.AreEqual("Curve", layout.Modules.ElementAt(1).Module.Name);
        CollectionAssert.AreEqual(new[] { 2 }, layout.TrackStretches.Single().ModuleIds.ToArray());
    }

    [TestMethod]
    public void TheLayoutBlockIsReadFromAHallPlanFile()
    {
        var block = JsonNode.Parse(JsonSerializer.Serialize(HallPlanLayout(), Options))!;
        var file = new JsonObject
        {
            ["format"] = HallPlanFile.Format,
            ["kind"] = "HallPlan",
            ["schemaVersion"] = 2,
            ["content"] = new JsonObject { ["name"] = "Club meeting", ["layout"] = block },
        };

        var layout = HallPlanFile.ReadLayout(file.ToJsonString());

        Assert.AreEqual(4, layout.SourceRevision);
        Assert.HasCount(2, layout.OperationLocations);
        Assert.AreSame(layout.OperationLocations.First(), layout.OperationLocations.Last().ControlledBy);
    }

    [TestMethod]
    public void AnEarlierKindIsReadToo()
    {
        var block = JsonNode.Parse(JsonSerializer.Serialize(HallPlanLayout(), Options))!;
        block["operationLocations"]![1]!["kind"] = "signalControlled";

        var layout = HallPlanFile.ReadLayout(block.ToJsonString());

        Assert.IsInstanceOfType<SignalControlledLocation>(layout.OperationLocations.Last());
    }

    [TestMethod]
    public void AnotherFileOrNoLayoutBlockIsRefused()
    {
        Assert.ThrowsExactly<InvalidDataException>(() => HallPlanFile.ReadLayout("""{ "format": "tellurian-plan", "schemaVersion": 2, "plan": {} }"""));
        Assert.ThrowsExactly<InvalidDataException>(() => HallPlanFile.ReadLayout("""{ "format": "tellurian-drawing", "kind": "Module", "schemaVersion": 2, "content": {} }"""));
        var missing = Assert.ThrowsExactly<InvalidDataException>(() => HallPlanFile.ReadLayout("""{ "format": "tellurian-drawing", "kind": "HallPlan", "schemaVersion": 2, "content": { "name": "Club" } }"""));
        StringAssert.Contains(missing.Message, "no layout block");
    }
}
