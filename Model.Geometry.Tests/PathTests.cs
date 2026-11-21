namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

[TestClass]
public sealed class PathTests
{
    private static SegmentPath StraightThenCurve() =>
        new PathBuilder(Pose.Origin).Straight(500).Curve(1000, 90).Build();

    [TestMethod]
    public void BuilderChainsTangentSegments()
    {
        var path = StraightThenCurve();
        Assert.HasCount(2, path.Segments);
        Assert.IsTrue(path.IsTangentContinuous());
        Assert.IsTrue(path.End.IsCloseTo(new(1500, 1000)));
        Assert.AreEqual(500 + 1000 * Math.PI / 2, path.Length, Precision);
    }

    [TestMethod]
    public void BuilderIgnoresZeroSteps()
    {
        var path = new PathBuilder(Pose.Origin).Straight(0).Curve(1000, 0).Straight(10).Build();
        Assert.HasCount(1, path.Segments);
    }

    [TestMethod]
    public void EmptyPathIsRejected() =>
        Assert.ThrowsExactly<ArgumentException>(() => new PathBuilder(Pose.Origin).Build());

    [TestMethod]
    public void DisconnectedSegmentsAreRejected() =>
        Assert.ThrowsExactly<ArgumentException>(() => new SegmentPath([new LineSegment(new(0, 0), new(1, 0)), new LineSegment(new(2, 0), new(3, 0))]));

    [TestMethod]
    public void OpenPathCanNotBeClosed() =>
        Assert.ThrowsExactly<ArgumentException>(() => new SegmentPath(StraightThenCurve().Segments, isClosed: true));

    [TestMethod]
    public void PoseAtWalksAlongSegments()
    {
        var path = StraightThenCurve();
        Assert.IsTrue(path.PoseAt(250).IsCloseTo(new Pose(new(250, 0), Angle.Zero)));
        Assert.IsTrue(path.PoseAt(-1).IsCloseTo(path.StartPose));
        Assert.IsTrue(path.PoseAt(1e6).IsCloseTo(path.EndPose));
        Assert.AreEqual(45, path.PoseAt(500 + 1000 * Math.PI / 4).Heading.Normalized.Degrees, Precision);
    }

    [TestMethod]
    public void ReversedPathRunsBackwards()
    {
        var path = StraightThenCurve();
        var reversed = path.Reversed();
        Assert.IsTrue(reversed.Start.IsCloseTo(path.End));
        Assert.IsTrue(reversed.End.IsCloseTo(path.Start));
        Assert.AreEqual(path.Length, reversed.Length, Precision);
    }

    [TestMethod]
    public void TransformedAndMirroredPaths()
    {
        var path = StraightThenCurve();
        var transform = new Transform(new Angle(90), new Vector(10, 0));
        Assert.IsTrue(path.Transformed(transform).End.IsCloseTo(transform.Apply(path.End)));
        Assert.IsTrue(Mirror.AboutCentreLine(path).End.IsCloseTo(new(-1500, 1000)));
    }

    [TestMethod]
    public void OffsetPathIsParallel()
    {
        var path = StraightThenCurve();
        var offset = path.Offset(-50);
        Assert.IsTrue(offset.Start.IsCloseTo(new(0, 50 * -1)));
        Assert.IsTrue(offset.End.IsCloseTo(new(1550, 1000)));
        Assert.IsTrue(offset.IsTangentContinuous());
    }

    [TestMethod]
    public void KinkedPathCanNotBeOffset()
    {
        var path = new PathBuilder(Pose.Origin).Straight(10).LineTo(new(10, 10)).Build();
        Assert.IsFalse(path.IsTangentContinuous());
        Assert.ThrowsExactly<InvalidOperationException>(() => path.Offset(1));
    }

    [TestMethod]
    public void ClosedPathTessellatesWithoutDuplicateEnd()
    {
        var square = new PathBuilder(Pose.Origin).Straight(100).Curve(1, 90).Straight(100).Curve(1, 90)
            .Straight(100).Curve(1, 90).Straight(100).Curve(1, 90).Build(isClosed: true);
        var polygon = square.Tessellate();
        Assert.IsTrue(polygon.IsClosed);
        Assert.IsFalse(polygon.Points[^1].IsCloseTo(polygon.Points[0]));
        Assert.IsTrue(polygon.IsCounterClockwise);
        Assert.AreEqual(square.Bounds.Width, polygon.Bounds.Width, 0.05);
    }

    [TestMethod]
    public void BoundsCoverCurve()
    {
        var bounds = StraightThenCurve().Bounds;
        Assert.AreEqual(1500, bounds.Max.X, Precision);
        Assert.AreEqual(1000, bounds.Max.Y, Precision);
    }

    [TestMethod]
    public void BetweenCutsAcrossSegments()
    {
        var path = StraightThenCurve();
        var quarter = 1000 * Math.PI / 4;
        var part = path.Between(250, 500 + quarter);
        Assert.HasCount(2, part.Segments);
        Assert.AreEqual(250 + quarter, part.Length, Precision);
        Assert.IsTrue(part.StartPose.IsCloseTo(path.PoseAt(250)));
        Assert.IsTrue(part.EndPose.IsCloseTo(path.PoseAt(500 + quarter)));
        Assert.IsTrue(part.IsTangentContinuous());
    }

    [TestMethod]
    public void BetweenWithinOneArcKeepsTheArc()
    {
        var arc = ArcSegment.FromStart(Pose.Origin, 1000, -60);
        var part = (ArcSegment)arc.Between(arc.Length / 3, arc.Length * 2 / 3);
        Assert.AreEqual(-20, part.SweepDegrees, Precision);
        Assert.AreEqual(1000, part.Radius, Precision);
        Assert.IsTrue(part.Start.IsCloseTo(arc.PointAt(arc.Length / 3)));
    }

    [TestMethod]
    public void BetweenIsClampedAndSkipsTinyParts()
    {
        var path = StraightThenCurve();
        Assert.HasCount(1, path.Between(-100, 500).Segments);
        Assert.AreEqual(path.Length, path.Between(0, 1e6).Length, Precision);
        Assert.ThrowsExactly<ArgumentException>(() => path.Between(300, 300));
    }

    [TestMethod]
    public void BetweenBeyondOneEndHasNoLength()
    {
        var path = StraightThenCurve();
        Assert.ThrowsExactly<ArgumentException>(() => path.Between(path.Length + 10, path.Length + 20));
        Assert.ThrowsExactly<ArgumentException>(() => path.Between(-20, -10));
        Assert.AreEqual(10, path.Between(path.Length - 10, path.Length + 20).Length, Precision);
    }

    [TestMethod]
    public void StraightOfNegativeLengthIsRejectedButRoundingIsIgnored()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new PathBuilder(Pose.Origin).Straight(-1));
        var path = new PathBuilder(Pose.Origin).Straight(-Tolerance.Length / 2).Straight(100).Build();
        Assert.HasCount(1, path.Segments);
    }
}
