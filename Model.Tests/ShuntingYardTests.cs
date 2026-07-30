namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the derivation of shunting yards from the <c>CargoServedFrom</c> relation and station regions:
/// a shunting yard is what a station covers, never a flag that could fall out of step with reality.
/// </summary>
[TestClass]
public class ShuntingYardTests
{
    private static Layout CreateLayout()
    {
        TestDataFactory.Init();
        return TestDataFactory.Layout();
    }

    private static Station AddStation(Layout layout, int id, string name, string signature, bool isShadow = false)
    {
        var station = new Station(id, name, signature) { IsShadow = isShadow };
        layout.OperationLocations.Add(station);
        return station;
    }

    [TestMethod]
    public void AStationServingAnotherLocationIsAShuntingYard()
    {
        var layout = CreateLayout();
        var shuntingYard = AddStation(layout, 101, "Munkeröd", "Mkd");
        var served = AddStation(layout, 102, "Rubjerg", "Rbj");
        served.CargoServedFrom = shuntingYard;

        var shuntingYards = layout.ShuntingYards;

        var found = shuntingYards.Single(y => y.Station.Equals(shuntingYard));
        Assert.AreEqual(served, found.ServedLocations.Single());
    }

    [TestMethod]
    public void AShadowStationCarryingRegionsIsAShuntingYardEvenWithNoServedLocation()
    {
        var layout = CreateLayout();
        var shadow = AddStation(layout, 103, "Mohult", "Moh", isShadow: true);
        shadow.Regions.Add(new Region { Id = 1, Name = "ØST" });
        shadow.Regions.Add(new Region { Id = 2, Name = "FYN" });

        var found = layout.ShuntingYards.Single(y => y.Station.Equals(shadow));

        // Requiring a served location would omit exactly the station a driver most needs listed.
        Assert.IsEmpty(found.ServedLocations);
        Assert.HasCount(2, found.ServedRegions);
    }

    [TestMethod]
    public void AStationCoveringNothingIsNotAShuntingYard()
    {
        var layout = CreateLayout();
        var plain = AddStation(layout, 104, "Delsbo", "Dls");

        // An entry with nothing under it says nothing.
        Assert.IsFalse(layout.ShuntingYards.Any(y => y.Station.Equals(plain)));
    }

    [TestMethod]
    public void AShuntingYardListsEveryLocationServedFromIt()
    {
        var layout = CreateLayout();
        var shuntingYard = AddStation(layout, 105, "Växjö", "Vxo");
        var first = AddStation(layout, 106, "Ålsheda", "Als");
        var second = AddStation(layout, 107, "Lenhovda", "Len");
        first.CargoServedFrom = shuntingYard;
        second.CargoServedFrom = shuntingYard;

        var found = layout.ShuntingYards.Single(y => y.Station.Equals(shuntingYard));

        Assert.HasCount(2, found.ServedLocations);
    }

    [TestMethod]
    public void AShadowStationCannotBeServedButMayServe()
    {
        var layout = CreateLayout();
        var shadow = AddStation(layout, 108, "Mohult", "Moh", isShadow: true);
        var ordinary = AddStation(layout, 109, "Bruket", "Bru");

        // Off-layout staging is where traffic originates, never a delivery destination.
        Assert.IsFalse(shadow.CanBeCargoServed, "A shadow station is not offered a serving shunting yard.");
        Assert.IsTrue(ordinary.CanBeCargoServed);
        Assert.Contains(shadow, layout.CargoServingStationsFor(ordinary),
            "A shadow shunting yard is a perfectly ordinary origin for local freight.");
    }

    [TestMethod]
    public void ALocationWithoutCargoExchangeCannotBeServed()
    {
        var layout = CreateLayout();
        var noCargo = AddStation(layout, 110, "Slokärr", "Slk");
        noCargo.HasCargoExchange = false;

        Assert.IsFalse(noCargo.CanBeCargoServed, "A location exchanging no cargo has nothing to be delivered.");
    }

    [TestMethod]
    public void AStationIsNotOfferedAsItsOwnServingShuntingYard()
    {
        var layout = CreateLayout();
        var station = AddStation(layout, 111, "Stilkøbing", "Stk");

        Assert.DoesNotContain(station, layout.CargoServingStationsFor(station));
    }
}
