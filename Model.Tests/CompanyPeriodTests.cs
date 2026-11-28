using System.Globalization;
using Tellurian.Trains.Schedules.Model.PlanFiles;
using Tellurian.Trains.Schedules.Model.Settings;
using Tellurian.Trains.Schedules.Model.Validations;

namespace Tellurian.Trains.Schedules.Model.Tests;

/// <summary>
/// Verifies the operating period of a company: companies may share a signature, one per period, but two
/// of them operating in a common year is a conflict.
/// </summary>
[TestClass]
public class CompanyPeriodTests
{
    private static readonly ValidationSettings Validation = new();

    [TestInitialize]
    public void UseInvariantCulture()
    {
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    private static Company Dsb(int id, int? from, int? to) =>
        new(id, $"DSB {from}-{to}", "DSB") { OperatesFromYear = from, OperatesToYear = to };

    [TestMethod]
    [DataRow(1988, 2008, "DSB (1988–2008)")]
    [DataRow(1988, null, "DSB (1988–)")]
    [DataRow(null, 2008, "DSB (–2008)")]
    [DataRow(1988, 1988, "DSB (1988)")]
    [DataRow(null, null, "DSB")]
    public void ChoiceTextNamesThePeriodWhenGiven(int? from, int? to, string expected)
    {
        var company = new Company(1, "DSB", "DSB") { OperatesFromYear = from, OperatesToYear = to };

        Assert.AreEqual(expected, company.ChoiceText);
    }

    [TestMethod]
    public void ChoiceOrderIsByNameThenPeriod()
    {
        Company[] companies =
        [
            new(1, "DSB", "DSB") { OperatesFromYear = 1988 },
            new(2, "Banedanmark", "BDK"),
            new(3, "DSB", "DSB") { OperatesFromYear = 1972, OperatesToYear = 1987 },
            new(4, "DSB", "DSB"),
        ];

        CollectionAssert.AreEqual(new[] { 2, 4, 3, 1 }, companies.InChoiceOrder.Select(c => c.Id).ToArray());
    }

    [TestMethod]
    [DataRow(1972, 1987, 1988, 2008, false)]
    [DataRow(1972, 1988, 1988, 2008, true)]
    [DataRow(null, 1987, 1988, null, false)]
    [DataRow(null, 1988, 1988, null, true)]
    [DataRow(1972, 1987, null, null, true)]
    [DataRow(1990, 2000, 1972, 2008, true)]
    public void PeriodsOverlapWhenTheyShareAYear(int? from, int? to, int? otherFrom, int? otherTo, bool overlaps)
    {
        var one = Dsb(1, from, to);
        var other = Dsb(2, otherFrom, otherTo);

        Assert.AreEqual(overlaps, one.OperatesInSamePeriodAs(other));
        Assert.AreEqual(overlaps, other.OperatesInSamePeriodAs(one));
    }

    [TestMethod]
    [DataRow(1988, 2008, "1988–2008")]
    [DataRow(1988, null, "1988–")]
    [DataRow(null, 2008, "–2008")]
    [DataRow(1988, 1988, "1988")]
    [DataRow(null, null, "")]
    public void ThePeriodIsShownAsText(int? from, int? to, string expected)
    {
        Assert.AreEqual(expected, Dsb(1, from, to).OperatingPeriodText);
    }

    [TestMethod]
    public void CompaniesWithTheSameSignatureInSeparatePeriodsAreNoConflict()
    {
        var plan = Arrange(Dsb(901, 1972, 1987), Dsb(902, 1988, 2008));

        Assert.IsEmpty(Overlaps(plan));
    }

    [TestMethod]
    public void CompaniesWithTheSameSignatureInOverlappingPeriodsAreReportedOnce()
    {
        var early = Dsb(901, 1972, 1990);
        var late = Dsb(902, 1988, 2008);
        var plan = Arrange(early, late);

        var error = Overlaps(plan).Single();

        Assert.AreSame(late, error.Company);
        Assert.AreSame(early, error.OtherCompany);
        Assert.AreEqual(ValidationScope.Layout, error.Scope);
        Assert.Contains("1988–2008", error.Message.Text);
    }

    [TestMethod]
    public void CompaniesWithTheSameSignatureAndNoPeriodsAreReported()
    {
        var plan = Arrange(new Company(901, "Danske Statsbaner", "DSB"), new Company(902, "DSB", "dsb"));

        Assert.HasCount(1, Overlaps(plan));
    }

    [TestMethod]
    public void ThePeriodIsKeptInThePlanFile()
    {
        var plan = Arrange(Dsb(901, 1972, null), Dsb(902, null, 2008));

        var restored = PlanFile.Read(PlanFile.Write(plan));

        var companies = restored.Timetable.Companies.Where(c => c.Signature == "DSB").OrderBy(c => c.Id).ToArray();
        Assert.AreEqual(1972, companies[0].OperatesFromYear);
        Assert.IsNull(companies[0].OperatesToYear);
        Assert.IsNull(companies[1].OperatesFromYear);
        Assert.AreEqual(2008, companies[1].OperatesToYear);
    }

    private static Plan Arrange(params Company[] companies)
    {
        TestDataFactory.Init();
        var plan = Plan.Create("Test", TestDataFactory.NewTimetable("Test", TestDataFactory.Layout()));
        foreach (var company in companies) plan.Timetable.Add(company);
        return plan;
    }

    private static List<ValidationError> Overlaps(Plan plan) =>
        [.. plan.GetValidationErrors(Validation).Where(e => e.ErrorType == ValidationErrorType.CompanyPeriodsOverlap)];
}
