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

    private static Layout LayoutIn(int countryId, bool useLocalLanguages = true)
    {
        var layout = new Layout { Name = "Test" };
        layout.Settings.Identity.DefaultCountryId = countryId;
        layout.Settings.General.UseObjectLanguageInReports = useLocalLanguages;
        return layout;
    }

    private static Company CompanyIn(int countryId) => new(countryId, $"C{countryId}", $"C{countryId}", countryId);

    [TestMethod]
    public void DefaultLanguageIsTheFirstLanguageOfTheDefaultCountry()
    {
        var layout = LayoutIn(Switzerland);
        Assert.AreEqual("de", layout.DefaultLanguage);
        Assert.AreEqual("de-CH", layout.DefaultReportCulture.Name);
    }

    [TestMethod]
    public void WithoutLocalLanguagesEveryItemUsesTheDefault()
    {
        var layout = LayoutIn(Sweden, useLocalLanguages: false);
        var culture = layout.ReportCultureOf(CompanyIn(Denmark), [CompanyIn(Denmark)], IsAvailable);
        Assert.AreEqual("sv-SE", culture.Name);
    }

    [TestMethod]
    public void TheItemsOwnCompanyDecidesTheLanguage()
    {
        var layout = LayoutIn(Sweden);
        var culture = layout.ReportCultureOf(CompanyIn(Denmark), [CompanyIn(Germany)], IsAvailable);
        Assert.AreEqual("da-DK", culture.Name);
    }

    [TestMethod]
    public void WithoutCompanyOperatorsSharingALanguageDecide()
    {
        var layout = LayoutIn(Sweden);
        var culture = layout.ReportCultureOf(null, [CompanyIn(Germany), CompanyIn(Austria), null], IsAvailable);
        Assert.AreEqual("de-DE", culture.Name);
    }

    [TestMethod]
    public void OperatorsOfDifferentLanguagesFallBackToTheDefault()
    {
        var layout = LayoutIn(Sweden);
        var culture = layout.ReportCultureOf(null, [CompanyIn(Germany), CompanyIn(Denmark)], IsAvailable);
        Assert.AreEqual("sv-SE", culture.Name);
    }

    [TestMethod]
    public void ACompanyInAnUnsupportedLanguageIsPassedOver()
    {
        var layout = LayoutIn(Sweden);
        Assert.AreEqual("de-DE", layout.ReportCultureOf(CompanyIn(Netherlands), [CompanyIn(Germany)], IsAvailable).Name);
        Assert.AreEqual("sv-SE", layout.ReportCultureOf(null, [CompanyIn(Netherlands)], IsAvailable).Name);
    }

    [TestMethod]
    public void ALocationUsesTheLanguageOfItsCountry()
    {
        var layout = LayoutIn(Sweden);
        var station = new Station(1, "Fredericia", "Fa") { CountryId = Denmark };
        Assert.AreEqual("da-DK", station.ReportCulture(layout, IsAvailable).Name);
    }

    [TestMethod]
    public void ALocationWithoutCountryUsesTheDefault()
    {
        var layout = LayoutIn(Denmark);
        var station = new Station(1, "Ort", "O");
        Assert.AreEqual("da-DK", station.ReportCulture(layout, IsAvailable).Name);
    }
}
