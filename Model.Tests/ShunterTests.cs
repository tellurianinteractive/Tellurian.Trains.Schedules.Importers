using System.Text.Json;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies shunters: locomotives stationed at one operation location for any shunting there. They work no
/// schedule, yet are rolling stock a participant brings with a DCC address like any other locomotive, and they go
/// with the location they are stationed at.
/// </summary>
[TestClass]
public class ShunterTests
{
    private static readonly JsonSerializerOptions Options = PlanJson.CreateOptions();

    private static Plan CreatePlan()
    {
        TestDataFactory.Init();
        var category = new TrainCategory { Id = 1, Name = "P", Prefix = "P" };
        var timetable = new Timetable("Test", TestDataFactory.Layout());
        timetable.Add(TestDataFactory.CreateTrainInForwardDirection(category, 1, Time.FromHourAndMinute(12, 00)));
        return Plan.Create("Test", timetable);
    }

    private static OperationLocation Goteborg(Plan plan) => plan.Layout.OperationLocations.Single(l => l.Signature == "G");

    [TestMethod]
    public void AShunterIsStationedAtItsLocationAndWorksNoSchedule()
    {
        var plan = CreatePlan();
        var location = Goteborg(plan);

        var shunter = plan.AddShunter(location, "Z65", 5, null, TractionType.Diesel);

        Assert.IsNotNull(shunter);
        Assert.AreEqual(ScheduledObjectType.Shunter, shunter.ObjectType);
        Assert.AreEqual(TractionType.Diesel, shunter.TractionType, "A shunter is powered, so it keeps its traction type.");
        Assert.AreSame(location, plan.StationOf(shunter));
        CollectionAssert.AreEqual(new[] { shunter }, plan.ShuntersAt(location).ToList());
        Assert.IsFalse(shunter.IsTraction, "A shunter hauls no train, so it is never offered for a schedule.");
        Assert.IsFalse(shunter.HasTurnusCard, "Without a schedule there is no turnus card to print.");
        Assert.IsNull(shunter.Start(useDays: false, maxSessions: 4), "It starts no train part.");
    }

    [TestMethod]
    public void NoShunterIsStationedAtASignalControlledLocation()
    {
        var plan = CreatePlan();
        var blockPost = plan.Layout.Add(new SignalControlledLocation(99, "Block post", "Bp"));

        Assert.IsFalse(blockPost.CanHaveShunters);
        Assert.IsNull(plan.AddShunter(blockPost, "Z65", 5, null));
        Assert.IsEmpty(plan.ScheduledObjects);
    }

    [TestMethod]
    public void AShunterIsRollingStockBroughtWithADccAddressAndNumber()
    {
        var plan = CreatePlan();
        var shunter = plan.AddShunter(Goteborg(plan), "Z65", 5, null)!;
        var anna = plan.FindOrAddParticipant("Anna")!;

        CollectionAssert.Contains(plan.RollingStock.ToList(), shunter);
        Assert.IsTrue(shunter.NeedsDccAddress);
        Assert.IsTrue(shunter.TakesVehicleNumber);
        Assert.IsNull(plan.AddContributor(shunter, anna, null), "Like any locomotive, it is not brought without an address.");
        var contributor = plan.AddContributor(shunter, anna, 65, vehicleNumber: "561");
        Assert.IsNotNull(contributor);
        Assert.AreEqual("561", contributor.VehicleNumber);
    }

    [TestMethod]
    public void AShunterClaimsItsIdentityOnEverySession()
    {
        var plan = CreatePlan();
        var shunter = plan.AddShunter(Goteborg(plan), "Z65", 5, null)!;

        Assert.AreSame(shunter, plan.VehicleClaiming(VehicleIdentity.Of(null, 5), Sessions.FromSessionNumbers(3)));
    }

    [TestMethod]
    public void TurningAShunterIntoAnotherKindOfVehicleUnstationsIt()
    {
        var plan = CreatePlan();
        var location = Goteborg(plan);
        var shunter = plan.AddShunter(location, "Z65", 5, null)!;

        plan.UpdateVehicle(shunter, ScheduledObjectType.Locomotive, null, "Z65", 5, null);

        Assert.IsNull(shunter.StationedAtId);
        Assert.IsEmpty(plan.ShuntersAt(location));
    }

    [TestMethod]
    public void DeletingALocationDeletesTheShuntersStationedThere()
    {
        var plan = CreatePlan();
        var location = Goteborg(plan);
        var shunter = plan.AddShunter(location, "Z65", 5, null)!;
        plan.AddContributor(shunter, plan.FindOrAddParticipant("Anna")!, 65);

        CollectionAssert.AreEqual(new[] { shunter }, plan.PreviewDelete(location).ShuntersRemoved.ToList());
        Assert.IsTrue(plan.TryDelete(location).IsSuccess);

        Assert.DoesNotContain(shunter, plan.ScheduledObjects);
        Assert.IsNull(plan.ContributionFor(shunter), "Who was to bring it goes with it.");
    }

    [TestMethod]
    public void AShunterSurvivesASave()
    {
        var plan = CreatePlan();
        var location = Goteborg(plan);
        var shunter = plan.AddShunter(location, "Z65", 5, null, TractionType.Electric)!;

        var restored = JsonSerializer.Deserialize<Plan>(JsonSerializer.Serialize(plan, Options), Options);

        Assert.IsNotNull(restored);
        var restoredShunter = restored.ScheduledObjects.Single(v => v.Id == shunter.Id);
        Assert.AreEqual(ScheduledObjectType.Shunter, restoredShunter.ObjectType);
        Assert.AreEqual(TractionType.Electric, restoredShunter.TractionType);
        Assert.AreEqual(location.Id, restored.StationOf(restoredShunter)?.Id);
    }
}
