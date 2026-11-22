namespace Tellurian.Trains.Schedules.Model.Geometry.Tests;

[TestClass]
public sealed class AreaMeasureTests
{
    private static Polyline Rectangle(double x, double y, double width, double height) =>
        new([new(x, y), new(x + width, y), new(x + width, y + height), new(x, y + height)], isClosed: true);

    private static void AreClose(double expected, double actual) =>
        Assert.AreEqual(expected, actual, Math.Max(expected, 1) * RelativePrecision);

    [TestMethod]
    public void UnionCountsOverlapsOnce() =>
        AreClose(100 * 100 + 100 * 100 - 50 * 50, AreaMeasure.Union([Rectangle(0, 0, 100, 100), Rectangle(50, 50, 100, 100)]));

    [TestMethod]
    public void UnionOfSeparatePolygonsIsTheirSum() =>
        AreClose(2 * 100 * 100, AreaMeasure.Union([Rectangle(0, 0, 100, 100), Rectangle(500, 500, 100, 100)]));

    [TestMethod]
    public void UnionOfNothingIsZero() => Assert.AreEqual(0, AreaMeasure.Union([]));

    [TestMethod]
    public void RemainingWithoutHolesIsTheRegion() =>
        AreClose(12000.0 * 8000, AreaMeasure.Remaining(Rectangle(0, 0, 12000, 8000), []));

    [TestMethod]
    public void RemainingTakesAwayHolesInsideTheRegion() =>
        AreClose(12000.0 * 8000 - 400 * 400 - 2000 * 1000, AreaMeasure.Remaining(Rectangle(0, 0, 12000, 8000), [Rectangle(4000, 4000, 400, 400), Rectangle(10000, 7000, 2000, 1000)]));

    [TestMethod]
    public void RemainingCountsOverlappingHolesOnce() =>
        AreClose(1000.0 * 1000 - (200 * 200 + 200 * 200 - 100 * 100), AreaMeasure.Remaining(Rectangle(0, 0, 1000, 1000), [Rectangle(100, 100, 200, 200), Rectangle(200, 200, 200, 200)]));

    [TestMethod]
    public void RemainingIgnoresThePartsOfHolesOutsideTheRegion() =>
        AreClose(1000.0 * 1000 - 100 * 200, AreaMeasure.Remaining(Rectangle(0, 0, 1000, 1000), [Rectangle(-100, 400, 200, 200), Rectangle(2000, 2000, 100, 100)]));

    [TestMethod]
    public void RemainingOfAnLShapedRegionWithAHoleAcrossItsNotch()
    {
        var region = new Polyline([new(0, 0), new(1000, 0), new(1000, 500), new(500, 500), new(500, 1000), new(0, 1000)], isClosed: true);
        // The hole covers 400 × 400 of which the quarter in the notch (outside the region) doesn't count.
        AreClose(750_000 - 3 * 200 * 200, AreaMeasure.Remaining(region, [Rectangle(300, 300, 400, 400)]));
    }

    [TestMethod]
    public void RemainingWithATurnedHole()
    {
        var diamond = new Polyline([new(500, 300), new(700, 500), new(500, 700), new(300, 500)], isClosed: true);
        AreClose(1000.0 * 1000 - 2 * 200 * 200, AreaMeasure.Remaining(Rectangle(0, 0, 1000, 1000), [diamond]));
    }
}
