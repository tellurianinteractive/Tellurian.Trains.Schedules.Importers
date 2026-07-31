using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies the reusable cargo flow description catalogue on <see cref="Timetable.CargoFlowOptions"/>:
/// ids are assigned on add, and the catalogue (with its origins and destinations referencing layout
/// operation locations) survives the browser-storage JSON round-trip used by Planning.App.
/// </summary>
[TestClass]
public class CargoFlowOptionsCatalogueTests
{
    private static readonly JsonSerializerOptions Options = PlanJson.CreateOptions();

    [TestMethod]
    public void AddAssignsSequentialIds()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();

        var a = timetable.Add(new CargoFlowOptions { OnlyWagonClasses = "A" });
        var b = timetable.Add(new CargoFlowOptions { OnlyWagonClasses = "B" });

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
            OnlyWagonClasses = "U,Z",
            Origins = { new Origin { Location = station } },
            Destinations = { new Destination { Location = station, MaxNumberOfWagons = 5, AndRegions = true } },
        });
        Assert.AreNotEqual(0, description.Id, "Adding a description assigns a catalogue id.");

        var plan = Plan.Create("Plan", timetable);
        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        var restoredCatalogue = restored.Timetable.CargoFlowOptions;
        Assert.HasCount(1, restoredCatalogue);

        var restoredDescription = restoredCatalogue.First();
        Assert.AreEqual(description.Id, restoredDescription.Id);
        Assert.AreEqual("U,Z", restoredDescription.OnlyWagonClasses);
        Assert.HasCount(1, restoredDescription.Origins);
        Assert.HasCount(1, restoredDescription.Destinations);

        var restoredDestination = restoredDescription.Destinations.First();
        Assert.AreEqual(5, restoredDestination.MaxNumberOfWagons);
        Assert.IsTrue(restoredDestination.AndRegions);
        Assert.AreEqual(station.Name, restoredDestination.Location.Name);
    }

    [TestMethod]
    public void IndustrialAreaCanBeOriginAndDestination()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var industry = new IndustrialArea(99, "Sockerbruket", "Skb") { Layout = timetable.Layout };
        timetable.Layout.OperationLocations.Add(industry);

        var description = timetable.Add(new CargoFlowOptions
        {
            Origins = { new Origin { Location = industry } },
            Destinations = { new Destination { Location = industry, AndRegions = true } },
        });

        Assert.IsTrue(industry.HasCargoExchange, "An industrial area always exchanges cargo.");
        Assert.AreEqual("Sockerbruket", description.OriginLocationNames);
        Assert.AreEqual("Sockerbruket", description.DestinationLocationNames);
        // AndRegions is set but an industrial area has no regions, so none are named.
        Assert.AreEqual("Sockerbruket", description.Destinations.First().ToString());
    }

    [TestMethod]
    public void PlanSavedUnderTheFormerStationNameStillLoads()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var station = timetable.Layout.OperationLocations.OfType<Station>().First();
        timetable.Add(new CargoFlowOptions
        {
            Origins = { new Origin { Location = station } },
            Destinations = { new Destination { Location = station } },
        });

        // Before the rename, origins and destinations wrote "Station" where "Location" is written now,
        // and nothing else in a plan was ever called "Location" — so this is exactly the earlier format.
        var legacyJson = JsonSerializer.Serialize(Plan.Create("Plan", timetable), Options)
            .Replace("\"Location\":", "\"Station\":", StringComparison.Ordinal);

        var restored = JsonSerializer.Deserialize<Plan>(legacyJson, Options);

        Assert.IsNotNull(restored);
        var restoredDescription = restored.Timetable.CargoFlowOptions.First();
        Assert.AreEqual(station.Name, restoredDescription.Origins.First().Location.Name);
        Assert.AreEqual(station.Name, restoredDescription.Destinations.First().Location.Name);
        // A track's own "Station" property is spelled the same and must be left alone.
        Assert.IsTrue(
            restored.Timetable.Layout.OperationLocations.SelectMany(l => l.Tracks).All(t => t.Station is not null),
            "Every track still knows the location it belongs to.");
    }

    [TestMethod]
    public void TheFormerStationNameIsNeverWritten()
    {
        // A standalone location, so the only "Station" that could appear would be the legacy alias.
        var json = JsonSerializer.Serialize(
            new Destination { Location = new IndustrialArea(1, "Sockerbruket", "Skb") }, Options);

        Assert.Contains("\"Location\":", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Station\":", json, StringComparison.Ordinal);
    }
}
