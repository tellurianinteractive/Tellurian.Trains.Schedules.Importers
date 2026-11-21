namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

[TestClass]
public sealed class MeasureTests
{
    private static Polyline Square(double x, double y, double size) =>
        new([new(x, y), new(x + size, y), new(x + size, y + size), new(x, y + size)], true);

    [TestMethod]
    [DataRow(5, 3, 3)]
    [DataRow(-4, 3, 5)]
    [DataRow(13, -4, 5)]
    public void PointToSegment(double x, double y, double expected) =>
        Assert.AreEqual(expected, Measure.PointToSegment(new(x, y), new(0, 0), new(10, 0)), Precision);

    [TestMethod]
    public void PointToDegenerateSegment() =>
        Assert.AreEqual(5, Measure.PointToSegment(new(3, 4), new(0, 0), new(0, 0)), Precision);

    [TestMethod]
    public void CrossingSegmentsIntersect()
    {
        var point = Measure.SegmentIntersection(new(0, 0), new(10, 10), new(0, 10), new(10, 0));
        Assert.IsNotNull(point);
        Assert.IsTrue(point.Value.IsCloseTo(new(5, 5)));
    }

    [TestMethod]
    public void TouchingSegmentsIntersectAtEnd()
    {
        var point = Measure.SegmentIntersection(new(0, 0), new(10, 0), new(10, 0), new(10, 5));
        Assert.IsNotNull(point);
        Assert.IsTrue(point.Value.IsCloseTo(new(10, 0)));
    }

    [TestMethod]
    public void SeparateAndParallelSegmentsDoNotIntersect()
    {
        Assert.IsNull(Measure.SegmentIntersection(new(0, 0), new(10, 0), new(0, 1), new(10, 1)));
        Assert.IsNull(Measure.SegmentIntersection(new(0, 0), new(10, 0), new(11, -1), new(11, 1)));
    }

    [TestMethod]
    public void CollinearSegments()
    {
        Assert.IsNotNull(Measure.SegmentIntersection(new(0, 0), new(10, 0), new(5, 0), new(15, 0)));
        Assert.IsNull(Measure.SegmentIntersection(new(0, 0), new(10, 0), new(11, 0), new(15, 0)));
        Assert.AreEqual(1, Measure.SegmentToSegment(new(0, 0), new(10, 0), new(11, 0), new(15, 0)), Precision);
    }

    [TestMethod]
    public void SegmentToSegmentDistance()
    {
        Assert.AreEqual(2, Measure.SegmentToSegment(new(0, 0), new(10, 0), new(5, 2), new(5, 8)), Precision);
        Assert.AreEqual(0, Measure.SegmentToSegment(new(0, 0), new(10, 0), new(5, -2), new(5, 8)));
    }

    [TestMethod]
    public void PolylineDistances()
    {
        var a = Square(0, 0, 100);
        var b = Square(130, 40, 50);
        Assert.AreEqual(30, Measure.PolylineToPolyline(a, b), Precision);
        Assert.AreEqual(30, Measure.PolylineToPolyline(b, a), Precision);
        Assert.IsFalse(Measure.Intersects(a, b));
        Assert.AreEqual(20, Measure.PointToPolyline(new(50, 120), a), Precision);
    }

    [TestMethod]
    public void OverlappingPolylinesIntersect()
    {
        var a = Square(0, 0, 100);
        var b = Square(50, 50, 100);
        Assert.IsTrue(Measure.Intersects(a, b));
        Assert.AreEqual(0, Measure.PolylineToPolyline(a, b));
        var points = Measure.Intersections(a, b);
        Assert.HasCount(2, points);
        Assert.IsTrue(points.Any(p => p.IsCloseTo(new(100, 50))));
        Assert.IsTrue(points.Any(p => p.IsCloseTo(new(50, 100))));
    }

    [TestMethod]
    public void FarApartPolylinesHaveNoIntersections() =>
        Assert.IsEmpty(Measure.Intersections(Square(0, 0, 10), Square(100, 100, 10)));

    [TestMethod]
    public void BoxDistance()
    {
        var a = BoundingBox.FromPoints([new(0, 0), new(10, 10)]);
        Assert.AreEqual(5, Measure.BoxDistance(a, BoundingBox.FromPoints([new(13, 14), new(20, 20)])), Precision);
        Assert.AreEqual(0, Measure.BoxDistance(a, BoundingBox.FromPoints([new(5, 5), new(20, 20)])));
        Assert.AreEqual(double.PositiveInfinity, Measure.BoxDistance(a, BoundingBox.Empty));
    }

    [TestMethod]
    public void PolygonAreaAndContainment()
    {
        var square = Square(0, 0, 10);
        Assert.AreEqual(100, square.SignedArea, Precision);
        Assert.AreEqual(40, square.Length, Precision);
        Assert.IsTrue(square.Contains(new(5, 5)));
        Assert.IsTrue(square.Contains(new(10, 5)));
        Assert.IsFalse(square.Contains(new(11, 5)));
        Assert.IsFalse(square.Contains(new(5, -0.1)));

        var clockwise = new Polyline(square.Points.Reverse(), true);
        Assert.AreEqual(-100, clockwise.SignedArea, Precision);
        Assert.IsTrue(clockwise.Contains(new(5, 5)));
    }

    [TestMethod]
    public void ConcavePolygonContainment()
    {
        // An L shape.
        var l = new Polyline([new(0, 0), new(20, 0), new(20, 10), new(10, 10), new(10, 20), new(0, 20)], true);
        Assert.IsTrue(l.Contains(new(5, 15)));
        Assert.IsFalse(l.Contains(new(15, 15)));
        Assert.AreEqual(300, l.SignedArea, Precision);
    }

    [TestMethod]
    public void OpenPolylineHasNoAreaAndContainsNothing()
    {
        var open = new Polyline([new(0, 0), new(10, 0), new(10, 10)], false);
        Assert.AreEqual(0, open.SignedArea);
        Assert.IsFalse(open.Contains(new(5, 1)));
        Assert.AreEqual(2, open.EdgeCount);
    }

    [TestMethod]
    public void PolylineNeedsTwoPoints() =>
        Assert.ThrowsExactly<ArgumentException>(() => new Polyline([new(0, 0)], false));
}
