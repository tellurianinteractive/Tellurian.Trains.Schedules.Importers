using System.Text.Json;
using Tellurian.Trains.Schedules.Model.Files;
using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Layouts.Catalogues;
using Tellurian.Trains.Schedules.Model.Layouts.Drawings;
using Tellurian.Trains.Schedules.Model.Layouts.Modules;
using Tellurian.Trains.Schedules.Model.Layouts.Tracks;

namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>Sides refer to exits by side and number, and track elements have whole numbers as ids (survey step 6b).</summary>
[TestClass]
public class ModuleDrawingTests
{
    private static readonly CatalogueReference Profile = new(Guid.Parse("5b6f0c1e-2a41-4d8e-9c3a-0f1d2e3a4d01"), "Profile");

    private static ModuleExit Exit(string label, ExitSide direction, double x, double y, double heading) =>
        new() { Label = label, Direction = direction, EndProfile = Profile, Pose = new(new(x, y), new Angle(heading)) };

    private static ModuleSide Side(ExitPlace from, ExitPlace to) =>
        new() { FromExit = from, ToExit = to, Path = PathDrawing.Polyline([new(0, 0), new(1, 0)]) };

    /// <summary>A junction: left and right along X, and a branch leaving across, whose direction isn't given yet.</summary>
    private static ModuleDrawing Junction() => new()
    {
        Exits = [Exit("A", ExitSide.Left, 0, 0, 180), Exit("B", ExitSide.Right, 1000, 0, 0), Exit("C", ExitSide.Undefined, 500, 300, 90)],
        Sides =
        [
            Side(ExitPlace.Left, ExitPlace.Right),
            Side(ExitPlace.Right, new(ExitSide.Undefined)),
            Side(new(ExitSide.Undefined), ExitPlace.Left),
        ],
    };

    [TestMethod]
    public void AnExitIsFoundByItsPlace()
    {
        var drawing = Junction();

        Assert.AreEqual("B", drawing.Exit(ExitPlace.Right)?.Label);
        Assert.AreEqual("C", drawing.Exit(new ExitPlace(ExitSide.Undefined))?.Label);
        Assert.IsNull(drawing.Exit(new ExitPlace(ExitSide.Left, 2)));
    }

    [TestMethod]
    public void AnInferredDirectionMovesTheSidesWithTheExit()
    {
        var inferred = Junction().WithInferredExitDirections();

        Assert.AreEqual(ExitSide.Up, inferred.Exits[2].Direction);
        Assert.AreEqual(new ExitPlace(ExitSide.Up), inferred.Sides[1].ToExit);
        Assert.AreEqual(new ExitPlace(ExitSide.Up), inferred.Sides[2].FromExit);
        Assert.AreEqual((ExitPlace.Left, ExitPlace.Right), (inferred.Sides[0].FromExit, inferred.Sides[0].ToExit));
    }

    [TestMethod]
    public void ExitsMovedInTheListKeepTheirSides()
    {
        var drawing = new ModuleDrawing
        {
            Exits = [Exit("R1", ExitSide.Right, 0, 0, 0), Exit("R2", ExitSide.Right, 0, 100, 0)],
            Sides = [Side(ExitPlace.Right, new(ExitSide.Right, 2))],
        };

        drawing.ChangeExits(exits => exits.Reverse());

        Assert.AreEqual((new ExitPlace(ExitSide.Right, 2), ExitPlace.Right), (drawing.Sides[0].FromExit, drawing.Sides[0].ToExit));
        Assert.AreEqual(("R1", "R2"), (drawing.Exit(drawing.Sides[0].FromExit)!.Label, drawing.Exit(drawing.Sides[0].ToExit)!.Label));
    }

    private static BufferStop Stop(int id) => new() { Id = id, Pose = new(new(0, 0), Angle.Zero) };

    [TestMethod]
    public void TrackElementsWithoutIdsAreNumbered()
    {
        var drawing = new ModuleDrawing
        {
            TrackElements = [Stop(2), Stop(0), Stop(2), Stop(0)],
        };

        drawing.NumberTrackElements();

        CollectionAssert.AreEqual(new[] { 2, 1, 3, 4 }, drawing.TrackElements.Select(e => e.Id).ToArray());
        Assert.AreEqual(5, drawing.FreeTrackElementId());
    }

    [TestMethod]
    public void TrackElementsAreNumberedWhenWrittenAndRead()
    {
        var drawing = new ModuleDrawing { TrackElements = [Stop(0), Stop(0)] };

        var json = JsonSerializer.Serialize(drawing, DrawingJsonContext.Default.ModuleDrawing);
        var stops = JsonSerializer.Deserialize(json, DrawingJsonContext.Default.ModuleDrawing)!;
        var file = System.Text.Json.Nodes.JsonNode.Parse(json)!;
        file["trackElements"]![0]!["id"] = 3;
        file["trackElements"]![1]!.AsObject().Remove("id");
        var read = JsonSerializer.Deserialize(file.ToJsonString(), DrawingJsonContext.Default.ModuleDrawing)!;

        CollectionAssert.AreEqual(new[] { 1, 2 }, stops.TrackElements.Select(e => e.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 3, 1 }, read.TrackElements.Select(e => e.Id).ToArray());
    }
}
