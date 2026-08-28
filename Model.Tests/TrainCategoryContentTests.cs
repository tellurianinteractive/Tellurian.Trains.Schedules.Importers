using System.Text.Json;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Covers what a train category says it exchanges — <see cref="TrainCategory.Content"/> — and the two
/// booleans an earlier version wrote instead.
/// </summary>
[TestClass]
public class TrainCategoryContentTests
{
    private static TrainCategory Category(TrainContent content, bool isShunting = false) =>
        new() { Id = 1, Name = "Test", Content = content, IsShunting = isShunting };

    [TestMethod]
    public void ContentSaysWhatTheCategoryExchanges()
    {
        Assert.IsTrue(Category(TrainContent.Passenger).IsPassenger);
        Assert.IsFalse(Category(TrainContent.Passenger).IsFreight);
        Assert.IsTrue(Category(TrainContent.Cargo).IsFreight);
        Assert.IsFalse(Category(TrainContent.Cargo).IsPassenger);
        var mixed = Category(TrainContent.Passenger | TrainContent.Cargo);
        Assert.IsTrue(mixed.IsPassenger);
        Assert.IsTrue(mixed.IsFreight);
    }

    [TestMethod]
    public void ACategoryExchangingNothingIsAServiceCategory()
    {
        var service = Category(TrainContent.None);

        Assert.IsTrue(service.IsService);
        Assert.IsFalse(service.IsPassenger);
        Assert.IsFalse(service.IsFreight);
    }

    [TestMethod]
    public void AShuntingCategoryIsAFreightCategoryAndNeverAServiceOne()
    {
        // What a shunting task handles is cargo wagons, whatever its content happens to say.
        Assert.IsTrue(Category(TrainContent.Cargo, isShunting: true).IsFreight);
        Assert.IsTrue(Category(TrainContent.None, isShunting: true).IsFreight);
        Assert.IsFalse(Category(TrainContent.None, isShunting: true).IsService);
    }

    [TestMethod]
    public void TheBooleansAnEarlierVersionWroteAreReadIntoContent()
    {
        Assert.AreEqual(TrainContent.Passenger, Read(isPassenger: true, isFreight: false));
        Assert.AreEqual(TrainContent.Cargo, Read(isPassenger: false, isFreight: true));
        Assert.AreEqual(TrainContent.Passenger | TrainContent.Cargo, Read(isPassenger: true, isFreight: true));
        // Neither one set could only mean a category that exchanges nothing: a service category.
        Assert.AreEqual(TrainContent.None, Read(isPassenger: false, isFreight: false));

        static TrainContent Read(bool isPassenger, bool isFreight)
        {
            var json = $$"""
                {"Id":1,"Name":"Test","IsPassenger":{{(isPassenger ? "true" : "false")}},"IsFreight":{{(isFreight ? "true" : "false")}}}
                """;
            var category = JsonSerializer.Deserialize<TrainCategory>(json, PlanJson.CreateOptions());
            Assert.IsNotNull(category);
            return category.Content;
        }
    }

    [TestMethod]
    public void ThoseBooleansAreNeverWrittenBack()
    {
        var json = JsonSerializer.Serialize(Category(TrainContent.Cargo), PlanJson.CreateOptions());

        Assert.Contains("\"Content\"", json);
        Assert.DoesNotContain("\"IsPassenger\"", json);
        Assert.DoesNotContain("\"IsFreight\"", json);
    }
}
