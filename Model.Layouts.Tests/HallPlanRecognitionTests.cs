using System.Text;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>Telling a hall plan, or a layout block on its own, from other JSON files by their top level alone.</summary>
[TestClass]
public class HallPlanRecognitionTests
{
    private static bool IsHallPlan(string json) => HallPlanFile.IsHallPlan(Encoding.UTF8.GetBytes(json));

    [TestMethod]
    public void AHallPlanFileIsRecognised()
    {
        Assert.IsTrue(IsHallPlan("""{ "format": "tellurian-drawing", "kind": "HallPlan", "content": { "layout": { "operationLocations": [] } } }"""));
        Assert.IsTrue(IsHallPlan("""{ "content": { }, "kind": "hallplan", "format": "tellurian-drawing" }"""), "in any order, the kind in any case");
    }

    [TestMethod]
    public void ALayoutBlockOnItsOwnIsRecognised()
    {
        Assert.IsTrue(IsHallPlan("""{ "name": "Club meeting", "operationLocations": [ { "kind": "Station", "id": 1 } ] }"""));
        Assert.IsTrue(IsHallPlan("""{ "modules": [] }"""));
    }

    [TestMethod]
    public void OtherFilesAreNot()
    {
        Assert.IsFalse(IsHallPlan("""{ "format": "tellurian-plan", "schemaVersion": 3, "plan": { } }"""), "a plan file");
        Assert.IsFalse(IsHallPlan("""{ "format": "tellurian-drawing", "kind": "Module", "content": { } }"""), "a module of Module Draw");
        Assert.IsFalse(IsHallPlan("""{ "name": "Plan", "timetable": { "layout": { "operationLocations": [] } } }"""), "a plan without envelope");
        Assert.IsFalse(IsHallPlan("""{ "operationLocations": 3 }"""), "not a list");
        Assert.IsFalse(IsHallPlan("""[ 1, 2 ]"""));
        Assert.IsFalse(IsHallPlan("""{ "format": "tellurian-drawing", """), "cut short");
        Assert.IsFalse(IsHallPlan("not json"));
        Assert.IsFalse(IsHallPlan(""));
    }
}
