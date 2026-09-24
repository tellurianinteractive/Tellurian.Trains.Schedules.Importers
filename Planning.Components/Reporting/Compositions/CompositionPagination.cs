using Tellurian.Trains.Schedules.Planning.Components.Reporting.Dispatch;

namespace Tellurian.Trains.Schedules.Planning.Components.Reporting.Compositions;

/// <summary>
/// Page geometry for the Train compositions report on A4 landscape. Measurements are millimetres, and
/// deterministic, so that pagination is arithmetic rather than measuring and can be tested.
/// </summary>
/// <remarks>
/// <para>
/// The stylesheet <c>TrainCompositionTable.razor.css</c> pins every size counted here — the heading box, the
/// column widths, the line heights, and the size of every rectangle — and the two must be calibrated together.
/// A constant that no longer matches does not print short: it prints past the foot of the page, where the row
/// that fell off is a train nobody checks.
/// </para>
/// <para>
/// Every rectangle is as wide as what it holds, so each one's width has to be worked out from its text before
/// the groups can be laid out: a wagon rectangle from its class and number, a cargo rectangle from its longest
/// destination, limit included. Text is charged by the character, biased high, so the estimate wraps no later
/// than the browser does. Only a rectangle clamped to the width of the composition, the destinations within it,
/// and the sessions, limit and turnus columns wrap onto further lines.
/// </para>
/// </remarks>
public sealed record CompositionPageGeometry
{
    /// <summary>Printable height of one page (A4 landscape: 210 − 2 × 10 mm margin).</summary>
    public double PrintableHeightMm { get; init; } = 190;

    /// <summary>Printable width of one page (A4 landscape: 297 − 2 × 10 mm margin).</summary>
    public double PrintableWidthMm { get; init; } = 277;

    /// <summary>
    /// The heading: the station's name, with the report and the plan it comes from beside it. Printed on every
    /// page, because the pile is torn apart and handed to different stations.
    /// </summary>
    public double HeadingHeightMm { get; init; } = 20;

    /// <summary>The column-header row, repeated on every page.</summary>
    public double ColumnHeaderHeightMm { get; init; } = 6.3;

    /// <summary>Heading plus column headers: what every page spends before its first row.</summary>
    public double HeaderHeightMm => HeadingHeightMm + ColumnHeaderHeightMm;

    /// <summary>What a row adds to its content: the cell padding above and below, and its rule.</summary>
    public double RowChromeHeightMm { get; init; } = 2.3;

    /// <summary>One line of text in the train's own columns.</summary>
    public double TextLineHeightMm { get; init; } = 5;

    /// <summary>Combined width of the columns before the composition.</summary>
    /// <remarks>Track 10, sessions 14, arrival 12, departure 12, train 24, destination 32, limit 18, turnus 20.</remarks>
    public double FixedColumnsWidthMm { get; init; } = 142;

    /// <summary>Width of the sessions column, part of <see cref="FixedColumnsWidthMm"/>.</summary>
    public double SessionsColumnWidthMm { get; init; } = 14;

    /// <summary>Width of the turnus column, part of <see cref="FixedColumnsWidthMm"/>.</summary>
    public double TurnusColumnWidthMm { get; init; } = 20;

    /// <summary>Width of the train's limit column, part of <see cref="FixedColumnsWidthMm"/>.</summary>
    public double LimitColumnWidthMm { get; init; } = 18;

    /// <summary>Total horizontal padding inside a cell.</summary>
    public double CellPaddingWidthMm { get; init; } = 2;

    /// <summary>
    /// Width one character of a column's text takes, biased high so a value is over-charged rather than
    /// under-charged: the columns hold figures and capitals, wider than lower-case prose.
    /// </summary>
    public double CharacterWidthMm { get; init; } = 1.75;

    /// <summary>
    /// Width one character of destination text takes, at the smaller type a cargo rectangle is set in.
    /// </summary>
    /// <remarks>
    /// This one decides how wide a rectangle is drawn, not only where its text wraps, so over-charging it
    /// costs the composition a line rather than a rectangle a millimetre. Measured in the browser across
    /// the destinations of the report's test plan at 1.16 to 1.41 mm — a region chip included — and set
    /// ten per cent above the widest of them.
    /// </remarks>
    public double DestinationCharacterWidthMm { get; init; } = 1.55;

    /// <summary>The space between two groups on the same line.</summary>
    public double GroupGapWidthMm { get; init; } = 2;

    /// <summary>The space between two lines of groups.</summary>
    public double GroupGapHeightMm { get; init; } = 1.5;

    /// <summary>
    /// Height of one wagon rectangle: a single line with the class and the number side by side, as tall as a
    /// one-line cargo rectangle.
    /// </summary>
    public double WagonHeightMm { get; init; } = 6.1;

    /// <summary>What a wagon rectangle adds across to its text: its border and side padding.</summary>
    public double WagonChromeWidthMm { get; init; } = 3;

    /// <summary>The space between the class and the number inside a wagon rectangle.</summary>
    public double WagonTextGapMm { get; init; } = 1;

    /// <summary>
    /// The narrowest any rectangle is drawn: about two letters, so a one-letter wagon class or a two-letter
    /// station signature still reads as a rectangle rather than as a mark between two others.
    /// </summary>
    public double RectangleMinWidthMm { get; init; } = 8;

    /// <summary>
    /// Width one character of a wagon class takes. Set in bold capitals, measured at up to 2.33 mm (a "G").
    /// </summary>
    public double WagonClassCharacterWidthMm { get; init; } = 2.4;

    /// <summary>Width one character of a wagon number takes. Mostly digits, measured at 1.47 mm.</summary>
    public double WagonNumberCharacterWidthMm { get; init; } = 1.5;

    /// <summary>The space between two wagon rectangles, across and down.</summary>
    public double WagonGapMm { get; init; } = 1;

    /// <summary>What a cargo rectangle adds across to its text: its border and side padding.</summary>
    public double CargoChromeWidthMm { get; init; } = 2.6;

    /// <summary>What a cargo rectangle adds down to its text: its border and top and bottom padding.</summary>
    public double CargoChromeHeightMm { get; init; } = 1.6;

    /// <summary>One line of destination text in a cargo rectangle.</summary>
    public double DestinationLineHeightMm { get; init; } = 4.5;

    /// <summary>Default geometry for an A4 landscape page with 10 mm margins.</summary>
    public static CompositionPageGeometry A4Landscape { get; } = new();

    /// <summary>The width the groups of a composition are laid out within.</summary>
    public double CompositionWidthMm => PrintableWidthMm - FixedColumnsWidthMm - CellPaddingWidthMm;

    /// <summary>Characters that fit on one line of the sessions column.</summary>
    public int CharactersPerSessionsLine => CharactersWithin(SessionsColumnWidthMm - CellPaddingWidthMm);

    /// <summary>Characters that fit on one line of the turnus column.</summary>
    public int CharactersPerTurnusLine => CharactersWithin(TurnusColumnWidthMm - CellPaddingWidthMm);

    /// <summary>Characters that fit on one line of the limit column.</summary>
    public int CharactersPerLimitLine => CharactersWithin(LimitColumnWidthMm - CellPaddingWidthMm);

    /// <summary>Characters of a column's text that fit on one line of the given width.</summary>
    /// <param name="widthMm">The width available to the text, chrome already taken off.</param>
    public int CharactersWithin(double widthMm) => Math.Max(1, (int)(widthMm / CharacterWidthMm));

    /// <summary>Characters of destination text that fit on one line of the given width.</summary>
    /// <param name="widthMm">The width available to the text, chrome already taken off.</param>
    public int DestinationCharactersWithin(double widthMm) => Math.Max(1, (int)(widthMm / DestinationCharacterWidthMm));
}

/// <summary>One printed page of the Train compositions report.</summary>
/// <param name="Station">The station the page belongs to; its name heads every one of its pages.</param>
/// <param name="Departures">The departures on this page.</param>
/// <param name="IsContinued">Whether an earlier page already printed the start of the station.</param>
public sealed record CompositionPage(
    StationCompositions Station,
    IReadOnlyList<CompositionDeparture> Departures,
    bool IsContinued);

/// <summary>Splits the stations of the Train compositions report into printed pages.</summary>
/// <remarks>
/// Every station starts on a page of its own, because the sheets are handed to different stations. A station
/// too long for one page continues on the next under the same heading. A station with no departures to show
/// gets no page: unlike a dispatch list, where every manned station clears trains, most stations here would
/// only be handed a sheet saying there is nothing to check.
/// </remarks>
public static class CompositionPaginator
{
    /// <summary>Builds the pages of the whole report.</summary>
    /// <param name="stations">The stations, in the order they are printed.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static IReadOnlyList<CompositionPage> BuildPages(
        IEnumerable<StationCompositions> stations, CompositionPageGeometry geometry)
    {
        stations = stations.ValueOrException(nameof(stations));
        geometry = geometry.ValueOrException(nameof(geometry));

        var pages = new List<CompositionPage>();
        foreach (var station in stations)
        {
            var isContinued = false;
            foreach (var departures in SplitIntoPages(station.Departures, geometry))
            {
                pages.Add(new CompositionPage(station, departures, isContinued));
                isContinued = true;
            }
        }
        return pages;
    }

    /// <summary>
    /// The height of one row: the tallest of the columns that wrap — the sessions and the turnus — and the
    /// composition, and the row's chrome.
    /// </summary>
    /// <param name="departure">The row.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double HeightMmOf(CompositionDeparture departure, CompositionPageGeometry geometry)
    {
        departure = departure.ValueOrException(nameof(departure));
        geometry = geometry.ValueOrException(nameof(geometry));

        var sessions = DispatchPaginator.LinesOf(departure.SessionsText, geometry.CharactersPerSessionsLine);
        // One entry per wagonset, each starting on a line of its own and wrapping within the column.
        var turnus = departure.TurnusTexts.Sum(text => DispatchPaginator.LinesOf(text, geometry.CharactersPerTurnusLine));
        // Each limit is one indivisible value, so they wrap between one another and never within one.
        var limit = DispatchPaginator.LinesOf(departure.LimitText, geometry.CharactersPerLimitLine);
        var text = Math.Max(Math.Max(sessions, turnus), limit) * geometry.TextLineHeightMm;
        return Math.Max(text, CompositionHeightMmOf(departure.Groups, geometry)) + geometry.RowChromeHeightMm;
    }

    /// <summary>
    /// The height of a composition: its groups laid out front first, side by side until the next one does not
    /// fit, each line as tall as its tallest group.
    /// </summary>
    /// <remarks>
    /// This follows the browser's own wrapping of the groups, which is why every group has a width that is
    /// known in advance: a cargo rectangle is as wide as its longest destination and a wagonset as wide as its
    /// wagons laid side by side, neither narrower than a rectangle is drawn nor wider than the composition.
    /// </remarks>
    /// <param name="groups">The groups, front first.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double CompositionHeightMmOf(IReadOnlyList<CompositionGroup> groups, CompositionPageGeometry geometry)
    {
        groups = groups.ValueOrException(nameof(groups));
        geometry = geometry.ValueOrException(nameof(geometry));

        return WrappedHeightMm(
            groups.Select(group => (WidthMmOf(group, geometry), HeightMmOf(group, geometry))),
            geometry.CompositionWidthMm, geometry.GroupGapWidthMm, geometry.GroupGapHeightMm);
    }

    /// <summary>The width a group is drawn at: as wide as its contents, within the composition.</summary>
    /// <remarks>
    /// A group too wide for one line takes the whole width, not the width of what fits on its first line: the
    /// browser shrinks an item wider than the line to the line, and wraps its contents within.
    /// </remarks>
    /// <param name="group">The group.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double WidthMmOf(CompositionGroup group, CompositionPageGeometry geometry)
    {
        group = group.ValueOrException(nameof(group));
        geometry = geometry.ValueOrException(nameof(geometry));

        var content = group switch
        {
            WagonsetComposition wagonset => SideBySideMm(wagonset, geometry),
            // The rectangle holds one destination per line, so it is as wide as the longest of them.
            CargoPositionComposition cargo => geometry.CargoChromeWidthMm + (cargo.Destinations
                .Select(destination => destination.Text.Length).DefaultIfEmpty(0).Max() * geometry.DestinationCharacterWidthMm),
            _ => 0,
        };
        return Math.Clamp(content, geometry.RectangleMinWidthMm, geometry.CompositionWidthMm);
    }

    /// <summary>The height a group is drawn at.</summary>
    /// <param name="group">The group.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double HeightMmOf(CompositionGroup group, CompositionPageGeometry geometry) => group switch
    {
        WagonsetComposition wagonset => WrappedHeightMm(
            wagonset.Wagons.Select(wagon => (WidthMmOf(wagon, geometry), geometry.WagonHeightMm)),
            WidthMmOf(wagonset, geometry), geometry.WagonGapMm, geometry.WagonGapMm),
        CargoPositionComposition cargo =>
            geometry.CargoChromeHeightMm + (DestinationLinesOf(cargo, geometry) * geometry.DestinationLineHeightMm),
        _ => geometry.WagonHeightMm,
    };

    // The lines of destination text in a cargo rectangle. A rectangle is as wide as its longest destination, so
    // each destination takes one line — unless the rectangle was clamped to the width of the composition, where
    // the long ones wrap within it.
    private static int DestinationLinesOf(CargoPositionComposition cargo, CompositionPageGeometry geometry)
    {
        var characters = geometry.DestinationCharactersWithin(WidthMmOf(cargo, geometry) - geometry.CargoChromeWidthMm);
        return Math.Max(1, cargo.Destinations.Sum(destination => DispatchPaginator.LinesOf(destination.Text, characters)));
    }

    /// <summary>
    /// The width a wagon rectangle is drawn at: its class, and its number beside it where it has one.
    /// </summary>
    /// <param name="wagon">The wagon.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double WidthMmOf(Wagon wagon, CompositionPageGeometry geometry)
    {
        wagon = wagon.ValueOrException(nameof(wagon));
        geometry = geometry.ValueOrException(nameof(geometry));

        var text = wagon.Class.Length * geometry.WagonClassCharacterWidthMm;
        if (!string.IsNullOrWhiteSpace(wagon.Number))
            text += geometry.WagonTextGapMm + (wagon.Number.Length * geometry.WagonNumberCharacterWidthMm);
        return Math.Max(geometry.RectangleMinWidthMm, geometry.WagonChromeWidthMm + text);
    }

    // The wagons of a wagonset laid side by side on one line.
    private static double SideBySideMm(WagonsetComposition wagonset, CompositionPageGeometry geometry) =>
        wagonset.Wagons.Sum(wagon => WidthMmOf(wagon, geometry)) +
        (Math.Max(0, wagonset.Wagons.Count - 1) * geometry.WagonGapMm);

    // The height of items laid out the way the browser wraps a flex row: side by side until the next one does
    // not fit, then on a new line, each line as tall as its tallest item. At least one item goes on each line,
    // however wide, as the browser does.
    private static double WrappedHeightMm(
        IEnumerable<(double Width, double Height)> items, double lineWidthMm, double gapWidthMm, double gapHeightMm)
    {
        var total = 0.0;
        var used = 0.0;
        var lineHeight = 0.0;
        foreach (var (width, height) in items)
        {
            if (used > 0 && used + gapWidthMm + width > lineWidthMm)
            {
                total += lineHeight + gapHeightMm;
                used = 0;
                lineHeight = 0;
            }
            used += used > 0 ? gapWidthMm + width : width;
            lineHeight = Math.Max(lineHeight, height);
        }
        return total + lineHeight;
    }

    // Fills pages with rows until the next one would overflow, always placing at least one row per page so that
    // a row taller than the page is still printed rather than looping forever.
    private static IEnumerable<IReadOnlyList<CompositionDeparture>> SplitIntoPages(
        IReadOnlyList<CompositionDeparture> departures, CompositionPageGeometry geometry)
    {
        var available = geometry.PrintableHeightMm - geometry.HeaderHeightMm;
        var current = new List<CompositionDeparture>();
        var used = 0.0;

        foreach (var departure in departures)
        {
            var height = HeightMmOf(departure, geometry);
            if (current.Count > 0 && used + height > available)
            {
                yield return current;
                current = [];
                used = 0;
            }
            current.Add(departure);
            used += height;
        }
        if (current.Count > 0) yield return current;
    }
}
