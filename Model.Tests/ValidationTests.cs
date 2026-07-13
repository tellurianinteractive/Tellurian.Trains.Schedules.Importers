using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class ValidationTests
{
    private static readonly ValidationSettings Settings = new();

    private static Timetable NewTimetable()
    {
        TestDataFactory.Init();
        return new Timetable("Test", TestDataFactory.Layout());
    }

    private static TrainCategory Category => new() { Id = 1, Name = "P", Prefix = "P" };

    // --- T4: trains sharing company, category and number must run on non-overlapping sessions ---

    [TestMethod]
    public void DuplicateTrainNumberOnOverlappingSessionsIsReported()
    {
        var timetable = NewTimetable();
        var category = Category;
        var a = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        a.Sessions = Sessions.FromSessionNumbers(1, 2, 3);
        var b = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(14, 00));
        b.Sessions = Sessions.FromSessionNumbers(3, 4, 5); // overlaps a on session 3
        // Added directly to bypass Timetable.Add's identity de-duplication, as an import can.
        timetable.Trains.Add(a);
        timetable.Trains.Add(b);
        var plan = Plan.Create("Test", timetable);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber)
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void DuplicateTrainNumberOnDisjointSessionsIsAllowed()
    {
        var timetable = NewTimetable();
        var category = Category;
        var a = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        a.Sessions = Sessions.FromSessionNumbers(1, 2, 3);
        var b = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(14, 00));
        b.Sessions = Sessions.FromSessionNumbers(4, 5, 6); // no session in common with a
        timetable.Trains.Add(a);
        timetable.Trains.Add(b);
        var plan = Plan.Create("Test", timetable);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void DistinctTrainNumbersAreNotFlaggedAsDuplicates()
    {
        var timetable = NewTimetable();
        var category = Category;
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00)));
        var plan = Plan.Create("Test", timetable);

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void TrainNumberValidationCanBeSwitchedOff()
    {
        var timetable = NewTimetable();
        var category = Category;
        var a = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        var b = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(14, 00));
        timetable.Trains.Add(a);
        timetable.Trains.Add(b);
        var plan = Plan.Create("Test", timetable);
        var settings = new ValidationSettings { ValidateTrainNumbers = false };

        var errors = plan.GetValidationErrors(settings)
            .Where(e => e.ErrorType == ValidationErrorType.DuplicateTrainNumber);

        Assert.IsEmpty(errors);
    }

    // --- S2: a schedule's parts must be geographically contiguous ---

    private static Plan PlanWithTwoForwardTrains()
    {
        var timetable = NewTimetable();
        var category = Category;
        // Two forward runs (G->Snu). Chaining one after the other leaves a gap: the vehicle would have
        // to jump from Snu back to G with no train.
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 3, Time.FromHourAndMinute(14, 00)));
        return Plan.Create("Test", timetable);
    }

    private static Plan PlanWithForwardAndReturn()
    {
        var timetable = NewTimetable();
        var category = Category;
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));  // G->Snu
        timetable.Add(TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00))); // Snu->G
        return Plan.Create("Test", timetable);
    }

    [TestMethod]
    public void NonContiguousScheduleIsReported()
    {
        var plan = PlanWithTwoForwardTrains();
        var first = plan.Timetable.Trains.First(t => t.Number == 1);
        var second = plan.Timetable.Trains.First(t => t.Number == 3);
        var schedule = plan.CreateSchedule();
        schedule.Add(first.AsTrainPart);  // ends at Snu
        schedule.Add(second.AsTrainPart); // starts at G, not Snu

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous)
            .ToList();

        Assert.HasCount(1, errors);
    }

    [TestMethod]
    public void ContiguousScheduleIsAllowed()
    {
        var plan = PlanWithForwardAndReturn();
        var forward = plan.Timetable.Trains.First(t => t.Number == 1);
        var @return = plan.Timetable.Trains.First(t => t.Number == 2);
        var schedule = plan.CreateSchedule();
        schedule.Add(forward.AsTrainPart);  // ends at Snu
        schedule.Add(@return.AsTrainPart);  // starts at Snu

        var errors = plan.GetValidationErrors(Settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous);

        Assert.IsEmpty(errors);
    }

    [TestMethod]
    public void ScheduleValidationCanBeSwitchedOff()
    {
        var plan = PlanWithTwoForwardTrains();
        var first = plan.Timetable.Trains.First(t => t.Number == 1);
        var second = plan.Timetable.Trains.First(t => t.Number == 3);
        var schedule = plan.CreateSchedule();
        schedule.Add(first.AsTrainPart);
        schedule.Add(second.AsTrainPart);
        var settings = new ValidationSettings { ValidateSchedules = false };

        var errors = plan.GetValidationErrors(settings)
            .Where(e => e.ErrorType == ValidationErrorType.ScheduleNotContiguous);

        Assert.IsEmpty(errors);
    }
}
