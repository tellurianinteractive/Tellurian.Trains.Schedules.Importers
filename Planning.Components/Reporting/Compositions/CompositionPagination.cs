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
/// than the browser does. Only a rectangle clamped to the width of the composition, and the destinations within
/// it, wrap onto further lines.
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
    public double RowChromeHeightMm { get; init; } = 2.25;

    /// <summary>One line of text in the train's own columns.</summary>
    public double TextLineHeightMm { get; init; } = 5;

    /// <summary>Combined width of the columns before the composition.</summary>
    /// <remarks>
    /// Track 10, to/from 32. The sessions, the train, its times and its limit are in the loco rectangle.
    /// </remarks>
    public double FixedColumnsWidthMm { get; init; } = 42;

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
    /// Height of one line of rectangles: the loco, and a wagonset frame with its wagons on one line, as tall as a
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

    /// <summary>
    /// What the shaded wagonset frame adds on each side of what it holds, across and down: its border and its
    /// padding.
    /// </summary>
    /// <remarks>
    /// The frame encloses the turnus and every wagon, so it reads as one group of wagons under one turnus number
    /// rather than as a further wagonset beside them. It is as tall as the loco, so a wagon inside it is the
    /// frame shorter.
    /// </remarks>
    public double WagonsetFrameMm { get; init; } = 0.6;

    /// <summary>Height of one wagon rectangle inside a wagonset frame, and of the turnus beside it.</summary>
    public double FramedWagonHeightMm => WagonHeightMm - (2 * WagonsetFrameMm);

    /// <summary>What the turnus adds across to its text inside the frame: its side padding.</summary>
    public double TurnusChromeWidthMm { get; init; } = 1.8;

    /// <summary>
    /// What the loco rectangle adds across to its text: its heavier border, its side padding, and the arrow at
    /// its leading end with the gap after it.
    /// </summary>
    public double LocoChromeWidthMm { get; init; } = 7;

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
/// <param name="View">Which side of the tracks the page is read from.</param>
public sealed record CompositionPage(
    StationCompositions Station,
    IReadOnlyList<CompositionDeparture> Departures,
    bool IsContinued,
    CompositionView View = CompositionView.AsDrawn)
{
    /// <summary>The heading a row's train is drawn with on this page.</summary>
    /// <param name="departure">The row.</param>
    public CompositionHeading HeadingOf(CompositionDeparture departure) =>
        departure.ValueOrException(nameof(departure)).Heading.SeenIn(View);

    /// <summary>The station's neighbours that lie to the left as this page is read.</summary>
    public IReadOnlyList<OperationLocation> LeftNeighbours =>
        View == CompositionView.AsDrawn ? Station.LeftNeighbours : Station.RightNeighbours;

    /// <summary>The station's neighbours that lie to the right as this page is read.</summary>
    public IReadOnlyList<OperationLocation> RightNeighbours =>
        View == CompositionView.AsDrawn ? Station.RightNeighbours : Station.LeftNeighbours;
}

/// <summary>Splits the stations of the Train compositions report into printed pages.</summary>
/// <remarks>
/// <para>
/// Every station starts on a page of its own, because the sheets are handed to different stations. A station
/// too long for one page continues on the next under the same heading. A station with no departures to show
/// gets no page: unlike a dispatch list, where every manned station clears trains, most stations here would
/// only be handed a sheet saying there is nothing to check.
/// </para>
/// <para>
/// Every page is printed twice, as drawn and mirrored, one after the other: the trains are drawn travelling
/// the way they pass the reader, and which side of the tracks the reader stands on is not known. Printed on
/// both sides of the paper, each sheet then holds the same trains both ways round, and the reader turns it to
/// the side that matches what they see. The two pages hold the same rows, since mirroring changes no height,
/// so every station takes an even number of pages and starts on the front of a sheet.
/// </para>
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
                pages.Add(new CompositionPage(station, departures, isContinued, CompositionView.AsDrawn));
                pages.Add(new CompositionPage(station, departures, isContinued, CompositionView.Mirrored));
                isContinued = true;
            }
        }
        return pages;
    }

    /// <summary>
    /// The height of one row: the taller of a line of the train's own columns, which do not wrap, and the
    /// composition with the loco at its front, and the row's chrome.
    /// </summary>
    /// <param name="departure">The row.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double HeightMmOf(CompositionDeparture departure, CompositionPageGeometry geometry)
    {
        departure = departure.ValueOrException(nameof(departure));
        geometry = geometry.ValueOrException(nameof(geometry));

        var composition = WrappedHeightMm(
            departure.Groups
                .Select(group => (WidthMmOf(group, geometry), HeightMmOf(group, geometry)))
                .Prepend((LocoWidthMmOf(departure, geometry), geometry.WagonHeightMm)),
            geometry.CompositionWidthMm, geometry.GroupGapWidthMm, geometry.GroupGapHeightMm);
        return Math.Max(geometry.TextLineHeightMm, composition) + geometry.RowChromeHeightMm;
    }

    /// <summary>
    /// The width of the loco rectangle at the front of a row's train: the sessions, the train, its times here
    /// and the most it may be made up of side by side on one line, with the arrow pointing the way it travels.
    /// </summary>
    /// <remarks>
    /// The train is set in bold and charged like a wagon class; the times are figures, charged like a wagon
    /// number; the limit, figures with their marks, is charged like the columns' text. Mirroring moves the rectangle to the other end of the train, and does not change its width.
    /// </remarks>
    /// <param name="departure">The row.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double LocoWidthMmOf(CompositionDeparture departure, CompositionPageGeometry geometry)
    {
        departure = departure.ValueOrException(nameof(departure));
        geometry = geometry.ValueOrException(nameof(geometry));

        var text =
            (departure.SessionsText.Length * geometry.CharacterWidthMm) +
            (departure.TrainIdentity.Length * geometry.WagonClassCharacterWidthMm) +
            (departure.TimeText.Length * geometry.WagonNumberCharacterWidthMm) +
            (2 * geometry.WagonTextGapMm);
        if (departure.LimitText.Length > 0)
            text += geometry.WagonTextGapMm + (departure.LimitText.Length * geometry.CharacterWidthMm);
        return Math.Min(geometry.CompositionWidthMm, geometry.LocoChromeWidthMm + text);
    }

    /// <summary>
    /// The height of a composition: its groups laid out front first, side by side until the next one does not
    /// fit, each line as tall as its tallest group.
    /// </summary>
    /// <remarks>
    /// This follows the browser's own wrapping of the groups, which is why every group has a width that is
    /// known in advance: a cargo rectangle is as wide as its longest destination and a wagonset frame as wide as
    /// its turnus and wagons laid side by side, neither narrower than a rectangle is drawn nor wider than the composition.
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
            // The places run on as one list, so the rectangle is as wide as the whole list on one line.
            CargoPositionComposition cargo => geometry.CargoChromeWidthMm + (ListOf(cargo).Length * geometry.DestinationCharacterWidthMm),
            ArrivingCargoComposition arriving => geometry.CargoChromeWidthMm + (ListOf(arriving).Length * geometry.DestinationCharacterWidthMm),
            _ => 0,
        };
        return Math.Clamp(content, geometry.RectangleMinWidthMm, geometry.CompositionWidthMm);
    }

    /// <summary>The height a group is drawn at.</summary>
    /// <param name="group">The group.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double HeightMmOf(CompositionGroup group, CompositionPageGeometry geometry) => group switch
    {
        WagonsetComposition wagonset => (2 * geometry.WagonsetFrameMm) + WrappedHeightMm(
            RectangleWidthsMmOf(wagonset, geometry).Select(width => (width, geometry.FramedWagonHeightMm)),
            WidthMmOf(wagonset, geometry) - (2 * geometry.WagonsetFrameMm), geometry.WagonGapMm, geometry.WagonGapMm),
        CargoPositionComposition cargo =>
            geometry.CargoChromeHeightMm + (LinesOf(cargo, ListOf(cargo), geometry) * geometry.DestinationLineHeightMm),
        ArrivingCargoComposition arriving =>
            geometry.CargoChromeHeightMm + (LinesOf(arriving, ListOf(arriving), geometry) * geometry.DestinationLineHeightMm),
        _ => geometry.WagonHeightMm,
    };

    // The places of a cargo rectangle as they are printed: one list, separated by commas. Origins are printed
    // after a "wagons from" in the report's language, charged at its English length.
    private static string ListOf(CargoPositionComposition cargo) =>
        string.Join(", ", cargo.Destinations.Select(destination => destination.IsOrigins ? OriginsPrefix + destination.Text : destination.Text));

    private const string OriginsPrefix = "Wagons from ";

    private static string ListOf(ArrivingCargoComposition arriving) => string.Join(", ", arriving.Origins);

    // The lines of a rectangle's list. The rectangle is as wide as the list, so it takes one line — unless the
    // rectangle was clamped to the width of the composition, where the list wraps within it between words.
    private static int LinesOf(CompositionGroup group, string list, CompositionPageGeometry geometry)
    {
        var characters = geometry.DestinationCharactersWithin(WidthMmOf(group, geometry) - geometry.CargoChromeWidthMm);
        return DispatchPaginator.LinesOf(list, characters);
    }

    /// <summary>
    /// The width of the turnus at the front of a wagonset, inside its shaded frame: the designation, and beside
    /// it the sessions where the wagonset is in the train on only some of them.
    /// </summary>
    /// <param name="wagonset">The wagonset.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double TurnusWidthMmOf(WagonsetComposition wagonset, CompositionPageGeometry geometry)
    {
        wagonset = wagonset.ValueOrException(nameof(wagonset));
        geometry = geometry.ValueOrException(nameof(geometry));

        var text = wagonset.Designation.Length * geometry.WagonClassCharacterWidthMm;
        if (wagonset.SessionsText is { Length: > 0 } sessions)
            text += geometry.WagonTextGapMm + (sessions.Length * geometry.CharacterWidthMm);
        // " x 12" after the turnus where the wagons are only counted.
        if (wagonset.UnlistedWagonCount > 0)
            text += geometry.WagonTextGapMm + ((3 + wagonset.UnlistedWagonCount.ToString().Length) * geometry.CharacterWidthMm);
        return geometry.TurnusChromeWidthMm + text;
    }

    // The rectangles of a wagonset front first: the one naming its turnus, then its wagons.
    private static IEnumerable<double> RectangleWidthsMmOf(WagonsetComposition wagonset, CompositionPageGeometry geometry) =>
        wagonset.Wagons.Select(wagon => WidthMmOf(wagon, geometry)).Prepend(TurnusWidthMmOf(wagonset, geometry));

    /// <summary>
    /// The width a wagon rectangle is drawn at: its class, with the number beside it where it has one.
    /// </summary>
    /// <param name="wagon">The wagon.</param>
    /// <param name="geometry">The page geometry to paginate against.</param>
    public static double WidthMmOf(CompositionWagon wagon, CompositionPageGeometry geometry)
    {
        wagon = wagon.ValueOrException(nameof(wagon));
        geometry = geometry.ValueOrException(nameof(geometry));

        List<double> texts = [];
        if (wagon.Class.Length > 0) texts.Add(wagon.Class.Length * geometry.WagonClassCharacterWidthMm);
        if (!string.IsNullOrWhiteSpace(wagon.Number)) texts.Add(wagon.Number.Length * geometry.WagonNumberCharacterWidthMm);
        var text = texts.Sum() + (Math.Max(0, texts.Count - 1) * geometry.WagonTextGapMm);
        return Math.Max(geometry.RectangleMinWidthMm, geometry.WagonChromeWidthMm + text);
    }

    // The rectangles of a wagonset laid side by side on one line, its turnus first, within the frame.
    private static double SideBySideMm(WagonsetComposition wagonset, CompositionPageGeometry geometry) =>
        (2 * geometry.WagonsetFrameMm) + RectangleWidthsMmOf(wagonset, geometry).Sum() + (wagonset.Wagons.Count * geometry.WagonGapMm);

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
