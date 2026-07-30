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
/// not. The constants are empirical: they encode what actually fits on an A5 page in this typography and
/// must be re-tuned if the type size or the page margin changes.
/// </para>
/// </remarks>
public static class DutyPagination
{
    /// <summary>Row units available on one A5 page body.</summary>
    /// <remarks>
    /// Already reduced by the continuation marker, which is reserved once per page rather than charged
    /// to a part: whether a part is last on its page is decided <em>by</em> the packing the height feeds
    /// into, so charging it per part would be circular. Reserving it in the budget costs only a slightly
    /// conservative page and errs towards unused space rather than overflow.
    /// </remarks>
    public const double PageBudget = 43;

    /// <summary>Row units for a part's header block: heading, category, extent, limits.</summary>
    /// <remarks>
    /// More than the four printed lines suggest, and deliberately so. The heading alone is set half
    /// again as large as the body text and padded above, and the block is followed by a rule and the
    /// margins around it; charging the lines only left parts running past the foot of the page. The
    /// figure is empirical — measured against a real print, not derived — so it must be re-checked if
    /// the heading size or the page margin changes.
    /// </remarks>
    public const double HeaderHeight = 8;

    /// <summary>Row units a table costs before its data rows: heading, header row and rule.</summary>
    public const double TableOverhead = 3;

    /// <summary>Characters that fit on one full-width note row before it wraps.</summary>
    public const int CharactersPerNoteRow = 80;

    /// <summary>
    /// The height of one train part in row units, counting only what is actually printed: a suppressed
    /// block costs nothing.
    /// </summary>
    /// <param name="part">The part to measure.</param>
    /// <param name="translator">Resolves the limit labels, which decide whether that line is printed.</param>
    public static double HeightOf(DriverDutyPart part, Func<string, string> translator)
    {
        part = part.ValueOrException(nameof(part));

        var height = HeaderHeight;
        if (part.LimitsLine(translator).Length == 0) height -= 1;

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

        double height = TableOverhead + rows.Count;
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
    /// <param name="translator">Resolves labels whose presence affects height.</param>
    /// <returns>The pages in reading order, numbered from 1. Always a multiple of four.</returns>
    public static IReadOnlyList<DutyPage> BuildPages(
        DriverDuty duty, SessionsSettings settings, Func<string, string> translator)
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
            var height = HeightOf(part, translator);

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

            var used = pending.Sum(p => HeightOf(p, translator));
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
