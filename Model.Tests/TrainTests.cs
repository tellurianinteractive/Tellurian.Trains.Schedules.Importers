using Tellurian.Trains.Schedules.Model;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class TrainTests
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    private Train Target;
    private static TrainCategory Category => new() { Id = 1, Prefix = "G", Name = "FreightTrain" };

    [TestInitialize]
    public void TestInitialize()
    {

        Target = new(11, Category, 1234, "");
    }

    [TestMethod]
    public void PropertiesAreSet()
    {
        Assert.AreEqual(Category, Target.Category);
        Assert.AreEqual(1234, Target.Number);
        Assert.AreEqual("", Target.ExternalId);
    }

    [TestMethod]
    public void SameNumberInDifferentCompaniesIsNotEqualEvenWhenCompaniesAreUnresolved()
    {
        var one = new Train(1, Category, 1234, "") { CompanyId = 1 };
        var another = new Train(2, Category, 1234, "") { CompanyId = 2 };
        var same = new Train(3, Category, 1234, "") { CompanyId = 1 };
        Assert.IsFalse(one.Equals(another));
        Assert.IsTrue(one.Equals(same));
    }

    [TestMethod]
    public void AddsFirstTimetableCall()
    {
        var station = TestDataFactory.CreateStation1();
        var call = new StationCall(1, station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45));
        Target.Add(call);
        Assert.IsFalse(Target.CheckTrainTimeSequence().Any());
    }

    [TestMethod]
    public void WhenSecondTimetableCallIsBeforeLastThenValidationErrors()
    {
        var station = TestDataFactory.CreateStation1();
        Target.Add(new StationCall(1, station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45)));
        Target.Add(new StationCall(2, station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45)));
        var validationErrors = Target.GetValidationErrors(new ValidationSettings());
        Assert.AreEqual(1, validationErrors.Count());
        Assert.IsFalse(validationErrors.Any(ve => string.IsNullOrWhiteSpace(ve.Message.Text)));
    }

    [TestMethod]
    public void WhenSecondTimetableCallIsAfterLastThenThrows()
    {
        var station = TestDataFactory.CreateStation1();
        var call1 = new StationCall(1, station.Tracks.First(), Time.FromHourAndMinute(12, 30), Time.FromHourAndMinute(12, 45));
        var call2 = new StationCall(2, station.Tracks.First(), Time.FromHourAndMinute(12, 50), Time.FromHourAndMinute(12, 55));
        Target.Add(call1);
        Target.Add(call2);
    }

    [TestMethod]
    public void SameNumberInCategoriesOfDifferentCompaniesIsNotEqual()
    {
        var sj = new TrainCategory { Id = 1, Prefix = "X", Name = "X", Company = new Company(1, "Statens Järnvägar", "SJ") };
        var dsb = new TrainCategory { Id = 2, Prefix = "IC", Name = "IC", Company = new Company(2, "DSB", "DSB") };
        var express = new Train(1, sj, 116, "");
        var intercity = new Train(2, dsb, 116, "");
        var another = new Train(3, sj, 116, "");

        Assert.AreNotEqual(express, intercity);
        Assert.AreEqual(express, another);
    }

    [TestMethod]
    public void EffectiveCompanyInheritsFromCategoryWhenTrainHasNone()
    {
        var categoryCompany = new Company(1, "Statens Järnvägar", "SJ");
        Target.Category = Category with { Company = categoryCompany };
        Target.Company = null;
        Assert.AreEqual(categoryCompany, Target.EffectiveCompany);
    }

    [TestMethod]
    public void EffectiveCompanyUsesTheTrainCompanyWhenSetAsOverride()
    {
        Target.Category = Category with { Company = new Company(1, "Statens Järnvägar", "SJ") };
        var ownCompany = new Company(2, "Green Cargo", "GC");
        Target.Company = ownCompany;
        Assert.AreEqual(ownCompany, Target.EffectiveCompany);
    }

    [TestMethod]
    public void EffectiveCompanyIsNullWhenNeitherTrainNorCategoryHasOne()
    {
        Target.Company = null;
        Assert.IsNull(Target.EffectiveCompany);
    }
}
