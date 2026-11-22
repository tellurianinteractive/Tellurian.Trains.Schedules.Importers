namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

[TestClass]
public sealed class SegmentTests
{
    [TestMethod]
    public void LineHasLengthHeadingAndPoints()
    {
        var line = new LineSegment(new(0, 0), new(30, 40));
        Assert.AreEqual(50, line.Length, Precision);
        Assert.IsTrue(line.PointAt(25).IsCloseTo(new(15, 20)));
        Assert.IsTrue(line.PointAt(-5).IsCloseTo(line.Start));
        Assert.IsTrue(line.PointAt(500).IsCloseTo(line.End));
        Assert.AreEqual(line.StartHeading, line.HeadingAt(10));
        Assert.AreEqual(line.StartHeading, line.EndHeading);
    }

    [TestMethod]
    public void ZeroLengthLineIsRejected() =>
        Assert.ThrowsExactly<ArgumentException>(() => new LineSegment(new(1, 1), new(1, 1)));

    [TestMethod]
    public void LineReverseTransformMirrorOffset()
    {
        var line = LineSegment.FromStart(Pose.Origin, 100);
        Assert.IsTrue(line.Reversed().Start.IsCloseTo(new(100, 0)));
        Assert.IsTrue(line.Transformed(new(new Angle(90), new Vector(0, 0))).End.IsCloseTo(new(0, 100)));
        Assert.IsTrue(line.MirroredX().End.IsCloseTo(new(-100, 0)));
        var offset = line.Offset(10);
        Assert.IsTrue(offset.Start.IsCloseTo(new(0, 10)));
        Assert.IsTrue(offset.End.IsCloseTo(new(100, 10)));
        var points = new List<Point>();
        line.AppendTessellation(points, 0.1);
        Assert.HasCount(1, points);
    }

    [TestMethod]
    public void LeftArcFromStart()
    {
        var arc = ArcSegment.FromStart(Pose.Origin, 1000, 90);
        Assert.IsTrue(arc.IsCounterClockwise);
        Assert.IsTrue(arc.Center.IsCloseTo(new(0, 1000)));
        Assert.IsTrue(arc.End.IsCloseTo(new(1000, 1000)));
        Assert.AreEqual(90, arc.EndHeading.Normalized.Degrees, Precision);
        Assert.AreEqual(1000 * Math.PI / 2, arc.Length, Precision);
        Assert.IsTrue(arc.StartPose.IsCloseTo(Pose.Origin));
    }

    [TestMethod]
    public void RightArcFromStart()
    {
        var arc = ArcSegment.FromStart(Pose.Origin, 500, -90);
        Assert.IsFalse(arc.IsCounterClockwise);
        Assert.IsTrue(arc.Center.IsCloseTo(new(0, -500)));
        Assert.IsTrue(arc.End.IsCloseTo(new(500, -500)));
        Assert.AreEqual(270, arc.EndHeading.Normalized.Degrees, Precision);
        Assert.IsTrue(arc.PointAt(arc.Length / 2).IsCloseTo(new(500 * Math.Sin(Math.PI / 4), -500 + 500 * Math.Cos(Math.PI / 4))));
        Assert.AreEqual(315, arc.HeadingAt(arc.Length / 2).Normalized.Degrees, Precision);
    }

    [TestMethod]
    public void ArcBoundsIncludeCardinalExtremes()
    {
        var arc = ArcSegment.FromStart(Pose.Origin, 100, 180);
        var bounds = arc.Bounds;
        Assert.AreEqual(100, bounds.Max.X, Precision);
        Assert.AreEqual(200, bounds.Max.Y, Precision);
        Assert.AreEqual(0, bounds.Min.X, Precision);
    }

    [TestMethod]
    public void FullCircleArc()
    {
        var arc = new ArcSegment(Point.Origin, 100, Angle.Zero, 360);
        Assert.IsTrue(arc.Start.IsCloseTo(arc.End));
        Assert.AreEqual(2 * Math.PI * 100, arc.Length, Precision);
        Assert.AreEqual(200, arc.Bounds.Width, Precision);
        Assert.IsTrue(arc.Covers(new Angle(123)));
    }

    [TestMethod]
    public void InvalidArcsAreRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ArcSegment(Point.Origin, 0, Angle.Zero, 10));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ArcSegment(Point.Origin, 10, Angle.Zero, 0));
    }

    [TestMethod]
    public void ArcReverseRunsBackwards()
    {
        var arc = ArcSegment.FromStart(Pose.Origin, 1000, 30);
        var reversed = arc.Reversed();
        Assert.IsTrue(reversed.Start.IsCloseTo(arc.End));
        Assert.IsTrue(reversed.End.IsCloseTo(arc.Start));
        Assert.IsTrue(reversed.StartHeading.IsCloseTo(arc.EndHeading + Angle.Half, Precision));
    }

    [TestMethod]
    public void ArcTransformAndMirror()
    {
        var arc = ArcSegment.FromStart(Pose.Origin, 1000, 30);
        var transform = new Transform(new Angle(60), new Vector(50, 70));
        var moved = arc.Transformed(transform);
        Assert.IsTrue(moved.End.IsCloseTo(transform.Apply(arc.End)));
        Assert.IsTrue(moved.EndHeading.IsCloseTo(transform.Apply(arc.EndHeading), Precision));

        var mirrored = arc.MirroredX();
        Assert.IsTrue(mirrored.Start.IsCloseTo(arc.Start.MirroredX()));
        Assert.IsTrue(mirrored.End.IsCloseTo(arc.End.MirroredX()));
    }

    [TestMethod]
    public void ArcOffsetChangesRadius()
    {
        var left = (ArcSegment)ArcSegment.FromStart(Pose.Origin, 1000, 30).Offset(100);
        Assert.AreEqual(900, left.Radius, Precision);
        var right = (ArcSegment)ArcSegment.FromStart(Pose.Origin, 1000, -30).Offset(100);
        Assert.AreEqual(1100, right.Radius, Precision);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ArcSegment.FromStart(Pose.Origin, 50, 30).Offset(60));
    }

    [TestMethod]
    public void ArcTessellationRespectsChordTolerance()
    {
        var arc = ArcSegment.FromStart(Pose.Origin, 2000, 45);
        var points = new List<Point> { arc.Start };
        arc.AppendTessellation(points, 0.05);
        Assert.IsTrue(points[^1].IsCloseTo(arc.End));
        for (var i = 1; i < points.Count; i++)
        {
            var middle = Point.Lerp(points[i - 1], points[i], 0.5);
            var sagitta = 2000 - middle.DistanceTo(arc.Center);
            Assert.IsLessThanOrEqualTo(0.05 + Precision, sagitta);
        }
    }
}
