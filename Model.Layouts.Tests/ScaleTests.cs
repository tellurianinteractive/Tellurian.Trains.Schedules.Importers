namespace Tellurian.Trains.Schedules.Model.Layouts.Tests;

/// <summary>The suite's one list of scales, and finding the known scale a named one is (step 7, decision 11).</summary>
[TestClass]
public class ScaleTests
{
    [TestMethod]
    public void AScaleIsFoundByItsId() => Assert.AreEqual("TT", Scale.Known(new() { Id = 2, ShortName = "whatever" })?.ShortName);

    [TestMethod]
    public void AScaleWithoutIdIsFoundByItsName()
    {
        Assert.AreEqual(3, Scale.Known(new() { ShortName = "h0" })?.Id);
        Assert.AreEqual(1, Scale.Known(new() { ShortName = " N " })?.Id);
    }

    [TestMethod]
    public void TwoScalesOfOneNameAreToldApartByTheirRatio()
    {
        Assert.AreEqual(5, Scale.Known(new() { ShortName = "0", Denominator = 45 })?.Id);
        Assert.AreEqual(101, Scale.Known(new() { ShortName = "0", Denominator = 48 })?.Id);
        Assert.IsNull(Scale.Known(new() { ShortName = "0" }), "0 alone is both 1:45 and 1:48.");
    }

    [TestMethod]
    public void AnUnknownScaleIsNotFound()
    {
        Assert.IsNull(Scale.Known(new() { ShortName = "Z" }));
        Assert.IsNull(Scale.Known(new() { ShortName = "H0", Denominator = 76 }));
        Assert.IsNull(Scale.Known(null));
    }

    [TestMethod]
    public void IdsAreUnique() => Assert.HasCount(Scale.Scales.Count(), Scale.Scales.Select(s => s.Id).Distinct());
}
