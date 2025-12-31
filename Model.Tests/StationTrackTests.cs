using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;


[TestClass]
public class StationTrackTests
{
    private StationTrack Target = default!;
    private Train Train1 = default!;
    private Train Train2 = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        var category1 = new TrainCategory() { Id = 10, Prefix = "G", ResourceName = "FreightTrain" };
        var category2 = new TrainCategory() { Id = 11, Prefix = "P", ResourceName = "PassengerTrain" };
        Target = TestDataFactory.CreateStationTrack();
        Train1 = new Train(1, category1, 4321);
        Train2 = new Train(1, category2, 1234);
    }

    [TestMethod]
    public void WhenNoCallsThenTimeslotIsFree()
    {
        Train1.Add(new StationCall(1, Target, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30)));
        Assert.AreEqual(1, Target.Calls.Count);
        Assert.AreEqual(Train1.Calls[0], Target.Calls.First());
    }

    [TestMethod]
    public void WhenArrival1IsSameTimeAsDeparture2ThenNotConflict()
    {
        Train1.Add(new StationCall(1, Target, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30)));
        Train2.Add(new StationCall(2, Target, Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45)));
        var validationErrors = Target.GetValidationErrors([]);
        Assert.AreEqual(0, validationErrors.Count());
        Assert.IsFalse(validationErrors.Any(ve => string.IsNullOrWhiteSpace(ve.Message.Text)));
    }

    [TestMethod]
    public void WhenCallsNotOverlapsThenTimeslotIsFree()
    {
        Train1.Add(new StationCall(1, Target, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30)));
        Train2.Add(new StationCall(2, Target, Time.FromHourAndMinute(12, 31), Time.FromHourAndMinute(12, 45)));
        Assert.AreEqual(2, Target.Calls.Count);
        var validationErrors = Target.GetValidationErrors([]);
        Assert.AreEqual(0, validationErrors.Count());
        Assert.IsFalse(validationErrors.Any(ve => string.IsNullOrWhiteSpace(ve.Message.Text)));
    }
}
