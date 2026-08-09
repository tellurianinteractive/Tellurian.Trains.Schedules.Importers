using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Rule T6: a passenger train that stops to exchange passengers must stand at a track with a platform.
/// See <c>ValidationExtensions.CheckPassengerExchange</c>.
/// </summary>
[TestClass]
public class PassengerExchangeValidationTests
{
    private static readonly TrainCategory Passenger = new() { Id = 1, Name = "Passenger", Prefix = "P", IsPassenger = true };
    private static readonly TrainCategory Freight = new() { Id = 2, Name = "Freight", Prefix = "G", IsFreight = true };

    [TestMethod]
    public void AStopAtATrackWithNoPlatformIsReported()
    {
        var station = StationWithTracks(1, "Alpha", "A", 2);
        var train = TrainStoppingAt(Passenger, station);

        var errors = train.CheckPassengerExchange().ToList();

        Assert.HasCount(1, errors);
        Assert.AreEqual(ValidationErrorType.PassengerExchangeWithoutPlatform, errors[0].ErrorType);
        Assert.IsTrue(errors[0].Involves(train));
        Assert.IsTrue(errors[0].Involves(station.Tracks.First()));
        Assert.IsFalse(string.IsNullOrWhiteSpace(errors[0].Message.Text));
    }

    [TestMethod]
    public void AStopAtATrackWithAPlatformIsNotReported()
    {
        var station = StationWithTracks(1, "Alpha", "A", 2);
        var train = TrainStoppingAt(Passenger, station);
        train.Calls[0].Track.PlatformLength = 4.5;

        Assert.IsEmpty(train.CheckPassengerExchange());
    }

    [TestMethod]
    public void ATrainMerelyStandingAtAPlatformlessTrackIsNotReported()
    {
        // The meet case: the train has a stop time but the call is neither an arrival nor a departure,
        // so nobody is getting on or off and no platform is wanted.
        var station = StationWithTracks(1, "Alpha", "A", 2);
        var train = TrainStoppingAt(Passenger, station);
        var call = train.Calls[0];
        call.IsArrival = false;
        call.IsDeparture = false;

        Assert.IsEmpty(train.CheckPassengerExchange());
    }

    [TestMethod]
    public void AnArrivalAloneAndADepartureAloneAreBothReported()
    {
        foreach (var (isArrival, isDeparture) in new[] { (true, false), (false, true) })
        {
            var station = StationWithTracks(1, "Alpha", "A", 1);
            var train = TrainStoppingAt(Passenger, station);
            var call = train.Calls[0];
            call.IsArrival = isArrival;
            call.IsDeparture = isDeparture;

            Assert.HasCount(1, train.CheckPassengerExchange().ToList(),
                $"IsArrival={isArrival}, IsDeparture={isDeparture} is a stop and needs a platform.");
        }
    }

    [TestMethod]
    public void AStopWhereNoPassengersAreExchangedIsNotReported()
    {
        var station = StationWithTracks(1, "Alpha", "A", 2);
        station.HasPassengerExchange = false;
        var train = TrainStoppingAt(Passenger, station);

        Assert.IsEmpty(train.CheckPassengerExchange());
    }

    [TestMethod]
    public void ATrainThatCarriesNoPassengersIsNotReported()
    {
        var station = StationWithTracks(1, "Alpha", "A", 2);
        var train = TrainStoppingAt(Freight, station);

        Assert.IsEmpty(train.CheckPassengerExchange());
    }

    [TestMethod]
    public void WhereOnlyOneTrackHasAPlatformOnlyTheTrainWithoutItIsReported()
    {
        // The small-station arrangement: two trains meet, one gets the platform and the other cannot.
        var station = StationWithTracks(1, "Alpha", "A", 2);
        var platform = station.Tracks.First();
        platform.PlatformLength = 4.5;
        var atPlatform = TrainStoppingAt(Passenger, station, platform);
        var atOtherTrack = TrainStoppingAt(Passenger, station, station.Tracks.Last(), id: 2, number: 1002);

        Assert.IsEmpty(atPlatform.CheckPassengerExchange());
        Assert.HasCount(1, atOtherTrack.CheckPassengerExchange().ToList());
    }

    [TestMethod]
    public void TheRuleCanBeSwitchedOff()
    {
        var station = StationWithTracks(1, "Alpha", "A", 1);
        var train = TrainStoppingAt(Passenger, station);

        Assert.HasCount(1, train.GetValidationErrors(new ValidationSettings()).ToList());
        Assert.IsEmpty(train.GetValidationErrors(new ValidationSettings { ValidatePassengerExchange = false }));
    }

    private static Station StationWithTracks(int id, string name, string signature, int trackCount)
    {
        var station = new Station(id, name, signature) { IsManned = true };
        for (var i = 0; i < trackCount; i++)
            station.Add(new StationTrack(id * 10 + i + 1, (i + 1).ToString(), isMain: i == 0, isScheduled: true));
        return station;
    }

    // A train with a single call at the given track, stopping there. The call's flags are set after it
    // joins the train, since Train.Add makes the first call a departure only.
    private static Train TrainStoppingAt(TrainCategory category, Station station, StationTrack? track = null, int id = 1, int number = 1001)
    {
        var train = new Train(id, number) { Category = category, CategoryId = category.Id };
        var call = train.Add(new StationCall(id, track ?? station.Tracks.First(), Time.FromHourAndMinute(8, 0), Time.FromHourAndMinute(8, 2)));
        call.IsArrival = true;
        call.IsDeparture = true;
        return train;
    }
}
