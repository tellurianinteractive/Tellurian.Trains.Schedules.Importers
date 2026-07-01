using Tellurian.Trains.Schedules.Planning.Components.Reporting;

namespace Tellurian.Trains.Schedules.Planning.Components.Tests;

/// <summary>
/// Verifies how <see cref="TimetablePaginator"/> assigns headings across pages: a table+direction broken
/// onto a new page (whether by a new column group or a row slice) gains the ", continued" marker, while
/// column groups stacked together on one page read under a single heading.
/// </summary>
[TestClass]
public class TimetablePaginationTests
{
    private static readonly PageGeometry Geometry = PageGeometry.A4Landscape;

    // One table with the given number of train columns and rows. More than ColumnsPerPage columns forces
    // several column groups; the row count controls each group's height.
    private static TimetableTable Table(int columns, int rows) => new()
    {
        Title = "1: A-B",
        TableNumber = 1,
        Columns = [.. Enumerable.Range(0, columns).Select(i => new TimetableTableColumn($"T{i}", IsPassenger: false))],
        Rows = [.. Enumerable.Range(0, rows).Select(i =>
            new TimetableTableRow(i.ToString(), $"S{i}",
                [.. Enumerable.Repeat(TimetableTimeCell.Empty, columns)]))],
    };

    [TestMethod]
    public void ColumnGroupsOnSeparatePages_GetContinuedHeading()
    {
        // 20 columns -> two column groups (15 + 5); 26 rows makes each group's tile ~103 mm, so even with
        // the title charged only once the two groups cannot share one 190 mm page and the second is
        // pushed to a new page.
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
    public void TitleCountedOncePerPage_KeepsBlocksTogether()
    {
        // 24 rows per group: charging each block a full header would need 24 + 7×24 = 192 mm (two pages),
        // but with the title counted once it is 18 + 7×24 = 186 mm, so both column groups stay on one page.
        var pages = TimetablePaginator.BuildPages([Table(columns: 20, rows: 24)], Geometry);

        Assert.AreEqual(1, pages.Count, "The title is charged once, so the two blocks fit one page.");
        Assert.AreEqual(2, pages[0].Tiles.Count);
    }
}
