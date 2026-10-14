namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class StartLabelTests
{
    // Forward 12:00 G->Snu, return 13:00 Snu->G, both on one locomotive schedule.
    private static (Plan Plan, Train Forward, Train Return, TrainCategory Category) CreatePlan()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        var forward = TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00));
        var back = TestDataFactory.CreateTrainInOppositeDirection(category, 2, Time.FromHourAndMinute(13, 00));
        timetable.Add(forward);
        timetable.Add(back);
        var plan = Plan.Create("Test", timetable);
        var schedule = plan.CreateSchedule();
        schedule.Append(forward.AsTrainPart);
        schedule.Append(back.AsTrainPart);
        var loco = plan.CreateVehicle(ScheduledObjectType.Locomotive, "BR 218", 1, null);
        plan.AssignVehicle(schedule, loco);
        return (plan, forward, back, category);
    }

    private static void Allow(Plan plan, Train train, StartLabelPrinting category, StartLabelPrinting station)
    {
        train.Category!.StartLabels = category;
        foreach (var call in train.Calls)
            if (call.OperationLocation is Station s) s.StartLabels = station;
    }

    [TestMethod]
    public void NoLabelsByDefault()
    {
        var (plan, _, _, _) = CreatePlan();
        Assert.IsEmpty(plan.StartLabels());
    }

    [TestMethod]
    public void AllGivesEveryTrainALabelAndNamesWhatTheUnitArrivedAs()
    {
        var (plan, forward, back, _) = CreatePlan();
        Allow(plan, forward, StartLabelPrinting.All, StartLabelPrinting.All);

        var labels = plan.StartLabels();

        Assert.HasCount(2, labels);
        Assert.IsEmpty(labels.Single(l => l.Train == forward).ArrivedAs);
        Assert.AreEqual(forward, labels.Single(l => l.Train == back).ArrivedAs.Single());
    }

    [TestMethod]
    public void FirstGivesOnlyTheTrainThatStartsTheUnitsDay()
    {
        var (plan, forward, _, _) = CreatePlan();
        Allow(plan, forward, StartLabelPrinting.First, StartLabelPrinting.All);

        Assert.AreEqual(forward, plan.StartLabels().Single().Train);
    }

    [TestMethod]
    public void TheNarrowerOfCategoryAndStationDecides()
    {
        var (plan, forward, _, _) = CreatePlan();
        Allow(plan, forward, StartLabelPrinting.All, StartLabelPrinting.None);
        Assert.IsEmpty(plan.StartLabels(), "None at the station beats All.");

        Allow(plan, forward, StartLabelPrinting.None, StartLabelPrinting.All);
        Assert.IsEmpty(plan.StartLabels(), "None on the category beats All.");

        Allow(plan, forward, StartLabelPrinting.First, StartLabelPrinting.All);
        Assert.HasCount(1, plan.StartLabels(), "First beats All.");
    }

    [TestMethod]
    public void NamesTheDutiesWorkingTheTrain()
    {
        var (plan, forward, _, _) = CreatePlan();
        Allow(plan, forward, StartLabelPrinting.First, StartLabelPrinting.First);
        var duty = plan.CreateDriverDuty();
        duty.Add(plan.Schedules.Single().Parts.First(p => p.Train == forward));

        Assert.AreEqual(duty.Identity, plan.StartLabels().Single().Duties.Single());
    }
}
