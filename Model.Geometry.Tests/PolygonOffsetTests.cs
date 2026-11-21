namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

[TestClass]
public sealed class PolygonOffsetTests
{
    private static readonly Polyline Square = new([new(0, 0), new(100, 0), new(100, 100), new(0, 100)], true);

    [TestMethod]
    public void GrowingRoundsTheCorners()
    {
        var grown = PolygonOffset.Offset(Square, 10, 0.01);
        Assert.IsTrue(grown.IsCounterClockwise);
        Assert.AreEqual(-10, grown.Bounds.Min.X, Precision);
        Assert.AreEqual(110, grown.Bounds.Max.Y, Precision);
        foreach (var point in grown.Points)
        {
            Assert.AreEqual(10, Measure.PointToPolyline(point, Square), Tolerance.Length);
        }
        // Rounded corners: the area is the square, four side strips and a full circle.
        Assert.AreEqual(100 * 100 + 4 * 100 * 10 + Math.PI * 100, grown.SignedArea, 1);
    }

    [TestMethod]
    public void ShrinkingMitresTheCorners()
    {
        var shrunk = PolygonOffset.Offset(Square, -10);
        Assert.HasCount(4, shrunk.Points);
        Assert.AreEqual(80 * 80, shrunk.SignedArea, Precision);
        Assert.IsTrue(shrunk.Points.Any(p => p.IsCloseTo(new(10, 10))));
    }

    [TestMethod]
    public void ClockwiseInputGivesTheSameResult()
    {
        var clockwise = new Polyline(Square.Points.Reverse(), true);
        Assert.AreEqual(PolygonOffset.Offset(Square, 5).SignedArea, PolygonOffset.Offset(clockwise, 5).SignedArea, Precision);
    }

    [TestMethod]
    public void ConcaveCornerIsMitredWhenGrowing()
    {
        var l = new Polyline([new(0, 0), new(20, 0), new(20, 10), new(10, 10), new(10, 20), new(0, 20)], true);
        var grown = PolygonOffset.Offset(l, 1);
        Assert.IsTrue(grown.Points.Any(p => p.IsCloseTo(new(11, 11))));
    }

    [TestMethod]
    public void SharpConcaveCornerFallsBackToTwoPoints()
    {
        // A narrow notch: the concave corner is so sharp that a mitre would be very long.
        var notch = new Polyline([new(0, 0), new(100, 0), new(100, 100), new(51, 100), new(50, 1), new(49, 100), new(0, 100)], true);
        var grown = PolygonOffset.Offset(notch, 5);
        Assert.IsTrue(grown.IsCounterClockwise);
        Assert.IsGreaterThan(notch.Points.Count, grown.Points.Count);
    }

    [TestMethod]
    public void CollinearAndDuplicatePointsAreHandled()
    {
        var withExtras = new Polyline([new(0, 0), new(50, 0), new(50, 0), new(100, 0), new(100, 100), new(0, 100), new(0, 0)], true);
        var shrunk = PolygonOffset.Offset(withExtras, -10);
        Assert.AreEqual(80 * 80, shrunk.SignedArea, Precision);
    }

    [TestMethod]
    public void ZeroDistanceReturnsTheCleanedPolygon() =>
        Assert.AreEqual(Square.SignedArea, PolygonOffset.Offset(Square, 0).SignedArea, Precision);

    [TestMethod]
    public void InvalidInputIsRejected()
    {
        Assert.ThrowsExactly<ArgumentException>(() => PolygonOffset.Offset(new([new(0, 0), new(1, 0)], false), 1));
        Assert.ThrowsExactly<ArgumentException>(() => PolygonOffset.Offset(new([new(0, 0), new(1, 0), new(1, 0)], true), 1));
    }
}
