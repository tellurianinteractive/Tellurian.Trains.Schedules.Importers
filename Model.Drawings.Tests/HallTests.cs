using Tellurian.Trains.Schedules.Model.Geometry;
using Tellurian.Trains.Schedules.Model.Drawings;
using Tellurian.Trains.Schedules.Model.Drawings.Halls;

namespace Tellurian.Trains.Schedules.Model.Drawings.Tests;

/// <summary>A hall's walls, obstacles, doors and areas, moved from Module Draw (survey step 5).</summary>
[TestClass]
public sealed class HallTests
{
    private const double Width = 12000;
    private const double Depth = 8000;

    /// <summary>A 12 × 8 m hall with two pillars, furniture, a door, an emergency exit and a no-go zone.</summary>
    private static Hall Club()
    {
        var hall = Hall.Rectangle("Club hall", Width, Depth);
        hall.Add(Obstacle.RoundPillar(new(4000, 4000), 400));
        hall.Add(Obstacle.RectangularPillar(new(8000, 4000), 300, 500, new(15)));
        hall.Add(new Obstacle { Kind = ObstacleKind.Furniture, Area = PathDrawing.Rectangle(new(11000, 7000), 2000, 1000, Angle.Zero) });
        hall.Add(new Door { Position = new(new(1500, 0), new(90)), Width = 1000, IsModuleEntrance = true });
        hall.Add(new Door { Kind = DoorKind.EmergencyExit, Position = new(new(Width, 4000), new(180)), Width = 1200, KeepFreeDepth = 1500 });
        hall.Add(new NoGoZone { Area = PathDrawing.Rectangle(new(6000, 7800), 800, 400, Angle.Zero), Reason = "Fire hose cabinet" });
        return hall;
    }

    [TestMethod]
    public void RectangleHasFourWallsCounterClockwise()
    {
        var hall = Hall.Rectangle("Hall", Width, Depth);
        Assert.AreEqual(4, hall.WallCount);
        Assert.IsTrue(hall.Outline.IsClosed);
        Assert.IsTrue(hall.Wall(1).Start.IsCloseTo(new(Width, 0), Precision));
        Assert.IsTrue(hall.Wall(1).End.IsCloseTo(new(Width, Depth), Precision));
        Assert.AreEqual(2 * (Width + Depth), hall.Outline.ToPath().Length, Precision);
    }

    [TestMethod]
    public void MissingWallIsRefused()
    {
        var hall = Hall.Rectangle("Hall", Width, Depth);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => hall.Wall(4));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => hall.Wall(-1));
    }

    [TestMethod]
    public void RoundPillarIsACircle()
    {
        var pillar = Obstacle.RoundPillar(new(4000, 3000), 400);
        var path = pillar.Area.ToPath();
        Assert.AreEqual(ObstacleKind.Pillar, pillar.Kind);
        Assert.IsTrue(path.IsClosed);
        Assert.AreEqual(Math.PI * 400, path.Length, Precision);
        Assert.AreEqual(400, path.Bounds.Width, Precision);
        Assert.IsTrue(path.Bounds.Center.IsCloseTo(new(4000, 3000), Precision));
    }

    [TestMethod]
    public void RectangularPillarIsCentredAndTurned()
    {
        var bounds = Obstacle.RectangularPillar(new(8000, 4000), 300, 500, new(90)).Area.ToPath().Bounds;
        Assert.AreEqual(500, bounds.Width, Precision);
        Assert.AreEqual(300, bounds.Height, Precision);
        Assert.IsTrue(bounds.Center.IsCloseTo(new(8000, 4000), Precision));
    }

    [TestMethod]
    public void RecognisesPillarShapes()
    {
        Assert.IsTrue(Obstacle.RoundPillar(new(4000, 3000), 400).TryGetCircle(out var center, out var diameter));
        Assert.IsTrue(center.IsCloseTo(new(4000, 3000), Precision));
        Assert.AreEqual(400, diameter, Precision);
        Assert.IsFalse(Obstacle.RoundPillar(new(4000, 3000), 400).TryGetRectangle(out _, out _, out _, out _));

        var square = Obstacle.RectangularPillar(new(8000, 4000), 300, 500, new(15));
        Assert.IsFalse(square.TryGetCircle(out _, out _));
        Assert.IsTrue(square.TryGetRectangle(out var squareCenter, out var width, out var depth, out var rotation));
        Assert.IsTrue(squareCenter.IsCloseTo(new(8000, 4000), Precision));
        Assert.AreEqual(300, width, Precision);
        Assert.AreEqual(500, depth, Precision);
        Assert.AreEqual(15, rotation.Degrees, Precision);
    }

    [TestMethod]
    public void AQuadrilateralThatIsNoRectangleIsNotRecognisedAsOne()
    {
        var area = PathDrawing.Polyline([new(0, 0), new(1000, 0), new(1200, 800), new(0, 800)], isClosed: true);
        Assert.IsFalse(area.TryGetRectangle(out _, out _, out _, out _));
    }

    [TestMethod]
    public void KeepFreeZoneIsInFrontOfTheDoor()
    {
        var door = new Door { Position = new(new(2000, 0), new(90)), Width = 1000 };
        var bounds = door.KeepFreeZone().ToPath().Bounds;
        Assert.IsTrue(bounds.Min.IsCloseTo(new(1500, 0), Precision));
        Assert.IsTrue(bounds.Max.IsCloseTo(new(2500, Door.DefaultKeepFreeDepth), Precision));
    }

    [TestMethod]
    public void KeepFreeZoneFollowsTheWall()
    {
        var door = new Door { Position = new(new(Width, 4000), new(180)), Width = 1200, KeepFreeDepth = 1500 };
        var bounds = door.KeepFreeZone().ToPath().Bounds;
        Assert.IsTrue(bounds.Min.IsCloseTo(new(10500, 3400), Precision));
        Assert.IsTrue(bounds.Max.IsCloseTo(new(Width, 4600), Precision));
    }

    [TestMethod]
    public void ItemsGetIdsUniqueAmongTheirKind()
    {
        var hall = Club();
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, hall.Obstacles.Select(o => o.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2 }, hall.Doors.Select(d => d.Id).ToArray());
        Assert.AreEqual(1, hall.NoGoZones[0].Id);
    }

    [TestMethod]
    public void AnItemKeepsAFreeIdAndGetsTheNextOneForATakenId()
    {
        var hall = Club();
        Assert.AreEqual(7, hall.Add(Obstacle.RoundPillar(new(2000, 2000), 300).With(o => o.Id = 7)).Id);
        Assert.AreEqual(8, hall.Add(Obstacle.RoundPillar(new(2500, 2000), 300).With(o => o.Id = 2)).Id);
    }

    [TestMethod]
    public void FindsObstaclesById()
    {
        var hall = Club();
        Assert.AreSame(hall.Obstacles[1], hall.FindObstacle(hall.Obstacles[1].Id));
        Assert.IsNull(hall.FindObstacle(99));
    }

    [TestMethod]
    public void ACopyOwnsItsItems()
    {
        var hall = Club();
        var copy = hall.With(h =>
        {
            h.Obstacles[0].Label = "Changed";
            h.Doors.RemoveAt(0);
        });
        Assert.IsNull(hall.Obstacles[0].Label);
        Assert.HasCount(2, hall.Doors);
        Assert.AreEqual("Changed", copy.Obstacles[0].Label);
        Assert.HasCount(1, copy.Doors);
    }

    [TestMethod]
    public void DoorOnACounterClockwiseWallHeadsIntoTheHall()
    {
        var hall = Hall.Rectangle("Hall", Width, Depth);
        Assert.IsTrue(hall.PoseOnWall(new(3000, -200)).IsCloseTo(new(new(3000, 0), new(90)), Precision));
        Assert.IsTrue(hall.PoseOnWall(new(11900, 5000)).IsCloseTo(new(new(Width, 5000), new(180)), Precision));
    }

    [TestMethod]
    public void DoorOnAClockwiseWallHeadsIntoTheHall()
    {
        var outline = PathDrawing.Polyline([new(0, 0), new(0, Depth), new(Width, Depth), new(Width, 0)], isClosed: true);
        Assert.IsTrue(Hall.PoseOnWall(outline, new(3000, 100)).IsCloseTo(new(new(3000, 0), new(90)), Precision));
    }

    [TestMethod]
    public void DoorsMoveWithTheirWall()
    {
        var hall = Club();
        var moved = hall.With(h => h.SetOutline(PathDrawing.Rectangle(Width, Depth, new Transform(Angle.Zero, new(0, -500)))));
        Assert.IsTrue(moved.Doors[0].Position.Position.IsCloseTo(new(1500, -500), Precision));
        Assert.IsTrue(hall.Doors[0].Position.Position.IsCloseTo(new(1500, 0), Precision), "The hall copied from is unchanged.");
        Assert.AreEqual(hall.Doors[0].Position.Heading.Degrees, moved.Doors[0].Position.Heading.Degrees, Precision);
    }

    [TestMethod]
    public void OpenWallsAreRefused()
    {
        var hall = Hall.Rectangle("Hall", Width, Depth);
        Assert.ThrowsExactly<ArgumentException>(() => hall.SetOutline(PathDrawing.Polyline([new(0, 0), new(100, 0), new(100, 100)])));
    }

    [TestMethod]
    public void FloorAreaOfARectangle() =>
        Assert.AreEqual(Width * Depth, Hall.Rectangle("Hall", Width, Depth).FloorArea, Precision);

    [TestMethod]
    public void EffectiveFloorAreaLeavesOutObstaclesDoorsAndZones()
    {
        var hall = Hall.Rectangle("Hall", Width, Depth);
        hall.Add(Obstacle.RectangularPillar(new(4000, 4000), 400, 400));
        hall.Add(new Door { Position = new(new(2000, 0), new(90)), Width = 1000, KeepFreeDepth = 1000 });
        hall.Add(new NoGoZone { Area = PathDrawing.Rectangle(new(10000, 6000), 1000, 1000, Angle.Zero), Reason = "Fire hose" });
        Assert.AreEqual(Width * Depth - 400 * 400 - 1000 * 1000 - 1000 * 1000, hall.EffectiveFloorArea, hall.FloorArea * RelativeArea);
    }

    [TestMethod]
    public void EffectiveFloorAreaCountsOverlapsOnce()
    {
        var hall = Hall.Rectangle("Hall", Width, Depth);
        // The pillar stands inside the no-go zone, which reaches 500 mm outside the walls.
        hall.Add(Obstacle.RectangularPillar(new(1000, 4000), 400, 400));
        hall.Add(new NoGoZone { Area = PathDrawing.Rectangle(new(500, 4000), 2000, 1000, Angle.Zero), Reason = "Stage" });
        Assert.AreEqual(Width * Depth - 1500 * 1000, hall.EffectiveFloorArea, hall.FloorArea * RelativeArea);
    }

    [TestMethod]
    public void EffectiveFloorAreaWithoutRoundPillarIsCloseToTheCircle()
    {
        var hall = Hall.Rectangle("Hall", Width, Depth);
        hall.Add(Obstacle.RoundPillar(new(4000, 4000), 400));
        Assert.AreEqual(Width * Depth - Math.PI * 200 * 200, hall.EffectiveFloorArea, hall.FloorArea * RelativeArea);
    }
}
