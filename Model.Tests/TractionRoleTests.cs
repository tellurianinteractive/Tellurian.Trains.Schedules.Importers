using System.Globalization;
using Tellurian.Trains.Schedules.Model.Notes;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a second locomotive can work a train in addition to the first, in the role its assignment
/// gives it: what that decides about running round, and that a locomotive at each end is said at departure.
/// </summary>
[TestClass]
public class TractionRoleTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static (Plan Plan, ScheduledTrainPart Part, ScheduledObject First, ScheduledObject Second) Arrange(
        bool firstPushPull, bool secondPushPull, TractionRole secondRole)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        var plan = Plan.Create("Test", timetable);
        var schedule = plan.CreateSchedule();
        var part = schedule.Append(plan.Timetable.Trains.First().AsTrainPart).Value;
        part.TractionOptions = new TractionOptions();

        var first = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Rc6", 4, null);
        first.IsReversibleTrain = firstPushPull;
        var second = plan.CreateVehicle(ScheduledObjectType.Locomotive, "Rc6", 5, null);
        second.IsReversibleTrain = secondPushPull;
        plan.AssignVehicle(schedule, first);
        plan.AssignVehicle(schedule, second, null, secondRole);
        return (plan, part, first, second);
    }

    [TestMethod]
    public void ALocomotiveIsAtTheFrontUnlessSaidOtherwise()
    {
        var (_, _, first, _) = Arrange(false, false, TractionRole.Back);

        Assert.AreEqual(TractionRole.Front, first.ScheduleAssignments.Single().Role);
    }

    [TestMethod]
    public void ALocomotiveAtTheBackSparesTheRunaround()
    {
        var (_, part, _, _) = Arrange(false, false, TractionRole.Back);

        Assert.IsFalse(part.NeedsRunaround);
    }

    [TestMethod]
    public void AReinforcingLocomotiveThatIsPushPullSparesTheRunaroundOfTheTrain()
    {
        var (_, part, _, _) = Arrange(false, true, TractionRole.Reinforcement);

        Assert.IsFalse(part.NeedsRunaround);
    }

    [TestMethod]
    public void TwoLocomotivesThatAreNeitherPushPullStillNeedTheRunaround()
    {
        var (_, part, _, _) = Arrange(false, false, TractionRole.Reinforcement);

        Assert.IsTrue(part.NeedsRunaround);
    }

    [TestMethod]
    public void ALocomotiveAtEachEndIsSaidAtDeparture()
    {
        var (plan, part, first, second) = Arrange(true, true, TractionRole.Back);

        var note = part.From.DriverNotes(Sessions.All, Settings, plan).OfType<LocomotiveAtEachEndNote>().Single();

        CollectionAssert.AreEqual(new[] { first }, note.Front.ToArray());
        CollectionAssert.AreEqual(new[] { second }, note.Back.ToArray());
        StringAssert.Contains(note.ToText, first.Designation);
        StringAssert.Contains(note.ToText, second.Designation);
    }

    [TestMethod]
    public void NoLocomotiveAtEachEndIsSaidWhenBothStandTogether()
    {
        var (plan, part, _, _) = Arrange(true, true, TractionRole.Reinforcement);

        Assert.IsFalse(part.From.DriverNotes(Sessions.All, Settings, plan).OfType<LocomotiveAtEachEndNote>().Any());
    }

    [TestMethod]
    public void TheTrainKnowsItsLocomotiveAtTheBack()
    {
        var (plan, part, _, second) = Arrange(true, true, TractionRole.Back);

        CollectionAssert.AreEqual(new[] { second }, plan.BackTractionOf(part.Train).ToArray());
    }

    private static IEnumerable<ValidationError> Roles(Plan plan) =>
        plan.GetValidationErrors(new ValidationSettings()).Where(e => e.ErrorType == ValidationErrorType.BackLocomotiveWithoutFront);

    [TestMethod]
    public void ABackLocomotiveWithAFrontOneIsValid()
    {
        var (plan, _, _, _) = Arrange(true, true, TractionRole.Back);

        Assert.IsFalse(Roles(plan).Any());
    }

    [TestMethod]
    public void ABackLocomotiveWithoutAFrontOneIsReported()
    {
        var (plan, _, first, _) = Arrange(true, true, TractionRole.Back);
        first.ScheduleAssignments.Single().Role = TractionRole.Reinforcement;

        var error = Roles(plan).Single();

        Assert.AreEqual(ValidationErrorType.BackLocomotiveWithoutFront, error.ErrorType);
    }

    [TestMethod]
    public void ABackLocomotiveIsReportedOnTheSessionsTheFrontOneDoesNotWork()
    {
        var (plan, _, first, _) = Arrange(true, true, TractionRole.Back);
        first.ScheduleAssignments.Single().Sessions = Sessions.FromSessionNumbers(1, 2, 3, 4, 5, 6, 7);

        var error = Roles(plan).Single();

        StringAssert.Contains(error.Message.Text, "8-14");
    }
}
