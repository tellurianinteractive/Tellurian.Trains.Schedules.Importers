using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a train category, a company and a country are each stored in one place only — the
/// catalogue that owns them — and that everything referring to one keeps just its id, without any of
/// it being lost on the way out or on the way back in.
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

    private static int CountOf(string text, string value)
    {
        var compact = text.Replace(" ", "", StringComparison.Ordinal);
        var count = 0;
        for (var i = compact.IndexOf(value, StringComparison.Ordinal); i >= 0;
             i = compact.IndexOf(value, i + value.Length, StringComparison.Ordinal)) count++;
        return count;
    }
}
