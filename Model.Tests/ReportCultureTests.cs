namespace Tellurian.Trains.Schedules.Model.Tests;

[TestClass]
public class ReportCultureTests
{
    private const int Sweden = 1;
    private const int Denmark = 3;
    private const int Germany = 4;
    private const int Switzerland = 5;
    private const int Austria = 14;
    private const int Netherlands = 7;

    private static readonly string[] Supported = ["en", "de", "da", "nb", "sv"];

    private static bool IsAvailable(string language) => Supported.Contains(language);

    private static Timetable TimetableIn(int countryId, bool useLocalLanguages = true)
    {
        var timetable = new Timetable("Test", new Layout { Name = "Test" });
        timetable.Settings.Identity.DefaultCountryId = countryId;
        timetable.Settings.General.UseObjectLanguageInReports = useLocalLanguages;
        return timetable;
    }

    private static Company CompanyIn(int countryId) => new(countryId, $"C{countryId}", $"C{countryId}", countryId);

    [TestMethod]
    public void DefaultLanguageIsTheFirstLanguageOfTheDefaultCountry()
    {
        var timetable = TimetableIn(Switzerland);
        Assert.AreEqual("de", timetable.DefaultLanguage);
        Assert.AreEqual("de-CH", timetable.DefaultReportCulture.Name);
    }

    [TestMethod]
    public void WithoutLocalLanguagesEveryItemUsesTheDefault()
    {
        var timetable = TimetableIn(Sweden, useLocalLanguages: false);
        var culture = timetable.ReportCultureOf(CompanyIn(Denmark), [CompanyIn(Denmark)], IsAvailable);
        Assert.AreEqual("sv-SE", culture.Name);
    }

    [TestMethod]
    public void TheItemsOwnCompanyDecidesTheLanguage()
    {
        var timetable = TimetableIn(Sweden);
        var culture = timetable.ReportCultureOf(CompanyIn(Denmark), [CompanyIn(Germany)], IsAvailable);
        Assert.AreEqual("da-DK", culture.Name);
    }

    [TestMethod]
    public void WithoutCompanyOperatorsSharingALanguageDecide()
    {
        var timetable = TimetableIn(Sweden);
        var culture = timetable.ReportCultureOf(null, [CompanyIn(Germany), CompanyIn(Austria), null], IsAvailable);
        Assert.AreEqual("de-DE", culture.Name);
    }

    [TestMethod]
    public void OperatorsOfDifferentLanguagesFallBackToTheDefault()
    {
        var timetable = TimetableIn(Sweden);
        var culture = timetable.ReportCultureOf(null, [CompanyIn(Germany), CompanyIn(Denmark)], IsAvailable);
        Assert.AreEqual("sv-SE", culture.Name);
    }

    [TestMethod]
    public void ACompanyInAnUnsupportedLanguageIsPassedOver()
    {
        var timetable = TimetableIn(Sweden);
        Assert.AreEqual("de-DE", timetable.ReportCultureOf(CompanyIn(Netherlands), [CompanyIn(Germany)], IsAvailable).Name);
        Assert.AreEqual("sv-SE", timetable.ReportCultureOf(null, [CompanyIn(Netherlands)], IsAvailable).Name);
    }

    [TestMethod]
    public void ALocationUsesTheLanguageOfItsCountry()
    {
        var timetable = TimetableIn(Sweden);
        var station = new Station(1, "Fredericia", "Fa") { CountryId = Denmark };
        Assert.AreEqual("da-DK", station.ReportCulture(timetable, IsAvailable).Name);
    }

    [TestMethod]
    public void ALocationWithoutCountryUsesTheDefault()
    {
        var timetable = TimetableIn(Denmark);
        var station = new Station(1, "Ort", "O");
        Assert.AreEqual("da-DK", station.ReportCulture(timetable, IsAvailable).Name);
    }

    [TestMethod]
    public void AChosenLanguageIsPrintedInTheCountryOfTheLayoutThatSpeaksIt()
    {
        var timetable = TimetableIn(Sweden);
        timetable.Layout.Countries.Add(Country.ById(2)!);
        Assert.AreEqual("sv-SE", timetable.ReportCultureIn("sv").Name);
        Assert.AreEqual("nb-NO", timetable.ReportCultureIn("nb").Name);
        // No country of the layout speaks it first: the language alone.
        Assert.AreEqual("de", timetable.ReportCultureIn("de").Name);
    }
}
