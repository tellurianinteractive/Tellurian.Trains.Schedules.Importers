using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a train category, a company, a country and a region are each stored in one place
/// only — the catalogue that owns them — and that everything referring to one keeps just its id,
/// without any of it being lost on the way out or on the way back in.
/// </summary>
[TestClass]
public class CatalogueStorageTests
{
    private static readonly JsonSerializerOptions Options = PlanJson.CreateOptions();

    // The settings an earlier version wrote with: no modifiers, so every reference carries the whole
    // object and the catalogues may hold nothing.
    private static readonly JsonSerializerOptions LegacyOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256,
    };

    // The same, but writing a layout's regions the way an earlier version did: no station ids at all,
    // and the catalogue after the locations, so a region is defined under the first station using it
    // and the catalogue is left holding a $ref into that station.
    private static readonly JsonSerializerOptions LegacyRegionOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { WriteRegionsTheOldWay } },
    };

    private static void WriteRegionsTheOldWay(JsonTypeInfo typeInfo)
    {
        foreach (var property in typeInfo.Properties)
        {
            if (typeInfo.Type == typeof(Layout) && property.Name == nameof(Layout.Regions)) property.Order = 1;
            if (typeInfo.Type == typeof(Station) && property.Name == nameof(Station.RegionIds))
                property.ShouldSerialize = static (_, _) => false;
        }
    }

    private static Plan CreatePlan(out Company danish, out Company german)
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        // Two companies with no id of their own, exactly as a plan carrying imported companies has them.
        danish = new Company(0, "Danske Statsbaner", "DSB", 3);
        german = new Company(0, "Deutsche Bahn", "DB", 4);
        timetable.Layout.Companies.Add(danish);
        timetable.Layout.Companies.Add(german);
        var trains = timetable.Trains.ToArray();
        trains[0].Company = danish;
        trains[1].Company = german;
        return Plan.Create("Plan", timetable);
    }

    [TestMethod]
    public void AForeignKeyFollowsTheObjectItPointsAt()
    {
        var plan = CreatePlan(out var danish, out _);
        var train = plan.Timetable.Trains.First();

        Assert.AreEqual(danish.Id, train.CompanyId, "Assigning the company is enough to set its id.");
        Assert.AreEqual(train.Category!.Id, train.CategoryId, "The same holds for the category.");

        danish.Id = 42;
        Assert.AreEqual(42, train.CompanyId, "The id keeps following the company it points at.");

        train.Company = null;
        Assert.IsNull(train.CompanyId, "Setting no company clears the id, rather than leaving the old one.");
        train.Category = null;
        Assert.IsNull(train.CategoryId, "The same holds for the category.");
    }

    [TestMethod]
    public void ATrainLeftWithNoCategoryOrCompanyStaysThatWayAcrossASave()
    {
        var plan = CreatePlan(out _, out _);
        var train = plan.Timetable.Trains.First();
        var number = train.Number;
        train.Company = null;
        train.Category = null;

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        var restoredTrain = restored.Timetable.Trains.Single(t => t.Number == number);
        Assert.IsNull(restoredTrain.Category, "A train given no category does not get its old one back.");
        Assert.IsNull(restoredTrain.Company, "Nor its old company.");
    }

    [TestMethod]
    public void ACategoryAndACompanyAreWrittenOnlyInTheirCatalogue()
    {
        var plan = CreatePlan(out _, out _);

        var json = JsonSerializer.Serialize(plan, Options);

        Assert.AreEqual(1, CountOf(json, "\"Signature\":\"DSB\""),
            "A company is written once — in the layout's catalogue.");
        Assert.AreEqual(1, CountOf(json, "\"Name\":\"FreightTrain\""),
            "A category is written once — in the timetable's catalogue.");
        Assert.DoesNotContain("\"Category\":", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Company\":", json, StringComparison.Ordinal);
        Assert.Contains("\"CategoryId\":", json, StringComparison.Ordinal);
        Assert.Contains("\"CompanyId\":", json, StringComparison.Ordinal);
    }

    [TestMethod]
    public void CompaniesWithoutAnIdOfTheirOwnStayApartAcrossASave()
    {
        var plan = CreatePlan(out var danish, out var german);
        Assert.AreEqual(danish.Id, german.Id, "Precondition: both companies share the default id.");

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        var trains = restored.Timetable.Trains.ToArray();
        Assert.AreEqual("DSB", trains[0].Company?.Signature);
        Assert.AreEqual("DB", trains[1].Company?.Signature);
        Assert.AreNotEqual(trains[0].Company!.Id, trains[1].Company!.Id,
            "Two companies are never left sharing one id, which a saved plan could not tell apart.");
        foreach (var train in trains)
            Assert.IsTrue(restored.Timetable.Layout.Companies.Any(c => ReferenceEquals(c, train.Company)),
                "A restored train's company is the catalogue entry itself.");
    }

    [TestMethod]
    public void ACompanyOnACategoryAVehicleOrADutySurvivesTheRoundTrip()
    {
        var plan = CreatePlan(out var danish, out var german);
        // The catalogue is filled in on the way out, so the category is reached through its train.
        var category = plan.Timetable.Trains.First().Category!;
        category.Company = german;
        var vehicle = plan.AddVehicle(new ScheduledObject(1, ScheduledObjectType.Locomotive, 1) { ExternalId = "Mx", Company = danish });
        var duty = plan.AddDriverDuty(new DriverDuty(1, "Duty 1") { Company = german });

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        Assert.AreEqual("DB", restored.Timetable.TrainCategories.Single(c => c.Name == category.Name).Company?.Signature);
        Assert.AreEqual(danish.Signature, restored.ScheduledObjects.Single(v => v.ExternalId == vehicle.ExternalId).Company?.Signature);
        Assert.AreEqual(german.Signature, restored.DriverDuties.Single(d => d.Id == duty.Id).Company?.Signature);
    }

    [TestMethod]
    public void ACountryIsStoredAsItsIdAndReadBackFromTheCatalogueInTheCode()
    {
        var plan = CreatePlan(out _, out _);
        plan.Layout.EnsureCountries();
        var expected = plan.Layout.Countries.Select(c => c.Id).OrderBy(id => id).ToArray();
        Assert.IsNotEmpty(expected, "Precondition: the layout uses at least one country.");

        var json = JsonSerializer.Serialize(plan, Options);
        Assert.DoesNotContain("\"ResourceKey\":", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"CountryCode\":", json, StringComparison.Ordinal);

        var restored = JsonSerializer.Deserialize<Plan>(json, Options);

        Assert.IsNotNull(restored);
        CollectionAssert.AreEqual(expected, restored.Layout.Countries.Select(c => c.Id).OrderBy(id => id).ToArray());
        foreach (var country in restored.Layout.Countries)
            Assert.AreEqual(Country.ById(country.Id), country,
                "A country is the catalogue's entry, not a copy the plan carries.");
    }

    [TestMethod]
    public void APlanWrittenWithWholeCountriesAndCompaniesStillReads()
    {
        var plan = CreatePlan(out _, out _);
        plan.Layout.EnsureCountries();
        var expectedCountries = plan.Layout.Countries.Select(c => c.Id).OrderBy(id => id).ToArray();

        var legacyJson = JsonSerializer.Serialize(plan, LegacyOptions);
        Assert.Contains("\"ResourceKey\":", legacyJson, StringComparison.Ordinal);

        var restored = JsonSerializer.Deserialize<Plan>(legacyJson, Options);

        Assert.IsNotNull(restored);
        CollectionAssert.AreEqual(expectedCountries, restored.Layout.Countries.Select(c => c.Id).OrderBy(id => id).ToArray());
        var trains = restored.Timetable.Trains.ToArray();
        Assert.AreEqual("DSB", trains[0].Company?.Signature, "The company written on the train is still read.");
        Assert.AreEqual("DB", trains[1].Company?.Signature);
        Assert.IsNotNull(trains[0].Category, "The category written on the train is still read.");
    }

    /// <summary>
    /// A plan is opened from a file with <c>DeserializeAsync</c>, which hands the reader one block at a
    /// time. A reader that has not yet seen the end of the document refuses to skip a token, so a
    /// converter reading past a property it does not know must not ask it to.
    /// </summary>
    [TestMethod]
    public async Task APlanWrittenWithWholeCountriesStillReadsFromAStream()
    {
        var plan = CreatePlan(out _, out _);
        plan.Layout.EnsureCountries();
        var expectedCountries = plan.Layout.Countries.Select(c => c.Id).OrderBy(id => id).ToArray();
        Assert.IsNotEmpty(expectedCountries, "Precondition: the layout uses at least one country.");

        using var legacyJson = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(plan, LegacyOptions)));

        var restored = await JsonSerializer.DeserializeAsync<Plan>(legacyJson, Options);

        Assert.IsNotNull(restored);
        CollectionAssert.AreEqual(expectedCountries, restored.Layout.Countries.Select(c => c.Id).OrderBy(id => id).ToArray());
    }

    private static Plan CreatePlanWithARegionOnAStation(out Region region, out Station station)
    {
        var plan = CreatePlan(out _, out _);
        var layout = plan.Layout;
        region = new Region { Id = 3, Name = "Söder", CountryId = 1, BackgroundColor = "#009933" };
        layout.Add(region);
        station = layout.OperationLocations.OfType<Station>().First();
        station.Add(region);
        return plan;
    }

    [TestMethod]
    public void ARegionIsWrittenOnlyInTheLayoutCatalogue()
    {
        var plan = CreatePlanWithARegionOnAStation(out _, out _);

        var json = JsonSerializer.Serialize(plan, Options);

        Assert.AreEqual(1, CountOf(json, "\"Name\":\"S\\u00F6der\""),
            "A region is written once — in the layout's catalogue.");
        Assert.AreEqual(1, CountOf(json, "\"Regions\":"),
            "That catalogue is the only list of regions written; no station writes its own.");
        Assert.Contains("\"RegionIds\":", json, StringComparison.Ordinal);
    }

    [TestMethod]
    public void AStationWithNoRegionsWritesNothingAboutThem()
    {
        var plan = CreatePlanWithARegionOnAStation(out _, out var station);
        Assert.IsGreaterThan(1, plan.Layout.OperationLocations.OfType<Station>().Count(),
            "Precondition: the layout has stations besides the one carrying the region.");

        var json = JsonSerializer.Serialize(plan, Options);

        Assert.AreEqual(1, CountOf(json, "\"RegionIds\":"),
            "Only the one station that has regions says anything about them.");
        Assert.Contains($"\"RegionIds\":[{station.Regions.Single().Id}]", json.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal,
            "The ids are a plain array, not the object ReferenceHandler.Preserve wraps a collection in.");
    }

    [TestMethod]
    public void AStationsRegionsAreTheCatalogueEntriesAfterARoundTrip()
    {
        var plan = CreatePlanWithARegionOnAStation(out var region, out var station);

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        var restoredStation = restored.Layout.OperationLocations.OfType<Station>().Single(s => s.Signature == station.Signature);
        var restoredRegion = restoredStation.Regions.Single();
        Assert.AreEqual(region.Id, restoredRegion.Id);
        Assert.AreEqual(region.Name, restoredRegion.Name);
        Assert.AreEqual(region.BackgroundColor, restoredRegion.BackgroundColor);
        Assert.IsTrue(restored.Layout.Regions.Any(r => ReferenceEquals(r, restoredRegion)),
            "A restored station's region is the catalogue entry itself, so editing it there shows here.");
    }

    [TestMethod]
    public void AStationLeftWithNoRegionsStaysThatWayAcrossASave()
    {
        var plan = CreatePlanWithARegionOnAStation(out var region, out var station);
        station.Regions.Remove(region);

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        var restoredStation = restored.Layout.OperationLocations.OfType<Station>().Single(s => s.Signature == station.Signature);
        Assert.IsEmpty(restoredStation.Regions, "A station given no region does not get its old one back.");
        Assert.AreEqual(1, restored.Layout.Regions.Count(r => r.Id == region.Id),
            "The region itself stays in the catalogue, which is what it was detached from the station into.");
    }

    [TestMethod]
    public void ARegionAStationHoldsIsPutIntoTheCatalogueBeforeThePlanIsWritten()
    {
        var plan = CreatePlan(out _, out _);
        var station = plan.Layout.OperationLocations.OfType<Station>().First();
        // The state an importer, or any code adding a region directly, can leave a layout in: the
        // station has the region, the catalogue that is the only place it gets written has not.
        station.Add(new Region { Id = 0, Name = "Fyn", CountryId = 3 });
        Assert.IsEmpty(plan.Layout.Regions, "Precondition: the catalogue does not hold it.");

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        Assert.AreEqual("Fyn", restored.Layout.Regions.Single().Name,
            "Writing the plan put it into the catalogue rather than losing it.");
        var restoredStation = restored.Layout.OperationLocations.OfType<Station>().Single(s => s.Signature == station.Signature);
        Assert.AreEqual("Fyn", restoredStation.Regions.Single().Name, "And the station still has it.");
        Assert.IsGreaterThan(0, restoredStation.Regions.Single().Id,
            "A region left on id zero could not be told from any other, so it is given one of its own.");
    }

    [TestMethod]
    public void APlanWrittenWithWholeRegionsOnItsStationsStillReads()
    {
        var plan = CreatePlanWithARegionOnAStation(out var region, out var station);

        var legacyJson = JsonSerializer.Serialize(plan, LegacyRegionOptions);
        Assert.DoesNotContain("\"RegionIds\":", legacyJson, StringComparison.Ordinal,
            "Precondition: a plan written by an earlier version has no such property.");
        Assert.IsGreaterThan(
            legacyJson.IndexOf("\"OperationLocations\":", StringComparison.Ordinal),
            legacyJson.IndexOf("\"Name\":\"S\\u00F6der\"", StringComparison.Ordinal),
            "Precondition: the region is defined under the station, the catalogue after it holding a $ref into it.");

        var restored = JsonSerializer.Deserialize<Plan>(legacyJson, Options);

        Assert.IsNotNull(restored);
        var restoredStation = restored.Layout.OperationLocations.OfType<Station>().Single(s => s.Signature == station.Signature);
        Assert.AreEqual(region.Name, restoredStation.Regions.Single().Name,
            "The region written on the station is still read.");
        Assert.IsTrue(restored.Layout.Regions.Any(r => ReferenceEquals(r, restoredStation.Regions.Single())),
            "And it is still the same object the catalogue holds.");
    }

    private static int CountOf(string text, string value)
    {
        var compact = text.Replace(" ", "", StringComparison.Ordinal);
        var count = 0;
        for (var i = compact.IndexOf(value, StringComparison.Ordinal); i >= 0;
             i = compact.IndexOf(value, i + value.Length, StringComparison.Ordinal)) count++;
        return count;
    }
}
