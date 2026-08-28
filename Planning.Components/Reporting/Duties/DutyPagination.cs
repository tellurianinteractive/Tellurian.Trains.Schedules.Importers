namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Duties;

/// <summary>
/// Builds a duty's logical pages and imposes them onto printed sheets.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline is: estimate part heights, pack them into pages, pad with blanks, append the overview
/// page, then impose. Steps up to the padding produce logical pages numbered 1…N as the driver reads
/// them; imposition reorders those into the physical sequence the printer needs.
/// </para>
/// <para>
/// Heights are <em>estimated</em>, never measured — abstract row units against a fixed page budget. That
/// is deterministic, testable without a browser, and reliable under print preview, where measurement is
/// not. The unit is <em>one table row</em>: 0.85 rem of text at the booklet's line-height of 1.5, plus
/// the cell padding and the row rule — 24.1 px at the 16 px root size, and the same for a vehicle, cargo
/// and timetable row alike. Rows are what a page is mostly made of and what varies from part to part, so
/// they cost exactly 1 and everything else is stated against them.
/// </para>
/// <para>
/// Every constant below is a pixel figure taken from the booklet's CSS (<c>.dutypage</c>,
/// <c>.dutypart</c> in <c>app.css</c>) divided by that 24.1 px and rounded up, so each can be re-derived
/// on its own. They must be re-derived if the type size, the line-height or the page padding changes —
/// a constant that no longer matches the stylesheet does not print short, it prints past the foot of the
/// page, where <c>overflow: hidden</c> takes the rest of the part away without a word.
/// </para>
/// </remarks>
public static class DutyPagination
{
    /// <summary>Row units available on one A5 page body.</summary>
    /// <remarks>
    /// <para>
    /// A5 is 210 mm — 793.7 px — tall, the page body takes 8 mm of padding at the top, and the
    /// continuation marker sits 14 mm from the foot with a 24 px line of its own. What is left for parts
    /// is 793.7 − 30.2 − 52.9 − 24 = 686.6 px, or 28.5 rows.
    /// </para>
    /// <para>
    /// The marker is reserved here rather than charged to a part: whether a part is last on its page is
    /// decided <em>by</em> the packing the height feeds into, so charging it per part would be circular.
    /// </para>
    /// </remarks>
    public const double PageBudget = 28;

    /// <summary>
    /// Row units for a part's header block: heading, category and extent, limits, and the rule closing
    /// the block.
    /// </summary>
    /// <remarks>
    /// 109.7 px, which is 4.55 rows: the heading is set half again as large as the body text and padded
    /// above (49.7 px), the two 0.9 rem lines take 21.6 px each, the rule 1 px, and the collapsed margins
    /// between them the remaining 15.8 px. Taken as 4.75 so the block is never charged short.
    /// </remarks>
    public const double HeaderHeight = 4.75;

    /// <summary>
    /// Row units a vehicle or cargo table costs before its data rows: heading, column headings, and the
    /// rule that closes the block.
    /// </summary>
    /// <remarks>
    /// 61.1 px, which is 2.53 rows: the 0.95 rem heading with its margins (25.1 px), the column headings
    /// (24.1 px — a row like any other) and the closing rule with its margins (11.9 px).
    /// </remarks>
    public const double TableOverhead = 2.75;

    /// <summary>Row units the timetable costs before its rows: heading and column headings.</summary>
    /// <remarks>
    /// 49.2 px, which is 2.04 rows — less than <see cref="TableOverhead"/> because the timetable ends the
    /// part and so carries no closing rule.
    /// </remarks>
    public const double TimetableOverhead = 2.25;

    /// <summary>Characters that fit on one full-width note row before it wraps.</summary>
    /// <remarks>
    /// A full-width note starts at the station column, so it runs the page width less the Arr/Dep column
    /// and its own padding — about 449 px. At 0.85 rem these notes average some 7 px a character, being
    /// full of times, train numbers and capitals, all wider than lower-case prose.
    /// </remarks>
    public const int CharactersPerNoteRow = 65;

    /// <summary>
    /// The height of one train part in row units, counting only what is actually printed: a suppressed
    /// block costs nothing.
    /// </summary>
    /// <param name="part">The part to measure.</param>
    public static double HeightOf(DriverDutyPart part)
    {
        part = part.ValueOrException(nameof(part));

        var height = HeaderHeight;
        // The limits line and the margin under it come to 23.8 px — one row, near enough to state as one.
        if (!part.HasLimits) height -= 1;

        height += TableHeight(part.TractionData.Vehicles.Count);
        height += TableHeight(part.WagonsetData.Vehicles.Count);
        height += TableHeight(part.CargoData.Flows.Count);
        height += TimetableHeight(part);
        return height;
    }

    /// <summary>The height of the timetable block alone, which is what moves on a split part.</summary>
    public static double TimetableHeight(DriverDutyPart part)
    {
        part = part.ValueOrException(nameof(part));
        var rows = part.TimetableRows;
        if (rows.Count == 0) return 0;

        double height = TimetableOverhead + rows.Count;
        foreach (var row in rows)
        {
            // One unit per stacked note — counting rather than measuring, because a full page width
            // holds roughly twice what the note column does and wrapping is now the exception. A note in
            // the column never wraps by construction, so only the full-width rows are charged for it.
            foreach (var note in row.StackedNotes)
                height += 1 + note.ToText.Length / CharactersPerNoteRow;
        }
        return height;
    }

    private static double TableHeight(int rowCount) => rowCount == 0 ? 0 : TableOverhead + rowCount;

    /// <summary>
    /// Builds the logical pages of one duty's booklet: front page, part pages, blank padding, overview.
    /// </summary>
    /// <param name="duty">The duty to print.</param>
    /// <param name="settings">How sessions are displayed.</param>
    /// <returns>The pages in reading order, numbered from 1. Always a multiple of four.</returns>
    public static IReadOnlyList<DutyPage> BuildPages(DriverDuty duty, SessionsSettings settings)
    {
        duty = duty.ValueOrException(nameof(duty));

        var parts = duty.OrderedParts
            .Select(p => new DriverDutyPart { TrainPart = p, Duty = duty, SessionsSettings = settings })
            .ToList();

        var pages = new List<DutyPage> { DutyPage.Front(1, duty) };
        var index = 0;
        var pending = new List<DriverDutyPart>();

        while (index < parts.Count)
        {
            var part = parts[index];
            var height = HeightOf(part);

            if (height > PageBudget)
            {
                // The part will not fit a page on its own, so its timetable moves to the facing page.
                // The two must be a spread — a driver consulting the timetable has to see the tables it
                // points at — and in a saddle-stitch booklet the spreads are (even, even+1). So a split
                // part has to begin on an even page.
                FlushPending();
                // The page it would start on is pages.Count + 1. When that is odd it faces nothing, so
                // the part moves to the next page and this one simply ends early — becoming a blank if
                // nothing was on it, the one case where a blank appears mid-booklet.
                if ((pages.Count + 1) % 2 != 0) pages.Add(DutyPage.Blank(pages.Count + 1, duty));
                pages.Add(DutyPage.SplitTables(pages.Count + 1, duty, part));
                pages.Add(DutyPage.SplitTimetable(pages.Count + 1, duty, part));
                index++;
                continue;
            }

            var used = pending.Sum(p => HeightOf(p));
            if (pending.Count > 0 && used + height > PageBudget) FlushPending();
            pending.Add(part);
            index++;
        }
        FlushPending();

        // The overview page is always last, so blanks go before it rather than at the very end — the
        // count passed in therefore includes the overview page still to come.
        var blanks = BookletImposition.BlanksNeeded(pages.Count + 1);
        for (var i = 0; i < blanks; i++) pages.Add(DutyPage.Blank(pages.Count + 1, duty));
        pages.Add(DutyPage.Overview(pages.Count + 1, duty));

        return pages;

        void FlushPending()
        {
            if (pending.Count == 0) return;
            pages.Add(DutyPage.Part(pages.Count + 1, duty, pending));
            pending = [];
        }
    }

    /// <inheritdoc cref="BookletImposition.PageOrder"/>
    public static IEnumerable<int> BookletPageOrder(int pageCount) => BookletImposition.PageOrder(pageCount);

    /// <inheritdoc cref="BookletImposition.Impose"/>
    public static IReadOnlyList<SheetSide<DutyPage>> Impose(IReadOnlyList<DutyPage> pages) =>
        BookletImposition.Impose(pages);
}
