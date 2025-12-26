using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class TrainTests
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private Train Target;
    private static TrainCategory Category => new() { Id = 1, Prefix = "G", ResourceName = "FreightTrain" };

    [TestInitialize]
    public void TestInitialize()
    {

        Target = new(11, Category, 1234, "");
    }

    [TestMethod]
    public void PropertiesAreSet()
    {
        Assert.AreEqual(Category, Target.Category);
        Assert.AreEqual(1234, Target.Number);
        Assert.AreEqual("", Target.ExtenalId);
    }

    [TestMethod]
    public void AddsFirstTimetableCall()
    {
        var station = TestDataFactory.CreateStation1();
        var call = new StationCall(station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45));
        Target.Add(call);
        Assert.IsFalse(Target.CheckTrainTimeSequence().Any());
    }

    [TestMethod]
    public void WhenSecondTimetableCallIsBeforeLastThenValidationErrors()
    {
        var station = TestDataFactory.CreateStation1();
        Target.Add(new StationCall(station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45)));
        Target.Add(new StationCall(station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45)));
        var validationErrors = Target.GetValidationErrors(new ValidationOptions());
        Assert.AreEqual(1, validationErrors.Count());
        Assert.IsFalse(validationErrors.Any(ve => string.IsNullOrWhiteSpace(ve.Text)));
    }

    [TestMethod]
    public void WhenSecondTimetableCallIsAfterLastThenThrows()
    {
        var station = TestDataFactory.CreateStation1();
        var call1 = new StationCall(station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45));
        var call2 = new StationCall(station.Tracks.First(), Time.FromHourAndMinute(12, 50), Time.FromHourAndMinute(12, 55));
        Target.Add(call1);
        Target.Add(call2);
    }
}
