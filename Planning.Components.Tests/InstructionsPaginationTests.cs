using Tellurian.Trains.Schedules.Planning.Components.Reporting;
using Tellurian.Trains.Schedules.Planning.Components.Reporting.Instructions;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Covers the general instructions booklet: splitting a single markdown flow across pages, and the
/// padding and imposition it shares unchanged with the duty booklets.
/// </summary>
[TestClass]
public class InstructionsPaginationTests
{
    private static string Paragraphs(int count, int linesEach = 1) =>
        string.Join("\n\n", Enumerable.Range(1, count)
            .Select(i => string.Join("\n", Enumerable.Repeat($"Paragraph {i}.", linesEach))));

    [TestMethod]
    public void AShortDocumentFitsOneContentPage()
    {
        var pages = InstructionsPagination.BuildPages(Paragraphs(3));

        Assert.HasCount(1, pages.Where(p => p.Kind == InstructionsPageKind.Content));
    }

    [TestMethod]
    public void ALongDocumentIsSplitAcrossSeveralContentPages()
    {
        var pages = InstructionsPagination.BuildPages(Paragraphs(60));

        Assert.IsTrue(pages.Count(p => p.Kind == InstructionsPageKind.Content) > 1,
            "The flow must fill pages rather than overflow one.");
    }

    [TestMethod]
    public void ThePageCountIsAlwaysAMultipleOfFour()
    {
        // The padding rule is shared with the duty booklets and is what makes each booklet a whole
        // number of A4 landscape sheets.
        foreach (var paragraphs in new[] { 0, 1, 5, 30, 60, 200 })
        {
            var pages = InstructionsPagination.BuildPages(Paragraphs(paragraphs));
            Assert.AreEqual(0, pages.Count % 4, $"{paragraphs} paragraphs produced {pages.Count} pages.");
        }
    }

    [TestMethod]
    public void TheOverviewPageIsAppendedForReadersWhoHoldNoDutyBooklet()
    {
        var pages = InstructionsPagination.BuildPages(Paragraphs(3));

        Assert.HasCount(1, pages.Where(p => p.Kind == InstructionsPageKind.Overview));
    }

    [TestMethod]
    public void TheOverviewPageCanBeLeftOff()
    {
        var pages = InstructionsPagination.BuildPages(Paragraphs(3), includeOverview: false);

        Assert.IsEmpty(pages.Where(p => p.Kind == InstructionsPageKind.Overview));
        Assert.AreEqual(0, pages.Count % 4);
    }

    [TestMethod]
    public void AnEmptyDocumentStillYieldsAWholeSheet()
    {
        var pages = InstructionsPagination.BuildPages("", includeOverview: false);

        Assert.HasCount(4, pages);
        Assert.AreEqual(InstructionsPageKind.Front, pages[0].Kind);
        Assert.IsTrue(pages.Skip(1).All(p => p.Kind == InstructionsPageKind.Blank));
    }

    [TestMethod]
    public void TheFirstPageIsAlwaysTheFrontPage()
    {
        foreach (var paragraphs in new[] { 0, 3, 60 })
        {
            var pages = InstructionsPagination.BuildPages(Paragraphs(paragraphs));
            Assert.AreEqual(InstructionsPageKind.Front, pages[0].Kind);
        }
    }

    [TestMethod]
    public void OnlyTheFirstContentPageCarriesTheInstructionsHeading()
    {
        var pages = InstructionsPagination.BuildPages(Paragraphs(60));
        var contentPages = pages.Where(p => p.Kind == InstructionsPageKind.Content).ToList();

        Assert.IsTrue(contentPages.Count > 1, "Need several content pages to test this meaningfully.");
        Assert.IsTrue(contentPages[0].IsFirstOfSection);
        Assert.IsTrue(contentPages.Skip(1).All(p => !p.IsFirstOfSection));
    }

    [TestMethod]
    public void AShortTrailingParagraphIsNotStrandedBehindAGap()
    {
        // Two paragraphs long enough to fill most of a page, then a short one, then one too large to
        // join it on the same page. Without the stranded-block rule the short paragraph would be the
        // last thing on the first page, with a big gap behind it while the large paragraph moves
        // entirely to the next page anyway.
        string Line(string marker, int totalLength) => marker + new string('x', totalLength - marker.Length);

        var bigA = Line("AAA", 700);
        var bigB = Line("BBB", 700);
        var shortP = Line("SHORT", 10);
        var bigC = Line("CCC", 1000);
        var markdown = string.Join("\n\n", [bigA, bigB, shortP, bigC]);

        var pages = InstructionsPagination.BuildPages(markdown, includeOverview: false);
        var contentPages = pages.Where(p => p.Kind == InstructionsPageKind.Content).ToList();

        Assert.HasCount(2, contentPages);
        Assert.IsFalse(contentPages[0].Markdown.Contains("SHORT", StringComparison.Ordinal),
            "The short paragraph must not be stranded on the first page.");
        Assert.IsTrue(contentPages[1].Markdown.Contains("SHORT", StringComparison.Ordinal));
        Assert.IsTrue(contentPages[1].Markdown.Contains("CCC", StringComparison.Ordinal));
    }

    [TestMethod]
    public void AHeadingStaysWithTheBlockItIntroduces()
    {
        var blocks = InstructionsPagination.Blocks("# Signalling\n\nAlways stop at a red signal.\n\nNext.").ToList();

        // Otherwise the two could fall either side of a fold.
        Assert.Contains("# Signalling", blocks[0]);
        Assert.Contains("Always stop at a red signal.", blocks[0]);
    }

    [TestMethod]
    public void PagesAreNumberedInReadingOrderFromOne()
    {
        var pages = InstructionsPagination.BuildPages(Paragraphs(60));

        for (var i = 0; i < pages.Count; i++)
            Assert.AreEqual(i + 1, pages[i].PageNumber);
    }

    [TestMethod]
    public void ImpositionIsSharedWithTheDutyBookletsUnchanged()
    {
        var pages = InstructionsPagination.BuildPages(Paragraphs(30));

        var sides = BookletImposition.Impose(pages);

        Assert.AreEqual(pages.Count / 2, sides.Count);
        Assert.AreEqual(pages.Count, sides[0].Left.PageNumber);
        Assert.AreEqual(1, sides[0].Right.PageNumber);
    }
}
