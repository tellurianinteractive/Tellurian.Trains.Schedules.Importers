namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// Turns a booklet's logical pages into the physical sheet order a saddle-stitch printer needs.
/// </summary>
/// <remarks>
/// Deliberately knows nothing about what a page holds: padding to a multiple of four and imposing the
/// order depend only on a page <em>count</em>. That is what lets the driver duty booklets and the
/// general instructions booklet share this step — the second report costs one page-splitter rather than
/// a second pipeline — and it is a pure permutation, so it is unit-testable without a browser.
/// </remarks>
public static class BookletImposition
{
    /// <summary>
    /// The number of blank pages needed to bring <paramref name="contentPages"/> to a multiple of four.
    /// </summary>
    /// <remarks>
    /// Four A5 pages are exactly one A4 landscape sheet, so padding makes every booklet a whole number
    /// of sheets. That is what lets a print-dialogue page range isolate one booklet; without it the last
    /// sheet of one would carry the first pages of the next.
    /// </remarks>
    public static int BlanksNeeded(int contentPages) => (4 - contentPages % 4) % 4;

    /// <summary>
    /// The order in which logical pages are placed on printed sheets, read in fours as front-left,
    /// front-right, back-left, back-right of each sheet.
    /// </summary>
    /// <remarks>
    /// The sheets nest inside one another: the outermost carries the first and last pages, and each
    /// sheet inwards moves one page in from each end — hence <c>1 + 2i</c> walking forwards and
    /// <c>N − 2i</c> walking backwards. A formula rather than a lookup table, so it extends to any
    /// multiple of four.
    /// </remarks>
    /// <param name="pageCount">The number of logical pages; must be a multiple of four.</param>
    public static IEnumerable<int> PageOrder(int pageCount)
    {
        for (var i = 0; i < pageCount / 4; i++)
        {
            yield return pageCount - 2 * i;      // front left
            yield return 1 + 2 * i;              // front right
            yield return 2 + 2 * i;              // back left
            yield return pageCount - 1 - 2 * i;  // back right
        }
    }

    /// <summary>
    /// Reorders logical pages into booklet order and pairs them into sheet sides.
    /// </summary>
    /// <typeparam name="T">The page type; imposition never looks inside it.</typeparam>
    /// <param name="pages">The logical pages, in reading order.</param>
    /// <returns>The sides to print, each holding the two A5 pages placed side by side.</returns>
    public static IReadOnlyList<SheetSide<T>> Impose<T>(IReadOnlyList<T> pages)
    {
        pages = pages.ValueOrException(nameof(pages));
        var ordered = PageOrder(pages.Count).Select(number => pages[number - 1]).ToList();
        var sides = new List<SheetSide<T>>(ordered.Count / 2);
        for (var i = 0; i < ordered.Count; i += 2)
            sides.Add(new SheetSide<T>(ordered[i], ordered[i + 1]));
        return sides;
    }
}

/// <summary>
/// One side of a printed A4 landscape sheet: two A5 portrait pages side by side.
/// </summary>
/// <remarks>
/// Two A5 portrait pages are 296 × 210 mm against A4 landscape's 297 × 210 mm — an exact fit. The
/// duplex flip must be on the <em>short</em> edge, because the fold runs vertically down the middle of
/// the sheet, so the back must be the mirror of the front about that axis.
/// </remarks>
/// <typeparam name="T">The page type.</typeparam>
/// <param name="Left">The page on the left half.</param>
/// <param name="Right">The page on the right half.</param>
public sealed record SheetSide<T>(T Left, T Right);
