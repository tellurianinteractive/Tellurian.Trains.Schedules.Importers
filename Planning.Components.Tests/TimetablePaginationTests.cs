using Tellurian.Trains.Schedules.Planning.Components.Reporting;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies how <see cref="TimetablePaginator"/> fills a page and how it assigns headings across pages:
/// a table+direction broken onto a new page (whether by a new column group or a row slice) gains the
/// ", continued" marker, while column groups stacked together on one page read under a single heading.
/// </summary>
[TestClass]
public class TimetablePaginationTests
{
    private static readonly PageGeometry Geometry = PageGeometry.A4Landscape;

    // One table with the given number of train columns and rows. More than ColumnsPerPage columns forces
    // several column groups; the row count controls each group's height.
    private static TimetableTable Table(int columns, int rows, int tableNumber = 1, string title = "1: A-B") => new()
    {
        Title = title,
        TableNumber = tableNumber,
        Columns = [.. Enumerable.Range(0, columns).Select(i => new TimetableTableColumn($"T{i}", IsPassenger: false, Sessions.All))],
        Rows = [.. Enumerable.Range(0, rows).Select(i =>
            new TimetableTableRow(i.ToString(), $"S{i}",
                [.. Enumerable.Repeat(TimetableTimeCell.Empty, columns)]))],
    };

    [TestMethod]
    public void BothDirectionsOfOneStretch_ShareOnePage_WhenTheyFit()
    {
        // The reported case: a stretch of 18 rows in each direction. Each tile is 8.4 + 2.9 + 18×4.2 =
        // 86.9 mm, so the pair needs 173.8 mm of the 177.4 mm a page has once the bottom margin is kept
        // clear — they belong on one page, and that is what the sheet measures (172.1 mm of 190 mm).
        // This is also the tightest fit the geometry has to hold, so it is what the row spacing is
        // calibrated against: leading that puts this pair over the usable height does not merely print
        // looser, it costs the commonest sheet in the timetable a second page.
        var pages = TimetablePaginator.BuildPages(
            [Table(columns: 10, rows: 18, title: "1: A-B"), Table(columns: 10, rows: 18, title: "1: B-A")],
            Geometry);

        Assert.AreEqual(1, pages.Count, "Both directions of an 18-row stretch fit one page.");
        Assert.AreEqual(2, pages[0].Tiles.Count);
    }

    [TestMethod]
    public void BothDirectionsOfOneStretch_GetSeparatePages_WhenTheyDoNotFit()
    {
        // 30 rows each: 137.3 mm a tile, so the pair cannot share a page and each direction gets its own.
        var pages = TimetablePaginator.BuildPages(
            [Table(columns: 10, rows: 30, title: "1: A-B"), Table(columns: 10, rows: 30, title: "1: B-A")],
            Geometry);

        Assert.AreEqual(2, pages.Count);
        Assert.AreEqual("1: A-B", pages[0].Tiles.Single().Table.Title);
        Assert.AreEqual("1: B-A", pages[1].Tiles.Single().Table.Title);
    }

    [TestMethod]
    public void NoPageIsPackedBeyondTheUsableHeight()
    {
        // The calibration guard. The estimate is only worth having if the packer never exceeds it: an
        // overfull page does not print short, it prints past the foot of the sheet. Swept across the row
        // counts a real stretch produces and across one, two and three column groups.
        foreach (var columns in new[] { 8, 15, 20, 40 })
        {
            foreach (var rows in Enumerable.Range(1, 60))
            {
                var pages = TimetablePaginator.BuildPages([Table(columns, rows)], Geometry);
                foreach (var page in pages)
                {
                    var height = PackedHeight(page);
                    Assert.IsTrue(height <= Geometry.UsableHeightMm + 1e-9,
                        $"{columns} columns × {rows} rows packed a page to {height:F1} mm, over the {Geometry.UsableHeightMm:F1} mm a page has.");
                }
            }
        }
    }

    [TestMethod]
    public void SmallStretches_ShareOnePage_SideBySide()
    {
        // Three branch lines, five trains and ten stations each: a block is 120 mm wide and 106.6 mm tall,
        // so two stand beside each other within the 277 mm of the sheet but a second one will not go under
        // the first. The third has nowhere left to go and opens a new page.
        var pages = TimetablePaginator.BuildPages(Stretches(count: 3, columns: 5, rows: 10), Geometry);

        Assert.AreEqual(2, pages.Count);
        Assert.AreEqual(2, pages[0].Columns.Count, "The second stretch should stand beside the first.");
        Assert.AreEqual("1: A-B", pages[0].Columns[0].Tiles[0].Table.Title);
        Assert.AreEqual("2: A-B", pages[0].Columns[1].Tiles[0].Table.Title);
        Assert.AreEqual("3: A-B", pages[1].Columns.Single().Tiles[0].Table.Title);
    }

    [TestMethod]
    public void SmallStretches_StackWithinAColumn_BeforeANewColumnIsOpened()
    {
        // Six-station branch lines: a block is 73 mm, so two fit in a column and the column is filled before
        // one is opened beside it — which is what keeps the blocks in stretch order when read column by column.
        var pages = TimetablePaginator.BuildPages(Stretches(count: 4, columns: 5, rows: 6), Geometry);

        Assert.AreEqual(1, pages.Count);
        Assert.AreEqual(2, pages[0].Columns.Count);
        CollectionAssert.AreEqual(
            new[] { "1: A-B", "1: B-A", "2: A-B", "2: B-A" },
            pages[0].Columns[0].Tiles.Select(tile => tile.Table.Title).ToArray());
        CollectionAssert.AreEqual(
            new[] { "3: A-B", "3: B-A", "4: A-B", "4: B-A" },
            pages[0].Columns[1].Tiles.Select(tile => tile.Table.Title).ToArray());
    }

    [TestMethod]
    public void AStretchTooBigForOnePage_KeepsItsPagesToItself()
    {
        // A 30-row stretch needs a page per direction. It must break the page it meets and take nothing
        // along, so the small stretches on either side of it keep their own sheets.
        var pages = TimetablePaginator.BuildPages(
        [
            Table(columns: 5, rows: 6, tableNumber: 1, title: "1: A-B"),
            Table(columns: 5, rows: 6, tableNumber: 1, title: "1: B-A"),
            Table(columns: 10, rows: 30, tableNumber: 2, title: "2: C-D"),
            Table(columns: 10, rows: 30, tableNumber: 2, title: "2: D-C"),
            Table(columns: 5, rows: 6, tableNumber: 3, title: "3: E-F"),
            Table(columns: 5, rows: 6, tableNumber: 3, title: "3: F-E"),
        ], Geometry);

        Assert.AreEqual(4, pages.Count);
        CollectionAssert.AreEqual(new[] { "1: A-B", "1: B-A" }, Titles(pages[0]));
        CollectionAssert.AreEqual(new[] { "2: C-D" }, Titles(pages[1]));
        CollectionAssert.AreEqual(new[] { "2: D-C" }, Titles(pages[2]));
        CollectionAssert.AreEqual(new[] { "3: E-F", "3: F-E" }, Titles(pages[3]));
    }

    [TestMethod]
    public void NoPageIsPackedBeyondTheUsableWidthOrHeight()
    {
        // The two-dimensional calibration guard, swept over runs of stretches of every shape the report
        // produces. A page that overflows sideways is the worse failure of the two: the browser does not
        // shrink it, it moves the surplus onto a sheet of its own.
        foreach (var columns in new[] { 2, 5, 8, 15, 20 })
        {
            foreach (var rows in new[] { 1, 4, 9, 18, 26, 45 })
            {
                var pages = TimetablePaginator.BuildPages(Stretches(count: 7, columns, rows), Geometry);
                foreach (var page in pages)
                {
                    var width = page.Columns.Sum(column => column.WidthMm)
                        + Geometry.ColumnGapMm * (page.Columns.Count - 1);
                    Assert.IsTrue(width <= Geometry.PrintableWidthMm + 1e-9,
                        $"{columns} columns × {rows} rows packed a page {width:F1} mm wide, over the {Geometry.PrintableWidthMm:F1} mm a page has.");
                    Assert.IsTrue(PackedHeight(page) <= Geometry.UsableHeightMm + 1e-9,
                        $"{columns} columns × {rows} rows packed a page to {PackedHeight(page):F1} mm, over the {Geometry.UsableHeightMm:F1} mm a page has.");
                    foreach (var column in page.Columns)
                        Assert.IsTrue(column.Tiles.Max(TileWidthMm) <= column.WidthMm + 1e-9,
                            "A column must reserve the width of its widest tile.");
                }
            }
        }
    }

    // A run of identically shaped stretches, both directions each, numbered from 1.
    private static IReadOnlyList<TimetableTable> Stretches(int count, int columns, int rows) =>
    [
        .. Enumerable.Range(1, count).SelectMany(number => new[]
        {
            Table(columns, rows, number, $"{number}: A-B"),
            Table(columns, rows, number, $"{number}: B-A"),
        })
    ];

    private static string[] Titles(TimetablePage page) => [.. page.Tiles.Select(tile => tile.Table.Title)];

    private static double TileWidthMm(TimetableTile tile) =>
        Geometry.FixedColumnsWidthMm + tile.Table.Columns.Count * Geometry.TrainColumnWidthMm;

    [TestMethod]
    public void ColumnGroupsOnSeparatePages_GetContinuedHeading()
    {
        // 20 columns -> two column groups (15 + 5); 26 rows makes each group's tile 120.5 mm, so even with
        // the heading dropped from the second the two cannot share one page and the second is pushed to a
        // new page.
        var pages = TimetablePaginator.BuildPages([Table(columns: 20, rows: 26)], Geometry);

        Assert.AreEqual(2, pages.Count, "The two column groups should occupy two pages.");
        Assert.IsTrue(pages[0].Tiles[0].ShowTitle);
        Assert.IsFalse(pages[0].Tiles[0].IsContinued, "The first page heading is not a continuation.");
        Assert.IsTrue(pages[1].Tiles[0].ShowTitle);
        Assert.IsTrue(pages[1].Tiles[0].IsContinued, "The second page should mark the heading as continued.");
    }

    [TestMethod]
    public void ColumnGroupsOnOnePage_ShareOneHeading()
    {
        // 20 columns -> two column groups, but only 5 rows each, so both groups fit on a single page and
        // the second tile's repeated heading is suppressed (no ", continued").
        var pages = TimetablePaginator.BuildPages([Table(columns: 20, rows: 5)], Geometry);

        Assert.AreEqual(1, pages.Count);
        Assert.AreEqual(2, pages[0].Tiles.Count);
        Assert.IsTrue(pages[0].Tiles[0].ShowTitle);
        Assert.IsFalse(pages[0].Tiles[1].ShowTitle, "The stacked second column group reuses the heading.");
        Assert.IsFalse(pages[0].Tiles[1].IsContinued);
    }

    [TestMethod]
    public void DroppedHeading_IsChargedAsAGapAndKeepsBlocksTogether()
    {
        // Round numbers of its own, because this is about the packing rule rather than the printed
        // geometry: with a 20 mm heading replaced by no gap at all, a 60 mm tile pair needs 120 mm when
        // both headings are charged but only 100 mm when the stacked one is dropped — so a 105 mm page
        // holds the pair only if the saving is taken.
        var geometry = new PageGeometry
        {
            PrintableHeightMm = 105,
            BottomMarginMm = 0,
            TitleHeightMm = 20,
            NoTitleGapMm = 0,
            ColumnHeaderHeightMm = 5,
            RowHeightMm = 5,
            ColumnsPerPage = 15,
        };

        var pages = TimetablePaginator.BuildPages([Table(columns: 20, rows: 7)], geometry);

        Assert.AreEqual(1, pages.Count, "The heading is charged once, so the two blocks fit one page.");
        Assert.AreEqual(2, pages[0].Tiles.Count);
    }

    // What a packed page measures: the tallest of its columns. Within a column every tile costs its own
    // height, except that one repeating the heading of the tile above it drops the heading and pays the
    // no-title gap instead. Mirrors the packer's own rule.
    private static double PackedHeight(TimetablePage page) =>
        page.Columns.Max(ColumnHeight);

    private static double ColumnHeight(TimetablePageColumn column)
    {
        var total = 0.0;
        string? above = null;
        foreach (var tile in column.Tiles)
        {
            total += tile.Table.Title == above
                ? tile.HeightMm - Geometry.TitleHeightMm + Geometry.NoTitleGapMm
                : tile.HeightMm;
            above = tile.Table.Title;
        }
        return total;
    }
}
