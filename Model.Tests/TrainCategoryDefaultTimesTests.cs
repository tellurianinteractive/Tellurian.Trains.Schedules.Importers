namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// A train category carries the preparation and finishing-up times its trains are planned with. Both can
/// be given to the trains that already exist, one at a time, and doing so moves nothing but the minutes at
/// the very ends of a train's run — a reapply never changes where or when a train actually runs.
/// </summary>
[TestClass]
public class TrainCategoryDefaultTimesTests
{
    [TestInitialize]
    public void TestInitialize() => TestDataFactory.Init();

    // G -> Yb -> Snu, departing G at startTime and arriving Snu 55 minutes later, made ready
    // preparationMinutes before it departs and put away finishingMinutes after it has arrived.
    private static Train TrainOf(TrainCategory category, int number, Time startTime, int preparationMinutes, int finishingMinutes)
    {
        var stations = TestDataFactory.Stations.ToArray();
        var train = new Train(number, category, number) { Category = category };
        _ = train.Add(new StationCall(1, stations[0]["3"], startTime.AddMinutes(-preparationMinutes), startTime));
        _ = train.Add(new StationCall(2, stations[1]["2"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
        _ = train.Add(new StationCall(3, stations[2]["1"], startTime.AddMinutes(55), startTime.AddMinutes(55 + finishingMinutes)));
        return train;
    }

    private static TrainCategory Category(int id, int preparation, int finishing) =>
        new() { Id = id, Name = $"C{id}", Prefix = "P", DefaultPreparationMinutes = preparation, DefaultFinishingMinutes = finishing };

    [TestMethod]
    public void ACategoryPreparesAndFinishesInTenMinutesUntilToldOtherwise()
    {
        var category = new TrainCategory { Id = 1, Name = "Passenger" };

        Assert.AreEqual(10, category.DefaultPreparationMinutes);
        Assert.AreEqual(10, category.DefaultFinishingMinutes);
    }

    [TestMethod]
    public void ATrainKnowsHowLongItIsPreparedAndFinished()
    {
        var train = TrainOf(Category(1, 10, 10), 1, Time.FromHourAndMinute(12, 00), preparationMinutes: 20, finishingMinutes: 15);

        Assert.AreEqual(20, train.PreparationMinutes);
        Assert.AreEqual(15, train.FinishingMinutes);
    }

    [TestMethod]
    public void ReapplyingThePreparationTimeMovesTheOriginArrivalAndNothingElse()
    {
        var category = Category(1, preparation: 25, finishing: 10);
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var train = timetable.Add(TrainOf(category, 1, Time.FromHourAndMinute(12, 00), preparationMinutes: 5, finishingMinutes: 15));

        var changed = timetable.ApplyDefaultPreparationMinutes(category);

        Assert.AreEqual(1, changed);
        Assert.AreEqual(Time.FromHourAndMinute(11, 35), train.CallsInRunOrder[0].Arrival, "The train is now made ready 25 minutes before it departs.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 00), train.CallsInRunOrder[0].Departure, "It still departs at twelve.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 55), train.CallsInRunOrder[^1].Arrival, "It still arrives at 12:55.");
        Assert.AreEqual(15, train.FinishingMinutes, "Its finishing-up time is untouched.");
    }

    [TestMethod]
    public void ReapplyingTheFinishingTimeMovesTheDestinationDepartureAndNothingElse()
    {
        var category = Category(1, preparation: 10, finishing: 30);
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var train = timetable.Add(TrainOf(category, 1, Time.FromHourAndMinute(12, 00), preparationMinutes: 5, finishingMinutes: 15));

        var changed = timetable.ApplyDefaultFinishingMinutes(category);

        Assert.AreEqual(1, changed);
        Assert.AreEqual(Time.FromHourAndMinute(13, 25), train.CallsInRunOrder[^1].Departure, "The train is now put away 30 minutes after it arrives.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 55), train.CallsInRunOrder[^1].Arrival, "It still arrives at 12:55.");
        Assert.AreEqual(5, train.PreparationMinutes, "Its preparation time is untouched.");
    }

    [TestMethod]
    public void OnlyTheTrainsOfTheCategoryAreReached()
    {
        var passenger = Category(1, preparation: 25, finishing: 10);
        var freight = Category(2, preparation: 45, finishing: 10);
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var passengerTrain = timetable.Add(TrainOf(passenger, 1, Time.FromHourAndMinute(12, 00), preparationMinutes: 5, finishingMinutes: 15));
        var freightTrain = timetable.Add(TrainOf(freight, 2, Time.FromHourAndMinute(14, 00), preparationMinutes: 5, finishingMinutes: 15));

        var changed = timetable.ApplyDefaultPreparationMinutes(passenger);

        Assert.AreEqual(1, changed);
        Assert.AreEqual(25, passengerTrain.PreparationMinutes);
        Assert.AreEqual(5, freightTrain.PreparationMinutes, "The freight train is of another category and is left as it was.");
    }

    [TestMethod]
    public void ATrainAlreadyPreparedForThatLongIsNotCounted()
    {
        var category = Category(1, preparation: 25, finishing: 10);
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TrainOf(category, 1, Time.FromHourAndMinute(12, 00), preparationMinutes: 25, finishingMinutes: 15));
        timetable.Add(TrainOf(category, 2, Time.FromHourAndMinute(14, 00), preparationMinutes: 5, finishingMinutes: 15));

        var changed = timetable.ApplyDefaultPreparationMinutes(category);

        Assert.AreEqual(1, changed, "Only the train that was prepared for another length of time is counted.");
    }

    [TestMethod]
    public void APreparationReachingBackBeforeMidnightIsRefused()
    {
        var train = TrainOf(Category(1, 10, 10), 1, Time.FromHourAndMinute(0, 10), preparationMinutes: 5, finishingMinutes: 10);

        var changed = train.SetPreparationMinutes(20);

        Assert.IsFalse(changed, "A train departing at 00:10 cannot be made ready from 23:50 the day before.");
        Assert.AreEqual(5, train.PreparationMinutes);
    }

    [TestMethod]
    public void ReapplyingIsRepeatable()
    {
        var category = Category(1, preparation: 25, finishing: 30);
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var train = timetable.Add(TrainOf(category, 1, Time.FromHourAndMinute(12, 00), preparationMinutes: 5, finishingMinutes: 15));

        timetable.ApplyDefaultPreparationMinutes(category);
        timetable.ApplyDefaultFinishingMinutes(category);
        var changedAgain = timetable.ApplyDefaultPreparationMinutes(category) + timetable.ApplyDefaultFinishingMinutes(category);

        Assert.AreEqual(0, changedAgain, "The second reapply has nothing left to change.");
        Assert.AreEqual(25, train.PreparationMinutes);
        Assert.AreEqual(30, train.FinishingMinutes);
    }
}
