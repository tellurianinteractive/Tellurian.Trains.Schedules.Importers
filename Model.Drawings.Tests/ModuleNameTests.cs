namespace Tellurian.Trains.Schedules.Model.Drawings.Tests;

/// <summary>What a module is called, from what is given: prefix, number and name.</summary>
[TestClass]
public class ModuleNameTests
{
    private static readonly OperationLocation Tellby = new Station(1, "Tellby", "Tby");

    [TestMethod]
    public void NumberAndPrefixGiveTheDesignation()
    {
        var module = new Module { Name = "Curve", Prefix = "AN", Number = 12 };
        Assert.AreEqual("AN 0012", module.Designation);
        Assert.AreEqual("AN 0012", module.DisplayName);
    }

    [TestMethod]
    public void AnOperationLocationIsNamedAfterTheDesignation()
    {
        var module = new Module { Name = "Tellby station", Prefix = "SF", Number = 7, HasFremoDesignation = true, OperationLocation = Tellby };
        Assert.AreEqual("SF 0007 Tellby", module.DisplayName);
    }

    [TestMethod]
    public void ANumberWithoutPrefixIsTheNumberAlone()
    {
        Assert.AreEqual("12", new Module { Name = "Curve", Number = 12 }.Designation);
        Assert.AreEqual("Curve 12", new Module { Name = "Curve", Number = 12 }.DisplayName);
        Assert.AreEqual("Curve 12", new Module { Name = "Curve", Number = 12, Prefix = " " }.DisplayName);
    }

    [TestMethod]
    public void WithoutNumberTheNameIsUsed()
    {
        Assert.AreEqual("Curve", new Module { Name = "Curve", Prefix = "SF" }.DisplayName);
        Assert.AreEqual("Tellby short", new Module { Name = "Tellby", ConfigurationLabel = "Short", OperationLocation = Tellby }.DisplayName);
        Assert.IsNull(new Module { Name = "Curve", Prefix = "SF" }.Designation);
    }

    [TestMethod]
    public void TheStandardAndItsScaleComeFirstWithoutRepeatedWords()
    {
        var standard = new ModuleStandard { ShortName = "FREMO/H0-RE", Scale = new() { ShortName = "H0" } };
        Assert.AreEqual("FREMO/H0-RE SF 0007 Tellby", new Module { Name = "Tellby", Prefix = "SF", Number = 7, Standard = standard, OperationLocation = Tellby }.DisplayName);
        Assert.AreEqual("FREMO/H0-RE Curve 12", new Module { Name = "Curve", Number = 12, Standard = standard }.DisplayName);
        Assert.AreEqual("FREMO/H0-RE curve", new Module { Name = "H0 curve", Standard = standard }.DisplayName, "A word of the name that the standard has is left out.");
        Assert.AreEqual("MMM-2R H0 Tellby short", new Module { Name = "Tellby", ConfigurationLabel = "Short", Standard = new() { ShortName = "MMM-2R", Scale = new() { ShortName = "H0" } } }.DisplayName);
    }

    [TestMethod]
    public void TheTitleIsTheNameWithoutStandardAndScale()
    {
        var standard = new ModuleStandard { ShortName = "FREMO/H0-RE", Scale = new() { ShortName = "H0" } };
        Assert.AreEqual("SF 0007 Tellby", new Module { Name = "Tellby", Prefix = "SF", Number = 7, Standard = standard, OperationLocation = Tellby }.Title);
        Assert.AreEqual("Curve 12", new Module { Name = "Curve", Number = 12, Standard = standard }.Title);
        Assert.AreEqual("SF 1234", new Module { Name = "", Prefix = "SF", Number = 1234 }.Title, "A module can be known by its designation alone.");
        Assert.AreEqual("12", new Module { Name = " ", Number = 12 }.Title);
        Assert.AreEqual(string.Empty, new Module { Name = "" }.Title);
    }

    [TestMethod]
    public void AStandardWithoutScaleIsTheStandardAlone()
    {
        Assert.AreEqual("FREMO AN 0012", new Module { Name = "Curve", Prefix = "AN", Number = 12, Standard = new() { ShortName = "FREMO" } }.DisplayName);
    }

    [TestMethod]
    public void TheOwnersAreNotPartOfTheName()
    {
        var module = new Module { Name = "Curve", ModuleOwners = "Stefan Fjällemark, Module Club" };
        Assert.AreEqual("Curve", module.DisplayName);
    }
}
