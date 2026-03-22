using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Planning.Layouts;

namespace Tellurian.Trains.Schedules.Planning.Tests;

[TestClass]
public class PathFinderTests
{
    // Simple layout tests

    [TestMethod]
    public void FindsDirectPathInSimpleLayout()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        var malmö = layout.OperationLocations.First(l => l.Signature == "M2");
        var eslöv = layout.OperationLocations.First(l => l.Signature == "E");

        var path = PathFinder.FindShortestPath(layout, malmö, eslöv);

        Assert.IsNotNull(path);
        Assert.AreEqual(2, path.Segments.Count);
        Assert.AreEqual(18, path.TotalDistance); // 10 + 8
        Assert.AreEqual("M2", path.From.Signature);
        Assert.AreEqual("E", path.To.Signature);
    }

    [TestMethod]
    public void FindsPathToBranchInSimpleLayout()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        var malmö = layout.OperationLocations.First(l => l.Signature == "M2");
        var kävlinge = layout.OperationLocations.First(l => l.Signature == "Kä");

        var path = PathFinder.FindShortestPath(layout, malmö, kävlinge);

        Assert.IsNotNull(path);
        Assert.AreEqual(2, path.Segments.Count);
        Assert.AreEqual(16, path.TotalDistance); // 10 + 6
    }

    [TestMethod]
    public void ReturnsNullForSameLocation()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        var malmö = layout.OperationLocations.First(l => l.Signature == "M2");

        var path = PathFinder.FindShortestPath(layout, malmö, malmö);

        Assert.IsNull(path);
    }

    // Full layout tests

    [TestMethod]
    public void FindsPathAlongMainLine()
    {
        var layout = TestLayoutFactory.CreateLayout();
        var lysekil = Location(layout, "Lys");
        var malmö = Location(layout, "M");

        var path = PathFinder.FindShortestPath(layout, lysekil, malmö);

        Assert.IsNotNull(path);
        // Lysekil→Göransberg(4)→Furudalen(9)→Kristineberg(8)→Munkeröd(9)→Kkö(7)→Devsjö(12)→Lekby(7)→Rotebro(14)→Malmö(8)
        Assert.AreEqual(78.0, path.TotalDistance);
        Assert.IsTrue(path.Segments.All(s => s.Direction == TrainPathDirection.Forward));
    }

    [TestMethod]
    public void FindsPathFromMalmöToVäxjöViaMunkeröd()
    {
        var layout = TestLayoutFactory.CreateLayout();
        var malmö = Location(layout, "M");
        var växjö = Location(layout, "Vö");

        var path = PathFinder.FindShortestPath(layout, malmö, växjö);

        Assert.IsNotNull(path);
        Assert.AreEqual("M", path.From.Signature);
        Assert.AreEqual("Vö", path.To.Signature);
        // Direction change must happen at Munkeröd (the only permitted station on this route)
        var changes = path.DirectionChanges.ToList();
        Assert.AreEqual(1, changes.Count);
        Assert.AreEqual("Mkd", changes[0].Signature);
    }

    [TestMethod]
    public void FindsPathBetweenBranchesViaRise()
    {
        var layout = TestLayoutFactory.CreateLayout();
        var steinsnes = Location(layout, "Sns");
        var växjö = Location(layout, "Vö");

        var path = PathFinder.FindShortestPath(layout, steinsnes, växjö);

        Assert.IsNotNull(path);
        // Direction change at Rise (backward from Steinsnes, then forward to Froland)
        var changes = path.DirectionChanges.ToList();
        Assert.IsTrue(changes.Any(c => c.Signature == "Ris"));
    }

    [TestMethod]
    public void FindsForwardOnlyPathFromBjørkåsenToVäxjö()
    {
        var layout = TestLayoutFactory.CreateLayout();
        var bjørkåsen = Location(layout, "Bjå");
        var växjö = Location(layout, "Vö");

        var path = PathFinder.FindShortestPath(layout, bjørkåsen, växjö);

        Assert.IsNotNull(path);
        Assert.IsTrue(path.Segments.All(s => s.Direction == TrainPathDirection.Forward));
        Assert.IsFalse(path.DirectionChanges.Any());
        // Bjørkåsen→Rise→Froland→Gården→Mohult→Loudden→Växjö
        Assert.AreEqual(8 + 7 + 4 + 11 + 8 + 6, path.TotalDistance); // 44
    }

    [TestMethod]
    public void AllForwardPathHasNoDirectionChanges()
    {
        var layout = TestLayoutFactory.CreateLayout();
        var lysekil = Location(layout, "Lys");
        var malmö = Location(layout, "M");

        var path = PathFinder.FindShortestPath(layout, lysekil, malmö);

        Assert.IsNotNull(path);
        Assert.IsFalse(path.DirectionChanges.Any());
    }

    [TestMethod]
    public void PathListsAllLocationsInOrder()
    {
        var layout = TestLayoutFactory.CreateSimpleLayout();
        var malmö = layout.OperationLocations.First(l => l.Signature == "M2");
        var hässleholm = layout.OperationLocations.First(l => l.Signature == "Hm");

        var path = PathFinder.FindShortestPath(layout, malmö, hässleholm);

        Assert.IsNotNull(path);
        var locations = path.Locations.ToList();
        Assert.AreEqual(4, locations.Count);
        Assert.AreEqual("M2", locations[0].Signature);
        Assert.AreEqual("Lu", locations[1].Signature);
        Assert.AreEqual("E", locations[2].Signature);
        Assert.AreEqual("Hm", locations[3].Signature);
    }

    [TestMethod]
    public void FindsPathWithTwoDirectionChanges()
    {
        // Devsjö → Bjørkåsen:
        // backward to Munkeröd (change), forward via Kkö→Kys→Gården→Mohult (change),
        // backward via Gården→Froland→Rise→Bjørkåsen
        var layout = TestLayoutFactory.CreateLayout();
        var devsjö = Location(layout, "Djö");
        var bjørkåsen = Location(layout, "Bjå");

        var path = PathFinder.FindShortestPath(layout, devsjö, bjørkåsen);

        Assert.IsNotNull(path);
        var changes = path.DirectionChanges.ToList();
        Assert.AreEqual(2, changes.Count);
        Assert.AreEqual("Mkd", changes[0].Signature); // Munkeröd
        Assert.AreEqual("Mot", changes[1].Signature); // Mohult
    }

    private static OperationLocation Location(Layout layout, string signature) =>
        layout.OperationLocations.First(l => l.Signature == signature);
}
