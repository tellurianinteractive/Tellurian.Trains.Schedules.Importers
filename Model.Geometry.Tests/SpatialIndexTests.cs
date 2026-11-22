using System.Diagnostics;

namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

[TestClass]
public sealed class SpatialIndexTests
{
    private static BoundingBox Box(double x, double y, double width, double height) =>
        BoundingBox.FromPoints([new(x, y), new(x + width, y + height)]);

    [TestMethod]
    public void QueryFindsIntersectingItems()
    {
        var index = new SpatialIndex<string>(500);
        index.Set("a", Box(0, 0, 1000, 500));
        index.Set("b", Box(1200, 0, 1000, 500));
        index.Set("c", Box(-3000, -3000, 100, 100));

        CollectionAssert.AreEquivalent(new[] { "a", "b" }, index.Query(Box(900, 100, 400, 10)).ToArray());
        CollectionAssert.AreEquivalent(new[] { "c" }, index.Query(Box(-2950, -2950, 1, 1)).ToArray());
        Assert.IsEmpty(index.Query(BoundingBox.Empty));
        Assert.AreEqual(3, index.Count);
    }

    [TestMethod]
    public void QueryNearUsesTrueBoxDistance()
    {
        var index = new SpatialIndex<int>(100);
        index.Set(1, Box(0, 0, 10, 10));
        // Diagonally 30/40 away: inflated boxes overlap, but the true distance is 50.
        var probe = Box(40, 50, 10, 10);
        Assert.IsEmpty(index.QueryNear(probe, 49));
        CollectionAssert.AreEqual(new[] { 1 }, index.QueryNear(probe, 50).ToArray());
    }

    [TestMethod]
    public void SetMovesAndRemoveForgets()
    {
        var index = new SpatialIndex<string>(100);
        index.Set("a", Box(0, 0, 10, 10));
        index.Set("a", Box(1000, 1000, 10, 10));
        Assert.IsEmpty(index.Query(Box(0, 0, 10, 10)));
        Assert.HasCount(1, index.Query(Box(1000, 1000, 1, 1)));
        Assert.AreEqual(1, index.Count);

        Assert.IsTrue(index.Remove("a"));
        Assert.IsFalse(index.Remove("a"));
        Assert.IsFalse(index.Contains("a"));
        Assert.IsEmpty(index.Query(Box(1000, 1000, 1, 1)));

        index.Set("b", Box(0, 0, 1, 1));
        index.Clear();
        Assert.AreEqual(0, index.Count);
    }

    [TestMethod]
    public void NegativeCoordinatesUseTheirOwnCells()
    {
        var index = new SpatialIndex<string>(100);
        index.Set("neg", Box(-50, -50, 10, 10));
        Assert.IsEmpty(index.Query(Box(10, 10, 10, 10)));
        Assert.HasCount(1, index.Query(Box(-45, -45, 1, 1)));
    }

    [TestMethod]
    public void InvalidArgumentsAreRejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SpatialIndex<int>(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new SpatialIndex<int>(double.PositiveInfinity));
        Assert.ThrowsExactly<ArgumentException>(() => new SpatialIndex<int>(10).Set(1, BoundingBox.Empty));
        Assert.ThrowsExactly<ArgumentException>(() => new SpatialIndex<int>(10).Set(1, new BoundingBox(new(0, 0), new(double.PositiveInfinity, 1))));
    }

    [TestMethod]
    public void HugeAndFarAwayBoxesAreFoundWithoutTheGrid()
    {
        var index = new SpatialIndex<string>(10);
        index.Set("small", Box(0, 0, 5, 5));
        index.Set("huge", Box(-1e9, -1e9, 2e9, 2e9));
        index.Set("far", Box(1e12, 1e12, 5, 5));

        var stopwatch = Stopwatch.StartNew();
        CollectionAssert.AreEquivalent(new[] { "small", "huge" }, index.Query(Box(1, 1, 1, 1)).ToArray());
        CollectionAssert.AreEquivalent(new[] { "far" }, index.Query(Box(1e12, 1e12, 1, 1)).ToArray());
        CollectionAssert.AreEquivalent(new[] { "small", "huge", "far" }, index.Query(Box(-1e13, -1e13, 2e13, 2e13)).ToArray());
        CollectionAssert.AreEquivalent(new[] { "small", "huge" }, index.QueryNear(Box(1, 1, 1, 1), double.PositiveInfinity).Where(i => i != "far").ToArray());
        stopwatch.Stop();
        Assert.IsLessThan(200, stopwatch.ElapsedMilliseconds, $"Took {stopwatch.ElapsedMilliseconds} ms.");

        Assert.IsTrue(index.Remove("huge"));
        CollectionAssert.AreEquivalent(new[] { "small" }, index.Query(Box(1, 1, 1, 1)).ToArray());
        index.Clear();
        Assert.IsEmpty(index.Query(Box(1e12, 1e12, 1, 1)));
    }

    [TestMethod]
    public void ClearanceCheckOn300ModulesIsFast()
    {
        // NF-2 budget check: 300 curved module outlines, tessellated, all-pairs nearby distance check.
        var outlines = new List<Polyline>();
        var pose = Pose.Origin;
        for (var i = 0; i < 300; i++)
        {
            var main = new PathBuilder(pose).Straight(300).Curve(2000, i % 2 == 0 ? 4 : -4).Straight(300).Build();
            var left = main.Offset(250);
            var right = main.Offset(-250).Reversed();
            var outline = new SegmentPath([.. left.Segments, new LineSegment(left.End, right.Start), .. right.Segments, new LineSegment(right.End, left.Start)], isClosed: true);
            outlines.Add(outline.Tessellate());
            pose = main.EndPose;
        }

        var stopwatch = Stopwatch.StartNew();
        var index = new SpatialIndex<int>(1000);
        for (var i = 0; i < outlines.Count; i++) index.Set(i, outlines[i].Bounds);
        var tooClose = 0;
        for (var i = 0; i < outlines.Count; i++)
        {
            foreach (var j in index.QueryNear(outlines[i].Bounds, 50))
            {
                if (j <= i) continue;
                if (Math.Abs(i - j) == 1) continue; // Joined neighbours touch by design.
                if (Measure.PolylineToPolyline(outlines[i], outlines[j], 50) < 50) tooClose++;
            }
        }
        stopwatch.Stop();

        Assert.AreEqual(0, tooClose);
        Assert.IsLessThan(200, stopwatch.ElapsedMilliseconds, $"Took {stopwatch.ElapsedMilliseconds} ms.");
    }
}
