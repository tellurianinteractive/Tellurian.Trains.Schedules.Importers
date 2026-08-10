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

    /// <summary>One paragraph long enough to be charged exactly <paramref name="lines"/> wrapped lines.</summary>
    /// <remarks>The first line is free; every further <c>CharactersPerLine</c> adds one.</remarks>
    private static string Paragraph(string marker, int lines)
    {
        var length = (lines - 1) * InstructionsPagination.CharactersPerLine;
        return marker + new string('x', Math.Max(0, length - marker.Length));
    }

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
    public void TheOverviewIsTheLastPageOfTheBooklet()
    {
        // Reached by opening the booklet at the back, as in the duty booklets — so the blank padding
        // goes before it, never after.
        foreach (var paragraphs in new[] { 0, 1, 5, 30 })
        {
            var pages = InstructionsPagination.BuildPages(Paragraphs(paragraphs));

            Assert.AreEqual(InstructionsPageKind.Overview, pages[^1].Kind, $"{paragraphs} paragraphs.");
            Assert.AreEqual(0, pages.Count % 4, $"{paragraphs} paragraphs produced {pages.Count} pages.");
        }
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
        //
        // The sizes are stated in charged lines rather than characters, so they follow the estimator's
        // constants: the two big paragraphs and the short one leave a gap wide enough for the rule
        // (33 − 2.5 title − 21.9 = 8.6 against the 8.25 it wants), and the last is too tall to join them.
        var bigA = Paragraph("AAA", lines: 10);
        var bigB = Paragraph("BBB", lines: 10);
        var shortP = Paragraph("SHORT", lines: 1);
        var bigC = Paragraph("CCC", lines: 15);
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
    public void ARunOfHeadingsStaysWithTheTextItIntroduces()
    {
        // A chapter heading followed straight by its first section heading. Attaching only the next
        // block would pair the two headings and leave the text they announce on its own.
        var blocks = InstructionsPagination
            .Blocks("# Operating\n\n## Signalling\n\nAlways stop at a red signal.\n\nNext.").ToList();

        Assert.Contains("# Operating", blocks[0]);
        Assert.Contains("## Signalling", blocks[0]);
        Assert.Contains("Always stop at a red signal.", blocks[0]);
        Assert.HasCount(2, blocks);
    }

    [TestMethod]
    public void NoPageEndsOnAHeadingWithItsTextOverleaf()
    {
        var markdown = string.Join("\n\n", Enumerable.Range(1, 40)
            .Select(i => $"# Chapter {i}\n\n## Section {i}\n\nWhat section {i} says."));

        var pages = InstructionsPagination.BuildPages(markdown, includeOverview: false);

        var contentPages = pages.Where(p => p.Kind == InstructionsPageKind.Content).ToList();
        Assert.IsTrue(contentPages.Count > 1, "Need several content pages to test this meaningfully.");
        foreach (var page in contentPages)
        {
            var lastLine = page.Markdown.Split('\n').Last(l => l.Trim().Length > 0);
            Assert.IsFalse(lastLine.TrimStart().StartsWith('#'),
                $"Page {page.PageNumber} ends on the heading '{lastLine}'.");
        }
    }

    [TestMethod]
    public void TheFirstContentPageLeavesRoomForTheSectionTitle()
    {
        // As many one-line paragraphs as fill a page that carries no title. The first content page also
        // prints the "Instructions" title, so they cannot all fit on it — before the title was charged,
        // the last of them printed past the foot of the page and was silently clipped.
        var perPage = (int)(InstructionsPagination.PageBudget / (1 + InstructionsPagination.BlockGap));

        var pages = InstructionsPagination.BuildPages(Paragraphs(perPage), includeOverview: false);

        Assert.IsTrue(pages.Count(p => p.Kind == InstructionsPageKind.Content) > 1,
            "A full page of text must not be crammed onto the page that also carries the section title.");
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
