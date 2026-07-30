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
        var category1 = new TrainCategory() { Id = 10, Prefix = "G", Name = "FreightTrain" };
        var category2 = new TrainCategory() { Id = 11, Prefix = "P", Name = "PassengerTrain" };
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

    // Every case above is a NEGATIVE one, which is how the conflict check came to be dead without
    // anyone noticing: its overlap predicate had both comparisons the wrong way round, so it reported
    // nothing at all and every "is free" test passed for the wrong reason.
    [TestMethod]
    public void WhenCallsOverlapThenConflictIsReported()
    {
        Train1.Add(new StationCall(1, Target, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30)));
        Train2.Add(new StationCall(2, Target, Time.FromHourAndMinute(12, 15), Time.FromHourAndMinute(12, 45)));

        var validationErrors = Target.GetValidationErrors([]).ToList();

        Assert.HasCount(1, validationErrors);
        Assert.IsFalse(validationErrors.Any(ve => string.IsNullOrWhiteSpace(ve.Message.Text)));
    }

    [TestMethod]
    public void TrainsOnDifferentSessionsNeverContendForTheTrack()
    {
        Train1.Sessions = Sessions.FromSessionNumbers(1, 3, 5);
        Train2.Sessions = Sessions.FromSessionNumbers(2, 4, 6);
        Train1.Add(new StationCall(1, Target, Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 30)));
        Train2.Add(new StationCall(2, Target, Time.FromHourAndMinute(12, 15), Time.FromHourAndMinute(12, 45)));

        // They are never at the station on the same day, so they cannot want the same track.
        Assert.IsEmpty(Target.GetValidationErrors([]));
    }
}
