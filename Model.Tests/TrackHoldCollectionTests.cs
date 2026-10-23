using Tellurian.Trains.Schedules.Model.Schedules;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// A locomotive that arrives in one train and leaves in the next with the wagons standing on the track
/// has to come to them: its call is no double booking of the track the wagons hold.
/// </summary>
[TestClass]
public class TrackHoldCollectionTests
{
    private static readonly TrainCategory Category = new() { Id = 1, Name = "P", Prefix = "P" };

    private static Time At(int hour, int minute) => Time.FromHourAndMinute(hour, minute);

    private static Train TrainOf(int number, StationTrack from, StationTrack to, Time arrival, Time departure)
    {
        var train = new Train(number, Category, number) { Category = Category, Sessions = Sessions.All };
        _ = train.Add(new StationCall(1, from, arrival.AddMinutes(-20), arrival.AddMinutes(-10)));
        _ = train.Add(new StationCall(2, to, arrival, departure));
        return train;
    }

    private static Train AnotherTrain(StationTrack track, StationTrack other)
    {
        var train = new Train(4, Category, 4) { Category = Category, Sessions = Sessions.All };
        _ = train.Add(new StationCall(1, track, At(11, 30), At(11, 40)));
        _ = train.Add(new StationCall(2, other, At(12, 10), At(12, 20)));
        return train;
    }

    // Wagons arrive at 03:00 and leave at 12:00 in the second train. A locomotive arrives at 11:00 and
    // leaves either in that same train or in a train of its own, 11:30.
    private static (Plan Plan, StationTrack Track) Arrange(bool locoTakesTheWagonsTrain)
    {
        TestDataFactory.Init();
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var station = (Station)TestDataFactory.Stations.First();
        var track = station.Tracks.First();
        var other = station.Tracks.Last();

        var wagonsIn = TrainOf(1, other, track, At(3, 00), At(3, 10));
        var wagonsOut = new Train(2, Category, 2) { Category = Category, Sessions = Sessions.All };
        _ = wagonsOut.Add(new StationCall(1, track, At(11, 50), At(12, 00)));
        _ = wagonsOut.Add(new StationCall(2, other, At(12, 30), At(12, 40)));
        var locoIn = TrainOf(3, other, track, At(11, 00), At(11, 10));
        var locoOut = locoTakesTheWagonsTrain ? wagonsOut : AnotherTrain(track, other);
        foreach (var train in locoTakesTheWagonsTrain ? new[] { wagonsIn, wagonsOut, locoIn } : [wagonsIn, wagonsOut, locoIn, locoOut]) timetable.Add(train);
        var plan = Plan.Create("Test", timetable);

        var wagons = plan.CreateSchedule();
        _ = wagons.Append(wagonsIn.AsTrainPart);
        _ = wagons.Append(wagonsOut.AsTrainPart);
        plan.AssignVehicle(wagons, plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 11, null), Sessions.All);

        var loco = plan.CreateSchedule();
        _ = loco.Append(locoIn.AsTrainPart);
        _ = loco.Append(locoOut.AsTrainPart);
        plan.AssignVehicle(loco, plan.CreateVehicle(ScheduledObjectType.Locomotive, "T66", 5, null), Sessions.All);
        return (plan, track);
    }

    [TestMethod]
    public void ALocomotiveArrivingToTakeTheStandingWagonsOnDoesNotConflictWithThem()
    {
        var (plan, track) = Arrange(locoTakesTheWagonsTrain: true);

        var errors = track.GetValidationErrors(plan.Schedules, extendOccupancyByVehicleStay: true).ToList();
        Assert.IsEmpty(errors, string.Join("; ", errors.Select(e => e.Message.Text)));
    }

    [TestMethod]
    public void ALocomotiveLeavingInAnotherTrainStillConflictsWithTheStandingWagons()
    {
        var (plan, track) = Arrange(locoTakesTheWagonsTrain: false);

        Assert.IsNotEmpty(track.GetValidationErrors(plan.Schedules, extendOccupancyByVehicleStay: true));
    }
}
