using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers rule L5: vehicles with a length may share a track with a length, as long as they fit, and as
/// long as the one that has to leave first is not stood behind one that leaves later.
/// </summary>
[TestClass]
public class TrackCapacityTests
{
    private static readonly TrainCategory Category = new() { Id = 1, Name = "P", Prefix = "P" };

    private StationTrack Gothenburg = default!;
    private StationTrack Ytterby = default!;
    private StationTrack Stenungsund = default!;

    private static Time At(int hour, int minute) => Time.FromHourAndMinute(hour, minute);

    [TestInitialize]
    public void TestInitialize()
    {
        TestDataFactory.Init();
        var stations = TestDataFactory.Stations.Cast<Station>().ToArray();
        Gothenburg = stations[0].Tracks.First();
        Ytterby = stations[1].Tracks.First();
        Stenungsund = stations[2].Tracks.First();
        Ytterby.Length = 10;
    }

    // A train that calls at Ytterby between a location on one side and a location on the other.
    private Train Via(int number, StationTrack before, StationTrack after, Time arrival, Time departure, double? metres)
    {
        var train = new Train(number, Category, number) { Category = Category, Sessions = Sessions.All };
        if (metres is { } length) train.Length = TrainCapacity.MetersOnly(length);
        _ = train.Add(new StationCall(1, before, arrival.AddMinutes(-30), arrival.AddMinutes(-30)));
        _ = train.Add(new StationCall(2, Ytterby, arrival, departure));
        _ = train.Add(new StationCall(3, after, departure.AddMinutes(30), departure.AddMinutes(30)));
        return train;
    }

    private List<ValidationError> Errors(params Train[] _) => [.. Ytterby.GetValidationErrors([])];

    private static IEnumerable<ValidationError> OfType(IEnumerable<ValidationError> errors, ValidationErrorType type) =>
        errors.Where(e => e.ErrorType == type);

    [TestMethod]
    public void TwoTrainsThatFitTogetherMayShareATrack()
    {
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), 4);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(10, 40), 5);

        Assert.IsEmpty(Errors(), "4 m and 5 m on a 10 m track, the later one leaving first.");
    }

    [TestMethod]
    public void TwoTrainsLongerTogetherThanTheTrackAreReported()
    {
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), 6);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(10, 40), 5);

        var errors = Errors();

        Assert.HasCount(1, OfType(errors, ValidationErrorType.StationTrackTooShort));
        Assert.IsEmpty(OfType(errors, ValidationErrorType.StationTrackConflict), "Sharing is judged by length, not as double booking.");
    }

    [TestMethod]
    public void TwoTrainsAtTheSameTimeButNotTogetherAreFine()
    {
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(10, 30), 6);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 30), At(11, 00), 6);

        Assert.IsEmpty(Errors(), "One arrives as the other leaves.");
    }

    [TestMethod]
    public void TheOuterTrainLeavingBeforeTheInnerOneIsReported()
    {
        // Both come in from Gothenburg and go back there: 200 stands nearer the way out than 100.
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(10, 40), 4);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(11, 00), 4);

        var blocked = OfType(Errors(), ValidationErrorType.StationTrackBlocked).ToList();

        Assert.HasCount(1, blocked);
        CollectionAssert.AreEquivalent(new[] { 100, 200 }, blocked[0].Trains.Select(t => t.Number).ToArray());
    }

    [TestMethod]
    public void TheInnerTrainLeavingBeforeTheOuterOneIsFine()
    {
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), 4);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(10, 40), 4);

        Assert.IsEmpty(OfType(Errors(), ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void ATrainThatLeavesByTheOtherEndIsNotStoodInTheWay()
    {
        // 100 goes on to Stenungsund while 200, which came in behind it from Gothenburg, goes back.
        _ = Via(100, Gothenburg, Stenungsund, At(10, 00), At(10, 40), 4);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(11, 00), 4);

        Assert.IsEmpty(OfType(Errors(), ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void ATrainCannotLeaveByTheFarEndPastOneStillStanding()
    {
        // 200 came in behind 100 from Gothenburg, but goes on to Stenungsund and so has to pass it.
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), 4);
        _ = Via(200, Gothenburg, Stenungsund, At(10, 10), At(10, 40), 4);

        Assert.HasCount(1, OfType(Errors(), ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void TrainsFromOppositeEndsCannotPassEachOther()
    {
        _ = Via(100, Gothenburg, Stenungsund, At(10, 00), At(10, 50), 3);
        _ = Via(200, Stenungsund, Gothenburg, At(10, 10), At(10, 40), 3);

        Assert.IsNotEmpty(OfType(Errors(), ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void AStubTrackHasOneEndOnly()
    {
        // Arriving from Gothenburg and leaving towards Stenungsund is two ends on a through track, and
        // one on a stub, where the train that came in last has to leave first.
        _ = Via(100, Gothenburg, Stenungsund, At(10, 00), At(10, 40), 4);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(11, 00), 4);
        Assert.IsEmpty(OfType(Errors(), ValidationErrorType.StationTrackBlocked));

        Ytterby.IsStub = true;

        Assert.HasCount(1, OfType(Errors(), ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void AnEndThatIsNotKnownIsNotJudged()
    {
        // 100 begins its run at Ytterby, so nothing says which end it stands at.
        var begins = new Train(100, Category, 100) { Category = Category, Sessions = Sessions.All, Length = TrainCapacity.MetersOnly(4) };
        _ = begins.Add(new StationCall(1, Ytterby, At(10, 00), At(10, 40)));
        _ = begins.Add(new StationCall(2, Stenungsund, At(11, 00), At(11, 00)));
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(11, 00), 4);

        Assert.IsEmpty(OfType(Errors(), ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void WithoutLengthsTwoTrainsOnATrackAreStillADoubleBooking()
    {
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), null);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(10, 40), null);

        var errors = Errors();

        Assert.HasCount(1, OfType(errors, ValidationErrorType.StationTrackConflict));
        Assert.IsEmpty(OfType(errors, ValidationErrorType.StationTrackTooShort));
    }

    [TestMethod]
    public void WithoutATrackLengthTwoTrainsOnATrackAreStillADoubleBooking()
    {
        Ytterby.Length = 0;
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), 4);
        _ = Via(200, Gothenburg, Gothenburg, At(10, 10), At(10, 40), 4);

        Assert.HasCount(1, OfType(Errors(), ValidationErrorType.StationTrackConflict));
    }

    [TestMethod]
    public void ATrainLongerThanTheTrackIsReported()
    {
        _ = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), 12);

        Assert.HasCount(1, OfType(Errors(), ValidationErrorType.StationTrackTooShort));
    }

    [TestMethod]
    public void TheLengthOfATrainIsTheLengthOfItsVehicles()
    {
        var train = Via(100, Gothenburg, Gothenburg, At(10, 00), At(11, 00), 2);
        var plan = Plan.Create("Test", new Timetable("Test", TestDataFactory.Layout()));
        var schedule = plan.CreateSchedule();
        _ = schedule.Append(train.AsTrainPart);
        var wagons = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", 11, null);
        wagons.Length = 12;
        plan.AssignVehicle(schedule, wagons, Sessions.All);

        // 12 m of wagons, not the 2 m the train's own limit says, which would have fitted.
        Assert.HasCount(1, OfType(Ytterby.GetValidationErrors(plan.Schedules), ValidationErrorType.StationTrackTooShort));
    }

    // Two wagonsets, each brought to Ytterby by one train and taken away by another, standing in between.
    private (Plan Plan, ScheduledObject First, ScheduledObject Second) StandingWagonsets(Time firstLeaves, Time secondLeaves)
    {
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var plan = Plan.Create("Test", timetable);
        ScheduledObject Wagonset(int number, Time arrives, Time leaves)
        {
            var arriving = Via(number, Gothenburg, Gothenburg, arrives, arrives.AddMinutes(5), null);
            var leaving = Via(number + 1, Gothenburg, Gothenburg, leaves.AddMinutes(-5), leaves, null);
            timetable.Add(arriving);
            timetable.Add(leaving);
            var schedule = plan.CreateSchedule();
            _ = schedule.Append(Part(arriving, toYtterby: true));
            _ = schedule.Append(Part(leaving, toYtterby: false));
            var wagons = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gbs", number, null);
            wagons.Length = 4;
            plan.AssignVehicle(schedule, wagons, Sessions.All);
            return wagons;
        }
        var first = Wagonset(10, At(8, 00), firstLeaves);
        var second = Wagonset(20, At(9, 00), secondLeaves);
        return (plan, first, second);
    }

    // A train part that ends at Ytterby (the wagons are brought) or starts there (they are fetched).
    private static ScheduledTrainPart Part(Train train, bool toYtterby)
    {
        return toYtterby ? train.AsTrainPart(0, 1) : train.AsTrainPart(1, 2);
    }

    [TestMethod]
    public void WagonsetsPutOnATrackAreFetchedInTheOrderTheyStandIn()
    {
        // The second wagonset is put there last, and is fetched first.
        var (plan, _, _) = StandingWagonsets(firstLeaves: At(12, 00), secondLeaves: At(11, 00));

        var errors = Ytterby.GetValidationErrors(plan.Schedules, extendOccupancyByVehicleStay: true);

        Assert.IsEmpty(OfType(errors, ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void AWagonsetPutOnATrackBehindAnotherIsReportedWhenTheOneInFrontIsFetchedLater()
    {
        var (plan, _, _) = StandingWagonsets(firstLeaves: At(11, 00), secondLeaves: At(12, 00));

        var errors = Ytterby.GetValidationErrors(plan.Schedules, extendOccupancyByVehicleStay: true);

        Assert.IsNotEmpty(OfType(errors, ValidationErrorType.StationTrackBlocked));
    }

    [TestMethod]
    public void WagonsetsTooLongForTheTrackTogetherAreReported()
    {
        var (plan, first, _) = StandingWagonsets(firstLeaves: At(12, 00), secondLeaves: At(11, 00));
        first.Length = 7;

        var errors = Ytterby.GetValidationErrors(plan.Schedules, extendOccupancyByVehicleStay: true);

        Assert.IsNotEmpty(OfType(errors, ValidationErrorType.StationTrackTooShort));
    }
}
