namespace Tellurian.Trains.Schedules.Planning.Components.Reporting;

/// <summary>
/// Page geometry used to estimate how a <see cref="TimetableTable"/> is split across printed
/// pages. Measurements are millimetres (fractional allowed); the values are deliberately deterministic so
/// pagination is pure arithmetic (no DOM measuring) and unit-testable.
/// <para>
/// Every value below is <em>measured off a rendered page</em> and then rounded up a hair, not derived
/// from the type size — deriving them is what got the row height wrong before. The geometry they measure
/// is pinned in <c>TimetableStretchTable.razor.css</c>, which states it for screen and paper alike so the
/// on-screen A4 sheet is a true preview. Calibrate the constants there and here together: a constant that
/// no longer matches does not print short, it prints past the foot of the page.
/// </para>
/// </summary>
public sealed record PageGeometry
{
    /// <summary>Printable height of one page in millimetres (A4 landscape: 210 − 2×10 margin).</summary>
    public double PrintableHeightMm { get; init; } = 190;

    /// <summary>
    /// Blank space kept at the foot of every page, so an estimate that drifts by a line or two still lands
    /// on the paper. Three table lines (3 × <see cref="RowHeightMm"/>, spelled out because a property
    /// initializer cannot read another one): enough to absorb a rounding error, small enough not to cost a page.
    /// </summary>
    public double BottomMarginMm { get; init; } = 3 * 4.2;

    /// <summary>Height a page has for content once the bottom margin is kept clear.</summary>
    public double UsableHeightMm => PrintableHeightMm - BottomMarginMm;

    /// <summary>
    /// Height of the stretch title (the <c>h3</c> at 10&#160;pt, plus its 2&#160;mm/1&#160;mm margins;
    /// measured 8.29). It prints once per page: a stacked column-group block that repeats a heading
    /// already shown on the page drops its title and pays <see cref="NoTitleGapMm"/> instead.
    /// </summary>
    public double TitleHeightMm { get; init; } = 8.4;

    /// <summary>
    /// The gap a block gets in place of a heading it does not repeat
    /// (<c>.timetable-section.no-title</c>'s top margin), so stacked column groups stay apart.
    /// </summary>
    public double NoTitleGapMm { get; init; } = 2.5;

    /// <summary>Height of the column-header row, printed by every block (it repeats in each column group; measured 2.86).</summary>
    public double ColumnHeaderHeightMm { get; init; } = 2.9;

    /// <summary>Height of the operating sessions/days row, printed under the header when shown (measured 2.86).</summary>
    public double SessionsRowHeightMm { get; init; } = 2.9;

    /// <summary>Title plus column-header row: the full header height a block prints when it shows its title.</summary>
    public double HeaderHeightMm => TitleHeightMm + ColumnHeaderHeightMm;

    /// <summary>
    /// Height of one printed table line in millimetres (measured 4.16). A split row (arrival + departure)
    /// costs two of these. The line spacing that produces it is set in the CSS and is pushed as far as the
    /// page budget allows: one notch more leading and a two-direction 18-row stretch stops fitting a sheet.
    /// </summary>
    public double RowHeightMm { get; init; } = 4.2;

    /// <summary>Maximum number of train columns per page; the three fixed columns are repeated in addition.</summary>
    public int ColumnsPerPage { get; init; } = 15;

    /// <summary>Default geometry for an A4 landscape page with 10&#160;mm margins.</summary>
    public static PageGeometry A4Landscape { get; } = new();
}

/// <summary>
/// One printable fragment of a timetable: a subset of columns (one horizontal column group) and a
/// subset of rows (one vertical slice) of a <see cref="TimetableTable"/>, sized to fit one page.
/// The three fixed columns (km / arr-dep / stations) are always part of <see cref="Table"/>.
/// </summary>
/// <param name="Table">The sliced table data to render. Its <c>Columns</c> and each row's <c>Cells</c> are aligned to this tile's column group.</param>
/// <param name="IsContinued">True when this tile continues the same table from the previous page; the title is then suffixed with ", continued".</param>
/// <param name="HeightMm">The estimated printed height of this tile, including its full header (title shown).</param>
/// <param name="ShowTitle">False when the tile directly above on the same page already shows the identical heading text, so the repeat is dropped.</param>
public sealed record TimetableTile(TimetableTable Table, bool IsContinued, double HeightMm, bool ShowTitle = true);

/// <summary>One physical printed page holding one or more <see cref="TimetableTile"/>s stacked vertically.</summary>
/// <param name="Tiles">The tiles on this page, in render order.</param>
public sealed record TimetablePage(IReadOnlyList<TimetableTile> Tiles);

/// <summary>
/// Splits timetable tables into page-sized tiles and packs those tiles onto physical pages.
/// Horizontal breaking: train columns are grouped in chunks of <see cref="PageGeometry.ColumnsPerPage"/>,
/// each group rendered with the three fixed columns repeated. Vertical breaking: rows are filled until
/// the next row would overflow the usable height, then a new tile starts with the header repeated.
/// Packing: tiles flow onto a page until the next tile would overflow, with a hard page break whenever
/// the stretch (<see cref="TimetableTable.TableNumber"/>) changes — so both directions of one stretch
/// share a page when they fit.
/// </summary>
public static class TimetablePaginator
{
    /// <summary>Splits the given tables into tiles, then packs the tiles onto pages.</summary>
    public static IReadOnlyList<TimetablePage> BuildPages(IEnumerable<TimetableTable> tables, PageGeometry geometry)
    {
        var pages = new List<TimetablePage>();

        // Each table is one direction; tables sharing a TableNumber are the directions of one stretch.
        foreach (var stretch in tables.GroupBy(table => table.TableNumber))
        {
            var directions = stretch.Select(table => SplitIntoTiles(table, geometry).ToList()).ToList();
            // Both directions on one notional page: same-title column groups stacked together print their
            // title only once, so estimate with that saving rather than charging every block a full header.
            var stretchHeight = PackedHeight(directions.SelectMany(direction => direction), geometry);

            if (stretchHeight <= geometry.UsableHeightMm)
            {
                // The whole stretch fits on one page: keep both directions together on it.
                pages.AddRange(PackTiles(directions.SelectMany(direction => direction), geometry));
            }
            else
            {
                // It doesn't fit: give each direction its own page break so the two never share a page.
                foreach (var direction in directions)
                    pages.AddRange(PackTiles(direction, geometry));
            }
        }

        return NormalizeHeadings(pages);
    }

    // Final pass over the packed pages deciding which tiles print a heading and which gain ", continued".
    // A tile prints its heading only the first time its title appears on a page, so stacked column groups
    // of one table read under a single heading. That heading gains the ", continued" suffix whenever the
    // same table+direction already appeared on an earlier page — covering a direction broken across pages
    // both horizontally (column groups) and vertically (row slices). This is the sole authority for
    // ShowTitle/IsContinued; any provisional values from tiling are overwritten here.
    private static IReadOnlyList<TimetablePage> NormalizeHeadings(List<TimetablePage> pages)
    {
        var seenOnEarlierPage = new HashSet<string>();
        var result = new List<TimetablePage>(pages.Count);
        foreach (var page in pages)
        {
            var shownOnThisPage = new HashSet<string>();
            var tiles = new List<TimetableTile>(page.Tiles.Count);
            foreach (var tile in page.Tiles)
            {
                var title = tile.Table.Title;
                var showTitle = shownOnThisPage.Add(title);
                var continued = seenOnEarlierPage.Contains(title);
                tiles.Add(tile with { ShowTitle = showTitle, IsContinued = continued });
            }
            seenOnEarlierPage.UnionWith(shownOnThisPage);
            result.Add(new TimetablePage(tiles));
        }
        return result;
    }

    /// <summary>
    /// Splits one table into tiles: first into horizontal column groups, then each group vertically
    /// into row slices that fit within one page once the header is accounted for.
    /// </summary>
    internal static IEnumerable<TimetableTile> SplitIntoTiles(TimetableTable table, PageGeometry geometry)
    {
        var columnCount = table.Columns.Count;
        for (var start = 0; start < columnCount; start += geometry.ColumnsPerPage)
        {
            var count = Math.Min(geometry.ColumnsPerPage, columnCount - start);
            var columns = table.Columns.Skip(start).Take(count).ToList();
            // Project each row onto this column group. Slicing the cells means IsSplit is re-evaluated
            // for this group only, so a station counts as a single row when no train in the group stops there.
            var rows = table.Rows
                .Select(row => row with { Cells = row.Cells.Skip(start).Take(count).ToList() })
                .ToList();

            var available = geometry.UsableHeightMm - geometry.HeaderHeightMm - SessionsRowHeightMm(table, geometry);
            var current = new List<TimetableTableRow>();
            var used = 0.0;

            // IsContinued is settled later in NormalizeHeadings (it depends on page placement), so tiles
            // are emitted with the default here.
            foreach (var row in rows)
            {
                var height = RowHeightMm(row, geometry);
                // Break before this row when it would overflow, but always keep at least one row per tile
                // so a row taller than the page still gets placed (and we never loop forever).
                if (current.Count > 0 && used + height > available)
                {
                    yield return MakeTile(table, columns, current, geometry);
                    current = [];
                    used = 0;
                }
                current.Add(row);
                used += height;
            }

            if (current.Count > 0)
                yield return MakeTile(table, columns, current, geometry);
        }
    }

    // Height of a run of tiles stacked on one page: each tile costs its full height, except that one
    // repeating the heading of the tile immediately above it (a stacked column group of the same table)
    // drops that heading and pays the smaller no-title gap in its place.
    private static double PackedHeight(IEnumerable<TimetableTile> tiles, PageGeometry geometry)
    {
        var total = 0.0;
        string? above = null;
        foreach (var tile in tiles)
        {
            total += tile.Table.Title == above ? StackedHeight(tile, geometry) : tile.HeightMm;
            above = tile.Table.Title;
        }
        return total;
    }

    // What a tile costs when it is stacked under a block with the same heading: its own heading is not
    // printed, so the title comes off and the gap that replaces it goes on.
    private static double StackedHeight(TimetableTile tile, PageGeometry geometry) =>
        tile.HeightMm - geometry.TitleHeightMm + geometry.NoTitleGapMm;

    /// <summary>
    /// Packs a single packing unit (a whole stretch when both directions fit on one page, otherwise one
    /// direction) onto pages, starting a new page whenever the next tile would overflow the usable height.
    /// A tile stacked under a same-title block reuses the page's single title, so it is charged the smaller
    /// <see cref="PageGeometry.NoTitleGapMm"/> in place of <see cref="PageGeometry.TitleHeightMm"/>; the
    /// first tile on a page always shows (and pays for) its title.
    /// </summary>
    internal static IReadOnlyList<TimetablePage> PackTiles(IEnumerable<TimetableTile> tiles, PageGeometry geometry)
    {
        var pages = new List<TimetablePage>();
        var current = new List<TimetableTile>();
        var used = 0.0;

        foreach (var tile in tiles)
        {
            var savesTitle = current.Count > 0 && tile.Table.Title == current[^1].Table.Title;
            var height = savesTitle ? StackedHeight(tile, geometry) : tile.HeightMm;
            if (current.Count > 0 && used + height > geometry.UsableHeightMm)
            {
                pages.Add(BuildPage(current));
                current = [];
                used = 0;
                height = tile.HeightMm; // first on the new page shows its title again
            }
            current.Add(tile);
            used += height;
        }

        if (current.Count > 0)
            pages.Add(BuildPage(current));

        return pages;
    }

    // Assemble a page from its packed tiles. Heading visibility and the ", continued" suffix are decided
    // later, across all pages, in NormalizeHeadings — the height estimate keeps a hidden title's few
    // millimetres of slack, which can only under-fill a page, never overflow it.
    private static TimetablePage BuildPage(List<TimetableTile> tiles) => new(tiles);

    private static TimetableTile MakeTile(
        TimetableTable source,
        IReadOnlyList<TimetableTableColumn> columns,
        IReadOnlyList<TimetableTableRow> rows,
        PageGeometry geometry)
    {
        var table = new TimetableTable
        {
            Title = source.Title,
            Columns = columns,
            Rows = rows,
            TableNumber = source.TableNumber,
            UseDays = source.UseDays,
            MaxSessions = source.MaxSessions,
            StartDay = source.StartDay,
            ShowSessionsRow = source.ShowSessionsRow,
        };
        var height = geometry.HeaderHeightMm + SessionsRowHeightMm(source, geometry) + rows.Sum(row => RowHeightMm(row, geometry));
        return new TimetableTile(table, IsContinued: false, height);
    }

    // A split row prints two lines (arrival + departure); a plain row prints one.
    private static double RowHeightMm(TimetableTableRow row, PageGeometry geometry) =>
        (row.IsSplit ? 2 : 1) * geometry.RowHeightMm;

    // The operating sessions/days row, when shown, adds one line to the repeated header. It is set
    // smaller than a data row, so it has its own constant.
    private static double SessionsRowHeightMm(TimetableTable table, PageGeometry geometry) =>
        table.ShowSessionsRow ? geometry.SessionsRowHeightMm : 0;
}
