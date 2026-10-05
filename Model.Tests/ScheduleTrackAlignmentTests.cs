namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a working whose traction cannot leave the track it arrived at — a trainset, or a
/// locomotive on a reversible train — has its arrival tracks put where its next train departs from, and
/// that a working whose locomotive can run light across the station is left as the planner set it unless
/// the planner asks for it to be aligned.
/// </summary>
[TestClass]
public class ScheduleTrackAlignmentTests
{
    // Forward 12:00 G "3" → Yb "2" → Snu "1" 12:55 and return 13:00 Snu "2" → Yb "1" → G "1" 13:55: the
    // return continues the forward run, but from another track than it arrives at, and gets back to G on
    // another track than the forward run left from. Both joints therefore need moving.
    private static Plan CreatePlan()
    {
        TestDataFactory.Init();
        var stations = TestDataFactory.Stations.ToArray();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());

        var forward = new Train(1, category, 1) { Category = category, Sessions = Sessions.All };
        _ = forward.Add(new StationCall(1, stations[0]["3"], Time.FromHourAndMinute(12, 00), Time.FromHourAndMinute(12, 00)));
        _ = forward.Add(new StationCall(2, stations[1]["2"], Time.FromHourAndMinute(12, 25), Time.FromHourAndMinute(12, 30)));
        _ = forward.Add(new StationCall(3, stations[2]["1"], Time.FromHourAndMinute(12, 55), Time.FromHourAndMinute(12, 55)));
        timetable.Add(forward);

        var back = new Train(2, category, 2) { Category = category, Sessions = Sessions.All };
        _ = back.Add(new StationCall(4, stations[2]["2"], Time.FromHourAndMinute(13, 00), Time.FromHourAndMinute(13, 00)));
        _ = back.Add(new StationCall(5, stations[1]["1"], Time.FromHourAndMinute(13, 25), Time.FromHourAndMinute(13, 30)));
        _ = back.Add(new StationCall(6, stations[0]["1"], Time.FromHourAndMinute(13, 55), Time.FromHourAndMinute(13, 55)));
        timetable.Add(back);

        return Plan.Create("Test", timetable);
    }

    // The plan above with one vehicle of the given kind working a schedule already in it, so that the
    // automatic build extends that schedule rather than starting one of its own.
    private static (Plan Plan, Schedule Schedule) Arrange(ScheduledObjectType vehicleType, bool reversibleTrain = false)
    {
        var plan = CreatePlan();
        var schedule = plan.CreateSchedule();
        schedule.Add(Forward(plan).AsTrainPart);
        var vehicle = plan.CreateVehicle(vehicleType, "T44", 42, null);
        vehicle.IsReversibleTrain = reversibleTrain;
        plan.AssignVehicle(schedule, vehicle);
        return (plan, schedule);
    }

    private static Train Forward(Plan plan) => plan.Timetable.Trains.Single(t => t.Number == 1);
    private static Train Back(Plan plan) => plan.Timetable.Trains.Single(t => t.Number == 2);

    private static StationCall LastCall(Train train) => train.CallsInRunOrder[^1];

    [TestMethod]
    public void ATrainsetArrivesAtTheTrackItsNextTrainDepartsFrom()
    {
        var (plan, _) = Arrange(ScheduledObjectType.Trainset);

        plan.BuildSchedulesAutomatically();

        Assert.AreEqual("2", LastCall(Forward(plan)).Track.Number,
            "The trainset leaves Snu on the track it came in on, so the forward run must arrive at the return train's departure track.");
    }

    [TestMethod]
    public void TheLastTrainArrivesAtTheFirstTrainsDepartureTrackWhereTheWorkingClosesOnItself()
    {
        var (plan, _) = Arrange(ScheduledObjectType.Trainset);

        plan.BuildSchedulesAutomatically();

        Assert.AreEqual("3", LastCall(Back(plan)).Track.Number,
            "The working ends at G where it began, so the trainset is left standing where the first departure fetches it.");
    }

    [TestMethod]
    public void TheMovedCallLeavesTheTrackItStoodOnAndJoinsTheOneItMovesTo()
    {
        var (plan, _) = Arrange(ScheduledObjectType.Trainset);
        var snu = plan.Layout.OperationLocations.Single(l => l.Signature == "Snu");

        plan.BuildSchedulesAutomatically();

        var arrival = LastCall(Forward(plan));
        Assert.IsFalse(snu["1"].Calls.Contains(arrival), "The arrival no longer stands on the track it was moved off.");
        Assert.IsTrue(snu["2"].Calls.Contains(arrival), "The arrival is registered on the track it was moved to.");
    }

    [TestMethod]
    public void ALocomotiveWorkingLeavesTheArrivalTracksAlone()
    {
        var (plan, _) = Arrange(ScheduledObjectType.Locomotive);

        plan.BuildSchedulesAutomatically();

        Assert.AreEqual("1", LastCall(Forward(plan)).Track.Number,
            "The locomotive runs light to whatever track its next train stands on, so nothing has to move.");
        Assert.AreEqual("1", LastCall(Back(plan)).Track.Number);
    }

    [TestMethod]
    public void ALocomotiveOnAReversibleTrainArrivesAtTheNextTrainsDepartureTrack()
    {
        var (plan, _) = Arrange(ScheduledObjectType.Locomotive, reversibleTrain: true);

        plan.BuildSchedulesAutomatically();

        Assert.AreEqual("2", LastCall(Forward(plan)).Track.Number,
            "A reversible train is driven from its other end where it stands, so it leaves from the track it arrived at.");
    }

    [TestMethod]
    public void AWorkingNoVehicleFitsYetKeepsItsPlannedTracks()
    {
        var plan = CreatePlan();

        var built = plan.BuildSchedulesAutomatically();

        Assert.HasCount(1, built.Created);
        Assert.AreEqual("1", LastCall(Forward(plan)).Track.Number,
            "What traction the new working gets is still open, so the planner's tracks stand.");
        Assert.AreEqual("1", LastCall(Back(plan)).Track.Number);
    }

    [TestMethod]
    public void AssigningTheTrainsetAlignsAWorkingThatWasBuiltBeforeItWasKnown()
    {
        var plan = CreatePlan();
        var schedule = plan.BuildSchedulesAutomatically().Created.Single();

        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Trainset, "Y1", 1267, null));

        Assert.AreEqual("2", LastCall(Forward(plan)).Track.Number,
            "The trainset is what says the working cannot change tracks, so assigning it puts the tracks right.");
        Assert.AreEqual("3", LastCall(Back(plan)).Track.Number);
    }

    [TestMethod]
    public void TickingReversibleTrainOnTheAssignedLocomotiveAlignsTheWorking()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Locomotive);
        _ = schedule.Append(Back(plan).AsTrainPart);
        var locomotive = schedule.Vehicles.Single();

        plan.UpdateVehicle(locomotive, locomotive.ObjectType, locomotive.ExternalId, locomotive.Class, locomotive.Number,
            locomotive.Company, isReversibleTrain: true);

        Assert.AreEqual("2", LastCall(Forward(plan)).Track.Number,
            "The locomotive now changes cab instead of running round, so it leaves from the track it came in on.");
        Assert.AreEqual("3", LastCall(Back(plan)).Track.Number);
    }

    [TestMethod]
    public void EditingALocomotiveThatStillRunsRoundLeavesTheTracksAlone()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Locomotive);
        _ = schedule.Append(Back(plan).AsTrainPart);
        var locomotive = schedule.Vehicles.Single();

        plan.UpdateVehicle(locomotive, locomotive.ObjectType, locomotive.ExternalId, "T43", locomotive.Number, locomotive.Company);

        Assert.AreEqual("1", LastCall(Forward(plan)).Track.Number);
    }

    [TestMethod]
    public void AligningOnRequestMovesTheTracksOfALocomotiveWorking()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Locomotive);
        _ = schedule.Append(Back(plan).AsTrainPart);

        var moved = schedule.AlignArrivalTracksWhateverTheTraction();

        Assert.AreEqual(2, moved);
        Assert.AreEqual("2", LastCall(Forward(plan)).Track.Number,
            "The planner asked for it, so the locomotive stays with its train and the arrival is moved.");
        Assert.AreEqual("3", LastCall(Back(plan)).Track.Number);
    }

    [TestMethod]
    public void AWorkingThatEndsSomewhereElseThanItBeganIsNotClosed()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Trainset);
        // Only the forward run: the working ends at Snu, and nothing fetches the trainset back from G.
        schedule.AlignArrivalTracks();

        Assert.AreEqual("3", Forward(plan).CallsInRunOrder[0].Track.Number, "A departure track is never moved.");
        Assert.AreEqual("1", LastCall(Forward(plan)).Track.Number,
            "The working neither continues nor closes on itself, so its one arrival stands where it was planned.");
    }

    [TestMethod]
    public void AddingATrainByHandAlignsTheWorking()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Trainset);

        var added = schedule.Append(Back(plan).AsTrainPart);

        Assert.IsTrue(added.HasValue, added.Message);
        Assert.AreEqual("2", LastCall(Forward(plan)).Track.Number,
            "Adding the return run by hand makes the same joint the automatic build would, and the trainset cannot change tracks at it.");
        Assert.AreEqual("3", LastCall(Back(plan)).Track.Number);
    }

    [TestMethod]
    public void WorkingATrainIntoAJointAlignsTheWorking()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Trainset);

        var worked = schedule.Insert(Back(plan).AsTrainPart);

        Assert.IsTrue(worked.HasValue, worked.Message);
        Assert.AreEqual("2", LastCall(Forward(plan)).Track.Number);
        Assert.AreEqual("3", LastCall(Back(plan)).Track.Number);
    }

    [TestMethod]
    public void AddingATrainByHandToALocomotiveWorkingLeavesTheTracksAlone()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Locomotive);

        _ = schedule.Append(Back(plan).AsTrainPart);

        Assert.AreEqual("1", LastCall(Forward(plan)).Track.Number,
            "The locomotive runs light to the track its next train stands on, by hand as automatically.");
    }

    [TestMethod]
    public void AReconstructionKeepsTheTracksItWasReadWith()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Trainset);

        _ = schedule.Add(Back(plan).AsTrainPart);

        Assert.AreEqual("1", LastCall(Forward(plan)).Track.Number,
            "The unguarded add rebuilds a working from a trusted source, which is authoritative about its own tracks.");
    }

    [TestMethod]
    public void ABrokenJointMovesNoTrack()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Trainset);
        // A second forward run G "3" 14:00 → Snu "1" 14:55, worked into the layover: the trainset would have
        // to be at G to make it, so the joint at Snu is broken until the leg back is worked in as well.
        var stations = TestDataFactory.Stations.ToArray();
        var second = new Train(3, Forward(plan).Category!, 3) { Category = Forward(plan).Category, Sessions = Sessions.All };
        _ = second.Add(new StationCall(7, stations[0]["3"], Time.FromHourAndMinute(14, 00), Time.FromHourAndMinute(14, 00)));
        _ = second.Add(new StationCall(8, stations[2]["1"], Time.FromHourAndMinute(14, 55), Time.FromHourAndMinute(14, 55)));
        plan.Timetable.Add(second);

        var worked = schedule.Insert(second.AsTrainPart);

        Assert.IsTrue(worked.HasValue, worked.Message);
        Assert.AreEqual("1", LastCall(Forward(plan)).Track.Number,
            "The vehicle does not stand still between the two runs, so there is no track for them to agree on.");
        Assert.AreEqual("1", LastCall(second).Track.Number, "Nor does the working close on itself at G.");
    }

    [TestMethod]
    public void AligningTwiceMovesNothingTheSecondTime()
    {
        var (plan, schedule) = Arrange(ScheduledObjectType.Trainset);
        plan.BuildSchedulesAutomatically();

        Assert.AreEqual(0, schedule.AlignArrivalTracks(), "The tracks already agree, so there is nothing left to move.");
    }
}
