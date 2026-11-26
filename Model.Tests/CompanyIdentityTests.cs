namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies that a company is identified by its id, not its signature: one operator may appear several
/// times with the same signature, e.g. once per period with a logo of its own.
/// </summary>
[TestClass]
public class CompanyIdentityTests
{
    [TestMethod]
    public void CompaniesWithTheSameSignatureButDifferentIdsAreDifferent()
    {
        var early = new Company(1, "DSB 1972-1987", "DSB");
        var late = new Company(2, "DSB 1988-2008", "DSB");

        Assert.AreNotEqual(early, late);
    }

    [TestMethod]
    public void CompaniesWithTheSameIdAreTheSame()
    {
        Assert.AreEqual(new Company(1, "DSB 1988-2008", "DSB"), new Company(1, "Danske Statsbaner", "DSB"));
    }

    [TestMethod]
    public void CompaniesNotYetGivenAnIdAreOnlyTheSameAsThemselves()
    {
        var one = new Company(0, "Danske Statsbaner", "DSB");
        var other = new Company(0, "Deutsche Bahn", "DB");

        Assert.AreEqual(one, one);
        Assert.AreNotEqual(one, other);
    }

    [TestMethod]
    public void TheLayoutHoldsSeveralCompaniesWithTheSameSignature()
    {
        TestDataFactory.Init();
        var layout = TestDataFactory.Layout();
        var before = layout.Companies.Count;

        layout.Add(new Company(901, "DSB 1972-1987", "DSB"));
        layout.Add(new Company(902, "DSB 1988-2008", "DSB"));

        Assert.HasCount(before + 2, layout.Companies);
        Assert.AreEqual(901, layout.Company("DSB").Value.Id, "The lowest id is found by signature.");
    }
}
