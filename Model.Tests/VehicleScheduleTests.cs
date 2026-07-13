namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class VehicleScheduleTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Schedule Target { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        Target = new Schedule(1);
        TestDataFactory.Init();
    }

    [TestMethod]
    public void ConstructorSetsProperties()
    {
        Assert.AreEqual(1, Target.Id);
    }

    [TestMethod]
    public void AddsNullTrainPartThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Target.Add(null!));
    }

    [TestMethod]
    public void AddsTrainPart()
    {
        var train = TestDataFactory.CreateTrains("Persontåg", Time.FromHourAndMinute(12, 00)).First();
        var part = train.AsTrainPart(0, 1);
        Target.Add(part);
        Assert.AreEqual(part, Target.Parts.First());
    }

    [TestMethod]
    public void AddingEquivalentTrainPartTwiceKeepsOne()
    {
        var train = TestDataFactory.CreateTrains("Persontåg", Time.FromHourAndMinute(12, 00)).First();
        Target.Add(train.AsTrainPart(0, 1));
        Target.Add(train.AsTrainPart(0, 1));
        Assert.HasCount(1, Target.Parts);
    }

    [TestMethod]
    public void FirstPartSetsScheduleNumberFromTrainNumber()
    {
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var train = TestDataFactory.CreateTrainInForwardDirection(category, 42, Time.FromHourAndMinute(12, 00));
        Target.Add(train.AsTrainPart);
        Assert.AreEqual(42, Target.Number);
    }

    [TestMethod]
    public void AppendedContinuationPartIsAccepted()
    {
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var first = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        var next = TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00));
        Target.Append(first.AsTrainPart);

        var result = Target.Append(next.AsTrainPart);

        Assert.IsTrue(result.HasValue);
        Assert.HasCount(2, Target.Parts);
    }

    [TestMethod]
    public void AppendingPartThatDoesNotStartWhereScheduleEndsIsRejected()
    {
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var first = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        // Another forward train starts at the same origin, not where the first train ends.
        var discontiguous = TestDataFactory.CreateTrainInForwardDirection(category, 2, Time.FromHourAndMinute(13, 00));
        Target.Append(first.AsTrainPart);

        var result = Target.Append(discontiguous.AsTrainPart);

        Assert.IsTrue(result.IsNone);
        Assert.HasCount(1, Target.Parts);
    }

    [TestMethod]
    public void AppendingOverlappingPartIsRejected()
    {
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var first = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        // Departs the end station while the first train is still running (12:00–12:55).
        var overlapping = TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(12, 30));
        Target.Append(first.AsTrainPart);

        var result = Target.Append(overlapping.AsTrainPart);

        Assert.IsTrue(result.IsNone);
        Assert.HasCount(1, Target.Parts);
    }
}
