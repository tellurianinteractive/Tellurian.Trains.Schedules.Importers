using Tellurian.Trains.Schedules.Planning.Components.Reporting;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the report font catalogue and the CSS it produces.
/// </summary>
/// <remarks>
/// What is stored is a family name typed by nobody and installed nowhere in particular, so these tests
/// are about the two ways that can go wrong: a report set in a font the machine has not got, and a name
/// from a stored plan reaching an inline style as something other than a font name.
/// </remarks>
[TestClass]
public class ReportFontsTests
{
    [TestMethod]
    public void NoFontChosenKeepsTheApplicationsOwnStack()
    {
        foreach (var unset in new[] { (string?)null, "", "   " })
            Assert.AreEqual(ReportFonts.DefaultStack, ReportFonts.CssFontFamily(unset));
    }

    [TestMethod]
    public void AChosenFontLeadsAFallbackOfItsOwnKind()
    {
        // The generic at the end is what a machine without the font uses, so a serif choice must not end
        // up rendered in whatever the browser's default happens to be.
        Assert.AreEqual("'Georgia', 'Times New Roman', Times, serif", ReportFonts.CssFontFamily("Georgia"));
        Assert.AreEqual("'Verdana', Arial, Helvetica, sans-serif", ReportFonts.CssFontFamily("Verdana"));
        Assert.AreEqual("'Consolas', 'Courier New', Courier, monospace", ReportFonts.CssFontFamily("Consolas"));
    }

    [TestMethod]
    public void AnUnknownFontIsStillUsedAndFallsBackToSansSerif()
    {
        // A plan made on another machine may name a font this catalogue has never heard of. It is still
        // the planner's choice, so it is used — only its fallback has to be guessed.
        Assert.AreEqual("'Futura PT', Arial, Helvetica, sans-serif", ReportFonts.CssFontFamily("Futura PT"));
    }

    [TestMethod]
    public void AFontNameCannotCloseTheStyleAttributeItIsWrittenInto()
    {
        // The name goes into an inline style built from a stored plan, so anything that would end the
        // declaration is removed rather than trusted.
        var css = ReportFonts.CssFontFamily("Georgia'; background: url(x); color: red");

        Assert.DoesNotContain(";", css);
        Assert.DoesNotContain("(", css);
        Assert.AreEqual(2, css.Count(c => c == '\''));
    }

    [TestMethod]
    public void TheCatalogueIsFreeOfDuplicatesAndOrderedByGroup()
    {
        var names = ReportFonts.Candidates.Select(f => f.Name).ToList();
        Assert.AreEqual(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());

        // The drop-down groups in catalogue order, so a family filed out of order would split its
        // group into two headings.
        var groups = ReportFonts.Candidates.Select(f => f.Group).ToList();
        Assert.AreEqual(groups.Distinct().Count(), CountRuns(groups));
    }

    [TestMethod]
    public void ARailwayFaceIsFiledWithTheRailwaysAndFallsBackToSansSerif()
    {
        // Every railway and transport signage face here is a grotesque, so a machine without the font
        // must not drop the report into a serif.
        Assert.AreEqual(ReportFontGroup.Railway, ReportFonts.GroupOf("Bahnschrift"));
        Assert.AreEqual("'Bahnschrift', Arial, Helvetica, sans-serif", ReportFonts.CssFontFamily("Bahnschrift"));
        Assert.AreEqual("'Farringdon', Arial, Helvetica, sans-serif", ReportFonts.CssFontFamily("Farringdon"));
    }

    [TestMethod]
    public void TheRailwayFacesLeadTheList()
    {
        // They are what this application is for, and they are the ones a planner comes here to pick —
        // the general-purpose fonts are the fallback, not the headline.
        Assert.AreEqual(ReportFontGroup.Railway, ReportFonts.Candidates[0].Group);
        Assert.IsGreaterThan(10, ReportFonts.Candidates.Count(f => f.Group == ReportFontGroup.Railway));
    }

    private static int CountRuns(IReadOnlyList<ReportFontGroup> groups) =>
        groups.Count == 0 ? 0 : 1 + groups.Zip(groups.Skip(1)).Count(pair => pair.First != pair.Second);
}
