using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Rule T9: a train arrives at and departs from only scheduled tracks.
/// See <c>ValidationExtensions.CheckScheduledTracks</c>.
/// </summary>
[TestClass]
public class ScheduledTrackValidationTests
{
    private static readonly TrainCategory Freight = new() { Id = 1, Name = "Freight", Prefix = "G", Content = TrainContent.Cargo };
    private static readonly TrainCategory Shunting = new() { Id = 2, Name = "Shunting", Prefix = "S", IsShunting = true };

    [TestMethod]
    public void AStopAtAnUnscheduledTrackIsReported()
    {
        var station = StationWithUnscheduledSiding();
        var siding = station.Tracks.Last();
        var train = TrainStoppingAt(Freight, siding);

        var errors = train.CheckScheduledTracks().ToList();

        Assert.HasCount(1, errors);
        Assert.AreEqual(ValidationErrorType.StopAtUnscheduledTrack, errors[0].ErrorType);
        Assert.AreEqual(Severity.Warning, errors[0].Message.Severity);
        Assert.IsTrue(errors[0].Involves(train));
        Assert.IsTrue(errors[0].Involves(siding));
        Assert.IsFalse(string.IsNullOrWhiteSpace(errors[0].Message.Text));
    }

    [TestMethod]
    public void AStopAtAScheduledTrackIsNotReported()
    {
        var station = StationWithUnscheduledSiding();
        var train = TrainStoppingAt(Freight, station.Tracks.First());

        Assert.IsEmpty(train.CheckScheduledTracks());
    }

    [TestMethod]
    public void AnArrivalAloneAndADepartureAloneAreBothReported()
    {
        foreach (var (isArrival, isDeparture) in new[] { (true, false), (false, true) })
        {
            var station = StationWithUnscheduledSiding();
            var train = TrainStoppingAt(Freight, station.Tracks.Last());
            var call = train.Calls[0];
            call.IsArrival = isArrival;
            call.IsDeparture = isDeparture;

            Assert.HasCount(1, train.CheckScheduledTracks().ToList(),
                $"IsArrival={isArrival}, IsDeparture={isDeparture} uses the unscheduled track.");
        }
    }

    [TestMethod]
    public void ATrainMerelyStandingAtAnUnscheduledTrackIsNotReported()
    {
        var station = StationWithUnscheduledSiding();
        var train = TrainStoppingAt(Freight, station.Tracks.Last());
        var call = train.Calls[0];
        call.IsArrival = false;
        call.IsDeparture = false;

        Assert.IsEmpty(train.CheckScheduledTracks());
    }

    [TestMethod]
    public void AShuntingTaskIsNotReported()
    {
        var station = StationWithUnscheduledSiding();
        var task = TrainStoppingAt(Shunting, station.Tracks.Last());

        Assert.IsEmpty(task.CheckScheduledTracks());
    }

    [TestMethod]
    public void TheRuleCanBeSwitchedOff()
    {
        var station = StationWithUnscheduledSiding();
        var train = TrainStoppingAt(Freight, station.Tracks.Last());

        Assert.HasCount(1, train.GetValidationErrors(new ValidationSettings())
            .Where(e => e.ErrorType == ValidationErrorType.StopAtUnscheduledTrack).ToList());
        Assert.IsEmpty(train.GetValidationErrors(new ValidationSettings { ValidateScheduledTracks = false })
            .Where(e => e.ErrorType == ValidationErrorType.StopAtUnscheduledTrack));
    }

    private static Station StationWithUnscheduledSiding()
    {
        var station = new Station(1, "Alpha", "A") { IsManned = true };
        station.Add(new StationTrack(11, "1", isMain: true, isScheduled: true));
        station.Add(new StationTrack(12, "2", isMain: false, isScheduled: false));
        return station;
    }

    // A train with a single call at the given track, stopping there. The call's flags are set after it
    // joins the train, since Train.Add makes the first call a departure only.
    private static Train TrainStoppingAt(TrainCategory category, StationTrack track)
    {
        var train = new Train(1, 1001) { Category = category, CategoryId = category.Id };
        var call = train.Add(new StationCall(1, track, Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 2)));
        call.IsArrival = true;
        call.IsDeparture = true;
        return train;
    }
}
