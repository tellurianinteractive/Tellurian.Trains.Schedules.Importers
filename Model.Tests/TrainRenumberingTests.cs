namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// A category's trains can be renumbered from its start number. They all move by the same amount, so the
/// gaps and pairs between their numbers are kept, and the amount is even so no train changes direction.
/// </summary>
[TestClass]
public class TrainRenumberingTests
{
    [TestInitialize]
    public void TestInitialize() => TestDataFactory.Init();

    private static (Timetable Timetable, Train[] Trains) TimetableWith(TrainCategory category, params int[] numbers)
    {
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var trains = numbers.Select((n, i) => timetable.Add(new Train(i + 1, category, n))).ToArray();
        return (timetable, trains);
    }

    private static TrainCategory Category(int startNumber, bool isShunting = false) =>
        new() { Id = 1, Name = "C", Prefix = "P", StartNumber = startNumber, IsShunting = isShunting };

    [TestMethod]
    public void TrainsMoveUpToTheNewStartNumberKeepingGapsAndPairs()
    {
        var category = Category(5000);
        var (timetable, trains) = TimetableWith(category, 101, 102, 103, 104, 111);
        category.StartNumber = 6000;

        var renumbered = timetable.RenumberTrains(category);

        Assert.AreEqual(5, renumbered);
        CollectionAssert.AreEqual(new[] { 6001, 6002, 6003, 6004, 6011 }, trains.Select(t => t.Number).ToArray());
    }

    [TestMethod]
    public void NoTrainChangesBetweenOddAndEven()
    {
        var category = Category(100);
        var (timetable, trains) = TimetableWith(category, 1, 2, 3);

        timetable.RenumberTrains(category);

        CollectionAssert.AreEqual(new[] { 101, 102, 103 }, trains.Select(t => t.Number).ToArray(),
            "Train 1 is odd, so it gets the first odd number at or above 100.");
    }

    [TestMethod]
    public void TrainsMoveDownToALowerStartNumber()
    {
        var category = Category(1);
        var (timetable, trains) = TimetableWith(category, 5001, 5002, 5010);

        timetable.RenumberTrains(category);

        CollectionAssert.AreEqual(new[] { 1, 2, 10 }, trains.Select(t => t.Number).ToArray());
    }

    [TestMethod]
    public void AnEvenLowestTrainGetsAnEvenStartNumber()
    {
        var category = Category(201);
        var (timetable, trains) = TimetableWith(category, 10, 11);

        timetable.RenumberTrains(category);

        CollectionAssert.AreEqual(new[] { 202, 203 }, trains.Select(t => t.Number).ToArray());
    }

    [TestMethod]
    public void ShuntingTasksStartFromTheStartNumberItself()
    {
        var category = Category(9001, isShunting: true);
        var (timetable, trains) = TimetableWith(category, 9000, 9001, 9005);

        timetable.RenumberTrains(category);

        CollectionAssert.AreEqual(new[] { 9001, 9002, 9006 }, trains.Select(t => t.Number).ToArray(),
            "A shunting task travels nowhere, so its parity means nothing.");
    }

    [TestMethod]
    public void TrainsOfOtherCategoriesAreLeftAlone()
    {
        var passenger = Category(100);
        var freight = new TrainCategory { Id = 2, Name = "G", Prefix = "G", StartNumber = 5000 };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var passengerTrain = timetable.Add(new Train(1, passenger, 1));
        var freightTrain = timetable.Add(new Train(2, freight, 5001));

        var renumbered = timetable.RenumberTrains(passenger);

        Assert.AreEqual(1, renumbered);
        Assert.AreEqual(101, passengerTrain.Number);
        Assert.AreEqual(5001, freightTrain.Number);
    }

    [TestMethod]
    public void TrainsAlreadyStartingFromTheStartNumberAreNotCounted()
    {
        var category = Category(100);
        var (timetable, _) = TimetableWith(category, 101, 102);

        Assert.AreEqual(0, timetable.RenumberTrains(category));
    }
}
