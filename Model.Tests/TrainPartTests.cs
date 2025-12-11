using Tellurian.Trains.Schedules.Importers.Model;

namespace Tellurian.Trains.Schedules.Importers.Model.Tests;

[TestClass]
public class TrainPartTests
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    public Train Train { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        Train = TestDataFactory.CreateTrain1();
    }

    [TestMethod]
    public void NullTrainThrows()
    {
        Assert.Throws<ArgumentNullException>(() => TrainPartExtensions.AsTrainPart(null, 0, 1));
    }

    [TestMethod]
    public void NegativeStartIndexThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Train.AsTrainPart(-1, 1));
    }

    [TestMethod]
    public void FromIndexIsLastIndexThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Train.AsTrainPart(Train.Calls.Count - 1, 1));
    }

    [TestMethod]
    public void ToIndexEqualToStartIndexThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Train.AsTrainPart(1, 1));
    }

    [TestMethod]
    public void ToIndexIsGreaterThanLastThrows()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Train.AsTrainPart(1, 3));
    }

    [TestMethod]
    public void FromAndToStationsAreSet()
    {
        var target = Train.AsTrainPart(1, 2);
        Assert.AreEqual("Ytterby", target.From.Station.Name);
        Assert.AreEqual("Stenungsund", target.To.Station.Name);
    }

    [TestMethod]
    public void EqualWorks()
    {
        var one = Train.AsTrainPart(1, 2);
        var another = Train.AsTrainPart(1, 2);
        Assert.AreEqual(one, another);
    }
}
