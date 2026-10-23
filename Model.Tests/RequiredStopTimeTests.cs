using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers the least time a train must stand at a stop: how a train clearance or loco runaround time is read
/// (below 10 minutes, from 10 seconds), how it becomes fast-clock time, that it raises the minimum stop
/// instead of adding to it, and when a shorter stop is reported.
/// </summary>
[TestClass]
public class RequiredStopTimeTests
{
    private static readonly TrainCategory Passenger = new() { Id = 1, Name = "Passenger", Prefix = "P", Content = TrainContent.Passenger };

    // Three stations in a row, the middle one the train stops at for the given minutes.
    private static Plan CreatePlan(int stopMinutes, bool middleIsManned = true)
    {
        var layout = new Layout { Id = 1, Name = "Test" };
        var timetable = new Timetable("Test", layout);
        timetable.TrainCategories.Add(Passenger);
        var train = new Train(1, Passenger, 100) { Sessions = Sessions.All };
        var time = Time.FromHourAndMinute(8, 0);
        var id = 1;
        foreach (var (name, signature) in new[] { ("Ytterby", "Yb"), ("Kode", "Kd"), ("Stenungsund", "Sts") })
        {
            var station = new Station(id, name, signature) { IsManned = signature == "Kd" && middleIsManned };
            var track = new StationTrack(id, "1");
            station.Add(track);
            layout.OperationLocations.Add(station);
            var stop = signature == "Kd" ? stopMinutes : 2;
            var call = new StationCall(id, track, time, time.AddMinutes(stop));
            train.Add(call);
            call.IsArrival = signature != "Yb";
            call.IsDeparture = signature != "Sts";
            time = time.AddMinutes(20);
            id++;
        }
        timetable.Add(train);
        return new Plan("Test", timetable);
    }

    private static StationTimings Timings(Plan plan) => plan.Layout.Settings.TimeAndSpeed.StationTimings;

    private static StationCall MiddleCall(Plan plan) => plan.Timetable.Trains.Single().CallsInRunOrder[1];

    private static List<ValidationError> TooShort(Plan plan, ValidationSettings? settings = null) =>
        [.. plan.GetValidationErrors(settings ?? new ValidationSettings())
            .Where(e => e.ErrorType == ValidationErrorType.StopShorterThanRequired)];

    // ---- Reading the value ----------------------------------------------------------------------

    [TestMethod]
    [DataRow(1, 60)]
    [DataRow(9, 540)]
    [DataRow(10, 10)]
    [DataRow(90, 90)]
    public void AValueBelowTenIsMinutesAndTenOrMoreIsSeconds(int value, int expectedSeconds)
    {
        Assert.AreEqual(TimeSpan.FromSeconds(expectedSeconds), StationTimings.RealDurationOf(value));
        Assert.AreEqual(value >= 10, StationTimings.IsSeconds(value));
    }

    [TestMethod]
    public void NothingEnteredIsNoTime()
    {
        Assert.AreEqual(TimeSpan.Zero, StationTimings.RealDurationOf(0));
        Assert.AreEqual(TimeSpan.Zero, StationTimings.RealDurationOf(-3));
    }

    [TestMethod]
    [DataRow(1, 5)]
    [DataRow(2, 10)]
    [DataRow(30, 3)]
    [DataRow(10, 1)]
    [DataRow(90, 8)]
    public void ARealDurationIsFastClockMinutesRoundedUp(int value, int expectedMinutes)
    {
        var settings = new TimeAndSpeedSettings { FastClockSpeed = 5 };

        Assert.AreEqual(expectedMinutes, settings.FastClockMinutesOf(value));
    }

    // ---- The least time a stop requires ---------------------------------------------------------

    [TestMethod]
    public void ClearanceAtAMannedStationRaisesTheMinimumStopInsteadOfAddingToIt()
    {
        var plan = CreatePlan(5);
        var location = MiddleCall(plan).OperationLocation;

        // Minimum stop 3, clearance 1 real minute at fast-clock speed 5.
        Assert.AreEqual(5, plan.Layout.Settings.TimeAndSpeed.RequiredStopMinutes(location, locoRunsRound: false));

        Timings(plan).TrainClearanceRealMinutes = 30;
        Assert.AreEqual(3, plan.Layout.Settings.TimeAndSpeed.RequiredStopMinutes(location, false), "30 seconds is 3 fast minutes, no more than the minimum stop.");
    }

    [TestMethod]
    public void AnUnmannedLocationNeedsNoClearance()
    {
        var plan = CreatePlan(3, middleIsManned: false);

        Assert.AreEqual(3, plan.Layout.Settings.TimeAndSpeed.RequiredStopMinutes(MiddleCall(plan).OperationLocation, false));
    }

    [TestMethod]
    public void TheLocoRunaroundIsTheLongestOfTheThreeWhenTheLocoIsRunRound()
    {
        var plan = CreatePlan(5);
        var location = MiddleCall(plan).OperationLocation;

        Assert.AreEqual(25, plan.Layout.Settings.TimeAndSpeed.RequiredStopMinutes(location, locoRunsRound: true));
    }

    [TestMethod]
    public void AStationsOwnTimeWinsOverTheLayoutDefault()
    {
        var plan = CreatePlan(5);
        var location = MiddleCall(plan).OperationLocation;
        location.Timings.TrainClearanceRealMinutes = 2;

        Assert.AreEqual(10, plan.Layout.Settings.TimeAndSpeed.RequiredStopMinutes(location, false));
    }

    // ---- Reporting a stop that is too short -----------------------------------------------------

    [TestMethod]
    public void AStopShorterThanTheClearanceIsReported()
    {
        var plan = CreatePlan(3);

        var errors = TooShort(plan);

        Assert.HasCount(1, errors);
        Assert.AreEqual(Severity.Warning, errors[0].Message.Severity);
        Assert.AreEqual(plan.Timetable.Trains.Single(), errors[0].Trains.Single());
    }

    [TestMethod]
    public void AStopAsLongAsTheRequirementIsNotReported()
    {
        Assert.IsEmpty(TooShort(CreatePlan(5)));
    }

    [TestMethod]
    public void AShorterStopIsPutRightByLengtheningItOrLoweringTheTimeRequired()
    {
        var plan = CreatePlan(3);
        Assert.HasCount(1, TooShort(plan), "Precondition.");

        Timings(plan).TrainClearanceRealMinutes = 30;
        Assert.IsEmpty(TooShort(plan), "Thirty seconds of clearance is three fast minutes.");

        Timings(plan).TrainClearanceRealMinutes = 1;
        var call = MiddleCall(plan);
        call.Departure = call.Arrival.AddMinutes(5);
        Assert.IsEmpty(TooShort(plan), "A longer stop.");
    }

    [TestMethod]
    public void TheCheckCanBeSwitchedOff()
    {
        Assert.IsEmpty(TooShort(CreatePlan(3), new ValidationSettings { ValidateRequiredStopTimes = false }));
    }

    [TestMethod]
    public void ARunaroundTheCallAsksForIsReportedWhenTheStopIsTooShortForIt()
    {
        var plan = CreatePlan(10);
        Assert.IsEmpty(TooShort(plan), "Precondition: long enough for the clearance.");

        MiddleCall(plan).Runaround = RunaroundMode.Always;

        Assert.HasCount(1, TooShort(plan), "The runaround takes 25 fast minutes.");
    }

    [TestMethod]
    public void TheEndsOfATrainAreNotChecked()
    {
        var plan = CreatePlan(5);
        var train = plan.Timetable.Trains.Single();
        train.CallsInRunOrder[0].Departure = train.CallsInRunOrder[0].Arrival;

        Assert.IsEmpty(TooShort(plan));
    }
}
