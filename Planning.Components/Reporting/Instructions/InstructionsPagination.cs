namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Instructions;

/// <summary>What one page of the general instructions booklet holds.</summary>
public enum InstructionsPageKind
{
    /// <summary>Page 1: the meeting name, validity dates, print date, and the programme.</summary>
    Front,

    /// <summary>Authored markdown, under the "Instructions" heading.</summary>
    Content,

    /// <summary>The layout topology and shunting yards, under the "Layout" heading — the last page of
    /// the booklet, appended for readers who hold no duty booklet.</summary>
    Overview,

    /// <summary>Padding, so the page count is a multiple of four. Sits before the overview, never
    /// after it: the overview is turned to by opening the booklet at the back.</summary>
    Blank,
}

/// <summary>One A5 page of the general instructions booklet.</summary>
/// <param name="PageNumber">Its position in the booklet, from 1.</param>
/// <param name="Kind">What the page holds.</param>
/// <param name="Markdown">The markdown blocks on a content page, already joined.</param>
/// <param name="IsFirstOfSection">
/// True only for the first <see cref="InstructionsPageKind.Content"/> page, which alone carries the
/// "Instructions" heading — the pages after it continue the same flow. Meaningless for the other
/// kinds, which each occur at most once and always carry their own heading.
/// </param>
public sealed record InstructionsPage(
    int PageNumber, InstructionsPageKind Kind, string Markdown = "", bool IsFirstOfSection = false);

/// <summary>
/// Splits the plan's authored content into booklet pages.
/// </summary>
/// <remarks>
/// <para>
/// Simpler than the duty booklet's pagination: the content is a single flow, so pages are filled by
/// splitting it at block boundaries rather than by packing indivisible items. The front page is fixed
/// at page 1 and the layout overview is fixed as the last page, so the only variable is how many
/// content pages the authored instructions need in between.
/// </para>
/// <para>
/// Heights are <em>estimated</em>, never measured, exactly as in <c>DutyPagination</c>. The unit here
/// is <em>one line of body text</em>: 0.9 rem at the booklet's line-height of 1.5, which is 21.6 px at
/// the 16 px root size. Every constant below is a pixel figure taken from the booklet's CSS
/// (<c>.dutypage</c>, <c>.section-title</c>, <c>.instructions-body</c> in <c>app.css</c>) divided by
/// that 21.6 px, so each can be re-derived on its own and must be re-derived if the type size, the
/// line-height or the page padding changes. A page body is <c>overflow: hidden</c>, so a constant that
/// no longer matches the stylesheet does not print short — it prints past the foot of the page and the
/// rest of that block is dropped without a word.
/// </para>
/// </remarks>
public static class InstructionsPagination
{
    /// <summary>Lines of body text that fit on one A5 page body.</summary>
    /// <remarks>
    /// The page body is the A4 landscape sheet's full 210 mm height less 8 mm of padding top and
    /// bottom: 194 mm, or 733.2 px, which is 33.9 lines. Taken as 33 — a line short of what fits — so
    /// an estimate that runs slightly optimistic still lands on the page.
    /// </remarks>
    public const double PageBudget = 33;

    /// <summary>
    /// Lines the "Instructions" section title costs, charged to the first content page only.
    /// </summary>
    /// <remarks>
    /// 53.5 px, which is 2.48 lines: 1.6 rem at line-height 1.5 is 38.4 px, its 0.4 em bottom margin
    /// 10.2 px, the 0.15 em padding under the text 3.8 px, and the rule below it 1 px.
    /// </remarks>
    public const double SectionTitleHeight = 2.5;

    /// <summary>Lines one heading line costs, whatever its level.</summary>
    /// <remarks>
    /// Stated from the tallest, h1: 1.2 rem at line-height 1.5 is 28.8 px plus a 0.3 em bottom margin
    /// (5.8 px) — 34.6 px, or 1.60 lines. Charged to h2 (1.44 lines, its larger top margin collapsing
    /// into the margin under the paragraph before it) and h3 (1.18) as well: one constant, never
    /// charged short.
    /// </remarks>
    public const double HeadingHeight = 1.6;

    /// <summary>The gap under a paragraph, a list item, or a block.</summary>
    /// <remarks>0.4 em at 0.9 rem is 5.8 px, which is 0.27 lines. Taken as 0.3.</remarks>
    public const double BlockGap = 0.3;

    /// <summary>Characters that fit on one line before the text wraps.</summary>
    /// <remarks>
    /// The page body is 148.5 − 16 = 132.5 mm, or 499.5 px, and lower-case prose at 0.9 rem measures 79
    /// characters across it — 6.3 px a character. Taken as 72, so text carrying capitals, numbers and
    /// station names, all wider than lower-case prose, is still not charged short.
    /// </remarks>
    public const int CharactersPerLine = 72;

    /// <summary>
    /// The tallest a trailing block may be and still count as "short" for the stranded-block rule
    /// below. Empirical, like the other constants here.
    /// </summary>
    public const int ShortTrailingBlockHeight = 4;

    /// <summary>
    /// The smallest gap, in line units, left below a short trailing block that counts as "a lot of
    /// blank space" for the stranded-block rule below. A quarter of the page: less than that reads as
    /// a normally-filled page rather than one visibly cut short.
    /// </summary>
    public const double StrandedBlockGap = PageBudget * 0.25;

    /// <summary>
    /// Builds the booklet's pages: the front page, the instructions split across content pages, blank
    /// padding to a whole number of sheets, and the layout overview last.
    /// </summary>
    /// <param name="markdown">The authored instructions. Empty yields a booklet with no content pages.</param>
    /// <param name="includeOverview">
    /// Whether to append the layout overview. The people who receive this booklet and never hold a
    /// duty booklet — station staff above all — get no layout overview from anywhere else.
    /// </param>
    public static IReadOnlyList<InstructionsPage> BuildPages(string? markdown, bool includeOverview = true)
    {
        var pages = new List<InstructionsPage> { new(1, InstructionsPageKind.Front) };
        var blocks = Blocks(markdown).ToList();
        var current = new List<string>();
        var used = 0.0;
        // The section title is printed once, on the first content page, so only that page pays for it.
        var overhead = SectionTitleHeight;
        var firstContentPageEmitted = false;

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var height = HeightOf(block);

            // A block taller than a page still prints, overflowing, rather than being truncated: the
            // author is the only one who knows what can go, so the report's job is to make it visible.
            if (current.Count > 0 && overhead + used + height > PageBudget) Flush();

            current.Add(block);
            used += height;

            // Avoid stranding a short block alone at the end of a page with a lot of blank space below
            // it, while the next block is too big to join it here and moves entirely to the next page
            // regardless. The page reads better ending on the fuller block before this one, with this
            // one carried over to join what follows instead of trailing behind on its own.
            var hasNext = i + 1 < blocks.Count;
            var nextFitsHere = hasNext && overhead + used + HeightOf(blocks[i + 1]) <= PageBudget;
            if (hasNext && !nextFitsHere && current.Count > 1 &&
                height <= ShortTrailingBlockHeight && PageBudget - overhead - used >= StrandedBlockGap)
            {
                current.RemoveAt(current.Count - 1);
                used -= height;
                Flush();
                current.Add(block);
                used = height;
            }
        }
        Flush();

        // The overview page is always last, so blanks go before it rather than at the very end — the
        // count passed in therefore includes the overview page still to come. Same rule as the duty
        // booklets, where the overview is reached by opening the booklet at the back.
        var blanks = BookletImposition.BlanksNeeded(pages.Count + (includeOverview ? 1 : 0));
        for (var i = 0; i < blanks; i++)
            pages.Add(new InstructionsPage(pages.Count + 1, InstructionsPageKind.Blank));

        if (includeOverview)
            pages.Add(new InstructionsPage(pages.Count + 1, InstructionsPageKind.Overview));

        return pages;

        void Flush()
        {
            if (current.Count == 0) return;
            pages.Add(new InstructionsPage(
                pages.Count + 1, InstructionsPageKind.Content, string.Join("\n\n", current),
                IsFirstOfSection: !firstContentPageEmitted));
            firstContentPageEmitted = true;
            current = [];
            used = 0;
            overhead = 0;
        }
    }

    /// <summary>
    /// The markdown's top-level blocks, split on blank lines — the boundaries a reader recognises, so a
    /// page never breaks in the middle of a paragraph or between a heading and what it introduces.
    /// </summary>
    public static IEnumerable<string> Blocks(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) yield break;
        var paragraphs = markdown.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .Where(b => b.Length > 0)
            .ToList();

        // A heading belongs with the block it introduces, so the two never fall either side of a fold.
        // A run of them — a chapter heading immediately followed by its first section heading — belongs
        // with it too: attaching only the next paragraph would leave the second heading carrying the
        // first, and a page could still end on headings alone with the text they announce overleaf.
        for (var i = 0; i < paragraphs.Count; i++)
        {
            if (!IsHeadingOnly(paragraphs[i])) { yield return paragraphs[i]; continue; }

            var start = i;
            while (i < paragraphs.Count - 1 && IsHeadingOnly(paragraphs[i])) i++;
            // Ends on the body the run introduces, unless the document itself ends on a heading — in
            // which case there is nothing overleaf for it to be separated from.
            yield return string.Join("\n\n", paragraphs.Skip(start).Take(i - start + 1));
        }
    }

    /// <summary>Whether a paragraph is headings and nothing else.</summary>
    private static bool IsHeadingOnly(string paragraph) =>
        paragraph.Split('\n').All(line => line.TrimStart().StartsWith('#'));

    // Estimated, never measured: each source line charged its rendered height, wrapping charged per
    // character, and the block's own bottom margin added once.
    private static double HeightOf(string block) =>
        block.Split('\n').Sum(HeightOfLine) + BlockGap;

    private static double HeightOfLine(string line)
    {
        var text = line.TrimStart();
        // The blank line that joins a heading to its body; the margins either side of it are already
        // counted in HeadingHeight.
        if (text.Length == 0) return 0;
        if (text.StartsWith('#')) return HeadingHeight;

        var wrapped = 1 + line.Length / CharactersPerLine;
        // A list item carries a bottom margin of its own; its wrapped continuation lines do not.
        return IsListItem(text) ? wrapped + BlockGap : wrapped;
    }

    private static bool IsListItem(string text)
    {
        if (text.Length > 1 && text[0] is '-' or '*' or '+' && text[1] == ' ') return true;

        var digits = 0;
        while (digits < text.Length && char.IsAsciiDigit(text[digits])) digits++;
        return digits > 0 && digits + 1 < text.Length && text[digits] is '.' or ')' && text[digits + 1] == ' ';
    }
}
