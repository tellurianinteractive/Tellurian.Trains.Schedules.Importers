namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Instructions;

/// <summary>What one page of the general instructions booklet holds.</summary>
public enum InstructionsPageKind
{
    /// <summary>Page 1: the meeting name, validity dates, print date, and the programme.</summary>
    Front,

    /// <summary>Authored markdown, under the "Instructions" heading.</summary>
    Content,

    /// <summary>The layout topology and shunting yards, under the "Layout" heading — appended for
    /// readers who hold no duty booklet.</summary>
    Overview,

    /// <summary>Padding, so the page count is a multiple of four.</summary>
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
/// Simpler than the duty booklet's pagination: the content is a single flow, so pages are filled by
/// splitting it at block boundaries rather than by packing indivisible items. The front page is fixed
/// at page 1 and the layout overview is fixed as the last page before padding, so the only variable is
/// how many content pages the authored instructions need in between.
/// </remarks>
public static class InstructionsPagination
{
    /// <summary>Text lines that fit on one A5 page body.</summary>
    /// <remarks>
    /// Empirical, like the duty booklet's budget: it encodes what actually fits in this typography and
    /// must be re-tuned if the type size or the page margin changes.
    /// </remarks>
    public const int PageBudget = 44;

    /// <summary>Characters that fit on one line before the text wraps.</summary>
    public const int CharactersPerLine = 60;

    /// <summary>
    /// The tallest a trailing block may be and still count as "short" for the stranded-block rule
    /// below. Empirical, like the other constants here.
    /// </summary>
    public const int ShortTrailingBlockHeight = 4;

    /// <summary>
    /// The smallest gap, in row units, left below a short trailing block that counts as "a lot of
    /// blank space" for the stranded-block rule below. A quarter of the page: less than that reads as
    /// a normally-filled page rather than one visibly cut short.
    /// </summary>
    public const double StrandedBlockGap = PageBudget * 0.25;

    /// <summary>
    /// Builds the booklet's pages: the front page, the instructions split across content pages, the
    /// layout overview, and blank padding to a whole number of sheets.
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
        var used = 0;
        var firstContentPageEmitted = false;

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var height = HeightOf(block);

            // A block taller than a page still prints, overflowing, rather than being truncated: the
            // author is the only one who knows what can go, so the report's job is to make it visible.
            if (current.Count > 0 && used + height > PageBudget) Flush();

            current.Add(block);
            used += height;

            // Avoid stranding a short block alone at the end of a page with a lot of blank space below
            // it, while the next block is too big to join it here and moves entirely to the next page
            // regardless. The page reads better ending on the fuller block before this one, with this
            // one carried over to join what follows instead of trailing behind on its own.
            var hasNext = i + 1 < blocks.Count;
            var nextFitsHere = hasNext && used + HeightOf(blocks[i + 1]) <= PageBudget;
            if (hasNext && !nextFitsHere && current.Count > 1 &&
                height <= ShortTrailingBlockHeight && PageBudget - used >= StrandedBlockGap)
            {
                current.RemoveAt(current.Count - 1);
                used -= height;
                Flush();
                current.Add(block);
                used = height;
            }
        }
        Flush();

        if (includeOverview)
            pages.Add(new InstructionsPage(pages.Count + 1, InstructionsPageKind.Overview));

        var blanks = BookletImposition.BlanksNeeded(pages.Count);
        for (var i = 0; i < blanks; i++)
            pages.Add(new InstructionsPage(pages.Count + 1, InstructionsPageKind.Blank));

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
        for (var i = 0; i < paragraphs.Count; i++)
        {
            if (paragraphs[i].StartsWith('#') && i < paragraphs.Count - 1)
            {
                yield return $"{paragraphs[i]}\n\n{paragraphs[i + 1]}";
                i++;
                continue;
            }
            yield return paragraphs[i];
        }
    }

    // Estimated, never measured: a line per source line, plus wrapping charged per character, and one
    // blank line separating blocks.
    private static int HeightOf(string block) =>
        block.Split('\n').Sum(line => 1 + line.Length / CharactersPerLine) + 1;
}
