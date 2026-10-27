using System.Globalization;
using Tellurian.Trains.Schedules.Model.Settings;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a wagonset running with its train has its remark told to the station at every call the train
/// makes on the way, though nothing is coupled or uncoupled, and that the loco driver is not told.
/// </summary>
[TestClass]
public class WagonsetRemarkNoteTests
{
    private static readonly SessionsSettings Settings = SessionsSettings.UseSessions(14);

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static (Plan Plan, Train Train) Arrange(string? remark, ScheduledObjectType? tractionType = null, bool reversible = false)
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var train = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        timetable.Add(train);
        var plan = Plan.Create("Test", timetable);
        var schedule = plan.CreateSchedule();
        schedule.Add(train.AsTrainPart);
        var wagonset = plan.CreateVehicle(ScheduledObjectType.Wagonset, "Gb", 7, null);
        wagonset.Remark = remark;
        plan.AssignVehicle(schedule, wagonset, Sessions.All);
        if (tractionType is { } type)
        {
            var traction = plan.CreateVehicle(type, "T", 1, null);
            traction.IsReversibleTrain = reversible;
            plan.AssignVehicle(schedule, traction, Sessions.All);
        }
        return (plan, train);
    }

    [TestMethod]
    public void TheStationIsToldTheRemarkAtEveryCallOfTheTrain()
    {
        var (plan, train) = Arrange("Dobbeltdækkere");

        foreach (var call in train.CallsInRunOrder)
        {
            var note = call.StationNotes(train.Sessions, Settings, plan).OfType<WagonsetRemarkNote>().Single();
            StringAssert.Contains(note.ToText, "Dobbeltdækkere");
            Assert.IsTrue(note.IsForArrival && note.IsForDeparture);
        }
    }

    [TestMethod]
    public void TheLocoDriverIsNotToldTheRemark()
    {
        var (plan, train) = Arrange("Dobbeltdækkere");

        foreach (var call in train.CallsInRunOrder)
            Assert.IsEmpty(call.DriverNotes(train.Sessions, Settings, plan).OfType<WagonsetRemarkNote>());
    }

    [TestMethod]
    public void AWagonsetWithoutARemarkGetsNoNote()
    {
        var (plan, train) = Arrange(null);

        foreach (var call in train.CallsInRunOrder)
            Assert.IsEmpty(call.StationNotes(train.Sessions, Settings, plan).OfType<WagonsetRemarkNote>());
    }

    [TestMethod]
    [DataRow(ScheduledObjectType.Trainset, false, false)]
    [DataRow(ScheduledObjectType.Locomotive, true, false)]
    [DataRow(ScheduledObjectType.Locomotive, false, true)]
    public void AWagonsetRunningAsAFixedUnitWithItsTractionGetsNoNote(ScheduledObjectType type, bool reversible, bool expected)
    {
        var (plan, train) = Arrange("Dobbeltdækkere", type, reversible);

        foreach (var call in train.CallsInRunOrder)
            Assert.AreEqual(expected, call.StationNotes(train.Sessions, Settings, plan).OfType<WagonsetRemarkNote>().Any());
    }
}
