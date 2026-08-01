using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that the train category catalogue is reconciled with the categories the trains actually
/// use: a plan whose catalogue is empty, or whose categories share an id, is a plan whose category
/// drop-down is empty while its trains are still grouped under categories, and whose trains of
/// different categories are taken for one another.
/// </summary>
[TestClass]
public class RebuildTrainCategoriesTests
{
    private static readonly JsonSerializerOptions Options = PlanJson.CreateOptions();

    // What an earlier version wrote: the same settings without the modifiers that keep a catalogue
    // entry to its catalogue, so every train carries its whole category and the catalogue may be empty.
    private static readonly JsonSerializerOptions LegacyOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 256,
    };

    [TestMethod]
    public void AddsTheCategoriesTheTrainsUseButTheCatalogueDoesNot()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        Assert.IsEmpty(timetable.TrainCategories, "Precondition: the catalogue is empty.");

        timetable.RebuildTrainCategories();

        foreach (var train in timetable.Trains)
            Assert.IsTrue(timetable.TrainCategories.Any(c => ReferenceEquals(c, train.Category)),
                "Every category a train uses is in the catalogue.");
    }

    [TestMethod]
    public void KeepsTheCategoriesTheCatalogueAlreadyHas()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var unused = new TrainCategory { Id = 7, Name = "Empty", Prefix = "E" };
        timetable.TrainCategories.Add(unused);

        timetable.RebuildTrainCategories();

        Assert.IsTrue(timetable.TrainCategories.Any(c => ReferenceEquals(c, unused)),
            "A category no train uses yet stays in the catalogue.");
        Assert.AreEqual(7, unused.Id, "A category with an id of its own keeps it.");
    }

    [TestMethod]
    public void GivesEveryCategoryAnIdOfItsOwn()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        // How a plan written before the catalogue existed reads back: every category carries the default
        // id, so the whole timetable looks like one category — and like the trains with no category.
        foreach (var train in timetable.Trains) train.Category!.Id = 0;

        timetable.RebuildTrainCategories();

        var ids = timetable.TrainCategories.Select(c => c.Id).ToArray();
        Assert.HasCount(timetable.TrainCategories.Count, ids.Distinct(), "The ids are unique.");
        Assert.IsTrue(ids.All(id => id > 0), "No category is left with the id that means 'no category'.");
        foreach (var train in timetable.Trains)
            Assert.AreEqual(train.Category!.Id, train.CategoryId,
                "A train's category id follows its renumbered category.");
    }

    [TestMethod]
    public void IsIdempotent()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();

        timetable.RebuildTrainCategories();
        var expected = timetable.TrainCategories.Select(c => (c.Name, c.Id)).ToArray();
        timetable.RebuildTrainCategories();

        Assert.AreEqual(expected.Length, timetable.TrainCategories.Count,
            "Rebuilding twice must not duplicate categories.");
        CollectionAssert.AreEqual(expected, timetable.TrainCategories.Select(c => (c.Name, c.Id)).ToArray(),
            "Rebuilding twice must not renumber categories again.");
    }

    [TestMethod]
    public void APlanSavedWithAnEmptyCatalogueLoadsWithItFilledIn()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var plan = Plan.Create("Plan", timetable);
        Assert.IsEmpty(plan.Timetable.TrainCategories, "Precondition: the plan has an empty catalogue.");
        var json = JsonSerializer.Serialize(plan, LegacyOptions);
        Assert.Contains("\"Name\":\"FreightTrain\"", json.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);

        var restored = JsonSerializer.Deserialize<Plan>(json, Options);

        Assert.IsNotNull(restored);
        var categories = restored.Timetable.TrainCategories;
        Assert.HasCount(2, categories, "Both categories the trains use are in the restored catalogue.");
        CollectionAssert.AreEquivalent(
            new[] { "FreightTrain", "PassengerTrain" },
            categories.Select(c => c.Name).ToArray());
        foreach (var train in restored.Timetable.Trains)
            Assert.IsTrue(categories.Any(c => ReferenceEquals(c, train.Category)),
                "A restored train's category is the catalogue entry itself, not a copy beside it.");
    }

    [TestMethod]
    public void SavingAPlanCompletesTheCatalogueSoNoCategoryIsLost()
    {
        TestDataFactory.Init();
        var timetable = TestDataFactory.CreateTimetable();
        var plan = Plan.Create("Plan", timetable);
        // A category is now written only in the catalogue, so a plan saved with an empty one — the state
        // an importer leaves it in — would otherwise write its categories nowhere at all.
        Assert.IsEmpty(plan.Timetable.TrainCategories, "Precondition: the plan has an empty catalogue.");

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        Assert.HasCount(2, restored.Timetable.TrainCategories);
        foreach (var train in restored.Timetable.Trains)
            Assert.IsNotNull(train.Category, "No train loses its category by being saved.");
    }
}
