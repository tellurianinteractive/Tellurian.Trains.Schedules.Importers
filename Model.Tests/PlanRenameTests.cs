namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// The layout, its timetable and the plan carry one name between them — the one edited as the layout
/// name in the settings. These cover renaming it, and the plans saved before renaming reached all three.
/// </summary>
[TestClass]
public class PlanRenameTests
{
    [TestMethod]
    public void RenameRenamesThePlanTheTimetableAndTheLayout()
    {
        var plan = PlanFactory.CreatePlan("New layout", "en");

        plan.Rename("Grimslöv H0");

        Assert.AreEqual("Grimslöv H0", plan.Name);
        Assert.AreEqual("Grimslöv H0", plan.Timetable.Name);
        Assert.AreEqual("Grimslöv H0", plan.Timetable.Layout.Name);
    }

    [TestMethod]
    public void ReconcileTakesTheNameFromTheLayout()
    {
        // A plan renamed by an earlier version, which set the layout's name and nothing else.
        var plan = PlanFactory.CreatePlan("New layout", "en");
        plan.Timetable.Layout.Name = "Grimslöv H0";

        plan.Reconcile();

        Assert.AreEqual("Grimslöv H0", plan.Name);
        Assert.AreEqual("Grimslöv H0", plan.Timetable.Name);
    }

    [TestMethod]
    public void ReconcileKeepsTheNameOfAnUnnamedLayout()
    {
        var plan = PlanFactory.CreatePlan("New layout", "en");
        plan.Timetable.Layout.Name = "";

        plan.Reconcile();

        Assert.AreEqual("New layout", plan.Name);
    }
}
