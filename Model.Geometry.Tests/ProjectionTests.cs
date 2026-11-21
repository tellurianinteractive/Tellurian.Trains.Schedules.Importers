namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

[TestClass]
public sealed class ProjectionTests
{
    private static SegmentPath StraightThenLeftCurve() =>
        new PathBuilder(Pose.Origin).Straight(500).Curve(1000, 90).Build();

    [TestMethod]
    public void PointBesideLineIsOffsetToTheLeft()
    {
        var projection = StraightThenLeftCurve().Project(new(200, 30));
        Assert.AreEqual(200, projection.Distance, Precision);
        Assert.AreEqual(30, projection.Offset, Precision);
        Assert.IsTrue(projection.Pose.IsCloseTo(new(new(200, 0), Angle.Zero)));
    }

    [TestMethod]
    public void PointToTheRightHasNegativeOffset() =>
        Assert.AreEqual(-40, StraightThenLeftCurve().Project(new(100, -40)).Offset, Precision);

    [TestMethod]
    public void PointInsideLeftCurveIsToTheLeft()
    {
        // Centre of the curve is at (500, 1000); a point at radius 900, 45° into the curve.
        var center = new Point(500, 1000);
        var point = center + Vector.FromAngle(new Angle(-45), 900);
        var projection = StraightThenLeftCurve().Project(point);
        Assert.AreEqual(500 + 1000 * Math.PI / 4, projection.Distance, Precision);
        Assert.AreEqual(100, projection.Offset, Precision);
    }

    [TestMethod]
    public void PointOutsideRightCurveIsToTheLeft()
    {
        var path = new PathBuilder(Pose.Origin).Curve(1000, -30).Build();
        var center = new Point(0, -1000);
        var point = center + Vector.FromAngle(new Angle(80), 1250);
        var projection = path.Project(point);
        Assert.AreEqual(1000 * 10 * Math.PI / 180, projection.Distance, Precision);
        Assert.AreEqual(250, projection.Offset, Precision);
    }

    [TestMethod]
    public void PointBeyondEndIsMeasuredAcrossTheEndPose()
    {
        var path = new PathBuilder(Pose.Origin).Straight(1000).Build();
        var projection = path.Project(new(1100, -60));
        Assert.AreEqual(1000, projection.Distance, Precision);
        Assert.AreEqual(-60, projection.Offset, Precision);
    }

    [TestMethod]
    public void PointBeforeArcStartSnapsToStart()
    {
        var path = new PathBuilder(Pose.Origin).Curve(1000, 30).Build();
        var projection = path.Project(new(-100, 20));
        Assert.AreEqual(0, projection.Distance, Precision);
        Assert.AreEqual(20, projection.Offset, Precision);
    }

    [TestMethod]
    public void ArcCentreProjectsToAnEnd()
    {
        var path = new PathBuilder(Pose.Origin).Curve(1000, 30).Build();
        var projection = path.Project(new(0, 1000));
        Assert.AreEqual(0, projection.Distance, Precision);
        Assert.AreEqual(1000, projection.Offset, Precision);
    }
}
