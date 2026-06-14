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
    }

    [TestMethod]
    public void ConstructorSetsProperties()
    {
        Assert.AreEqual(1, Target.Id);
    }

    [TestMethod]
    public void AddsNullTrainPartThrows()
    {
        Assert.Throws<ArgumentNullException>(() => Target.Add(null));
    }

    [TestMethod]
    public void AddsTrainPart()
    {
        TestDataFactory.Init();
        var train = TestDataFactory.CreateTrains("Persontåg", Time.FromHourAndMinute(12, 00)).First();
        var part = train.AsTrainPart(0, 1);
        Target.Add(part);
        Assert.AreEqual(part, Target.Parts.First());
    }

    [TestMethod]
    public void AddingEquivalentTrainPartTwiceKeepsOne()
    {
        TestDataFactory.Init();
        var train = TestDataFactory.CreateTrains("Persontåg", Time.FromHourAndMinute(12, 00)).First();
        Target.Add(train.AsTrainPart(0, 1));
        Target.Add(train.AsTrainPart(0, 1));
        Assert.HasCount(1, Target.Parts);
    }
}
