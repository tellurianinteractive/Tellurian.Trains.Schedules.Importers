using System.Globalization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies how a <see cref="Destination"/> with <see cref="Destination.AndLocalDestinations"/> names the
/// locations whose cargo is served from its station, instead of summing them up in a phrase.
/// </summary>
[TestClass]
public class DestinationLocalDestinationsTests
{
    private static (Layout Layout, Station Yard, Station Alpha, Station Beta) CreateLayout()
    {
        var layout = new Layout { Name = "Test" };
        var yard = (Station)layout.Add(new Station(1, "Yard", "Yd"));
        var alpha = (Station)layout.Add(new Station(2, "Alpha", "Al"));
        var beta = (Station)layout.Add(new Station(3, "Beta", "Be"));
        return (layout, yard, alpha, beta);
    }

    [TestMethod]
    public void TheServedLocationsFollowTheStationInLayoutOrder()
    {
        var (_, yard, alpha, beta) = CreateLayout();
        beta.CargoServedFrom = yard;
        alpha.CargoServedFrom = yard;

        var destination = new Destination { Location = yard, AndLocalDestinations = true };

        Assert.AreEqual("Yard, Alpha, Beta", destination.PlaceText);
    }

    [TestMethod]
    public void AStationServingNothingIsNamedAlone()
    {
        var (_, yard, _, _) = CreateLayout();

        var destination = new Destination { Location = yard, AndLocalDestinations = true };

        Assert.AreEqual("Yard", destination.PlaceText);
    }

    [TestMethod]
    public void TheServedLocationsAreLeftOutWhenNotAskedFor()
    {
        var (_, yard, alpha, _) = CreateLayout();
        alpha.CargoServedFrom = yard;

        var destination = new Destination { Location = yard };

        Assert.AreEqual("Yard", destination.PlaceText);
    }

    [TestMethod]
    public void AndBeyondClosesTheList()
    {
        CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-GB");
        var (_, yard, alpha, _) = CreateLayout();
        alpha.CargoServedFrom = yard;

        var destination = new Destination { Location = yard, AndLocalDestinations = true, AndBeyond = true };

        Assert.AreEqual("Yard, Alpha and beyond", destination.PlaceText);
    }

    [TestMethod]
    public void TheNamesAreEncodedInMarkup()
    {
        var (_, yard, alpha, _) = CreateLayout();
        alpha.Name = "A&B";
        alpha.CargoServedFrom = yard;

        var destination = new Destination { Location = yard, AndLocalDestinations = true };

        Assert.AreEqual("Yard, A&amp;B", destination.PlaceHtml.Value);
    }
}
