using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies the reusable cargo flow description catalogue on <see cref="Timetable.CargoFlowOptions"/>:
/// ids are assigned on add, and the catalogue (with its origins and destinations referencing layout
/// stations) survives the browser-storage JSON round-trip used by Planning.App.
/// </summary>
[TestClass]
public class CargoFlowOptionsCatalogueTests
{
    private static readonly JsonSerializerOptions Options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256
    };

    [TestMethod]
    public void AddAssignsSequentialIds()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();

        var a = timetable.Add(new CargoFlowOptions { Name = "A" });
        var b = timetable.Add(new CargoFlowOptions { Name = "B" });

        Assert.AreEqual(1, a.Id);
        Assert.AreEqual(2, b.Id);
        Assert.HasCount(2, timetable.CargoFlowOptions);
    }

    [TestMethod]
    public void CargoFlowOptionsCatalogueSurvivesPlanRoundTrip()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var station = timetable.Layout.OperationLocations.OfType<Station>().First();

        var description = timetable.Add(new CargoFlowOptions
        {
            Name = "Coal to the harbour",
            Origins = { new Origin { Station = station } },
            Destinations = { new Destination { Station = station, MaxNumberOfWagons = 5, AndRegions = true } },
        });
        Assert.AreNotEqual(0, description.Id, "Adding a description assigns a catalogue id.");

        var plan = Plan.Create("Plan", timetable);
        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        var restoredCatalogue = restored.Timetable.CargoFlowOptions;
        Assert.HasCount(1, restoredCatalogue);

        var restoredDescription = restoredCatalogue.First();
        Assert.AreEqual(description.Id, restoredDescription.Id);
        Assert.AreEqual("Coal to the harbour", restoredDescription.Name);
        Assert.HasCount(1, restoredDescription.Origins);
        Assert.HasCount(1, restoredDescription.Destinations);

        var restoredDestination = restoredDescription.Destinations.First();
        Assert.AreEqual(5, restoredDestination.MaxNumberOfWagons);
        Assert.IsTrue(restoredDestination.AndRegions);
        Assert.AreEqual(station.Name, restoredDestination.Station.Name);
    }
}
