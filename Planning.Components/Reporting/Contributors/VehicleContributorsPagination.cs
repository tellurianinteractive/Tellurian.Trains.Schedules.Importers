namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Contributors;

/// <summary>
/// Page geometry for the Vehicle contributors report on A4 landscape. Measurements are millimetres, and
/// deterministic, so that pagination is arithmetic rather than measuring and can be tested.
/// </summary>
/// <remarks>
/// <para>
/// Nothing is estimated from the width of a text. The browser makes every column but the notes as wide as the
/// widest text it holds — its heading included — on one line, and gives the notes what is left. Notes are
/// expected to be short, so each is counted as one line. The heights are pinned in
/// <c>VehicleContributorsTable.razor.css</c> — the heading's box, the column-header row and the line height — and
/// must be calibrated together with the values here. A constant that no longer matches does not print short: it
/// prints past the foot of the page, where the row that fell off is a vehicle nobody sets up.
/// </para>
/// </remarks>
public sealed record VehicleContributorsPageGeometry
{
    /// <summary>Printable height of one page (A4 landscape: 210 − 2 × 10 mm margin).</summary>
    public double PrintableHeightMm { get; init; } = 190;

    /// <summary>
    /// The heading: the name of the station or participant, with the report and the plan it comes from beside
    /// it. Printed on every page, since the pile is torn apart and handed to different people.
    /// </summary>
    public double HeadingHeightMm { get; init; } = 20;

    /// <summary>The column-header row, one line, repeated on every page.</summary>
    public double ColumnHeaderHeightMm { get; init; } = 6;

    /// <summary>Heading plus column headers: what every page spends before its first row.</summary>
    public double HeaderHeightMm => HeadingHeightMm + ColumnHeaderHeightMm;

    /// <summary>Height of a row with at most one note.</summary>
    public double RowHeightMm { get; init; } = 6.3;

    /// <summary>Height each further note adds.</summary>
    public double LineHeightMm { get; init; } = 5;

    /// <summary>Default geometry for an A4 landscape page with 10 mm margins.</summary>
    public static VehicleContributorsPageGeometry A4Landscape { get; } = new();
}

/// <summary>One printed page of the Vehicle contributors report.</summary>
/// <param name="Group">The group the page belongs to; its name heads every one of its pages.</param>
/// <param name="Lines">The rows on this page.</param>
/// <param name="IsContinued">Whether an earlier page already printed the start of the group.</param>
public sealed record VehicleContributorsPage(
    VehicleContributorsGroup Group,
    IReadOnlyList<VehicleContributorLine> Lines,
    bool IsContinued);

/// <summary>Splits the groups of the Vehicle contributors report into printed pages.</summary>
/// <remarks>
/// Every group starts on a page of its own: a station's sheet goes to the station's owner and a participant's to
/// the participant, so a page carrying the end of one and the start of the next belongs to neither. A group too
/// long for one page continues on the next, under the same heading.
/// </remarks>
public static class VehicleContributorsPaginator
{
    /// <summary>Builds the pages of the whole report.</summary>
    /// <param name="groups">The groups, in the order they are printed.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static IReadOnlyList<VehicleContributorsPage> BuildPages(
        IEnumerable<VehicleContributorsGroup> groups, VehicleContributorsPageGeometry geometry)
    {
        groups = groups.ValueOrException(nameof(groups));
        geometry = geometry.ValueOrException(nameof(geometry));

        var pages = new List<VehicleContributorsPage>();
        foreach (var group in groups)
        {
            var isContinued = false;
            foreach (var lines in SplitIntoPages(group, geometry))
            {
                pages.Add(new VehicleContributorsPage(group, lines, isContinued));
                isContinued = true;
            }
        }
        return pages;
    }

    /// <summary>
    /// The height of one row: a line, plus a line for each further note. Nothing else in a row wraps, each note
    /// starts a line of its own, and a note is short enough to fit on it.
    /// </summary>
    /// <param name="line">The row.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double HeightMmOf(VehicleContributorLine line, VehicleContributorsPageGeometry geometry)
    {
        line = line.ValueOrException(nameof(line));
        geometry = geometry.ValueOrException(nameof(geometry));

        return geometry.RowHeightMm + (Math.Max(line.Notes.Count, 1) - 1) * geometry.LineHeightMm;
    }

    // Fills pages with rows until the next one would overflow, always placing at least one row per page so that
    // a row taller than the page is still printed rather than looping forever.
    private static IEnumerable<IReadOnlyList<VehicleContributorLine>> SplitIntoPages(
        VehicleContributorsGroup group, VehicleContributorsPageGeometry geometry)
    {
        var available = geometry.PrintableHeightMm - geometry.HeaderHeightMm;
        var current = new List<VehicleContributorLine>();
        var used = 0.0;

        foreach (var line in group.Lines)
        {
            var height = HeightMmOf(line, geometry);
            if (current.Count > 0 && used + height > available)
            {
                yield return current;
                current = [];
                used = 0;
            }
            current.Add(line);
            used += height;
        }
        if (current.Count > 0) yield return current;
    }
}
