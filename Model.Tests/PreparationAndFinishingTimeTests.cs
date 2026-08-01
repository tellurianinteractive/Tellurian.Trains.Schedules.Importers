using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// A train's preparation time — the gap between its origin call's arrival and its departure — and its
/// finishing-up time at the destination are real pieces of work, not idle minutes. They belong to the
/// span the vehicles and the driver are tied up, and so count wherever overlap is judged.
/// </summary>
[TestClass]
public class PreparationAndFinishingTimeTests
{
    private static readonly ValidationSettings Settings = new();
    private static readonly TrainCategory Category = new() { Id = 1, Name = "P", Prefix = "P" };

    [TestInitialize]
    public void TestInitialize() => TestDataFactory.Init();

    // G -> Yb -> Snu: departs G at startTime, arrives Snu 55 minutes later. The train is made ready
    // preparationMinutes before it departs and put away finishingMinutes after it has arrived.
    private static Train ForwardTrain(int number, Time startTime, int preparationMinutes = 0, int finishingMinutes = 0)
    {
        var stations = TestDataFactory.Stations.ToArray();
        var train = new Train(number, Category, number) { Category = Category };
        _ = train.Add(new StationCall(1, stations[0]["3"], startTime.AddMinutes(-preparationMinutes), startTime));
        _ = train.Add(new StationCall(2, stations[1]["2"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
        _ = train.Add(new StationCall(3, stations[2]["1"], startTime.AddMinutes(55), startTime.AddMinutes(55 + finishingMinutes)));
        return train;
    }

    // Snu -> Yb -> G, the return working of the same route.
    private static Train ReturnTrain(int number, Time startTime, int preparationMinutes = 0, int finishingMinutes = 0)
    {
        var stations = TestDataFactory.Stations.ToArray();
        var train = new Train(number, Category, number) { Category = Category };
        _ = train.Add(new StationCall(1, stations[2]["2"], startTime.AddMinutes(-preparationMinutes), startTime));
        _ = train.Add(new StationCall(2, stations[1]["1"], startTime.AddMinutes(25), startTime.AddMinutes(30)));
        _ = train.Add(new StationCall(3, stations[0]["3"], startTime.AddMinutes(55), startTime.AddMinutes(55 + finishingMinutes)));
        return train;
    }

    // Forward 12:00 G->Snu (12:55) put away by 13:10, return prepared from 12:55 and departing Snu 13:05.
    // The two runs never move at the same time, but the finishing-up and the preparation do overlap.
    private static Plan PlanWhereFinishingRunsIntoPreparation()
    {
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(ForwardTrain(1, Time.FromHourAndMinute(12, 00), finishingMinutes: 15));
        timetable.Add(ReturnTrain(2, Time.FromHourAndMinute(13, 05), preparationMinutes: 10));
        return Plan.Create("Test", timetable);
    }

    private static ScheduledTrainPart AddLocoWorking(Plan plan, int trainNumber, int locoNumber)
    {
        var schedule = plan.CreateSchedule();
        var part = schedule.Add(plan.Timetable.Trains.First(t => t.Number == trainNumber).AsTrainPart);
        plan.AssignVehicle(schedule, plan.CreateVehicle(ScheduledObjectType.Locomotive, "L", locoNumber, null));
        return part;
    }

    [TestMethod]
    public void WorkingSpanRunsFromThePreparationToTheFinishingTime()
    {
        var train = ForwardTrain(1, Time.FromHourAndMinute(12, 00), preparationMinutes: 20, finishingMinutes: 15);

        var span = train.AsTrainPart.WorkingSpan;

        Assert.AreEqual(Time.FromHourAndMinute(11, 40), span.From, "Preparation starts 20 minutes before the 12:00 departure.");
        Assert.AreEqual(Time.FromHourAndMinute(13, 10), span.To, "Finishing up ends 15 minutes after the 12:55 arrival.");
    }

    [TestMethod]
    public void AnIntermediateCallIsNotExtended()
    {
        var train = ForwardTrain(1, Time.FromHourAndMinute(12, 00), preparationMinutes: 20, finishingMinutes: 15);

        var span = train.AsTrainPart(0, 1).WorkingSpan; // G (origin) -> Yb (a stop on the way)

        Assert.AreEqual(Time.FromHourAndMinute(11, 40), span.From, "The origin's preparation time counts.");
        Assert.AreEqual(Time.FromHourAndMinute(12, 25), span.To, "Ytterby is a stop on the way, so the part ends on arrival there.");
    }

    [TestMethod]
    public void ConsecutivePartsOfTheSameTrainDoNotOverlap()
    {
        var train = ForwardTrain(1, Time.FromHourAndMinute(12, 00), preparationMinutes: 20, finishingMinutes: 15);
        var first = train.AsTrainPart(0, 1);
        var second = train.AsTrainPart(1, 2);

        Assert.IsFalse(second.IsOverlapping([first]), "The handover at Ytterby is not an overlap.");
    }

    [TestMethod]
    public void AppendIsRejectedWhenTheNextTrainIsPreparedBeforeThisOneIsPutAway()
    {
        var forward = ForwardTrain(1, Time.FromHourAndMinute(12, 00), finishingMinutes: 15); // put away by 13:10
        var @return = ReturnTrain(2, Time.FromHourAndMinute(13, 05), preparationMinutes: 10); // made ready from 12:55
        var schedule = new Schedule(1);
        schedule.Append(forward.AsTrainPart);

        var result = schedule.Append(@return.AsTrainPart);

        Assert.IsTrue(result.IsNone, "The loco cannot be made ready for the return while it is still being put away.");
        Assert.HasCount(1, schedule.Parts);
    }

    [TestMethod]
    public void AppendIsAcceptedWhenPreparationStartsWhereFinishingUpEnds()
    {
        var forward = ForwardTrain(1, Time.FromHourAndMinute(12, 00), finishingMinutes: 15); // put away by 13:10
        var @return = ReturnTrain(2, Time.FromHourAndMinute(13, 20), preparationMinutes: 10); // made ready from 13:10
        var schedule = new Schedule(1);
        schedule.Append(forward.AsTrainPart);

        var result = schedule.Append(@return.AsTrainPart);

        Assert.IsTrue(result.HasValue, "One piece of work ending exactly where the next begins is a handover, not a clash.");
        Assert.HasCount(2, schedule.Parts);
    }

    [TestMethod]
    public void ScheduleOverlappingOnlyInPreparationAndFinishingTimeIsReported()
    {
        var plan = PlanWhereFinishingRunsIntoPreparation();
        var schedule = plan.CreateSchedule();
        schedule.Add(plan.Timetable.Trains.First(t => t.Number == 1).AsTrainPart);
        schedule.Add(plan.Timetable.Trains.First(t => t.Number == 2).AsTrainPart);

        var errors = plan.GetValidationErrors(Settings);

        Assert.IsNotEmpty(errors.Where(e => e.ErrorType == ValidationErrorType.VehicleScheduleOverlap));
    }

    [TestMethod]
    public void OverlapMessageShowsThePreparationAndFinishingTimes()
    {
        var plan = PlanWhereFinishingRunsIntoPreparation();
        var schedule = plan.CreateSchedule();
        schedule.Add(plan.Timetable.Trains.First(t => t.Number == 1).AsTrainPart);
        schedule.Add(plan.Timetable.Trains.First(t => t.Number == 2).AsTrainPart);

        var error = plan.GetValidationErrors(Settings).First(e => e.ErrorType == ValidationErrorType.VehicleScheduleOverlap);

        var text = error.Message.Text;
        StringAssert.Contains(text, "13:10", "Train 1 is not put away until 13:10, which is where the overlap lies.");
        StringAssert.Contains(text, "12:55", "Train 2 is made ready from 12:55, which is where the overlap lies.");
        Assert.DoesNotContain("13:05", text, "Train 2's departure says nothing about the overlap and is not shown.");
    }

    [TestMethod]
    public void DriverCannotTakeAPartPreparedWhileStillFinishingTheLastOne()
    {
        var plan = PlanWhereFinishingRunsIntoPreparation();
        var forwardPart = AddLocoWorking(plan, trainNumber: 1, locoNumber: 1);
        var returnPart = AddLocoWorking(plan, trainNumber: 2, locoNumber: 2);
        var duty = plan.CreateDriverDuty();
        duty.Append(forwardPart);

        var result = duty.Append(returnPart);

        Assert.IsTrue(result.IsNone, "Putting train 1 away until 13:10 and preparing train 2 from 12:55 is one driver in two places.");
        Assert.HasCount(1, duty.Parts);
    }
}
