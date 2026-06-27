namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class PlanFactoryTests
{
    [TestMethod]
    public void CreatePlanSeedsTheSessionCatalogue()
    {
        var plan = PlanFactory.CreatePlan("New", "en");
        Assert.HasCount(6, plan.Timetable.Sessions);
    }

    [TestMethod]
    public void CreatePlanSeedsTheStandardTrainCategories()
    {
        var plan = PlanFactory.CreatePlan("New", "en");
        Assert.HasCount(2, plan.Timetable.TrainCategories);
    }

    [TestMethod]
    public void CreatePlanDerivesTheDefaultCountryFromTheLanguage()
    {
        var plan = PlanFactory.CreatePlan("New", "sv");
        var layout = plan.Timetable.Layout;
        Assert.AreEqual(Country.ByLanguage("sv").Id, layout.Settings.Identity.DefaultCountryId);
        Assert.IsTrue(layout.Countries.Any(c => c.Id == layout.Settings.Identity.DefaultCountryId),
            "The default country is added to the layout's country catalogue.");
    }

    [TestMethod]
    public void CreatePlanAppliesNewLayoutSettingDefaults()
    {
        var plan = PlanFactory.CreatePlan("New", "en");
        var settings = plan.Timetable.Layout.Settings;
        Assert.AreEqual(TimeSpan.FromHours(18), settings.General.EndTime);
        Assert.AreEqual(2, settings.TimeAndSpeed.StationTimings.MinimumStopMinutes);
    }
}
