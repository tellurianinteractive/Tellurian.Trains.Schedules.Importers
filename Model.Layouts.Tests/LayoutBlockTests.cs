using System.Text.Json;
using System.Text.Json.Nodes;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>
/// A drawn layout as JSON, as in the layout block of a hall plan: each location written once, everything else referred
/// to by id, and the objects put back after reading (<see cref="DrawnLayout.ToBlock"/>, <see cref="HallPlanFile"/>).
/// </summary>
[TestClass]
public class LayoutBlockTests
{
    /// <summary>Tellby – Ytterby with a curve between them, Ytterby controlled from Tellby.</summary>
    private static DrawnLayout HallPlanLayout()
    {
        var drawn = new DrawnLayout { Layout = new Layout { SourceId = Guid.Parse("20000000-0000-0000-0000-000000000002"), SourceRevision = 4 } };
        var layout = drawn.Layout;
        var tellby = new Station(1, "Tellby", "Tby") { IsManned = true, Layout = layout };
        tellby.Tracks.Add(new StationTrack(11, "1") { Station = tellby });
        tellby.Tracks.Add(new StationTrack(12, "2") { Station = tellby });
        var ytterby = new Station(2, "Ytterby", "Yb") { ControlledBy = tellby, Layout = layout };
        ytterby.Tracks.Add(new StationTrack(21, "1") { Station = ytterby });
        layout.OperationLocations.Add(tellby);
        layout.OperationLocations.Add(ytterby);
        layout.TrackStretches.Add(new TrackStretch(1, tellby, ytterby, 12.5) { StartSide = ExitSide.Right, EndSide = ExitSide.Left, ModuleIds = [2] });
        drawn.Modules.Add(new PlacedModule { Id = 1, Module = new Module { Name = "Tellby", Prefix = "SF", Number = 7, OperationLocation = tellby, Tracks = tellby.Tracks } });
        drawn.Modules.Add(new PlacedModule { Id = 2, Module = new Module { Name = "Curve", Tracks = { new StationTrack(31, "1") } } });
        drawn.Modules.Add(new PlacedModule { Id = 3, Module = new Module { Name = "Ytterby", OperationLocation = ytterby, Tracks = ytterby.Tracks } });
        return drawn;
    }

    [TestMethod]
    public void EachLocationIsWrittenOnceAndReferredToById()
    {
        var json = HallPlanLayout().ToBlock();

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
        var drawn = DrawnLayout.FromBlock(JsonNode.Parse(HallPlanLayout().ToBlock().ToJsonString())!.AsObject());
        var layout = drawn.Layout;

        var tellby = layout.OperationLocations.First();
        var ytterby = layout.OperationLocations.Last();
        Assert.AreSame(tellby, ytterby.ControlledBy);
        Assert.AreSame(tellby, layout.TrackStretches.Single().Start);
        Assert.AreSame(ytterby, layout.TrackStretches.Single().End);
        Assert.AreSame(tellby, tellby.Tracks.First().Station);
        var module = drawn.Modules.First().Module;
        Assert.AreSame(tellby, module.OperationLocation);
        Assert.AreSame(tellby.Tracks, module.Tracks, "one list of tracks for the location and its module");
        Assert.AreEqual("Curve", drawn.Modules.ElementAt(1).Module.Name);
        CollectionAssert.AreEqual(new[] { 2 }, layout.TrackStretches.Single().ModuleIds.ToArray());
    }

    [TestMethod]
    public void TheLayoutBlockIsReadFromAHallPlanFile()
    {
        var block = HallPlanLayout().ToBlock();
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
        CollectionAssert.AreEqual(new[] { "SF 0007 Tellby", "Curve", "Ytterby" }, layout.Modules.Select(m => m.Title).ToArray(), "the modules without their drawings");
        Assert.AreEqual(1, layout.Modules.First().OperationLocationId);
        Assert.IsNull(layout.Modules.ElementAt(1).OperationLocationId);
    }

    [TestMethod]
    public void TheBlockKeepsTheDrawnPartsBesideTheLayout()
    {
        var drawn = HallPlanLayout();
        drawn.PreferredStandard = new ModuleStandard { Id = Guid.NewGuid(), ShortName = "FREMO H0-RE" };
        drawn.Joints.Add(new Joint { First = new(1, ExitSide.Right), Second = new(2, ExitSide.Left) });

        var block = drawn.ToBlock();
        var read = DrawnLayout.FromBlock(block);

        Assert.AreEqual("FREMO H0-RE", block["preferredStandard"]!["shortName"]!.GetValue<string>());
        Assert.HasCount(1, block["joints"]!.AsArray());
        Assert.AreEqual("FREMO H0-RE", read.PreferredStandard!.ShortName);
        Assert.HasCount(1, read.Joints);
        Assert.HasCount(3, read.Modules);
        Assert.IsEmpty(read.Layout.Modules, "the block has the modules themselves, not the planner's summaries");
    }

    [TestMethod]
    public void AnEarlierKindIsReadToo()
    {
        var block = HallPlanLayout().ToBlock();
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
